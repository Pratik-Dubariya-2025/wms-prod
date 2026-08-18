using MediatR;
using WMS.Application.Common.Attributes;
using WMS.Application.Common.Constants;
using WMS.Application.Common.Interfaces;
using WMS.Application.Common.Models;
using WMS.Domain.Interfaces;

namespace WMS.Application.Features.Policies.Commands
{
    [RequirePermission(PermissionCodes.PolicyWrite)]
    public class DeleteAccessPolicyCommand : IRequest<ApiResponse<bool>>
    {
        public Guid Id { get; set; }
    }

    public class DeleteAccessPolicyCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        IPolicyCacheService policyCache,
        IRealtimeNotificationService realtimeNotificationService)
        : IRequestHandler<DeleteAccessPolicyCommand, ApiResponse<bool>>
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly ICurrentUserService _currentUserService = currentUserService;
        private readonly IPolicyCacheService _policyCache = policyCache;
        private readonly IRealtimeNotificationService _realtimeNotificationService = realtimeNotificationService;

        public async Task<ApiResponse<bool>> Handle(DeleteAccessPolicyCommand request, CancellationToken cancellationToken)
        {
            var policy = await _unitOfWork.AccessPolicy.GetFirstOrDefaultAsync(p => p.Id == request.Id && !p.IsDeleted);
            if (policy == null)
            {
                return ApiResponse<bool>.Failure("Policy not found.", null, 404);
            }

            // Soft delete
            policy.IsDeleted = true;
            policy.DeletedBy = _currentUserService.Username ?? "System";
            policy.DeletedAt = DateTime.UtcNow;

            _unitOfWork.AccessPolicy.Update(policy);
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

            return ApiResponse<bool>.Success(true, "Access policy deleted successfully.");
        }
    }
}
