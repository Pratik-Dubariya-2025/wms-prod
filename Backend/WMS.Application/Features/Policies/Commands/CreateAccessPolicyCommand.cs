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
    public class CreateAccessPolicyCommand : IRequest<ApiResponse<Guid>>
    {
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public Guid ModuleId { get; set; }
        public string Action { get; set; } = null!;
        public string Effect { get; set; } = "Allow";
        public Guid? RoleId { get; set; }
        public Guid? UserId { get; set; }
        public string? RowFilterExpression { get; set; }
        public int Priority { get; set; } = 100;
        public DateTime? ExpiresAt { get; set; }

        /// <summary>Conditions to attach to this policy (optional).</summary>
        public List<CreatePolicyConditionInput> Conditions { get; set; } = [];

        /// <summary>Field restrictions to attach to this policy (optional).</summary>
        public List<CreateFieldRestrictionInput> FieldRestrictions { get; set; } = [];
    }

    public class CreatePolicyConditionInput
    {
        public int ConditionGroup { get; set; }
        public string Attribute { get; set; } = null!;
        public string Operator { get; set; } = null!;
        public string Value { get; set; } = null!;
    }

    public class CreateFieldRestrictionInput
    {
        public string FieldName { get; set; } = null!;
        public string RestrictionType { get; set; } = "Hide";
        public string? MaskPattern { get; set; }
    }

    public class CreateAccessPolicyCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        IPolicyCacheService policyCache,
        IRealtimeNotificationService realtimeNotificationService,
        IPolicyResourceRegistry resourceRegistry)
        : IRequestHandler<CreateAccessPolicyCommand, ApiResponse<Guid>>
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly ICurrentUserService _currentUserService = currentUserService;
        private readonly IPolicyCacheService _policyCache = policyCache;
        private readonly IRealtimeNotificationService _realtimeNotificationService = realtimeNotificationService;
        private readonly IPolicyResourceRegistry _resourceRegistry = resourceRegistry;

        public async Task<ApiResponse<Guid>> Handle(CreateAccessPolicyCommand request, CancellationToken cancellationToken)
        {
            // Stateless checks (RoleId/UserId exclusivity, Effect, Priority range) are enforced by
            // CreateAccessPolicyCommandValidator via the ValidationBehaviour pipeline. Only DB-dependent
            // checks remain here.

            // Validate Module exists
            var module = await _unitOfWork.Module.GetFirstOrDefaultAsync(m => m.Id == request.ModuleId && !m.IsDeleted);
            if (module == null)
            {
                return ApiResponse<Guid>.Failure("Module not found.", null, 404);
            }

            // Validate Role exists (if specified)
            if (request.RoleId != null)
            {
                var role = await _unitOfWork.Role.GetFirstOrDefaultAsync(r => r.Id == request.RoleId && !r.IsDeleted);
                if (role == null)
                {
                    return ApiResponse<Guid>.Failure("Role not found.", null, 404);
                }
            }

            // Validate User exists (if specified)
            if (request.UserId != null)
            {
                var user = await _unitOfWork.User.GetFirstOrDefaultAsync(u => u.Id == request.UserId && !u.IsDeleted);
                if (user == null)
                {
                    return ApiResponse<Guid>.Failure("User not found.", null, 404);
                }
            }

            // Check for duplicate policy name
            var nameExists = await _unitOfWork.AccessPolicy.GetFirstOrDefaultAsync(
                p => p.Name.ToLower() == request.Name.ToLower() && !p.IsDeleted);
            if (nameExists != null)
            {
                return ApiResponse<Guid>.Failure("A policy with this name already exists.", null, 409);
            }

            // Catch typo'd condition attributes / row-filter fields before they silently produce an
            // always-false (always-deny) filter later.
            var fieldErrors = PolicyFieldValidationHelper.ValidateFieldNames(
                _resourceRegistry, module.Code, request.RowFilterExpression,
                request.Conditions.Select(c => c.Attribute));
            if (fieldErrors.Count > 0)
            {
                return ApiResponse<Guid>.Failure(string.Join(" ", fieldErrors), null, 400);
            }

            DateTime? normalizedExpiresAt = request.ExpiresAt.HasValue
                ? DateTime.SpecifyKind(request.ExpiresAt.Value.ToUniversalTime(), DateTimeKind.Utc)
                : null;

            var policy = new AccessPolicy
            {
                Name = request.Name.Trim(),
                Description = request.Description,
                ModuleId = request.ModuleId,
                Action = request.Action.Trim(),
                Effect = request.Effect,
                RoleId = request.RoleId,
                UserId = request.UserId,
                RowFilterExpression = request.RowFilterExpression,
                Priority = request.Priority,
                IsActive = true,
                ExpiresAt = normalizedExpiresAt,
                CreatedBy = _currentUserService.Username ?? "System"
            };

            // Add conditions
            foreach (var condInput in request.Conditions)
            {
                policy.Conditions.Add(new PolicyCondition
                {
                    ConditionGroup = condInput.ConditionGroup,
                    Attribute = condInput.Attribute.Trim(),
                    Operator = condInput.Operator.Trim().ToLower(),
                    Value = condInput.Value.Trim(),
                    CreatedBy = _currentUserService.Username ?? "System"
                });
            }

            // Add field restrictions
            foreach (var fieldInput in request.FieldRestrictions)
            {
                policy.FieldRestrictions.Add(new FieldRestriction
                {
                    FieldName = fieldInput.FieldName.Trim(),
                    RestrictionType = fieldInput.RestrictionType,
                    MaskPattern = fieldInput.MaskPattern,
                    CreatedBy = _currentUserService.Username ?? "System"
                });
            }

            await _unitOfWork.AccessPolicy.AddAsync(policy);
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

            return ApiResponse<Guid>.Success(policy.Id, "Access policy created successfully.", 201);
        }
    }
}
