using Manam.Models.UserManagement;

namespace Manam.Services.Abstractions.UserManagement;

/// <summary>
/// Service interface for user authentication operations
/// </summary>
public interface IAuthenticationService
{
    /// <summary>
    /// Authenticate user with email and password
    /// </summary>
    /// <param name="request">Login request with credentials</param>
    /// <returns>Login response with JWT token if successful</returns>
    Task<LoginResponse> LoginAsync(LoginRequest request);

    /// <summary>
    /// Register a new user account
    /// </summary>
    /// <param name="request">Registration request with user details</param>
    /// <returns>Registration response with new user ID if successful</returns>
    Task<RegisterResponse> RegisterAsync(RegisterRequest request);

    /// <summary>
    /// Validate JWT token
    /// </summary>
    /// <param name="token">JWT token to validate</param>
    /// <returns>User ID if token is valid, null otherwise</returns>
    Task<int?> ValidateTokenAsync(string token);

    /// <summary>
    /// Refresh JWT token
    /// </summary>
    /// <param name="refreshToken">Refresh token</param>
    /// <returns>New JWT token if refresh is successful</returns>
    Task<string?> RefreshTokenAsync(string refreshToken);

    /// <summary>
    /// Request password reset
    /// </summary>
    /// <param name="email">User email</param>
    /// <returns>Password reset token</returns>
    Task<string?> RequestPasswordResetAsync(string email);

    /// <summary>
    /// Reset password with reset token
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="token">Password reset token</param>
    /// <param name="newPassword">New password</param>
    /// <returns>True if password reset successful</returns>
    Task<bool> ResetPasswordAsync(int userId, string token, string newPassword);

    /// <summary>
    /// Verify user email
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <returns>True if email verification successful</returns>
    Task<bool> VerifyEmailAsync(int userId);
}
