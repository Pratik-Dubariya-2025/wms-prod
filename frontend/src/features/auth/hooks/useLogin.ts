import { useMutation } from '@tanstack/react-query';
import { useNavigate, useLocation } from 'react-router-dom';
import { login } from '@/api/authApi';
import { useAuthStore } from '@/store/authStore';
import { usePermissionStore } from '@/store/permissionStore';
import { getUserFromToken } from '@/services/tokenService';
import { toast } from '@/components/ui/Toast/Toast';
import { ROUTES } from '@/constants/routes';
import type { LoginCredentials } from '../types/auth.types';
import type { AxiosError } from 'axios';

/**
 * Login mutation hook.
 * On success: stores access token, extracts permissions from JWT, redirects to dashboard.
 */
export function useLogin() {
  const navigate = useNavigate();
  const location = useLocation();
  const setAuth = useAuthStore((s) => s.setAuth);
  const { setRoles } = usePermissionStore();

  return useMutation({
    mutationFn: (credentials: LoginCredentials) => login(credentials),
    onSuccess: (response) => {
      if (response.succeeded && response.data) {
        let accessToken: string | null = null;
        let requiresMfa = false;

        if (typeof response.data === 'string') {
          accessToken = response.data;
        } else if (typeof response.data === 'object' && response.data !== null) {
          const dataObj = response.data as any;
          accessToken = dataObj.accessToken || dataObj.AccessToken || null;
          requiresMfa = !!(dataObj.requiresMfa || dataObj.RequiresMfa);
        }

        if (requiresMfa) {
          toast('MFA verification is required to complete sign-in.', { icon: '🔑' });
          return;
        }

        if (accessToken) {
          setAuth(accessToken);

          // Extract roles & permissions from JWT claims
          const user = getUserFromToken(accessToken);
          if (user) {
            setRoles(user.roles);
          }

          toast.success(response.message || 'Logged in successfully');

          // Redirect to the page the user originally wanted, or dashboard
          const from = (location.state as { from?: { pathname: string } })?.from?.pathname || ROUTES.DASHBOARD;
          navigate(from, { replace: true });
        }
      } else {
        toast.error(response.message || 'Login failed');
      }
    },

    onError: (error: AxiosError<any>) => {
      const data = error.response?.data;
      const message = data?.message || data?.Message || 'Login failed. Please try again.';
      if (
        message === 'FIRST_TIME_LOGIN' ||
        (typeof data === 'string' && data.includes('FIRST_TIME_LOGIN'))
      ) {
        toast('First time login detected. Please verify your email to continue.', { icon: 'ℹ️' });
        navigate('/verify-email', { replace: true });
        return;

      }
      toast.error(message);
    },



  });
}
