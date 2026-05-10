using System;
using GameStoreWeb.Models;

namespace GameStoreWeb.Services;

public interface IUserStatsService
{
    Task<UserStats> GetOrCreateAsync(Guid userId);
    Task UpdateAfterSessionAsync(Guid userId, Guid gameId, int secondsPlayed);
    Task UpdateAfterPurchaseAsync(Guid userId);
    Task<UserStats?> GetStatsAsync(Guid userId);
    Task RecalculateStatsAsync(Guid userId);
}
