namespace Manam.Models;

/// <summary>
/// Standardized API response wrapper
/// </summary>
public class ApiResponse<T>
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public T? Data { get; set; }
    public Dictionary<string, string[]>? Errors { get; set; }
    public string? TraceId { get; set; }
}

/// <summary>
/// Standardized error response
/// </summary>
public class ErrorResponse
{
    public string Message { get; set; } = string.Empty;
    public string? TraceId { get; set; }
    public Dictionary<string, string[]>? Errors { get; set; }
}
