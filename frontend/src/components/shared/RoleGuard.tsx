import type { ReactNode } from 'react';
import { usePermissions } from '@/hooks/usePermissions';

interface RoleGuardProps {
  /** Required role name(s) */
  roles: string | string[];
  /** Content to show if role check passes */
  children: ReactNode;
  /** Optional fallback when user is not in required role */
  fallback?: ReactNode;
}

/**
 * Renders children only if the current user has one of the specified roles.
 */
export function RoleGuard({ roles, children, fallback = null }: RoleGuardProps) {
  const { isInRole } = usePermissions();

  const roleList = Array.isArray(roles) ? roles : [roles];
  const hasRole = roleList.some((role) => isInRole(role));

  return hasRole ? <>{children}</> : <>{fallback}</>;
}
