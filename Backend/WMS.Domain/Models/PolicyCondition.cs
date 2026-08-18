namespace WMS.Domain.Models
{
    /// <summary>
    /// A condition attached to an AccessPolicy.
    /// Conditions in the same ConditionGroup are combined with AND.
    /// Conditions across different ConditionGroups are combined with OR.
    /// 
    /// Example:
    ///   Group 0: user.departmentId eq {resource.departmentId} AND user.role eq MANAGER
    ///   Group 1: user.id eq {resource.ownerId}
    ///   Evaluation: (Group0.Cond1 AND Group0.Cond2) OR (Group1.Cond1)
    /// </summary>
    public class PolicyCondition : BaseModel
    {
        public Guid PolicyId { get; set; }
        public AccessPolicy Policy { get; set; } = null!;

        /// <summary>
        /// Group number for AND/OR logic.
        /// Same group = AND, different groups = OR.
        /// </summary>
        public int ConditionGroup { get; set; }

        /// <summary>
        /// The attribute to evaluate.
        /// Examples: "user.departmentId", "user.teamId", "user.id", "resource.ownerId", "resource.departmentId"
        /// </summary>
        public string Attribute { get; set; } = null!;

        /// <summary>
        /// Comparison operator: eq, neq, in, not_in, contains, gt, lt, gte, lte
        /// </summary>
        public string Operator { get; set; } = null!;

        /// <summary>
        /// The value to compare against.
        /// Can be a static value (e.g., "MANAGER") or a dynamic binding (e.g., "{user.id}").
        /// </summary>
        public string Value { get; set; } = null!;
    }
}
