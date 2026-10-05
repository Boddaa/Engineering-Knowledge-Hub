import React from 'react';
import { Link } from 'react-router-dom';
import { AlertTriangle, ArrowRight } from 'lucide-react';
import { Badge } from '@/components/ui/Badge';
import { Skeleton } from '@/components/ui/Skeleton';
import { useKnowledgeIncidents } from '../hooks/useKnowledgeIncidents';
import {
  normalizeIncidentStatus,
  normalizeIncidentSeverity,
  getIncidentStatusBadgeVariant,
  getIncidentSeverityBadgeVariant,
  formatDate,
} from '@/features/incidents';
import styles from './KnowledgeLinkedIncidents.module.css';

export interface KnowledgeLinkedIncidentsProps {
  articleId: string;
}

export const KnowledgeLinkedIncidents: React.FC<KnowledgeLinkedIncidentsProps> = ({
  articleId,
}) => {
  const { incidents, isLoading, error } = useKnowledgeIncidents(articleId);

  if (isLoading) {
    return (
      <section className={styles.section} aria-label="Loading linked incidents">
        <Skeleton width="180px" height="20px" />
        <Skeleton width="100%" height="48px" />
      </section>
    );
  }

  if (error || incidents.length === 0) {
    return null; // Do not render empty decorative sections if no real links exist
  }

  return (
    <section className={styles.section} aria-label="Linked operational incidents">
      <div className={styles.header}>
        <div className={styles.titleGroup}>
          <AlertTriangle size={16} className={styles.viewIcon} aria-hidden="true" />
          <h3 className={styles.title}>
            Linked Operational Incidents ({incidents.length})
          </h3>
        </div>
      </div>

      <div className={styles.incidentList}>
        {incidents.map((incident) => {
          const status = normalizeIncidentStatus(incident.status);
          const severity = normalizeIncidentSeverity(incident.severity);

          return (
            <Link
              key={incident.id}
              to={`/incidents/${incident.id}`}
              className={styles.incidentItem}
              aria-label={`View linked incident: ${incident.title}`}
            >
              <div className={styles.incidentInfo}>
                <Badge
                  variant={getIncidentSeverityBadgeVariant(incident.severity)}
                  size="sm"
                >
                  {severity}
                </Badge>
                <Badge
                  variant={getIncidentStatusBadgeVariant(incident.status)}
                  size="sm"
                  dot
                >
                  {status}
                </Badge>
                <span className={styles.incidentTitle}>{incident.title}</span>
              </div>

              <div className={styles.incidentMeta}>
                <span className={styles.dateText}>{formatDate(incident.createdAt)}</span>
                <ArrowRight size={14} className={styles.viewIcon} aria-hidden="true" />
              </div>
            </Link>
          );
        })}
      </div>
    </section>
  );
};
