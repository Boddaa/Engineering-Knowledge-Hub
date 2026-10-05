import React, { useState, useEffect } from 'react';
import { X, CheckCircle, AlertCircle } from 'lucide-react';
import { Button } from '@/components/ui/Button';
import { Input } from '@/components/ui/Input';
import type { ResolveIncidentRequest } from '../types/incidentTypes';
import styles from './IncidentResolveModal.module.css';

export interface IncidentResolveModalProps {
  isOpen: boolean;
  incidentTitle: string;
  isSubmitting: boolean;
  apiError: string | null;
  onClose: () => void;
  onConfirm: (request: ResolveIncidentRequest) => void;
}

const UUID_REGEX = /^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$/i;

export const IncidentResolveModal: React.FC<IncidentResolveModalProps> = ({
  isOpen,
  incidentTitle,
  isSubmitting,
  apiError,
  onClose,
  onConfirm,
}) => {
  const [resolvedById, setResolvedById] = useState('');
  const [resolution, setResolution] = useState('');
  const [rootCause, setRootCause] = useState('');
  const [validationError, setValidationError] = useState<string | null>(null);

  useEffect(() => {
    if (isOpen) {
      setResolvedById('');
      setResolution('');
      setRootCause('');
      setValidationError(null);
    }
  }, [isOpen]);

  if (!isOpen) return null;

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();

    const trimmedId = resolvedById.trim();
    if (!trimmedId) {
      setValidationError('Resolver ID is required.');
      return;
    }
    if (!UUID_REGEX.test(trimmedId)) {
      setValidationError('Resolver ID must be a valid UUID.');
      return;
    }

    const trimmedResolution = resolution.trim();
    if (!trimmedResolution) {
      setValidationError('Resolution details are required.');
      return;
    }

    setValidationError(null);
    onConfirm({
      resolvedById: trimmedId,
      resolution: trimmedResolution,
      rootCause: rootCause.trim() ? rootCause.trim() : null,
    });
  };

  return (
    <div
      className={styles.backdrop}
      role="dialog"
      aria-modal="true"
      aria-labelledby="resolve-modal-title"
      onClick={onClose}
    >
      <div className={styles.modal} onClick={(e) => e.stopPropagation()}>
        <div className={styles.header}>
          <div>
            <h2 id="resolve-modal-title" className={styles.headerTitle}>
              Resolve Incident
            </h2>
            <div className={styles.headerSubtitle}>
              Recording resolution for: {incidentTitle}
            </div>
          </div>
          <button
            type="button"
            className={styles.closeButton}
            onClick={onClose}
            aria-label="Close dialog"
            disabled={isSubmitting}
          >
            <X size={18} />
          </button>
        </div>

        <form onSubmit={handleSubmit}>
          <div className={styles.body}>
            {(validationError || apiError) && (
              <div className={styles.errorBanner} role="alert">
                <AlertCircle size={16} />
                <span>{validationError || apiError}</span>
              </div>
            )}

            <div className={styles.fieldGroup}>
              <label htmlFor="resolvedById" className={styles.fieldLabel}>
                Resolver ID (UUID) <span className={styles.requiredStar}>*</span>
              </label>
              <Input
                id="resolvedById"
                value={resolvedById}
                onChange={(e) => {
                  setResolvedById(e.target.value);
                  setValidationError(null);
                }}
                placeholder="e.g. 550e8400-e29b-41d4-a716-446655440000"
                disabled={isSubmitting}
                autoFocus
                required
              />
            </div>

            <div className={styles.fieldGroup}>
              <label htmlFor="resolution" className={styles.fieldLabel}>
                Resolution Summary <span className={styles.requiredStar}>*</span>
              </label>
              <textarea
                id="resolution"
                rows={3}
                value={resolution}
                onChange={(e) => {
                  setResolution(e.target.value);
                  setValidationError(null);
                }}
                placeholder="Describe the fix, configuration change, or mitigation applied..."
                disabled={isSubmitting}
                className={styles.textareaInput}
                required
              />
            </div>

            <div className={styles.fieldGroup}>
              <label htmlFor="rootCause" className={styles.fieldLabel}>
                Root Cause (Optional)
              </label>
              <textarea
                id="rootCause"
                rows={2}
                value={rootCause}
                onChange={(e) => setRootCause(e.target.value)}
                placeholder="Identified underlying bug, race condition, saturation, or config flaw..."
                disabled={isSubmitting}
                className={styles.textareaInput}
              />
            </div>
          </div>

          <div className={styles.footer}>
            <Button
              type="button"
              variant="subtle"
              onClick={onClose}
              disabled={isSubmitting}
            >
              Cancel
            </Button>
            <Button
              type="submit"
              variant="primary"
              disabled={isSubmitting}
              isLoading={isSubmitting}
              icon={<CheckCircle size={16} />}
            >
              {isSubmitting ? 'Resolving...' : 'Confirm Resolution'}
            </Button>
          </div>
        </form>
      </div>
    </div>
  );
};
