import { classNames } from '@/utils/classNames';
import type { ReactNode } from 'react';

type BadgeVariant = 'default' | 'indigo' | 'cyan' | 'emerald' | 'warning' | 'danger' | 'purple' | 'primary' | 'info';

interface BadgeProps {
  variant?: BadgeVariant;
  children: ReactNode;
  className?: string;
}

const variantClasses: Record<BadgeVariant, string> = {
  default: 'bg-wms-hover text-wms-secondary',
  indigo: 'bg-wms-indigo/15 text-wms-indigo',
  primary: 'bg-wms-indigo/15 text-wms-indigo',
  cyan: 'bg-wms-cyan/15 text-wms-cyan',
  info: 'bg-wms-cyan/15 text-wms-cyan',
  emerald: 'bg-wms-emerald/15 text-wms-emerald',
  warning: 'bg-wms-warning/15 text-wms-warning',
  danger: 'bg-wms-danger/15 text-wms-danger',
  purple: 'bg-wms-purple/15 text-wms-purple',
};

export function Badge({ variant = 'default', children, className }: BadgeProps) {
  return (
    <span
      className={classNames(
        'inline-flex items-center gap-1 px-2.5 py-0.5 rounded-full text-xs font-semibold',
        variantClasses[variant],
        className,
      )}
    >
      {children}
    </span>
  );
}
