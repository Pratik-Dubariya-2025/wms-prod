using MediatR;
using WMS.Application.Common.Attributes;
using WMS.Application.Common.Constants;
using WMS.Application.Common.Interfaces;
using WMS.Application.Common.Models;
using WMS.Domain.Interfaces;

namespace WMS.Application.Features.Tasks.Commands
{
    /// <summary>
    /// Approve a time log entry. Per SRS 7.4 (timelogs:approve),
    /// only TL, Manager, or Admin can approve time logs.
    /// Row-level: the approver must be the project's TL/Owner, or a Manager/Admin.
    /// </summary>
    [RequirePermission(PermissionCodes.TimeLogApprove)]
    public class ApproveTimeLogCommand : IRequest<ApiResponse<bool>>
    {
        public Guid TaskId { get; set; }
        public Guid TimeLogId { get; set; }
    }

    public class ApproveTimeLogCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService)
        : IRequestHandler<ApproveTimeLogCommand, ApiResponse<bool>>
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly ICurrentUserService _currentUserService = currentUserService;

        public async Task<ApiResponse<bool>> Handle(
            ApproveTimeLogCommand request, CancellationToken cancellationToken)
        {
            if (!_currentUserService.UserId.HasValue)
            {
                return ApiResponse<bool>.Failure("Unauthorized access.", null, 401);
            }

            var currentUserId = _currentUserService.UserId.Value;

            // Validate task exists
            var task = await _unitOfWork.TaskItem.GetFirstOrDefaultAsync(
                t => t.Id == request.TaskId && !t.IsDeleted);
            if (task == null)
            {
                return ApiResponse<bool>.Failure("Task not found.", null, 404);
            }

            // Validate time log exists and belongs to the task
            var timeLog = await _unitOfWork.TimeLog.GetFirstOrDefaultAsync(
                tl => tl.Id == request.TimeLogId && tl.TaskId == request.TaskId && !tl.IsDeleted);
            if (timeLog == null)
            {
                return ApiResponse<bool>.Failure("Time log not found for this task.", null, 404);
            }

            if (timeLog.IsApproved)
            {
                return ApiResponse<bool>.Failure("This time log has already been approved.", null, 400);
            }

            // Cannot approve your own time log
            if (timeLog.UserId == currentUserId)
            {
                return ApiResponse<bool>.Failure("You cannot approve your own time log.", null, 403);
            }

            // Row-level check: approver must be ADMIN or the project's TL/Owner. A MANAGER
            // is not global — they only qualify when they own/lead this specific project.
            bool isAdmin = _currentUserService.Roles.Contains("ADMIN");

            if (!isAdmin)
            {
                var project = await _unitOfWork.Project.GetFirstOrDefaultAsync(
                    p => p.Id == task.ProjectId && !p.IsDeleted);

                bool isProjectTL = project != null && project.TeamLeadId == currentUserId;
                bool isProjectOwner = project != null && project.OwnerId == currentUserId;

                if (!isProjectTL && !isProjectOwner)
                {
                    return ApiResponse<bool>.Failure(
                        "Only the project's Team Lead, project Owner, or an Admin can approve time logs.",
                        null, 403);
                }
            }

            timeLog.IsApproved = true;
            timeLog.ApprovedById = currentUserId;
            timeLog.ApprovedAt = DateTime.UtcNow;
            timeLog.ModifiedBy = _currentUserService.Username ?? "System";
            timeLog.ModifiedAt = DateTime.UtcNow;

            _unitOfWork.TimeLog.Update(timeLog);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return ApiResponse<bool>.Success(true, "Time log approved successfully.");
        }
    }
}
