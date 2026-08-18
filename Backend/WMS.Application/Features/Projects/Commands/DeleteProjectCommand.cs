using MediatR;
using WMS.Application.Common.Attributes;
using WMS.Application.Common.Constants;
using WMS.Application.Common.Interfaces;
using WMS.Application.Common.Models;
using WMS.Domain.Interfaces;

namespace WMS.Application.Features.Projects.Commands
{
    [RequirePermission(PermissionCodes.ProjectDelete)]
    public class DeleteProjectCommand : IRequest<ApiResponse<bool>>
    {
        public Guid Id { get; set; }
    }

    public class DeleteProjectCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
        : IRequestHandler<DeleteProjectCommand, ApiResponse<bool>>
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly ICurrentUserService _currentUserService = currentUserService;

        public async Task<ApiResponse<bool>> Handle(
            DeleteProjectCommand request, CancellationToken cancellationToken)
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

            // Row-level check: only ADMIN or the project Owner/Team Lead may delete it.
            if (!ProjectAccessAuthorizer.CanManage(project, _currentUserService))
            {
                return ApiResponse<bool>.Failure(
                    "You do not have permission to delete this project.", null, 403);
            }

            project.IsDeleted = true;
            project.DeletedBy = _currentUserService.Username ?? "System";
            project.DeletedAt = DateTime.UtcNow;

            _unitOfWork.Project.Update(project);

            // Soft-delete project members
            var members = await _unitOfWork.ProjectMember.GetAllAsync(
                pm => pm.ProjectId == request.Id && !pm.IsDeleted);
            foreach (var member in members)
            {
                member.IsDeleted = true;
                member.DeletedBy = _currentUserService.Username ?? "System";
                member.DeletedAt = DateTime.UtcNow;
                _unitOfWork.ProjectMember.Update(member);
            }

            // Soft-delete tasks associated with this project
            var tasks = await _unitOfWork.TaskItem.GetAllAsync(
                t => t.ProjectId == request.Id && !t.IsDeleted);
            foreach (var task in tasks)
            {
                task.IsDeleted = true;
                task.DeletedBy = _currentUserService.Username ?? "System";
                task.DeletedAt = DateTime.UtcNow;
                _unitOfWork.TaskItem.Update(task);
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return ApiResponse<bool>.Success(true, "Project deleted successfully.");
        }
    }
}
