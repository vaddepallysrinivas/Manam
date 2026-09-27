using Manam.Models;

namespace Manam.Storage.Abstractions;

/// <summary>
/// User repository interface for user-specific data access
/// </summary>
public interface IUserRepository : IRepository<User>
{
    Task<User?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default);
    Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<int> UpdateLockoutAsync(Guid userId, DateTime? lockoutUntil, CancellationToken cancellationToken = default);
    Task<int> ResetFailedLoginAttemptsAsync(Guid userId, CancellationToken cancellationToken = default);
}
