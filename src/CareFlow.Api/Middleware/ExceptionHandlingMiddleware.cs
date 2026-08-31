using System.Net;
using CareFlow.Application.Exceptions;
using CareFlow.Shared.Models;
using FluentValidationException = FluentValidation.ValidationException;

namespace CareFlow.Api.Middleware;

public class ExceptionHandlingMiddleware
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
        var (statusCode, response) = exception switch
        {
            NotFoundException notFound => (HttpStatusCode.NotFound, ApiResponse.Fail(notFound.Message)),
            ValidationException validation => (HttpStatusCode.BadRequest, ApiResponse.Fail(validation.Message, validation.Errors)),
            ConflictException conflict => (HttpStatusCode.Conflict, ApiResponse.Fail(conflict.Message)),
            UnauthorizedException unauthorized => (HttpStatusCode.Unauthorized, ApiResponse.Fail(unauthorized.Message)),
            ForbiddenException forbidden => (HttpStatusCode.Forbidden, ApiResponse.Fail(forbidden.Message)),
            FluentValidationException fluent => (HttpStatusCode.BadRequest, ApiResponse.Fail("Validation failed.", fluent.Errors.Select(e => e.ErrorMessage).ToList())),
            _ => (HttpStatusCode.InternalServerError, ApiResponse.Fail("An unexpected error occurred. Please try again."))
        };

        if (statusCode == HttpStatusCode.InternalServerError)
            _logger.LogError(exception, "Unhandled exception occurred.");
        else
            _logger.LogWarning(exception, "Handled exception: {Message}", exception.Message);

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;
        await context.Response.WriteAsJsonAsync(response);
    }
}
