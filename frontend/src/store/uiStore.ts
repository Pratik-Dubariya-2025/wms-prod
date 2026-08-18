import { create } from 'zustand';

export type Theme = 'light' | 'dark';

const THEME_KEY = 'wms-theme';

/** Reads the persisted theme; defaults to light. */
export function getInitialTheme(): Theme {
  if (typeof window === 'undefined') return 'light';
  const stored = window.localStorage.getItem(THEME_KEY);
  return stored === 'dark' ? 'dark' : 'light';
}

/** Applies the theme to <html> and persists it. */
export function applyTheme(theme: Theme) {
  if (typeof document === 'undefined') return;
  document.documentElement.classList.toggle('dark', theme === 'dark');
  window.localStorage.setItem(THEME_KEY, theme);
}

interface UiState {
  /** Sidebar expanded or collapsed */
  isSidebarOpen: boolean;
  /** Toggle sidebar */
  toggleSidebar: () => void;
  /** Set sidebar state explicitly */
  setSidebarOpen: (open: boolean) => void;

  /** Active color theme (default light) */
  theme: Theme;
  /** Toggle between light and dark */
  toggleTheme: () => void;
  /** Set theme explicitly */
  setTheme: (theme: Theme) => void;

  /** Global loading overlay (for route transitions, etc.) */
  isGlobalLoading: boolean;
  setGlobalLoading: (loading: boolean) => void;
}

export const useUiStore = create<UiState>()((set, get) => ({
  isSidebarOpen: true,
  toggleSidebar: () => set((state) => ({ isSidebarOpen: !state.isSidebarOpen })),
  setSidebarOpen: (open) => set({ isSidebarOpen: open }),

  theme: getInitialTheme(),
  toggleTheme: () => {
    const next: Theme = get().theme === 'dark' ? 'light' : 'dark';
    applyTheme(next);
    set({ theme: next });
  },
  setTheme: (theme) => {
    applyTheme(theme);
    set({ theme });
  },

  isGlobalLoading: false,
  setGlobalLoading: (loading) => set({ isGlobalLoading: loading }),
}));
