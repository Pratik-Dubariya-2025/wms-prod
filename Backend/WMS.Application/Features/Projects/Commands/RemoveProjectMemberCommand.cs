using MediatR;
using WMS.Application.Common.Attributes;
using WMS.Application.Common.Constants;
using WMS.Application.Common.Interfaces;
using WMS.Application.Common.Models;
using WMS.Domain.Interfaces;

namespace WMS.Application.Features.Projects.Commands
{
    [RequirePermission(PermissionCodes.ProjectUpdate)]
    public class RemoveProjectMemberCommand : IRequest<ApiResponse<bool>>
    {
        public Guid ProjectId { get; set; }
        public Guid UserId { get; set; }
    }

    public class RemoveProjectMemberCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
        : IRequestHandler<RemoveProjectMemberCommand, ApiResponse<bool>>
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly ICurrentUserService _currentUserService = currentUserService;

        public async Task<ApiResponse<bool>> Handle(
            RemoveProjectMemberCommand request, CancellationToken cancellationToken)
        {
            if (!_currentUserService.UserId.HasValue)
            {
                return ApiResponse<bool>.Failure("Unauthorized access.", null, 401);
            }

            // Verify project exists
            var project = await _unitOfWork.Project.GetFirstOrDefaultAsync(
                p => p.Id == request.ProjectId && !p.IsDeleted);
            if (project == null)
            {
                return ApiResponse<bool>.Failure("Project not found.", null, 404);
            }

            // Row-level check: only ADMIN or the project Owner/Team Lead may remove members.
            if (!ProjectAccessAuthorizer.CanManage(project, _currentUserService))
            {
                return ApiResponse<bool>.Failure(
                    "You do not have permission to remove members from this project.", null, 403);
            }

            // Block removing the Team Lead
            if (project.TeamLeadId == request.UserId)
            {
                return ApiResponse<bool>.Failure("Cannot remove the Team Lead of the project. Reassign the Team Lead role first.", null, 400);
            }

            // Find the membership
            var member = await _unitOfWork.ProjectMember.GetFirstOrDefaultAsync(
                pm => pm.ProjectId == request.ProjectId && pm.UserId == request.UserId && !pm.IsDeleted);

            if (member == null)
            {
                return ApiResponse<bool>.Failure("Project member not found.", null, 404);
            }

            // Soft-delete membership
            member.IsDeleted = true;
            member.DeletedBy = _currentUserService.Username ?? "System";
            member.DeletedAt = DateTime.UtcNow;

            _unitOfWork.ProjectMember.Update(member);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return ApiResponse<bool>.Success(true, "Project member removed successfully.");
        }
    }
}
