using AustoPredictor.Data.Repositories;
using AustoPredictor.Domain.Entities;

namespace AustoPrem.Services;

public interface IPredictionService
{
    Task<List<Team>> GetTeamsAsync();
    Task<Dictionary<int, (int PredictedPosition, PredictionDesignation Designation)>> GetPredictionsForPlayerAsync(int playerId);
    Task<SaveResult> SavePredictionsAsync(int playerId, IReadOnlyDictionary<int, (int PredictedPosition, PredictionDesignation Designation)> predictions);
}

public enum SaveResult
{
    Success,
    PlayerLocked,
    PlayerNotFound,
    InvalidSelection,
}

public class PredictionService(IPredictionRepository repository) : IPredictionService
{
    public Task<List<Team>> GetTeamsAsync() => repository.GetTeamsAsync();

    public Task<Dictionary<int, (int PredictedPosition, PredictionDesignation Designation)>> GetPredictionsForPlayerAsync(int playerId)
        => repository.GetPredictionsForPlayerAsync(playerId);

    public async Task<SaveResult> SavePredictionsAsync(int playerId, IReadOnlyDictionary<int, (int PredictedPosition, PredictionDesignation Designation)> predictions)
    {
        var result = await repository.SavePredictionsAsync(playerId, predictions);
        return result switch
        {
            AustoPredictor.Data.Repositories.SaveResult.Success => SaveResult.Success,
            AustoPredictor.Data.Repositories.SaveResult.PlayerLocked => SaveResult.PlayerLocked,
            AustoPredictor.Data.Repositories.SaveResult.PlayerNotFound => SaveResult.PlayerNotFound,
            _ => SaveResult.InvalidSelection,
        };
    }
}
