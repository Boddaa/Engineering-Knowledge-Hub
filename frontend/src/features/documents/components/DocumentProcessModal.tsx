import React, { useState, useEffect } from 'react';
import { X, Play, AlertCircle } from 'lucide-react';
import { Button } from '@/components/ui/Button';
import styles from './DocumentProcessModal.module.css';

export interface DocumentProcessModalProps {
  isOpen: boolean;
  documentFileName: string;
  isSubmitting: boolean;
  apiError: string | null;
  onClose: () => void;
  onConfirm: (contentOverride?: string) => void;
}

export const DocumentProcessModal: React.FC<DocumentProcessModalProps> = ({
  isOpen,
  documentFileName,
  isSubmitting,
  apiError,
  onClose,
  onConfirm,
}) => {
  const [contentOverride, setContentOverride] = useState<string>('');

  useEffect(() => {
    if (isOpen) {
      setContentOverride('');
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
    onConfirm(contentOverride.trim() || undefined);
  };

  return (
    <div
      className={styles.backdrop}
      role="dialog"
      aria-modal="true"
      aria-labelledby="process-doc-title"
      onClick={onClose}
    >
      <div className={styles.modal} onClick={(e) => e.stopPropagation()}>
        <div className={styles.header}>
          <div>
            <h2 id="process-doc-title" className={styles.headerTitle}>
              <Play size={18} className={styles.headerIcon} aria-hidden="true" />
              Process Document
            </h2>
            <p className={styles.headerSubtitle}>
              Extract and prepare text from &ldquo;{documentFileName}&rdquo;
            </p>
          </div>
          <button
            type="button"
            className={styles.closeButton}
            onClick={onClose}
            aria-label="Close process document dialog"
            disabled={isSubmitting}
          >
            <X size={18} />
          </button>
        </div>

        <form onSubmit={handleSubmit}>
          <div className={styles.body}>
            {apiError && (
              <div className={styles.errorBanner} role="alert">
                <AlertCircle size={16} />
                <span>{apiError}</span>
              </div>
            )}

            <div className={styles.infoBox}>
              <span>
                Processing triggers text extraction on the registered content payload and transitions
                the document status to <strong>Processed</strong>. Once processed, you can generate
                semantic chunks and synthesize draft knowledge articles.
              </span>
            </div>

            <div className={styles.fieldGroup}>
              <label htmlFor="process-override-content" className={styles.fieldLabel}>
                Content Override (Optional)
              </label>
              <textarea
                id="process-override-content"
                className={styles.textarea}
                value={contentOverride}
                onChange={(e) => setContentOverride(e.target.value)}
                placeholder="Leave blank to extract from existing registered document payload, or supply updated text/payload..."
                rows={5}
                disabled={isSubmitting}
              />
              <span className={styles.fieldHelper}>
                Leave blank under normal circumstances. Only supply if you need to override the raw payload during extraction.
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
              icon={<Play size={14} />}
              isLoading={isSubmitting}
              disabled={isSubmitting}
            >
              Start Processing
            </Button>
          </div>
        </form>
      </div>
    </div>
  );
};
