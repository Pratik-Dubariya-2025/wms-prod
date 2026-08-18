using MediatR;
using WMS.Application.Common.Attributes;
using WMS.Application.Common.Constants;
using WMS.Application.Common.Interfaces;
using WMS.Application.Common.Models;
using WMS.Domain.Interfaces;
using WMS.Domain.Models;

namespace WMS.Application.Features.Projects.Commands
{
    [RequirePermission(PermissionCodes.ProjectCreate)]
    public class CreateProjectCommand : IRequest<ApiResponse<Guid>>
    {
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public string Status { get; set; } = "Planning";
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        // Department is derived from the creator — any value sent here is ignored.
        public Guid? TeamLeadId { get; set; }
    }

    public class CreateProjectCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
        : IRequestHandler<CreateProjectCommand, ApiResponse<Guid>>
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly ICurrentUserService _currentUserService = currentUserService;

        public async Task<ApiResponse<Guid>> Handle(
            CreateProjectCommand request, CancellationToken cancellationToken)
        {
            if (!_currentUserService.UserId.HasValue)
            {
                return ApiResponse<Guid>.Failure("Unauthorized access.", null, 401);
            }

            var currentUserId = _currentUserService.UserId.Value;

            // The project is always created in the creator's department.
            var currentUser = await _unitOfWork.User.GetFirstOrDefaultAsync(
                u => u.Id == currentUserId && !u.IsDeleted);
            if (currentUser == null)
            {
                return ApiResponse<Guid>.Failure("Current user not found.", null, 404);
            }

            // Validate status
            string[] validStatuses = ["Planning", "Active", "OnHold", "Completed", "Cancelled"];
            if (!validStatuses.Contains(request.Status))
            {
                return ApiResponse<Guid>.Failure(
                    $"Invalid status. Must be one of: {string.Join(", ", validStatuses)}", null, 400);
            }

            // Validate the selected Team Lead and derive the project's team from them.
            Guid? projectTeamId = null;
            if (request.TeamLeadId.HasValue)
            {
                var teamLead = await _unitOfWork.User.GetFirstOrDefaultAsync(
                    u => u.Id == request.TeamLeadId.Value && u.IsActive && !u.IsDeleted);
                if (teamLead == null)
                {
                    return ApiResponse<Guid>.Failure("Team Lead not found or inactive.", null, 400);
                }
                if (teamLead.DepartmentId != currentUser.DepartmentId)
                {
                    return ApiResponse<Guid>.Failure("Team Lead must belong to your department.", null, 400);
                }
                if (!teamLead.TeamId.HasValue)
                {
                    return ApiResponse<Guid>.Failure("Selected user is not a Team Lead with a team.", null, 400);
                }

                // The selected user must actually lead the team we are attaching —
                // guards against attaching a project via an ordinary team member.
                var ledTeam = await _unitOfWork.Team.GetFirstOrDefaultAsync(
                    t => t.Id == teamLead.TeamId.Value && t.TeamLeadId == teamLead.Id && !t.IsDeleted);
                if (ledTeam == null)
                {
                    return ApiResponse<Guid>.Failure("Selected user does not lead their team.", null, 400);
                }
                projectTeamId = teamLead.TeamId;
            }

            var project = new Project
            {
                Name = request.Name,
                Description = request.Description,
                Status = request.Status,
                StartDate = request.StartDate,
                EndDate = request.EndDate,
                DepartmentId = currentUser.DepartmentId,
                OwnerId = currentUserId,
                TeamLeadId = request.TeamLeadId,
                TeamId = projectTeamId,
                CreatedBy = _currentUserService.Username ?? "System",
            };

            await _unitOfWork.Project.AddAsync(project);

            // If TL is assigned, also add them as a project member
            if (request.TeamLeadId.HasValue)
            {
                var tlMember = new ProjectMember
                {
                    ProjectId = project.Id,
                    UserId = request.TeamLeadId.Value,
                    CreatedBy = _currentUserService.Username ?? "System",
                };
                await _unitOfWork.ProjectMember.AddAsync(tlMember);
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return ApiResponse<Guid>.Success(project.Id, "Project created successfully.");
        }
    }
}
