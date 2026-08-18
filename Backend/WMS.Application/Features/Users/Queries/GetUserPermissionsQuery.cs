using MediatR;
using WMS.Application.Common.Attributes;
using WMS.Application.Common.Constants;
using WMS.Application.Common.Models;
using WMS.Domain.Interfaces;

namespace WMS.Application.Features.Users.Queries
{
    [RequirePermission(PermissionCodes.UserRead)]
    public class GetUserPermissionsQuery : IRequest<ApiResponse<List<string>>>
    {
        public Guid UserId { get; set; }
    }

    public class GetUserPermissionsQueryHandler(IUnitOfWork unitOfWork) 
        : IRequestHandler<GetUserPermissionsQuery, ApiResponse<List<string>>>
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;

        public async Task<ApiResponse<List<string>>> Handle(GetUserPermissionsQuery request, CancellationToken cancellationToken)
        {
            var user = await _unitOfWork.User.GetFirstOrDefaultAsync(u => u.Id == request.UserId && !u.IsDeleted);
            if (user == null)
            {
                return ApiResponse<List<string>>.Failure("User not found.", null, 404);
            }

            List<string> rolePermissions = await _unitOfWork.RolePermission.GetAllAsync(
                s => s.Permission.Code,
                s => !s.Role.IsDeleted && s.Role.UserRoles.Any(ur => ur.UserId == request.UserId));

            var overrides = await _unitOfWork.UserPermissionOverride.IncludeAndGetAllAsync(
                o => o.UserId == request.UserId && !o.IsDeleted && (o.ExpiresAt == null || o.ExpiresAt > DateTime.UtcNow),
                o => o.Permission
            );

            var deniedPermissions = overrides
                .Where(o => o.Permission != null && !o.IsGranted)
                .Select(o => o.Permission.Code)
                .ToList();

            var grantedPermissions = overrides
                .Where(o => o.Permission != null && o.IsGranted)
                .Select(o => o.Permission.Code)
                .ToList();

            List<string> permissions = rolePermissions
                .Except(deniedPermissions, StringComparer.OrdinalIgnoreCase)
                .Union(grantedPermissions, StringComparer.OrdinalIgnoreCase)
                .ToList();

            return ApiResponse<List<string>>.Success(permissions, "Permissions retrieved successfully.");
        }
    }
}
