using Microsoft.AspNetCore.SignalR;
using WMS.API.Hubs;
using WMS.Application.Common.Interfaces;
using WMS.Domain.Interfaces;

namespace WMS.API.Services
{
    /// <summary>
    /// SignalR-backed implementation of <see cref="IRealtimeNotificationService"/>.
    /// Recomputes user permissions from the database/cache and pushes them
    /// to the affected clients over WebSocket.
    /// </summary>
    public class SignalRNotificationService(
        IHubContext<WmsHub> hubContext,
        IServiceScopeFactory scopeFactory) : IRealtimeNotificationService
    {
        private readonly IHubContext<WmsHub> _hubContext = hubContext;
        private readonly IServiceScopeFactory _scopeFactory = scopeFactory;

        /// <inheritdoc />
        public async Task NotifyRolePermissionsChangedAsync(Guid roleId, CancellationToken ct = default)
        {
            using var scope = _scopeFactory.CreateScope();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            var permissionCache = scope.ServiceProvider.GetRequiredService<IPermissionCacheService>();

            // Find every user that belongs to this role
            var userIds = await unitOfWork.UserRole.GetAllAsync(
                select: ur => ur.UserId,
                where: ur => ur.RoleId == roleId);

            // For each affected user: invalidate cache, recompute, push
            foreach (var userId in userIds)
            {
                permissionCache.InvalidateUser(userId);
                var permissions = await permissionCache.GetPermissionsAsync(userId);

                var targetUserId = userId.ToString().ToLowerInvariant();

                await _hubContext.Clients
                    .User(targetUserId)
                    .SendAsync("PermissionsUpdated", permissions, ct);
            }

            // Also broadcast a lightweight event to the role group
            await _hubContext.Clients
                .Group($"role:{roleId}")
                .SendAsync("RolePermissionsChanged", roleId, ct);
        }

        /// <inheritdoc />
        public async Task NotifyUserPermissionsChangedAsync(Guid userId, CancellationToken ct = default)
        {
            using var scope = _scopeFactory.CreateScope();
            var permissionCache = scope.ServiceProvider.GetRequiredService<IPermissionCacheService>();

            permissionCache.InvalidateUser(userId);
            var permissions = await permissionCache.GetPermissionsAsync(userId);

            var targetUserId = userId.ToString().ToLowerInvariant();

            await _hubContext.Clients
                .User(targetUserId)
                .SendAsync("PermissionsUpdated", permissions, ct);
        }
    }
}
