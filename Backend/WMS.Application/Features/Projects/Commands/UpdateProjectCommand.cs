using MediatR;
using WMS.Application.Common.Attributes;
using WMS.Application.Common.Constants;
using WMS.Application.Common.Interfaces;
using WMS.Application.Common.Models;
using WMS.Domain.Interfaces;
using WMS.Domain.Models;

namespace WMS.Application.Features.Projects.Commands
{
    [RequirePermission(PermissionCodes.ProjectUpdate)]
    public class UpdateProjectCommand : IRequest<ApiResponse<bool>>
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public string Status { get; set; } = "Planning";
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public Guid DepartmentId { get; set; }
        public Guid? TeamLeadId { get; set; }
    }

    public class UpdateProjectCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
        : IRequestHandler<UpdateProjectCommand, ApiResponse<bool>>
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly ICurrentUserService _currentUserService = currentUserService;

        public async Task<ApiResponse<bool>> Handle(
            UpdateProjectCommand request, CancellationToken cancellationToken)
        {
            if (!_currentUserService.UserId.HasValue)
            {
                return ApiResponse<bool>.Failure("Unauthorized access.", null, 401);
            }

            var project = await _unitOfWork.Project.GetFirstOrDefaultAsync(
                p => p.Id == request.Id && !p.IsDeleted);
            if (project == null)
            {
                return ApiResponse<bool>.Failure("Project not found.", null, 404);
            }

            // Row-level check: only ADMIN or the project Owner/Team Lead may edit it.
            if (!ProjectAccessAuthorizer.CanManage(project, _currentUserService))
            {
                return ApiResponse<bool>.Failure(
                    "You do not have permission to edit this project.", null, 403);
            }

            // Validate department
            var department = await _unitOfWork.Department.GetFirstOrDefaultAsync(
                d => d.Id == request.DepartmentId && !d.IsDeleted);
            if (department == null)
            {
                return ApiResponse<bool>.Failure("Department not found.", null, 400);
            }

            // Validate status
            string[] validStatuses = ["Planning", "Active", "OnHold", "Completed", "Cancelled"];
            if (!validStatuses.Contains(request.Status))
            {
                return ApiResponse<bool>.Failure(
                    $"Invalid status. Must be one of: {string.Join(", ", validStatuses)}", null, 400);
            }

            // Validate team lead if provided
            Guid? projectTeamId = null;
            if (request.TeamLeadId.HasValue)
            {
                var teamLead = await _unitOfWork.User.GetFirstOrDefaultAsync(
                    u => u.Id == request.TeamLeadId.Value && u.IsActive && !u.IsDeleted);
                if (teamLead == null)
                {
                    return ApiResponse<bool>.Failure("Team Lead not found or inactive.", null, 400);
                }
                if (teamLead.DepartmentId != request.DepartmentId)
                {
                    return ApiResponse<bool>.Failure("Team Lead must belong to the selected department.", null, 400);
                }
                if (!teamLead.TeamId.HasValue)
                {
                    return ApiResponse<bool>.Failure("Selected user is not a Team Lead with a team.", null, 400);
                }
                projectTeamId = teamLead.TeamId;
            }

            var oldTeamLeadId = project.TeamLeadId;

            project.Name = request.Name;
            project.Description = request.Description;
            project.Status = request.Status;
            project.StartDate = request.StartDate;
            project.EndDate = request.EndDate;
            project.DepartmentId = request.DepartmentId;
            project.TeamLeadId = request.TeamLeadId;
            project.TeamId = projectTeamId;
            project.ModifiedBy = _currentUserService.Username ?? "System";
            project.ModifiedAt = DateTime.UtcNow;

            _unitOfWork.Project.Update(project);

            // If TL is assigned and changed, ensure they are a member
            if (request.TeamLeadId.HasValue && request.TeamLeadId != oldTeamLeadId)
            {
                var tlMember = await _unitOfWork.ProjectMember.GetFirstOrDefaultAsync(
                    pm => pm.ProjectId == project.Id && pm.UserId == request.TeamLeadId.Value && !pm.IsDeleted);
                if (tlMember == null)
                {
                    tlMember = new ProjectMember
                    {
                        ProjectId = project.Id,
                        UserId = request.TeamLeadId.Value,
                        CreatedBy = _currentUserService.Username ?? "System",
                    };
                    await _unitOfWork.ProjectMember.AddAsync(tlMember);
                }
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return ApiResponse<bool>.Success(true, "Project updated successfully.");
        }
    }
}
