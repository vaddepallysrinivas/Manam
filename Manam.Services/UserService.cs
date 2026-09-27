using System.Security.Cryptography;
using System.Text;
using Manam.Models;
using Manam.Storage;

namespace Manam.Services;

/// <summary>
/// User service implementation
/// </summary>
public sealed class UserService : IUserService
{
    private readonly IUserRepository _userRepository;

    public UserService(IUserRepository userRepository)
    {
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
    }

    public async Task<UserDto?> GetUserByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(id, cancellationToken);
        return MapToDto(user);
    }

    public async Task<IReadOnlyList<UserDto>> GetAllUsersAsync(CancellationToken cancellationToken = default)
    {
        var users = await _userRepository.GetAllAsync(cancellationToken);
        return users.Select(MapToDto).ToList().AsReadOnly();
    }

    public async Task<UserDto?> GetUserByUsernameAsync(string username, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByUsernameAsync(username, cancellationToken);
        return MapToDto(user);
    }

    public async Task<UserDto?> GetUserByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByEmailAsync(email, cancellationToken);
        return MapToDto(user);
    }

    public async Task<Guid> CreateUserAsync(CreateUserRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Username) ||
            string.IsNullOrWhiteSpace(request.Email) ||
            string.IsNullOrWhiteSpace(request.Password))
        {
            throw new ArgumentException("Username, Email, and Password are required.");
        }

        // Check if user already exists
        var existingUser = await _userRepository.GetByUsernameAsync(request.Username, cancellationToken);
        if (existingUser != null)
        {
            throw new InvalidOperationException("Username already exists.");
        }

        var userId = Guid.NewGuid();
        var passwordHash = HashPassword(request.Password);

        var user = new User
        {
            Id = userId,
            Username = request.Username,
            Email = request.Email,
            PasswordHash = passwordHash,
            FirstName = request.FirstName,
            LastName = request.LastName,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _userRepository.CreateAsync(user, cancellationToken);
        return userId;
    }

    public async Task<bool> UpdateUserAsync(Guid id, UpdateUserRequest request, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(id, cancellationToken);
        if (user == null)
        {
            return false;
        }

        if (!string.IsNullOrWhiteSpace(request.Email))
            user.Email = request.Email;

        if (!string.IsNullOrWhiteSpace(request.FirstName))
            user.FirstName = request.FirstName;

        if (!string.IsNullOrWhiteSpace(request.LastName))
            user.LastName = request.LastName;

        if (request.IsActive.HasValue)
            user.IsActive = request.IsActive.Value;

        user.UpdatedAt = DateTime.UtcNow;

        var result = await _userRepository.UpdateAsync(user, cancellationToken);
        return result > 0;
    }

    public async Task<bool> DeleteUserAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var result = await _userRepository.DeleteAsync(id, cancellationToken);
        return result > 0;
    }

    private static UserDto? MapToDto(User? user)
    {
        if (user == null)
            return null;

        return new UserDto
        {
            Id = user.Id,
            Username = user.Username,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            IsActive = user.IsActive
        };
    }

    private static string HashPassword(string password)
    {
        using var sha256 = SHA256.Create();
        var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
        return Convert.ToBase64String(hashedBytes);
    }
}
