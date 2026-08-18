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
    public class UpdateAccessPolicyCommand : IRequest<ApiResponse<bool>>
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public Guid ModuleId { get; set; }
        public string Action { get; set; } = null!;
        public string Effect { get; set; } = "Allow";
        public Guid? RoleId { get; set; }
        public Guid? UserId { get; set; }
        public string? RowFilterExpression { get; set; }
        public int Priority { get; set; } = 100;
        public bool IsActive { get; set; } = true;
        public DateTime? ExpiresAt { get; set; }
    }

    public class UpdateAccessPolicyCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        IPolicyCacheService policyCache,
        IRealtimeNotificationService realtimeNotificationService,
        IPolicyResourceRegistry resourceRegistry)
        : IRequestHandler<UpdateAccessPolicyCommand, ApiResponse<bool>>
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly ICurrentUserService _currentUserService = currentUserService;
        private readonly IPolicyCacheService _policyCache = policyCache;
        private readonly IRealtimeNotificationService _realtimeNotificationService = realtimeNotificationService;
        private readonly IPolicyResourceRegistry _resourceRegistry = resourceRegistry;

        public async Task<ApiResponse<bool>> Handle(UpdateAccessPolicyCommand request, CancellationToken cancellationToken)
        {
            var policy = await _unitOfWork.AccessPolicy.IncludeAndGetFirstOrDefaultAsync<object>(
                p => p.Id == request.Id && !p.IsDeleted,
                p => p.Conditions);
            if (policy == null)
            {
                return ApiResponse<bool>.Failure("Policy not found.", null, 404);
            }

            // Capture old targets for cache invalidation
            Guid? oldUserId = policy.UserId;
            Guid? oldRoleId = policy.RoleId;

            // Stateless checks (RoleId/UserId exclusivity, Effect, Priority range) are enforced by
            // UpdateAccessPolicyCommandValidator via the ValidationBehaviour pipeline.

            // Check module
            var module = await _unitOfWork.Module.GetFirstOrDefaultAsync(m => m.Id == request.ModuleId && !m.IsDeleted);
            if (module == null)
            {
                return ApiResponse<bool>.Failure("Module not found.", null, 404);
            }

            // Check name uniqueness (exclude self)
            var nameExists = await _unitOfWork.AccessPolicy.GetFirstOrDefaultAsync(
                p => p.Name.ToLower() == request.Name.ToLower() && p.Id != request.Id && !p.IsDeleted);
            if (nameExists != null)
            {
                return ApiResponse<bool>.Failure("A policy with this name already exists.", null, 409);
            }

            // Catch typo'd row-filter fields, and re-validate existing condition attributes in case the
            // module was changed to one with a different target entity.
            var fieldErrors = PolicyFieldValidationHelper.ValidateFieldNames(
                _resourceRegistry, module.Code, request.RowFilterExpression,
                policy.Conditions.Where(c => !c.IsDeleted).Select(c => c.Attribute));
            if (fieldErrors.Count > 0)
            {
                return ApiResponse<bool>.Failure(string.Join(" ", fieldErrors), null, 400);
            }

            DateTime? normalizedExpiresAt = request.ExpiresAt.HasValue
                ? DateTime.SpecifyKind(request.ExpiresAt.Value.ToUniversalTime(), DateTimeKind.Utc)
                : null;

            policy.Name = request.Name.Trim();
            policy.Description = request.Description;
            policy.ModuleId = request.ModuleId;
            policy.Action = request.Action.Trim();
            policy.Effect = request.Effect;
            policy.RoleId = request.RoleId;
            policy.UserId = request.UserId;
            policy.RowFilterExpression = request.RowFilterExpression;
            policy.Priority = request.Priority;
            policy.IsActive = request.IsActive;
            policy.ExpiresAt = normalizedExpiresAt;
            policy.ModifiedBy = _currentUserService.Username ?? "System";

            _unitOfWork.AccessPolicy.Update(policy);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Invalidate cache and notify for old targets
            if (oldUserId.HasValue)
            {
                await _policyCache.InvalidateUserPoliciesAsync(oldUserId.Value);
                await _realtimeNotificationService.NotifyUserPermissionsChangedAsync(oldUserId.Value, cancellationToken);
            }
            if (oldRoleId.HasValue)
            {
                await _policyCache.InvalidateRolePoliciesAsync(oldRoleId.Value);
                await _realtimeNotificationService.NotifyRolePermissionsChangedAsync(oldRoleId.Value, cancellationToken);
            }

            // Invalidate cache and notify for new targets (if different)
            if (policy.UserId.HasValue && policy.UserId != oldUserId)
            {
                await _policyCache.InvalidateUserPoliciesAsync(policy.UserId.Value);
                await _realtimeNotificationService.NotifyUserPermissionsChangedAsync(policy.UserId.Value, cancellationToken);
            }
            if (policy.RoleId.HasValue && policy.RoleId != oldRoleId)
            {
                await _policyCache.InvalidateRolePoliciesAsync(policy.RoleId.Value);
                await _realtimeNotificationService.NotifyRolePermissionsChangedAsync(policy.RoleId.Value, cancellationToken);
            }

            return ApiResponse<bool>.Success(true, "Access policy updated successfully.");
        }
    }
}
