using AustoPredictor.Data.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AustoPredictor.Data;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddAustoPredictorData(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' was not found. Set it in appsettings.json / appsettings.Development.json or via configuration.");

        services.AddDbContextFactory<AustoPredictorDbContext>(options =>
            options.UseSqlServer(connectionString));

        services.AddScoped<IPlayerRepository, PlayerRepository>();
        services.AddScoped<IPredictionRepository, PredictionRepository>();

        return services;
    }
}
