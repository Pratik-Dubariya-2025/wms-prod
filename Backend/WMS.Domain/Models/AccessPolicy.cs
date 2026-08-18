namespace WMS.Domain.Models
{
    /// <summary>
    /// A runtime-configurable access policy (PBAC layer).
    /// Sits alongside the existing RBAC system without modifying it.
    /// One policy = one rule targeting a specific module + action for a role or user.
    /// </summary>
    public class AccessPolicy : BaseModel
    {
        public string Name { get; set; } = null!;
        public string? Description { get; set; }

        /// <summary>Which module this policy targets (e.g., Project Management, Leave Management).</summary>
        public Guid ModuleId { get; set; }
        public Module Module { get; set; } = null!;

        /// <summary>The CRUD action: Create, Read, Update, Delete, Approve.</summary>
        public string Action { get; set; } = null!;

        /// <summary>Allow or Deny. Explicit Deny always wins over Allow.</summary>
        public string Effect { get; set; } = "Allow";

        /// <summary>Assign to a role (mutually exclusive with UserId).</summary>
        public Guid? RoleId { get; set; }
        public Role? Role { get; set; }

        /// <summary>Assign to a specific user (mutually exclusive with RoleId).</summary>
        public Guid? UserId { get; set; }
        public User? User { get; set; }

        /// <summary>
        /// Row-level filter expression stored as JSON.
        /// Example: {"field":"DepartmentId","operator":"eq","value":"{user.departmentId}","valueType":"dynamic"}
        /// Null means no row filtering.
        /// </summary>
        public string? RowFilterExpression { get; set; }

        /// <summary>Lower number = higher priority. Used for conflict resolution.</summary>
        public int Priority { get; set; } = 100;

        /// <summary>Only active policies are evaluated.</summary>
        public bool IsActive { get; set; } = true;

        /// <summary>Auto-expire temporary policies.</summary>
        public DateTime? ExpiresAt { get; set; }

        // Navigation properties
        public List<PolicyCondition> Conditions { get; set; } = [];
        public List<FieldRestriction> FieldRestrictions { get; set; } = [];
    }
}
