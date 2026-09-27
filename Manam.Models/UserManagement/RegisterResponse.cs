namespace Manam.Models.UserManagement;

/// <summary>
/// Response model for user registration
/// </summary>
public class RegisterResponse
{
    /// <summary>
    /// Registration success flag
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Status message
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// New user ID
    /// </summary>
    public int? UserId { get; set; }

    /// <summary>
    /// User email
    /// </summary>
    public string? Email { get; set; }

    /// <summary>
    /// Errors if registration failed
    /// </summary>
    public Dictionary<string, string> Errors { get; set; } = new();
}
