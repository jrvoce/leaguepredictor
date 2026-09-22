using LeaguePredictor.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace LeaguePredictor.Data.Repositories;

public interface IPlayerRepository
{
    Task<List<Player>> GetAllPlayersAsync();
    Task<Player?> GetPlayerAsync(int playerId);
    Task<Player?> AddPlayerAsync(string name);
    Task SetLockedAsync(int playerId, bool isLocked);
    Task DeletePlayerAsync(int playerId);
}

public class PlayerRepository(IDbContextFactory<LeaguePredictorDbContext> contextFactory) : IPlayerRepository
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
