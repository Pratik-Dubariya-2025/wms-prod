using MediatR;
using WMS.Application.Common.Attributes;
using WMS.Application.Common.Constants;
using WMS.Application.Common.Interfaces;
using WMS.Application.Common.Models;
using WMS.Domain.Interfaces;
using WMS.Domain.Models;

namespace WMS.Application.Features.Projects.Commands
{
    [RequirePermission(PermissionCodes.ProjectUpdate)]
    public class AddProjectMemberCommand : IRequest<ApiResponse<Guid>>
    {
        public Guid ProjectId { get; set; }
        public Guid UserId { get; set; }
    }

    public class AddProjectMemberCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
        : IRequestHandler<AddProjectMemberCommand, ApiResponse<Guid>>
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly ICurrentUserService _currentUserService = currentUserService;

        public async Task<ApiResponse<Guid>> Handle(
            AddProjectMemberCommand request, CancellationToken cancellationToken)
        {
            if (!_currentUserService.UserId.HasValue)
            {
                return ApiResponse<Guid>.Failure("Unauthorized access.", null, 401);
            }

            // Verify project exists
            var project = await _unitOfWork.Project.GetFirstOrDefaultAsync(
                p => p.Id == request.ProjectId && !p.IsDeleted);
            if (project == null)
            {
                return ApiResponse<Guid>.Failure("Project not found.", null, 404);
            }

            // Row-level check: only ADMIN or the project Owner/Team Lead may add members.
            if (!ProjectAccessAuthorizer.CanManage(project, _currentUserService))
            {
                return ApiResponse<Guid>.Failure(
                    "You do not have permission to add members to this project.", null, 403);
            }

            // Verify user exists and is active
            var user = await _unitOfWork.User.GetFirstOrDefaultAsync(
                u => u.Id == request.UserId && u.IsActive && !u.IsDeleted);
            if (user == null)
            {
                return ApiResponse<Guid>.Failure("User not found or inactive.", null, 404);
            }

            // Verify if user is already a member (including soft-deleted ones)
            var existingMember = await _unitOfWork.ProjectMember.GetFirstOrDefaultWithFiltersIgnoredAsync(
                pm => pm.ProjectId == request.ProjectId && pm.UserId == request.UserId);

            if (existingMember != null)
            {
                if (!existingMember.IsDeleted)
                {
                    return ApiResponse<Guid>.Failure("User is already a member of this project.", null, 400);
                }

                // If they were deleted, reactivate them
                existingMember.IsDeleted = false;
                existingMember.DeletedAt = null;
                existingMember.DeletedBy = null;
                existingMember.ModifiedBy = _currentUserService.Username ?? "System";
                existingMember.ModifiedAt = DateTime.UtcNow;

                _unitOfWork.ProjectMember.Update(existingMember);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                return ApiResponse<Guid>.Success(existingMember.Id, "Project member added successfully.");
            }

            // Create new membership
            var member = new ProjectMember
            {
                ProjectId = request.ProjectId,
                UserId = request.UserId,
                CreatedBy = _currentUserService.Username ?? "System",
            };

            await _unitOfWork.ProjectMember.AddAsync(member);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return ApiResponse<Guid>.Success(member.Id, "Project member added successfully.");
        }
    }
}
