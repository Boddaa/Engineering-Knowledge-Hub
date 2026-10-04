import React from 'react';
import styles from './Divider.module.css';

export interface DividerProps extends React.HTMLAttributes<HTMLHRElement> {
  orientation?: 'horizontal' | 'vertical';
}

export const Divider: React.FC<DividerProps> = ({
  orientation = 'horizontal',
  className,
  ...props
}) => {
  return (
    <hr
      className={`
        ${styles.divider}
        ${styles[orientation]}
        ${className || ''}
      `}
      aria-orientation={orientation}
      {...props}
    />
  );
};
