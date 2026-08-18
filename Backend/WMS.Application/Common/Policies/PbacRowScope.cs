using System.Linq.Expressions;
using WMS.Application.Common.Interfaces;

namespace WMS.Application.Common.Policies
{
    public readonly record struct PbacRowScopeResult<T>(bool UsePbac, Expression<Func<T, bool>>? Filter) where T : class;

    /// <summary>
    /// Shared entry point for wiring PBAC row-level scoping into a list query handler. Every handler
    /// for a module registered in <see cref="IPolicyResourceRegistry"/> should resolve its filter
    /// through this instead of calling <c>IPolicyEvaluationService.GetListFilterAsync</c> directly -
    /// it's the one place documented in the PBAC coverage test (see PbacCoverageTests), so a module
    /// missing row scoping fails a test instead of shipping silently.
    /// </summary>
    public static class PbacRowScope
    {
        /// <summary>
        /// Resolves the PBAC row filter for a module+action. <see cref="PbacRowScopeResult{T}.UsePbac"/>
        /// is true when PBAC governs this module+action for the user - callers MUST NOT apply their own
        /// legacy/RBAC fallback scoping in that case, even if <see cref="PbacRowScopeResult{T}.Filter"/>
        /// is null (null with UsePbac=true means an unrestricted Allow policy matched).
        /// </summary>
        public static async Task<PbacRowScopeResult<T>> ResolveAsync<T>(
            IPolicyEvaluationService policyEvaluationService,
            Guid? currentUserId,
            string moduleCode,
            string action) where T : class
        {
            if (!currentUserId.HasValue)
            {
                return new PbacRowScopeResult<T>(false, null);
            }

            var listFilter = await policyEvaluationService.GetListFilterAsync<T>(currentUserId.Value, moduleCode, action);
            return new PbacRowScopeResult<T>(listFilter.PbacGoverned, listFilter.Filter);
        }
    }
}
