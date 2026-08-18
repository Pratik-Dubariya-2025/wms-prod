import { Routes, Route } from 'react-router-dom';
import { ROUTES } from '@/constants/routes';

// Shared components
import { ProtectedRoute } from '@/components/shared/ProtectedRoute';
import { AppShell } from '@/components/layout/AppShell';
import { RouteGate } from '@/components/shared/RouteGate';

// Constants
import { PERMISSIONS } from '@/constants/permissions';

// Pages
import { LoginPage } from '@/features/auth/pages/LoginPage';
import { VerifyEmailPage } from '@/features/auth/pages/VerifyEmailPage';
import { ForgotPasswordPage } from '@/features/auth/pages/ForgotPasswordPage';
import { ResetPasswordPage } from '@/features/auth/pages/ResetPasswordPage';
import { ChangePasswordPage } from '@/features/auth/pages/ChangePasswordPage';
import { CssGuide } from '@/components/CssGuide';
import Dashboard from '@/pages/Dashboard';
import UsersPage from '@/features/users/pages/UsersPage';
import RolesPage from '@/features/roles/pages/RolesPage';
import PoliciesPage from '@/features/policies/pages/PoliciesPage';
import DepartmentsPage from '@/features/departments/pages/DepartmentsPage';
import ProjectsPage from '@/features/projects/pages/ProjectsPage';
import ProjectDetailPage from '@/features/projects/pages/ProjectDetailPage';
import LeavesPage from '@/features/leaves/pages/LeavesPage';
import LeadsPage from '@/features/crm/pages/LeadsPage';
import InvoicesPage from '@/features/invoices/pages/InvoicesPage';

/**
 * All route definitions in one place.
 */
export function AppRouter() {
  return (
    <Routes>
      {/* Public routes */}
      <Route path={ROUTES.LOGIN} element={<LoginPage />} />
      <Route path={ROUTES.VERIFY_EMAIL} element={<VerifyEmailPage />} />
      <Route path={ROUTES.FORGOT_PASSWORD} element={<ForgotPasswordPage />} />
      <Route path={ROUTES.RESET_PASSWORD} element={<ResetPasswordPage />} />
      <Route path={ROUTES.CHANGE_PASSWORD} element={<ChangePasswordPage />} />
      <Route path={ROUTES.CSS_GUIDE} element={<CssGuide />} />



      {/* Protected routes — requires authentication */}
      <Route element={<ProtectedRoute />}>
        <Route element={<AppShell />}>
          <Route path={ROUTES.DASHBOARD} element={<Dashboard />} />
          <Route
            path={ROUTES.USER}
            element={
              <RouteGate permissions={[
                PERMISSIONS.USER_READ,
                PERMISSIONS.USER_CREATE,
                PERMISSIONS.USER_UPDATE,
                PERMISSIONS.USER_DELETE
              ]}>
                <UsersPage />
              </RouteGate>
            }
          />
          <Route
            path={ROUTES.ROLE}
            element={
              <RouteGate permissions={[
                PERMISSIONS.ROLE_READ,
                PERMISSIONS.ROLE_CREATE,
                PERMISSIONS.ROLE_UPDATE,
                PERMISSIONS.ROLE_DELETE
              ]}>
                <RolesPage />
              </RouteGate>
            }
          />
          <Route
            path={ROUTES.POLICIES}
            element={
              <RouteGate permissions={[
                PERMISSIONS.POLICY_READ
              ]}>
                <PoliciesPage />
              </RouteGate>
            }
          />
          <Route
            path={ROUTES.DEPARTMENT}
            element={
              <RouteGate permissions={[
                PERMISSIONS.DEPARTMENT_READ,
                PERMISSIONS.DEPARTMENT_CREATE,
                PERMISSIONS.DEPARTMENT_UPDATE,
                PERMISSIONS.DEPARTMENT_DELETE
              ]}>
                <DepartmentsPage />
              </RouteGate>
            }
          />
          <Route
            path={ROUTES.PROJECTS}
            element={
              <RouteGate permissions={[
                PERMISSIONS.PROJECT_READ,
                PERMISSIONS.PROJECT_CREATE,
                PERMISSIONS.PROJECT_UPDATE,
                PERMISSIONS.PROJECT_DELETE
              ]}>
                <ProjectsPage />
              </RouteGate>
            }
          />
          <Route
            path={ROUTES.PROJECT_DETAILS}
            element={
              <RouteGate permissions={[
                PERMISSIONS.PROJECT_READ,
                PERMISSIONS.PROJECT_CREATE,
                PERMISSIONS.PROJECT_UPDATE,
                PERMISSIONS.PROJECT_DELETE
              ]}>
                <ProjectDetailPage />
              </RouteGate>
            }
          />
          <Route
            path={ROUTES.LEAVES}
            element={
              <RouteGate permissions={[PERMISSIONS.LEAVE_READ]}>
                <LeavesPage key="mine" approvalsOnly={false} />
              </RouteGate>
            }
          />
          <Route
            path="/leaves/approvals"
            element={
              <RouteGate permissions={[PERMISSIONS.LEAVE_APPROVE]}>
                <LeavesPage key="approvals" approvalsOnly={true} />
              </RouteGate>
            }
          />
          <Route
            path="/leaves/team"
            element={
              <RouteGate permissions={[PERMISSIONS.LEAVE_READ]}>
                <LeavesPage key="team" teamOnly={true} />
              </RouteGate>
            }
          />
          <Route
            path={ROUTES.LEADS}
            element={
              <RouteGate permissions={[
                PERMISSIONS.LEADS_READ,
                PERMISSIONS.LEADS_WRITE
              ]}>
                <LeadsPage />
              </RouteGate>
            }
          />
          <Route
            path={ROUTES.INVOICES}
            element={
              <RouteGate permissions={[
                PERMISSIONS.INVOICES_READ,
                PERMISSIONS.INVOICES_WRITE
              ]}>
                <InvoicesPage />
              </RouteGate>
            }
          />
        </Route>
      </Route>
    </Routes>
  );
}
