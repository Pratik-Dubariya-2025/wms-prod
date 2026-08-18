using MediatR;
using WMS.Application.Common.Attributes;
using WMS.Application.Common.Constants;
using WMS.Application.Common.Interfaces;
using WMS.Application.Common.Models;
using WMS.Domain.Interfaces;
using WMS.Domain.Models;

namespace WMS.Application.Features.Policies.Commands
{
    [RequirePermission(PermissionCodes.PolicyWrite)]
    public class AddPolicyConditionCommand : IRequest<ApiResponse<Guid>>
    {
        public Guid PolicyId { get; set; }
        public int ConditionGroup { get; set; }
        public string Attribute { get; set; } = null!;
        public string Operator { get; set; } = null!;
        public string Value { get; set; } = null!;
    }

    public class AddPolicyConditionCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        IPolicyCacheService policyCache,
        IRealtimeNotificationService realtimeNotificationService,
        IPolicyResourceRegistry resourceRegistry)
        : IRequestHandler<AddPolicyConditionCommand, ApiResponse<Guid>>
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly ICurrentUserService _currentUserService = currentUserService;
        private readonly IPolicyCacheService _policyCache = policyCache;
        private readonly IRealtimeNotificationService _realtimeNotificationService = realtimeNotificationService;
        private readonly IPolicyResourceRegistry _resourceRegistry = resourceRegistry;

        private static readonly HashSet<string> ValidOperators = new(StringComparer.OrdinalIgnoreCase)
        {
            "eq", "neq", "in", "not_in", "contains", "gt", "lt", "gte", "lte"
        };

        public async Task<ApiResponse<Guid>> Handle(AddPolicyConditionCommand request, CancellationToken cancellationToken)
        {
            var policy = await _unitOfWork.AccessPolicy.IncludeAndGetFirstOrDefaultAsync<object>(
                p => p.Id == request.PolicyId && !p.IsDeleted,
                p => p.Module);
            if (policy == null)
            {
                return ApiResponse<Guid>.Failure("Policy not found.", null, 404);
            }

            string op = request.Operator.Trim().ToLower();
            if (!ValidOperators.Contains(op))
            {
                return ApiResponse<Guid>.Failure(
                    $"Invalid operator '{request.Operator}'. Valid operators: {string.Join(", ", ValidOperators)}.", null, 400);
            }

            var fieldErrors = PolicyFieldValidationHelper.ValidateFieldNames(
                _resourceRegistry, policy.Module.Code, null, [request.Attribute]);
            if (fieldErrors.Count > 0)
            {
                return ApiResponse<Guid>.Failure(string.Join(" ", fieldErrors), null, 400);
            }

            var condition = new PolicyCondition
            {
                PolicyId = request.PolicyId,
                ConditionGroup = request.ConditionGroup,
                Attribute = request.Attribute.Trim(),
                Operator = op,
                Value = request.Value.Trim(),
                CreatedBy = _currentUserService.Username ?? "System"
            };

            await _unitOfWork.PolicyCondition.AddAsync(condition);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Invalidate cache and notify
            if (policy.UserId.HasValue)
            {
                await _policyCache.InvalidateUserPoliciesAsync(policy.UserId.Value);
                await _realtimeNotificationService.NotifyUserPermissionsChangedAsync(policy.UserId.Value, cancellationToken);
            }
            else if (policy.RoleId.HasValue)
            {
                await _policyCache.InvalidateRolePoliciesAsync(policy.RoleId.Value);
                await _realtimeNotificationService.NotifyRolePermissionsChangedAsync(policy.RoleId.Value, cancellationToken);
            }

            return ApiResponse<Guid>.Success(condition.Id, "Condition added successfully.", 201);
        }
    }
}
