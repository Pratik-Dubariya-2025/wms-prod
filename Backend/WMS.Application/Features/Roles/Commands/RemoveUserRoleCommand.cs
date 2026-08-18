using MediatR;
using WMS.Application.Common.Attributes;
using WMS.Application.Common.Constants;
using WMS.Application.Common.Interfaces;
using WMS.Application.Common.Models;
using WMS.Domain.Interfaces;

namespace WMS.Application.Features.Roles.Commands
{
    [RequirePermission(PermissionCodes.RoleUpdate)]
    public class RemoveUserRoleCommand : IRequest<ApiResponse<bool>>
    {
        public Guid UserId { get; set; }
        public Guid RoleId { get; set; }
    }

    public class RemoveUserRoleCommandHandler(
        IUnitOfWork unitOfWork, 
        ICurrentUserService currentUserService,
        IRealtimeNotificationService realtimeService) 
        : IRequestHandler<RemoveUserRoleCommand, ApiResponse<bool>>
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly ICurrentUserService _currentUserService = currentUserService;
        private readonly IRealtimeNotificationService _realtimeService = realtimeService;

        public async Task<ApiResponse<bool>> Handle(RemoveUserRoleCommand request, CancellationToken cancellationToken)
        {
            if (!_currentUserService.UserId.HasValue)
            {
                return ApiResponse<bool>.Failure("Unauthorized access.", null, 401);
            }

            Guid currentUserId = _currentUserService.UserId.Value;

            var targetUser = await _unitOfWork.User.GetFirstOrDefaultAsync(u => u.Id == request.UserId && !u.IsDeleted);
            if (targetUser == null)
            {
                return ApiResponse<bool>.Failure("User not found.", null, 404);
            }

            var role = await _unitOfWork.Role.GetFirstOrDefaultAsync(r => r.Id == request.RoleId && !r.IsDeleted);
            if (role == null)
            {
                return ApiResponse<bool>.Failure("Role not found.", null, 404);
            }

            // Prevent self-demotion from Admin
            if (targetUser.Id == currentUserId && role.Name.ToUpperInvariant() == "ADMIN")
            {
                return ApiResponse<bool>.Failure("You cannot remove your own Admin role.", null, 400);
            }

            // Enforce Hierarchy check
            if (targetUser.Id != currentUserId)
            {
                var currentUserRoles = await _unitOfWork.Role.GetAllAsync(r => r, r => r.UserRoles.Any(ur => ur.UserId == currentUserId));
                int currentMaxRank = GetMaxRoleRank(currentUserRoles.Select(r => r.Name));
                
                int targetRoleRank = GetRoleRank(role.Name);
                if (targetRoleRank >= currentMaxRank)
                {
                    return ApiResponse<bool>.Failure("Hierarchy violation: You cannot remove a role with a rank equal to or higher than yours.", null, 403);
                }
            }

            var exists = await _unitOfWork.UserRole.GetFirstOrDefaultAsync(ur => ur.UserId == request.UserId && ur.RoleId == request.RoleId);
            if (exists == null)
            {
                return ApiResponse<bool>.Success(true, "Role is not assigned to this user.");
            }

            _unitOfWork.UserRole.Remove(exists);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Invalidate cache + push updated permissions to this user via SignalR
            await _realtimeService.NotifyUserPermissionsChangedAsync(request.UserId, cancellationToken);

            return ApiResponse<bool>.Success(true, "Role removed from user successfully.");
        }

        private static int GetMaxRoleRank(IEnumerable<string> roleNames)
        {
            int max = 0;
            foreach (var name in roleNames)
            {
                int rank = GetRoleRank(name);
                if (rank > max) max = rank;
            }
            return max;
        }

        private static int GetRoleRank(string name)
        {
            return name.ToUpperInvariant() switch
            {
                "ADMIN" => 4,
                "HR_MANAGER" => 3,
                "TEAM_LEAD" => 2,
                "EMPLOYEE" => 1,
                _ => 0
            };
        }
    }
}
