import React from 'react';
import { Link } from 'react-router-dom';
import { ArrowRight, Calendar, User, BookOpen, AlertTriangle } from 'lucide-react';
import type { IncidentDto } from '../types/incidentTypes';
import {
  normalizeIncidentStatus,
  normalizeIncidentSeverity,
  getIncidentStatusBadgeVariant,
  getIncidentSeverityBadgeVariant,
  formatDate,
} from '../utils/incidentFormatters';
import { Badge } from '@/components/ui/Badge';
import styles from './IncidentCard.module.css';

export interface IncidentCardProps {
  incident: IncidentDto;
}

export const IncidentCard: React.FC<IncidentCardProps> = ({ incident }) => {
  const status = normalizeIncidentStatus(incident.status);
  const severity = normalizeIncidentSeverity(incident.severity);
  const createdDate = formatDate(incident.createdAt);
  const linkCount = incident.articleLinks ? incident.articleLinks.length : 0;

  return (
    <Link
      to={`/incidents/${incident.id}`}
      className={styles.card}
      aria-label={`View operational incident: ${incident.title}`}
    >
      <div className={styles.header}>
        <h3 className={styles.title}>{incident.title}</h3>
        <div className={styles.badgeGroup}>
          <Badge
            variant={getIncidentSeverityBadgeVariant(incident.severity)}
            size="sm"
            icon={<AlertTriangle size={11} aria-hidden="true" />}
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
        </div>
      </div>

      <p className={styles.description}>
        {incident.problemDescription || 'No description recorded.'}
      </p>

      {incident.symptoms && (
        <p className={styles.symptoms} title={incident.symptoms}>
          Symptoms: {incident.symptoms}
        </p>
      )}

      <div className={styles.footer}>
        <div className={styles.metaGroup}>
          <div className={styles.metaItem}>
            <Calendar size={13} aria-hidden="true" />
            <span>{createdDate}</span>
          </div>

          <div className={styles.metaItem} title={`Created by ${incident.createdById}`}>
            <User size={13} aria-hidden="true" />
            <span className={styles.codeSnippet}>
              {incident.createdById.substring(0, 8)}...
            </span>
          </div>

          {linkCount > 0 && (
            <div className={styles.metaItem} title={`${linkCount} linked knowledge articles`}>
              <BookOpen size={13} aria-hidden="true" />
              <span>{linkCount} linked article{linkCount > 1 ? 's' : ''}</span>
            </div>
          )}
        </div>

        <span className={styles.viewAction}>
          Inspect <ArrowRight size={13} aria-hidden="true" />
        </span>
      </div>
    </Link>
  );
};
