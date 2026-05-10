using System;
using GameStoreWeb.Data;
using GameStoreWeb.Models;
using GameStoreWeb.DTOs;
using Microsoft.EntityFrameworkCore;

namespace GameStoreWeb.Services;

public class GameSessionService : IGameSessionService
{
    private readonly IDbContextFactory<GameStoreDbContext> _dbFactory;
    private readonly IStreakService _streakService;
    private readonly IUserStatsService _userStatsService;
    private readonly IAchievementService _achievementService;
    private readonly IChallengeService _challengeService;
    private readonly INotificationService _notificationService;

    public GameSessionService(IDbContextFactory<GameStoreDbContext> dbFactory,
        IStreakService streakService,
        IUserStatsService userStatsService,
        IAchievementService achievementService,
        IChallengeService challengeService,
        INotificationService notificationService)
    {
        _dbFactory = dbFactory;
        _streakService = streakService;
        _userStatsService = userStatsService;
        _achievementService = achievementService;
        _challengeService = challengeService;
        _notificationService = notificationService;
    }

    public async Task<Guid> StartSessionAsync(Guid userId, Guid gameId)
    {
        var _db = await _dbFactory.CreateDbContextAsync();
        if (userId == Guid.Empty)
            throw new ArgumentException("Invalid userId");

        if (gameId == Guid.Empty)
            throw new ArgumentException("Invalid gameId");

        var session = new GameSession
        {
            GameId = gameId,
            UserId = userId,
            StartedAt = DateTime.UtcNow
        };

        _db.GameSessions.Add(session);
        await _db.SaveChangesAsync();

        return session.Id;
    }
    public async Task<List<AchievementUnlockedDto>> EndSessionAsync(Guid sessionId)
    {
        var _db = await _dbFactory.CreateDbContextAsync();
        var session = await _db.GameSessions
            .FirstOrDefaultAsync(x => x.Id == sessionId);

        if (session == null)
            throw new Exception("Session not found");

        if (session.EndedAt != null)
            throw new Exception("Session already ended");

        session.EndedAt = DateTime.UtcNow;

        var duration = session.EndedAt - session.StartedAt;
        var durationSeconds = (int)duration.Value.TotalSeconds;
        session.DurationSeconds = durationSeconds;

        // 🔥 1. Update Challenges
        await _challengeService.UpdateProgressAsync(session.UserId, durationSeconds);

        // 🔥 2. Save session changes early
        await _db.SaveChangesAsync();

        // 🔥 3. Update streak
        var streakResult = await _streakService.UpdateStreakAsync(session.UserId);

        if (streakResult.UpdatedToday)
        {
            await _notificationService.SendStreakUpdateAsync(
                session.UserId,
                streakResult.Streak.CurrentStreak,
                streakResult.Streak.MaxStreak);
        }

        // 🔥 4. Update stats
        await _userStatsService.UpdateAfterSessionAsync(
            session.UserId,
            session.GameId,
            durationSeconds);

        // 🔥 5. Achievements
        var unlockedAchievements = await _achievementService
            .EvaluateAchievementsAsync(session.UserId);

        return unlockedAchievements;
    }
}
