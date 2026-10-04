import React from 'react';
import { ChevronLeft, ChevronRight } from 'lucide-react';
import { Button } from '@/components/ui/Button';
import styles from './KnowledgePagination.module.css';

export interface KnowledgePaginationProps {
  page: number;
  totalPages: number;
  totalCount: number;
  hasPreviousPage: boolean;
  hasNextPage: boolean;
  isLoading?: boolean;
  onPageChange: (newPage: number) => void;
}

export const KnowledgePagination: React.FC<KnowledgePaginationProps> = ({
  page,
  totalPages,
  totalCount,
  hasPreviousPage,
  hasNextPage,
  isLoading = false,
  onPageChange,
}) => {
  // If there are 0 or 1 pages and no items, don't render pagination controls
  if (totalPages <= 1 && totalCount <= 0) {
    return null;
  }

  const effectiveTotalPages = Math.max(1, totalPages);

  return (
    <nav className={styles.paginationContainer} aria-label="Knowledge articles pagination">
      <Button
        variant="subtle"
        size="sm"
        icon={<ChevronLeft size={16} />}
        disabled={!hasPreviousPage || page <= 1 || isLoading}
        onClick={() => onPageChange(page - 1)}
        aria-label="Go to previous page"
      >
        Previous
      </Button>

      <div className={styles.pageInfo}>
        <span>
          Page <strong>{page}</strong> of <strong>{effectiveTotalPages}</strong>
        </span>
        <span className={styles.totalBadge}>
          ({totalCount} article{totalCount !== 1 ? 's' : ''})
        </span>
      </div>

      <Button
        variant="subtle"
        size="sm"
        rightIcon={<ChevronRight size={16} />}
        disabled={!hasNextPage || page >= effectiveTotalPages || isLoading}
        onClick={() => onPageChange(page + 1)}
        aria-label="Go to next page"
      >
        Next
      </Button>
    </nav>
  );
};
