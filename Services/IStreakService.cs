using System;

namespace GameStoreWeb.Services;

public interface IStreakService
{
    Task<StreakService.StreakResult> UpdateStreakAsync(Guid userId);
}
