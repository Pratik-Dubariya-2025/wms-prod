using MediatR;
using WMS.Application.Common.Attributes;
using WMS.Application.Common.Constants;
using WMS.Application.Common.Interfaces;
using WMS.Application.Common.Models;
using WMS.Domain.Interfaces;
using WMS.Domain.Models;

namespace WMS.Application.Features.Roles.Commands
{
    [RequirePermission(PermissionCodes.UserPermissionOverrideManage)]
    public class AddUserPermissionOverrideCommand : IRequest<ApiResponse<bool>>
    {
        public Guid UserId { get; set; }
        public string PermissionCode { get; set; } = null!;
        public bool IsGranted { get; set; }
        public string? Reason { get; set; }
        public DateTime? ExpiresAt { get; set; }
    }

    public class AddUserPermissionOverrideCommandHandler(
        IUnitOfWork unitOfWork, 
        IRealtimeNotificationService realtimeService,
        ICurrentUserService currentUserService) 
        : IRequestHandler<AddUserPermissionOverrideCommand, ApiResponse<bool>>
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly IRealtimeNotificationService _realtimeService = realtimeService;
        private readonly ICurrentUserService _currentUserService = currentUserService;

        public async Task<ApiResponse<bool>> Handle(AddUserPermissionOverrideCommand request, CancellationToken cancellationToken)
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

            DateTime? normalizedExpiresAt = request.ExpiresAt.HasValue
                ? DateTime.SpecifyKind(request.ExpiresAt.Value.ToUniversalTime(), DateTimeKind.Utc)
                : null;

            var exists = await _unitOfWork.UserPermissionOverride.GetFirstOrDefaultAsync(
                upo => upo.UserId == request.UserId && upo.PermissionId == permission.Id && !upo.IsDeleted);

            if (exists != null)
            {
                exists.IsGranted = request.IsGranted;
                exists.Reason = request.Reason;
                exists.ExpiresAt = normalizedExpiresAt;
                exists.ModifiedBy = _currentUserService.Username ?? "System";
                exists.ModifiedAt = DateTime.UtcNow;
                _unitOfWork.UserPermissionOverride.Update(exists);
            }
            else
            {
                var upo = new UserPermissionOverride
                {
                    UserId = request.UserId,
                    PermissionId = permission.Id,
                    IsGranted = request.IsGranted,
                    Reason = request.Reason,
                    ExpiresAt = normalizedExpiresAt,
                    CreatedBy = _currentUserService.Username ?? "System"
                };
                await _unitOfWork.UserPermissionOverride.AddAsync(upo);
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Invalidate cache + push updated permissions to this user via SignalR
            await _realtimeService.NotifyUserPermissionsChangedAsync(request.UserId, cancellationToken);

            return ApiResponse<bool>.Success(true, "Permission override added/updated successfully.");
        }
    }
}
