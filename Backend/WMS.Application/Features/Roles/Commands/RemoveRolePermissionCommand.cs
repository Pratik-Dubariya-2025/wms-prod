using MediatR;
using WMS.Application.Common.Attributes;
using WMS.Application.Common.Constants;
using WMS.Application.Common.Interfaces;
using WMS.Application.Common.Models;
using WMS.Domain.Interfaces;

namespace WMS.Application.Features.Roles.Commands
{
    [RequirePermission(PermissionCodes.RoleUpdate)]
    public class RemoveRolePermissionCommand : IRequest<ApiResponse<bool>>
    {
        public Guid RoleId { get; set; }
        public string PermissionCode { get; set; } = null!;
    }

    public class RemoveRolePermissionCommandHandler(
        IUnitOfWork unitOfWork,
        IRealtimeNotificationService realtimeService) 
        : IRequestHandler<RemoveRolePermissionCommand, ApiResponse<bool>>
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly IRealtimeNotificationService _realtimeService = realtimeService;

        public async Task<ApiResponse<bool>> Handle(RemoveRolePermissionCommand request, CancellationToken cancellationToken)
        {
            var role = await _unitOfWork.Role.GetFirstOrDefaultAsync(r => r.Id == request.RoleId && !r.IsDeleted);
            if (role == null)
            {
                return ApiResponse<bool>.Failure("Role not found.", null, 404);
            }

            var permission = await _unitOfWork.Permission.GetFirstOrDefaultAsync(p => p.Code == request.PermissionCode && !p.IsDeleted);
            if (permission == null)
            {
                return ApiResponse<bool>.Failure("Permission not found.", null, 404);
            }

            var exists = await _unitOfWork.RolePermission.GetFirstOrDefaultAsync(rp => rp.RoleId == request.RoleId && rp.PermissionId == permission.Id);
            if (exists == null)
            {
                return ApiResponse<bool>.Success(true, "Permission is not assigned to this role.");
            }

            _unitOfWork.RolePermission.Remove(exists);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Invalidate caches + push updated permissions to all users in this role via SignalR
            await _realtimeService.NotifyRolePermissionsChangedAsync(request.RoleId, cancellationToken);

            return ApiResponse<bool>.Success(true, "Permission removed from role successfully.");
        }
    }
}
