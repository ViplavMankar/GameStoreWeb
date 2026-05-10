using System;
using GameStoreWeb.DTOs;

namespace GameStoreWeb.Services;

public interface IGameSessionService
{
    Task<Guid> StartSessionAsync(Guid userId, Guid gameId);
    Task<List<AchievementUnlockedDto>> EndSessionAsync(Guid sessionId);
}
