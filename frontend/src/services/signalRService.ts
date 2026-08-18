import * as signalR from '@microsoft/signalr';
import { useAuthStore } from '@/store/authStore';
import { usePermissionStore } from '@/store/permissionStore';

/**
 * Singleton SignalR connection manager.
 *
 * Manages the lifecycle of a single HubConnection to `/hubs/wms`.
 * Automatically handles:
 *   - JWT authentication via access_token query param
 *   - Auto-reconnect with exponential backoff
 *   - Re-joining role groups after reconnect
 *   - Updating Zustand permission store on server push
 */

let connection: signalR.HubConnection | null = null;
let isStarting = false;

/** Returns the active hub connection (or null if not started). */
export function getConnection(): signalR.HubConnection | null {
  return connection;
}

/**
 * Start (or re-use) the SignalR connection.
 * Safe to call multiple times — will no-op if already connected or connecting.
 */
export async function startSignalR(): Promise<void> {
  // If connection exists and is not disconnected, or is currently starting, no-op
  if (connection && connection.state !== signalR.HubConnectionState.Disconnected) {
    return;
  }
  if (isStarting) return;

  const token = useAuthStore.getState().accessToken;
  if (!token) return;

  isStarting = true;

  try {
    if (!connection) {
      connection = new signalR.HubConnectionBuilder()
        .withUrl('/hubs/wms', {
          accessTokenFactory: () => {
            return useAuthStore.getState().accessToken ?? '';
          },
        })
        .withAutomaticReconnect([0, 2_000, 5_000, 10_000, 30_000])
        .configureLogging(signalR.LogLevel.Warning)
        .build();

      // ── Server → Client event handlers ──────────────────────────────

      /**
       * "PermissionsUpdated" — server pushes the full recomputed string[]
       * whenever a user's effective permissions change.
       */
      connection.on('PermissionsUpdated', (permissions: string[]) => {
        usePermissionStore.getState().setPermissions(permissions);
      });

      /**
       * "RolePermissionsChanged" — informational broadcast to the role group.
       */
      connection.on('RolePermissionsChanged', () => {
        // Reserved for role-level events if needed
      });

      // ── Lifecycle events ────────────────────────────────────────────

      connection.onreconnected(async () => {
        await joinRoleGroups();
      });

      connection.onclose(() => {
        // Connection closed callback
      });
    }

    if (connection.state === signalR.HubConnectionState.Disconnected) {
      await connection.start();
      await joinRoleGroups();
    }
  } catch {
    // Connection attempt failed
  } finally {
    isStarting = false;
  }
}

/**
 * Gracefully stop the SignalR connection.
 * Called on user logout.
 */
export async function stopSignalR(): Promise<void> {
  if (connection) {
    try {
      if (connection.state !== signalR.HubConnectionState.Disconnected) {
        await connection.stop();
      }
    } catch {
      // swallow — connection may already be closed
    }
    connection = null;
  }
}

/**
 * Join the SignalR groups for the user's current roles.
 */
async function joinRoleGroups(): Promise<void> {
  if (!connection || connection.state !== signalR.HubConnectionState.Connected) return;

  const roles = usePermissionStore.getState().roles;
  for (const roleId of roles) {
    try {
      await connection.invoke('JoinRoleGroup', roleId);
    } catch {
      // Failed to join role group
    }
  }
}
