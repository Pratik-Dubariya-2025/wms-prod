using MediatR;
using WMS.Application.Common.Attributes;
using WMS.Application.Common.Constants;
using WMS.Application.Common.Interfaces;
using WMS.Application.Common.Models;
using WMS.Application.Features.Projects.DTOs;
using WMS.Domain.Interfaces;

namespace WMS.Application.Features.Projects.Queries
{
    /// <summary>
    /// Returns the metadata needed to create a project: the current user's department
    /// (projects are always created within it) and the Tech Leads in that department
    /// who have a team and can lead the project.
    /// Visible to anyone who can create, update, or delete projects.
    /// </summary>
    [RequirePermission(
        PermissionCodes.ProjectCreate,
        PermissionCodes.ProjectUpdate,
        PermissionCodes.ProjectDelete,
        RequireAny = true)]
    public class GetProjectCreateMetaQuery : IRequest<ApiResponse<ProjectCreateMetaDto>>
    {
    }

    public class GetProjectCreateMetaQueryHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService)
        : IRequestHandler<GetProjectCreateMetaQuery, ApiResponse<ProjectCreateMetaDto>>
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly ICurrentUserService _currentUserService = currentUserService;

        public async Task<ApiResponse<ProjectCreateMetaDto>> Handle(
            GetProjectCreateMetaQuery request, CancellationToken cancellationToken)
        {
            if (!_currentUserService.UserId.HasValue)
            {
                return ApiResponse<ProjectCreateMetaDto>.Failure("Unauthorized access.", null, 401);
            }

            var currentUserId = _currentUserService.UserId.Value;

            var user = await _unitOfWork.User.GetWithRolesAndPermissionsAsync(currentUserId, cancellationToken);
            if (user == null || user.IsDeleted)
            {
                return ApiResponse<ProjectCreateMetaDto>.Failure("Current user not found.", null, 404);
            }

            var isAdmin = user.UserRoles.Any(ur => ur.Role.Code == "ADMIN" && !ur.Role.IsDeleted);

            // Tech Leads in the same department who already have a team.
            var teamLeads = await _unitOfWork.User.GetAllAsync(
                select: u => new TeamLeadLookup
                {
                    Id = u.Id,
                    FullName = u.FirstName + " " + u.LastName,
                    TeamId = u.TeamId,
                    TeamName = u.Team != null ? u.Team.Name : null,
                },
                where: u => u.DepartmentId == user.DepartmentId
                            && u.IsActive
                            && !u.IsDeleted
                            && u.TeamId != null
                            && u.Designation.Name == "Tech Lead"
                            && (isAdmin || u.ManagerId == currentUserId));

            var meta = new ProjectCreateMetaDto
            {
                DepartmentId = user.DepartmentId,
                DepartmentName = user.Department != null ? user.Department.Name : "Unknown",
                TeamLeads = teamLeads
            };

            return ApiResponse<ProjectCreateMetaDto>.Success(meta);
        }
    }
}
