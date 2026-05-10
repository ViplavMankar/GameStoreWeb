using System;
using GameStoreWeb.DTOs;

namespace GameStoreWeb.Services;

public interface IAchievementService
{
    Task<List<AchievementDto>> GetUserAchievementsAsync();
    Task<List<AchievementUnlockedDto>> EvaluateAchievementsAsync(Guid userId);
}
