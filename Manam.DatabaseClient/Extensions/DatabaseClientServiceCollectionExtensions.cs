using Manam.DatabaseClient.Abstractions;
using Manam.DatabaseClient.Implementations;
using Microsoft.Extensions.DependencyInjection;

namespace Manam.DatabaseClient.Extensions;

/// <summary>
/// Extension methods for registering database client services
/// </summary>
public static class DatabaseClientServiceCollectionExtensions
{
    /// <summary>
    /// Adds database client services to the service collection
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="connectionString">The database connection string</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddDatabaseClientServices(
        this IServiceCollection services,
        string connectionString)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new ArgumentException("Connection string cannot be null or empty.", nameof(connectionString));
        }

        // Register the SQL Dapper Broker as a singleton with the connection string
        services.AddSingleton<ISqlDapperBroker>(_ => new SqlDapperBroker(connectionString));

        return services;
    }
}
