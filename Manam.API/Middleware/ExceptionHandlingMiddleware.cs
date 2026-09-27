using System.Net;
using Manam.Models;
using Microsoft.AspNetCore.Http;
using Serilog;

namespace Manam.API.Middleware;

/// <summary>
/// Middleware for global exception handling
/// </summary>
public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;

    public ExceptionHandlingMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception exception)
        {
            await HandleExceptionAsync(context, exception);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var correlationId = context.Items["CorrelationId"]?.ToString() ?? "N/A";
        var traceId = context.Items["TraceId"]?.ToString() ?? context.TraceIdentifier;

        Log.Error(
            exception,
            "Handled exception: {ExceptionType} | Message: {Message} | CorrelationId: {CorrelationId} | TraceId: {TraceId}",
            exception.GetType().Name,
            exception.Message,
            correlationId,
            traceId);

        var response = new ErrorResponse
        {
            Message = GetUserFriendlyMessage(exception),
            TraceId = traceId
        };

        context.Response.ContentType = "application/json";

        switch (exception)
        {
            case ArgumentNullException:
            case ArgumentException:
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                response.Message = "Invalid input parameters.";
                break;

            case InvalidOperationException:
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                break;

            case UnauthorizedAccessException:
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                response.Message = "Unauthorized access.";
                break;

            case KeyNotFoundException:
                context.Response.StatusCode = StatusCodes.Status404NotFound;
                response.Message = "Resource not found.";
                break;

            default:
                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                response.Message = "An internal server error occurred. Please contact support if the problem persists.";
                break;
        }

        return context.Response.WriteAsJsonAsync(response);
    }

    private static string GetUserFriendlyMessage(Exception exception)
    {
        return exception switch
        {
            ArgumentNullException => "A required parameter was not provided.",
            ArgumentException => "An invalid argument was provided.",
            InvalidOperationException => exception.Message,
            UnauthorizedAccessException => "You do not have permission to access this resource.",
            TimeoutException => "The request timed out. Please try again.",
            _ => "An unexpected error occurred. Please try again later."
        };
    }
}

/// <summary>
/// Extension method for adding exception handling middleware
/// </summary>
public static class ExceptionHandlingMiddlewareExtensions
{
    public static IApplicationBuilder UseExceptionHandling(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<ExceptionHandlingMiddleware>();
    }
}
