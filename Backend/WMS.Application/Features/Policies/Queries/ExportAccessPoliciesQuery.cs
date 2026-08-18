using MediatR;
using WMS.Application.Common.Attributes;
using WMS.Application.Common.Constants;
using WMS.Application.Common.Models;
using WMS.Domain.Interfaces;

namespace WMS.Application.Features.Policies.Queries
{
    [RequirePermission(PermissionCodes.PolicyRead)]
    public class ExportAccessPoliciesQuery : IRequest<ApiResponse<List<ExportPolicyDto>>>
    {
    }

    public class ExportPolicyDto
    {
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public string ModuleCode { get; set; } = null!;
        public string Action { get; set; } = null!;
        public string Effect { get; set; } = null!;
        public string? RoleName { get; set; }
        public string? UserEmail { get; set; }
        public string? RowFilterExpression { get; set; }
        public int Priority { get; set; }
        public bool IsActive { get; set; }
        public List<ExportConditionDto> Conditions { get; set; } = [];
        public List<ExportFieldRestrictionDto> FieldRestrictions { get; set; } = [];
    }

    public class ExportConditionDto
    {
        public int ConditionGroup { get; set; }
        public string Attribute { get; set; } = null!;
        public string Operator { get; set; } = null!;
        public string Value { get; set; } = null!;
    }

    public class ExportFieldRestrictionDto
    {
        public string FieldName { get; set; } = null!;
        public string RestrictionType { get; set; } = null!;
        public string? MaskPattern { get; set; }
    }

    public class ExportAccessPoliciesQueryHandler(IUnitOfWork unitOfWork)
        : IRequestHandler<ExportAccessPoliciesQuery, ApiResponse<List<ExportPolicyDto>>>
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;

        public async Task<ApiResponse<List<ExportPolicyDto>>> Handle(ExportAccessPoliciesQuery request, CancellationToken cancellationToken)
        {
            var policies = await _unitOfWork.AccessPolicy.IncludeAndGetAllAsync(
                p => !p.IsDeleted,
                p => p.Module,
                p => p.Role!,
                p => p.User!,
                p => p.Conditions,
                p => p.FieldRestrictions
            );

            var exportList = policies.Select(p => new ExportPolicyDto
            {
                Name = p.Name,
                Description = p.Description,
                ModuleCode = p.Module?.Code ?? "",
                Action = p.Action,
                Effect = p.Effect,
                RoleName = p.Role?.Name,
                UserEmail = p.User?.Email,
                RowFilterExpression = p.RowFilterExpression,
                Priority = p.Priority,
                IsActive = p.IsActive,
                Conditions = p.Conditions.Where(c => !c.IsDeleted).Select(c => new ExportConditionDto
                {
                    ConditionGroup = c.ConditionGroup,
                    Attribute = c.Attribute,
                    Operator = c.Operator,
                    Value = c.Value
                }).ToList(),
                FieldRestrictions = p.FieldRestrictions.Where(f => !f.IsDeleted).Select(f => new ExportFieldRestrictionDto
                {
                    FieldName = f.FieldName,
                    RestrictionType = f.RestrictionType,
                    MaskPattern = f.MaskPattern
                }).ToList()
            }).ToList();

            return ApiResponse<List<ExportPolicyDto>>.Success(exportList, "Policies exported successfully.");
        }
    }
}
