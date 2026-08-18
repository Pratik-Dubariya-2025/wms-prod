using System.Reflection;
using MediatR;
using WMS.Application.Common.Attributes;
using WMS.Application.Common.Interfaces;
using WMS.Application.Common.Models;
using WMS.Domain.Interfaces;

namespace WMS.Application.Common.Behaviours
{
    /// <summary>
    /// MediatR pipeline behavior that evaluates PBAC (Policy-Based Access Control) policies.
    /// If policies exist for the requested module and action, they are evaluated.
    /// If no policies exist, it falls back to the standard RBAC AuthorizationBehaviour.
    /// </summary>
    public class PolicyEvaluationBehaviour<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : notnull
    {
        private readonly ICurrentUserService _currentUserService;
        private readonly IPolicyEvaluationService _policyEvaluationService;
        private readonly IFieldRestrictionManager _fieldRestrictionManager;
        private readonly IUnitOfWork _unitOfWork;

        public PolicyEvaluationBehaviour(
            ICurrentUserService currentUserService,
            IPolicyEvaluationService policyEvaluationService,
            IFieldRestrictionManager fieldRestrictionManager,
            IUnitOfWork unitOfWork)
        {
            _currentUserService = currentUserService;
            _policyEvaluationService = policyEvaluationService;
            _fieldRestrictionManager = fieldRestrictionManager;
            _unitOfWork = unitOfWork;
        }

        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            var attribute = typeof(TRequest).GetCustomAttribute<RequirePermissionAttribute>();

            // No permission required -> proceed
            if (attribute == null || attribute.Permissions.Length == 0)
            {
                return await next();
            }

            // User must be authenticated
            if (!_currentUserService.IsAuthenticated || !_currentUserService.UserId.HasValue)
            {
                return CreateFailureResponse("You are not authenticated.", 401);
            }

            var userId = _currentUserService.UserId.Value;

            // We evaluate the primary permission. If there are multiple, evaluate the first or all.
            // In standard CRUD features, there is exactly one permission code.
            var permissionCode = attribute.Permissions[0];
            var parts = permissionCode.Split('.');
            
            if (parts.Length >= 2)
            {
                var moduleCode = parts[0];
                var action = parts[1];

                var perm = await _unitOfWork.Permission.IncludeAndGetFirstOrDefaultAsync<object>(
                    p => p.Code.ToLower() == permissionCode.ToLower(),
                    p => p.Module
                );

                if (perm != null && perm.Module != null)
                {
                    moduleCode = perm.Module.Code;
                    action = perm.Action;
                }

                // Check PBAC
                var evaluationResult = await _policyEvaluationService.EvaluateAsync(userId, moduleCode, action, request);

                if (!evaluationResult.NoPoliciesExist)
                {
                    if (!evaluationResult.IsAllowed)
                    {
                        return CreateFailureResponse(
                            evaluationResult.DenialReason ?? "Access denied by policy.", 403);
                    }

                    // Save field restrictions, scoped to the module they were evaluated against so the
                    // response filter doesn't mask same-named properties on unrelated DTOs.
                    if (evaluationResult.FieldRestrictions.Any())
                    {
                        _fieldRestrictionManager.SetRestrictions(evaluationResult.FieldRestrictions);
                        _fieldRestrictionManager.ModuleCode = moduleCode;
                    }

                    // Set flag to bypass AuthorizationBehaviour
                    _fieldRestrictionManager.IsPbacAuthorized = true;

                    // Policy allowed access! Bypass RBAC by jumping straight to request handler execution.
                    // (But still allow subsequent behaviors like Validation and Performance logging to run).
                    // To do this, we proceed to next(), which executes the rest of the pipeline.
                    // Since we return here, we short-circuit AuthorizationBehaviour (assuming PolicyEvaluationBehaviour runs BEFORE AuthorizationBehaviour).
                    return await next();
                }
            }

            // Fallback to next behavior in pipeline (which will be AuthorizationBehaviour if registered in that order)
            return await next();
        }

        private static TResponse CreateFailureResponse(string message, int statusCode)
        {
            Type responseType = typeof(TResponse);

            if (responseType.IsGenericType && responseType.GetGenericTypeDefinition() == typeof(ApiResponse<>))
            {
                var failureMethod = responseType.GetMethod(
                    nameof(ApiResponse<object>.Failure),
                    [typeof(string), typeof(IDictionary<string, string[]>), typeof(int)]);

                if (failureMethod != null)
                {
                    object? result = failureMethod.Invoke(null, [message, null, statusCode]);
                    return (TResponse)result!;
                }
            }

            throw new UnauthorizedAccessException(message);
        }
    }
}
