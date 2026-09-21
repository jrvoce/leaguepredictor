namespace AustoPredictor.Data.Entities;

/// <summary>
/// One row of a player's prediction: "this team will finish in this league position".
/// A player has exactly 10 of these rows, one per team, with PredictedPosition being
/// a permutation of 1-10.
///
/// Deliberately kept separate from anything about the real league table, so a later
/// scoring feature (e.g. an ActualStanding table plus a scoring service that compares
/// PredictedPosition to the real position) can be added without touching this table.
/// </summary>
public class PlayerPrediction
{
    public int Id { get; set; }

    public int PlayerId { get; set; }
    public Player Player { get; set; } = null!;

    public int TeamId { get; set; }
    public Team Team { get; set; } = null!;

    /// <summary>Predicted final league position, 1 (champions) to 10 (bottom).</summary>
    public int PredictedPosition { get; set; }

    public PredictionDesignation Designation { get; set; }

    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;
}
