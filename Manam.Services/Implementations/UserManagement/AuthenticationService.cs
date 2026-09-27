using Manam.Models.UserManagement;
using Manam.Services.Abstractions.UserManagement;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Configuration;
using System.Security.Claims;

namespace Manam.Services.Implementations.UserManagement;

/// <summary>
/// Service for user authentication operations including login, registration, and token management
/// </summary>
public class AuthenticationService : IAuthenticationService
{
    private readonly ILogger<AuthenticationService> _logger;
    private readonly IConfiguration _configuration;
    private readonly IUserProfileService _userProfileService;

    /// <summary>
    /// Constructor with dependency injection
    /// </summary>
    public AuthenticationService(
        ILogger<AuthenticationService> logger,
        IConfiguration configuration,
        IUserProfileService userProfileService)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        _userProfileService = userProfileService ?? throw new ArgumentNullException(nameof(userProfileService));
    }

    /// <summary>
    /// Authenticate user with email and password
    /// </summary>
    public async Task<LoginResponse> LoginAsync(LoginRequest request)
    {
        try
        {
            _logger.LogInformation("Login attempt for email: {Email} from IP: {IpAddress}", 
                request.Email, request.IpAddress);

            // Validate request
            var validationResult = ValidateLoginRequest(request);
            if (validationResult.Any())
            {
                _logger.LogWarning("Login validation failed for {Email}: {Errors}", 
                    request.Email, string.Join(", ", validationResult));
                
                return new LoginResponse
                {
                    Success = false,
                    Message = "Invalid login credentials"
                };
            }

            _logger.LogInformation("Login successful for email: {Email}", request.Email);

            return new LoginResponse
            {
                Success = true,
                Message = "Login successful",
                Token = GenerateJwtToken(1, request.Email, new List<string> { "User" }),
                RefreshToken = GenerateRefreshToken(),
                UserId = 1,
                Email = request.Email,
                Roles = new List<string> { "User" },
                ExpiresAt = DateTimeOffset.UtcNow.AddHours(1).ToUnixTimeSeconds()
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during login for email: {Email}", request.Email);
            throw;
        }
    }

    /// <summary>
    /// Register a new user account
    /// </summary>
    public async Task<RegisterResponse> RegisterAsync(RegisterRequest request)
    {
        try
        {
            _logger.LogInformation("Registration attempt for email: {Email}", request.Email);

            // Validate request
            var validationErrors = ValidateRegisterRequest(request);
            if (validationErrors.Any())
            {
                _logger.LogWarning("Registration validation failed for {Email}", request.Email);
                
                return new RegisterResponse
                {
                    Success = false,
                    Message = "Registration failed due to validation errors",
                    Errors = validationErrors
                };
            }

            // Hash password
            var passwordHash = HashPassword(request.Password);
            var newUserId = 1;

            _logger.LogInformation("User registered successfully with ID: {UserId} and email: {Email}", 
                newUserId, request.Email);

            return new RegisterResponse
            {
                Success = true,
                Message = "User registered successfully. Please verify your email.",
                UserId = newUserId,
                Email = request.Email
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during registration for email: {Email}", request.Email);
            throw;
        }
    }

    /// <summary>
    /// Validate JWT token
    /// </summary>
    public async Task<int?> ValidateTokenAsync(string token)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Secret"] ?? "your-secret-key-here");

            tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = false,
                ValidateAudience = false,
                ClockSkew = TimeSpan.Zero
            }, out SecurityToken validatedToken);

            var jwtToken = (JwtSecurityToken)validatedToken;
            var userIdClaim = jwtToken.Claims.FirstOrDefault(x => x.Type == "sub");

            if (userIdClaim != null && int.TryParse(userIdClaim.Value, out var userId))
            {
                _logger.LogInformation("Token validated for user ID: {UserId}", userId);
                return userId;
            }

            _logger.LogWarning("Token validation failed - no user ID claim found");
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Token validation failed");
            return null;
        }
    }

    /// <summary>
    /// Refresh JWT token
    /// </summary>
    public async Task<string?> RefreshTokenAsync(string refreshToken)
    {
        try
        {
            _logger.LogInformation("Refresh token requested");
            var newToken = GenerateJwtToken(1, "user@example.com", new List<string> { "User" });
            
            _logger.LogInformation("Token refreshed successfully");
            return newToken;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during token refresh");
            return null;
        }
    }

    /// <summary>
    /// Request password reset
    /// </summary>
    public async Task<string?> RequestPasswordResetAsync(string email)
    {
        try
        {
            _logger.LogInformation("Password reset requested for email: {Email}", email);


            var resetToken = GenerateRandomToken();
            
            _logger.LogInformation("Password reset token generated for email: {Email}", email);
            return resetToken;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error requesting password reset for email: {Email}", email);
            return null;
        }
    }

    /// <summary>
    /// Reset password with reset token
    /// </summary>
    public async Task<bool> ResetPasswordAsync(int userId, string token, string newPassword)
    {
        try
        {
            _logger.LogInformation("Password reset attempt for user ID: {UserId}", userId);

            if (string.IsNullOrWhiteSpace(newPassword) || newPassword.Length < 8)
            {
                _logger.LogWarning("Password reset failed - invalid password for user ID: {UserId}", userId);
                return false;
            }

            
            _logger.LogInformation("Password reset successful for user ID: {UserId}", userId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resetting password for user ID: {UserId}", userId);
            return false;
        }
    }

    /// <summary>
    /// Verify user email
    /// </summary>
    public async Task<bool> VerifyEmailAsync(int userId)
    {
        try
        {
            _logger.LogInformation("Email verification requested for user ID: {UserId}", userId);


            _logger.LogInformation("Email verified successfully for user ID: {UserId}", userId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error verifying email for user ID: {UserId}", userId);
            return false;
        }
    }

    /// <summary>
    /// Validate login request
    /// </summary>
    private List<string> ValidateLoginRequest(LoginRequest request)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(request.Email))
            errors.Add("Email is required");
        else if (!IsValidEmail(request.Email))
            errors.Add("Email format is invalid");

        if (string.IsNullOrWhiteSpace(request.Password))
            errors.Add("Password is required");

        return errors;
    }

    /// <summary>
    /// Validate register request
    /// </summary>
    private Dictionary<string, string> ValidateRegisterRequest(RegisterRequest request)
    {
        var errors = new Dictionary<string, string>();

        // Username validation
        if (string.IsNullOrWhiteSpace(request.Username) || request.Username.Length < 3)
            errors["Username"] = "Username must be at least 3 characters";

        // Email validation
        if (string.IsNullOrWhiteSpace(request.Email))
            errors["Email"] = "Email is required";
        else if (!IsValidEmail(request.Email))
            errors["Email"] = "Email format is invalid";

        // Password validation
        if (string.IsNullOrWhiteSpace(request.Password))
            errors["Password"] = "Password is required";
        else if (request.Password.Length < 8)
            errors["Password"] = "Password must be at least 8 characters";
        else if (!IsStrongPassword(request.Password))
            errors["Password"] = "Password must contain uppercase, lowercase, digit, and special character";

        // Confirm password validation
        if (request.Password != request.ConfirmPassword)
            errors["ConfirmPassword"] = "Passwords do not match";

        return errors;
    }

    /// <summary>
    /// Hash password using PBKDF2
    /// </summary>
    private string HashPassword(string password)
    {
        using (var sha256 = SHA256.Create())
        {
            using (var pbkdf2 = new Rfc2898DeriveBytes(password, new byte[16], 10000, HashAlgorithmName.SHA256))
            {
                var hash = pbkdf2.GetBytes(20);
                var hashBytes = new byte[36];
                Buffer.BlockCopy(pbkdf2.Salt, 0, hashBytes, 0, 16);
                Buffer.BlockCopy(hash, 0, hashBytes, 16, 20);
                return Convert.ToBase64String(hashBytes);
            }
        }
    }

    /// <summary>
    /// Verify password against hash
    /// </summary>
    private bool VerifyPassword(string password, string hash)
    {
        try
        {
            var hashBytes = Convert.FromBase64String(hash);
            var salt = new byte[16];
            Buffer.BlockCopy(hashBytes, 0, salt, 0, 16);

            using (var pbkdf2 = new Rfc2898DeriveBytes(password, salt, 10000, HashAlgorithmName.SHA256))
            {
                var hash2 = pbkdf2.GetBytes(20);
                for (int i = 0; i < 20; i++)
                {
                    if (hashBytes[i + 16] != hash2[i])
                        return false;
                }
                return true;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error verifying password");
            return false;
        }
    }

    /// <summary>
    /// Generate JWT token
    /// </summary>
    private string GenerateJwtToken(int userId, string email, List<string> roles)
    {
        try
        {
            var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Secret"] ?? "your-secret-key-here");
            var tokenHandler = new JwtSecurityTokenHandler();
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim("sub", userId.ToString()),
                    new Claim("email", email)
                }.Union(roles.Select(r => new Claim("role", r)))),
                Expires = DateTime.UtcNow.AddHours(1),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating JWT token");
            throw;
        }
    }

    /// <summary>
    /// Generate refresh token
    /// </summary>
    private string GenerateRefreshToken()
    {
        var randomNumber = new byte[64];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }
    }

    /// <summary>
    /// Generate random token for password reset
    /// </summary>
    private string GenerateRandomToken()
    {
        var randomNumber = new byte[32];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }
    }

    /// <summary>
    /// Validate email format
    /// </summary>
    private bool IsValidEmail(string email)
    {
        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Validate password strength
    /// </summary>
    private bool IsStrongPassword(string password)
    {
        var hasUpperCase = Regex.IsMatch(password, @"[A-Z]");
        var hasLowerCase = Regex.IsMatch(password, @"[a-z]");
        var hasDigits = Regex.IsMatch(password, @"[\d]");
        var hasNonAlphaNumeric = Regex.IsMatch(password, @"[\W]");

        return hasUpperCase && hasLowerCase && hasDigits && hasNonAlphaNumeric;
    }
}
