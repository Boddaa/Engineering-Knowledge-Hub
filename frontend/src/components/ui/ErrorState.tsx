import React from 'react';
import { AlertCircle, RefreshCw } from 'lucide-react';
import { Button } from './Button';
import styles from './ErrorState.module.css';

export interface ErrorStateProps {
  title?: string;
  message?: string;
  details?: string;
  onRetry?: () => void;
  actionText?: string;
  className?: string;
}

export const ErrorState: React.FC<ErrorStateProps> = ({
  title = 'Something went wrong',
  message = 'An unexpected error occurred while loading this view. Please try again.',
  details,
  onRetry,
  actionText = 'Try Again',
  className,
}) => {
  return (
    <div className={`${styles.errorState} ${className || ''}`} role="alert">
      <div className={styles.iconWrapper}>
        <AlertCircle size={28} />
      </div>
      <h3 className={styles.title}>{title}</h3>
      <p className={styles.message}>{message}</p>
      {details && <div className={styles.details}>{details}</div>}
      {onRetry && (
        <div className={styles.actions}>
          <Button
            variant="subtle"
            size="md"
            icon={<RefreshCw size={14} />}
            onClick={onRetry}
          >
            {actionText}
          </Button>
        </div>
      )}
    </div>
  );
};
