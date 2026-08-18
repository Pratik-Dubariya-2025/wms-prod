using MediatR;
using WMS.Application.Common.Attributes;
using WMS.Application.Common.Constants;
using WMS.Application.Common.Models;
using WMS.Application.Features.Tasks.DTOs;
using WMS.Domain.Interfaces;

namespace WMS.Application.Features.Tasks.Queries
{
    /// <summary>
    /// Fetches the list of project members for the task creation form assignee dropdown.
    /// Scoped to a specific project.
    /// </summary>
    [RequirePermission(PermissionCodes.TaskCreate)]
    public class GetTaskCreateMetaQuery : IRequest<ApiResponse<TaskCreateMetaDto>>
    {
        public Guid ProjectId { get; set; }
    }

    public class GetTaskCreateMetaQueryHandler(IUnitOfWork unitOfWork)
        : IRequestHandler<GetTaskCreateMetaQuery, ApiResponse<TaskCreateMetaDto>>
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;

        public async Task<ApiResponse<TaskCreateMetaDto>> Handle(
            GetTaskCreateMetaQuery request, CancellationToken cancellationToken)
        {
            // Get project to include TL and Owner
            var project = await _unitOfWork.Project.GetFirstOrDefaultAsync(
                p => p.Id == request.ProjectId && !p.IsDeleted);
            if (project == null)
            {
                return ApiResponse<TaskCreateMetaDto>.Failure("Project not found.", null, 404);
            }

            // Get members of this project (for the assignee dropdown)
            List<TaskUserLookup> members = await _unitOfWork.ProjectMember.GetAllAsync(
                select: pm => new TaskUserLookup
                {
                    Id = pm.UserId,
                    FullName = pm.User.FirstName + " " + pm.User.LastName,
                },
                where: pm => pm.ProjectId == request.ProjectId && !pm.IsDeleted && pm.User.IsActive && !pm.User.IsDeleted
            );

            TaskCreateMetaDto meta = new()
            {
                Users = members
            };

            return ApiResponse<TaskCreateMetaDto>.Success(meta);
        }
    }
}
