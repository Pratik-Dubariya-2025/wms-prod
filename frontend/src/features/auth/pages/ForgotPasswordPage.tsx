import { Navigate } from 'react-router-dom';
import { ForgotPasswordForm } from '../components/ForgotPasswordForm';
import { useAuthStore } from '@/store/authStore';
import { ROUTES } from '@/constants/routes';

export function ForgotPasswordPage() {
  const isAuthenticated = useAuthStore((s) => s.isAuthenticated);

  if (isAuthenticated) {
    return <Navigate to={ROUTES.DASHBOARD} replace />;
  }

  return (
    <div className="min-h-screen flex items-center justify-center p-4 relative overflow-x-hidden">
      {/* Ambient glow effects */}
      <div className="absolute top-1/4 -left-32 w-96 h-96 bg-wms-indigo/20 rounded-full blur-3xl pointer-events-none" />
      <div className="absolute bottom-1/4 -right-32 w-96 h-96 bg-wms-cyan/10 rounded-full blur-3xl pointer-events-none" />

      <div className="relative w-full max-w-md">
        {/* Logo + heading */}
        <div className="text-center mb-8">
          <div className="inline-flex items-center justify-center h-14 w-14 rounded-2xl bg-gradient-to-br from-wms-indigo to-wms-cyan mb-4">
            <span className="text-white font-bold text-xl">W</span>
          </div>
          <h1 className="text-3xl font-extrabold text-wms-text tracking-tight">Recover Password</h1>
          <p className="text-sm text-wms-secondary mt-2">
            Workspace Management System Account Recovery
          </p>
        </div>

        {/* Card */}
        <div className="glass-card rounded-xl p-6 sm:p-8">
          <ForgotPasswordForm />
        </div>
      </div>
    </div>
  );
}
