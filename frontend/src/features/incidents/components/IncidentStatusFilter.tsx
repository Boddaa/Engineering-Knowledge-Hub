import React from 'react';
import type { IncidentStatus } from '../types/incidentTypes';
import { VALID_INCIDENT_STATUSES } from '../utils/incidentFormatters';
import styles from './IncidentStatusFilter.module.css';

export interface IncidentStatusFilterProps {
  selectedStatus: IncidentStatus | undefined;
  onSelectStatus: (status?: IncidentStatus) => void;
}

export const IncidentStatusFilter: React.FC<IncidentStatusFilterProps> = ({
  selectedStatus,
  onSelectStatus,
}) => {
  return (
    <div
      className={styles.filterBar}
      role="group"
      aria-label="Filter incidents by lifecycle status"
    >
      <button
        type="button"
        className={`${styles.filterButton} ${selectedStatus === undefined ? styles.isActive : ''}`}
        onClick={() => onSelectStatus(undefined)}
        aria-pressed={selectedStatus === undefined}
      >
        All Incidents
      </button>

      {VALID_INCIDENT_STATUSES.map((status) => {
        const isActive = selectedStatus === status;
        let dotClass = styles.dotOpen;
        if (status === 'Investigating') dotClass = styles.dotInvestigating;
        if (status === 'Resolved') dotClass = styles.dotResolved;
        if (status === 'Closed') dotClass = styles.dotClosed;

        return (
          <button
            key={status}
            type="button"
            className={`${styles.filterButton} ${isActive ? styles.isActive : ''}`}
            onClick={() => onSelectStatus(status)}
            aria-pressed={isActive}
          >
            <span className={`${styles.dot} ${dotClass}`} aria-hidden="true" />
            {status}
          </button>
        );
      })}
    </div>
  );
};
