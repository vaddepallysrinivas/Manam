namespace Manam.Auth.Abstractions;

/// <summary>
/// Authentication service interface for JWT token generation and validation
/// </summary>
public interface IAuthenticationService
{
    Task<string> GenerateTokenAsync(Guid userId, string username);
    bool ValidateToken(string token);
}
