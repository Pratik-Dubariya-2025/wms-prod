/** TanStack Query key factory for cache consistency */
export const queryKeys = {
  auth: {
    all: ['auth'] as const,
    check: () => [...queryKeys.auth.all, 'check'] as const,
  },
} as const;
