using MediatR;
using WMS.Application.Common.Attributes;
using WMS.Application.Common.Constants;
using WMS.Application.Common.Interfaces;
using WMS.Application.Common.Models;
using WMS.Domain.Interfaces;

namespace WMS.Application.Features.Roles.Commands
{
    [RequirePermission(PermissionCodes.RoleUpdate)]
    public class UpdateRoleCommand : IRequest<ApiResponse<bool>>
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public int Priority { get; set; } = 100;
    }

    public class UpdateRoleCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService) 
        : IRequestHandler<UpdateRoleCommand, ApiResponse<bool>>
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly ICurrentUserService _currentUserService = currentUserService;

        public async Task<ApiResponse<bool>> Handle(UpdateRoleCommand request, CancellationToken cancellationToken)
        {
            var role = await _unitOfWork.Role.GetFirstOrDefaultAsync(r => r.Id == request.Id && !r.IsDeleted);
            if (role == null)
            {
                return ApiResponse<bool>.Failure("Role not found.", null, 404);
            }

            if (role.IsSystemRole)
            {
                // Can only update Description of system roles
                role.Description = request.Description;
            }
            else
            {
                var exists = await _unitOfWork.Role.GetFirstOrDefaultAsync(r => r.Name.ToLower() == request.Name.ToLower() && r.Id != request.Id && !r.IsDeleted);
                if (exists != null)
                {
                    return ApiResponse<bool>.Failure("A role with this name already exists.", null, 409);
                }

                role.Name = request.Name;
                role.Description = request.Description;
                role.Priority = request.Priority;
            }

            role.ModifiedBy = _currentUserService.Username ?? "System";
            role.ModifiedAt = DateTime.UtcNow;

            _unitOfWork.Role.Update(role);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return ApiResponse<bool>.Success(true, "Role updated successfully.");
        }
    }
}
