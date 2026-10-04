import React from 'react';
import styles from './Panel.module.css';

export interface PanelProps extends React.HTMLAttributes<HTMLDivElement> {
  elevation?: 0 | 1 | 2 | 3;
  padding?: 'none' | 'sm' | 'md' | 'lg' | 'xl';
  interactive?: boolean;
}

export const Panel: React.FC<PanelProps> = ({
  children,
  elevation = 1,
  padding = 'md',
  interactive = false,
  className,
  ...props
}) => {
  return (
    <div
      className={`
        ${styles.panel}
        ${styles[`elevation-${elevation}`]}
        ${styles[`padding-${padding}`]}
        ${interactive ? styles.interactive : ''}
        ${className || ''}
      `}
      {...props}
    >
      {children}
    </div>
  );
};
