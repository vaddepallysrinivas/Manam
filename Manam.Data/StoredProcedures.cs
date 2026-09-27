using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace Manam.Data;

/// <summary>
/// Data access layer for executing stored procedures against the Manam database
/// Synced with database schema on DESKTOP-O4ATQ75\MSSQLSERVER2\Manam
/// Tables: maRoles, maExternalProviders, maUsers, maUserRoles, maUserExternalAuth, maUserLoginHistory, maPasswordResetTokens
/// Stored Procedures: 15 total (sp_ma* prefix)
/// </summary>
public class StoredProcedures : IStoredProcedures
{
    private readonly string _connectionString;
    private readonly ILogger<StoredProcedures> _logger;

    /// <summary>
    /// Constructor with dependency injection
    /// </summary>
    public StoredProcedures(string connectionString, ILogger<StoredProcedures> logger)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new ArgumentNullException(nameof(connectionString));
        
        _connectionString = connectionString;
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    #region User Management SPs

    /// <summary>
    /// sp_maLoginUser - Authenticate user with email and password
    /// Returns: UserId, Email, PasswordHash, FirstName, LastName, IsActive, IsDeleted, CreatedAt
    /// </summary>
    public async Task<DataTable> sp_maLoginUserAsync(string email, string passwordHash, string ipAddress, string userAgent)
    {
        try
        {
            _logger.LogInformation("Executing sp_maLoginUser for email: {Email}", email);

            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                using (var command = new SqlCommand("sp_maLoginUser", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.CommandTimeout = 30;

                    command.Parameters.AddWithValue("@Email", email ?? "");
                    command.Parameters.AddWithValue("@PasswordHash", passwordHash ?? "");
                    command.Parameters.AddWithValue("@IpAddress", ipAddress ?? "");
                    command.Parameters.AddWithValue("@UserAgent", userAgent ?? "");

                    using (var adapter = new SqlDataAdapter(command))
                    {
                        var dataTable = new DataTable();
                        await Task.Run(() => adapter.Fill(dataTable));
                        
                        _logger.LogInformation("sp_maLoginUser executed successfully. Rows: {RowCount}", dataTable.Rows.Count);
                        return dataTable;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing sp_maLoginUser for email: {Email}", email);
            throw;
        }
    }

    /// <summary>
    /// sp_maInsertUser - Create new user account
    /// Returns: @UserId (output parameter)
    /// </summary>
    public async Task<int> sp_maInsertUserAsync(string username, string email, string passwordHash, 
        string firstName, string lastName, string phoneNumber)
    {
        try
        {
            _logger.LogInformation("Executing sp_maInsertUser for email: {Email}", email);

            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                using (var command = new SqlCommand("sp_maInsertUser", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.CommandTimeout = 30;

                    command.Parameters.AddWithValue("@Username", username ?? "");
                    command.Parameters.AddWithValue("@Email", email ?? "");
                    command.Parameters.AddWithValue("@PasswordHash", passwordHash ?? "");
                    command.Parameters.AddWithValue("@FirstName", firstName ?? "");
                    command.Parameters.AddWithValue("@LastName", lastName ?? "");
                    command.Parameters.AddWithValue("@PhoneNumber", phoneNumber ?? (object)DBNull.Value);

                    var returnValueParameter = command.Parameters.Add("@UserId", SqlDbType.Int);
                    returnValueParameter.Direction = ParameterDirection.Output;

                    await command.ExecuteNonQueryAsync();

                    var userId = (int)(returnValueParameter.Value ?? 0);
                    _logger.LogInformation("sp_maInsertUser executed successfully. UserId: {UserId}", userId);
                    
                    return userId;
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing sp_maInsertUser for email: {Email}", email);
            throw;
        }
    }

    /// <summary>
    /// sp_maUpdateUser - Update user profile information
    /// </summary>
    public async Task<bool> sp_maUpdateUserAsync(int userId, string firstName, string lastName, 
        string phoneNumber, DateTime? updatedAt = null)
    {
        try
        {
            _logger.LogInformation("Executing sp_maUpdateUser for UserId: {UserId}", userId);

            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                using (var command = new SqlCommand("sp_maUpdateUser", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.CommandTimeout = 30;

                    command.Parameters.AddWithValue("@UserId", userId);
                    command.Parameters.AddWithValue("@FirstName", firstName ?? "");
                    command.Parameters.AddWithValue("@LastName", lastName ?? "");
                    command.Parameters.AddWithValue("@PhoneNumber", phoneNumber ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@UpdatedAt", updatedAt ?? DateTime.UtcNow);

                    var result = await command.ExecuteNonQueryAsync();
                    
                    _logger.LogInformation("sp_maUpdateUser executed successfully for UserId: {UserId}", userId);
                    return result > 0;
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing sp_maUpdateUser for UserId: {UserId}", userId);
            throw;
        }
    }

    /// <summary>
    /// sp_maGetUserById - Retrieve user by ID (active users only, IsDeleted = 0)
    /// Returns: UserId, Username, Email, FirstName, LastName, PhoneNumber, IsEmailVerified, IsPhoneVerified, IsActive, CreatedAt, UpdatedAt
    /// </summary>
    public async Task<DataTable> sp_maGetUserByIdAsync(int userId)
    {
        try
        {
            _logger.LogInformation("Executing sp_maGetUserById for UserId: {UserId}", userId);

            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                using (var command = new SqlCommand("sp_maGetUserById", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.CommandTimeout = 30;

                    command.Parameters.AddWithValue("@UserId", userId);

                    using (var adapter = new SqlDataAdapter(command))
                    {
                        var dataTable = new DataTable();
                        await Task.Run(() => adapter.Fill(dataTable));
                        
                        _logger.LogInformation("sp_maGetUserById executed successfully. Rows: {RowCount}", dataTable.Rows.Count);
                        return dataTable;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing sp_maGetUserById for UserId: {UserId}", userId);
            throw;
        }
    }

    /// <summary>
    /// sp_maGetUserByEmail - Retrieve user by email address (active users only, IsDeleted = 0)
    /// Returns: UserId, Username, Email, FirstName, LastName, PhoneNumber, IsEmailVerified, IsPhoneVerified, IsActive, CreatedAt, UpdatedAt
    /// </summary>
    public async Task<DataTable> sp_maGetUserByEmailAsync(string email)
    {
        try
        {
            _logger.LogInformation("Executing sp_maGetUserByEmail for email: {Email}", email);

            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                using (var command = new SqlCommand("sp_maGetUserByEmail", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.CommandTimeout = 30;

                    command.Parameters.AddWithValue("@Email", email ?? "");

                    using (var adapter = new SqlDataAdapter(command))
                    {
                        var dataTable = new DataTable();
                        await Task.Run(() => adapter.Fill(dataTable));
                        
                        _logger.LogInformation("sp_maGetUserByEmail executed successfully. Rows: {RowCount}", dataTable.Rows.Count);
                        return dataTable;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing sp_maGetUserByEmail for email: {Email}", email);
            throw;
        }
    }

    /// <summary>
    /// sp_maGetAllUsers - Retrieve all users with pagination (active users only, IsDeleted = 0)
    /// Returns: DataTable with user list and @TotalCount (output parameter)
    /// </summary>
    public async Task<(DataTable users, int totalCount)> sp_maGetAllUsersAsync(int pageNumber, int pageSize)
    {
        try
        {
            _logger.LogInformation("Executing sp_maGetAllUsers. Page: {PageNumber}, Size: {PageSize}", pageNumber, pageSize);

            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                using (var command = new SqlCommand("sp_maGetAllUsers", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.CommandTimeout = 30;

                    command.Parameters.AddWithValue("@PageNumber", pageNumber);
                    command.Parameters.AddWithValue("@PageSize", pageSize);

                    var totalCountParam = command.Parameters.Add("@TotalCount", SqlDbType.Int);
                    totalCountParam.Direction = ParameterDirection.Output;

                    using (var adapter = new SqlDataAdapter(command))
                    {
                        var dataTable = new DataTable();
                        await Task.Run(() => adapter.Fill(dataTable));

                        int totalCount = (int)(totalCountParam.Value ?? 0);
                        
                        _logger.LogInformation("sp_maGetAllUsers executed successfully. Rows: {RowCount}, Total: {TotalCount}", 
                            dataTable.Rows.Count, totalCount);
                        
                        return (dataTable, totalCount);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing sp_maGetAllUsers");
            throw;
        }
    }

    /// <summary>
    /// sp_maSoftDeleteUser - Soft delete user account (GDPR compliant)
    /// Sets IsDeleted = 1 and DeletedAt = current timestamp
    /// </summary>
    public async Task<bool> sp_maSoftDeleteUserAsync(int userId)
    {
        try
        {
            _logger.LogInformation("Executing sp_maSoftDeleteUser for UserId: {UserId}", userId);

            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                using (var command = new SqlCommand("sp_maSoftDeleteUser", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.CommandTimeout = 30;

                    command.Parameters.AddWithValue("@UserId", userId);

                    var result = await command.ExecuteNonQueryAsync();
                    
                    _logger.LogInformation("sp_maSoftDeleteUser executed successfully for UserId: {UserId}", userId);
                    return result > 0;
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing sp_maSoftDeleteUser for UserId: {UserId}", userId);
            throw;
        }
    }

    #endregion

    #region Role Management SPs

    /// <summary>
    /// sp_maGetUserRoles - Get all roles assigned to a user
    /// Returns: RoleId, RoleName, Description
    /// </summary>
    public async Task<DataTable> sp_maGetUserRolesAsync(int userId)
    {
        try
        {
            _logger.LogInformation("Executing sp_maGetUserRoles for UserId: {UserId}", userId);

            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                using (var command = new SqlCommand("sp_maGetUserRoles", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.CommandTimeout = 30;

                    command.Parameters.AddWithValue("@UserId", userId);

                    using (var adapter = new SqlDataAdapter(command))
                    {
                        var dataTable = new DataTable();
                        await Task.Run(() => adapter.Fill(dataTable));
                        
                        _logger.LogInformation("sp_maGetUserRoles executed successfully. Roles: {RowCount}", dataTable.Rows.Count);
                        return dataTable;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing sp_maGetUserRoles for UserId: {UserId}", userId);
            throw;
        }
    }

    /// <summary>
    /// sp_maAssignRoleToUser - Assign a role to a user
    /// Adds entry to maUserRoles table
    /// </summary>
    public async Task<bool> sp_maAssignRoleToUserAsync(int userId, int roleId, int assignedBy)
    {
        try
        {
            _logger.LogInformation("Executing sp_maAssignRoleToUser for UserId: {UserId}, RoleId: {RoleId}", userId, roleId);

            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                using (var command = new SqlCommand("sp_maAssignRoleToUser", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.CommandTimeout = 30;

                    command.Parameters.AddWithValue("@UserId", userId);
                    command.Parameters.AddWithValue("@RoleId", roleId);
                    command.Parameters.AddWithValue("@AssignedBy", assignedBy);

                    var result = await command.ExecuteNonQueryAsync();
                    
                    _logger.LogInformation("sp_maAssignRoleToUser executed successfully for UserId: {UserId}, RoleId: {RoleId}", 
                        userId, roleId);
                    
                    return result > 0;
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing sp_maAssignRoleToUser for UserId: {UserId}, RoleId: {RoleId}", userId, roleId);
            throw;
        }
    }

    /// <summary>
    /// sp_maGetUserWithRoles - Get user profile with all roles and external auth providers
    /// Returns: User details plus associated roles and external auth providers
    /// </summary>
    public async Task<DataTable> sp_maGetUserWithRolesAsync(int userId)
    {
        try
        {
            _logger.LogInformation("Executing sp_maGetUserWithRoles for UserId: {UserId}", userId);

            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                using (var command = new SqlCommand("sp_maGetUserWithRoles", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.CommandTimeout = 30;

                    command.Parameters.AddWithValue("@UserId", userId);

                    using (var adapter = new SqlDataAdapter(command))
                    {
                        var dataTable = new DataTable();
                        await Task.Run(() => adapter.Fill(dataTable));
                        
                        _logger.LogInformation("sp_maGetUserWithRoles executed successfully. Rows: {RowCount}", dataTable.Rows.Count);
                        return dataTable;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing sp_maGetUserWithRoles for UserId: {UserId}", userId);
            throw;
        }
    }

    #endregion

    #region External Provider SPs

    /// <summary>
    /// sp_maAddExternalAuth - Link external authentication provider (Google, Facebook, GitHub)
    /// ProviderId: 1=Google, 2=Facebook, 3=GitHub
    /// </summary>
    public async Task<bool> sp_maAddExternalAuthAsync(int userId, int providerId, string externalUserId, 
        string externalEmail, string accessToken)
    {
        try
        {
            _logger.LogInformation("Executing sp_maAddExternalAuth for UserId: {UserId}, ProviderId: {ProviderId}", 
                userId, providerId);

            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                using (var command = new SqlCommand("sp_maAddExternalAuth", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.CommandTimeout = 30;

                    command.Parameters.AddWithValue("@UserId", userId);
                    command.Parameters.AddWithValue("@ProviderId", providerId);
                    command.Parameters.AddWithValue("@ExternalUserId", externalUserId ?? "");
                    command.Parameters.AddWithValue("@ExternalEmail", externalEmail ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@AccessToken", accessToken ?? "");

                    var result = await command.ExecuteNonQueryAsync();
                    
                    _logger.LogInformation("sp_maAddExternalAuth executed successfully for UserId: {UserId}, ProviderId: {ProviderId}", 
                        userId, providerId);
                    
                    return result > 0;
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing sp_maAddExternalAuth for UserId: {UserId}, ProviderId: {ProviderId}", 
                userId, providerId);
            throw;
        }
    }

    #endregion

    #region Authentication History SPs

    /// <summary>
    /// sp_maLogLoginHistory - Log user login attempt with IP and user agent
    /// Tracks all login attempts (successful and failed) for audit trail
    /// </summary>
    public async Task<bool> sp_maLogLoginHistoryAsync(int userId, string ipAddress, string userAgent, bool isSuccessful)
    {
        try
        {
            _logger.LogInformation("Executing sp_maLogLoginHistory for UserId: {UserId}, IsSuccessful: {IsSuccessful}", 
                userId, isSuccessful);

            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                using (var command = new SqlCommand("sp_maLogLoginHistory", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.CommandTimeout = 30;

                    command.Parameters.AddWithValue("@UserId", userId);
                    command.Parameters.AddWithValue("@IpAddress", ipAddress ?? "");
                    command.Parameters.AddWithValue("@UserAgent", userAgent ?? "");
                    command.Parameters.AddWithValue("@IsSuccessful", isSuccessful);
                    command.Parameters.AddWithValue("@LoginAt", DateTime.UtcNow);

                    var result = await command.ExecuteNonQueryAsync();
                    
                    _logger.LogInformation("sp_maLogLoginHistory executed successfully for UserId: {UserId}", userId);
                    return result > 0;
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing sp_maLogLoginHistory for UserId: {UserId}", userId);
            throw;
        }
    }

    #endregion

    #region Password Reset SPs

    /// <summary>
    /// sp_maCreatePasswordResetToken - Generate password reset token
    /// Inserts into maPasswordResetTokens table with expiration time
    /// </summary>
    public async Task<string> sp_maCreatePasswordResetTokenAsync(int userId, string token, DateTime expiresAt)
    {
        try
        {
            _logger.LogInformation("Executing sp_maCreatePasswordResetToken for UserId: {UserId}", userId);

            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                using (var command = new SqlCommand("sp_maCreatePasswordResetToken", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.CommandTimeout = 30;

                    command.Parameters.AddWithValue("@UserId", userId);
                    command.Parameters.AddWithValue("@Token", token ?? "");
                    command.Parameters.AddWithValue("@ExpiresAt", expiresAt);

                    var tokenParam = command.Parameters.Add("@GeneratedToken", SqlDbType.NVarChar, 500);
                    tokenParam.Direction = ParameterDirection.Output;

                    await command.ExecuteNonQueryAsync();

                    var generatedToken = tokenParam.Value?.ToString() ?? token;
                    _logger.LogInformation("sp_maCreatePasswordResetToken executed successfully for UserId: {UserId}", userId);
                    
                    return generatedToken;
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing sp_maCreatePasswordResetToken for UserId: {UserId}", userId);
            throw;
        }
    }

    /// <summary>
    /// sp_maResetPassword - Reset user password with token validation
    /// Validates token expiration and updates password hash
    /// </summary>
    public async Task<bool> sp_maResetPasswordAsync(int userId, string token, string newPasswordHash)
    {
        try
        {
            _logger.LogInformation("Executing sp_maResetPassword for UserId: {UserId}", userId);

            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                using (var command = new SqlCommand("sp_maResetPassword", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.CommandTimeout = 30;

                    command.Parameters.AddWithValue("@UserId", userId);
                    command.Parameters.AddWithValue("@Token", token ?? "");
                    command.Parameters.AddWithValue("@NewPasswordHash", newPasswordHash ?? "");

                    var result = await command.ExecuteNonQueryAsync();
                    
                    _logger.LogInformation("sp_maResetPassword executed successfully for UserId: {UserId}", userId);
                    return result > 0;
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing sp_maResetPassword for UserId: {UserId}", userId);
            throw;
        }
    }

    /// <summary>
    /// sp_maVerifyEmail - Mark user email as verified
    /// Sets IsEmailVerified = 1 in maUsers table
    /// </summary>
    public async Task<bool> sp_maVerifyEmailAsync(int userId)
    {
        try
        {
            _logger.LogInformation("Executing sp_maVerifyEmail for UserId: {UserId}", userId);

            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                using (var command = new SqlCommand("sp_maVerifyEmail", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.CommandTimeout = 30;

                    command.Parameters.AddWithValue("@UserId", userId);

                    var result = await command.ExecuteNonQueryAsync();
                    
                    _logger.LogInformation("sp_maVerifyEmail executed successfully for UserId: {UserId}", userId);
                    return result > 0;
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing sp_maVerifyEmail for UserId: {UserId}", userId);
            throw;
        }
    }

    #endregion
}
