using Manam.Services.Abstractions.UserManagement;
using Manam.Services.Implementations.UserManagement;
using Microsoft.Extensions.DependencyInjection;

namespace Manam.Services.Extensions;

/// <summary>
/// Extension methods for user management services registration
/// </summary>
public static class UserManagementServiceExtensions
{
    /// <summary>
    /// Add user management services to the DI container
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <returns>Updated service collection</returns>
    public static IServiceCollection AddUserManagementServices(this IServiceCollection services)
    {
        // Register authentication service
        services.AddScoped<IAuthenticationService, AuthenticationService>();
        
        // Register user profile service
        services.AddScoped<IUserProfileService, UserProfileService>();

        return services;
    }
}
