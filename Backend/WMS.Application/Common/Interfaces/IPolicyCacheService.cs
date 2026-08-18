using WMS.Application.Features.Policies.DTOs;

namespace WMS.Application.Common.Interfaces
{
    /// <summary>
    /// Provides cached lookup of active Access Policies for a given user.
    /// Uses Redis IDistributedCache internally.
    /// </summary>
    public interface IPolicyCacheService
    {
        /// <summary>
        /// Gets all active access policies applicable to the user (based on their roles and user ID).
        /// </summary>
        Task<List<AccessPolicyDto>> GetPoliciesForUserAsync(Guid userId);

        /// <summary>
        /// Invalidates the policy cache for a specific user.
        /// </summary>
        Task InvalidateUserPoliciesAsync(Guid userId);

        /// <summary>
        /// Invalidates the policy cache for all users of a specific role.
        /// </summary>
        Task InvalidateRolePoliciesAsync(Guid roleId);

        /// <summary>
        /// Invalidates all policy caches.
        /// </summary>
        Task InvalidateAllPoliciesAsync();
    }
}
