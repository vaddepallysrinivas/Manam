using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace Manam.Data;

/// <summary>
/// Interface for stored procedure execution
/// Defines all data access methods for user management operations
/// Database: DESKTOP-O4ATQ75\MSSQLSERVER2\Manam
/// </summary>
public interface IStoredProcedures
{
    #region User Management

    /// <summary>
    /// Authenticate user with email and password
    /// </summary>
    Task<DataTable> sp_maLoginUserAsync(string email, string passwordHash, string ipAddress, string userAgent);

    /// <summary>
    /// Create new user account
    /// </summary>
    Task<int> sp_maInsertUserAsync(string username, string email, string passwordHash, 
        string firstName, string lastName, string phoneNumber);

    /// <summary>
    /// Update user profile
    /// </summary>
    Task<bool> sp_maUpdateUserAsync(int userId, string firstName, string lastName, 
        string phoneNumber, DateTime? updatedAt = null);

    /// <summary>
    /// Get user by ID
    /// </summary>
    Task<DataTable> sp_maGetUserByIdAsync(int userId);

    /// <summary>
    /// Get user by email
    /// </summary>
    Task<DataTable> sp_maGetUserByEmailAsync(string email);

    /// <summary>
    /// Get all users with pagination
    /// </summary>
    Task<(DataTable users, int totalCount)> sp_maGetAllUsersAsync(int pageNumber, int pageSize);

    /// <summary>
    /// Soft delete user (GDPR compliant)
    /// </summary>
    Task<bool> sp_maSoftDeleteUserAsync(int userId);

    #endregion

    #region Roles

    /// <summary>
    /// Get all roles for a user
    /// </summary>
    Task<DataTable> sp_maGetUserRolesAsync(int userId);

    /// <summary>
    /// Assign role to user
    /// </summary>
    Task<bool> sp_maAssignRoleToUserAsync(int userId, int roleId, int assignedBy);

    /// <summary>
    /// Get user with all roles and external auth
    /// </summary>
    Task<DataTable> sp_maGetUserWithRolesAsync(int userId);

    #endregion

    #region External Providers

    /// <summary>
    /// Link external authentication provider
    /// </summary>
    Task<bool> sp_maAddExternalAuthAsync(int userId, int providerId, string externalUserId, 
        string externalEmail, string accessToken);

    #endregion

    #region Authentication History

    /// <summary>
    /// Log login attempt
    /// </summary>
    Task<bool> sp_maLogLoginHistoryAsync(int userId, string ipAddress, string userAgent, bool isSuccessful);

    #endregion

    #region Password Reset

    /// <summary>
    /// Create password reset token
    /// </summary>
    Task<string> sp_maCreatePasswordResetTokenAsync(int userId, string token, DateTime expiresAt);

    /// <summary>
    /// Reset password with token
    /// </summary>
    Task<bool> sp_maResetPasswordAsync(int userId, string token, string newPasswordHash);

    /// <summary>
    /// Verify email address
    /// </summary>
    Task<bool> sp_maVerifyEmailAsync(int userId);

    #endregion
}
