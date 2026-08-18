using MediatR;
using WMS.Application.Common.Attributes;
using WMS.Application.Common.Constants;
using WMS.Application.Common.Interfaces;
using WMS.Application.Common.Models;
using WMS.Domain.Interfaces;

namespace WMS.Application.Features.Tasks.Commands
{
    [RequirePermission(PermissionCodes.TaskDelete)]
    public class DeleteTaskCommand : IRequest<ApiResponse<bool>>
    {
        public Guid Id { get; set; }
        public Guid ProjectId { get; set; }
    }

    public class DeleteTaskCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService)
        : IRequestHandler<DeleteTaskCommand, ApiResponse<bool>>
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly ICurrentUserService _currentUserService = currentUserService;

        public async Task<ApiResponse<bool>> Handle(
            DeleteTaskCommand request, CancellationToken cancellationToken)
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

            // Prevent deletion if task is InReview or above
            string[] lockedStatuses = ["InReview", "Done", "Closed"];
            if (lockedStatuses.Contains(task.Status))
            {
                return ApiResponse<bool>.Failure(
                    $"Cannot delete task because it is in '{task.Status}' status. Tasks in InReview, Done, or Closed status cannot be deleted.",
                    null, 400);
            }

            if (task.ProjectId != request.ProjectId)
            {
                return ApiResponse<bool>.Failure("Task does not belong to the specified project.", null, 400);
            }

            // Row-level check: only ADMIN, the task creator, project owner, or project TL
            // can delete. A MANAGER is not global — they only qualify via ownership/TL of
            // this specific project.
            var project = await _unitOfWork.Project.GetFirstOrDefaultAsync(
                p => p.Id == task.ProjectId && !p.IsDeleted);

            bool isAdmin = _currentUserService.Roles.Contains("ADMIN");
            bool isCreator = task.CreatedById == currentUserId;
            bool isProjectOwner = project != null && project.OwnerId == currentUserId;
            bool isProjectTL = project != null && project.TeamLeadId == currentUserId;

            if (!isAdmin && !isCreator && !isProjectOwner && !isProjectTL)
            {
                return ApiResponse<bool>.Failure(
                    "You do not have permission to delete this task. Only the task creator, project owner, or team lead can delete tasks.",
                    null, 403);
            }

            // Soft-delete the task
            task.IsDeleted = true;
            task.DeletedBy = _currentUserService.Username ?? "System";
            task.DeletedAt = DateTime.UtcNow;
            _unitOfWork.TaskItem.Update(task);

            // Soft-delete associated time logs
            var timeLogs = await _unitOfWork.TimeLog.GetAllAsync(
                tl => tl.TaskId == request.Id && !tl.IsDeleted);
            foreach (var log in timeLogs)
            {
                log.IsDeleted = true;
                log.DeletedBy = _currentUserService.Username ?? "System";
                log.DeletedAt = DateTime.UtcNow;
                _unitOfWork.TimeLog.Update(log);
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return ApiResponse<bool>.Success(true, "Task deleted successfully.");
        }
    }
}
