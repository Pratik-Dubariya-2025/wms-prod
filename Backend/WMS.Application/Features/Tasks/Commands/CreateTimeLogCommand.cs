using MediatR;
using WMS.Application.Common.Attributes;
using WMS.Application.Common.Constants;
using WMS.Application.Common.Interfaces;
using WMS.Application.Common.Models;
using WMS.Domain.Interfaces;
using WMS.Domain.Models;

namespace WMS.Application.Features.Tasks.Commands
{
    /// <summary>
    /// Log time against a task. Per SRS 7.4 (timelogs:write:self),
    /// a user can only log time for themselves.
    /// </summary>
    [RequirePermission(PermissionCodes.TimeLogCreate)]
    public class CreateTimeLogCommand : IRequest<ApiResponse<Guid>>
    {
        public Guid TaskId { get; set; }
        public decimal LoggedHours { get; set; }
        public DateTime LogDate { get; set; }
        public string? Notes { get; set; }
    }

    public class CreateTimeLogCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService)
        : IRequestHandler<CreateTimeLogCommand, ApiResponse<Guid>>
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly ICurrentUserService _currentUserService = currentUserService;

        public async Task<ApiResponse<Guid>> Handle(
            CreateTimeLogCommand request, CancellationToken cancellationToken)
        {
            if (!_currentUserService.UserId.HasValue)
            {
                return ApiResponse<Guid>.Failure("Unauthorized access.", null, 401);
            }

            var currentUserId = _currentUserService.UserId.Value;

            // Validate task exists
            var task = await _unitOfWork.TaskItem.GetFirstOrDefaultAsync(
                t => t.Id == request.TaskId && !t.IsDeleted);
            if (task == null)
            {
                return ApiResponse<Guid>.Failure("Task not found.", null, 404);
            }

            // Row-level: user can only log time if they are the task assignee,
            // the task creator, or a project member
            bool isAssignee = task.AssigneeId == currentUserId;
            bool isCreator = task.CreatedById == currentUserId;
            bool isProjectMember = false;

            if (!isAssignee && !isCreator)
            {
                var membership = await _unitOfWork.ProjectMember.GetFirstOrDefaultAsync(
                    pm => pm.ProjectId == task.ProjectId && pm.UserId == currentUserId && !pm.IsDeleted);
                isProjectMember = membership != null;
            }

            if (!isAssignee && !isCreator && !isProjectMember)
            {
                return ApiResponse<Guid>.Failure(
                    "You can only log time on tasks you are assigned to or are a member of the project.", null, 403);
            }

            // Validate logged hours
            if (request.LoggedHours <= 0 || request.LoggedHours > 24)
            {
                return ApiResponse<Guid>.Failure(
                    "Logged hours must be between 0.01 and 24.", null, 400);
            }

            // Validate log date is not in the future
            if (request.LogDate.Date > DateTime.UtcNow.Date)
            {
                return ApiResponse<Guid>.Failure(
                    "Cannot log time for a future date.", null, 400);
            }

            // Validate log date is not before task creation date
            if (request.LogDate.Date < task.CreatedAt.Date)
            {
                return ApiResponse<Guid>.Failure(
                    $"Cannot log time for a date before the task was created ({task.CreatedAt:yyyy-MM-dd}).", null, 400);
            }

            // Task must be in a workable status (not Draft or Closed)
            string[] workableStatuses = ["InProgress", "InReview", "Done"];
            if (!workableStatuses.Contains(task.Status))
            {
                return ApiResponse<Guid>.Failure(
                    $"Cannot log time on a task with status '{task.Status}'. Task must be InProgress, InReview, or Done.",
                    null, 400);
            }

            TimeLog timeLog = new()
            {
                TaskId = request.TaskId,
                UserId = currentUserId,
                LoggedHours = request.LoggedHours,
                LogDate = request.LogDate.Date,
                Notes = request.Notes,
                IsApproved = false,
                CreatedBy = _currentUserService.Username ?? "System"
            };

            await _unitOfWork.TimeLog.AddAsync(timeLog);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return ApiResponse<Guid>.Success(timeLog.Id, "Time logged successfully.", 201);
        }
    }
}
