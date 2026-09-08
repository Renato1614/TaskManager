using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using TaskManager.Application.Common;

namespace TaskManager.Api.Middleware;

public sealed class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var (statusCode, title, errors) = exception switch
        {
            ValidationException validationException => (
                StatusCodes.Status400BadRequest,
                "Validation failed",
                validationException.Errors.Select(x => x.ErrorMessage).ToArray()),

            AppException appException => (
                GetStatusCode(appException.Type),
                appException.Message,
                null),

            UnauthorizedAccessException unauthorizedAccessException => (
                StatusCodes.Status401Unauthorized,
                unauthorizedAccessException.Message,
                null),

            _ => (
                StatusCodes.Status500InternalServerError,
                "An unexpected error occurred.",
                null)
        };

        if (statusCode == StatusCodes.Status500InternalServerError)
        {
            logger.LogError(exception, "Unhandled request failure.");
        }

        httpContext.Response.StatusCode = statusCode;

        if (statusCode == StatusCodes.Status204NoContent)
        {
            return true;
        }

        var problem = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Instance = httpContext.Request.Path
        };

        if (errors is not null)
        {
            problem.Extensions["errors"] = errors;
        }

        await httpContext.Response.WriteAsJsonAsync(problem, cancellationToken);
        return true;
    }

    private static int GetStatusCode(AppErrorType errorType) => errorType switch
    {
        AppErrorType.Conflict => StatusCodes.Status409Conflict,
        AppErrorType.Unauthorized => StatusCodes.Status401Unauthorized,
        AppErrorType.Forbidden => StatusCodes.Status403Forbidden,
        AppErrorType.NotFound => StatusCodes.Status204NoContent,
        _ => StatusCodes.Status400BadRequest
    };
}
