using System;
using GameStoreWeb.DTOs;
using GameStoreWeb.Models;

namespace GameStoreWeb.Services;

public interface IChallengeService
{
    Task<List<ChallengeDto>> GetMyChallengesAsync(Guid userId);
    Task<List<DailyChallenge>> GetTodayChallengesAsync();
    Task UpdateProgressAsync(Guid userId, int durationSeconds);
}
