using LeaguePredictor.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace LeaguePredictor.Data;

public class LeaguePredictorDbContext(DbContextOptions<LeaguePredictorDbContext> options) : DbContext(options)
{
    public DbSet<Team> Teams => Set<Team>();
    public DbSet<Player> Players => Set<Player>();
    public DbSet<PlayerPrediction> PlayerPredictions => Set<PlayerPrediction>();

    /// <summary>
    /// The 10 Gallagher Premiership teams for the season this predictor covers.
    /// Fixed reference data, seeded via migration.
    /// </summary>
    public static readonly string[] TeamNames =
    [
        "Bath",
        "Bristol Bears",
        "Exeter Chiefs",
        "Gloucester",
        "Harlequins",
        "Leicester Tigers",
        "Newcastle Red Bulls",
        "Northampton Saints",
        "Sale Sharks",
        "Saracens",
    ];

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Team>(entity =>
        {
            entity.Property(t => t.Name).HasMaxLength(100).IsRequired();
            entity.HasIndex(t => t.Name).IsUnique();

            entity.HasData(TeamNames.Select((name, index) => new Team
            {
                Id = index + 1,
                Name = name,
            }));
        });

        modelBuilder.Entity<Player>(entity =>
        {
            entity.Property(p => p.Name).HasMaxLength(100).IsRequired();
            entity.HasIndex(p => p.Name).IsUnique();
        });

        modelBuilder.Entity<PlayerPrediction>(entity =>
        {
            entity.HasOne(pp => pp.Player)
                .WithMany(p => p.Predictions)
                .HasForeignKey(pp => pp.PlayerId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(pp => pp.Team)
                .WithMany(t => t.Predictions)
                .HasForeignKey(pp => pp.TeamId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(pp => new { pp.PlayerId, pp.TeamId }).IsUnique();
            entity.HasIndex(pp => new { pp.PlayerId, pp.PredictedPosition }).IsUnique();
        });
    }
}
