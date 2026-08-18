using MediatR;
using WMS.Application.Common.Constants;
using WMS.Application.Common.Interfaces;
using WMS.Application.Common.Models;
using WMS.Application.Features.Users.DTOs;
using WMS.Domain.Interfaces;
using WMS.Domain.Models;

namespace WMS.Application.Features.Users.Queries
{
    public class GetUserByIdQuery : IRequest<ApiResponse<UserDetailDto>>
    {
        public Guid Id { get; set; }
    }

    public class GetUserByIdQueryHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService)
        : IRequestHandler<GetUserByIdQuery, ApiResponse<UserDetailDto>>
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly ICurrentUserService _currentUserService = currentUserService;

        public async Task<ApiResponse<UserDetailDto>> Handle(
            GetUserByIdQuery request, CancellationToken cancellationToken)
        {
            // Allow users to view their own profile; require permission for others
            bool isSelf = _currentUserService.UserId == request.Id;
            if (!isSelf && !_currentUserService.HasPermission(PermissionCodes.UserRead))
            {
                return ApiResponse<UserDetailDto>
                    .Failure("You do not have permission to view user details.", statusCode: 403);
            }

            // Fetch user with full navigation graph
            User? user = await _unitOfWork.User
                .GetWithRolesAndPermissionsAsync(request.Id, cancellationToken);

            if (user == null)
            {
                return ApiResponse<UserDetailDto>
                    .Failure("User not found.", statusCode: 404);
            }

            UserDetailDto dto = new()
            {
                Id = user.Id,
                EmployeeCode = user.EmployeeCode,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                Username = user.Username,
                PhoneNumber = user.PhoneNumber,
                IsActive = user.IsActive,
                DepartmentId = user.DepartmentId,
                DepartmentName = user.Department?.Name ?? string.Empty,
                DesignationId = user.DesignationId,
                DesignationName = user.Designation?.Name ?? string.Empty,
                Roles = user.UserRoles?.Select(ur => new UserRoleDto
                {
                    RoleId = ur.RoleId,
                    RoleName = ur.Role?.Name ?? string.Empty
                }).ToList() ?? [],
                CreatedAt = user.CreatedAt,
                CreatedBy = user.CreatedBy,
                ModifiedAt = user.ModifiedAt
            };

            return ApiResponse<UserDetailDto>.Success(dto);
        }
    }
}
