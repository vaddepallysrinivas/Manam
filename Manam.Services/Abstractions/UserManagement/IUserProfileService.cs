using Manam.Models.UserManagement;

namespace Manam.Services.Abstractions.UserManagement;

/// <summary>
/// Service interface for user profile management operations
/// </summary>
public interface IUserProfileService
{
    /// <summary>
    /// Get user profile by user ID
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <returns>User profile with roles and external auth info</returns>
    Task<UserProfileResponse?> GetProfileAsync(int userId);

    /// <summary>
    /// Update user profile information
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="firstName">First name</param>
    /// <param name="lastName">Last name</param>
    /// <param name="phoneNumber">Phone number</param>
    /// <returns>True if update successful</returns>
    Task<bool> UpdateProfileAsync(int userId, string? firstName, string? lastName, string? phoneNumber);

    /// <summary>
    /// Delete user account (soft delete)
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <returns>True if delete successful</returns>
    Task<bool> DeleteUserAsync(int userId);

    /// <summary>
    /// Get all users with pagination
    /// </summary>
    /// <param name="pageNumber">Page number (1-based)</param>
    /// <param name="pageSize">Items per page</param>
    /// <returns>List of users with total count</returns>
    Task<(List<UserProfileResponse> Users, int TotalCount)> GetAllUsersAsync(int pageNumber, int pageSize);

    /// <summary>
    /// Link external authentication provider
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="providerId">Provider ID (1=Google, 2=Facebook, 3=GitHub)</param>
    /// <param name="externalUserId">External user ID</param>
    /// <param name="externalEmail">External email</param>
    /// <param name="accessToken">OAuth access token</param>
    /// <returns>True if link successful</returns>
    Task<bool> LinkExternalProviderAsync(int userId, int providerId, string externalUserId, string? externalEmail, string? accessToken);

    /// <summary>
    /// Assign role to user
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="roleId">Role ID</param>
    /// <param name="assignedBy">Admin user ID who assigned the role</param>
    /// <returns>True if assignment successful</returns>
    Task<bool> AssignRoleAsync(int userId, int roleId, int assignedBy);
}
