using System;

namespace GameStoreWeb.Services;

public interface INotificationService
{
    Task SendStreakUpdateAsync(Guid userId, int current, int max);
}
