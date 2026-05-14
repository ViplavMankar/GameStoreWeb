using System;
using GameStoreWeb.Data;
using GameStoreWeb.DTOs;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace GameStoreWeb.Services;

public class LeaderboardService : ILeaderboardService
{
    private readonly IDbContextFactory<GameStoreDbContext> _contextFactory;
    // private readonly HttpClient _authClient;
    private readonly UserManager<Models.ApplicationUser> _userManager;
    public LeaderboardService(IDbContextFactory<GameStoreDbContext> contextFactory, UserManager<Models.ApplicationUser> userManager)
    {
        _contextFactory = contextFactory;
        // _authClient = httpClientFactory.CreateClient("AuthService");
        _userManager = userManager;
    }
    public async Task<List<LeaderboardDto>> GetPlaytimeLeaderboardAsync()
    {
        var context = await _contextFactory.CreateDbContextAsync();

        var users = await context.UserStatistics
            .OrderByDescending(x => x.TotalSecondsPlayed)
            .Take(50)
            .ToListAsync();

        var result = new List<LeaderboardDto>();

        int rank = 1;

        foreach (var user in users)
        {
            var username = await ResolveUsernameAsync(user.UserId);

            result.Add(new LeaderboardDto
            {
                Rank = rank++,
                Username = username ?? "Unknown", // safety fallback
                Value = user.TotalSecondsPlayed
            });
        }

        return result;
    }
    public async Task<List<LeaderboardDto>> GetXPLeaderboardAsync()
    {
        var _context = await _contextFactory.CreateDbContextAsync();
        var users = await _context.UserStatistics
                .OrderByDescending(x => x.TotalXP)
                .Take(50)
                .ToListAsync();

        List<LeaderboardDto> result = new List<LeaderboardDto>();
        int rank = 1;
        foreach (var user in users)
        {
            var username = await ResolveUsernameAsync(user.UserId);
            result.Add(new LeaderboardDto
            {
                Rank = rank++,
                Username = username ?? "Unknown", // safety fallback
                Value = user.TotalXP
            });
        }
        return result;
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
