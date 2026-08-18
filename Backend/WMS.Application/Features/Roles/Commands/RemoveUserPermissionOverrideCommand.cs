using MediatR;
using WMS.Application.Common.Attributes;
using WMS.Application.Common.Constants;
using WMS.Application.Common.Interfaces;
using WMS.Application.Common.Models;
using WMS.Domain.Interfaces;

namespace WMS.Application.Features.Roles.Commands
{
    [RequirePermission(PermissionCodes.UserPermissionOverrideManage)]
    public class RemoveUserPermissionOverrideCommand : IRequest<ApiResponse<bool>>
    {
        public Guid UserId { get; set; }
        public string PermissionCode { get; set; } = null!;
    }

    public class RemoveUserPermissionOverrideCommandHandler(
        IUnitOfWork unitOfWork, 
        IRealtimeNotificationService realtimeService,
        ICurrentUserService currentUserService) 
        : IRequestHandler<RemoveUserPermissionOverrideCommand, ApiResponse<bool>>
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly IRealtimeNotificationService _realtimeService = realtimeService;
        private readonly ICurrentUserService _currentUserService = currentUserService;

        public async Task<ApiResponse<bool>> Handle(RemoveUserPermissionOverrideCommand request, CancellationToken cancellationToken)
        {
            var user = await _unitOfWork.User.GetFirstOrDefaultAsync(u => u.Id == request.UserId && !u.IsDeleted);
            if (user == null)
            {
                return ApiResponse<bool>.Failure("User not found.", null, 404);
            }

            var permission = await _unitOfWork.Permission.GetFirstOrDefaultAsync(p => p.Code == request.PermissionCode && !p.IsDeleted);
            if (permission == null)
            {
                return ApiResponse<bool>.Failure("Permission not found.", null, 404);
            }

            var exists = await _unitOfWork.UserPermissionOverride.GetFirstOrDefaultAsync(
                upo => upo.UserId == request.UserId && upo.PermissionId == permission.Id && !upo.IsDeleted);

            if (exists == null)
            {
                return ApiResponse<bool>.Success(true, "Permission override does not exist for this user.");
            }

            exists.IsDeleted = true;
            exists.DeletedBy = _currentUserService.Username ?? "System";
            exists.DeletedAt = DateTime.UtcNow;

            _unitOfWork.UserPermissionOverride.Update(exists);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Invalidate cache + push updated permissions to this user via SignalR
            await _realtimeService.NotifyUserPermissionsChangedAsync(request.UserId, cancellationToken);

            return ApiResponse<bool>.Success(true, "Permission override removed successfully.");
        }
    }
}
