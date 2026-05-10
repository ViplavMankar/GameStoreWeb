using System;
using GameStoreWeb.Data;
using GameStoreWeb.DTOs;
using GameStoreWeb.Models;
using Microsoft.EntityFrameworkCore;

namespace GameStoreWeb.Services;

public class CollectionService : ICollectionService
{
    private readonly IDbContextFactory<GameStoreDbContext> _contextFactory;

    public CollectionService(IDbContextFactory<GameStoreDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }
    public async Task<bool> AddToCollectionAsync(Guid userId, Guid gameId)
    {
        using var _context = await _contextFactory.CreateDbContextAsync();
        var exists = await _context.UserCollections
            .AnyAsync(c => c.UserId == userId && c.GameId == gameId);

        if (exists) return false;

        _context.UserCollections.Add(new UserCollection
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            GameId = gameId
        });

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> RemoveFromCollectionAsync(Guid id, Guid userId)
    {
        using var _context = await _contextFactory.CreateDbContextAsync();
        var collectionEntry = await _context.UserCollections.FirstOrDefaultAsync(c => c.UserId == userId && c.GameId == id);

        if (collectionEntry == null) return false;

        _context.UserCollections.Remove(collectionEntry);

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<IEnumerable<DTOs.GameReadDto>> GetUserCollectionAsync(Guid userId)
    {
        using var _context = await _contextFactory.CreateDbContextAsync();
        return await (from c in _context.UserCollections
                      join g in _context.Games on c.GameId equals g.Id
                      where c.UserId == userId
                      select new DTOs.GameReadDto
                      {
                          Id = g.Id,
                          Title = g.Title,
                          ThumbnailUrl = g.ThumbnailUrl,
                          Description = g.Description,
                          GameUrl = g.GameUrl
                      }).ToListAsync();
    }

    public async Task<bool> IsGameSavedAsync(Guid userId, Guid gameId)
    {
        using var _context = await _contextFactory.CreateDbContextAsync();
        return await _context.UserCollections.AnyAsync(c => c.UserId == userId && c.GameId == gameId);
    }
}
