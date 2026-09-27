namespace Manam.Models.UserManagement;

/// <summary>
/// Request model for user login with email and password
/// </summary>
public class LoginRequest
{
    /// <summary>
    /// User's email address
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// User's password (will be hashed on server)
    /// </summary>
    public string Password { get; set; } = string.Empty;

    /// <summary>
    /// Client IP address for audit logging
    /// </summary>
    public string? IpAddress { get; set; }

    /// <summary>
    /// Browser user agent for audit logging
    /// </summary>
    public string? UserAgent { get; set; }
}
