using System;
using GameStoreWeb.DTOs;

namespace GameStoreWeb.Services;

public interface ICollectionService
{
    Task<bool> AddToCollectionAsync(Guid userId, Guid gameId);
    Task<bool> RemoveFromCollectionAsync(Guid id, Guid userId);
    Task<IEnumerable<GameReadDto>> GetUserCollectionAsync(Guid userId);
    Task<bool> IsGameSavedAsync(Guid userId, Guid gameId);
}
