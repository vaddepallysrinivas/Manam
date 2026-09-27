using System.Diagnostics;
using Microsoft.AspNetCore.Http;
using Serilog;
using Serilog.Context;

namespace Manam.API.Middleware;

/// <summary>
/// Middleware for request/response logging and correlation
/// </summary>
public class RequestCorrelationMiddleware
{
    private readonly RequestDelegate _next;
    private const string CorrelationIdHeader = "X-Correlation-ID";
    private const string TraceIdHeader = "X-Trace-ID";

    public RequestCorrelationMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId = context.Request.Headers.TryGetValue(CorrelationIdHeader, out var value)
            ? value.ToString()
            : Guid.NewGuid().ToString();

        var traceId = Activity.Current?.Id ?? context.TraceIdentifier;

        context.Items["CorrelationId"] = correlationId;
        context.Items["TraceId"] = traceId;

        context.Response.Headers.Append(CorrelationIdHeader, correlationId);
        context.Response.Headers.Append(TraceIdHeader, traceId);

        // Add to Serilog context for logging
        using (LogContext.PushProperty("CorrelationId", correlationId))
        using (LogContext.PushProperty("TraceId", traceId))
        {
            await _next(context);
        }
    }
}

/// <summary>
/// Extension method for adding request correlation middleware
/// </summary>
public static class RequestCorrelationMiddlewareExtensions
{
    public static IApplicationBuilder UseRequestCorrelation(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<RequestCorrelationMiddleware>();
    }
}
