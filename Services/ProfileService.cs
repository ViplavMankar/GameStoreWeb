using System;
using GameStoreWeb.Data;
using GameStoreWeb.DTOs;
using Microsoft.EntityFrameworkCore;

namespace GameStoreWeb.Services;

public class ProfileService : IProfileService
{
    private readonly IDbContextFactory<GameStoreDbContext> _contextFactory;
    private readonly HttpClient _authClient;

    public ProfileService(IDbContextFactory<GameStoreDbContext> contextFactory, IHttpClientFactory httpClientFactory)
    {
        _contextFactory = contextFactory;
        _authClient = httpClientFactory.CreateClient("AuthService");
    }
    public async Task<PlayerProfileDto> GetPlayerProfileAsync(Guid userId)
    {
        var username = await ResolveUsernameAsync(userId);
        var _context = await _contextFactory.CreateDbContextAsync();

        var sessions = await _context.GameSessions
            .Where(s => s.UserId == userId && s.EndedAt != null)
            .Include(s => s.Game)
            .ToListAsync();

        var totalSessions = sessions.Count;

        var totalPlaytime = sessions.Sum(s =>
            (int)(s.EndedAt.Value - s.StartedAt).TotalMinutes);

        var totalGamesPlayed = sessions
            .Select(s => s.GameId)
            .Distinct()
            .Count();

        var favoriteGame = sessions
            .GroupBy(s => s.Game.Title)
            .OrderByDescending(g => g.Count())
            .Select(g => g.Key)
            .FirstOrDefault();

        var recentGames = sessions
            .OrderByDescending(s => s.StartedAt)
            .Take(5)
            .Select(s => new RecentGameDto
            {
                GameTitle = s.Game.Title,
                DurationMinutes = (int)(s.EndedAt.Value - s.StartedAt).TotalMinutes,
                PlayedAt = s.StartedAt
            })
            .ToList();

        return new PlayerProfileDto
        {
            Username = username,
            TotalSessions = totalSessions,
            TotalGamesPlayed = totalGamesPlayed,
            TotalPlaytimeMinutes = totalPlaytime,
            FavoriteGame = favoriteGame,
            RecentGames = recentGames
        };
    }
    private async Task<string?> ResolveUsernameAsync(Guid? userId)
    {
        if (userId == null)
        {
            return "Anonymous";
        }
        try
        {
            var response = await _authClient.GetAsync($"api/users/{userId.ToString()}");
            if (!response.IsSuccessStatusCode)
                return "Unknown";

            var user = await response.Content.ReadFromJsonAsync<UserReadDto>();
            return user?.Username ?? "Unknown";
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
