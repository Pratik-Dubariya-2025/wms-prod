import { usePermissions } from "@/hooks/usePermissions";
import { Link } from "react-router-dom";
import { ROUTES } from "@/constants/routes";
import { PERMISSIONS } from "@/constants/permissions";

export default function Dashboard () {
    const { hasPermission } = usePermissions();

    return (
        <div className="space-y-6">
            <div className="flex items-center justify-between border-b border-wms-border pb-5">
                <div>
                    <h1 className="text-2xl font-bold text-wms-text tracking-tight">Dashboard</h1>
                    <p className="text-sm text-wms-secondary mt-1">Workspace Management System</p>
                </div>
            </div>
            <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
                {/* Card for User Management - only show if they can read users */}
                {hasPermission(PERMISSIONS.USER_READ) && (
                    <div className="glass-card p-6 rounded-xl">
                        <h2 className="text-lg font-semibold text-wms-text">Users</h2>
                        <p className="text-sm text-wms-muted mt-2">Manage employee status and assignments.</p>
                        <Link to={ROUTES.USER} className="mt-4 inline-block text-sm font-medium text-wms-indigo hover:text-wms-indigo-hover">
                            View Users &rarr;
                        </Link>
                    </div>
                )}
                {/* Card for Role Management - only show if they can read roles */}
                {hasPermission(PERMISSIONS.ROLE_READ) && (
                    <div className="glass-card p-6 rounded-xl">
                        <h2 className="text-lg font-semibold text-wms-text">Roles & Permissions</h2>
                        <p className="text-sm text-wms-muted mt-2">Assign system roles and configure access policies.</p>
                        <Link to={ROUTES.ROLE} className="mt-4 inline-block text-sm font-medium text-wms-indigo hover:text-wms-indigo-hover">
                            View Roles &rarr;
                        </Link>
                    </div>
                )}
            </div>
        </div>
    );
}