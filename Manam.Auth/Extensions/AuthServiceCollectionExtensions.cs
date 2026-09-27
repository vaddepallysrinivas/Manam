using Manam.Auth.Abstractions;
using Manam.Auth.Implementations;
using Microsoft.Extensions.DependencyInjection;

namespace Manam.Auth.Extensions;

/// <summary>
/// Extension methods for registering authentication services
/// </summary>
public static class AuthServiceCollectionExtensions
{
    /// <summary>
    /// Adds authentication services to the service collection
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddAuthenticationServices(this IServiceCollection services)
    {
        // Register authentication services as scoped - one instance per HTTP request
        services.AddScoped<IAuthenticationService, AuthenticationService>();

        return services;
    }
}
