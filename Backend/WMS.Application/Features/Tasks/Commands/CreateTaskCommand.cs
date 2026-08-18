using MediatR;
using WMS.Application.Common.Attributes;
using WMS.Application.Common.Constants;
using WMS.Application.Common.Interfaces;
using WMS.Application.Common.Models;
using WMS.Domain.Interfaces;
using WMS.Domain.Models;

namespace WMS.Application.Features.Tasks.Commands
{
    [RequirePermission(PermissionCodes.TaskCreate)]
    public class CreateTaskCommand : IRequest<ApiResponse<Guid>>
    {
        public string Title { get; set; } = null!;
        public string? Description { get; set; }
        public string Priority { get; set; } = "Medium";
        public decimal? EstimatedHours { get; set; }
        public DateTime? DueDate { get; set; }
        public Guid ProjectId { get; set; }
        public Guid? TeamId { get; set; }
        public Guid? AssigneeId { get; set; }
    }

    public class CreateTaskCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService)
        : IRequestHandler<CreateTaskCommand, ApiResponse<Guid>>
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly ICurrentUserService _currentUserService = currentUserService;

        public async Task<ApiResponse<Guid>> Handle(
            CreateTaskCommand request, CancellationToken cancellationToken)
        {
            if (!_currentUserService.UserId.HasValue)
            {
                return ApiResponse<Guid>.Failure("Unauthorized access.", null, 401);
            }

            // Validate project exists
            var project = await _unitOfWork.Project.GetFirstOrDefaultAsync(
                p => p.Id == request.ProjectId && !p.IsDeleted);
            if (project == null)
            {
                return ApiResponse<Guid>.Failure("Project not found.", null, 400);
            }

            // Row-level check: only ADMIN, the project owner, project TL, or a project
            // member can create tasks. MANAGER is not global — a manager only qualifies
            // when they own/lead this specific project.
            var currentUserId = _currentUserService.UserId.Value;
            bool isAdmin = _currentUserService.Roles.Contains("ADMIN");
            bool isProjectOwner = project.OwnerId == currentUserId;
            bool isProjectTL = project.TeamLeadId == currentUserId;

            bool isMember = false;
            if (!isAdmin && !isProjectOwner && !isProjectTL)
            {
                var membership = await _unitOfWork.ProjectMember.GetFirstOrDefaultAsync(
                    pm => pm.ProjectId == request.ProjectId && pm.UserId == currentUserId && !pm.IsDeleted);
                isMember = membership != null;
            }

            if (!isAdmin && !isProjectOwner && !isProjectTL && !isMember)
            {
                return ApiResponse<Guid>.Failure(
                    "You do not have permission to create tasks for this project. You must be a project member, team lead, or project owner.",
                    null, 403);
            }

            // Derive TeamId from Project
            if (!project.TeamId.HasValue)
            {
                return ApiResponse<Guid>.Failure(
                    "Cannot create task. This project does not have an assigned team. Please assign a Team Lead to the project first.", null, 400);
            }
            Guid teamId = project.TeamId.Value;

            // Validate assignee if provided — must be an active member of the project.
            if (request.AssigneeId.HasValue)
            {
                var isProjectMember = await _unitOfWork.ProjectMember.GetFirstOrDefaultAsync(
                    pm => pm.ProjectId == request.ProjectId && pm.UserId == request.AssigneeId.Value && !pm.IsDeleted);
                if (isProjectMember == null)
                {
                    return ApiResponse<Guid>.Failure("Assignee must be a member of the project.", null, 400);
                }

                var assignee = await _unitOfWork.User.GetFirstOrDefaultAsync(
                    u => u.Id == request.AssigneeId.Value && u.IsActive && !u.IsDeleted);
                if (assignee == null)
                {
                    return ApiResponse<Guid>.Failure("Assignee not found or inactive.", null, 400);
                }
            }

            // Validate priority
            string[] validPriorities = ["Low", "Medium", "High", "Critical"];
            if (!validPriorities.Contains(request.Priority))
            {
                return ApiResponse<Guid>.Failure(
                    $"Invalid priority. Must be one of: {string.Join(", ", validPriorities)}", null, 400);
            }

            TaskItem task = new()
            {
                Title = request.Title,
                Description = request.Description,
                Status = "Draft",
                Priority = request.Priority,
                EstimatedHours = request.EstimatedHours,
                DueDate = request.DueDate,
                ProjectId = request.ProjectId,
                TeamId = teamId,
                AssigneeId = request.AssigneeId,
                CreatedById = _currentUserService.UserId.Value,
                CreatedBy = _currentUserService.Username ?? "System"
            };

            await _unitOfWork.TaskItem.AddAsync(task);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return ApiResponse<Guid>.Success(task.Id, "Task created successfully.", 201);
        }
    }
}
