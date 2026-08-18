using MediatR;
using WMS.Application.Common.Attributes;
using WMS.Application.Common.Constants;
using WMS.Application.Common.Interfaces;
using WMS.Application.Common.Models;
using WMS.Domain.Interfaces;
using WMS.Domain.Models;

namespace WMS.Application.Features.Roles.Commands
{
    [RequirePermission(PermissionCodes.RoleCreate)]
    public class CreateRoleCommand : IRequest<ApiResponse<Guid>>
    {
        public string Name { get; set; } = null!;
        public string Code { get; set; } = null!;
        public int Priority { get; set; } = 100;
        public string? Description { get; set; }
    }

    public class CreateRoleCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService) 
        : IRequestHandler<CreateRoleCommand, ApiResponse<Guid>>
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly ICurrentUserService _currentUserService = currentUserService;

        public async Task<ApiResponse<Guid>> Handle(CreateRoleCommand request, CancellationToken cancellationToken)
        {
            var exists = await _unitOfWork.Role.GetFirstOrDefaultAsync(r => r.Name.ToLower() == request.Name.ToLower() && !r.IsDeleted);
            if (exists != null)
            {
                return ApiResponse<Guid>.Failure("A role with this name already exists.", null, 409);
            }

            var codeExists = await _unitOfWork.Role.GetFirstOrDefaultAsync(r => r.Code.ToLower() == request.Code.ToLower() && !r.IsDeleted);
            if (codeExists != null)
            {
                return ApiResponse<Guid>.Failure("A role with this code already exists.", null, 409);
            }

            var role = new Role
            {
                Name = request.Name,
                Code = request.Code.Trim().ToUpper(),
                Priority = request.Priority,
                Description = request.Description,
                IsSystemRole = false,
                CreatedBy = _currentUserService.Username ?? "System"
            };

            await _unitOfWork.Role.AddAsync(role);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return ApiResponse<Guid>.Success(role.Id, "Role created successfully.", 201);
        }
    }
}
