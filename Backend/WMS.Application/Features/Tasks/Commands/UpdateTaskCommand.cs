using MediatR;
using WMS.Application.Common.Attributes;
using WMS.Application.Common.Constants;
using WMS.Application.Common.Interfaces;
using WMS.Application.Common.Models;
using WMS.Domain.Interfaces;
using WMS.Domain.Models;

namespace WMS.Application.Features.Tasks.Commands
{
    [RequirePermission(PermissionCodes.TaskUpdate)]
    public class UpdateTaskCommand : IRequest<ApiResponse<bool>>
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = null!;
        public string? Description { get; set; }
        public string Status { get; set; } = "Draft";
        public string Priority { get; set; } = "Medium";
        public decimal? EstimatedHours { get; set; }
        public DateTime? DueDate { get; set; }
        public Guid ProjectId { get; set; }
        public Guid? TeamId { get; set; }
        public Guid? AssigneeId { get; set; }
    }

    public class UpdateTaskCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService)
        : IRequestHandler<UpdateTaskCommand, ApiResponse<bool>>
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly ICurrentUserService _currentUserService = currentUserService;

        public async Task<ApiResponse<bool>> Handle(
            UpdateTaskCommand request, CancellationToken cancellationToken)
        {
            if (!_currentUserService.UserId.HasValue)
            {
                return ApiResponse<bool>.Failure("Unauthorized access.", null, 401);
            }

            var currentUserId = _currentUserService.UserId.Value;

            var task = await _unitOfWork.TaskItem.GetFirstOrDefaultAsync(
                t => t.Id == request.Id && !t.IsDeleted);
            if (task == null)
            {
                return ApiResponse<bool>.Failure("Task not found.", null, 404);
            }

            // Prevent editing if task is InReview or above
            string[] lockedStatuses = ["InReview", "Done", "Closed"];
            if (lockedStatuses.Contains(task.Status))
            {
                return ApiResponse<bool>.Failure(
                    $"Cannot edit task because it is in '{task.Status}' status. Tasks in InReview, Done, or Closed status cannot be modified.",
                    null, 400);
            }

            // Validate project exists
            var project = await _unitOfWork.Project.GetFirstOrDefaultAsync(
                p => p.Id == request.ProjectId && !p.IsDeleted);
            if (project == null)
            {
                return ApiResponse<bool>.Failure("Project not found.", null, 400);
            }

            // Validate status transitions and permissions if status is changing
            if (request.Status != task.Status)
            {
                var allowedTransitions = new Dictionary<string, string[]>
                {
                    { "Draft", new[] { "InProgress" } },
                    { "InProgress", new[] { "InReview" } },
                    { "InReview", new[] { "Done", "InProgress" } },
                    { "Done", new[] { "Closed" } },
                    { "Closed", Array.Empty<string>() }
                };

                if (!allowedTransitions.TryGetValue(task.Status, out var allowed) ||
                    !allowed.Contains(request.Status))
                {
                    return ApiResponse<bool>.Failure(
                        $"Invalid status transition from '{task.Status}' to '{request.Status}'.",
                        null, 400);
                }

                var elevatedTargetStatuses = new HashSet<string> { "Done", "Closed" };
                if (elevatedTargetStatuses.Contains(request.Status))
                {
                    bool hasElevatedRole =
                        _currentUserService.Roles.Contains("ADMIN") ||
                        _currentUserService.Roles.Contains("MANAGER") ||
                        _currentUserService.Roles.Contains("TL");

                    if (!hasElevatedRole)
                    {
                        if (project.TeamLeadId != currentUserId)
                        {
                            return ApiResponse<bool>.Failure(
                                $"Only a Team Lead or above can move a task to '{request.Status}'.",
                                null, 403);
                        }
                    }
                }
            }

            // Row-level check: only ADMIN, the task creator, assignee, project owner, or
            // project TL can edit. A MANAGER is not global here — they only qualify via
            // ownership/TL of this specific project.
            bool isAdmin = _currentUserService.Roles.Contains("ADMIN");
            bool isCreator = task.CreatedById == currentUserId;
            bool isAssignee = task.AssigneeId == currentUserId;
            bool isProjectOwner = project.OwnerId == currentUserId;
            bool isProjectTL = project.TeamLeadId == currentUserId;

            if (!isAdmin && !isCreator && !isAssignee && !isProjectOwner && !isProjectTL)
            {
                return ApiResponse<bool>.Failure(
                    "You do not have permission to edit this task. Only the task creator, assignee, team lead, or project owner can edit task details.",
                    null, 403);
            }

            // Derive TeamId from Project
            if (!project.TeamId.HasValue)
            {
                return ApiResponse<bool>.Failure(
                    "Cannot update task. This project does not have an assigned team. Please assign a Team Lead to the project first.", null, 400);
            }
            Guid teamId = project.TeamId.Value;

            // Validate assignee if provided
            if (request.AssigneeId.HasValue)
            {
                var isProjectMember = await _unitOfWork.ProjectMember.GetFirstOrDefaultAsync(
                    pm => pm.ProjectId == request.ProjectId && pm.UserId == request.AssigneeId.Value && !pm.IsDeleted);
                if (isProjectMember == null)
                {
                    return ApiResponse<bool>.Failure("Assignee must be a member of the project.", null, 400);
                }

                var assignee = await _unitOfWork.User.GetFirstOrDefaultAsync(
                    u => u.Id == request.AssigneeId.Value && u.IsActive && !u.IsDeleted);
                if (assignee == null)
                {
                    return ApiResponse<bool>.Failure("Assignee not found or inactive.", null, 400);
                }
            }

            // Validate status
            string[] validStatuses = ["Draft", "InProgress", "InReview", "Done", "Closed"];
            if (!validStatuses.Contains(request.Status))
            {
                return ApiResponse<bool>.Failure(
                    $"Invalid status. Must be one of: {string.Join(", ", validStatuses)}", null, 400);
            }

            // Validate priority
            string[] validPriorities = ["Low", "Medium", "High", "Critical"];
            if (!validPriorities.Contains(request.Priority))
            {
                return ApiResponse<bool>.Failure(
                    $"Invalid priority. Must be one of: {string.Join(", ", validPriorities)}", null, 400);
            }

            task.Title = request.Title;
            task.Description = request.Description;
            task.Status = request.Status;
            task.Priority = request.Priority;
            task.EstimatedHours = request.EstimatedHours;
            task.DueDate = request.DueDate;
            task.ProjectId = request.ProjectId;
            task.TeamId = teamId;
            task.AssigneeId = request.AssigneeId;
            task.ModifiedBy = _currentUserService.Username ?? "System";
            task.ModifiedAt = DateTime.UtcNow;

            _unitOfWork.TaskItem.Update(task);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return ApiResponse<bool>.Success(true, "Task updated successfully.");
        }
    }
}
