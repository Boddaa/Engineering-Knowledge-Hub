import React, { useState, useEffect } from 'react';
import { X, BookOpen, AlertCircle, Link } from 'lucide-react';
import { Button } from '@/components/ui/Button';
import { SourceSelect } from './SourceSelect';
import styles from './AttachSourceModal.module.css';

export interface AttachSourceModalProps {
  isOpen: boolean;
  articleTitle: string;
  excludeSourceIds?: string[];
  isSubmitting: boolean;
  apiError: string | null;
  onClose: () => void;
  onConfirm: (sourceId: string, referenceText?: string) => void;
}

export const AttachSourceModal: React.FC<AttachSourceModalProps> = ({
  isOpen,
  articleTitle,
  excludeSourceIds = [],
  isSubmitting,
  apiError,
  onClose,
  onConfirm,
}) => {
  const [selectedSourceId, setSelectedSourceId] = useState<string>('');
  const [referenceText, setReferenceText] = useState<string>('');
  const [validationError, setValidationError] = useState<string | null>(null);

  useEffect(() => {
    if (isOpen) {
      setSelectedSourceId('');
      setReferenceText('');
      setValidationError(null);
    }
  }, [isOpen]);

  useEffect(() => {
    const handleKeyDown = (e: KeyboardEvent) => {
      if (e.key === 'Escape' && isOpen && !isSubmitting) {
        onClose();
      }
    };
    window.addEventListener('keydown', handleKeyDown);
    return () => window.removeEventListener('keydown', handleKeyDown);
  }, [isOpen, isSubmitting, onClose]);

  if (!isOpen) return null;

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();

    if (!selectedSourceId) {
      setValidationError('Please select a knowledge source to attach.');
      return;
    }

    setValidationError(null);
    onConfirm(selectedSourceId, referenceText.trim() || undefined);
  };

  return (
    <div
      className={styles.backdrop}
      role="dialog"
      aria-modal="true"
      aria-labelledby="attach-source-title"
      onClick={onClose}
    >
      <div className={styles.modal} onClick={(e) => e.stopPropagation()}>
        <div className={styles.header}>
          <div>
            <h2 id="attach-source-title" className={styles.headerTitle}>
              <BookOpen size={16} className={styles.headerIcon} />
              Attach Knowledge Source
            </h2>
            <p className={styles.headerSubtitle}>
              Link verified evidence or supporting documentation to &ldquo;{articleTitle}&rdquo;
            </p>
          </div>
          <button
            type="button"
            className={styles.closeButton}
            onClick={onClose}
            aria-label="Close attach source dialog"
            disabled={isSubmitting}
          >
            <X size={18} />
          </button>
        </div>

        <form onSubmit={handleSubmit} noValidate>
          <div className={styles.body}>
            {(validationError || apiError) && (
              <div className={styles.errorBanner} role="alert">
                <AlertCircle size={16} />
                <span>{validationError || apiError}</span>
              </div>
            )}

            <SourceSelect
              id="modal-source-select"
              label="Select Source *"
              value={selectedSourceId}
              onChange={(sourceId) => {
                setSelectedSourceId(sourceId);
                setValidationError(null);
              }}
              excludeIds={excludeSourceIds}
              disabled={isSubmitting}
              required
              error={validationError ?? undefined}
              helperText="Choose an approved knowledge source registered in the engineering hub."
              showDetailsPreview
            />

            <div className={styles.fieldGroup}>
              <label htmlFor="modal-reference-text" className={styles.fieldLabel}>
                Reference / Citation Note (Optional)
              </label>
              <textarea
                id="modal-reference-text"
                className={styles.textarea}
                value={referenceText}
                onChange={(e) => setReferenceText(e.target.value)}
                placeholder="e.g. Chapter 4: Circuit Breaker Specification, RFC 7519 § 3.1, or Internal Architecture Wiki permalink"
                rows={3}
                disabled={isSubmitting}
                maxLength={500}
              />
              <span className={styles.fieldHelper}>
                Specific section, page, or anchor citation to help engineers trace exact evidence.
              </span>
            </div>
          </div>

          <div className={styles.footer}>
            <Button
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
              variant="primary"
              size="md"
              icon={<Link size={15} />}
              isLoading={isSubmitting}
              disabled={isSubmitting || !selectedSourceId}
            >
              Attach Source
            </Button>
          </div>
        </form>
      </div>
    </div>
  );
};
