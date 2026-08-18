using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using WMS.Application.Common.Interfaces;
using WMS.Domain.Interfaces;

namespace WMS.Infrastructure.Identity
{
    /// <summary>
    /// Caches user permissions in Redis IDistributedCache to avoid DB hits on every request.
    /// Cache key: "user_permissions_{userId}"
    /// Default TTL: 5 minutes (absolute expiration).
    /// </summary>
    public class PermissionCacheService(IDistributedCache cache, IUnitOfWork unitOfWork) : IPermissionCacheService
    {
        private readonly IDistributedCache _cache = cache;
        private readonly IUnitOfWork _unitOfWork = unitOfWork;

        private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(5);

        public async Task<List<string>> GetPermissionsAsync(Guid userId)
        {
            string cacheKey = GetCacheKey(userId);

            string? cachedJson = await _cache.GetStringAsync(cacheKey);
            if (!string.IsNullOrEmpty(cachedJson))
            {
                var cachedPermissions = JsonSerializer.Deserialize<List<string>>(cachedJson);
                if (cachedPermissions != null)
                {
                    return cachedPermissions;
                }
            }

            // Cache miss — query DB
            List<string> rolePermissions = await _unitOfWork.RolePermission.GetAllAsync(
                s => s.Permission.Code,
                s => !s.Role.IsDeleted && s.Role.UserRoles.Any(ur => ur.UserId == userId));

            var overrides = await _unitOfWork.UserPermissionOverride.IncludeAndGetAllAsync(
                o => o.UserId == userId && !o.IsDeleted && (o.ExpiresAt == null || o.ExpiresAt > DateTime.UtcNow),
                o => o.Permission
            );

            var deniedPermissions = overrides
                .Where(o => o.Permission != null && !o.IsGranted)
                .Select(o => o.Permission.Code)
                .ToList();

            var grantedPermissions = overrides
                .Where(o => o.Permission != null && o.IsGranted)
                .Select(o => o.Permission.Code)
                .ToList();

            // Fetch user roles and active policies to integrate PBAC permissions
            var userRoleIds = await _unitOfWork.UserRole.GetAllAsync(
                ur => ur.RoleId,
                ur => ur.UserId == userId
            );

            var activePolicies = await _unitOfWork.AccessPolicy.GetAllAsync(
                p => !p.IsDeleted && p.IsActive
                    && (p.ExpiresAt == null || p.ExpiresAt > DateTime.UtcNow)
                    && (p.UserId == userId || (p.RoleId != null && userRoleIds.Contains(p.RoleId.Value)))
            );

            var allPermissions = await _unitOfWork.Permission.GetAllAsync(p => !p.IsDeleted);
            var policyAllowedCodes = new List<string>();
            var policyDeniedCodes = new List<string>();

            foreach (var policy in activePolicies)
            {
                var matchedPermission = allPermissions.FirstOrDefault(p =>
                    p.ModuleId == policy.ModuleId &&
                    string.Equals(p.Action, policy.Action, StringComparison.OrdinalIgnoreCase));

                if (matchedPermission != null)
                {
                    if (string.Equals(policy.Effect, "Allow", StringComparison.OrdinalIgnoreCase))
                    {
                        policyAllowedCodes.Add(matchedPermission.Code);
                    }
                    else if (string.Equals(policy.Effect, "Deny", StringComparison.OrdinalIgnoreCase))
                    {
                        policyDeniedCodes.Add(matchedPermission.Code);
                    }
                }
            }

            List<string> permissions = rolePermissions
                .Except(deniedPermissions, StringComparer.OrdinalIgnoreCase)
                .Union(grantedPermissions, StringComparer.OrdinalIgnoreCase)
                .Union(policyAllowedCodes, StringComparer.OrdinalIgnoreCase)
                .Except(policyDeniedCodes, StringComparer.OrdinalIgnoreCase)
                .ToList();

            // Store in cache with absolute expiration.
            var absoluteExpiration = DateTimeOffset.UtcNow.Add(CacheDuration);

            // Find the earliest active override or policy expiration time
            var earliestExpiration = overrides
                .Where(o => o.ExpiresAt.HasValue && o.ExpiresAt.Value > DateTime.UtcNow)
                .Select(o => o.ExpiresAt!.Value)
                .Concat(activePolicies
                    .Where(p => p.ExpiresAt.HasValue && p.ExpiresAt.Value > DateTime.UtcNow)
                    .Select(p => p.ExpiresAt!.Value))
                .OrderBy(dt => dt)
                .FirstOrDefault();

            if (earliestExpiration != default)
            {
                var earliestOffset = new DateTimeOffset(DateTime.SpecifyKind(earliestExpiration, DateTimeKind.Utc));
                if (earliestOffset < absoluteExpiration)
                {
                    absoluteExpiration = earliestOffset > DateTimeOffset.UtcNow ? earliestOffset : DateTimeOffset.UtcNow;
                }
            }

            var cacheOptions = new DistributedCacheEntryOptions
            {
                AbsoluteExpiration = absoluteExpiration
            };

            await _cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(permissions), cacheOptions);

            return permissions;
        }

        public void InvalidateUser(Guid userId)
        {
            _cache.Remove(GetCacheKey(userId));
        }

        public async Task InvalidateUserAsync(Guid userId)
        {
            await _cache.RemoveAsync(GetCacheKey(userId));
        }

        private static string GetCacheKey(Guid userId) => $"user_permissions_{userId.ToString().ToLowerInvariant()}";
    }
}
