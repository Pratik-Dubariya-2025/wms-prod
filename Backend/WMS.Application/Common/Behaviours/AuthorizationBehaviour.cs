using System.Reflection;
using MediatR;
using WMS.Application.Common.Attributes;
using WMS.Application.Common.Interfaces;
using WMS.Application.Common.Models;

namespace WMS.Application.Common.Behaviours
{
    /// <summary>
    /// MediatR pipeline behavior that intercepts requests decorated with
    /// <see cref="RequirePermissionAttribute"/> and enforces permission checks
    /// before the handler executes.
    /// 
    /// This runs early in the pipeline (before validation) so unauthorized
    /// requests are rejected without wasting resources on validation or DB queries.
    /// </summary>
    public class AuthorizationBehaviour<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : notnull
    {
        private readonly ICurrentUserService _currentUserService;
        private readonly IFieldRestrictionManager _fieldRestrictionManager;

        public AuthorizationBehaviour(ICurrentUserService currentUserService, IFieldRestrictionManager fieldRestrictionManager)
        {
            _currentUserService = currentUserService;
            _fieldRestrictionManager = fieldRestrictionManager;
        }

        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            var attribute = typeof(TRequest).GetCustomAttribute<RequirePermissionAttribute>();

            // No attribute → no permission required → proceed
            if (attribute == null || attribute.Permissions.Length == 0)
            {
                return await next();
            }

            // User must be authenticated
            if (!_currentUserService.IsAuthenticated)
            {
                return CreateFailureResponse("You are not authenticated.", 401);
            }

            // If PBAC has already evaluated and allowed this request, bypass the flat RBAC permission check.
            if (_fieldRestrictionManager.IsPbacAuthorized)
            {
                return await next();
            }

            // Check permissions based on RequireAny flag
            bool hasPermission = attribute.RequireAny
                ? attribute.Permissions.Any(p => _currentUserService.HasPermission(p))
                : attribute.Permissions.All(p => _currentUserService.HasPermission(p));

            if (!hasPermission)
            {
                string required = string.Join(", ", attribute.Permissions);
                return CreateFailureResponse(
                    $"You do not have the required permission(s): {required}.", 403);
            }

            return await next();
        }

        /// <summary>
        /// Creates an ApiResponse failure result. Works with ApiResponse{T} responses.
        /// Falls back to throwing ForbiddenAccessException for non-ApiResponse types.
        /// </summary>
        private static TResponse CreateFailureResponse(string message, int statusCode)
        {
            // Check if TResponse is ApiResponse<T>
            Type responseType = typeof(TResponse);

            if (responseType.IsGenericType && responseType.GetGenericTypeDefinition() == typeof(ApiResponse<>))
            {
                // Use reflection to call ApiResponse<T>.Failure(message, statusCode)
                var failureMethod = responseType.GetMethod(
                    nameof(ApiResponse<object>.Failure),
                    [typeof(string), typeof(IDictionary<string, string[]>), typeof(int)]);

                if (failureMethod != null)
                {
                    object? result = failureMethod.Invoke(null, [message, null, statusCode]);
                    return (TResponse)result!;
                }
            }

            // Fallback for non-ApiResponse return types
            throw new UnauthorizedAccessException(message);
        }
    }
}
