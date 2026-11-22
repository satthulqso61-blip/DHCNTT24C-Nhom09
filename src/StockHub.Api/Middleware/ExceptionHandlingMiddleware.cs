using System.Diagnostics;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using StockHub.Application.Common.Exceptions;

namespace StockHub.Api.Middleware;

public sealed class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var traceId = Activity.Current?.Id ?? context.TraceIdentifier;

        context.Response.ContentType = "application/problem+json";

        switch (exception)
        {
            case ValidationException validationException:
            {
                context.Response.StatusCode = StatusCodes.Status400BadRequest;

                var problemDetails = new HttpValidationProblemDetails(validationException.Errors)
                {
                    Status = StatusCodes.Status400BadRequest,
                    Title = "Validation failure",
                    Detail = "One or more validation errors occurred.",
                    Instance = context.Request.Path,
                    Extensions =
                    {
                        ["code"] = "VALIDATION_ERROR",
                        ["traceId"] = traceId
                    }
                };

                await context.Response.WriteAsync(JsonSerializer.Serialize(problemDetails));
                break;
            }

            case AppException appException:
            {
                context.Response.StatusCode = appException.StatusCode;

                var problemDetails = new ProblemDetails
                {
                    Status = appException.StatusCode,
                    Title = "Business rule error",
                    Detail = appException.Message,
                    Instance = context.Request.Path,
                    Extensions =
                    {
                        ["code"] = appException.ErrorCode,
                        ["traceId"] = traceId
                    }
                };

                await context.Response.WriteAsync(JsonSerializer.Serialize(problemDetails));
                break;
            }

            default:
            {
                _logger.LogError(exception, "An unhandled exception occurred: {Message}", exception.Message);

                context.Response.StatusCode = StatusCodes.Status500InternalServerError;

                var problemDetails = new ProblemDetails
                {
                    Status = StatusCodes.Status500InternalServerError,
                    Title = "An error occurred while processing your request.",
                    Detail = "Internal Server Error",
                    Instance = context.Request.Path,
                    Extensions =
                    {
                        ["code"] = "INTERNAL_SERVER_ERROR",
                        ["traceId"] = traceId
                    }
                };

                await context.Response.WriteAsync(JsonSerializer.Serialize(problemDetails));
                break;
            }
        }
    }
}
