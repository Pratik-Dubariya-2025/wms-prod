using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using WMS.Application.Common.Interfaces;
using WMS.Domain.Interfaces;

namespace WMS.Infrastructure.Identity
{
    /// <summary>
    /// Caches hierarchy-id lookups in Redis under versioned keys
    /// ("subordinates_v{version}_{id}" / "reporting_team_v{version}_{id}"). Invalidation bumps a
    /// single global version counter instead of enumerating/deleting affected keys - stale entries
    /// simply age out of the version bucket and expire on their own TTL.
    /// </summary>
    public class UserHierarchyService(IDistributedCache cache, IUnitOfWork unitOfWork) : IUserHierarchyService
    {
        private readonly IDistributedCache _cache = cache;
        private readonly IUnitOfWork _unitOfWork = unitOfWork;

        private const string VersionKey = "user_hierarchy_version";
        private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(15);

        public Task<IReadOnlySet<Guid>> GetSubordinateIdsAsync(Guid managerId) =>
            GetCachedHierarchyAsync("subordinates", managerId, () => _unitOfWork.User.GetSubordinateIdsAsync(managerId));

        public Task<IReadOnlySet<Guid>> GetReportingTeamIdsAsync(Guid officerId) =>
            GetCachedHierarchyAsync("reporting_team", officerId, () => _unitOfWork.User.GetReportingTeamIdsAsync(officerId));

        private async Task<IReadOnlySet<Guid>> GetCachedHierarchyAsync(string cachePrefix, Guid rootId, Func<Task<List<Guid>>> load)
        {
            var version = await GetVersionAsync();
            var cacheKey = GetCacheKey(cachePrefix, rootId, version);

            var cachedJson = await _cache.GetStringAsync(cacheKey);
            if (!string.IsNullOrEmpty(cachedJson))
            {
                var cached = JsonSerializer.Deserialize<List<Guid>>(cachedJson);
                if (cached != null)
                {
                    return new HashSet<Guid>(cached);
                }
            }

            var ids = await load();

            await _cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(ids), new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = CacheDuration
            });

            return new HashSet<Guid>(ids);
        }

        public async Task InvalidateAsync()
        {
            var version = await GetVersionAsync();
            await _cache.SetStringAsync(VersionKey, (version + 1).ToString());
        }

        private async Task<long> GetVersionAsync()
        {
            var raw = await _cache.GetStringAsync(VersionKey);
            return raw != null && long.TryParse(raw, out var v) ? v : 0;
        }

        private static string GetCacheKey(string prefix, Guid rootId, long version) =>
            $"{prefix}_v{version}_{rootId.ToString().ToLowerInvariant()}";
    }
}
