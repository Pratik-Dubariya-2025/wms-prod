namespace WMS.Application.Common.Interfaces
{
    /// <summary>
    /// Resolves the "reports to me" org-chart hierarchy for PBAC row-level scoping
    /// (e.g. a Dept Manager's policy row filter referencing "{user.SubordinateIds}").
    /// Backed by a recursive query over User.ManagerId, cached since org charts change rarely.
    /// </summary>
    public interface IUserHierarchyService
    {
        /// <summary>
        /// Transitive set of every user who reports up to <paramref name="managerId"/> via ManagerId,
        /// direct or indirect, excluding the manager themselves.
        /// </summary>
        Task<IReadOnlySet<Guid>> GetSubordinateIdsAsync(Guid managerId);

        /// <summary>
        /// Transitive set of every user in <paramref name="officerId"/>'s day-to-day reporting chain
        /// via ReportingOfficerId, direct or indirect, excluding the officer themselves. Use this
        /// instead of <see cref="GetSubordinateIdsAsync"/> when ManagerId is a flat/org-wide field in
        /// your data and ReportingOfficerId is what actually carries the tiered hierarchy.
        /// </summary>
        Task<IReadOnlySet<Guid>> GetReportingTeamIdsAsync(Guid officerId);

        /// <summary>
        /// Invalidates all cached hierarchy lookups (both ManagerId- and ReportingOfficerId-based).
        /// Call whenever any User.ManagerId or User.ReportingOfficerId changes - a single coarse
        /// invalidation is preferred over tracking which specific managers/ancestors are affected,
        /// since org reassignments are rare and this keeps the cache trivially correct.
        /// </summary>
        Task InvalidateAsync();
    }
}
