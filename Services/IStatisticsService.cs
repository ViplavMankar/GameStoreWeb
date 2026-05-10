using System;
using GameStoreWeb.DTOs;

namespace GameStoreWeb.Services;

public interface IStatisticsService
{
    Task<List<TrendingGameDto>> GetTrendingGamesAsync();
    Task<List<GamePlaytimeDto>> GetTotalPlaytimePerGameAsync();
    Task<List<DailyActivePlayersDto>> GetDailyActivePlayersAsync(int days);
}
