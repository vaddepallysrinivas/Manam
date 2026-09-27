namespace Manam.Models.UserManagement;

/// <summary>
/// Response model for successful login with user and token details
/// </summary>
public class LoginResponse
{
    /// <summary>
    /// Login success flag
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Status message
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// JWT token for authenticated requests
    /// </summary>
    public string? Token { get; set; }

    /// <summary>
    /// Refresh token for token renewal
    /// </summary>
    public string? RefreshToken { get; set; }

    /// <summary>
    /// User ID
    /// </summary>
    public int? UserId { get; set; }

    /// <summary>
    /// User email
    /// </summary>
    public string? Email { get; set; }

    /// <summary>
    /// User's first name
    /// </summary>
    public string? FirstName { get; set; }

    /// <summary>
    /// User's last name
    /// </summary>
    public string? LastName { get; set; }

    /// <summary>
    /// User's roles
    /// </summary>
    public List<string> Roles { get; set; } = new();

    /// <summary>
    /// Token expiration time (Unix timestamp)
    /// </summary>
    public long ExpiresAt { get; set; }
}
