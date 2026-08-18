namespace WMS.Application.Common.Interfaces
{
    /// <summary>
    /// Provides cached permission lookups for a given user.
    /// Implementations should use IMemoryCache to avoid hitting the DB
    /// on every permission check within the cache window.
    /// </summary>
    public interface IPermissionCacheService
    {
        /// <summary>
        /// Gets the list of permission codes for the specified user.
        /// Results are cached in-memory for a configurable duration.
        /// </summary>
        Task<List<string>> GetPermissionsAsync(Guid userId);

        /// <summary>
        /// Invalidates the cached permissions for a specific user.
        /// Call this when a user's roles or permissions change.
        /// </summary>
        void InvalidateUser(Guid userId);

        /// <summary>
        /// Asynchronously invalidates the cached permissions for a specific user.
        /// </summary>
        Task InvalidateUserAsync(Guid userId);
    }
}
