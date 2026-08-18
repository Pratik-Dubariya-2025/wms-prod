using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace WMS.API.Hubs
{
    /// <summary>
    /// Central SignalR hub for WMS real-time communication.
    /// Currently handles permission synchronisation; designed to be
    /// extended for future features (notifications, task updates, etc.).
    /// 
    /// Client events sent by this hub:
    ///   • "PermissionsUpdated"  — string[] of recomputed permission codes
    ///   • "RolePermissionsChanged" — Guid roleId (informational)
    /// </summary>
    [Authorize]
    public class WmsHub : Hub
    {
        /// <summary>
        /// Client calls this after connecting to subscribe to a role group.
        /// All users sharing the same role receive role-level broadcasts.
        /// </summary>
        public async Task JoinRoleGroup(string roleId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"role:{roleId}");
        }

        /// <summary>
        /// Client calls this to unsubscribe from a role group
        /// (e.g., when a role is removed from the user).
        /// </summary>
        public async Task LeaveRoleGroup(string roleId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"role:{roleId}");
        }

        public override async Task OnConnectedAsync()
        {
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            await base.OnDisconnectedAsync(exception);
        }
    }
}
