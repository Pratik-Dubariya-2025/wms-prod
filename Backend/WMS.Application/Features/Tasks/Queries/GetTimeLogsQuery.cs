using MediatR;
using WMS.Application.Common.Attributes;
using WMS.Application.Common.Constants;
using WMS.Application.Common.Interfaces;
using WMS.Application.Common.Models;
using WMS.Application.Features.Tasks.DTOs;
using WMS.Domain.Interfaces;

namespace WMS.Application.Features.Tasks.Queries
{
    /// <summary>
    /// Get time logs for a task. Per SRS 7.4 (timelogs:read:*),
    /// the returned data is row-scoped by user role:
    ///   - SE/SSE/ASE_TSE: only own time logs
    ///   - TL: all time logs for tasks in their projects
    ///   - Manager/Admin: all time logs
    /// </summary>
    [RequirePermission(PermissionCodes.TimeLogRead)]
    public class GetTimeLogsQuery : IRequest<ApiResponse<List<TimeLogDto>>>
    {
        public Guid TaskId { get; set; }
    }

    public class GetTimeLogsQueryHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService)
        : IRequestHandler<GetTimeLogsQuery, ApiResponse<List<TimeLogDto>>>
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly ICurrentUserService _currentUserService = currentUserService;

        public async Task<ApiResponse<List<TimeLogDto>>> Handle(
            GetTimeLogsQuery request, CancellationToken cancellationToken)
        {
            if (!_currentUserService.UserId.HasValue)
            {
                return ApiResponse<List<TimeLogDto>>.Failure("Unauthorized access.", null, 401);
            }

            var currentUserId = _currentUserService.UserId.Value;

            // Validate task exists
            var task = await _unitOfWork.TaskItem.GetFirstOrDefaultAsync(
                t => t.Id == request.TaskId && !t.IsDeleted);
            if (task == null)
            {
                return ApiResponse<List<TimeLogDto>>.Failure("Task not found.", null, 404);
            }

            bool isAdmin = _currentUserService.Roles.Contains("ADMIN");
            bool isTL = _currentUserService.Roles.Contains("TL");

            // Check if user is the project's TL or Owner. A MANAGER is not global — they
            // only see all of a task's time logs when they own/lead that project.
            bool isProjectTLOrOwner = false;
            if (!isAdmin)
            {
                var project = await _unitOfWork.Project.GetFirstOrDefaultAsync(
                    p => p.Id == task.ProjectId && !p.IsDeleted);
                isProjectTLOrOwner = project != null &&
                    (project.TeamLeadId == currentUserId || project.OwnerId == currentUserId);
            }

            List<TimeLogDto> timeLogs;

            if (isAdmin || isTL || isProjectTLOrOwner)
            {
                // Elevated roles: see all time logs for this task
                timeLogs = await _unitOfWork.TimeLog.GetAllAsync(
                    select: tl => new TimeLogDto
                    {
                        Id = tl.Id,
                        TaskId = tl.TaskId,
                        TaskTitle = tl.Task.Title,
                        UserId = tl.UserId,
                        UserName = tl.User.FirstName + " " + tl.User.LastName,
                        LoggedHours = tl.LoggedHours,
                        LogDate = tl.LogDate,
                        Notes = tl.Notes,
                        IsApproved = tl.IsApproved,
                        ApprovedByName = tl.ApprovedBy != null
                            ? tl.ApprovedBy.FirstName + " " + tl.ApprovedBy.LastName
                            : null,
                        ApprovedAt = tl.ApprovedAt,
                        CreatedAt = tl.CreatedAt
                    },
                    where: tl => tl.TaskId == request.TaskId && !tl.IsDeleted
                );
            }
            else
            {
                // Regular users: see only their own time logs for this task
                timeLogs = await _unitOfWork.TimeLog.GetAllAsync(
                    select: tl => new TimeLogDto
                    {
                        Id = tl.Id,
                        TaskId = tl.TaskId,
                        TaskTitle = tl.Task.Title,
                        UserId = tl.UserId,
                        UserName = tl.User.FirstName + " " + tl.User.LastName,
                        LoggedHours = tl.LoggedHours,
                        LogDate = tl.LogDate,
                        Notes = tl.Notes,
                        IsApproved = tl.IsApproved,
                        ApprovedByName = tl.ApprovedBy != null
                            ? tl.ApprovedBy.FirstName + " " + tl.ApprovedBy.LastName
                            : null,
                        ApprovedAt = tl.ApprovedAt,
                        CreatedAt = tl.CreatedAt
                    },
                    where: tl => tl.TaskId == request.TaskId && tl.UserId == currentUserId && !tl.IsDeleted
                );
            }

            return ApiResponse<List<TimeLogDto>>.Success(timeLogs);
        }
    }
}
