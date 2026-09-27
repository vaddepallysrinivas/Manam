using Manam.Models;

namespace Manam.Services.Abstractions;

/// <summary>
/// User service interface for business logic operations
/// </summary>
public interface IUserService
{
    Task<UserDto?> GetUserByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<UserDto>> GetAllUsersAsync(CancellationToken cancellationToken = default);
    Task<UserDto?> GetUserByUsernameAsync(string username, CancellationToken cancellationToken = default);
    Task<UserDto?> GetUserByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<Guid> CreateUserAsync(CreateUserRequest request, CancellationToken cancellationToken = default);
    Task<bool> UpdateUserAsync(Guid id, UpdateUserRequest request, CancellationToken cancellationToken = default);
    Task<bool> DeleteUserAsync(Guid id, CancellationToken cancellationToken = default);
}
