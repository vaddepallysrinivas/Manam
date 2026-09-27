using Manam.Models;
using Manam.Models.UserManagement;
using Manam.Services.Abstractions.UserManagement;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Security.Claims;

namespace Manam.API.Controllers.UserManagement;

/// <summary>
/// Controller for user profile management operations (view, edit, delete)
/// Requires authentication for most endpoints
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
[Produces("application/json")]
[Authorize]
public class UserProfileController : ControllerBase
{
    private readonly IUserProfileService _userProfileService;
    private readonly ILogger<UserProfileController> _logger;

    /// <summary>
    /// Constructor with dependency injection
    /// </summary>
    public UserProfileController(
        IUserProfileService userProfileService,
        ILogger<UserProfileController> logger)
    {
        _userProfileService = userProfileService ?? throw new ArgumentNullException(nameof(userProfileService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Get current logged-in user's profile
    /// </summary>
    /// <returns>User profile with roles and external auth providers</returns>
    /// <response code="200">User profile retrieved successfully</response>
    /// <response code="401">Unauthorized - user not authenticated</response>
    /// <response code="404">User not found</response>
    /// <response code="500">Internal server error</response>
    [HttpGet("me")]
    [ProducesResponseType(typeof(ApiResponse<UserProfileResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<UserProfileResponse>>> GetProfile()
    {
        try
        {
            var userId = GetUserIdFromClaims();
            if (userId <= 0)
            {
                _logger.LogWarning("Unable to extract user ID from claims");
                return Unauthorized(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Unauthorized"
                });
            }

            _logger.LogInformation("Fetching profile for user ID: {UserId}", userId);

            var profile = await _userProfileService.GetProfileAsync(userId);

            if (profile == null)
            {
                _logger.LogWarning("User profile not found for ID: {UserId}", userId);
                return NotFound(new ApiResponse<object>
                {
                    Success = false,
                    Message = "User profile not found"
                });
            }

            _logger.LogInformation("Profile retrieved successfully for user ID: {UserId}", userId);
            return Ok(new ApiResponse<UserProfileResponse>
            {
                Success = true,
                Message = "Profile retrieved successfully",
                Data = profile
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error in GetProfile endpoint");
            return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse<object>
            {
                Success = false,
                Message = "An unexpected error occurred",
                Errors = new[] { ex.Message }
            });
        }
    }

    /// <summary>
    /// Get user profile by ID (requires admin role)
    /// </summary>
    /// <param name="userId">User ID to fetch</param>
    /// <returns>User profile with roles and external auth providers</returns>
    /// <response code="200">User profile retrieved successfully</response>
    /// <response code="401">Unauthorized - user not authenticated</response>
    /// <response code="403">Forbidden - user doesn't have admin role</response>
    /// <response code="404">User not found</response>
    /// <response code="500">Internal server error</response>
    [HttpGet("{userId}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse<UserProfileResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<UserProfileResponse>>> GetUserById(int userId)
    {
        try
        {
            _logger.LogInformation("Fetching profile for user ID: {UserId}", userId);

            if (userId <= 0)
            {
                _logger.LogWarning("Invalid user ID: {UserId}", userId);
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Invalid user ID"
                });
            }

            var profile = await _userProfileService.GetProfileAsync(userId);

            if (profile == null)
            {
                _logger.LogWarning("User profile not found for ID: {UserId}", userId);
                return NotFound(new ApiResponse<object>
                {
                    Success = false,
                    Message = "User profile not found"
                });
            }

            _logger.LogInformation("Profile retrieved successfully for user ID: {UserId}", userId);
            return Ok(new ApiResponse<UserProfileResponse>
            {
                Success = true,
                Message = "Profile retrieved successfully",
                Data = profile
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error in GetUserById endpoint");
            return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse<object>
            {
                Success = false,
                Message = "An unexpected error occurred",
                Errors = new[] { ex.Message }
            });
        }
    }

    /// <summary>
    /// Get all users with pagination (requires admin role)
    /// </summary>
    /// <param name="pageNumber">Page number (1-based)</param>
    /// <param name="pageSize">Items per page (max 100)</param>
    /// <returns>Paginated list of users</returns>
    /// <response code="200">Users retrieved successfully</response>
    /// <response code="401">Unauthorized - user not authenticated</response>
    /// <response code="403">Forbidden - user doesn't have admin role</response>
    /// <response code="500">Internal server error</response>
    [HttpGet]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<object>>> GetAllUsers([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
    {
        try
        {
            _logger.LogInformation("Fetching all users. Page: {PageNumber}, Size: {PageSize}", pageNumber, pageSize);

            if (pageNumber <= 0 || pageSize <= 0 || pageSize > 100)
            {
                _logger.LogWarning("Invalid pagination parameters. Page: {PageNumber}, Size: {PageSize}", pageNumber, pageSize);
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Invalid pagination parameters"
                });
            }

            var (users, totalCount) = await _userProfileService.GetAllUsersAsync(pageNumber, pageSize);

            _logger.LogInformation("Retrieved {UserCount} users, total count: {TotalCount}", users.Count, totalCount);
            return Ok(new ApiResponse<object>
            {
                Success = true,
                Message = "Users retrieved successfully",
                Data = new { Users = users, TotalCount = totalCount, PageNumber = pageNumber, PageSize = pageSize }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error in GetAllUsers endpoint");
            return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse<object>
            {
                Success = false,
                Message = "An unexpected error occurred",
                Errors = new[] { ex.Message }
            });
        }
    }

    /// <summary>
    /// Update current user's profile
    /// </summary>
    /// <param name="request">Profile update request</param>
    /// <returns>Update result</returns>
    /// <response code="200">Profile updated successfully</response>
    /// <response code="400">Invalid request data</response>
    /// <response code="401">Unauthorized - user not authenticated</response>
    /// <response code="500">Internal server error</response>
    [HttpPut("me")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<object>>> UpdateProfile([FromBody] UpdateProfileRequest request)
    {
        try
        {
            var userId = GetUserIdFromClaims();
            if (userId <= 0)
            {
                _logger.LogWarning("Unable to extract user ID from claims");
                return Unauthorized(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Unauthorized"
                });
            }

            _logger.LogInformation("Updating profile for user ID: {UserId}", userId);

            if (request == null)
            {
                _logger.LogWarning("Update profile request is null");
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Update request cannot be null"
                });
            }

            var result = await _userProfileService.UpdateProfileAsync(userId, request.FirstName, request.LastName, request.PhoneNumber);

            if (!result)
            {
                _logger.LogWarning("Profile update failed for user ID: {UserId}", userId);
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Profile update failed"
                });
            }

            _logger.LogInformation("Profile updated successfully for user ID: {UserId}", userId);
            return Ok(new ApiResponse<object>
            {
                Success = true,
                Message = "Profile updated successfully"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error in UpdateProfile endpoint");
            return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse<object>
            {
                Success = false,
                Message = "An unexpected error occurred",
                Errors = new[] { ex.Message }
            });
        }
    }

    /// <summary>
    /// Delete current user's account (soft delete)
    /// </summary>
    /// <returns>Deletion result</returns>
    /// <response code="200">Account deleted successfully</response>
    /// <response code="401">Unauthorized - user not authenticated</response>
    /// <response code="500">Internal server error</response>
    [HttpDelete("me")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<object>>> DeleteAccount()
    {
        try
        {
            var userId = GetUserIdFromClaims();
            if (userId <= 0)
            {
                _logger.LogWarning("Unable to extract user ID from claims");
                return Unauthorized(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Unauthorized"
                });
            }

            _logger.LogInformation("Deleting account for user ID: {UserId}", userId);

            var result = await _userProfileService.DeleteUserAsync(userId);

            if (!result)
            {
                _logger.LogWarning("Account deletion failed for user ID: {UserId}", userId);
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Account deletion failed"
                });
            }

            _logger.LogInformation("Account deleted successfully for user ID: {UserId}", userId);
            return Ok(new ApiResponse<object>
            {
                Success = true,
                Message = "Account deleted successfully. Your data has been securely removed."
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error in DeleteAccount endpoint");
            return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse<object>
            {
                Success = false,
                Message = "An unexpected error occurred",
                Errors = new[] { ex.Message }
            });
        }
    }

    /// <summary>
    /// Link external authentication provider to user account
    /// </summary>
    /// <param name="request">External provider link request</param>
    /// <returns>Link result</returns>
    /// <response code="200">External provider linked successfully</response>
    /// <response code="400">Invalid request data</response>
    /// <response code="401">Unauthorized - user not authenticated</response>
    /// <response code="500">Internal server error</response>
    [HttpPost("link-provider")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<object>>> LinkExternalProvider([FromBody] LinkProviderRequest request)
    {
        try
        {
            var userId = GetUserIdFromClaims();
            if (userId <= 0)
            {
                _logger.LogWarning("Unable to extract user ID from claims");
                return Unauthorized(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Unauthorized"
                });
            }

            _logger.LogInformation("Linking provider {ProviderId} for user ID: {UserId}", request?.ProviderId, userId);

            if (request == null)
            {
                _logger.LogWarning("Link provider request is null");
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Request cannot be null"
                });
            }

            var result = await _userProfileService.LinkExternalProviderAsync(
                userId, request.ProviderId, request.ExternalUserId, request.ExternalEmail, request.AccessToken);

            if (!result)
            {
                _logger.LogWarning("Provider link failed for user ID: {UserId}", userId);
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Failed to link external provider"
                });
            }

            _logger.LogInformation("Provider linked successfully for user ID: {UserId}", userId);
            return Ok(new ApiResponse<object>
            {
                Success = true,
                Message = "External provider linked successfully"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error in LinkExternalProvider endpoint");
            return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse<object>
            {
                Success = false,
                Message = "An unexpected error occurred",
                Errors = new[] { ex.Message }
            });
        }
    }

    /// <summary>
    /// Assign role to user (requires admin role)
    /// </summary>
    /// <param name="userId">User ID to assign role to</param>
    /// <param name="request">Role assignment request</param>
    /// <returns>Assignment result</returns>
    /// <response code="200">Role assigned successfully</response>
    /// <response code="400">Invalid request data</response>
    /// <response code="401">Unauthorized - user not authenticated</response>
    /// <response code="403">Forbidden - user doesn't have admin role</response>
    /// <response code="500">Internal server error</response>
    [HttpPost("{userId}/roles")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<object>>> AssignRole(int userId, [FromBody] AssignRoleRequest request)
    {
        try
        {
            var adminUserId = GetUserIdFromClaims();
            _logger.LogInformation("Admin {AdminUserId} assigning role {RoleId} to user {UserId}", adminUserId, request?.RoleId, userId);

            if (request == null || request.RoleId <= 0)
            {
                _logger.LogWarning("Invalid role assignment request");
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Invalid role ID"
                });
            }

            var result = await _userProfileService.AssignRoleAsync(userId, request.RoleId, adminUserId);

            if (!result)
            {
                _logger.LogWarning("Role assignment failed for user ID: {UserId}", userId);
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Failed to assign role"
                });
            }

            _logger.LogInformation("Role assigned successfully to user ID: {UserId}", userId);
            return Ok(new ApiResponse<object>
            {
                Success = true,
                Message = "Role assigned successfully"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error in AssignRole endpoint");
            return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse<object>
            {
                Success = false,
                Message = "An unexpected error occurred",
                Errors = new[] { ex.Message }
            });
        }
    }

    /// <summary>
    /// Extract user ID from JWT claims
    /// </summary>
    private int GetUserIdFromClaims()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value 
            ?? User.FindFirst("sub")?.Value;

        if (int.TryParse(userIdClaim, out var userId))
            return userId;

        return 0;
    }
}

/// <summary>
/// Request model for updating user profile
/// </summary>
public class UpdateProfileRequest
{
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
}

/// <summary>
/// Request model for linking external provider
/// </summary>
public class LinkProviderRequest
{
    /// <summary>
    /// External provider ID (1=Google, 2=Facebook, 3=GitHub)
    /// </summary>
    public int ProviderId { get; set; }

    /// <summary>
    /// External user ID from provider
    /// </summary>
    public string ExternalUserId { get; set; } = string.Empty;

    /// <summary>
    /// External email from provider
    /// </summary>
    public string? ExternalEmail { get; set; }

    /// <summary>
    /// OAuth access token
    /// </summary>
    public string? AccessToken { get; set; }
}

/// <summary>
/// Request model for assigning role to user
/// </summary>
public class AssignRoleRequest
{
    /// <summary>
    /// Role ID to assign
    /// </summary>
    public int RoleId { get; set; }
}
