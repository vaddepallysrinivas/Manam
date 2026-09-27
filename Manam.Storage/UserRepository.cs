using System.Data;
using Manam.DatabaseClient;
using Manam.Models;

namespace Manam.Storage;

/// <summary>
/// User repository interface
/// </summary>
public interface IUserRepository : IRepository<User>
{
    Task<User?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default);
    Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<int> UpdateLockoutAsync(Guid userId, DateTime? lockoutUntil, CancellationToken cancellationToken = default);
    Task<int> ResetFailedLoginAttemptsAsync(Guid userId, CancellationToken cancellationToken = default);
}

/// <summary>
/// User repository implementation using Dapper
/// </summary>
public sealed class UserRepository : IUserRepository
{
    private readonly ISqlDapperBroker _broker;

    public UserRepository(ISqlDapperBroker broker)
    {
        _broker = broker ?? throw new ArgumentNullException(nameof(broker));
    }

    public async Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var parameters = new { userId = id };

        return await _broker.QuerySingleOrDefaultAsync<User>(
            StoredProcedures.SpGetUserById,
            parameters,
            CommandType.StoredProcedure,
            cancellationToken);
    }

    public async Task<IReadOnlyList<User>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _broker.QueryAsync<User>(
            StoredProcedures.SpGetAllUsers,
            commandType: CommandType.StoredProcedure,
            cancellationToken: cancellationToken);
    }

    public async Task<User?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default)
    {
        var parameters = new { username };

        return await _broker.QuerySingleOrDefaultAsync<User>(
            StoredProcedures.SpGetUserByUsername,
            parameters,
            CommandType.StoredProcedure,
            cancellationToken);
    }

    public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        var parameters = new { email };

        return await _broker.QuerySingleOrDefaultAsync<User>(
            StoredProcedures.SpGetUserByEmail,
            parameters,
            CommandType.StoredProcedure,
            cancellationToken);
    }

    public async Task<int> CreateAsync(User entity, CancellationToken cancellationToken = default)
    {
        var parameters = new
        {
            entity.Id,
            entity.Username,
            entity.Email,
            entity.PasswordHash,
            entity.FirstName,
            entity.LastName,
            entity.IsActive,
            entity.CreatedAt,
            entity.UpdatedAt
        };

        return await _broker.ExecuteAsync(
            StoredProcedures.SpCreateUser,
            parameters,
            CommandType.StoredProcedure,
            cancellationToken);
    }

    public async Task<int> UpdateAsync(User entity, CancellationToken cancellationToken = default)
    {
        var parameters = new
        {
            entity.Id,
            entity.Username,
            entity.Email,
            entity.FirstName,
            entity.LastName,
            entity.IsActive,
            entity.UpdatedAt
        };

        return await _broker.ExecuteAsync(
            StoredProcedures.SpUpdateUser,
            parameters,
            CommandType.StoredProcedure,
            cancellationToken);
    }

    public async Task<int> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var parameters = new { userId = id };

        return await _broker.ExecuteAsync(
            StoredProcedures.SpDeleteUser,
            parameters,
            CommandType.StoredProcedure,
            cancellationToken);
    }

    public async Task<int> UpdateLockoutAsync(Guid userId, DateTime? lockoutUntil, CancellationToken cancellationToken = default)
    {
        var parameters = new { userId, lockoutUntil };

        return await _broker.ExecuteAsync(
            StoredProcedures.SpUpdateUserLockout,
            parameters,
            CommandType.StoredProcedure,
            cancellationToken);
    }

    public async Task<int> ResetFailedLoginAttemptsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var parameters = new { userId };

        return await _broker.ExecuteAsync(
            StoredProcedures.SpResetFailedLoginAttempts,
            parameters,
            CommandType.StoredProcedure,
            cancellationToken);
    }
}
