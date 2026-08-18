import { useEffect } from 'react';
import { useAuthStore } from '@/store/authStore';
import { startSignalR, stopSignalR } from '@/services/signalRService';

/**
 * React hook that manages the SignalR connection lifecycle.
 *
 * Mount this ONCE at the app root (e.g. in App.tsx).
 * Connects when the user is authenticated and disconnects on logout.
 */
export function useSignalR(): void {
  const isAuthenticated = useAuthStore((s) => s.isAuthenticated);

  useEffect(() => {
    if (isAuthenticated) {
      startSignalR();
    } else {
      stopSignalR();
    }
  }, [isAuthenticated]);
}
