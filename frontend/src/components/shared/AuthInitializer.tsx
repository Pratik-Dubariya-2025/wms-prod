import { useEffect, useState } from 'react';
import { useAuthStore } from '@/store/authStore';
import { getMe, refreshToken } from '@/api/authApi';
import { Spinner } from '@/components/ui/Spinner/Spinner';
import { usePermissionStore } from '@/store/permissionStore';
import { getInitialTheme } from '@/store/uiStore';

interface AuthInitializerProps {
  children: React.ReactNode;
}

export function AuthInitializer({ children }: AuthInitializerProps) {
  const { accessToken, setAuth, clearAuth } = useAuthStore();
  const { setPermissions, setRoles } = usePermissionStore();
  const [isInitializing, setIsInitializing] = useState(true);

  useEffect(() => {
    const initializeAuth = async () => {
      let currentToken = accessToken;
      if (!accessToken) {
        try {
          const response = await refreshToken();
          if (response?.data) {
            setAuth(response.data);
            currentToken = response.data;
          }
        } catch {
          clearAuth();
        }
      }

      if (currentToken) {
        try {
          const response = await getMe();
          if (response.succeeded && response.data) {
            setPermissions(response.data);
          }
        } catch {
          clearAuth();
        }
      }
      setIsInitializing(false);
    };

    initializeAuth();
  }, [accessToken, setAuth, clearAuth, setPermissions, setRoles]);

  if (isInitializing) {
    return (
      <div className="min-h-screen flex flex-col items-center justify-center relative overflow-hidden">
        <div className="absolute top-1/4 -left-32 w-96 h-96 bg-wms-indigo/10 rounded-full blur-3xl pointer-events-none" />
        <div className="absolute bottom-1/4 -right-32 w-96 h-96 bg-wms-cyan/5 rounded-full blur-3xl pointer-events-none" />

        <div className="flex flex-col items-center gap-4 z-10">
          <img src={getInitialTheme() === 'dark' ? '/dark_main_logo.png' : '/light_main_logo.png'} alt="WMS logo" height={45} width={45} />
          <Spinner size="lg" label="Verifying session..." />
        </div>
      </div>
    );
  }

  return <>{children}</>;
}
