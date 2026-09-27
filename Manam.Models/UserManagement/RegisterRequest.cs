namespace Manam.Models.UserManagement;

/// <summary>
/// Request model for user registration
/// </summary>
public class RegisterRequest
{
    /// <summary>
    /// Username for login
    /// </summary>
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// User's email address
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// User's password (min 8 chars, uppercase, lowercase, digit, special char)
    /// </summary>
    public string Password { get; set; } = string.Empty;

    /// <summary>
    /// Password confirmation
    /// </summary>
    public string ConfirmPassword { get; set; } = string.Empty;

    /// <summary>
    /// User's first name
    /// </summary>
    public string? FirstName { get; set; }

    /// <summary>
    /// User's last name
    /// </summary>
    public string? LastName { get; set; }

    /// <summary>
    /// User's phone number
    /// </summary>
    public string? PhoneNumber { get; set; }

    /// <summary>
    /// Client IP address for audit logging
    /// </summary>
    public string? IpAddress { get; set; }

    /// <summary>
    /// Browser user agent for audit logging
    /// </summary>
    public string? UserAgent { get; set; }
}
