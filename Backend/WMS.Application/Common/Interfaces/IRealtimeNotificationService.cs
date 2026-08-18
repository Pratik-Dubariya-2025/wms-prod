namespace WMS.Application.Common.Interfaces
{
    /// <summary>
    /// Abstraction for pushing real-time notifications to connected clients.
    /// The Application layer uses this interface without any knowledge of SignalR.
    /// Implementations live in the API/Infrastructure layer.
    /// </summary>
    public interface IRealtimeNotificationService
    {
        /// <summary>
        /// Recomputes and pushes updated permissions to every user
        /// that belongs to the specified role.
        /// </summary>
        Task NotifyRolePermissionsChangedAsync(Guid roleId, CancellationToken ct = default);

        /// <summary>
        /// Recomputes and pushes updated permissions to a specific user.
        /// Used when a user's role assignment or permission override changes.
        /// </summary>
        Task NotifyUserPermissionsChangedAsync(Guid userId, CancellationToken ct = default);
    }
}
