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
    public class GetProjectByIdQuery : IRequest<ApiResponse<ProjectDetailDto>>
    {
        public Guid Id { get; set; }
    }

    public class GetProjectByIdQueryHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
        : IRequestHandler<GetProjectByIdQuery, ApiResponse<ProjectDetailDto>>
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly ICurrentUserService _currentUserService = currentUserService;

        public async Task<ApiResponse<ProjectDetailDto>> Handle(
            GetProjectByIdQuery request, CancellationToken cancellationToken)
        {
            if (!_currentUserService.UserId.HasValue)
            {
                return ApiResponse<ProjectDetailDto>.Failure("Unauthorized access.", null, 401);
            }

            var currentUserId = _currentUserService.UserId.Value;
            var currentUserRoles = _currentUserService.Roles;

            // Fetch the project with a select projection
            var project = await _unitOfWork.Project.GetFirstOrDefaultAsync(
                select: p => new ProjectDetailDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Description = p.Description,
                    Status = p.Status,
                    StartDate = p.StartDate,
                    EndDate = p.EndDate,
                    DepartmentId = p.DepartmentId,
                    DepartmentName = p.Department.Name,
                    OwnerId = p.OwnerId,
                    OwnerName = p.Owner.FirstName + " " + p.Owner.LastName,
                    TeamLeadId = p.TeamLeadId,
                    TeamLeadName = p.TeamLead != null
                        ? p.TeamLead.FirstName + " " + p.TeamLead.LastName
                        : null,
                    TeamId = p.TeamId,
                    MemberCount = p.Members.Count(m => !m.IsDeleted),
                    TaskSummary = new TaskStatusSummary
                    {
                        Total = p.Tasks.Count(t => !t.IsDeleted),
                        Draft = p.Tasks.Count(t => !t.IsDeleted && t.Status == "Draft"),
                        InProgress = p.Tasks.Count(t => !t.IsDeleted && t.Status == "InProgress"),
                        InReview = p.Tasks.Count(t => !t.IsDeleted && t.Status == "InReview"),
                        Done = p.Tasks.Count(t => !t.IsDeleted && t.Status == "Done"),
                        Closed = p.Tasks.Count(t => !t.IsDeleted && t.Status == "Closed"),
                    },
                    CreatedAt = p.CreatedAt,
                    CreatedBy = p.CreatedBy,
                    ModifiedAt = p.ModifiedAt,
                    ModifiedBy = p.ModifiedBy,
                },
                where: p => p.Id == request.Id && !p.IsDeleted
            );

            if (project == null)
            {
                return ApiResponse<ProjectDetailDto>.Failure("Project not found.", null, 404);
            }

            // RLS: only ADMIN sees any project. Everyone else — including MANAGER —
            // must be the owner, TL, or a member of this specific project.
            if (!currentUserRoles.Contains("ADMIN"))
            {
                bool hasAccess = project.OwnerId == currentUserId ||
                                 project.TeamLeadId == currentUserId;

                if (!hasAccess)
                {
                    // Check membership
                    var isMember = await _unitOfWork.ProjectMember.GetFirstOrDefaultAsync(
                        pm => pm.ProjectId == request.Id && pm.UserId == currentUserId && !pm.IsDeleted);
                    if (isMember == null)
                    {
                        return ApiResponse<ProjectDetailDto>.Failure("You are not authorized to view this project.", null, 403);
                    }
                }
            }

            return ApiResponse<ProjectDetailDto>.Success(project);
        }
    }
}
