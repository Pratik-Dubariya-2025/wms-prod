import { create } from 'zustand';

interface PermissionState {
  /** User's loaded permission codes */
  permissions: string[];
  /** User's loaded role names */
  roles: string[];

  /** Replace all permissions (e.g., on login or SignalR sync) */
  setPermissions: (permissions: string[]) => void;
  /** Replace all roles */
  setRoles: (roles: string[]) => void;
  /** Check if user has a specific permission */
  hasPermission: (code: string) => boolean;
  /** Check if user is in a specific role */
  isInRole: (role: string) => boolean;
  /** Clear on logout */
  clearPermissions: () => void;
}

export const usePermissionStore = create<PermissionState>()((set, get) => ({
  permissions: [],
  roles: [],

  setPermissions: (permissions) => set({ permissions }),
  setRoles: (roles) => set({ roles }),

  hasPermission: (code) => get().permissions.some(p => p.toLowerCase() === code.toLowerCase()),
  isInRole: (role) => get().roles.some(r => r.toLowerCase() === role.toLowerCase()),

  clearPermissions: () => set({ permissions: [], roles: [] }),
}));
