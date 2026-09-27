using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using System;

namespace Manam.Data.Extensions;

/// <summary>
/// Dependency Injection extension methods for data access layer
/// Registers StoredProcedures and database services
/// </summary>
public static class DataServiceExtensions
{
    /// <summary>
    /// Register data access services including stored procedures
    /// Usage: builder.Services.AddDataServices(configuration);
    /// </summary>
    public static IServiceCollection AddDataServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        if (services == null)
            throw new ArgumentNullException(nameof(services));

        if (configuration == null)
            throw new ArgumentNullException(nameof(configuration));

        // Get connection string from appsettings.json
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found in configuration");

        // Register StoredProcedures as singleton (connection string doesn't change)
        services.AddSingleton<IStoredProcedures>(serviceProvider =>
        {
            var logger = serviceProvider.GetRequiredService<Microsoft.Extensions.Logging.ILogger<StoredProcedures>>();
            return new StoredProcedures(connectionString, logger);
        });

        return services;
    }

    /// <summary>
    /// Register data access services with custom connection string
    /// Usage: builder.Services.AddDataServices("Server=localhost;Database=Manam;...");
    /// </summary>
    public static IServiceCollection AddDataServices(
        this IServiceCollection services,
        string connectionString)
    {
        if (services == null)
            throw new ArgumentNullException(nameof(services));

        if (string.IsNullOrWhiteSpace(connectionString))
            throw new ArgumentException("Connection string cannot be null or empty", nameof(connectionString));

        // Register StoredProcedures as singleton
        services.AddSingleton<IStoredProcedures>(serviceProvider =>
        {
            var logger = serviceProvider.GetRequiredService<Microsoft.Extensions.Logging.ILogger<StoredProcedures>>();
            return new StoredProcedures(connectionString, logger);
        });

        return services;
    }
}
