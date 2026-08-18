using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using WMS.Application.Common.Interfaces;
using WMS.Application.Features.Policies.DTOs;
using WMS.Domain.Interfaces;

namespace WMS.Infrastructure.Identity
{
    /// <summary>
    /// Implementation of IPolicyCacheService using Redis IDistributedCache.
    /// Caches the active access policies for users to bypass DB reads on policy evaluations.
    /// Redis is treated as a pure optimization: any failure talking to it is logged and swallowed,
    /// falling back to a direct DB read, since PolicyEvaluationBehaviour runs on every
    /// [RequirePermission] request app-wide - a Redis outage must not 500 every protected endpoint.
    /// </summary>
    public class PolicyCacheService(
        IDistributedCache cache,
        IUnitOfWork unitOfWork,
        ILogger<PolicyCacheService> logger) : IPolicyCacheService
    {
        private readonly IDistributedCache _cache = cache;
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly ILogger<PolicyCacheService> _logger = logger;
        private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(5);

        public async Task<List<AccessPolicyDto>> GetPoliciesForUserAsync(Guid userId)
        {
            string cacheKey = GetCacheKey(userId);

            string? cachedJson = null;
            try
            {
                cachedJson = await _cache.GetStringAsync(cacheKey);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Policy cache read failed for {CacheKey}; falling back to database.", cacheKey);
            }

            if (!string.IsNullOrEmpty(cachedJson))
            {
                var cachedPolicies = JsonSerializer.Deserialize<List<AccessPolicyDto>>(cachedJson);
                if (cachedPolicies != null)
                {
                    return cachedPolicies;
                }
            }

            // Cache miss (or cache unavailable) - Fetch active roles of the user
            var userRoleIds = await _unitOfWork.UserRole.GetAllAsync(
                ur => ur.RoleId,
                ur => ur.UserId == userId
            );

            // Fetch active policies targeting either this user ID or any of their roles
            var policies = await _unitOfWork.AccessPolicy.IncludeAndGetAllAsync(
                p => !p.IsDeleted && p.IsActive
                    && (p.ExpiresAt == null || p.ExpiresAt > DateTime.UtcNow)
                    && (p.UserId == userId || (p.RoleId != null && userRoleIds.Contains(p.RoleId.Value))),
                p => p.Module,
                p => p.Role!,
                p => p.User!,
                p => p.Conditions,
                p => p.FieldRestrictions
            );

            // Map to DTOs for serialization
            var policyDtos = policies
                .OrderBy(p => p.Priority)
                .ThenBy(p => p.Name)
                .Select(p => new AccessPolicyDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Description = p.Description,
                    ModuleId = p.ModuleId,
                    ModuleName = p.Module?.Name ?? "",
                    ModuleCode = p.Module?.Code ?? "",
                    Action = p.Action,
                    Effect = p.Effect,
                    RoleId = p.RoleId,
                    RoleName = p.Role?.Name,
                    UserId = p.UserId,
                    UserName = p.User != null ? $"{p.User.FirstName} {p.User.LastName}" : null,
                    RowFilterExpression = p.RowFilterExpression,
                    Priority = p.Priority,
                    IsActive = p.IsActive,
                    ExpiresAt = p.ExpiresAt,
                    CreatedAt = p.CreatedAt,
                    Conditions = p.Conditions
                        .Where(c => !c.IsDeleted)
                        .OrderBy(c => c.ConditionGroup)
                        .Select(c => new PolicyConditionDto
                        {
                            Id = c.Id,
                            ConditionGroup = c.ConditionGroup,
                            Attribute = c.Attribute,
                            Operator = c.Operator,
                            Value = c.Value
                        }).ToList(),
                    FieldRestrictions = p.FieldRestrictions
                        .Where(f => !f.IsDeleted)
                        .Select(f => new FieldRestrictionDto
                        {
                            Id = f.Id,
                            FieldName = f.FieldName,
                            RestrictionType = f.RestrictionType,
                            MaskPattern = f.MaskPattern
                        }).ToList()
                })
                .ToList();

            // Set Redis cache expiration
            var absoluteExpiration = DateTimeOffset.UtcNow.Add(CacheDuration);

            // If any policy expires sooner, bound the cache TTL by it
            var earliestPolicyExpiration = policies
                .Where(p => p.ExpiresAt.HasValue && p.ExpiresAt.Value > DateTime.UtcNow)
                .Select(p => p.ExpiresAt!.Value)
                .OrderBy(dt => dt)
                .FirstOrDefault();

            if (earliestPolicyExpiration != default)
            {
                var earliestOffset = new DateTimeOffset(DateTime.SpecifyKind(earliestPolicyExpiration, DateTimeKind.Utc));
                if (earliestOffset < absoluteExpiration)
                {
                    absoluteExpiration = earliestOffset > DateTimeOffset.UtcNow ? earliestOffset : DateTimeOffset.UtcNow;
                }
            }

            var cacheOptions = new DistributedCacheEntryOptions
            {
                AbsoluteExpiration = absoluteExpiration
            };

            try
            {
                await _cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(policyDtos), cacheOptions);
            }
            catch (Exception ex)
            {
                // Best-effort: if Redis is unavailable we already have the correct data from the DB above,
                // just serve it without caching this time.
                _logger.LogWarning(ex, "Policy cache write failed for {CacheKey}; continuing without caching.", cacheKey);
            }

            return policyDtos;
        }

        public async Task InvalidateUserPoliciesAsync(Guid userId)
        {
            try
            {
                await _cache.RemoveAsync(GetCacheKey(userId));
            }
            catch (Exception ex)
            {
                // Worst case the cache entry expires naturally within CacheDuration; don't let a Redis
                // outage break the policy write operation that triggered this invalidation.
                _logger.LogWarning(ex, "Policy cache invalidation failed for user {UserId}.", userId);
            }
        }

        public async Task InvalidateRolePoliciesAsync(Guid roleId)
        {
            // Simple approach: since we don't track all user IDs mapped to a role directly in a fast way without DB query,
            // we can retrieve user IDs belonging to this role and invalidate them.
            var userIds = await _unitOfWork.UserRole.GetAllAsync(
                ur => ur.UserId,
                ur => ur.RoleId == roleId
            );

            foreach (var userId in userIds)
            {
                await InvalidateUserPoliciesAsync(userId);
            }
        }

        public async Task InvalidateAllPoliciesAsync()
        {
            // In distributed systems we typically delete matching keys. For simplicity here,
            // we can fetch all users who might have active policies, or let cache expire.
            // But we can query all user IDs in the system to invalidate cache entries if needed,
            // or we can use Redis database flush/pattern matching if available.
            // Let's retrieve all active users and invalidate them.
            var userIds = await _unitOfWork.User.GetAllAsync(
                u => u.Id,
                u => !u.IsDeleted
            );

            foreach (var userId in userIds)
            {
                await InvalidateUserPoliciesAsync(userId);
            }
        }

        private static string GetCacheKey(Guid userId) => $"user_policies_{userId.ToString().ToLowerInvariant()}";
    }
}
