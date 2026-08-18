using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using WMS.Application.Common.Interfaces;

namespace WMS.Infrastructure.Identity
{
    public class CurrentUserService(
        IHttpContextAccessor httpContextAccessor,
        IPermissionCacheService permissionCacheService) : ICurrentUserService
    {
        private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;
        private readonly IPermissionCacheService _permissionCacheService = permissionCacheService;

        private ClaimsPrincipal? User => _httpContextAccessor.HttpContext?.User;

        /// <summary>
        /// Gets the current authenticated user's ID from the JWT sub/NameIdentifier claim.
        /// </summary>
        public Guid? UserId
        {
            get
            {
                string? userId = User?.FindFirstValue(ClaimTypes.NameIdentifier);
                return userId != null ? Guid.Parse(userId) : null;
            }
        }

        /// <summary>
        /// Gets the current user's username from the Name claim.
        /// </summary>
        public string? Username => User?.FindFirstValue(ClaimTypes.Name);

        /// <summary>
        /// Gets the current user's email from the Email claim.
        /// </summary>
        public string? Email => User?.FindFirstValue(ClaimTypes.Email);

        /// <summary>
        /// Gets the current user's employee code from the custom EmployeeCode claim.
        /// </summary>
        public string? EmployeeCode => User?.FindFirstValue("EmployeeCode");

        /// <summary>
        /// Gets ALL roles assigned to the current user.
        /// Supports multi-role: returns all ClaimTypes.Role values.
        /// </summary>
        public IEnumerable<string> Roles =>
            User?.FindAll(ClaimTypes.Role).Select(c => c.Value) ?? [];

        /// <summary>
        /// Gets ALL permissions for the current user from the IMemoryCache-backed service.
        /// Permissions are fetched from DB on cache miss and cached for 5 minutes.
        /// </summary>
        public IEnumerable<string> Permissions
        {
            get
            {
                if (UserId == null) return [];
                return _permissionCacheService.GetPermissionsAsync(UserId.Value)
                    .GetAwaiter().GetResult();
            }
        }

        /// <summary>
        /// Returns true if the current HTTP context has an authenticated user identity.
        /// </summary>
        public bool IsAuthenticated =>
            User?.Identity?.IsAuthenticated ?? false;

        /// <summary>
        /// Checks if the current user has a specific role.
        /// Case-insensitive comparison.
        /// </summary>
        public bool IsInRole(string role) =>
            Roles.Any(r => r.Equals(role, StringComparison.OrdinalIgnoreCase));

        /// <summary>
        /// Checks if the current user has a specific permission code.
        /// Example: HasPermission("user.create")
        /// Case-insensitive comparison.
        /// </summary>
        public bool HasPermission(string permissionCode) =>
            Permissions.Any(p => p.Equals(permissionCode, StringComparison.OrdinalIgnoreCase));
    }
}
