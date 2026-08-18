import React from 'react';
import * as SwitchPrimitives from '@radix-ui/react-switch';
import { classNames } from '@/utils/classNames';

export interface SwitchProps {
  checked: boolean;
  onChange: (checked: boolean) => void;
  disabled?: boolean;
  isLoading?: boolean;
  size?: 'sm' | 'md' | 'lg';
  id?: string;
  name?: string;
  label?: string;
  ariaLabel?: string;
  className?: string;
}

const sizeConfig = {
  sm: {
    root: 'w-8 h-4.5 p-0.5',
    thumb: 'w-3.5 h-3.5 data-[state=checked]:translate-x-3.5',
  },
  md: {
    root: 'w-10 h-5.5 p-0.5',
    thumb: 'w-4.5 h-4.5 data-[state=checked]:translate-x-4.5',
  },
  lg: {
    root: 'w-12 h-6.5 p-0.5',
    thumb: 'w-5.5 h-5.5 data-[state=checked]:translate-x-5.5',
  },
};

export const Switch: React.FC<SwitchProps> = ({
  checked,
  onChange,
  disabled = false,
  isLoading = false,
  size = 'sm',
  id,
  name,
  label,
  ariaLabel,
  className,
}) => {
  const config = sizeConfig[size] || sizeConfig.sm;
  const isDisabled = disabled || isLoading;

  return (
    <div className={classNames('inline-flex items-center gap-2.5', className)}>
      <SwitchPrimitives.Root
        id={id}
        name={name}
        checked={checked}
        onCheckedChange={onChange}
        disabled={isDisabled}
        aria-label={ariaLabel || label}
        className={classNames(
          'peer relative inline-flex flex-shrink-0 cursor-pointer items-center rounded-full transition-colors duration-200 ease-in-out outline-none focus-visible:ring-2 focus-visible:ring-wms-indigo focus-visible:ring-offset-2 disabled:cursor-not-allowed disabled:opacity-50',
          config.root,
          checked
            ? 'bg-wms-indigo shadow-sm shadow-wms-indigo/30'
            : 'bg-slate-300 dark:bg-slate-700/80 border border-slate-300/40 dark:border-slate-600/50'
        )}
      >
        <SwitchPrimitives.Thumb
          className={classNames(
            'pointer-events-none block rounded-full bg-white shadow-md ring-0 transition-transform duration-200 ease-in-out data-[state=unchecked]:translate-x-0 flex items-center justify-center',
            config.thumb
          )}
        >
          {isLoading && (
            <span className="w-2.5 h-2.5 border-2 border-wms-indigo border-t-transparent rounded-full animate-spin" />
          )}
        </SwitchPrimitives.Thumb>
      </SwitchPrimitives.Root>

      {label && (
        <label
          htmlFor={id}
          className={classNames(
            'text-sm font-medium text-wms-text select-none cursor-pointer',
            isDisabled && 'opacity-50 cursor-not-allowed'
          )}
        >
          {label}
        </label>
      )}
    </div>
  );
};
