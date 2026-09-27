using Manam.Services.Abstractions;
using Manam.Services.Implementations;
using Microsoft.Extensions.DependencyInjection;

namespace Manam.Services.Extensions;

/// <summary>
/// Extension methods for registering business services
/// </summary>
public static class ServicesServiceCollectionExtensions
{
    /// <summary>
    /// Adds business services to the service collection
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddBusinessServices(this IServiceCollection services)
    {
        // Register business services as scoped - one instance per HTTP request
        services.AddScoped<IUserService, UserService>();

        return services;
    }
}
