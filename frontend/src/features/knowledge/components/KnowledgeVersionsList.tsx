import React, { useState } from 'react';
import {
  Calendar,
  Eye,
  EyeOff,
  History,
  Lock,
  RotateCcw,
  User,
  X,
} from 'lucide-react';
import { Badge } from '@/components/ui/Badge';
import { Button } from '@/components/ui/Button';
import { Skeleton } from '@/components/ui/Skeleton';
import type { KnowledgeVersionDto } from '../types/knowledgeTypes';
import { formatDate } from '../utils/knowledgeFormatters';
import styles from './KnowledgeVersionsList.module.css';

export interface KnowledgeVersionsListProps {
  versions: KnowledgeVersionDto[];
  isLoading: boolean;
  error: string | null;
  onRetry: () => void;
}

export const KnowledgeVersionsList: React.FC<KnowledgeVersionsListProps> = ({
  versions,
  isLoading,
  error,
  onRetry,
}) => {
  const [activeVersionNumber, setActiveVersionNumber] = useState<number | null>(null);

  const activeSnapshot =
    activeVersionNumber !== null
      ? versions.find((v) => v.versionNumber === activeVersionNumber) || null
      : null;

  const handleToggleSnapshot = (versionNumber: number) => {
    setActiveVersionNumber((prev) => (prev === versionNumber ? null : versionNumber));
  };

  return (
    <section className={styles.versionsSection} aria-label="Version history">
      <div className={styles.sectionHeader}>
        <div className={styles.titleGroup}>
          <History size={18} className="text-accent" aria-hidden="true" />
          <h3 className={styles.title}>Version History</h3>
          <Badge variant="default" size="sm">
            {versions.length}
          </Badge>
        </div>
      </div>

      {/* Loading Skeleton */}
      {isLoading && (
        <div className={styles.loadingSkeleton} aria-busy="true" aria-label="Loading versions">
          <Skeleton variant="rectangular" height={60} />
          <Skeleton variant="rectangular" height={60} />
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
      {!isLoading && !error && versions.length === 0 && (
        <div className={styles.emptyState}>
          <p>No versions available yet.</p>
          <span className="text-caption">
            Historical snapshots are created automatically upon article approval and publication.
          </span>
        </div>
      )}

      {/* Versions List */}
      {!isLoading && !error && versions.length > 0 && (
        <div className={styles.versionList}>
          {versions.map((ver) => {
            const isSelected = activeVersionNumber === ver.versionNumber;
            return (
              <div key={ver.id} className={styles.versionCard}>
                <div className={styles.versionHeader}>
                  <div className={styles.versionMeta}>
                    <Badge variant={ver.versionNumber === 1 ? 'default' : 'cyan'} size="sm">
                      v{ver.versionNumber}
                    </Badge>
                    <span className={styles.metaItem}>
                      <Calendar size={12} aria-hidden="true" />
                      {formatDate(ver.createdAt)}
                    </span>
                    <span className={styles.metaItemMono} title={ver.changedById}>
                      <User size={12} aria-hidden="true" />
                      {ver.changedById.substring(0, 8)}...
                    </span>
                  </div>

                  <Button
                    variant={isSelected ? 'primary' : 'subtle'}
                    size="sm"
                    icon={isSelected ? <EyeOff size={13} /> : <Eye size={13} />}
                    onClick={() => handleToggleSnapshot(ver.versionNumber)}
                    aria-expanded={isSelected}
                    aria-label={`${isSelected ? 'Hide' : 'Inspect'} snapshot for version ${ver.versionNumber}`}
                  >
                    {isSelected ? 'Close Snapshot' : 'Inspect Snapshot'}
                  </Button>
                </div>

                <p className={styles.summaryText}>
                  {ver.changeSummary || 'Approved and published.'}
                </p>
              </div>
            );
          })}
        </div>
      )}

      {/* Snapshot Inspection View */}
      {activeSnapshot && (
        <div
          className={styles.snapshotView}
          role="region"
          aria-label={`Snapshot details for version ${activeSnapshot.versionNumber}`}
        >
          <div className={styles.snapshotBanner}>
            <div className={styles.snapshotNotice}>
              <Lock size={14} aria-hidden="true" />
              <span>Snapshot: Immutable Historical Version (v{activeSnapshot.versionNumber}) — Read-Only</span>
            </div>
            <Button
              variant="ghost"
              size="sm"
              icon={<X size={14} />}
              onClick={() => setActiveVersionNumber(null)}
              aria-label="Close snapshot inspector"
            >
              Close
            </Button>
          </div>

          <div className={styles.snapshotContent}>
            <div>
              <span className="text-caption">Historical Title</span>
              <h4 className={styles.snapshotTitle}>{activeSnapshot.title}</h4>
            </div>

            <div className={styles.versionMeta}>
              <span className={styles.metaItem}>
                <strong>Changed by:</strong>
                <span className="text-code">{activeSnapshot.changedById}</span>
              </span>
              <span className={styles.metaItem}>
                <strong>Recorded on:</strong>
                <span>{formatDate(activeSnapshot.createdAt)}</span>
              </span>
            </div>

            <div>
              <span className="text-caption">Historical Content</span>
              <div className={styles.snapshotBody}>
                {activeSnapshot.content}
              </div>
            </div>
          </div>
        </div>
      )}
    </section>
  );
};
