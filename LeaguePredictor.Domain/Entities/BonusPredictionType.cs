namespace LeaguePredictor.Domain.Entities;

/// <summary>
/// Identifies which bonus question a <see cref="PlayerBonusPrediction"/> answers. Stored as
/// an int, so never renumber existing values; add new ones with the next free number.
/// </summary>
public enum BonusPredictionType
{
    /// <summary>Total league points scored across all teams over the whole season.</summary>
    TotalLeaguePoints = 1,
}
