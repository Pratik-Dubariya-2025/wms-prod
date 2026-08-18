using MediatR;
using WMS.Application.Common.Attributes;
using WMS.Application.Common.Constants;
using WMS.Application.Common.Interfaces;
using WMS.Application.Common.Models;
using WMS.Domain.Interfaces;

namespace WMS.Application.Features.Tasks.Commands
{
    [RequirePermission(PermissionCodes.TaskUpdate)]
    public class UpdateTaskStatusCommand : IRequest<ApiResponse<bool>>
    {
        public Guid Id { get; set; }
        public string Status { get; set; } = null!;
        public Guid ProjectId { get; set; }
    }

    public class UpdateTaskStatusCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService)
        : IRequestHandler<UpdateTaskStatusCommand, ApiResponse<bool>>
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly ICurrentUserService _currentUserService = currentUserService;

        /// <summary>
        /// Valid task status transitions (SRS 3.4.1):
        ///   Draft → InProgress
        ///   InProgress → InReview
        ///   InReview → Done (TL or above only) | InProgress (reject back)
        ///   Done → Closed (TL or above only)
        /// </summary>
        private static readonly Dictionary<string, string[]> AllowedTransitions = new()
        {
            { "Draft", new[] { "InProgress" } },
            { "InProgress", new[] { "InReview" } },
            { "InReview", new[] { "Done", "InProgress" } },
            { "Done", new[] { "Closed" } },
            { "Closed", Array.Empty<string>() }
        };

        /// <summary>
        /// Statuses that require elevated roles (TL, Manager, Admin) to transition TO.
        /// Per SRS: "SSE can move a task to InReview; only TL or above can move it to Done."
        /// </summary>
        private static readonly HashSet<string> ElevatedTargetStatuses = new() { "Done", "Closed" };

        public async Task<ApiResponse<bool>> Handle(
            UpdateTaskStatusCommand request, CancellationToken cancellationToken)
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

            if (task.ProjectId != request.ProjectId)
            {
                return ApiResponse<bool>.Failure("Task does not belong to the specified project.", null, 400);
            }

            // Validate the target status value
            string[] validStatuses = ["Draft", "InProgress", "InReview", "Done", "Closed"];
            if (!validStatuses.Contains(request.Status))
            {
                return ApiResponse<bool>.Failure(
                    $"Invalid status. Must be one of: {string.Join(", ", validStatuses)}", null, 400);
            }

            // Validate the transition is allowed from current status
            if (!AllowedTransitions.TryGetValue(task.Status, out var allowed) ||
                !allowed.Contains(request.Status))
            {
                return ApiResponse<bool>.Failure(
                    $"Invalid status transition from '{task.Status}' to '{request.Status}'.",
                    null, 400);
            }

            // Role-level check: Only TL, Manager, or Admin can transition to Done or Closed
            if (ElevatedTargetStatuses.Contains(request.Status))
            {
                bool hasElevatedRole =
                    _currentUserService.Roles.Contains("ADMIN") ||
                    _currentUserService.Roles.Contains("MANAGER") ||
                    _currentUserService.Roles.Contains("TL");

                if (!hasElevatedRole)
                {
                    // Also allow if the user is the project's Team Lead (by assignment, not role name)
                    var project = await _unitOfWork.Project.GetFirstOrDefaultAsync(
                        p => p.Id == task.ProjectId && !p.IsDeleted);
                    bool isProjectTL = project != null && project.TeamLeadId == currentUserId;

                    if (!isProjectTL)
                    {
                        return ApiResponse<bool>.Failure(
                            $"Only a Team Lead or above can move a task to '{request.Status}'.",
                            null, 403);
                    }
                }
            }

            // Row-level check: user must be related to the task (assignee, creator,
            // project owner/TL) or ADMIN. A MANAGER is not global — they only qualify via
            // ownership/TL of this specific project (checked below).
            bool isRelated =
                task.AssigneeId == currentUserId ||
                task.CreatedById == currentUserId ||
                _currentUserService.Roles.Contains("ADMIN");

            if (!isRelated)
            {
                var project = await _unitOfWork.Project.GetFirstOrDefaultAsync(
                    p => p.Id == task.ProjectId && !p.IsDeleted);
                isRelated = project != null &&
                    (project.OwnerId == currentUserId || project.TeamLeadId == currentUserId);
            }

            if (!isRelated)
            {
                return ApiResponse<bool>.Failure(
                    "You are not authorized to change the status of this task.", null, 403);
            }

            task.Status = request.Status;
            task.ModifiedBy = _currentUserService.Username ?? "System";
            task.ModifiedAt = DateTime.UtcNow;

            _unitOfWork.TaskItem.Update(task);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return ApiResponse<bool>.Success(true, "Task status updated successfully.");
        }
    }
}
