namespace WMS.Application.Features.Policies.DTOs
{
    public class AccessPolicyDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public string? Description { get; set; }

        public Guid ModuleId { get; set; }
        public string ModuleName { get; set; } = null!;
        public string ModuleCode { get; set; } = null!;

        public string Action { get; set; } = null!;
        public string Effect { get; set; } = null!;

        public Guid? RoleId { get; set; }
        public string? RoleName { get; set; }

        public Guid? UserId { get; set; }
        public string? UserName { get; set; }

        public string? RowFilterExpression { get; set; }
        public int Priority { get; set; }
        public bool IsActive { get; set; }
        public DateTime? ExpiresAt { get; set; }
        public DateTime CreatedAt { get; set; }

        public List<PolicyConditionDto> Conditions { get; set; } = [];
        public List<FieldRestrictionDto> FieldRestrictions { get; set; } = [];
    }

    public class AccessPolicyListDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public string ModuleName { get; set; } = null!;
        public string ModuleCode { get; set; } = null!;
        public string Action { get; set; } = null!;
        public string Effect { get; set; } = null!;
        public string? TargetName { get; set; }
        public string TargetType { get; set; } = null!;
        public int Priority { get; set; }
        public bool IsActive { get; set; }
        public DateTime? ExpiresAt { get; set; }
        public int ConditionCount { get; set; }
        public int FieldRestrictionCount { get; set; }
    }

    public class PolicyConditionDto
    {
        public Guid Id { get; set; }
        public int ConditionGroup { get; set; }
        public string Attribute { get; set; } = null!;
        public string Operator { get; set; } = null!;
        public string Value { get; set; } = null!;
    }

    public class FieldRestrictionDto
    {
        public Guid Id { get; set; }
        public string FieldName { get; set; } = null!;
        public string RestrictionType { get; set; } = null!;
        public string? MaskPattern { get; set; }
    }

    public class ModuleWithActionsDto
    {
        public Guid Id { get; set; }
        public string Code { get; set; } = null!;
        public string Name { get; set; } = null!;
        public List<string> AvailableActions { get; set; } = [];
    }
}
