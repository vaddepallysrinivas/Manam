using Manam.Models;
using Manam.Models.UserManagement;
using Manam.Services.Abstractions.UserManagement;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Manam.API.Controllers.UserManagement;

/// <summary>
/// Controller for user authentication operations (login, register, password reset)
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
[Produces("application/json")]
public class AuthenticationController : ControllerBase
{
    private readonly IAuthenticationService _authService;
    private readonly ILogger<AuthenticationController> _logger;

    /// <summary>
    /// Constructor with dependency injection
    /// </summary>
    public AuthenticationController(
        IAuthenticationService authService,
        ILogger<AuthenticationController> logger)
    {
        _authService = authService ?? throw new ArgumentNullException(nameof(authService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Login with email and password
    /// </summary>
    /// <param name="request">Login credentials</param>
    /// <returns>Login response with JWT token</returns>
    /// <response code="200">Login successful with JWT token</response>
    /// <response code="400">Invalid credentials or validation error</response>
    /// <response code="500">Internal server error</response>
    [HttpPost("login")]
    [ProducesResponseType(typeof(ApiResponse<LoginResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<LoginResponse>>> Login([FromBody] LoginRequest request)
    {
        try
        {
            _logger.LogInformation("Login endpoint called for email: {Email}", request?.Email);

            if (request == null)
            {
                _logger.LogWarning("Login request is null");
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Login request cannot be null",
                    Errors = new[] { "Invalid request body" }
                });
            }

            // Set IP address and User agent from request
            request.IpAddress ??= GetClientIpAddress();
            request.UserAgent ??= GetUserAgent();

            var result = await _authService.LoginAsync(request);

            if (!result.Success)
            {
                _logger.LogWarning("Login failed for email: {Email}", request.Email);
                return BadRequest(new ApiResponse<LoginResponse>
                {
                    Success = false,
                    Message = result.Message,
                    Data = result
                });
            }

            _logger.LogInformation("Login successful for email: {Email}", request.Email);
            return Ok(new ApiResponse<LoginResponse>
            {
                Success = true,
                Message = result.Message,
                Data = result
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error in Login endpoint");
            return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse<object>
            {
                Success = false,
                Message = "An unexpected error occurred during login",
                Errors = new[] { ex.Message }
            });
        }
    }

    /// <summary>
    /// Register new user account
    /// </summary>
    /// <param name="request">Registration details</param>
    /// <returns>Registration response with new user ID</returns>
    /// <response code="201">User registered successfully</response>
    /// <response code="400">Validation error or user already exists</response>
    /// <response code="500">Internal server error</response>
    [HttpPost("register")]
    [ProducesResponseType(typeof(ApiResponse<RegisterResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<RegisterResponse>>> Register([FromBody] RegisterRequest request)
    {
        try
        {
            _logger.LogInformation("Register endpoint called for email: {Email}", request?.Email);

            if (request == null)
            {
                _logger.LogWarning("Register request is null");
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Register request cannot be null",
                    Errors = new[] { "Invalid request body" }
                });
            }

            // Set IP address and User agent from request
            request.IpAddress ??= GetClientIpAddress();
            request.UserAgent ??= GetUserAgent();

            var result = await _authService.RegisterAsync(request);

            if (!result.Success)
            {
                _logger.LogWarning("Registration failed for email: {Email}", request.Email);
                return BadRequest(new ApiResponse<RegisterResponse>
                {
                    Success = false,
                    Message = result.Message,
                    Data = result
                });
            }

            _logger.LogInformation("User registered successfully with ID: {UserId}", result.UserId);
            return CreatedAtAction(nameof(Register), new ApiResponse<RegisterResponse>
            {
                Success = true,
                Message = result.Message,
                Data = result
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error in Register endpoint");
            return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse<object>
            {
                Success = false,
                Message = "An unexpected error occurred during registration",
                Errors = new[] { ex.Message }
            });
        }
    }

    /// <summary>
    /// Request password reset via email
    /// </summary>
    /// <param name="email">User's email address</param>
    /// <returns>Password reset request response</returns>
    /// <response code="200">Password reset email sent</response>
    /// <response code="400">Invalid email</response>
    /// <response code="500">Internal server error</response>
    [HttpPost("forgot-password")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<object>>> ForgotPassword([FromQuery] string email)
    {
        try
        {
            _logger.LogInformation("ForgotPassword endpoint called for email: {Email}", email);

            if (string.IsNullOrWhiteSpace(email))
            {
                _logger.LogWarning("Empty email provided for password reset");
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Email is required",
                    Errors = new[] { "Email cannot be empty" }
                });
            }

            var token = await _authService.RequestPasswordResetAsync(email);

            if (token == null)
            {
                _logger.LogWarning("Password reset failed for email: {Email}", email);
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Password reset request failed"
                });
            }

            _logger.LogInformation("Password reset token generated for email: {Email}", email);
            return Ok(new ApiResponse<object>
            {
                Success = true,
                Message = "Password reset email sent. Please check your inbox."
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error in ForgotPassword endpoint");
            return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse<object>
            {
                Success = false,
                Message = "An unexpected error occurred",
                Errors = new[] { ex.Message }
            });
        }
    }

    /// <summary>
    /// Reset password with reset token
    /// </summary>
    /// <param name="request">Reset password request with token and new password</param>
    /// <returns>Password reset response</returns>
    /// <response code="200">Password reset successful</response>
    /// <response code="400">Invalid token or password</response>
    /// <response code="500">Internal server error</response>
    [HttpPost("reset-password")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<object>>> ResetPassword([FromBody] ResetPasswordRequest request)
    {
        try
        {
            _logger.LogInformation("ResetPassword endpoint called for user ID: {UserId}", request?.UserId);

            if (request == null || request.UserId <= 0)
            {
                _logger.LogWarning("Invalid reset password request");
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Invalid request"
                });
            }

            var result = await _authService.ResetPasswordAsync(request.UserId, request.Token, request.NewPassword);

            if (!result)
            {
                _logger.LogWarning("Password reset failed for user ID: {UserId}", request.UserId);
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Password reset failed. Token may be invalid or expired."
                });
            }

            _logger.LogInformation("Password reset successful for user ID: {UserId}", request.UserId);
            return Ok(new ApiResponse<object>
            {
                Success = true,
                Message = "Password reset successful. You can now login with your new password."
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error in ResetPassword endpoint");
            return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse<object>
            {
                Success = false,
                Message = "An unexpected error occurred",
                Errors = new[] { ex.Message }
            });
        }
    }

    /// <summary>
    /// Verify user email
    /// </summary>
    /// <param name="userId">User ID to verify email for</param>
    /// <returns>Email verification response</returns>
    /// <response code="200">Email verified successfully</response>
    /// <response code="400">Invalid user ID</response>
    /// <response code="500">Internal server error</response>
    [HttpPost("verify-email/{userId}")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<object>>> VerifyEmail(int userId)
    {
        try
        {
            _logger.LogInformation("VerifyEmail endpoint called for user ID: {UserId}", userId);

            if (userId <= 0)
            {
                _logger.LogWarning("Invalid user ID for email verification: {UserId}", userId);
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Invalid user ID"
                });
            }

            var result = await _authService.VerifyEmailAsync(userId);

            if (!result)
            {
                _logger.LogWarning("Email verification failed for user ID: {UserId}", userId);
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Email verification failed"
                });
            }

            _logger.LogInformation("Email verified successfully for user ID: {UserId}", userId);
            return Ok(new ApiResponse<object>
            {
                Success = true,
                Message = "Email verified successfully"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error in VerifyEmail endpoint");
            return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse<object>
            {
                Success = false,
                Message = "An unexpected error occurred",
                Errors = new[] { ex.Message }
            });
        }
    }

    /// <summary>
    /// Get client IP address from request
    /// </summary>
    private string GetClientIpAddress()
    {
        if (HttpContext.Request.Headers.TryGetValue("X-Forwarded-For", out var forwardedFor))
            return forwardedFor.ToString().Split(',')[0].Trim();

        return HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
    }

    /// <summary>
    /// Get user agent from request
    /// </summary>
    private string GetUserAgent()
    {
        return HttpContext.Request.Headers["User-Agent"].ToString() ?? "Unknown";
    }
}

/// <summary>
/// Request model for password reset
/// </summary>
public class ResetPasswordRequest
{
    /// <summary>
    /// User ID
    /// </summary>
    public int UserId { get; set; }

    /// <summary>
    /// Password reset token
    /// </summary>
    public string Token { get; set; } = string.Empty;

    /// <summary>
    /// New password
    /// </summary>
    public string NewPassword { get; set; } = string.Empty;

    /// <summary>
    /// Confirm new password
    /// </summary>
    public string ConfirmPassword { get; set; } = string.Empty;
}
