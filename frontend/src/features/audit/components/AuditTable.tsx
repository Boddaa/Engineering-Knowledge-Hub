import React, { useState } from 'react';
import { Link } from 'react-router-dom';
import {
  ExternalLink,
  Copy,
  Check,
  ChevronLeft,
  ChevronRight,
  Eye,
  Inbox,
} from 'lucide-react';
import { Badge } from '@/components/ui/Badge';
import { Button } from '@/components/ui/Button';
import { Skeleton } from '@/components/ui/Skeleton';
import type { AuditLogDto, AuditPaginationState } from '../types/auditTypes';
import {
  formatAuditDate,
  getActionBadgeVariant,
  isEntityRoutable,
  getEntityRoute,
  truncateText,
} from '../utils/auditFormatters';
import styles from './AuditTable.module.css';

export interface AuditTableProps {
  logs: AuditLogDto[];
  isLoading: boolean;
  pagination: AuditPaginationState;
  totalPages: number;
  totalFilteredCount: number;
  totalRawCount: number;
  onPageChange: (page: number) => void;
  onInspectLog: (log: AuditLogDto) => void;
}

export const AuditTable: React.FC<AuditTableProps> = ({
  logs,
  isLoading,
  pagination,
  totalPages,
  totalFilteredCount,
  totalRawCount,
  onPageChange,
  onInspectLog,
}) => {
  const [copiedId, setCopiedId] = useState<string | null>(null);

  const handleCopy = (text: string, identifierKey: string) => {
    navigator.clipboard.writeText(text);
    setCopiedId(identifierKey);
    setTimeout(() => {
      setCopiedId(null);
    }, 2000);
  };

  if (isLoading) {
    return (
      <div className={styles.tableContainer} aria-busy="true" aria-label="Loading governance audit logs">
        <table className={styles.table}>
          <thead>
            <tr>
              <th>Timestamp</th>
              <th>Action</th>
              <th>Entity</th>
              <th>User</th>
              <th>Details</th>
              <th style={{ textAlign: 'right' }}>Actions</th>
            </tr>
          </thead>
          <tbody>
            {[1, 2, 3, 4, 5].map((i) => (
              <tr key={i}>
                <td><Skeleton variant="text" width={140} height={16} /></td>
                <td><Skeleton variant="rectangular" width={75} height={22} /></td>
                <td><Skeleton variant="text" width={160} height={16} /></td>
                <td><Skeleton variant="text" width={110} height={16} /></td>
                <td><Skeleton variant="text" width={220} height={16} /></td>
                <td style={{ textAlign: 'right' }}><Skeleton variant="rectangular" width={60} height={28} /></td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>
    );
  }

  if (logs.length === 0) {
    return (
      <div className={styles.tableContainer}>
        <div className={styles.emptyState}>
          <Inbox size={32} />
          <h4 style={{ margin: 0, fontWeight: 600 }}>No audit records found</h4>
          <p style={{ margin: 0, fontSize: '13px' }}>
            {totalRawCount > 0
              ? 'No audit logs match your search filter criteria. Try resetting search.'
              : 'No audit logs have been recorded yet for the selected query scope.'}
          </p>
        </div>
      </div>
    );
  }

  const startRecord = (pagination.page - 1) * pagination.pageSize + 1;
  const endRecord = Math.min(pagination.page * pagination.pageSize, totalFilteredCount);

  return (
    <div className={styles.tableContainer}>
      <table className={styles.table} aria-label="Governance audit log table">
        <thead>
          <tr>
            <th>Timestamp</th>
            <th>Action</th>
            <th>Entity</th>
            <th>User</th>
            <th>Details</th>
            <th style={{ textAlign: 'right' }}>Actions</th>
          </tr>
        </thead>
        <tbody>
          {logs.map((log) => {
            const isRoutable = isEntityRoutable(log.entityName);
            const route = getEntityRoute(log.entityName, log.entityId);
            const entityCopyKey = `entity-${log.id}`;
            const userCopyKey = `user-${log.id}`;

            return (
              <tr key={log.id}>
                {/* Timestamp */}
                <td className={styles.dateCell}>
                  {formatAuditDate(log.createdAt)}
                </td>

                {/* Action */}
                <td>
                  <Badge variant={getActionBadgeVariant(log.action)} size="sm">
                    {log.action || 'Unknown'}
                  </Badge>
                </td>

                {/* Entity */}
                <td>
                  <div className={styles.entityCell}>
                    <div className={styles.entityHeader}>
                      <span className={styles.entityName}>
                        {log.entityName || 'Unspecified'}
                      </span>
                    </div>

                    <div style={{ display: 'flex', alignItems: 'center', gap: '4px' }}>
                      {isRoutable && route ? (
                        <Link
                          to={route}
                          className={styles.entityLink}
                          title={`Navigate to Knowledge Article (${log.entityId})`}
                        >
                          <span>{truncateText(log.entityId, 14)}</span>
                          <ExternalLink size={10} aria-hidden="true" />
                        </Link>
                      ) : (
                        <span
                          className={styles.unsupportedBadge}
                          title={`Entity ID: ${log.entityId} (No direct route for ${log.entityName || 'entity'})`}
                        >
                          <span>{truncateText(log.entityId, 14)}</span>
                        </span>
                      )}

                      <button
                        type="button"
                        className={styles.copyButton}
                        onClick={() => handleCopy(log.entityId, entityCopyKey)}
                        title="Copy Entity UUID to clipboard"
                        aria-label={`Copy Entity UUID ${log.entityId}`}
                      >
                        {copiedId === entityCopyKey ? (
                          <Check size={12} style={{ color: 'var(--color-success)' }} />
                        ) : (
                          <Copy size={12} />
                        )}
                      </button>
                    </div>
                  </div>
                </td>

                {/* User */}
                <td>
                  <span className={styles.userCell} title={`User ID: ${log.userId}`}>
                    <span>{truncateText(log.userId, 14)}</span>
                    <button
                      type="button"
                      className={styles.copyButton}
                      onClick={() => handleCopy(log.userId, userCopyKey)}
                      title="Copy User UUID to clipboard"
                      aria-label={`Copy User UUID ${log.userId}`}
                    >
                      {copiedId === userCopyKey ? (
                        <Check size={12} style={{ color: 'var(--color-success)' }} />
                      ) : (
                        <Copy size={12} />
                      )}
                    </button>
                  </span>
                </td>

                {/* Details Narrative */}
                <td>
                  <div
                    className={`${styles.detailsCell} ${!log.details ? styles.detailsEmpty : ''}`}
                    title={log.details || 'No additional details provided'}
                  >
                    {log.details ? truncateText(log.details, 85) : '—'}
                  </div>
                </td>

                {/* Actions */}
                <td className={styles.actionsCell}>
                  <Button
                    variant="subtle"
                    size="sm"
                    icon={<Eye size={13} />}
                    onClick={() => onInspectLog(log)}
                    aria-label={`Inspect audit log ${log.id}`}
                  >
                    Inspect
                  </Button>
                </td>
              </tr>
            );
          })}
        </tbody>
      </table>

      {/* Pagination Footer */}
      <div className={styles.paginationBar}>
        <div className={styles.paginationSummary}>
          Showing <strong>{startRecord}</strong> to <strong>{endRecord}</strong> of{' '}
          <strong>{totalFilteredCount}</strong> records
          {totalRawCount !== totalFilteredCount && (
            <span> (filtered from {totalRawCount} loaded)</span>
          )}
        </div>

        <div className={styles.paginationControls}>
          <button
            type="button"
            className={styles.pageButton}
            onClick={() => onPageChange(pagination.page - 1)}
            disabled={pagination.page <= 1}
            aria-label="Previous page"
          >
            <ChevronLeft size={14} />
          </button>

          {Array.from({ length: Math.min(5, totalPages) }, (_, idx) => {
            let pageNum = idx + 1;
            if (totalPages > 5) {
              const startOffset = Math.max(1, Math.min(pagination.page - 2, totalPages - 4));
              pageNum = startOffset + idx;
            }

            return (
              <button
                key={pageNum}
                type="button"
                className={`${styles.pageButton} ${pagination.page === pageNum ? styles.pageButtonActive : ''}`}
                onClick={() => onPageChange(pageNum)}
                aria-current={pagination.page === pageNum ? 'page' : undefined}
              >
                {pageNum}
              </button>
            );
          })}

          <button
            type="button"
            className={styles.pageButton}
            onClick={() => onPageChange(pagination.page + 1)}
            disabled={pagination.page >= totalPages}
            aria-label="Next page"
          >
            <ChevronRight size={14} />
          </button>
        </div>
      </div>
    </div>
  );
};
