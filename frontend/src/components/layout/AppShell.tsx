import { Outlet } from 'react-router-dom';
import { Sidebar } from './Sidebar';
import { Topbar } from './Topbar';
import { useUiStore } from '@/store/uiStore';
import { classNames } from '@/utils/classNames';

/**
 * AppShell — the main layout shell for authenticated pages.
 * Renders Sidebar + Topbar + page content via <Outlet />.
 */
export function AppShell() {
  const { isSidebarOpen } = useUiStore();

  return (
    <div className="flex h-screen overflow-hidden">
      {/* Sidebar */}
      <Sidebar />

      {/* Main content area */}
      <div
        className={classNames(
          'flex-1 flex flex-col overflow-hidden transition-all duration-300',
          isSidebarOpen ? 'ml-0 md:ml-64' : 'ml-0 md:ml-16',
        )}
      >
        {/* Top bar */}
        <Topbar />

        {/* Page content */}
        <main className="flex-1 overflow-y-auto p-4 sm:p-6">
          <Outlet />
        </main>
      </div>
    </div>
  );
}
