using System.Diagnostics;
using Microsoft.AspNetCore.Http;
using Serilog;

namespace Manam.API.Middleware;

/// <summary>
/// Middleware for comprehensive request/response logging
/// </summary>
public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;

    public RequestLoggingMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var stopwatch = Stopwatch.StartNew();
        var originalBodyStream = context.Response.Body;

        try
        {
            var correlationId = context.Items["CorrelationId"]?.ToString() ?? "N/A";
            var traceId = context.Items["TraceId"]?.ToString() ?? "N/A";

            // Log incoming request
            Log.Information(
                "Incoming HTTP Request: {Method} {Path} | CorrelationId: {CorrelationId} | TraceId: {TraceId}",
                context.Request.Method,
                context.Request.Path,
                correlationId,
                traceId);

            await _next(context);

            stopwatch.Stop();

            // Log outgoing response
            Log.Information(
                "Outgoing HTTP Response: {Method} {Path} | StatusCode: {StatusCode} | Duration: {DurationMs}ms | CorrelationId: {CorrelationId}",
                context.Request.Method,
                context.Request.Path,
                context.Response.StatusCode,
                stopwatch.ElapsedMilliseconds,
                correlationId);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            var correlationId = context.Items["CorrelationId"]?.ToString() ?? "N/A";

            Log.Error(
                ex,
                "Unhandled exception in request pipeline | Path: {Path} | Duration: {DurationMs}ms | CorrelationId: {CorrelationId}",
                context.Request.Path,
                stopwatch.ElapsedMilliseconds,
                correlationId);

            throw;
        }
    }
}

/// <summary>
/// Extension method for adding request logging middleware
/// </summary>
public static class RequestLoggingMiddlewareExtensions
{
    public static IApplicationBuilder UseRequestLogging(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<RequestLoggingMiddleware>();
    }
}
