using MediatR;
using WMS.Application.Common.Attributes;
using WMS.Application.Common.Constants;
using WMS.Application.Common.Interfaces;
using WMS.Application.Common.Models;
using WMS.Domain.Interfaces;

namespace WMS.Application.Features.Roles.Queries
{
    public class RolePermissionsDto
    {
        public List<string> AssignedPermissions { get; set; } = new();
        public List<string> ManageablePermissions { get; set; } = new();
    }

    [RequirePermission(PermissionCodes.RoleRead)]
    public class GetRolePermissionsQuery : IRequest<ApiResponse<RolePermissionsDto>>
    {
        public Guid RoleId { get; set; }
    }

    public class GetRolePermissionsQueryHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        IPolicyEvaluationService policyEvaluationService) 
        : IRequestHandler<GetRolePermissionsQuery, ApiResponse<RolePermissionsDto>>
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly ICurrentUserService _currentUserService = currentUserService;
        private readonly IPolicyEvaluationService _policyEvaluationService = policyEvaluationService;

        public async Task<ApiResponse<RolePermissionsDto>> Handle(GetRolePermissionsQuery request, CancellationToken cancellationToken)
        {
            var role = await _unitOfWork.Role.GetFirstOrDefaultAsync(r => r.Id == request.RoleId && !r.IsDeleted);
            if (role == null)
            {
                return ApiResponse<RolePermissionsDto>.Failure("Role not found.", null, 404);
            }

            var assignedPermissions = await _unitOfWork.RolePermission.GetAllAsync(
                rp => rp.Permission.Code,
                rp => rp.RoleId == request.RoleId
            );

            var manageablePermissions = new List<string>();
            var allPermissions = await _unitOfWork.Permission.GetAllAsync(p => !p.IsDeleted);

            var currentUserId = _currentUserService.UserId;
            var isAdmin = _currentUserService.Roles.Contains("ADMIN");

            if (isAdmin)
            {
                manageablePermissions = allPermissions.Select(p => p.Code).ToList();
            }
            else if (currentUserId.HasValue)
            {
                // Fetch the user + policy cache once per action (not once per permission) - evaluate
                // all permissions against that single fetch.
                var resourceContexts = allPermissions
                    .Select(perm => (object?)new Dictionary<string, object>
                    {
                        { "RoleId", request.RoleId },
                        { "PermissionCode", perm.Code }
                    })
                    .ToList();

                var readResults = await _policyEvaluationService.EvaluateBatchAsync(
                    currentUserId.Value, "ROLE_MGMT", "Read", resourceContexts);
                var updateResults = await _policyEvaluationService.EvaluateBatchAsync(
                    currentUserId.Value, "ROLE_MGMT", "Update", resourceContexts);

                for (int i = 0; i < allPermissions.Count; i++)
                {
                    if (readResults[i].IsAllowed || updateResults[i].IsAllowed)
                    {
                        manageablePermissions.Add(allPermissions[i].Code);
                    }
                }
            }

            var filteredAssigned = assignedPermissions
                .Where(code => manageablePermissions.Contains(code))
                .ToList();

            var dto = new RolePermissionsDto
            {
                AssignedPermissions = filteredAssigned,
                ManageablePermissions = manageablePermissions
            };

            return ApiResponse<RolePermissionsDto>.Success(dto, "Role permissions retrieved successfully.");
        }
    }
}
