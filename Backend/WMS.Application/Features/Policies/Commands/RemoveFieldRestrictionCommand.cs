using MediatR;
using WMS.Application.Common.Attributes;
using WMS.Application.Common.Constants;
using WMS.Application.Common.Interfaces;
using WMS.Application.Common.Models;
using WMS.Domain.Interfaces;

namespace WMS.Application.Features.Policies.Commands
{
    [RequirePermission(PermissionCodes.PolicyWrite)]
    public class RemoveFieldRestrictionCommand : IRequest<ApiResponse<bool>>
    {
        public Guid PolicyId { get; set; }
        public Guid FieldRestrictionId { get; set; }
    }

    public class RemoveFieldRestrictionCommandHandler(
        IUnitOfWork unitOfWork,
        IPolicyCacheService policyCache,
        IRealtimeNotificationService realtimeNotificationService)
        : IRequestHandler<RemoveFieldRestrictionCommand, ApiResponse<bool>>
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly IPolicyCacheService _policyCache = policyCache;
        private readonly IRealtimeNotificationService _realtimeNotificationService = realtimeNotificationService;

        public async Task<ApiResponse<bool>> Handle(RemoveFieldRestrictionCommand request, CancellationToken cancellationToken)
        {
            var restriction = await _unitOfWork.FieldRestriction.GetFirstOrDefaultAsync(
                f => f.Id == request.FieldRestrictionId && f.PolicyId == request.PolicyId && !f.IsDeleted);

            if (restriction == null)
            {
                return ApiResponse<bool>.Failure("Field restriction not found.", null, 404);
            }

            var policy = await _unitOfWork.AccessPolicy.GetFirstOrDefaultAsync(p => p.Id == request.PolicyId && !p.IsDeleted);

            _unitOfWork.FieldRestriction.Remove(restriction);
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

            return ApiResponse<bool>.Success(true, "Field restriction removed successfully.");
        }
    }
}
