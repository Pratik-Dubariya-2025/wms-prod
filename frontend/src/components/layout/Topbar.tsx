import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { LogOut, Menu, Shield, Sun, Moon } from 'lucide-react';
import { useUiStore } from '@/store/uiStore';
import { ConfirmDialog } from '@/components/shared/ConfirmDialog';
import { useCurrentUser } from '@/hooks/useCurrentUser';
import { useAuthStore } from '@/store/authStore';
import { usePermissionStore } from '@/store/permissionStore';
import { logout } from '@/api/authApi';
import { Avatar } from '@/components/ui/Avatar/Avatar';
import { toast } from '@/components/ui/Toast/Toast';
import { ROUTES } from '@/constants/routes';
import { SecuritySettingsDialog } from './SecuritySettingsDialog';

export function Topbar() {
  const { isSidebarOpen, toggleSidebar, theme, toggleTheme } = useUiStore();
  const user = useCurrentUser();
  const clearAuth = useAuthStore((s) => s.clearAuth);
  const clearPermissions = usePermissionStore((s) => s.clearPermissions);
  const [showConfirm, setShowConfirm] = useState(false);
  const [showSecuritySettings, setShowSecuritySettings] = useState(false);
  const [isLoggingOut, setIsLoggingOut] = useState(false);
  const navigate = useNavigate();

  async function handleLogout() {
    setIsLoggingOut(true);
    try {
      await logout();
    } catch {
      // Even if server-side logout fails, clear client state
    } finally {
      setIsLoggingOut(false);
      setShowConfirm(false);
      clearAuth();
      clearPermissions();
      toast.success('Logged out successfully');
      navigate(ROUTES.LOGIN);
    }
  }

  const displayName = user
    ? `${user.username}`
    : 'User';

  return (
    <header className="h-16 flex items-center justify-between px-3 border-b border-wms-border bg-wms-surface/80 backdrop-blur-md">
      {/* Left side — sidebar toggle button */}
      <div>
        <button
          onClick={toggleSidebar}
          className="p-2 rounded-lg text-wms-muted hover:text-wms-text hover:bg-wms-hover transition duration-200 cursor-pointer"
          title={isSidebarOpen ? 'Collapse sidebar' : 'Expand sidebar'}
        >
          <Menu className="h-7 w-7" />
        </button>
      </div>

      {/* Right side — user info + logout */}
      <div className="flex items-center gap-4">
        <div className="flex items-center gap-3">
          <Avatar name={displayName} size="sm" />
          <div className="hidden sm:block">
            <p className="text-sm font-semibold text-wms-text leading-tight">{displayName}</p>
            {user?.email && (
              <p className="text-xs text-wms-muted leading-tight">{user.email}</p>
            )}
          </div>
        </div>

        <div className="h-6 w-px bg-wms-hover" />

        <button
          onClick={toggleTheme}
          className="p-2 rounded-lg text-wms-muted hover:text-wms-warning hover:bg-wms-hover transition duration-200 cursor-pointer"
          title={theme === 'dark' ? 'Switch to light mode' : 'Switch to dark mode'}
        >
          {theme === 'dark' ? <Sun className="h-4 w-4" /> : <Moon className="h-4 w-4" />}
        </button>

        <button
          onClick={() => setShowSecuritySettings(true)}
          className="p-2 rounded-lg text-wms-muted hover:text-wms-indigo hover:bg-wms-hover transition duration-200 cursor-pointer"
          title="Security Settings"
        >
          <Shield className="h-4 w-4" />
        </button>

        <button
          onClick={() => setShowConfirm(true)}
          className="p-2 rounded-lg text-wms-muted hover:text-wms-danger hover:bg-wms-danger/10 transition duration-200 cursor-pointer"
          title="Logout"
        >
          <LogOut className="h-4 w-4" />
        </button>
      </div>

      <ConfirmDialog
        isOpen={showConfirm}
        onClose={() => setShowConfirm(false)}
        onConfirm={handleLogout}
        title="Confirm Logout"
        message="Are you sure you want to log out of the Workspace Management System?"
        confirmLabel="Logout"
        cancelLabel="Cancel"
        isLoading={isLoggingOut}
        variant="danger"
      />

      <SecuritySettingsDialog
        isOpen={showSecuritySettings}
        onClose={() => setShowSecuritySettings(false)}
      />
    </header>
  );
}
