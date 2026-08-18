using MediatR;
using WMS.Application.Common.Attributes;
using WMS.Application.Common.Constants;
using WMS.Application.Common.Interfaces;
using WMS.Application.Common.Models;
using WMS.Application.Features.Tasks.DTOs;
using WMS.Domain.Interfaces;

namespace WMS.Application.Features.Tasks.Queries
{
    [RequirePermission(PermissionCodes.TaskRead)]
    public class GetTaskByIdQuery : IRequest<ApiResponse<TaskDetailDto>>
    {
        public Guid Id { get; set; }
    }

    public class GetTaskByIdQueryHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
        : IRequestHandler<GetTaskByIdQuery, ApiResponse<TaskDetailDto>>
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly ICurrentUserService _currentUserService = currentUserService;

        public async Task<ApiResponse<TaskDetailDto>> Handle(
            GetTaskByIdQuery request, CancellationToken cancellationToken)
        {
            var currentUserId = _currentUserService.UserId;
            var currentUserRoles = _currentUserService.Roles;

            TaskDetailDto? taskDetail = await _unitOfWork.TaskItem.GetFirstOrDefaultAsync(
                select: t => new TaskDetailDto
                {
                    Id = t.Id,
                    Title = t.Title,
                    Description = t.Description,
                    Status = t.Status,
                    Priority = t.Priority,
                    EstimatedHours = t.EstimatedHours,
                    DueDate = t.DueDate,
                    ProjectId = t.ProjectId,
                    ProjectName = t.Project.Name,
                    TeamId = t.TeamId,
                    TeamName = t.Team != null ? t.Team.Name : null,
                    AssigneeId = t.AssigneeId,
                    AssigneeName = t.Assignee != null
                        ? t.Assignee.FirstName + " " + t.Assignee.LastName
                        : null,
                    AssigneeEmail = t.Assignee != null ? t.Assignee.Email : null,
                    CreatedById = t.CreatedById,
                    CreatedByName = t.CreatedByUser.FirstName + " " + t.CreatedByUser.LastName,
                    CreatedAt = t.CreatedAt,
                    CreatedBy = t.CreatedBy,
                    ModifiedAt = t.ModifiedAt,
                    ModifiedBy = t.ModifiedBy
                },
                where: t => t.Id == request.Id && !t.IsDeleted &&
                    (currentUserRoles.Contains("ADMIN") ||
                     t.AssigneeId == currentUserId ||
                     t.CreatedById == currentUserId ||
                     (t.Project != null && (t.Project.OwnerId == currentUserId || t.Project.TeamLeadId == currentUserId || t.Project.Members.Any(m => m.UserId == currentUserId && !m.IsDeleted))))
            );

            if (taskDetail == null)
            {
                return ApiResponse<TaskDetailDto>.Failure("Task not found.", null, 404);
            }

            return ApiResponse<TaskDetailDto>.Success(taskDetail);
        }
    }
}
