using System;
using GameStoreWeb.Data;
using GameStoreWeb.Models;
using Microsoft.EntityFrameworkCore;

namespace GameStoreWeb.Services;

public class PricingService : IPricingService
{
    private readonly IDbContextFactory<GameStoreDbContext> _dbFactory;

    public PricingService(IDbContextFactory<GameStoreDbContext> dbFactory) => _dbFactory = dbFactory;

    public async Task<GamePrice> SetPriceForGameAsync(Guid gameId, long pricePaise, string currency = "INR", DateTimeOffset? effectiveFrom = null)
    {
        // Deactivate current active price(s) for this game/currency
        using var _db = await _dbFactory.CreateDbContextAsync();
        var actives = await _db.GamePrices
            .Where(p => p.GameId == gameId && p.Currency == currency && p.IsActive)
            .ToListAsync();

        foreach (var p in actives)
        {
            p.IsActive = false;
            p.EffectiveTo = DateTimeOffset.UtcNow;
        }

        var newPrice = new GamePrice
        {
            GameId = gameId,
            PricePaise = pricePaise,
            Currency = currency,
            IsActive = true,
            EffectiveFrom = effectiveFrom ?? DateTimeOffset.UtcNow
        };

        _db.GamePrices.Add(newPrice);
        await _db.SaveChangesAsync();
        return newPrice;
    }

    public async Task<GamePrice?> GetActivePriceAsync(Guid gameId, string currency = "INR")
    {
        using var _db = await _dbFactory.CreateDbContextAsync();
        return await _db.GamePrices
            .Where(p => p.GameId == gameId && p.Currency == currency && p.IsActive)
            .OrderByDescending(p => p.EffectiveFrom)
            .FirstOrDefaultAsync();
    }

    public async Task<List<GamePrice>> GetPriceHistoryAsync(Guid gameId, string currency = "INR")
    {
        using var _db = await _dbFactory.CreateDbContextAsync();
        return await _db.GamePrices
            .Where(p => p.GameId == gameId && p.Currency == currency)
            .OrderByDescending(p => p.EffectiveFrom)
            .ToListAsync();
    }
}
