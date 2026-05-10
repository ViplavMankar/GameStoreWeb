using System;
using GameStoreWeb.DTOs;

namespace GameStoreWeb.Services;

public interface ILeaderboardService
{
    Task<List<LeaderboardDto>> GetPlaytimeLeaderboardAsync();
    Task<List<LeaderboardDto>> GetXPLeaderboardAsync();
}
