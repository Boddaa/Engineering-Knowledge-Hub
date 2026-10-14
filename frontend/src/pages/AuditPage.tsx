import React, { useState } from 'react';
import { RefreshCw, Shield, Activity, Layers, Filter } from 'lucide-react';
import { PageShell } from '@/components/layout/PageShell';
import { Button } from '@/components/ui/Button';
import { ErrorState } from '@/components/ui/ErrorState';
import {
  useAuditLogs,
  AuditFilterBar,
  AuditTable,
  AuditDetailModal,
  type AuditLogDto,
} from '@/features/audit';
import styles from './AuditPage.module.css';

export const AuditPage: React.FC = () => {
  const {
    paginatedLogs,
    totalRawCount,
    totalFilteredCount,
    pagination,
    totalPages,
    isLoading,
    error,
    queryStatus,
    filters,
    setFilters,
    setPage,
    setPageSize,
    refetch,
  } = useAuditLogs();

  const [inspectingLog, setInspectingLog] = useState<AuditLogDto | null>(null);

  const handleResetFilters = () => {
    setFilters({
      mode: 'all',
      entityName: '',
      entityId: '',
      userId: '',
      searchTerm: '',
    });
  };

  const getScopeLabel = () => {
    if (filters.mode === 'entity') {
      return `Entity: ${filters.entityName || 'Unset'}`;
    }
    if (filters.mode === 'user') {
      return 'User Audit Trail';
    }
    return 'All Records';
  };

  const getObservabilityStatus = () => {
    switch (queryStatus) {
      case 'idle':
        return {
          label: 'Not queried yet',
          color: 'var(--color-text-secondary)',
        };
      case 'loading':
        return {
          label: 'Querying...',
          color: 'var(--color-text-secondary)',
        };
      case 'success':
        return {
          label: 'Query successful',
          color: 'var(--color-success)',
        };
      case 'error':
        return {
          label: 'Unavailable / Error',
          color: 'var(--color-error)',
        };
    }
  };

  const statusInfo = getObservabilityStatus();

  return (
    <PageShell
      title="System Governance & Audit Logs"
      description="System compliance audit trail, entity lifecycle transitions, and user observability."
      headerActions={
        <Button
          variant="subtle"
          icon={<RefreshCw size={14} className={isLoading ? 'animate-spin' : ''} />}
          onClick={refetch}
          disabled={isLoading}
          aria-label="Refresh audit logs"
        >
          Refresh Logs
        </Button>
      }
    >
      {/* Observability Stats Cards */}
      <div className={styles.statsGrid}>
        <div className={styles.statCard}>
          <div className={styles.statIconWrapper}>
            <Shield size={20} />
          </div>
          <div className={styles.statContent}>
            <span className={styles.statLabel}>Audit Query Status</span>
            <span className={styles.statValue} style={{ color: statusInfo.color, fontSize: '15px' }}>
              {statusInfo.label}
            </span>
          </div>
        </div>

        <div className={styles.statCard}>
          <div className={styles.statIconWrapper}>
            <Layers size={20} />
          </div>
          <div className={styles.statContent}>
            <span className={styles.statLabel}>Loaded In Memory</span>
            <span className={styles.statValue}>{isLoading ? '...' : totalRawCount}</span>
          </div>
        </div>

        <div className={styles.statCard}>
          <div className={styles.statIconWrapper}>
            <Filter size={20} />
          </div>
          <div className={styles.statContent}>
            <span className={styles.statLabel}>Filtered Matches</span>
            <span className={styles.statValue}>{isLoading ? '...' : totalFilteredCount}</span>
          </div>
        </div>

        <div className={styles.statCard}>
          <div className={styles.statIconWrapper}>
            <Activity size={20} />
          </div>
          <div className={styles.statContent}>
            <span className={styles.statLabel}>Current Scope</span>
            <span className={styles.statValue} style={{ fontSize: '14px' }}>
              {getScopeLabel()}
            </span>
          </div>
        </div>
      </div>

      {/* Filter and Search Bar */}
      <AuditFilterBar
        filters={filters}
        onFilterChange={setFilters}
        pagination={pagination}
        onPageSizeChange={setPageSize}
        onReset={handleResetFilters}
        isLoading={isLoading}
      />

      {/* Error state if query failed or validation rejected */}
      {error && (
        <div className={styles.errorBanner}>
          <ErrorState
            title="Unable to load audit logs"
            message={error}
            onRetry={refetch}
            actionText="Retry Request"
          />
        </div>
      )}

      {/* Main Table with Bounded Client-Side Pagination */}
      <AuditTable
        logs={paginatedLogs}
        isLoading={isLoading}
        pagination={pagination}
        totalPages={totalPages}
        totalFilteredCount={totalFilteredCount}
        totalRawCount={totalRawCount}
        onPageChange={setPage}
        onInspectLog={(log) => setInspectingLog(log)}
      />

      {/* Modal for Deep Record Inspection */}
      <AuditDetailModal
        log={inspectingLog}
        isOpen={inspectingLog !== null}
        onClose={() => setInspectingLog(null)}
      />
    </PageShell>
  );
};
