using System;
using GameStoreWeb.Data;
using GameStoreWeb.DTOs;
using GameStoreWeb.Models;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.SignalR;
using GameStoreWeb.Hubs;

namespace GameStoreWeb.Services;

public class AchievementService : IAchievementService
{
    private readonly IDbContextFactory<GameStoreDbContext> _contextFactory;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IHubContext<RealtimeHub> _hub;

    public AchievementService(IDbContextFactory<GameStoreDbContext> contextFactory,
        IHttpContextAccessor httpContextAccessor,
        IHubContext<RealtimeHub> hub)
    {
        _contextFactory = contextFactory;
        _httpContextAccessor = httpContextAccessor;
        _hub = hub;
    }

    public async Task<List<AchievementDto>> GetUserAchievementsAsync()
    {
        var user = _httpContextAccessor.HttpContext?.User;

        if (user?.Identity == null || !user.Identity.IsAuthenticated)
            throw new UnauthorizedAccessException();

        var userId = Guid.Parse(user.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        var _context = await _contextFactory.CreateDbContextAsync();

        // 🔹 Get unlocked achievements
        var unlockedIds = await _context.UserAchievements
            .Where(x => x.UserId == userId)
            .Select(x => x.AchievementId)
            .ToListAsync();

        // 🔹 Get all achievements
        var achievements = await _context.Achievements.ToListAsync();

        return achievements.Select(a => new AchievementDto
        {
            Name = a.Name,
            Description = a.Description,
            Icon = a.Icon,
            IsUnlocked = unlockedIds.Contains(a.Id)
        }).ToList();
    }
    public async Task<List<AchievementUnlockedDto>> EvaluateAchievementsAsync(Guid userId)
    {
        var _context = await _contextFactory.CreateDbContextAsync();
        var stats = await _context.UserStatistics
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.UserId == userId);

        if (stats == null)
            return new List<AchievementUnlockedDto>();

        var unlockedIds = await _context.UserAchievements
            .Where(x => x.UserId == userId)
            .Select(x => x.AchievementId)
            .ToListAsync();

        var lockedAchievements = await _context.Achievements
            .Where(a => !unlockedIds.Contains(a.Id))
            .ToListAsync();

        var newlyUnlocked = lockedAchievements
            .Where(a => IsUnlocked(a, stats))
            .ToList();

        if (newlyUnlocked.Any())
        {
            var now = DateTime.UtcNow;

            var userAchievements = newlyUnlocked.Select(a => new UserAchievement
            {
                UserId = userId,
                AchievementId = a.Id,
                UnlockedAt = now
            });

            _context.UserAchievements.AddRange(userAchievements);
            await _context.SaveChangesAsync();

            foreach (var achievement in newlyUnlocked)
            {
                var dto = new AchievementUnlockedDto
                {
                    Id = achievement.Id,
                    Name = achievement.Name,
                    Description = achievement.Description
                };

                await _hub.Clients.User(userId.ToString())
                    .SendAsync("AchievementUnlocked", dto);
                // await _hub.Clients.All.SendAsync("AchievementUnlocked", dto);
            }
        }

        return newlyUnlocked.Select(a => new AchievementUnlockedDto
        {
            Id = a.Id,
            Name = a.Name,
            Description = a.Description
        }).ToList();
    }
    private bool IsUnlocked(Achievement achievement, UserStats stats)
    {
        return achievement.ConditionType switch
        {
            "GamesPlayed" => stats.GamesPlayed >= achievement.TargetValue,
            "PlayTime" => stats.TotalSecondsPlayed >= achievement.TargetValue,
            "Sessions" => stats.SessionsCount >= achievement.TargetValue,
            "Purchases" => stats.TotalPurchases >= achievement.TargetValue,
            _ => false
        };
    }
}
