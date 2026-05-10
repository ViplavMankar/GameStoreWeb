using System;
using GameStoreWeb.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace GameStoreWeb.Services;

public class NotificationService : INotificationService
{
    private readonly IHubContext<RealtimeHub> _hub;

    public NotificationService(IHubContext<RealtimeHub> hub)
    {
        _hub = hub;
    }

    public async Task SendStreakUpdateAsync(Guid userId, int current, int max)
    {
        await _hub.Clients.User(userId.ToString())
            .SendAsync("StreakUpdated", new { current, max });
    }
}
