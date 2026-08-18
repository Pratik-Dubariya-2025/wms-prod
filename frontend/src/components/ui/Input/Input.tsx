import { forwardRef } from 'react';
import type { InputProps } from './Input.types';
import { classNames } from '@/utils/classNames';

export const Input = forwardRef<HTMLInputElement, InputProps>(
  ({ label, error, helperText, leftIcon, rightIcon, fullWidth = true, className, id, ...rest }, ref) => {
    const inputId = id || label?.toLowerCase().replace(/\s+/g, '-');

    return (
      <div className={classNames('flex flex-col gap-1.5', fullWidth && 'w-full')}>
        {label && (
          <label
            htmlFor={inputId}
            className="text-xs font-semibold text-wms-muted uppercase tracking-wide"
          >
            {label}
          </label>
        )}
        <div className="relative">
          {leftIcon && (
            <span className="absolute left-3 top-1/2 -translate-y-1/2 text-wms-muted">
              {leftIcon}
            </span>
          )}
          <input
            ref={ref}
            id={inputId}
            className={classNames(
              'w-full rounded-lg bg-wms-hover border text-sm text-wms-text placeholder:text-wms-muted',
              'px-4 py-2.5 transition duration-200 outline-none',
              'focus:border-wms-indigo focus:ring-2 focus:ring-wms-indigo/20',
              error
                ? 'border-wms-danger/50 focus:border-wms-danger focus:ring-wms-danger/20'
                : 'border-wms-border hover:border-wms-border',
              !!leftIcon && 'pl-10',
              !!rightIcon && 'pr-10',
              className,
            )}
            {...rest}
          />
          {rightIcon && (
            <span className="absolute right-3 top-1/2 -translate-y-1/2 text-wms-muted">
              {rightIcon}
            </span>
          )}
        </div>
        {error && <p className="text-xs text-wms-danger">{error}</p>}
        {helperText && !error && <p className="text-xs text-wms-muted">{helperText}</p>}
      </div>
    );
  },
);

Input.displayName = 'Input';
