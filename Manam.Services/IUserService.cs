using Manam.Models;

namespace Manam.Services;

/// <summary>
/// User service interface
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

/// <summary>
/// Create user request DTO
/// </summary>
public class CreateUserRequest
{
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
}

/// <summary>
/// Update user request DTO
/// </summary>
public class UpdateUserRequest
{
    public string? Email { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public bool? IsActive { get; set; }
}
