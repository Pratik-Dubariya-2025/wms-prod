import { useState } from 'react';
import { NavLink, useLocation } from 'react-router-dom';
import { useUiStore } from '@/store/uiStore';
import { classNames } from '@/utils/classNames';
import { ROUTES } from '@/constants/routes';
import { LayoutDashboard, Users, Shield, ShieldAlert, Building2, FolderGit, ClipboardList, Briefcase, FileText, ChevronDown, ChevronUp } from 'lucide-react';
import { BrandText } from '@/components/layout/BrandText';
import { usePermissions } from '@/hooks/usePermissions';
import { PERMISSIONS } from '@/constants/permissions';


interface NavItem {
  label: string;
  path: string;
  icon: React.ReactNode;
  permissions?: string[];
}

const navItems: NavItem[] = [
  { label: 'Dashboard', path: ROUTES.DASHBOARD, icon: <LayoutDashboard className="h-5 w-5" /> },
  {
    label: 'Users',
    path: '/users',
    icon: <Users className="h-5 w-5" />,
    permissions: [
      PERMISSIONS.USER_READ,
      PERMISSIONS.USER_CREATE,
      PERMISSIONS.USER_UPDATE,
      PERMISSIONS.USER_DELETE
    ]
  },
  {
    label: 'Roles',
    path: '/roles',
    icon: <Shield className="h-5 w-5" />,
    permissions: [
      PERMISSIONS.ROLE_READ,
      PERMISSIONS.ROLE_CREATE,
      PERMISSIONS.ROLE_UPDATE,
      PERMISSIONS.ROLE_DELETE
    ]
  },
  {
    label: 'Policies',
    path: '/policies',
    icon: <ShieldAlert className="h-5 w-5" />,
    permissions: [
      PERMISSIONS.POLICY_READ,
      PERMISSIONS.POLICY_WRITE
    ]
  },
  {
    label: 'Departments',
    path: '/departments',
    icon: <Building2 className="h-5 w-5" />,
    permissions: [
      PERMISSIONS.DEPARTMENT_READ,
      PERMISSIONS.DEPARTMENT_CREATE,
      PERMISSIONS.DEPARTMENT_UPDATE,
      PERMISSIONS.DEPARTMENT_DELETE
    ]
  },
  {
    label: 'Projects',
    path: ROUTES.PROJECTS,
    icon: <FolderGit className="h-5 w-5" />,
    permissions: [
      PERMISSIONS.PROJECT_READ
    ]
  },
  {
    label: 'Leaves',
    path: ROUTES.LEAVES,
    icon: <ClipboardList className="h-5 w-5" />
  },
  {
    label: 'CRM Leads',
    path: ROUTES.LEADS,
    icon: <Briefcase className="h-5 w-5" />,
    permissions: [
      PERMISSIONS.LEADS_READ,
      PERMISSIONS.LEADS_WRITE
    ]
  },
  {
    label: 'Invoices',
    path: ROUTES.INVOICES,
    icon: <FileText className="h-5 w-5" />,
    permissions: [
      PERMISSIONS.INVOICES_READ,
      PERMISSIONS.INVOICES_WRITE
    ]
  },
];

export function Sidebar() {
  const { isSidebarOpen, toggleSidebar, theme } = useUiStore();
  const { hasPermission, roles } = usePermissions();
  const location = useLocation();
  const logoSrc = theme === 'dark' ? '/dark_main_logo.png' : '/light_main_logo.png';

  const canApprove = hasPermission(PERMISSIONS.LEAVE_APPROVE) || roles.some((r) => ['ADMIN', 'HR', 'ACCOUNTS', 'MANAGER', 'TL'].includes(r));
  const [isLeavesExpanded, setIsLeavesExpanded] = useState(false);
  const isLeavesPathActive = location.pathname.startsWith('/leaves');

  const allowedNavItems = navItems.filter(
    (item) => !item.permissions || item.permissions.some((perm) => hasPermission(perm))
  );

  const handleLeavesClick = (e: React.MouseEvent) => {
    if (canApprove) {
      e.preventDefault();
      if (!isSidebarOpen) {
        toggleSidebar();
        setIsLeavesExpanded(true);
      } else {
        setIsLeavesExpanded(!isLeavesExpanded);
      }
    }
  };

  return (
    <>
      {/* Mobile backdrop */}
      {isSidebarOpen && (
        <div
          className="fixed inset-0 z-40 bg-black/60 backdrop-blur-sm md:hidden animate-fade-in"
          onClick={toggleSidebar}
        />
      )}
      <aside
        className={classNames(
          'fixed left-0 top-0 h-full z-50 flex flex-col transition-all duration-300',
          'bg-wms-surface/95 backdrop-blur-xl border-r border-wms-border',
          isSidebarOpen ? 'w-64 translate-x-0' : 'w-64 md:w-16 -translate-x-full md:translate-x-0',
        )}
      >
      {/* Logo area */}
      <div className="flex items-center h-16 px-4 border-b border-wms-border">
        <div className="flex items-center gap-3 overflow-hidden">
          {/* <div className="flex-shrink-0 h-8 w-8 rounded-lg bg-gradient-to-br from-wms-indigo to-wms-cyan flex items-center justify-center">
            <span className="text-wms-text font-bold text-sm">W</span>
          </div> */}
          <img src={logoSrc} alt="WMS logo" height={50} width={50} />
          {isSidebarOpen && (
            <BrandText className="flex-shrink-0" />
          )}
        </div>
      </div>

      {/* Navigation */}
      <nav className="flex-1 px-2 py-4 flex flex-col gap-1 overflow-y-auto">
        {allowedNavItems.map((item) => {
          if (item.label === 'Leaves' && canApprove) {
            return (
              <div key="leaves-menu" className="flex flex-col">
                <button
                  onClick={handleLeavesClick}
                  className={classNames(
                    'flex items-center gap-3 rounded-lg font-medium transition duration-200 w-full text-left cursor-pointer',
                    isSidebarOpen ? 'px-4 py-3' : 'px-3 py-3 justify-center',
                    isLeavesPathActive
                      ? 'bg-wms-indigo/15 text-wms-indigo border-l-2 border-wms-indigo'
                      : 'text-wms-secondary hover:bg-wms-hover hover:text-wms-text',
                  )}
                >
                  <span className="flex-shrink-0">{item.icon}</span>
                  {isSidebarOpen && (
                    <>
                      <span className="text-sm whitespace-nowrap">{item.label}</span>
                      <span className="ml-auto flex-shrink-0 text-wms-muted">
                        {isLeavesExpanded ? <ChevronUp className="h-4 w-4" /> : <ChevronDown className="h-4 w-4" />}
                      </span>
                    </>
                  )}
                </button>
                {isLeavesExpanded && isSidebarOpen && (
                  <div className="flex flex-col gap-1 mt-1 pl-4 animate-fade-in">
                    <NavLink
                      to={ROUTES.LEAVES}
                      end
                      className={({ isActive }) =>
                        classNames(
                          'flex items-center gap-3 px-4 py-2 rounded-lg text-xs font-semibold transition duration-200',
                          isActive
                            ? 'bg-wms-indigo/10 text-wms-indigo'
                            : 'text-wms-secondary hover:bg-wms-hover hover:text-wms-text',
                        )
                      }
                    >
                      <span className="w-1.5 h-1.5 rounded-full bg-current" />
                      My Leaves
                    </NavLink>
                    <NavLink
                      to="/leaves/team"
                      className={({ isActive }) =>
                        classNames(
                          'flex items-center gap-3 px-4 py-2 rounded-lg text-xs font-semibold transition duration-200',
                          isActive
                            ? 'bg-wms-indigo/10 text-wms-indigo'
                            : 'text-wms-secondary hover:bg-wms-hover hover:text-wms-text',
                        )
                      }
                    >
                      <span className="w-1.5 h-1.5 rounded-full bg-current" />
                      Team Leaves
                    </NavLink>
                    <NavLink
                      to="/leaves/approvals"
                      className={({ isActive }) =>
                        classNames(
                          'flex items-center gap-3 px-4 py-2 rounded-lg text-xs font-semibold transition duration-200',
                          isActive
                            ? 'bg-wms-indigo/10 text-wms-indigo'
                            : 'text-wms-secondary hover:bg-wms-hover hover:text-wms-text',
                        )
                      }
                    >
                      <span className="w-1.5 h-1.5 rounded-full bg-current" />
                      Leave Approvals
                    </NavLink>
                  </div>
                )}
              </div>
            );
          }

          return (
            <NavLink
              key={item.path}
              to={item.path}
              end={item.path === '/'}
              className={({ isActive }) =>
                classNames(
                  'flex items-center gap-3 rounded-lg font-medium transition duration-200',
                  isSidebarOpen ? 'px-4 py-3' : 'px-3 py-3 justify-center',
                  isActive
                    ? 'bg-wms-indigo/15 text-wms-indigo border-l-2 border-wms-indigo'
                    : 'text-wms-secondary hover:bg-wms-hover hover:text-wms-text',
                )
              }
            >
              <span className="flex-shrink-0">{item.icon}</span>
              {isSidebarOpen && <span className="text-sm whitespace-nowrap">{item.label}</span>}
            </NavLink>
          );
        })}
      </nav>
      </aside>
    </>
  );
}
