import React from 'react';
import { PageHeader } from './PageHeader';
import { Skeleton } from '../ui/Skeleton';
import { ErrorState } from '../ui/ErrorState';
import styles from './PageShell.module.css';

export interface PageShellProps {
  title: string;
  description?: string;
  badge?: React.ReactNode;
  headerActions?: React.ReactNode;
  toolbar?: React.ReactNode;
  isLoading?: boolean;
  loadingFallback?: React.ReactNode;
  error?: string | null;
  onRetry?: () => void;
  isEmpty?: boolean;
  emptyState?: React.ReactNode;
  children?: React.ReactNode;
  className?: string;
}

export const PageShell: React.FC<PageShellProps> = ({
  title,
  description,
  badge,
  headerActions,
  toolbar,
  isLoading = false,
  loadingFallback,
  error = null,
  onRetry,
  isEmpty = false,
  emptyState,
  children,
  className,
}) => {
  return (
    <main className={`${styles.container} ${className || ''}`} tabIndex={-1}>
      <PageHeader
        title={title}
        description={description}
        badge={badge}
        actions={headerActions}
      />

      {toolbar && <div className={styles.toolbar}>{toolbar}</div>}

      <div className={styles.content}>
        {isLoading && (
          loadingFallback || (
            <div className={styles.loadingWrapper} aria-busy="true" aria-label="Loading page content">
              <Skeleton variant="rectangular" height={140} />
              <div style={{ display: 'grid', gridTemplateColumns: 'repeat(3, 1fr)', gap: 'var(--space-md)' }}>
                <Skeleton variant="rectangular" height={160} />
                <Skeleton variant="rectangular" height={160} />
                <Skeleton variant="rectangular" height={160} />
              </div>
            </div>
          )
        )}

        {!isLoading && error && (
          <ErrorState
            title="Unable to load content"
            message={error}
            onRetry={onRetry}
          />
        )}

        {!isLoading && !error && isEmpty && (
          emptyState || null
        )}

        {!isLoading && !error && !isEmpty && children}
      </div>
    </main>
  );
};
