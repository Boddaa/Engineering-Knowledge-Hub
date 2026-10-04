import React from 'react';
import styles from './Badge.module.css';

export type BadgeVariant =
  | 'default'
  | 'cyan'
  | 'violet'
  | 'success'
  | 'warning'
  | 'error'
  | 'outline';

export interface BadgeProps extends React.HTMLAttributes<HTMLSpanElement> {
  variant?: BadgeVariant;
  size?: 'sm' | 'md';
  dot?: boolean;
  icon?: React.ReactNode;
}

export const Badge: React.FC<BadgeProps> = ({
  children,
  variant = 'default',
  size = 'md',
  dot = false,
  icon,
  className,
  ...props
}) => {
  return (
    <span
      className={`
        ${styles.badge}
        ${styles[`variant-${variant}`]}
        ${styles[`size-${size}`]}
        ${className || ''}
      `}
      {...props}
    >
      {dot && <span className={styles.dot} aria-hidden="true" />}
      {icon && <span aria-hidden="true">{icon}</span>}
      {children}
    </span>
  );
};
