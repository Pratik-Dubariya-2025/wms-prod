import type { ReactNode } from 'react';
import { classNames } from '@/utils/classNames';

interface PageWrapperProps {
  children: ReactNode;
  className?: string;
  /** Max width constraint */
  maxWidth?: 'sm' | 'md' | 'lg' | 'xl' | 'full';
}

const maxWidthClasses: Record<string, string> = {
  sm: 'max-w-3xl',
  md: 'max-w-5xl',
  lg: 'max-w-7xl',
  xl: 'max-w-[90rem]',
  full: 'max-w-full',
};

/**
 * Consistent page padding/spacing wrapper.
 */
export function PageWrapper({ children, className, maxWidth = 'xl' }: PageWrapperProps) {
  return (
    <div className={classNames('mx-auto w-full', maxWidthClasses[maxWidth], className)}>
      {children}
    </div>
  );
}
