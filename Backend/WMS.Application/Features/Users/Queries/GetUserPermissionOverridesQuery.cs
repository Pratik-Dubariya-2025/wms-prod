using MediatR;
using WMS.Application.Common.Attributes;
using WMS.Application.Common.Constants;
using WMS.Application.Common.Interfaces;
using WMS.Application.Common.Models;
using WMS.Application.Features.Users.DTOs;
using WMS.Domain.Interfaces;

namespace WMS.Application.Features.Users.Queries
{
    public class UserPermissionOverridesDto
    {
        public List<UserPermissionOverrideDto> Overrides { get; set; } = [];

        /// <summary>
        /// Permission codes the CURRENT caller is allowed to grant/revoke for this target user.
        /// ADMIN sees everything; anyone else is scoped through USER_MGMT/ManagePermissions PBAC
        /// policies (e.g. a Dept Manager limited to their subordinates + specific modules). The
        /// frontend uses this to hide toggles the caller isn't allowed to touch.
        /// </summary>
        public List<string> ManageablePermissions { get; set; } = [];
    }

    [RequirePermission(PermissionCodes.UserRead)]
    public class GetUserPermissionOverridesQuery : IRequest<ApiResponse<UserPermissionOverridesDto>>
    {
        public Guid UserId { get; set; }
    }

    public class GetUserPermissionOverridesQueryHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        IPolicyEvaluationService policyEvaluationService)
        : IRequestHandler<GetUserPermissionOverridesQuery, ApiResponse<UserPermissionOverridesDto>>
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly ICurrentUserService _currentUserService = currentUserService;
        private readonly IPolicyEvaluationService _policyEvaluationService = policyEvaluationService;

        public async Task<ApiResponse<UserPermissionOverridesDto>> Handle(GetUserPermissionOverridesQuery request, CancellationToken cancellationToken)
        {
            var user = await _unitOfWork.User.GetFirstOrDefaultAsync(u => u.Id == request.UserId && !u.IsDeleted);
            if (user == null)
            {
                return ApiResponse<UserPermissionOverridesDto>.Failure("User not found.", null, 404);
            }

            var overrides = await _unitOfWork.UserPermissionOverride.IncludeAndGetAllAsync(
                o => o.UserId == request.UserId && !o.IsDeleted && (o.ExpiresAt == null || o.ExpiresAt > DateTime.UtcNow),
                o => o.Permission
            );

            var overrideDtos = overrides
                .Where(o => o.Permission != null)
                .Select(o => new UserPermissionOverrideDto
                {
                    PermissionCode = o.Permission.Code,
                    IsGranted = o.IsGranted,
                    Reason = o.Reason,
                    ExpiresAt = o.ExpiresAt.HasValue ? DateTime.SpecifyKind(o.ExpiresAt.Value, DateTimeKind.Utc) : null
                }).ToList();

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
                // Fetch the user + policy cache once, evaluate every candidate permission against it
                // instead of once per permission (mirrors GetRolePermissionsQuery's approach).
                var resourceContexts = allPermissions
                    .Select(perm => (object?)new Dictionary<string, object>
                    {
                        { "UserId", request.UserId },
                        { "PermissionCode", perm.Code }
                    })
                    .ToList();

                var results = await _policyEvaluationService.EvaluateBatchAsync(
                    currentUserId.Value, "USER_MGMT", "ManagePermissions", resourceContexts);

                for (int i = 0; i < allPermissions.Count; i++)
                {
                    if (results[i].IsAllowed)
                    {
                        manageablePermissions.Add(allPermissions[i].Code);
                    }
                }
            }

            var dto = new UserPermissionOverridesDto
            {
                Overrides = overrideDtos,
                ManageablePermissions = manageablePermissions
            };

            return ApiResponse<UserPermissionOverridesDto>.Success(dto, "User permission overrides retrieved successfully.");
        }
    }
}
