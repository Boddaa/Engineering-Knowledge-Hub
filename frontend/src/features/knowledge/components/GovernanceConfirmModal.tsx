import React, { useState, useEffect, useRef } from 'react';
import {
  AlertCircle,
  AlertTriangle,
  Archive,
  CheckCircle2,
  FileCheck,
  X,
  XCircle,
} from 'lucide-react';
import { Button } from '@/components/ui/Button';
import { Input } from '@/components/ui/Input';
import type { GovernanceActionType } from '../hooks/useKnowledgeGovernance';
import styles from './GovernanceConfirmModal.module.css';

export interface GovernanceConfirmModalProps {
  isOpen: boolean;
  action: GovernanceActionType | null;
  articleTitle: string;
  isSubmitting: boolean;
  apiError: string | null;
  onClose: () => void;
  onConfirm: (payload: {
    reviewerId?: string;
    submitterId?: string;
    actorId?: string;
    changeSummary?: string;
  }) => void;
}

const UUID_REGEX = /^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$/i;

export const GovernanceConfirmModal: React.FC<GovernanceConfirmModalProps> = ({
  isOpen,
  action,
  articleTitle,
  isSubmitting,
  apiError,
  onClose,
  onConfirm,
}) => {
  const [reviewerId, setReviewerId] = useState('');
  const [submitterId, setSubmitterId] = useState('');
  const [actorId, setActorId] = useState('');
  const [changeSummary, setChangeSummary] = useState('');
  const [validationError, setValidationError] = useState<string | null>(null);

  const initialInputRef = useRef<HTMLInputElement>(null);
  const cancelButtonRef = useRef<HTMLButtonElement>(null);

  // Reset local state when opened/action changes
  useEffect(() => {
    if (isOpen) {
      setReviewerId('');
      setSubmitterId('');
      setActorId('');
      setChangeSummary('');
      setValidationError(null);

      // Focus first input or cancel button after mount
      const timer = setTimeout(() => {
        if (initialInputRef.current) {
          initialInputRef.current.focus();
        } else if (cancelButtonRef.current) {
          cancelButtonRef.current.focus();
        }
      }, 50);
      return () => clearTimeout(timer);
    }
  }, [isOpen, action]);

  // Handle ESC key to dismiss
  useEffect(() => {
    if (!isOpen) return;

    const handleKeyDown = (e: KeyboardEvent) => {
      if (e.key === 'Escape' && !isSubmitting) {
        onClose();
      }
    };

    window.addEventListener('keydown', handleKeyDown);
    return () => window.removeEventListener('keydown', handleKeyDown);
  }, [isOpen, isSubmitting, onClose]);

  if (!isOpen || !action) {
    return null;
  }

  const validateAndSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    setValidationError(null);

    if (action === 'approve' || action === 'reject') {
      const trimmed = reviewerId.trim();
      if (!trimmed) {
        setValidationError('Reviewer ID is required.');
        return;
      }
      if (!UUID_REGEX.test(trimmed)) {
        setValidationError('Reviewer ID must be a valid UUID (e.g. 00000000-0000-0000-0000-000000000000).');
        return;
      }
    }

    if (action === 'submit-review' && submitterId.trim()) {
      if (!UUID_REGEX.test(submitterId.trim())) {
        setValidationError('Submitter ID must be a valid UUID.');
        return;
      }
    }

    if ((action === 'deprecate' || action === 'archive') && actorId.trim()) {
      if (!UUID_REGEX.test(actorId.trim())) {
        setValidationError('Actor ID must be a valid UUID.');
        return;
      }
    }

    onConfirm({
      reviewerId: reviewerId.trim() || undefined,
      submitterId: submitterId.trim() || undefined,
      actorId: actorId.trim() || undefined,
      changeSummary: changeSummary.trim() || undefined,
    });
  };

  const getModalConfig = () => {
    switch (action) {
      case 'submit-review':
        return {
          title: 'Submit Article for Review?',
          description: `Submit "${articleTitle}" for peer review. The article will transition to Pending Review status.`,
          icon: <FileCheck size={20} />,
          iconClass: styles.iconDefault,
          confirmLabel: 'Submit for Review',
          confirmVariant: 'primary' as const,
        };
      case 'approve':
        return {
          title: 'Approve and Publish Article?',
          description: `Approving "${articleTitle}" marks it as Verified and publishes it. An immutable historical version snapshot will be generated automatically.`,
          icon: <CheckCircle2 size={20} />,
          iconClass: styles.iconDefault,
          confirmLabel: 'Approve & Publish',
          confirmVariant: 'primary' as const,
        };
      case 'reject':
        return {
          title: 'Reject Article?',
          description: `Rejecting "${articleTitle}" returns it to Draft status with Rejected verification so the author can revise it.`,
          icon: <XCircle size={20} />,
          iconClass: styles.iconDanger,
          confirmLabel: 'Reject Article',
          confirmVariant: 'danger' as const,
        };
      case 'deprecate':
        return {
          title: 'Deprecate Article?',
          description: `Deprecating "${articleTitle}" flags it as superseded or outdated in search results and technical catalogs.`,
          icon: <AlertTriangle size={20} />,
          iconClass: styles.iconWarning,
          confirmLabel: 'Deprecate Article',
          confirmVariant: 'danger' as const,
        };
      case 'archive':
        return {
          title: 'Archive Article?',
          description: `Archiving "${articleTitle}" permanently locks it for reference. It will become read-only and no further governance transitions can be performed.`,
          icon: <Archive size={20} />,
          iconClass: styles.iconDanger,
          confirmLabel: 'Archive Article',
          confirmVariant: 'danger' as const,
        };
    }
  };

  const config = getModalConfig();

  return (
    <div
      className={styles.backdrop}
      onClick={(e) => {
        if (e.target === e.currentTarget && !isSubmitting) {
          onClose();
        }
      }}
      role="presentation"
    >
      <div
        className={styles.modal}
        role="dialog"
        aria-modal="true"
        aria-labelledby="gov-modal-title"
        aria-describedby="gov-modal-desc"
      >
        {/* Header */}
        <div className={styles.header}>
          <div className={styles.headerContent}>
            <div className={`${styles.iconContainer} ${config.iconClass}`} aria-hidden="true">
              {config.icon}
            </div>
            <h2 id="gov-modal-title" className={styles.title}>
              {config.title}
            </h2>
          </div>
          <button
            type="button"
            className={styles.closeButton}
            onClick={onClose}
            disabled={isSubmitting}
            aria-label="Close dialog"
          >
            <X size={18} />
          </button>
        </div>

        {/* Body Form */}
        <form onSubmit={validateAndSubmit} noValidate>
          <div className={styles.body}>
            <p id="gov-modal-desc" className={styles.description}>
              {config.description}
            </p>

            {/* Validation Error */}
            {validationError && (
              <div className={styles.errorBanner} role="alert">
                <AlertCircle size={16} aria-hidden="true" style={{ flexShrink: 0, marginTop: 2 }} />
                <div>
                  <strong>Validation error</strong>
                  <div>{validationError}</div>
                </div>
              </div>
            )}

            {/* API Error */}
            {apiError && (
              <div className={styles.errorBanner} role="alert">
                <AlertCircle size={16} aria-hidden="true" style={{ flexShrink: 0, marginTop: 2 }} />
                <div>
                  <strong>Action failed</strong>
                  <div>{apiError}</div>
                </div>
              </div>
            )}

            {/* Context Fields */}
            <div className={styles.fieldsContainer}>
              {/* Approve & Reject need ReviewerId */}
              {(action === 'approve' || action === 'reject') && (
                <Input
                  ref={initialInputRef}
                  id="gov-reviewer-id"
                  label="Reviewer User ID *"
                  value={reviewerId}
                  onChange={(e) => {
                    setReviewerId(e.target.value);
                    if (validationError) setValidationError(null);
                  }}
                  placeholder="xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx"
                  helperText="Existing user UUID of the reviewer. Peer review rule: author cannot approve or reject their own article."
                  disabled={isSubmitting}
                  aria-required="true"
                />
              )}

              {/* Approve supports optional changeSummary */}
              {action === 'approve' && (
                <Input
                  id="gov-change-summary"
                  label="Change Summary (Optional)"
                  value={changeSummary}
                  onChange={(e) => setChangeSummary(e.target.value)}
                  placeholder="e.g. Approved and published following peer review."
                  helperText="Notes on what was reviewed or verified for this version snapshot."
                  disabled={isSubmitting}
                  maxLength={500}
                />
              )}

              {/* Submit Review supports optional submitterId */}
              {action === 'submit-review' && (
                <Input
                  ref={initialInputRef}
                  id="gov-submitter-id"
                  label="Submitter User ID (Optional)"
                  value={submitterId}
                  onChange={(e) => {
                    setSubmitterId(e.target.value);
                    if (validationError) setValidationError(null);
                  }}
                  placeholder="xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx"
                  helperText="Existing user UUID of submitter. If left blank, the article's existing author is used."
                  disabled={isSubmitting}
                />
              )}

              {/* Deprecate & Archive support optional actorId */}
              {(action === 'deprecate' || action === 'archive') && (
                <Input
                  ref={initialInputRef}
                  id="gov-actor-id"
                  label="Actor User ID (Optional)"
                  value={actorId}
                  onChange={(e) => {
                    setActorId(e.target.value);
                    if (validationError) setValidationError(null);
                  }}
                  placeholder="xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx"
                  helperText="Existing user UUID performing this transition."
                  disabled={isSubmitting}
                />
              )}
            </div>
          </div>

          {/* Footer Actions */}
          <div className={styles.footer}>
            <Button
              ref={cancelButtonRef}
              type="button"
              variant="subtle"
              size="md"
              onClick={onClose}
              disabled={isSubmitting}
            >
              Cancel
            </Button>
            <Button
              type="submit"
              variant={config.confirmVariant}
              size="md"
              isLoading={isSubmitting}
              disabled={isSubmitting}
            >
              {config.confirmLabel}
            </Button>
          </div>
        </form>
      </div>
    </div>
  );
};
