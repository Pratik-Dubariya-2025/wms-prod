import type { ReactNode } from 'react';
import { Inbox } from 'lucide-react';

interface EmptyStateProps {
  icon?: ReactNode;
  title?: string;
  message?: string;
  action?: ReactNode;
}

/**
 * Empty state placeholder for lists with no data.
 */
export function EmptyState({
  icon,
  title = 'No data found',
  message = 'There are no items to display at the moment.',
  action,
}: EmptyStateProps) {
  return (
    <div className="glass-card rounded-xl p-12 flex flex-col items-center justify-center text-center gap-4">
      <div className="p-4 rounded-full bg-wms-hover">
        {icon || <Inbox className="h-8 w-8 text-wms-muted" />}
      </div>
      <div>
        <h3 className="text-base font-semibold text-wms-text mb-1">{title}</h3>
        <p className="text-sm text-wms-muted max-w-sm">{message}</p>
      </div>
      {action && <div className="mt-2">{action}</div>}
    </div>
  );
}
