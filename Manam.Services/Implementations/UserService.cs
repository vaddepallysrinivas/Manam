using System.Security.Cryptography;
using System.Text;
using Manam.Models;
using Manam.Services.Abstractions;
using Manam.Storage.Abstractions;
using Microsoft.Extensions.Logging;

namespace Manam.Services.Implementations;

/// <summary>
/// User service implementation with business logic and validation.
/// Handles user lifecycle operations including creation, updates, and authentication-related tasks.
/// </summary>
public sealed class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly ILogger<UserService> _logger;
    private const int MaxFailedLoginAttempts = 5;
    private readonly TimeSpan _lockoutDuration = TimeSpan.FromMinutes(15);

    public UserService(
        IUserRepository userRepository,
        ILogger<UserService> logger)
    {
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Gets a user by ID, returning only safe-to-display information.
    /// </summary>
    public async Task<UserDto?> GetUserByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Retrieving user by ID: {UserId}", id);
            var user = await _userRepository.GetByIdAsync(id, cancellationToken);
            return MapToDto(user);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving user by ID: {UserId}", id);
            throw;
        }
    }

    /// <summary>
    /// Gets all users from the system.
    /// </summary>
    public async Task<IReadOnlyList<UserDto>> GetAllUsersAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Retrieving all users from system");
            var users = await _userRepository.GetAllAsync(cancellationToken);
            var dtos = users.Select(MapToDto).ToList().AsReadOnly();
            _logger.LogDebug("Retrieved {UserCount} users from system", dtos.Count);
            return dtos;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all users from system");
            throw;
        }
    }

    /// <summary>
    /// Gets a user by username.
    /// </summary>
    public async Task<UserDto?> GetUserByUsernameAsync(string username, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Retrieving user by username: {Username}", username);
            var user = await _userRepository.GetByUsernameAsync(username, cancellationToken);
            return MapToDto(user);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving user by username: {Username}", username);
            throw;
        }
    }

    /// <summary>
    /// Gets a user by email address.
    /// </summary>
    public async Task<UserDto?> GetUserByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Retrieving user by email");
            var user = await _userRepository.GetByEmailAsync(email, cancellationToken);
            return MapToDto(user);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving user by email");
            throw;
        }
    }

    /// <summary>
    /// Creates a new user with validation and duplicate checking.
    /// </summary>
    public async Task<Guid> CreateUserAsync(CreateUserRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            // Validate input
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            if (string.IsNullOrWhiteSpace(request.Username))
                throw new ArgumentException("Username is required.", nameof(request.Username));

            if (string.IsNullOrWhiteSpace(request.Email))
                throw new ArgumentException("Email is required.", nameof(request.Email));

            if (string.IsNullOrWhiteSpace(request.Password))
                throw new ArgumentException("Password is required.", nameof(request.Password));

            if (request.Password.Length < 8)
                throw new ArgumentException("Password must be at least 8 characters long.", nameof(request.Password));

            // Check if username already exists
            _logger.LogDebug("Checking if username already exists: {Username}", request.Username);
            var existingUserByUsername = await _userRepository.GetByUsernameAsync(request.Username, cancellationToken);
            if (existingUserByUsername != null)
            {
                _logger.LogWarning("Username already exists: {Username}", request.Username);
                throw new InvalidOperationException($"Username '{request.Username}' already exists.");
            }

            // Check if email already exists
            _logger.LogDebug("Checking if email already exists");
            var existingUserByEmail = await _userRepository.GetByEmailAsync(request.Email, cancellationToken);
            if (existingUserByEmail != null)
            {
                _logger.LogWarning("Email already registered");
                throw new InvalidOperationException($"Email '{request.Email}' is already registered.");
            }

            var userId = Guid.NewGuid();
            var passwordHash = HashPassword(request.Password);
            var now = DateTime.UtcNow;

            var user = new User
            {
                Id = userId,
                Username = request.Username,
                Email = request.Email,
                PasswordHash = passwordHash,
                FirstName = request.FirstName ?? string.Empty,
                LastName = request.LastName ?? string.Empty,
                IsActive = true,
                FailedLoginAttempts = 0,
                LockoutUntil = null,
                CreatedAt = now,
                UpdatedAt = now
            };

            _logger.LogInformation("Creating new user: {Username}", request.Username);
            await _userRepository.CreateAsync(user, cancellationToken);
            _logger.LogInformation("User created successfully: {UserId}", userId);
            return userId;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating user");
            throw;
        }
    }

    /// <summary>
    /// Updates an existing user with selective field updates.
    /// </summary>
    public async Task<bool> UpdateUserAsync(Guid id, UpdateUserRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            _logger.LogDebug("Fetching user for update: {UserId}", id);
            var user = await _userRepository.GetByIdAsync(id, cancellationToken);
            if (user == null)
            {
                _logger.LogWarning("User not found for update: {UserId}", id);
                return false;
            }

            bool updated = false;

            // Update email if provided
            if (!string.IsNullOrWhiteSpace(request.Email) && request.Email != user.Email)
            {
                // Check if new email is already in use
                var existingUser = await _userRepository.GetByEmailAsync(request.Email, cancellationToken);
                if (existingUser != null && existingUser.Id != id)
                {
                    throw new InvalidOperationException($"Email '{request.Email}' is already in use.");
                }
                user.Email = request.Email;
                updated = true;
            }

            // Update first name if provided
            if (!string.IsNullOrWhiteSpace(request.FirstName) && request.FirstName != user.FirstName)
            {
                user.FirstName = request.FirstName;
                updated = true;
            }

            // Update last name if provided
            if (!string.IsNullOrWhiteSpace(request.LastName) && request.LastName != user.LastName)
            {
                user.LastName = request.LastName;
                updated = true;
            }

            // Update active status if provided
            if (request.IsActive.HasValue && request.IsActive.Value != user.IsActive)
            {
                user.IsActive = request.IsActive.Value;
                updated = true;
            }

            // Only update if something changed
            if (updated)
            {
                user.UpdatedAt = DateTime.UtcNow;
                _logger.LogInformation("Updating user: {UserId}", id);
                var result = await _userRepository.UpdateAsync(user, cancellationToken);
                return result > 0;
            }

            _logger.LogDebug("No changes to update for user: {UserId}", id);
            return true; // No changes needed
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user: {UserId}", id);
            throw;
        }
    }

    /// <summary>
    /// Deletes a user from the system.
    /// </summary>
    public async Task<bool> DeleteUserAsync(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Deleting user: {UserId}", id);
            var result = await _userRepository.DeleteAsync(id, cancellationToken);
            if (result > 0)
                _logger.LogInformation("User deleted successfully: {UserId}", id);
            else
                _logger.LogWarning("Failed to delete user: {UserId}", id);
            return result > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting user: {UserId}", id);
            throw;
        }
    }

    /// <summary>
    /// Maps a User entity to UserDto, excluding sensitive information.
    /// </summary>
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

    /// <summary>
    /// Hashes a password using SHA256.
    /// In production, use BCrypt or Argon2 for better security.
    /// </summary>
    private static string HashPassword(string password)
    {
        if (string.IsNullOrWhiteSpace(password))
            throw new ArgumentException("Password cannot be null or empty", nameof(password));

        using var sha256 = SHA256.Create();
        var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
        return Convert.ToBase64String(hashedBytes);
    }
}
