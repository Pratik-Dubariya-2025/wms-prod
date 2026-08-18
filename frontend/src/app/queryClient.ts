import { QueryClient } from '@tanstack/react-query';

export const queryClient = new QueryClient({
  defaultOptions: {
    queries: {
      /** Don't refetch when the browser tab regains focus */
      refetchOnWindowFocus: false,
      /** Retry failed queries once */
      retry: 1,
      /** Data is considered fresh for 30 seconds */
      staleTime: 30 * 1000,
    },
    mutations: {
      retry: false,
    },
  },
});
