import React from 'react';
import styles from './Avatar.module.css';

export interface AvatarProps extends React.HTMLAttributes<HTMLDivElement> {
  src?: string;
  name: string;
  size?: 'sm' | 'md' | 'lg';
  status?: 'online' | 'away' | 'offline';
}

export const Avatar: React.FC<AvatarProps> = ({
  src,
  name,
  size = 'md',
  status,
  className,
  ...props
}) => {
  const getInitials = (str: string) => {
    const parts = str.trim().split(/\s+/);
    if (parts.length >= 2) {
      return (parts[0][0] + parts[1][0]).toUpperCase();
    }
    return str.slice(0, 2).toUpperCase();
  };

  return (
    <div
      className={`
        ${styles.avatar}
        ${styles[`size-${size}`]}
        ${className || ''}
      `}
      title={name}
      aria-label={name}
      {...props}
    >
      {src ? (
        <img src={src} alt={name} className={styles.image} />
      ) : (
        <span>{getInitials(name)}</span>
      )}
      {status && (
        <span
          className={`
            ${styles.statusIndicator}
            ${styles[`status-${status}`]}
          `}
          aria-hidden="true"
        />
      )}
    </div>
  );
};
