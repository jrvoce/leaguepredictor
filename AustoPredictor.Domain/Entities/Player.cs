namespace AustoPredictor.Domain.Entities;

/// <summary>
/// A person taking part in the predictor. Players are added by an admin (no self
/// sign-up) and choose their name from the selection page to enter or view their
/// prediction.
/// </summary>
public class Player
{
    public int Id { get; set; }

    public required string Name { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Once true, the player's prediction can no longer be changed via the selection
    /// page. Toggled by an admin, typically once the league season starts.
    /// </summary>
    public bool IsLocked { get; set; }

    public DateTime? LockedAtUtc { get; set; }

    public ICollection<PlayerPrediction> Predictions { get; set; } = new List<PlayerPrediction>();
}
