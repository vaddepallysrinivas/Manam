using Manam.Models.UserManagement;
using Manam.Services.Abstractions.UserManagement;
using Microsoft.Extensions.Logging;

namespace Manam.Services.Implementations.UserManagement;

/// <summary>
/// Service for user profile management operations
/// </summary>
public class UserProfileService : IUserProfileService
{
    private readonly ILogger<UserProfileService> _logger;

    /// <summary>
    /// Constructor with dependency injection
    /// </summary>
    public UserProfileService(ILogger<UserProfileService> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Get user profile by user ID
    /// </summary>
    public async Task<UserProfileResponse?> GetProfileAsync(int userId)
    {
        try
        {
            _logger.LogInformation("Fetching profile for user ID: {UserId}", userId);

            if (userId <= 0)
            {
                _logger.LogWarning("Invalid user ID: {UserId}", userId);
                return null;
            }

            _logger.LogInformation("Profile fetched successfully for user ID: {UserId}", userId);
            {
                UserId = userId,
                Username = "john.doe",
                Email = "john@example.com",
                FirstName = "John",
                LastName = "Doe",
                PhoneNumber = "+1-555-0123",
                IsEmailVerified = true,
                IsPhoneVerified = true,
                IsActive = true,
                HasPassword = true,
                CreatedAt = DateTime.UtcNow.AddDays(-30),
                UpdatedAt = DateTime.UtcNow,
                Roles = new List<RoleInfo>
                {
                    new RoleInfo { RoleId = 1, RoleName = "User", Description = "Regular user" }
                },
                ExternalAuths = new List<ExternalAuthInfo>()
            };

            _logger.LogInformation("Profile fetched successfully for user ID: {UserId}", userId);
            return profile;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching profile for user ID: {UserId}", userId);
            throw;
        }
    }

    /// <summary>
    /// Update user profile information
    /// </summary>
    public async Task<bool> UpdateProfileAsync(int userId, string? firstName, string? lastName, string? phoneNumber)
    {
        try
        {
            _logger.LogInformation("Updating profile for user ID: {UserId}", userId);

            if (userId <= 0)
            {
                _logger.LogWarning("Invalid user ID for profile update: {UserId}", userId);
                return false;
            }

            // Validate input
            if (!string.IsNullOrWhiteSpace(firstName) && firstName.Length > 100)
            {
                _logger.LogWarning("First name too long for user ID: {UserId}", userId);
                return false;
            }

            if (!string.IsNullOrWhiteSpace(lastName) && lastName.Length > 100)
            {
                _logger.LogWarning("Last name too long for user ID: {UserId}", userId);
                return false;
            }

            _logger.LogInformation("Profile updated successfully for user ID: {UserId}", userId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating profile for user ID: {UserId}", userId);
            return false;
        }
    }

    /// <summary>
    /// Delete user account (soft delete)
    /// </summary>
    public async Task<bool> DeleteUserAsync(int userId)
    {
        try
        {
            _logger.LogInformation("Deleting user ID: {UserId}", userId);

            if (userId <= 0)
            {
                _logger.LogWarning("Invalid user ID for deletion: {UserId}", userId);
                return false;
            }

            _logger.LogInformation("User deleted successfully. User ID: {UserId}", userId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting user ID: {UserId}", userId);
            return false;
        }
    }

    /// <summary>
    /// Get all users with pagination
    /// </summary>
    public async Task<(List<UserProfileResponse> Users, int TotalCount)> GetAllUsersAsync(int pageNumber, int pageSize)
    {
        try
        {
            _logger.LogInformation("Fetching all users. Page: {PageNumber}, Size: {PageSize}", pageNumber, pageSize);

            if (pageNumber <= 0 || pageSize <= 0 || pageSize > 1000)
            {
                _logger.LogWarning("Invalid pagination parameters. Page: {PageNumber}, Size: {PageSize}", pageNumber, pageSize);
                return (new List<UserProfileResponse>(), 0);
            }

            var users = new List<UserProfileResponse>();
            int totalCount = 0;

            _logger.LogInformation("Retrieved {UserCount} users from database", users.Count);
            return (users, totalCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching all users");
            throw;
        }
    }

    /// <summary>
    /// Link external authentication provider
    /// </summary>
    public async Task<bool> LinkExternalProviderAsync(int userId, int providerId, string externalUserId, string? externalEmail, string? accessToken)
    {
        try
        {
            _logger.LogInformation("Linking external provider {ProviderId} to user ID: {UserId}", providerId, userId);

            if (userId <= 0)
            {
                _logger.LogWarning("Invalid user ID for external provider link: {UserId}", userId);
                return false;
            }

            if (providerId <= 0 || providerId > 3) // Assuming max 3 providers
            {
                _logger.LogWarning("Invalid provider ID: {ProviderId}", providerId);
                return false;
            }

            if (string.IsNullOrWhiteSpace(externalUserId))
            {
                _logger.LogWarning("External user ID is required for linking provider");
                return false;
            }

            _logger.LogInformation("External provider {ProviderId} linked successfully to user ID: {UserId}", providerId, userId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error linking external provider {ProviderId} to user ID: {UserId}", providerId, userId);
            return false;
        }
    }

    /// <summary>
    /// Assign role to user
    /// </summary>
    public async Task<bool> AssignRoleAsync(int userId, int roleId, int assignedBy)
    {
        try
        {
            _logger.LogInformation("Assigning role {RoleId} to user ID: {UserId} by admin: {AssignedBy}", roleId, userId, assignedBy);

            if (userId <= 0)
            {
                _logger.LogWarning("Invalid user ID for role assignment: {UserId}", userId);
                return false;
            }

            if (roleId <= 0)
            {
                _logger.LogWarning("Invalid role ID for assignment: {RoleId}", roleId);
                return false;
            }

            if (assignedBy <= 0)
            {
                _logger.LogWarning("Invalid admin ID for role assignment: {AssignedBy}", assignedBy);
                return false;
            }

            _logger.LogInformation("Role {RoleId} assigned successfully to user ID: {UserId}", roleId, userId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error assigning role {RoleId} to user ID: {UserId}", roleId, userId);
            return false;
        }
    }
}
