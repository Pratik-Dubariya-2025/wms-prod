using System.Linq.Expressions;

namespace WMS.Application.Common.Interfaces
{
    public class PolicyEvaluationResult
    {
        public bool IsAllowed { get; set; } = true;
        public string? DenialReason { get; set; }

        /// <summary>
        /// True if no policies exist for the requested module and action.
        /// This allows the system to fall back to the existing RBAC check (backward compatibility).
        /// </summary>
        public bool NoPoliciesExist { get; set; } = true;

        /// <summary>
        /// List of fields that are restricted (hidden, masked, or read-only) for the current request.
        /// </summary>
        public List<FieldRestrictionResult> FieldRestrictions { get; set; } = [];
    }

    public class FieldRestrictionResult
    {
        public string FieldName { get; set; } = null!;
        public string RestrictionType { get; set; } = "Hide"; // Hide, Mask, ReadOnly
        public string? MaskPattern { get; set; }
    }

    public class PolicyPreviewResult
    {
        public bool IsAllowed { get; set; }
        public string? DenialReason { get; set; }
        public bool NoPoliciesExist { get; set; }
        public List<EvaluatedPolicyDetail> EvaluatedPolicies { get; set; } = [];
        public List<FieldRestrictionResult> FinalFieldRestrictions { get; set; } = [];
        public string? ResolvedRowFilter { get; set; }
    }

    public class EvaluatedPolicyDetail
    {
        public Guid PolicyId { get; set; }
        public string PolicyName { get; set; } = null!;
        public string Effect { get; set; } = null!;
        public int Priority { get; set; }
        public bool PassedConditions { get; set; }
        public List<EvaluatedConditionDetail> Conditions { get; set; } = [];
    }

    public class EvaluatedConditionDetail
    {
        public int ConditionGroup { get; set; }
        public string Attribute { get; set; } = null!;
        public string Operator { get; set; } = null!;
        public string Value { get; set; } = null!;
        public string? ResolvedLeftValue { get; set; }
        public string? ResolvedRightValue { get; set; }
        public bool Passed { get; set; }
    }

    public class PbacListFilterResult<T> where T : class
    {
        /// <summary>
        /// True if PBAC policies exist for this module+action, meaning the caller should trust
        /// <see cref="Filter"/> exclusively and NOT fall back to any legacy RBAC-based row scoping.
        /// False means no PBAC policies exist at all - the caller should apply its own fallback scoping.
        /// </summary>
        public bool PbacGoverned { get; set; }

        /// <summary>
        /// The row filter to apply. Null means "no filtering needed" when <see cref="PbacGoverned"/> is
        /// true (an unrestricted Allow policy matched), or "not applicable" when it's false.
        /// </summary>
        public Expression<Func<T, bool>>? Filter { get; set; }
    }

    public interface IPolicyEvaluationService
    {
        /// <summary>
        /// Evaluates all applicable access policies for a given user, module, and action.
        /// </summary>
        Task<PolicyEvaluationResult> EvaluateAsync(
            Guid userId,
            string moduleCode,
            string action,
            object? resourceContext = null);

        /// <summary>
        /// Builds and returns an expression representing the row filter to be applied to a query.
        /// Returns null if no row filter is applicable or required. Prefer <see cref="GetListFilterAsync{T}"/>
        /// for new call sites - it combines the "does PBAC govern this?" check and the filter build into
        /// a single evaluation pass instead of two.
        /// </summary>
        Task<Expression<Func<T, bool>>?> GetRowFilterExpressionAsync<T>(
            Guid userId,
            string moduleCode,
            string action) where T : class;

        /// <summary>
        /// Single-pass combination of the "does PBAC govern this module+action for this user" check and
        /// the row filter expression build, for list/paginated query handlers.
        /// </summary>
        Task<PbacListFilterResult<T>> GetListFilterAsync<T>(
            Guid userId,
            string moduleCode,
            string action) where T : class;

        /// <summary>
        /// Performs a step-by-step dry run evaluation for administrators to test policies.
        /// </summary>
        Task<PolicyPreviewResult> PreviewEvaluationAsync(
            Guid userId,
            string moduleCode,
            string action,
            string? resourceContextJson = null);

        /// <summary>
        /// Evaluates the same user + module + action against many resource contexts in one call,
        /// fetching the user and the policy cache exactly once instead of once per resource. Use this
        /// instead of looping <see cref="EvaluateAsync"/> when checking many candidate resources
        /// (e.g. "is each permission in the system manageable by this user").
        /// </summary>
        Task<List<PolicyEvaluationResult>> EvaluateBatchAsync(
            Guid userId,
            string moduleCode,
            string action,
            IEnumerable<object?> resourceContexts);
    }
}
