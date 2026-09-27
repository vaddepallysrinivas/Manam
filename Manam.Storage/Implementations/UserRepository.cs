using System.Data;
using Manam.DatabaseClient;
using Manam.DatabaseClient.Abstractions;
using Manam.Models;
using Manam.Storage.Abstractions;
using Microsoft.Extensions.Logging;

namespace Manam.Storage.Implementations;

/// <summary>
/// User repository implementation using Dapper and SQL Server stored procedures.
/// All operations are fully async and support cancellation tokens.
/// </summary>
public sealed class UserRepository : IUserRepository
{
    private readonly Manam.DatabaseClient.Abstractions.ISqlDapperBroker _broker;
    private readonly ILogger<UserRepository> _logger;

    public UserRepository(
        Manam.DatabaseClient.Abstractions.ISqlDapperBroker broker,
        ILogger<UserRepository> logger)
    {
        _broker = broker ?? throw new ArgumentNullException(nameof(broker));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Gets a user by ID from the database.
    /// </summary>
    public async Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Fetching user with ID: {UserId}", id);
            var parameters = new { Id = id };

            var user = await _broker.QuerySingleOrDefaultAsync<User>(
                StoredProcedures.SpGetUserById,
                parameters,
                CommandType.StoredProcedure,
                cancellationToken);

            if (user != null)
                _logger.LogDebug("User found: {Username}", user.Username);
            else
                _logger.LogDebug("User not found with ID: {UserId}", id);

            return user;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching user with ID: {UserId}", id);
            throw;
        }
    }

    /// <summary>
    /// Gets all users from the database.
    /// </summary>
    public async Task<IReadOnlyList<User>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Fetching all users from database");
            var users = await _broker.QueryAsync<User>(
                StoredProcedures.SpGetAllUsers,
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken);

            _logger.LogDebug("Retrieved {UserCount} users from database", users.Count);
            return users;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching all users from database");
            throw;
        }
    }

    /// <summary>
    /// Gets a user by username from the database.
    /// </summary>
    public async Task<User?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(username))
            throw new ArgumentException("Username cannot be null or empty", nameof(username));

        try
        {
            _logger.LogDebug("Fetching user with username: {Username}", username);
            var parameters = new { Username = username };

            var user = await _broker.QuerySingleOrDefaultAsync<User>(
                StoredProcedures.SpGetUserByUsername,
                parameters,
                CommandType.StoredProcedure,
                cancellationToken);

            if (user != null)
                _logger.LogDebug("User found with username: {Username}", username);
            else
                _logger.LogDebug("User not found with username: {Username}", username);

            return user;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching user with username: {Username}", username);
            throw;
        }
    }

    /// <summary>
    /// Gets a user by email from the database.
    /// </summary>
    public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email cannot be null or empty", nameof(email));

        try
        {
            _logger.LogDebug("Fetching user with email: {Email}", email);
            var parameters = new { Email = email };

            var user = await _broker.QuerySingleOrDefaultAsync<User>(
                StoredProcedures.SpGetUserByEmail,
                parameters,
                CommandType.StoredProcedure,
                cancellationToken);

            if (user != null)
                _logger.LogDebug("User found with email: {Email}", email);
            else
                _logger.LogDebug("User not found with email: {Email}", email);

            return user;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching user with email: {Email}", email);
            throw;
        }
    }

    /// <summary>
    /// Creates a new user in the database.
    /// </summary>
    public async Task<int> CreateAsync(User entity, CancellationToken cancellationToken = default)
    {
        if (entity == null)
            throw new ArgumentNullException(nameof(entity));

        try
        {
            _logger.LogDebug("Creating new user: {Username}", entity.Username);
            var parameters = new
            {
                Id = entity.Id,
                Username = entity.Username,
                Email = entity.Email,
                PasswordHash = entity.PasswordHash,
                FirstName = entity.FirstName,
                LastName = entity.LastName,
                IsActive = entity.IsActive
            };

            var result = await _broker.ExecuteAsync(
                StoredProcedures.SpCreateUser,
                parameters,
                CommandType.StoredProcedure,
                cancellationToken);

            if (result > 0)
                _logger.LogInformation("User created successfully: {UserId}", entity.Id);
            else
                _logger.LogWarning("Failed to create user: {Username}", entity.Username);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating user: {Username}", entity.Username);
            throw;
        }
    }

    /// <summary>
    /// Updates an existing user in the database.
    /// </summary>
    public async Task<int> UpdateAsync(User entity, CancellationToken cancellationToken = default)
    {
        if (entity == null)
            throw new ArgumentNullException(nameof(entity));

        try
        {
            _logger.LogDebug("Updating user: {UserId}", entity.Id);
            var parameters = new
            {
                Id = entity.Id,
                Username = entity.Username,
                Email = entity.Email,
                PasswordHash = entity.PasswordHash,
                FirstName = entity.FirstName,
                LastName = entity.LastName,
                IsActive = entity.IsActive
            };

            var result = await _broker.ExecuteAsync(
                StoredProcedures.SpUpdateUser,
                parameters,
                CommandType.StoredProcedure,
                cancellationToken);

            if (result > 0)
                _logger.LogInformation("User updated successfully: {UserId}", entity.Id);
            else
                _logger.LogWarning("Failed to update user: {UserId}", entity.Id);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user: {UserId}", entity.Id);
            throw;
        }
    }

    /// <summary>
    /// Deletes a user from the database.
    /// </summary>
    public async Task<int> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Deleting user: {UserId}", id);
            var parameters = new { Id = id };

            var result = await _broker.ExecuteAsync(
                StoredProcedures.SpDeleteUser,
                parameters,
                CommandType.StoredProcedure,
                cancellationToken);

            if (result > 0)
                _logger.LogInformation("User deleted successfully: {UserId}", id);
            else
                _logger.LogWarning("Failed to delete user: {UserId}", id);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting user: {UserId}", id);
            throw;
        }
    }

    /// <summary>
    /// Updates the lockout time for a user (for account security).
    /// </summary>
    public async Task<int> UpdateLockoutAsync(Guid userId, DateTime? lockoutUntil, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Updating lockout for user: {UserId}, LockoutUntil: {LockoutUntil}", userId, lockoutUntil);
            var parameters = new { Id = userId, LockoutUntil = lockoutUntil };

            var result = await _broker.ExecuteAsync(
                StoredProcedures.SpUpdateUserLockout,
                parameters,
                CommandType.StoredProcedure,
                cancellationToken);

            if (result > 0)
                _logger.LogInformation("User lockout updated: {UserId}", userId);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating lockout for user: {UserId}", userId);
            throw;
        }
    }

    /// <summary>
    /// Resets failed login attempts for a user.
    /// </summary>
    public async Task<int> ResetFailedLoginAttemptsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Resetting failed login attempts for user: {UserId}", userId);
            var parameters = new { Id = userId };

            var result = await _broker.ExecuteAsync(
                StoredProcedures.SpResetFailedLoginAttempts,
                parameters,
                CommandType.StoredProcedure,
                cancellationToken);

            if (result > 0)
                _logger.LogInformation("Failed login attempts reset for user: {UserId}", userId);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resetting failed login attempts for user: {UserId}", userId);
            throw;
        }
    }
}
