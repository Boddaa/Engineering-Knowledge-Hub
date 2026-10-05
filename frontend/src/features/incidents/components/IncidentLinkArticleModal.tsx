import React, { useState, useEffect } from 'react';
import { X, Link2, AlertCircle } from 'lucide-react';
import { Button } from '@/components/ui/Button';
import { Input } from '@/components/ui/Input';
import type { LinkArticleRequest, ArticleRelationshipType } from '../types/incidentTypes';
import { VALID_RELATIONSHIP_TYPES, toRelationshipTypeNumeric } from '../utils/incidentFormatters';
import styles from './IncidentLinkArticleModal.module.css';

export interface IncidentLinkArticleModalProps {
  isOpen: boolean;
  incidentTitle: string;
  isSubmitting: boolean;
  apiError: string | null;
  onClose: () => void;
  onConfirm: (request: LinkArticleRequest) => void;
}

const UUID_REGEX = /^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$/i;

export const IncidentLinkArticleModal: React.FC<IncidentLinkArticleModalProps> = ({
  isOpen,
  incidentTitle,
  isSubmitting,
  apiError,
  onClose,
  onConfirm,
}) => {
  const [articleId, setArticleId] = useState('');
  const [relationshipType, setRelationshipType] = useState<ArticleRelationshipType>('RelatesTo');
  const [validationError, setValidationError] = useState<string | null>(null);

  useEffect(() => {
    if (isOpen) {
      setArticleId('');
      setRelationshipType('RelatesTo');
      setValidationError(null);
    }
  }, [isOpen]);

  if (!isOpen) return null;

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();

    const trimmedId = articleId.trim();
    if (!trimmedId) {
      setValidationError('Knowledge Article ID is required.');
      return;
    }
    if (!UUID_REGEX.test(trimmedId)) {
      setValidationError('Knowledge Article ID must be a valid UUID.');
      return;
    }

    setValidationError(null);
    onConfirm({
      articleId: trimmedId,
      relationshipType: toRelationshipTypeNumeric(relationshipType),
    });
  };

  return (
    <div
      className={styles.backdrop}
      role="dialog"
      aria-modal="true"
      aria-labelledby="link-modal-title"
      onClick={onClose}
    >
      <div className={styles.modal} onClick={(e) => e.stopPropagation()}>
        <div className={styles.header}>
          <div>
            <h2 id="link-modal-title" className={styles.headerTitle}>
              Link Knowledge Article
            </h2>
            <div className={styles.headerSubtitle}>
              Associating knowledge with: {incidentTitle}
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
              <label htmlFor="linkArticleId" className={styles.fieldLabel}>
                Knowledge Article ID (UUID) <span className={styles.requiredStar}>*</span>
              </label>
              <Input
                id="linkArticleId"
                value={articleId}
                onChange={(e) => {
                  setArticleId(e.target.value);
                  setValidationError(null);
                }}
                placeholder="e.g. 11111111-2222-3333-4444-555555555555"
                disabled={isSubmitting}
                autoFocus
                required
              />
              <span className={styles.fieldHint}>
                Unique identifier of the verified runbook, architecture guide, or reference doc
              </span>
            </div>

            <div className={styles.fieldGroup}>
              <label htmlFor="relationshipType" className={styles.fieldLabel}>
                Relationship Semantic <span className={styles.requiredStar}>*</span>
              </label>
              <select
                id="relationshipType"
                value={relationshipType}
                onChange={(e) =>
                  setRelationshipType(e.target.value as ArticleRelationshipType)
                }
                disabled={isSubmitting}
                className={styles.selectInput}
              >
                {VALID_RELATIONSHIP_TYPES.map((type) => (
                  <option key={type} value={type}>
                    {type}
                  </option>
                ))}
              </select>
              <span className={styles.fieldHint}>
                How this knowledge relates to the operational incident (e.g. Resolves, Supports)
              </span>
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
              icon={<Link2 size={16} />}
            >
              {isSubmitting ? 'Linking...' : 'Link Article'}
            </Button>
          </div>
        </form>
      </div>
    </div>
  );
};
