using MediatR;
using WMS.Application.Common.Attributes;
using WMS.Application.Common.Constants;
using WMS.Application.Common.Interfaces;
using WMS.Application.Common.Models;
using WMS.Domain.Interfaces;

namespace WMS.Application.Features.Policies.Commands
{
    [RequirePermission(PermissionCodes.PolicyWrite)]
    public class RemovePolicyConditionCommand : IRequest<ApiResponse<bool>>
    {
        public Guid PolicyId { get; set; }
        public Guid ConditionId { get; set; }
    }

    public class RemovePolicyConditionCommandHandler(
        IUnitOfWork unitOfWork,
        IPolicyCacheService policyCache,
        IRealtimeNotificationService realtimeNotificationService)
        : IRequestHandler<RemovePolicyConditionCommand, ApiResponse<bool>>
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly IPolicyCacheService _policyCache = policyCache;
        private readonly IRealtimeNotificationService _realtimeNotificationService = realtimeNotificationService;

        public async Task<ApiResponse<bool>> Handle(RemovePolicyConditionCommand request, CancellationToken cancellationToken)
        {
            var condition = await _unitOfWork.PolicyCondition.GetFirstOrDefaultAsync(
                c => c.Id == request.ConditionId && c.PolicyId == request.PolicyId && !c.IsDeleted);

            if (condition == null)
            {
                return ApiResponse<bool>.Failure("Condition not found.", null, 404);
            }

            var policy = await _unitOfWork.AccessPolicy.GetFirstOrDefaultAsync(p => p.Id == request.PolicyId && !p.IsDeleted);

            _unitOfWork.PolicyCondition.Remove(condition);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Invalidate cache and notify
            if (policy != null)
            {
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
            }

            return ApiResponse<bool>.Success(true, "Condition removed successfully.");
        }
    }
}
