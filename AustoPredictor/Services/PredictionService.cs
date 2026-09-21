using AustoPredictor.Data;
using AustoPredictor.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace AustoPredictor.Services;

public class PredictionService(IDbContextFactory<AustoPredictorDbContext> contextFactory)
{
    public async Task<List<Team>> GetTeamsAsync()
    {
        await using var context = await contextFactory.CreateDbContextAsync();
        return await context.Teams
            .OrderBy(t => t.Name)
            .ToListAsync();
    }

    /// <summary>Returns the player's saved predictions as TeamId -> PredictedPosition, or empty if none saved yet.</summary>
    public async Task<Dictionary<int, int>> GetPredictionsForPlayerAsync(int playerId)
    {
        await using var context = await contextFactory.CreateDbContextAsync();
        return await context.PlayerPredictions
            .Where(pp => pp.PlayerId == playerId)
            .ToDictionaryAsync(pp => pp.TeamId, pp => pp.PredictedPosition);
    }

    public enum SaveResult
    {
        Success,
        PlayerLocked,
        PlayerNotFound,
        InvalidSelection,
    }

    /// <summary>
    /// Replaces the player's full prediction. <paramref name="predictions"/> must contain
    /// exactly one entry per known team, with a distinct position 1-10 assigned to each.
    /// Refuses to save if the player is locked.
    /// </summary>
    public async Task<SaveResult> SavePredictionsAsync(int playerId, IReadOnlyDictionary<int, int> predictions)
    {
        await using var context = await contextFactory.CreateDbContextAsync();

        var player = await context.Players.FindAsync(playerId);
        if (player is null)
        {
            return SaveResult.PlayerNotFound;
        }

        if (player.IsLocked)
        {
            return SaveResult.PlayerLocked;
        }

        var teamIds = await context.Teams.Select(t => t.Id).ToListAsync();
        var validSelection =
            predictions.Count == teamIds.Count &&
            teamIds.All(predictions.ContainsKey) &&
            predictions.Values.OrderBy(v => v).SequenceEqual(Enumerable.Range(1, teamIds.Count));

        if (!validSelection)
        {
            return SaveResult.InvalidSelection;
        }

        var existing = await context.PlayerPredictions
            .Where(pp => pp.PlayerId == playerId)
            .ToListAsync();

        context.PlayerPredictions.RemoveRange(existing);

        foreach (var (teamId, position) in predictions)
        {
            context.PlayerPredictions.Add(new PlayerPrediction
            {
                PlayerId = playerId,
                TeamId = teamId,
                PredictedPosition = position,
                UpdatedAtUtc = DateTime.UtcNow,
            });
        }

        await context.SaveChangesAsync();
        return SaveResult.Success;
    }
}
