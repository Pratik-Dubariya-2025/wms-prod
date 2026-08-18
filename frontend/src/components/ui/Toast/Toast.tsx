/**
 * Toast re-export and custom configuration.
 * Uses react-hot-toast with WMS-themed styling.
 */
import toast, { Toaster as HotToaster } from 'react-hot-toast';

export { toast };

/**
 * Pre-configured Toaster component with WMS dark theme styling.
 * Add this once in AppProviders.
 */
export function Toaster() {
  return (
    <HotToaster
      position="top-right"
      gutter={8}
      containerClassName="!top-4 !right-4"
      toastOptions={{
        duration: 4000,
        // Colors reference theme CSS variables so toasts follow light/dark.
        style: {
          background: 'var(--wms-surface)',
          backdropFilter: 'blur(16px)',
          color: 'var(--wms-text)',
          border: '1px solid var(--wms-border)',
          borderRadius: '0.75rem',
          boxShadow: '0 10px 30px -12px rgba(15, 23, 42, 0.25)',
          fontSize: '0.875rem',
          fontFamily: "'Outfit', sans-serif",
          maxWidth: 'min(90vw, 360px)',
        },
        success: {
          iconTheme: {
            primary: '#10b981',
            secondary: '#ffffff',
          },
        },
        error: {
          iconTheme: {
            primary: '#ef4444',
            secondary: '#ffffff',
          },
        },
      }}
    />
  );
}
