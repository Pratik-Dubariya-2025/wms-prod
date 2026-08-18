using MediatR;
using WMS.Application.Common.Attributes;
using WMS.Application.Common.Constants;
using WMS.Application.Common.Interfaces;
using WMS.Application.Common.Models;
using WMS.Domain.Interfaces;
using WMS.Domain.Models;

namespace WMS.Application.Features.Policies.Commands
{
    [RequirePermission(PermissionCodes.PolicyWrite)]
    public class AddFieldRestrictionCommand : IRequest<ApiResponse<Guid>>
    {
        public Guid PolicyId { get; set; }
        public string FieldName { get; set; } = null!;
        public string RestrictionType { get; set; } = "Hide";
        public string? MaskPattern { get; set; }
    }

    public class AddFieldRestrictionCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        IPolicyCacheService policyCache,
        IRealtimeNotificationService realtimeNotificationService)
        : IRequestHandler<AddFieldRestrictionCommand, ApiResponse<Guid>>
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly ICurrentUserService _currentUserService = currentUserService;
        private readonly IPolicyCacheService _policyCache = policyCache;
        private readonly IRealtimeNotificationService _realtimeNotificationService = realtimeNotificationService;

        private static readonly HashSet<string> ValidRestrictionTypes = new(StringComparer.OrdinalIgnoreCase)
        {
            "Hide", "Mask", "ReadOnly"
        };

        public async Task<ApiResponse<Guid>> Handle(AddFieldRestrictionCommand request, CancellationToken cancellationToken)
        {
            var policy = await _unitOfWork.AccessPolicy.GetFirstOrDefaultAsync(p => p.Id == request.PolicyId && !p.IsDeleted);
            if (policy == null)
            {
                return ApiResponse<Guid>.Failure("Policy not found.", null, 404);
            }

            if (!ValidRestrictionTypes.Contains(request.RestrictionType))
            {
                return ApiResponse<Guid>.Failure(
                    $"Invalid restriction type '{request.RestrictionType}'. Valid types: {string.Join(", ", ValidRestrictionTypes)}.", null, 400);
            }

            // Check duplicate field for same policy
            var exists = await _unitOfWork.FieldRestriction.GetFirstOrDefaultAsync(
                f => f.PolicyId == request.PolicyId && f.FieldName.ToLower() == request.FieldName.ToLower() && !f.IsDeleted);
            if (exists != null)
            {
                return ApiResponse<Guid>.Failure($"Field '{request.FieldName}' is already restricted in this policy.", null, 409);
            }

            var restriction = new FieldRestriction
            {
                PolicyId = request.PolicyId,
                FieldName = request.FieldName.Trim(),
                RestrictionType = request.RestrictionType,
                MaskPattern = request.MaskPattern,
                CreatedBy = _currentUserService.Username ?? "System"
            };

            await _unitOfWork.FieldRestriction.AddAsync(restriction);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Invalidate cache and notify
            if (policy.UserId.HasValue)
            {
                await _policyCache.InvalidateUserPoliciesAsync(policy.UserId.Value);
                await _realtimeNotificationService.NotifyUserPermissionsChangedAsync(policy.UserId.Value, cancellationToken);
            }
            else if (policy.RoleId.HasValue)
            {
                await _policyCache.InvalidateRolePoliciesAsync(policy.RoleId.Value);
                await _realtimeNotificationService.NotifyRolePermissionsChangedAsync(policy.RoleId.Value, cancellationToken);
            }

            return ApiResponse<Guid>.Success(restriction.Id, "Field restriction added successfully.", 201);
        }
    }
}
