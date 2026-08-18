import { Loader2 } from 'lucide-react';
import { classNames } from '@/utils/classNames';

interface SpinnerProps {
  size?: 'sm' | 'md' | 'lg';
  className?: string;
  /** Optional label displayed below the spinner */
  label?: string;
}

const sizeClasses: Record<string, string> = {
  sm: 'h-4 w-4',
  md: 'h-6 w-6',
  lg: 'h-10 w-10',
};

export function Spinner({ size = 'md', className, label }: SpinnerProps) {
  return (
    <div className="flex flex-col items-center justify-center gap-2">
      <Loader2
        className={classNames('animate-spin text-wms-indigo', sizeClasses[size], className)}
      />
      {label && <span className="text-xs text-wms-muted">{label}</span>}
    </div>
  );
}
