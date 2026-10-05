import React from 'react';
import {
  AlertTriangle,
  Archive,
  CheckCircle2,
  FileCheck,
  Lock,
  X,
  XCircle,
} from 'lucide-react';
import { Button } from '@/components/ui/Button';
import { Badge } from '@/components/ui/Badge';
import {
  normalizeKnowledgeStatus,
  normalizeVerificationStatus,
  getStatusBadgeVariant,
  getVerificationBadgeVariant,
} from '../utils/knowledgeFormatters';
import type {
  KnowledgeStatus,
  VerificationStatus,
} from '../types/knowledgeTypes';
import type { GovernanceActionType } from '../hooks/useKnowledgeGovernance';
import styles from './GovernanceActionBar.module.css';

export interface GovernanceActionBarProps {
  status: KnowledgeStatus | number;
  verificationStatus: VerificationStatus | number;
  isSubmitting: boolean;
  executingAction: GovernanceActionType | null;
  onOpenAction: (action: GovernanceActionType) => void;
  successFeedback: string | null;
  onDismissFeedback: () => void;
}

export const GovernanceActionBar: React.FC<GovernanceActionBarProps> = ({
  status,
  verificationStatus,
  isSubmitting,
  executingAction,
  onOpenAction,
  successFeedback,
  onDismissFeedback,
}) => {
  const normalizedStatus = normalizeKnowledgeStatus(status);
  const normalizedVerification = normalizeVerificationStatus(verificationStatus);

  // Status-aware lifecycle transitions determined directly from Domain rules:
  // - Submit for Review: valid from Draft or Deprecated
  // - Approve / Reject: valid from PendingReview
  // - Deprecate: valid from Published
  // - Archive: valid from Draft, PendingReview, Verified, Published, Deprecated (not Archived)
  const canSubmitForReview =
    normalizedStatus === 'Draft' || normalizedStatus === 'Deprecated';
  const canApprove = normalizedStatus === 'PendingReview';
  const canReject = normalizedStatus === 'PendingReview';
  const canDeprecate = normalizedStatus === 'Published';
  const canArchive = normalizedStatus !== 'Archived';
  const isArchived = normalizedStatus === 'Archived';

  return (
    <section
      className={styles.actionBarCard}
      aria-label="Article governance and lifecycle management"
    >
      {/* Success Feedback Banner */}
      {successFeedback && (
        <div className={styles.successBanner} role="status" aria-live="polite">
          <div className={styles.successMessage}>
            <CheckCircle2 size={16} aria-hidden="true" />
            <span>{successFeedback}</span>
          </div>
          <button
            type="button"
            className={styles.dismissButton}
            onClick={onDismissFeedback}
            aria-label="Dismiss feedback message"
          >
            <X size={14} />
          </button>
        </div>
      )}

      <div className={styles.mainRow}>
        <div className={styles.governanceInfo}>
          <span className={styles.governanceLabel}>Lifecycle Governance</span>
          <div className={styles.governanceStatus}>
            <Badge variant={getStatusBadgeVariant(status)}>
              {normalizedStatus}
            </Badge>
            <Badge variant={getVerificationBadgeVariant(verificationStatus)} dot>
              {normalizedVerification}
            </Badge>
          </div>
        </div>

        <div className={styles.actionsGroup}>
          {isArchived ? (
            <span className={styles.archivedNotice}>
              <Lock size={14} aria-hidden="true" />
              Article is archived and read-only.
            </span>
          ) : (
            <>
              {/* Submit for Review (Draft or Deprecated) */}
              {canSubmitForReview && (
                <Button
                  variant="primary"
                  size="sm"
                  icon={<FileCheck size={14} />}
                  onClick={() => onOpenAction('submit-review')}
                  disabled={isSubmitting}
                  isLoading={executingAction === 'submit-review'}
                >
                  Submit for Review
                </Button>
              )}

              {/* Approve (Pending Review) */}
              {canApprove && (
                <Button
                  variant="primary"
                  size="sm"
                  icon={<CheckCircle2 size={14} />}
                  onClick={() => onOpenAction('approve')}
                  disabled={isSubmitting}
                  isLoading={executingAction === 'approve'}
                >
                  Approve
                </Button>
              )}

              {/* Reject (Pending Review) */}
              {canReject && (
                <Button
                  variant="danger"
                  size="sm"
                  icon={<XCircle size={14} />}
                  onClick={() => onOpenAction('reject')}
                  disabled={isSubmitting}
                  isLoading={executingAction === 'reject'}
                >
                  Reject
                </Button>
              )}

              {/* Deprecate (Published) */}
              {canDeprecate && (
                <Button
                  variant="subtle"
                  size="sm"
                  icon={<AlertTriangle size={14} />}
                  onClick={() => onOpenAction('deprecate')}
                  disabled={isSubmitting}
                  isLoading={executingAction === 'deprecate'}
                >
                  Deprecate
                </Button>
              )}

              {/* Archive (Any non-archived status) */}
              {canArchive && (
                <Button
                  variant="subtle"
                  size="sm"
                  icon={<Archive size={14} />}
                  onClick={() => onOpenAction('archive')}
                  disabled={isSubmitting}
                  isLoading={executingAction === 'archive'}
                >
                  Archive
                </Button>
              )}
            </>
          )}
        </div>
      </div>
    </section>
  );
};
