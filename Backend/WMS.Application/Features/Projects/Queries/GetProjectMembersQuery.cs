using MediatR;
using WMS.Application.Common.Attributes;
using WMS.Application.Common.Constants;
using WMS.Application.Common.Interfaces;
using WMS.Application.Common.Models;
using WMS.Application.Features.Projects.DTOs;
using WMS.Domain.Interfaces;

namespace WMS.Application.Features.Projects.Queries
{
    [RequirePermission(PermissionCodes.ProjectRead)]
    public class GetProjectMembersQuery : IRequest<ApiResponse<List<ProjectMemberDto>>>
    {
        public Guid ProjectId { get; set; }
    }

    public class GetProjectMembersQueryHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
        : IRequestHandler<GetProjectMembersQuery, ApiResponse<List<ProjectMemberDto>>>
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly ICurrentUserService _currentUserService = currentUserService;

        public async Task<ApiResponse<List<ProjectMemberDto>>> Handle(
            GetProjectMembersQuery request, CancellationToken cancellationToken)
        {
            // Verify project exists
            var project = await _unitOfWork.Project.GetFirstOrDefaultAsync(
                p => p.Id == request.ProjectId && !p.IsDeleted);
            if (project == null)
            {
                return ApiResponse<List<ProjectMemberDto>>.Failure("Project not found.", null, 404);
            }

            // Row-level access: only ADMIN, the owner/TL, or a member may view the roster.
            if (!await ProjectAccessAuthorizer.CanAccessAsync(project, _unitOfWork, _currentUserService))
            {
                return ApiResponse<List<ProjectMemberDto>>.Failure(
                    "You are not authorized to view this project's members.", null, 403);
            }

            var members = await _unitOfWork.ProjectMember.GetAllAsync(
                select: pm => new ProjectMemberDto
                {
                    Id = pm.Id,
                    UserId = pm.UserId,
                    FullName = pm.User.FirstName + " " + pm.User.LastName,
                    Email = pm.User.Email,
                    DesignationName = pm.User.Designation.Name,
                    JoinedAt = pm.CreatedAt,
                },
                where: pm => pm.ProjectId == request.ProjectId && !pm.IsDeleted
            );

            return ApiResponse<List<ProjectMemberDto>>.Success(members);
        }
    }
}
