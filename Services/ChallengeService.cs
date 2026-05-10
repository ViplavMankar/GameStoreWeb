using System;
using GameStoreWeb.Data;
using GameStoreWeb.DTOs;
using GameStoreWeb.Models;
using Microsoft.EntityFrameworkCore;

namespace GameStoreWeb.Services;

public class ChallengeService : IChallengeService
{
    private readonly IDbContextFactory<GameStoreDbContext> _contextFactory;

    public ChallengeService(IDbContextFactory<GameStoreDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }
    public Task<List<DailyChallenge>> GetTodayChallengesAsync()
    {
        var today = DateTime.UtcNow.Date;
        return _contextFactory.CreateDbContextAsync()
            .Result.DailyChallenges
            .Where(x => x.Date == today)
            .ToListAsync();
    }
    public async Task<List<ChallengeDto>> GetMyChallengesAsync(Guid userId)
    {
        var today = DateTime.UtcNow.Date;
        var _context = await _contextFactory.CreateDbContextAsync();

        var challenges = await _context.DailyChallenges
            .Where(x => x.Date == today)
            .ToListAsync();

        var progressList = await _context.UserDailyChallengeProgresses
            .Where(x => x.UserId == userId)
            .ToListAsync();

        var result = challenges.Select(ch =>
        {
            var progress = progressList
                .FirstOrDefault(p => p.ChallengeId == ch.Id);

            return new ChallengeDto
            {
                Id = ch.Id,
                Title = ch.Title,
                Description = ch.Description,
                TargetValue = ch.TargetValue,
                XPReward = ch.XPReward,
                CurrentProgress = progress?.CurrentProgress ?? 0,
                IsCompleted = progress?.IsCompleted ?? false
            };
        }).ToList();

        return result;
    }
    public async Task UpdateProgressAsync(Guid userId, int durationSeconds)
    {
        var today = DateTime.UtcNow.Date;
        var _db = await _contextFactory.CreateDbContextAsync();

        var challenges = await _db.DailyChallenges
            .Where(x => x.Date == today)
            .ToListAsync();

        foreach (var challenge in challenges)
        {
            var progress = await _db.UserDailyChallengeProgresses
                .FirstOrDefaultAsync(x =>
                    x.UserId == userId &&
                    x.ChallengeId == challenge.Id);

            if (progress == null)
            {
                progress = new UserDailyChallengeProgress
                {
                    UserId = userId,
                    ChallengeId = challenge.Id
                };

                _db.UserDailyChallengeProgresses.Add(progress);
            }

            if (progress.IsCompleted)
                continue;

            if (challenge.Title == "Play Games")
                progress.CurrentProgress += 1;

            else if (challenge.Title == "Play Time")
                progress.CurrentProgress += durationSeconds;

            if (progress.CurrentProgress >= challenge.TargetValue)
            {
                progress.IsCompleted = true;
                progress.CompletedAt = DateTime.UtcNow;

                var stats = await _db.UserStatistics
                    .FirstOrDefaultAsync(x => x.UserId == userId);

                if (stats != null)
                    stats.TotalXP += challenge.XPReward;
            }
        }
    }
}
