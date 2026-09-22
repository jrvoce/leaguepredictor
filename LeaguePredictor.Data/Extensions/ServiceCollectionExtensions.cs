using LeaguePredictor.Data.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace LeaguePredictor.Data;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddLeaguePredictorData(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' was not found. Set it in appsettings.json / appsettings.Development.json or via configuration.");

        services.AddDbContextFactory<LeaguePredictorDbContext>(options =>
            options.UseSqlServer(connectionString));

        services.AddScoped<IPlayerRepository, PlayerRepository>();
        services.AddScoped<IPredictionRepository, PredictionRepository>();

        return services;
    }
}
