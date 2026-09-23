using LeaguePredictor.Data.Repositories;
using LeaguePredictor.Domain.Entities;

namespace LeaguePredictor.Services;

public interface IPlayerService
{
    Task<List<Player>> GetAllPlayersAsync();
    Task<Player?> GetPlayerAsync(int playerId);
    Task<Player?> AddPlayerAsync(string name);
    Task SetLockedAsync(int playerId, bool isLocked);
    Task DeletePlayerAsync(int playerId);
}

public class PlayerService(IPlayerRepository repository) : IPlayerService
{
    public Task<List<Player>> GetAllPlayersAsync() => repository.GetAllPlayersAsync();

    public Task<Player?> GetPlayerAsync(int playerId) => repository.GetPlayerAsync(playerId);

    public Task<Player?> AddPlayerAsync(string name) => repository.AddPlayerAsync(name);

    public Task SetLockedAsync(int playerId, bool isLocked) => repository.SetLockedAsync(playerId, isLocked);

    public Task DeletePlayerAsync(int playerId) => repository.DeletePlayerAsync(playerId);
}
