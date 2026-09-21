using AustoPredictor.Data;
using AustoPredictor.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace AustoPredictor.Services;

public class PlayerService(IDbContextFactory<AustoPredictorDbContext> contextFactory)
{
    public async Task<List<Player>> GetAllPlayersAsync()
    {
        await using var context = await contextFactory.CreateDbContextAsync();
        return await context.Players
            .OrderBy(p => p.Name)
            .ToListAsync();
    }

    public async Task<Player?> GetPlayerAsync(int playerId)
    {
        await using var context = await contextFactory.CreateDbContextAsync();
        return await context.Players.FindAsync(playerId);
    }

    /// <summary>Adds a new player. Returns null if the name is empty or already taken.</summary>
    public async Task<Player?> AddPlayerAsync(string name)
    {
        name = name.Trim();
        if (string.IsNullOrWhiteSpace(name))
        {
            return null;
        }

        await using var context = await contextFactory.CreateDbContextAsync();

        var exists = await context.Players.AnyAsync(p => p.Name == name);
        if (exists)
        {
            return null;
        }

        var player = new Player { Name = name };
        context.Players.Add(player);
        await context.SaveChangesAsync();
        return player;
    }

    /// <summary>Locks a player's prediction so it can no longer be edited from the selection page.</summary>
    public async Task SetLockedAsync(int playerId, bool isLocked)
    {
        await using var context = await contextFactory.CreateDbContextAsync();
        var player = await context.Players.FindAsync(playerId);
        if (player is null)
        {
            return;
        }

        player.IsLocked = isLocked;
        player.LockedAtUtc = isLocked ? DateTime.UtcNow : null;
        await context.SaveChangesAsync();
    }

    public async Task DeletePlayerAsync(int playerId)
    {
        await using var context = await contextFactory.CreateDbContextAsync();
        var player = await context.Players.FindAsync(playerId);
        if (player is null)
        {
            return;
        }

        context.Players.Remove(player);
        await context.SaveChangesAsync();
    }
}
