using System.Text.Json;
using WMS.Application.Common.Exceptions;
using WMS.Application.Common.Models;

namespace WMS.API.Middleware
{
    public class CustomExceptionHandlerMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<CustomExceptionHandlerMiddleware> _logger;

        public CustomExceptionHandlerMiddleware(RequestDelegate next, ILogger<CustomExceptionHandlerMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task Invoke(HttpContext context)
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
            var code = StatusCodes.Status500InternalServerError;
            var result = string.Empty;

            switch (exception)
            {
                case ValidationException validationException:
                    code = StatusCodes.Status400BadRequest;
                    result = JsonSerializer.Serialize(
                        ApiResponse.Failure(
                            message: "One or more validation failures occurred.",
                            errors: validationException.Errors,
                            statusCode: code
                        )
                    );
                    break;

                case NotFoundException notFoundException:
                    code = StatusCodes.Status404NotFound;
                    result = JsonSerializer.Serialize(
                        ApiResponse.Failure(
                            message: notFoundException.Message,
                            errors: null,
                            statusCode: code
                        )
                    );
                    break;

                case ForbiddenException:
                    code = StatusCodes.Status403Forbidden;
                    result = JsonSerializer.Serialize(
                        ApiResponse.Failure(
                            message: "You are not authorized to access this resource.",
                            errors: null,
                            statusCode: code
                        )
                    );
                    break;

                case ConflictException conflictException:
                    code = StatusCodes.Status409Conflict;
                    result = JsonSerializer.Serialize(
                        ApiResponse.Failure(
                            message: conflictException.Message,
                            errors: null,
                            statusCode: code
                        )
                    );
                    break;

                default:
                    _logger.LogError(exception, "WMS Request Exception: An unhandled exception occurred.");
                    result = JsonSerializer.Serialize(
                        ApiResponse.Failure(
                            message: "An internal server error occurred.",
                            errors: null,
                            statusCode: code
                        )
                    );
                    break;
            }

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = code;

            await context.Response.WriteAsync(result);
        }
    }

    public static class CustomExceptionHandlerMiddlewareExtensions
    {
        public static IApplicationBuilder UseCustomExceptionHandler(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<CustomExceptionHandlerMiddleware>();
        }
    }
}
