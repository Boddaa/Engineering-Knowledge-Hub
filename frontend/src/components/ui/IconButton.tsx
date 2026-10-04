import React from 'react';
import styles from './IconButton.module.css';

export interface IconButtonProps extends React.ButtonHTMLAttributes<HTMLButtonElement> {
  'aria-label'?: string;
  label?: string; // Accessible label alias
  icon?: React.ReactNode;
  children?: React.ReactNode;
  size?: 'sm' | 'md';
  variant?: 'ghost' | 'subtle';
}

export const IconButton = React.forwardRef<HTMLButtonElement, IconButtonProps>(
  (
    {
      size = 'md',
      variant = 'ghost',
      icon,
      children,
      label,
      'aria-label': ariaLabel,
      className = '',
      ...props
    },
    ref
  ) => {
    const accessibleLabel = label || ariaLabel || 'Action button';

    const classNames = [
      styles.iconButton,
      styles[`size-${size}`],
      styles[`variant-${variant}`],
      className,
    ]
      .filter(Boolean)
      .join(' ');

    return (
      <button
        ref={ref}
        className={classNames}
        aria-label={accessibleLabel}
        title={accessibleLabel}
        {...props}
      >
        {icon || children}
      </button>
    );
  }
);

IconButton.displayName = 'IconButton';
