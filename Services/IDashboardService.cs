using System;

namespace GameStoreWeb.Services;

public interface IDashboardService
{
    Task<int> GetXP(Guid userId);
    Task<(int CurrentStreak, int MaxStreak)> GetStreak(Guid userId);
}
