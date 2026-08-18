import { usePermissionStore } from '@/store/permissionStore';

/**
 * Hook to check if the current user has specific permissions.
 */
export function usePermissions() {
  const { permissions, roles, hasPermission, isInRole } = usePermissionStore();

  /** Check if user has ALL of the given permissions */
  function hasAllPermissions(codes: string[]): boolean {
    return codes.every((code) => permissions.includes(code));
  }

  /** Check if user has ANY of the given permissions */
  function hasAnyPermission(codes: string[]): boolean {
    return codes.some((code) => permissions.includes(code));
  }

  return {
    permissions,
    roles,
    hasPermission,
    hasAllPermissions,
    hasAnyPermission,
    isInRole,
  };
}
