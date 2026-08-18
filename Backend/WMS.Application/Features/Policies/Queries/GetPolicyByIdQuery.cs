using MediatR;
using WMS.Application.Common.Attributes;
using WMS.Application.Common.Constants;
using WMS.Application.Common.Models;
using WMS.Application.Features.Policies.DTOs;
using WMS.Domain.Interfaces;

namespace WMS.Application.Features.Policies.Queries
{
    [RequirePermission(PermissionCodes.PolicyRead)]
    public class GetPolicyByIdQuery : IRequest<ApiResponse<AccessPolicyDto>>
    {
        public Guid Id { get; set; }
    }

    public class GetPolicyByIdQueryHandler(IUnitOfWork unitOfWork)
        : IRequestHandler<GetPolicyByIdQuery, ApiResponse<AccessPolicyDto>>
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;

        public async Task<ApiResponse<AccessPolicyDto>> Handle(GetPolicyByIdQuery request, CancellationToken cancellationToken)
        {
            var policy = await _unitOfWork.AccessPolicy.IncludeAndGetFirstOrDefaultAsync<object>(
                p => p.Id == request.Id && !p.IsDeleted,
                p => p.Module,
                p => p.Role!,
                p => p.User!,
                p => p.Conditions,
                p => p.FieldRestrictions
            );

            if (policy == null)
            {
                return ApiResponse<AccessPolicyDto>.Failure("Policy not found.", null, 404);
            }

            var dto = new AccessPolicyDto
            {
                Id = policy.Id,
                Name = policy.Name,
                Description = policy.Description,
                ModuleId = policy.ModuleId,
                ModuleName = policy.Module?.Name ?? "",
                ModuleCode = policy.Module?.Code ?? "",
                Action = policy.Action,
                Effect = policy.Effect,
                RoleId = policy.RoleId,
                RoleName = policy.Role?.Name,
                UserId = policy.UserId,
                UserName = policy.User != null ? $"{policy.User.FirstName} {policy.User.LastName}" : null,
                RowFilterExpression = policy.RowFilterExpression,
                Priority = policy.Priority,
                IsActive = policy.IsActive,
                ExpiresAt = policy.ExpiresAt,
                CreatedAt = policy.CreatedAt,
                Conditions = policy.Conditions
                    .Where(c => !c.IsDeleted)
                    .OrderBy(c => c.ConditionGroup)
                    .Select(c => new PolicyConditionDto
                    {
                        Id = c.Id,
                        ConditionGroup = c.ConditionGroup,
                        Attribute = c.Attribute,
                        Operator = c.Operator,
                        Value = c.Value
                    }).ToList(),
                FieldRestrictions = policy.FieldRestrictions
                    .Where(f => !f.IsDeleted)
                    .Select(f => new FieldRestrictionDto
                    {
                        Id = f.Id,
                        FieldName = f.FieldName,
                        RestrictionType = f.RestrictionType,
                        MaskPattern = f.MaskPattern
                    }).ToList()
            };

            return ApiResponse<AccessPolicyDto>.Success(dto, "Policy retrieved successfully.");
        }
    }
}
