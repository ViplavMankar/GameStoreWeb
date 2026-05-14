using System;
using GameStoreWeb.Data;
using GameStoreWeb.DTOs;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace GameStoreWeb.Services;

public class GameService : IGameService
{
    private readonly IDbContextFactory<GameStoreDbContext> _contextFactory;
    // private readonly HttpClient _authClient;
    private readonly UserManager<Models.ApplicationUser> _userManager;

    public GameService(IDbContextFactory<GameStoreDbContext> contextFactory, UserManager<Models.ApplicationUser> userManager)
    {
        _contextFactory = contextFactory;
        // _authClient = httpClientFactory.CreateClient("AuthService");
        _userManager = userManager;
    }

    public async Task<IEnumerable<GameReadDto>> GetAllAsync()
    {
        using var _context = await _contextFactory.CreateDbContextAsync();
        return await _context.Games
            .Select(g => new GameReadDto
            {
                Id = g.Id,
                Title = g.Title,
                Description = g.Description,
                GameUrl = g.GameUrl,
                ThumbnailUrl = g.ThumbnailUrl,
                AuthorUserId = g.AuthorUserId,
                CreatedAt = g.CreatedAt
            }).ToListAsync();
    }

    public async Task<GameReadUserDto?> GetByIdWithUsernameAsync(Guid id)
    {
        using var _context = await _contextFactory.CreateDbContextAsync();
        var game = await _context.Games.FindAsync(id);
        if (game == null) return null;

        var username = await ResolveUsernameAsync(game.AuthorUserId);

        return new GameReadUserDto
        {
            Id = game.Id,
            Title = game.Title,
            Description = game.Description,
            ThumbnailUrl = game.ThumbnailUrl,
            GameUrl = game.GameUrl,
            AuthorUserId = game.AuthorUserId,
            AuthorUsername = username,
            CreatedAt = game.CreatedAt,
            UpdatedAt = game.UpdatedAt
        };
    }

    public async Task<GameReadDto> CreateAsync(GameCreateDto dto, Guid authorId)
    {
        using var _context = await _contextFactory.CreateDbContextAsync();
        var game = new Models.Game
        {
            Id = Guid.NewGuid(),
            Title = dto.Title,
            Description = dto.Description,
            ThumbnailUrl = dto.ThumbnailUrl,
            GameUrl = dto.GameUrl,
            AuthorUserId = authorId,
        };
        _context.Games.Add(game);
        await _context.SaveChangesAsync();

        return new GameReadDto
        {
            Id = game.Id,
            Title = game.Title,
            Description = game.Description,
            GameUrl = game.GameUrl,
            ThumbnailUrl = game.ThumbnailUrl,
            AuthorUserId = game.AuthorUserId,
            CreatedAt = game.CreatedAt,
            UpdatedAt = game.UpdatedAt
        };
    }

    public async Task<GameReadDto> EditAsync(Guid id, GameEditDto dto, Guid authorId)
    {
        using var _context = await _contextFactory.CreateDbContextAsync();
        var game = await _context.Games.FindAsync(id);
        if (game == null)
        {
            throw new KeyNotFoundException("Game Not Found");
        }
        if (game.AuthorUserId != authorId)
        {
            throw new UnauthorizedAccessException("You are not allowed to edit this game!");
        }
        game.Title = dto.Title ?? game.Title;
        game.ThumbnailUrl = dto.ThumbnailUrl ?? game.ThumbnailUrl;
        game.Description = dto.Description ?? game.Description;
        game.GameUrl = dto.GameUrl ?? game.GameUrl;

        game.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return new GameReadDto
        {
            Id = game.Id,
            Title = game.Title,
            Description = game.Description,
            ThumbnailUrl = game.ThumbnailUrl,
            GameUrl = game.GameUrl,
            AuthorUserId = game.AuthorUserId,
            CreatedAt = game.CreatedAt,
            UpdatedAt = game.UpdatedAt,
        };
    }

    public async Task<bool> DeleteAsync(Guid id, Guid authorId)
    {
        using var _context = await _contextFactory.CreateDbContextAsync();
        var game = await _context.Games.FindAsync(id);
        if (game == null)
        {
            return false;
        }
        if (game.AuthorUserId != authorId)
        {
            throw new UnauthorizedAccessException("You are not allowed to delete this game!");
        }
        _context.Games.Remove(game);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<IEnumerable<GameReadDto>> GetMyGamesAsync(Guid userId)
    {
        using var _context = await _contextFactory.CreateDbContextAsync();
        return await _context.Games
                            .Where(g => g.AuthorUserId == userId)
                            .Select(g => new GameReadDto
                            {
                                Id = g.Id,
                                Title = g.Title,
                                Description = g.Description,
                                GameUrl = g.GameUrl,
                                ThumbnailUrl = g.ThumbnailUrl,
                                AuthorUserId = g.AuthorUserId,
                                CreatedAt = g.CreatedAt
                            }).ToListAsync();

    }

    public async Task<GameReadDto?> GetMySingleGameAsync(Guid userId, Guid gameId)
    {
        using var _context = await _contextFactory.CreateDbContextAsync();
        return await _context.Games
                            .Where(g => g.AuthorUserId == userId && g.Id == gameId)
                            .Select(g => new GameReadDto
                            {
                                Id = g.Id,
                                Title = g.Title,
                                Description = g.Description,
                                GameUrl = g.GameUrl,
                                ThumbnailUrl = g.ThumbnailUrl,
                                AuthorUserId = g.AuthorUserId,
                                CreatedAt = g.CreatedAt
                            }).FirstOrDefaultAsync();
    }

    private async Task<string?> ResolveUsernameAsync(Guid? userId)
    {
        if (userId == null)
        {
            return "Anonymous";
        }
        try
        {
            // var response = await _authClient.GetAsync($"api/users/{userId.ToString()}");
            var user = await _userManager.FindByIdAsync(userId.ToString());
            return user?.UserName ?? "Unknown";
        }
        catch
        {
            return "Unknown";
        }
    }
    private class UserReadDto
    {
        public string Id { get; set; }
        public string Username { get; set; } = string.Empty;
    }
}
