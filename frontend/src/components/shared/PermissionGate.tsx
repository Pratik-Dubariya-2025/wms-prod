import type { ReactNode } from 'react';
import { usePermissions } from '@/hooks/usePermissions';

interface PermissionGateProps {
  /** Required permission code(s) */
  permissions: string | string[];
  /** If true, user needs ALL permissions. If false, ANY will suffice. Default: false */
  requireAll?: boolean;
  /** Content to show if permission check passes */
  children: ReactNode;
  /** Optional fallback when user lacks permission */
  fallback?: ReactNode;
}

/**
 * Conditionally renders children based on user's permissions.
 */
export function PermissionGate({
  permissions,
  requireAll = false,
  children,
  fallback = null,
}: PermissionGateProps) {
  const { hasPermission, hasAllPermissions, hasAnyPermission } = usePermissions();

  const codes = Array.isArray(permissions) ? permissions : [permissions];

  const hasAccess = codes.length === 1
    ? hasPermission(codes[0])
    : requireAll
      ? hasAllPermissions(codes)
      : hasAnyPermission(codes);

  return hasAccess ? <>{children}</> : <>{fallback}</>;
}
