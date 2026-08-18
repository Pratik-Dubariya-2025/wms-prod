using MediatR;
using WMS.Application.Common.Attributes;
using WMS.Application.Common.Constants;
using WMS.Application.Common.Models;
using WMS.Application.Features.Users.DTOs;
using WMS.Application.Features.Roles.DTOs;
using WMS.Domain.Interfaces;

namespace WMS.Application.Features.Users.Queries
{
    [RequirePermission(PermissionCodes.UserRead)]
    public class GetUserRolesQuery : IRequest<ApiResponse<List<RoleDto>>>
    {
        public Guid UserId { get; set; }
    }

    public class GetUserRolesQueryHandler(IUnitOfWork unitOfWork) 
        : IRequestHandler<GetUserRolesQuery, ApiResponse<List<RoleDto>>>
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;

        public async Task<ApiResponse<List<RoleDto>>> Handle(GetUserRolesQuery request, CancellationToken cancellationToken)
        {
            var user = await _unitOfWork.User.GetFirstOrDefaultAsync(u => u.Id == request.UserId && !u.IsDeleted);
            if (user == null)
            {
                return ApiResponse<List<RoleDto>>.Failure("User not found.", null, 404);
            }

            var roles = await _unitOfWork.Role.GetAllAsync(
                r => new RoleDto
                {
                    Id = r.Id,
                    Name = r.Name,
                    Code = r.Code,
                    Priority = r.Priority,
                    Description = r.Description
                },
                r => r.UserRoles.Any(ur => ur.UserId == request.UserId) && !r.IsDeleted
            );

            return ApiResponse<List<RoleDto>>.Success(roles, "User roles retrieved successfully.");
        }
    }
}
