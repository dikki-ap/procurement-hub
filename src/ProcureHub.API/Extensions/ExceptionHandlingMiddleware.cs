using System.Text.Json;
using ProcureHub.SharedKernel.Common;
using ProcureHub.SharedKernel.Exceptions;

namespace ProcureHub.API.Extensions;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next   = next;
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
            NotFoundException e => (
                StatusCodes.Status404NotFound,
                ApiResponse<object?>.Fail(e.Message)),

            ValidationException e => (
                StatusCodes.Status422UnprocessableEntity,
                new ApiResponse<object?>
                {
                    Success = false,
                    Message = "One or more validation failures occurred.",
                    Errors  = e.Errors
                }),

            ForbiddenException e => (
                StatusCodes.Status403Forbidden,
                ApiResponse<object?>.Fail(e.Message)),

            ConflictException e => (
                StatusCodes.Status409Conflict,
                ApiResponse<object?>.Fail(e.Message)),

            BusinessRuleException e => (
                StatusCodes.Status422UnprocessableEntity,
                ApiResponse<object?>.Fail(e.Message)),

            UnauthorizedAccessException e => (
                StatusCodes.Status401Unauthorized,
                ApiResponse<object?>.Fail(e.Message)),

            _ => (
                StatusCodes.Status500InternalServerError,
                ApiResponse<object?>.Fail("An unexpected error occurred."))
        };

        if (statusCode == StatusCodes.Status500InternalServerError)
            _logger.LogError(exception, "Unhandled exception");
        else
            _logger.LogWarning(exception, "Handled exception: {ExceptionType}", exception.GetType().Name);

        context.Response.StatusCode  = statusCode;
        context.Response.ContentType = "application/json";

        var json = JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await context.Response.WriteAsync(json);
    }
}

public static class ExceptionHandlingMiddlewareExtensions
{
    public static IApplicationBuilder UseExceptionHandling(this IApplicationBuilder app)
        => app.UseMiddleware<ExceptionHandlingMiddleware>();
}
