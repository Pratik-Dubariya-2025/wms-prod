using WMS.Application.Common.Interfaces;
using WMS.Domain.Interfaces;
using WMS.Domain.Models;

namespace WMS.Application.Features.Projects
{
    /// <summary>
    /// Centralized row-level authorization for project-scoped operations.
    ///
    /// Only <c>ADMIN</c> has unrestricted, cross-project access. Every other role —
    /// including <c>MANAGER</c> — is scoped to the projects they actually belong to:
    /// a project they Own, a project they lead (TeamLead), or a project they are an
    /// active Member of. A MANAGER is NOT a global super-user; one manager must never
    /// see or modify another manager's project, its members, or its tasks.
    /// </summary>
    internal static class ProjectAccessAuthorizer
    {
        public const string RoleAdmin = "ADMIN";

        /// <summary>True if the user has unrestricted, cross-project access.</summary>
        public static bool IsGlobalAdmin(ICurrentUserService user) =>
            user.Roles.Contains(RoleAdmin);

        /// <summary>
        /// May the user <i>manage</i> the project — i.e. edit/delete the project itself,
        /// add/remove its members, or create/edit/delete its tasks? Limited to ADMIN and
        /// the project Owner or Team Lead (ordinary members cannot manage).
        /// </summary>
        public static bool CanManage(Project project, ICurrentUserService user)
        {
            if (!user.UserId.HasValue)
            {
                return false;
            }
            if (IsGlobalAdmin(user))
            {
                return true;
            }

            var userId = user.UserId.Value;
            return project.OwnerId == userId || project.TeamLeadId == userId;
        }

        /// <summary>
        /// May the user <i>view</i> the project? ADMIN, the Owner/Team Lead, or any active
        /// project member. Performs a membership lookup only when needed.
        /// </summary>
        public static async Task<bool> CanAccessAsync(
            Project project,
            IUnitOfWork unitOfWork,
            ICurrentUserService user)
        {
            if (CanManage(project, user))
            {
                return true;
            }
            if (!user.UserId.HasValue)
            {
                return false;
            }

            var userId = user.UserId.Value;
            var membership = await unitOfWork.ProjectMember.GetFirstOrDefaultAsync(
                pm => pm.ProjectId == project.Id && pm.UserId == userId && !pm.IsDeleted);
            return membership != null;
        }
    }
}
