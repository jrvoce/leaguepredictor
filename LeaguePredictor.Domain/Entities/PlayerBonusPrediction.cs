namespace LeaguePredictor.Domain.Entities;

/// <summary>
/// A player's answer to one bonus question, e.g. "total league points scored in the season".
/// One row per player per <see cref="BonusPredictionType"/>, so a further integer-valued
/// bonus question only needs a new enum value, not a new table or column.
/// </summary>
public class PlayerBonusPrediction
{
    public int Id { get; set; }

    public int PlayerId { get; set; }
    public Player Player { get; set; } = null!;

    public BonusPredictionType Type { get; set; }

    public int Value { get; set; }

    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;
}
