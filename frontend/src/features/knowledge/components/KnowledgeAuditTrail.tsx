import React from 'react';
import {
  Clock,
  RotateCcw,
  ShieldCheck,
  User,
} from 'lucide-react';
import { Badge } from '@/components/ui/Badge';
import { Button } from '@/components/ui/Button';
import { Skeleton } from '@/components/ui/Skeleton';
import type { AuditLogDto } from '../types/knowledgeTypes';
import { formatDate } from '../utils/knowledgeFormatters';
import styles from './KnowledgeAuditTrail.module.css';

export interface KnowledgeAuditTrailProps {
  auditLogs: AuditLogDto[];
  isLoading: boolean;
  error: string | null;
  onRetry: () => void;
}

function formatActionLabel(action: string): string {
  // e.g. "ArticleSubmittedForReview" -> "Article Submitted For Review"
  return action.replace(/([a-z])([A-Z])/g, '$1 $2');
}

export const KnowledgeAuditTrail: React.FC<KnowledgeAuditTrailProps> = ({
  auditLogs,
  isLoading,
  error,
  onRetry,
}) => {
  return (
    <section className={styles.auditSection} aria-label="Governance audit trail">
      <div className={styles.sectionHeader}>
        <div className={styles.titleGroup}>
          <ShieldCheck size={18} className="text-accent" aria-hidden="true" />
          <h3 className={styles.title}>Audit History</h3>
          <Badge variant="default" size="sm">
            {auditLogs.length}
          </Badge>
        </div>
      </div>

      {/* Loading Skeleton */}
      {isLoading && (
        <div className={styles.loadingSkeleton} aria-busy="true" aria-label="Loading audit history">
          <Skeleton variant="rectangular" height={48} />
          <Skeleton variant="rectangular" height={48} />
          <Skeleton variant="rectangular" height={48} />
        </div>
      )}

      {/* Error State */}
      {!isLoading && error && (
        <div className={styles.errorState} role="alert">
          <span>{error}</span>
          <Button
            variant="subtle"
            size="sm"
            icon={<RotateCcw size={14} />}
            onClick={onRetry}
          >
            Retry
          </Button>
        </div>
      )}

      {/* Empty State */}
      {!isLoading && !error && auditLogs.length === 0 && (
        <div className={styles.emptyState}>
          <p>No audit history available yet.</p>
        </div>
      )}

      {/* Timeline List */}
      {!isLoading && !error && auditLogs.length > 0 && (
        <div className={styles.timelineList}>
          {auditLogs.map((log) => (
            <div key={log.id} className={styles.timelineItem}>
              <div className={styles.timelineTrack}>
                <div className={styles.timelineNode} />
                <div className={styles.timelineLine} />
              </div>

              <div className={styles.itemContent}>
                <div className={styles.itemHeader}>
                  <span className={styles.actionTag} title={log.action}>
                    {formatActionLabel(log.action)}
                  </span>
                  <span className={styles.timestamp}>
                    <Clock size={12} aria-hidden="true" />
                    {formatDate(log.createdAt)}
                  </span>
                </div>

                <div className={styles.details}>{log.details}</div>

                <div className={styles.actorMeta}>
                  <span title={log.userId}>
                    <User size={11} style={{ display: 'inline', marginRight: 3 }} aria-hidden="true" />
                    Actor: {log.userId.substring(0, 8)}...
                  </span>
                  {log.entityName && <span>Target: {log.entityName}</span>}
                </div>
              </div>
            </div>
          ))}
        </div>
      )}
    </section>
  );
};
