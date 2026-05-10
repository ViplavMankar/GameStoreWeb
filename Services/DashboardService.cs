using GameStoreWeb.Data;
using Microsoft.EntityFrameworkCore;

namespace GameStoreWeb.Services;

public class DashboardService : IDashboardService
{
    private readonly GameStoreDbContext _context;

    public DashboardService(GameStoreDbContext context)
    {
        _context = context;
    }

    public async Task<int> GetXP(Guid userId)
    {
        return await _context.UserStatistics
            .Where(us => us.UserId == userId)
            .Select(us => us.TotalXP)
            .FirstOrDefaultAsync();
    }

    public async Task<(int CurrentStreak, int MaxStreak)> GetStreak(Guid userId)
    {
        var streak = await _context.UserStreaks
            .Where(s => s.UserId == userId)
            .Select(s => new { s.CurrentStreak, s.MaxStreak })
            .FirstOrDefaultAsync();

        return (
            streak?.CurrentStreak ?? 0,
            streak?.MaxStreak ?? 0
        );
    }
}