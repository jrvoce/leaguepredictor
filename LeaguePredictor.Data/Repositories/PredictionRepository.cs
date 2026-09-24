using LeaguePredictor.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace LeaguePredictor.Data.Repositories;

public interface IPredictionRepository
{
    Task<List<Team>> GetTeamsAsync();
    Task<Dictionary<int, (int PredictedPosition, PredictionDesignation Designation)>> GetPredictionsForPlayerAsync(int playerId);
    Task<Dictionary<BonusPredictionType, int>> GetBonusPredictionsForPlayerAsync(int playerId);
    Task<SaveResult> SavePredictionsAsync(
        int playerId,
        IReadOnlyDictionary<int, (int PredictedPosition, PredictionDesignation Designation)> predictions,
        IReadOnlyDictionary<BonusPredictionType, int> bonusPredictions);
}

public enum SaveResult
{
    Success,
    PlayerLocked,
    PlayerNotFound,
    InvalidSelection,
}

public class PredictionRepository(IDbContextFactory<LeaguePredictorDbContext> contextFactory) : IPredictionRepository
{
    public async Task<List<Team>> GetTeamsAsync()
    {
        await using var context = await contextFactory.CreateDbContextAsync();
        return await context.Teams
            .OrderBy(t => t.Name)
            .ToListAsync();
    }

    public async Task<Dictionary<int, (int PredictedPosition, PredictionDesignation Designation)>> GetPredictionsForPlayerAsync(int playerId)
    {
        await using var context = await contextFactory.CreateDbContextAsync();
        return await context.PlayerPredictions
            .Where(pp => pp.PlayerId == playerId)
            .ToDictionaryAsync(pp => pp.TeamId, pp => (pp.PredictedPosition, pp.Designation));
    }

    public async Task<Dictionary<BonusPredictionType, int>> GetBonusPredictionsForPlayerAsync(int playerId)
    {
        await using var context = await contextFactory.CreateDbContextAsync();
        return await context.PlayerBonusPredictions
            .Where(bp => bp.PlayerId == playerId)
            .ToDictionaryAsync(bp => bp.Type, bp => bp.Value);
    }

    public async Task<SaveResult> SavePredictionsAsync(
        int playerId,
        IReadOnlyDictionary<int, (int PredictedPosition, PredictionDesignation Designation)> predictions,
        IReadOnlyDictionary<BonusPredictionType, int> bonusPredictions)
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
            predictions.Values.Select(v => v.PredictedPosition).OrderBy(v => v).SequenceEqual(Enumerable.Range(1, teamIds.Count)) &&
            predictions.Values.Count(v => v.Designation == PredictionDesignation.Champion) == 1 &&
            predictions.Values.Count(v => v.Designation == PredictionDesignation.RunnerUp) == 1 &&
            predictions.Values.All(v => v.Designation is PredictionDesignation.None or PredictionDesignation.Champion or PredictionDesignation.RunnerUp) &&
            predictions.Values.All(v => v.Designation == PredictionDesignation.None || v.PredictedPosition <= 4) &&
            Enum.GetValues<BonusPredictionType>().All(t => bonusPredictions.TryGetValue(t, out var bonus) && bonus >= 0) &&
            bonusPredictions.Keys.All(k => Enum.IsDefined(k));

        if (!validSelection)
        {
            return SaveResult.InvalidSelection;
        }

        var existing = await context.PlayerPredictions
            .Where(pp => pp.PlayerId == playerId)
            .ToListAsync();

        context.PlayerPredictions.RemoveRange(existing);

        foreach (var (teamId, prediction) in predictions)
        {
            context.PlayerPredictions.Add(new PlayerPrediction
            {
                PlayerId = playerId,
                TeamId = teamId,
                PredictedPosition = prediction.PredictedPosition,
                Designation = prediction.Designation,
                UpdatedAtUtc = DateTime.UtcNow,
            });
        }

        var existingBonuses = await context.PlayerBonusPredictions
            .Where(bp => bp.PlayerId == playerId)
            .ToListAsync();

        context.PlayerBonusPredictions.RemoveRange(existingBonuses);

        foreach (var (type, value) in bonusPredictions)
        {
            context.PlayerBonusPredictions.Add(new PlayerBonusPrediction
            {
                PlayerId = playerId,
                Type = type,
                Value = value,
                UpdatedAtUtc = DateTime.UtcNow,
            });
        }

        await context.SaveChangesAsync();
        return SaveResult.Success;
    }
}
