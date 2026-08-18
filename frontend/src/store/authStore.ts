import { create } from 'zustand';
import { persist } from 'zustand/middleware';

interface AuthState {
  /** JWT access token */
  accessToken: string | null;
  /** Whether the user is authenticated */
  isAuthenticated: boolean;

  /** Set access token and mark as authenticated */
  setAuth: (accessToken: string) => void;
  /** Clear auth state (logout) */
  clearAuth: () => void;
}

export const useAuthStore = create<AuthState>()(
  persist(
    (set) => ({
      accessToken: null,
      isAuthenticated: false,

      setAuth: (accessToken) =>
        set({
          accessToken,
          isAuthenticated: true,
        }),

      clearAuth: () =>
        set({
          accessToken: null,
          isAuthenticated: false,
        }),
    }),
    {
      name: 'wms-auth',
      partialize: (state) => ({
        accessToken: state.accessToken,
        isAuthenticated: state.isAuthenticated,
      }),
    },
  ),
);
