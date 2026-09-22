namespace LeaguePredictor.Domain.Entities;

/// <summary>
/// One of the 10 Gallagher Premiership teams. This is reference data, seeded once
/// via the DbContext model and not expected to change at runtime.
/// </summary>
public class Team
{
    public int Id { get; set; }

    public required string Name { get; set; }

    public ICollection<PlayerPrediction> Predictions { get; set; } = new List<PlayerPrediction>();
}
