import { Navigate } from 'react-router-dom';
import { usePermissions } from '@/hooks/usePermissions';
import { ROUTES } from '@/constants/routes';
import type { ReactNode } from 'react';

interface RouteGateProps {
  permissions: string[];
  children: ReactNode;
}

/**
 * Guard component for page-level access control.
 * If the user has NONE of the specified permissions, they are redirected to Dashboard.
 */
export function RouteGate({ permissions, children }: RouteGateProps) {
  const { hasAnyPermission } = usePermissions();

  const hasAccess = hasAnyPermission(permissions);

  if (!hasAccess) {
    return <Navigate to={ROUTES.DASHBOARD} replace />;
  }

  return <>{children}</>;
}
