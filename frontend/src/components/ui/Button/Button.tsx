import type { ButtonProps } from './Button.types';
import { classNames } from '@/utils/classNames';
import { Loader2 } from 'lucide-react';

const variantClasses: Record<string, string> = {
  primary: 'bg-wms-indigo hover:bg-indigo-700 text-white',
  secondary: 'bg-wms-hover hover:bg-wms-border text-wms-text border border-wms-border',
  danger: 'bg-wms-danger/10 text-wms-danger hover:bg-wms-danger/20',
  success: 'bg-wms-emerald/10 text-wms-emerald hover:bg-wms-emerald/20',
  ghost: 'bg-transparent hover:bg-wms-hover text-wms-secondary hover:text-wms-text',
};

const sizeClasses: Record<string, string> = {
  sm: 'text-xs px-3 py-1.5',
  md: 'text-sm px-5 py-2.5',
  lg: 'text-base px-6 py-3',
};

export function Button({
  variant = 'primary',
  size = 'md',
  isLoading = false,
  leftIcon,
  rightIcon,
  fullWidth = false,
  className,
  disabled,
  children,
  ...rest
}: ButtonProps) {
  return (
    <button
      className={classNames(
        'inline-flex items-center justify-center gap-2 rounded-lg font-semibold transition duration-200 cursor-pointer',
        'focus:outline-none focus:ring-2 focus:ring-wms-indigo/50 focus:ring-offset-2 focus:ring-offset-wms-bg',
        'disabled:opacity-50 disabled:cursor-not-allowed',
        variantClasses[variant],
        sizeClasses[size],
        fullWidth && 'w-full',
        className,
      )}
      disabled={disabled || isLoading}
      {...rest}
    >
      {isLoading ? (
        <Loader2 className="h-4 w-4 animate-spin" />
      ) : leftIcon ? (
        <span className="flex-shrink-0">{leftIcon}</span>
      ) : null}
      {children}
      {rightIcon && !isLoading && <span className="flex-shrink-0">{rightIcon}</span>}
    </button>
  );
}
