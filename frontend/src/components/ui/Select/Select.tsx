import { forwardRef } from 'react';
import { classNames } from '@/utils/classNames';
import { ChevronDown } from 'lucide-react';

interface SelectOption {
  label: string;
  value: string;
  disabled?: boolean;
}

interface SelectProps extends Omit<React.SelectHTMLAttributes<HTMLSelectElement>, 'children'> {
  label?: string;
  options: SelectOption[];
  error?: string;
  placeholder?: string;
  fullWidth?: boolean;
}

export const Select = forwardRef<HTMLSelectElement, SelectProps>(
  ({ label, options, error, placeholder, fullWidth = true, className, id, ...rest }, ref) => {
    const selectId = id || label?.toLowerCase().replace(/\s+/g, '-');

    return (
      <div className={classNames('flex flex-col gap-1.5', fullWidth && 'w-full')}>
        {label && (
          <label
            htmlFor={selectId}
            className="text-xs font-semibold text-wms-muted uppercase tracking-wide"
          >
            {label}
          </label>
        )}
        <div className="relative">
          <select
            ref={ref}
            id={selectId}
            className={classNames(
              'w-full rounded-lg bg-wms-hover border text-sm text-wms-text appearance-none',
              'px-4 py-2.5 pr-10 transition duration-200 outline-none cursor-pointer',
              'focus:border-wms-indigo focus:ring-2 focus:ring-wms-indigo/20',
              error
                ? 'border-wms-danger/50 focus:border-wms-danger focus:ring-wms-danger/20'
                : 'border-wms-border hover:border-wms-border',
              className,
            )}
            {...rest}
          >
            {placeholder && (
              <option value="" disabled className="bg-wms-bg text-wms-muted">
                {placeholder}
              </option>
            )}
            {options.map((opt) => (
              <option
                key={opt.value}
                value={opt.value}
                disabled={opt.disabled}
                className="bg-wms-bg text-wms-text"
              >
                {opt.label}
              </option>
            ))}
          </select>
          <ChevronDown className="absolute right-3 top-1/2 -translate-y-1/2 h-4 w-4 text-wms-muted pointer-events-none" />
        </div>
        {error && <p className="text-xs text-wms-danger">{error}</p>}
      </div>
    );
  },
);

Select.displayName = 'Select';
