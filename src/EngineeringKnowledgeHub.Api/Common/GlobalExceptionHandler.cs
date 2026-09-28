namespace EngineeringKnowledgeHub.Api.Common;

using EngineeringKnowledgeHub.Application.Common.Exceptions;
using EngineeringKnowledgeHub.Domain.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

/// <summary>
/// Centralized global exception handler providing standard RFC 7807 Problem Details.
/// Maps domain and application exceptions to appropriate HTTP status codes and ensures
/// internal exception details (stack traces, SQL, connection strings) are never leaked in production.
/// </summary>
public sealed class GlobalExceptionHandler(
    ILogger<GlobalExceptionHandler> logger,
    IHostEnvironment environment) : IExceptionHandler
{
    private static readonly string[] SensitiveKeywords =
    [
        "Server=", "Database=", "Password=", "User Id=", "Trusted_Connection=",
        "SELECT ", "INSERT ", "UPDATE ", "DELETE ", "DROP ", "ALTER ", "EXEC ",
        "SqlException", "DbUpdateException", "SocketException", "Stack trace:",
        "at System.", "at Microsoft.", "at EngineeringKnowledgeHub."
    ];

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        // Internal exception is captured in structured server-side logging with complete stack trace
        logger.LogError(exception, "Exception occurred during request execution: {Message}", exception.Message);

        var (statusCode, title, type) = exception switch
        {
            NotFoundException => (StatusCodes.Status404NotFound, "Not Found", "https://tools.ietf.org/html/rfc7231#section-6.5.4"),
            ValidationException => (StatusCodes.Status400BadRequest, "Bad Request", "https://tools.ietf.org/html/rfc7231#section-6.5.1"),
            ConflictException => (StatusCodes.Status409Conflict, "Conflict", "https://tools.ietf.org/html/rfc7231#section-6.5.8"),
            DomainRuleViolationException => (StatusCodes.Status409Conflict, "Conflict", "https://tools.ietf.org/html/rfc7231#section-6.5.8"),
            DomainException => (StatusCodes.Status400BadRequest, "Bad Request", "https://tools.ietf.org/html/rfc7231#section-6.5.1"),
            _ => (StatusCodes.Status500InternalServerError, "An unexpected error occurred.", "https://tools.ietf.org/html/rfc7231#section-6.6.1")
        };

        var detail = GetSafeDetail(exception, statusCode);

        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Type = type,
            Detail = detail,
            Instance = httpContext.Request.Path
        };

        httpContext.Response.StatusCode = statusCode;
        httpContext.Response.ContentType = "application/problem+json";

        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

        return true;
    }

    private string GetSafeDetail(Exception exception, int statusCode)
    {
        // Unexpected server errors:
        // In Development, expose the raw exception message for developer ergonomics.
        // In Production, always return a generic safe message.
        if (statusCode == StatusCodes.Status500InternalServerError)
        {
            return environment.IsDevelopment()
                ? exception.Message
                : "An unexpected server error occurred. Please try again later.";
        }

        // Known Domain & Application exceptions:
        // In Development, always expose the exception message.
        if (environment.IsDevelopment())
        {
            return exception.Message;
        }

        // In Production, domain & application exceptions are designed to carry client-safe business messages.
        // However, if an exception message contains multiline stack traces, SQL, connection strings, or internal type names,
        // it must be sanitized to protect internal infrastructure details.
        if (ContainsSensitiveDetails(exception.Message))
        {
            return exception switch
            {
                NotFoundException => "The requested resource was not found.",
                ValidationException => "The request failed validation.",
                ConflictException => "The request could not be completed due to a conflict with the current state of the resource.",
                DomainRuleViolationException => "A business rule violation occurred.",
                DomainException => "A business rule violation occurred.",
                _ => "An unexpected error occurred."
            };
        }

        return exception.Message;
    }

    private static bool ContainsSensitiveDetails(string? message)
    {
        if (string.IsNullOrWhiteSpace(message))
        {
            return false;
        }

        // Check for multiline content (indicates raw stack traces or internal logs)
        if (message.Contains('\n') || message.Contains('\r'))
        {
            return true;
        }

        // Check for sensitive database/infrastructure/type keywords
        foreach (var keyword in SensitiveKeywords)
        {
            if (message.Contains(keyword, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }
}


