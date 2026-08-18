using WMS.Domain.Models;

namespace WMS.Domain.Interfaces.Repositories
{
    public interface IUserRepository : IRepository<User>
    {
        Task<User?> GetWithRolesAndPermissionsAsync(Guid userId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Active users at the highest designation level in the department (the
        /// department heads / managers). The filtering and MAX(level) computation
        /// are pushed to SQL so only the eligible rows are materialised.
        /// </summary>
        Task<List<User>> GetEligibleManagersAsync(
            Guid departmentId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Active users in the department that report to the given manager and
        /// outrank the invited designation level — i.e. valid reporting officers.
        /// </summary>
        Task<List<User>> GetEligibleReportingOfficersAsync(
            Guid departmentId, int invitedDesignationLevel, Guid managerId,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Transitive set of every user who reports up to <paramref name="managerId"/> through
        /// the ManagerId chain (direct and indirect reports), computed via a recursive CTE so it
        /// scales to any org depth without loading the hierarchy into memory. Excludes the manager
        /// themselves.
        /// </summary>
        Task<List<Guid>> GetSubordinateIdsAsync(Guid managerId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Transitive set of every user in <paramref name="officerId"/>'s day-to-day reporting chain
        /// through ReportingOfficerId (direct and indirect), excluding the officer themselves. Distinct
        /// from <see cref="GetSubordinateIdsAsync"/>: in orgs where ManagerId is a flat, org-wide field
        /// (e.g. one Manager for a whole department) but ReportingOfficerId carries the real tiered
        /// day-to-day hierarchy (ASE -> TL/SSE -> Manager), this is the one that reflects "who actually
        /// reports to me."
        /// </summary>
        Task<List<Guid>> GetReportingTeamIdsAsync(Guid officerId, CancellationToken cancellationToken = default);

        Task<(Guid? UserId, int ErrorCode)> InviteUserViaSpAsync(
            string employeeCode,
            string firstName,
            string lastName,
            string email,
            string username,
            string passwordHash,
            string? phoneNumber,
            Guid departmentId,
            Guid designationId,
            string createdBy,
            List<Guid> roleIds,
            Guid currentUserId,
            Guid? managerId,
            Guid? reportingOfficerId,
            CancellationToken cancellationToken = default);
    }
}
