namespace Manam.Models.UserManagement;

/// <summary>
/// Response model for user profile
/// </summary>
public class UserProfileResponse
{
    /// <summary>
    /// User ID
    /// </summary>
    public int UserId { get; set; }

    /// <summary>
    /// Username
    /// </summary>
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// User's email
    /// </summary>
    public string Email { get; set; } = string.Empty;

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
    /// Profile picture URL
    /// </summary>
    public string? ProfilePictureUrl { get; set; }

    /// <summary>
    /// Is email verified
    /// </summary>
    public bool IsEmailVerified { get; set; }

    /// <summary>
    /// Is phone verified
    /// </summary>
    public bool IsPhoneVerified { get; set; }

    /// <summary>
    /// Is user active
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// Has password authentication enabled
    /// </summary>
    public bool HasPassword { get; set; }

    /// <summary>
    /// User's assigned roles
    /// </summary>
    public List<RoleInfo> Roles { get; set; } = new();

    /// <summary>
    /// External authentication providers linked to this user
    /// </summary>
    public List<ExternalAuthInfo> ExternalAuths { get; set; } = new();

    /// <summary>
    /// Account creation date
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Last update date
    /// </summary>
    public DateTime UpdatedAt { get; set; }
}

/// <summary>
/// Role information for user profile
/// </summary>
public class RoleInfo
{
    /// <summary>
    /// Role ID
    /// </summary>
    public int RoleId { get; set; }

    /// <summary>
    /// Role name
    /// </summary>
    public string RoleName { get; set; } = string.Empty;

    /// <summary>
    /// Role description
    /// </summary>
    public string? Description { get; set; }
}

/// <summary>
/// External authentication provider info
/// </summary>
public class ExternalAuthInfo
{
    /// <summary>
    /// Provider name (Google, Facebook, GitHub)
    /// </summary>
    public string ProviderName { get; set; } = string.Empty;

    /// <summary>
    /// External email
    /// </summary>
    public string? ExternalEmail { get; set; }

    /// <summary>
    /// Last authenticated timestamp
    /// </summary>
    public DateTime? LastAuthenticatedAt { get; set; }
}
