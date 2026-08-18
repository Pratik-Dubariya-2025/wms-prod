import type { ReactNode } from 'react';
import { BrowserRouter } from 'react-router-dom';
import { QueryClientProvider } from '@tanstack/react-query';
import { queryClient } from './queryClient';
import { Toaster } from '@/components/ui/Toast/Toast';
import { ErrorBoundary } from '@/components/shared/ErrorBoundary';

interface AppProvidersProps {
  children: ReactNode;
}

/**
 * Wraps the app with all required providers:
 *  - ErrorBoundary (outermost)
 *  - QueryClientProvider (TanStack Query)
 *  - BrowserRouter (React Router)
 *  - Toaster (react-hot-toast)
 */
export function AppProviders({ children }: AppProvidersProps) {
  return (
    <ErrorBoundary>
      <QueryClientProvider client={queryClient}>
        <BrowserRouter>
          {children}
          <Toaster />
        </BrowserRouter>
      </QueryClientProvider>
    </ErrorBoundary>
  );
}
