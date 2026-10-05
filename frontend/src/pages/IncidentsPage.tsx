import React from 'react';
import { useNavigate } from 'react-router-dom';
import { Plus, AlertTriangle } from 'lucide-react';
import { PageShell } from '@/components/layout/PageShell';
import { Button } from '@/components/ui/Button';
import { Skeleton } from '@/components/ui/Skeleton';
import { EmptyState } from '@/components/ui/EmptyState';
import { ErrorState } from '@/components/ui/ErrorState';
import {
  useIncidents,
  IncidentCard,
  IncidentStatusFilter,
} from '@/features/incidents';
import styles from './IncidentsPage.module.css';

export const IncidentsPage: React.FC = () => {
  const navigate = useNavigate();
  const {
    incidents,
    isLoading,
    error,
    selectedStatus,
    changeStatusFilter,
    retry,
  } = useIncidents();

  const handleReportIncident = () => {
    navigate('/incidents/new');
  };

  return (
    <PageShell
      title="Operational Incidents"
      description="Production failures, active investigations, root cause analyses, and runbook linkages."
      headerActions={
        <Button
          variant="primary"
          icon={<Plus size={16} />}
          onClick={handleReportIncident}
        >
          Report Incident
        </Button>
      }
    >
      <div className={styles.container}>
        {/* Status Filter & Metrics Toolbar */}
        <div className={styles.topBar}>
          <IncidentStatusFilter
            selectedStatus={selectedStatus}
            onSelectStatus={changeStatusFilter}
          />

          {!isLoading && !error && (
            <span className={styles.countLabel}>
              Showing {incidents.length} incident{incidents.length === 1 ? '' : 's'}
            </span>
          )}
        </div>

        {/* Loading State */}
        {isLoading && (
          <div className={styles.skeletonList} aria-label="Loading incidents list">
            {[1, 2, 3, 4].map((n) => (
              <div key={n} className={styles.skeletonCard}>
                <div className={styles.skeletonHeader}>
                  <Skeleton width="45%" height="22px" />
                  <Skeleton width="120px" height="20px" />
                </div>
                <Skeleton width="90%" height="16px" />
                <Skeleton width="70%" height="16px" />
                <div className={styles.skeletonMeta}>
                  <Skeleton width="100px" height="14px" />
                  <Skeleton width="120px" height="14px" />
                </div>
              </div>
            ))}
          </div>
        )}

        {/* Error State */}
        {!isLoading && error && (
          <ErrorState
            title="Failed to load incidents"
            message={error}
            onRetry={retry}
          />
        )}

        {/* Empty State */}
        {!isLoading && !error && incidents.length === 0 && (
          <EmptyState
            icon={<AlertTriangle size={36} />}
            title={
              selectedStatus
                ? `No incidents found with status "${selectedStatus}"`
                : 'No operational incidents recorded yet'
            }
            description={
              selectedStatus
                ? 'Try selecting a different status filter or view all recorded incidents.'
                : 'Record your first operational incident to begin tracking symptoms, root causes, and verified knowledge resolutions.'
            }
            action={
              selectedStatus ? (
                <Button variant="subtle" onClick={() => changeStatusFilter(undefined)}>
                  Clear Status Filter
                </Button>
              ) : (
                <Button
                  variant="primary"
                  icon={<Plus size={16} />}
                  onClick={handleReportIncident}
                >
                  Report Incident
                </Button>
              )
            }
          />
        )}

        {/* Incidents List */}
        {!isLoading && !error && incidents.length > 0 && (
          <div className={styles.incidentList} role="feed" aria-label="Operational incidents feed">
            {incidents.map((incident) => (
              <IncidentCard key={incident.id} incident={incident} />
            ))}
          </div>
        )}
      </div>
    </PageShell>
  );
};
