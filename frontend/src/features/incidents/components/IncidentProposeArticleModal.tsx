import React, { useState, useEffect } from 'react';
import { X, BookOpen, AlertCircle } from 'lucide-react';
import { Button } from '@/components/ui/Button';
import { Input } from '@/components/ui/Input';
import type { CreateKnowledgeFromIncidentRequest } from '../types/incidentTypes';
import styles from './IncidentProposeArticleModal.module.css';

export interface IncidentProposeArticleModalProps {
  isOpen: boolean;
  incidentTitle: string;
  isSubmitting: boolean;
  apiError: string | null;
  onClose: () => void;
  onConfirm: (request: CreateKnowledgeFromIncidentRequest) => void;
}

const UUID_REGEX = /^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$/i;

export const IncidentProposeArticleModal: React.FC<IncidentProposeArticleModalProps> = ({
  isOpen,
  incidentTitle,
  isSubmitting,
  apiError,
  onClose,
  onConfirm,
}) => {
  const [authorId, setAuthorId] = useState('');
  const [knowledgeTypeId, setKnowledgeTypeId] = useState('');
  const [title, setTitle] = useState('');
  const [summary, setSummary] = useState('');
  const [customContent, setCustomContent] = useState('');
  const [slug, setSlug] = useState('');
  const [validationError, setValidationError] = useState<string | null>(null);

  useEffect(() => {
    if (isOpen) {
      setAuthorId('');
      setKnowledgeTypeId('');
      setTitle(incidentTitle || '');
      setSummary('');
      setCustomContent('');
      setSlug('');
      setValidationError(null);
    }
  }, [isOpen, incidentTitle]);

  if (!isOpen) return null;

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();

    const trimmedAuthorId = authorId.trim();
    if (!trimmedAuthorId) {
      setValidationError('Author ID is required.');
      return;
    }
    if (!UUID_REGEX.test(trimmedAuthorId)) {
      setValidationError('Author ID must be a valid UUID.');
      return;
    }

    const trimmedTypeId = knowledgeTypeId.trim();
    if (!trimmedTypeId) {
      setValidationError('Knowledge Type ID is required.');
      return;
    }
    if (!UUID_REGEX.test(trimmedTypeId)) {
      setValidationError('Knowledge Type ID must be a valid UUID.');
      return;
    }

    const trimmedSlug = slug.trim();
    if (trimmedSlug) {
      if (trimmedSlug.length > 200) {
        setValidationError('Slug cannot exceed 200 characters.');
        return;
      }
      if (!/^[a-z0-9\u0600-\u06FF]+(?:-[a-z0-9\u0600-\u06FF]+)*$/i.test(trimmedSlug)) {
        setValidationError('Slug must contain only letters, numbers, and hyphens without spaces.');
        return;
      }
    }

    setValidationError(null);
    onConfirm({
      authorId: trimmedAuthorId,
      knowledgeTypeId: trimmedTypeId,
      title: title.trim() ? title.trim() : null,
      summary: summary.trim() ? summary.trim() : null,
      customContent: customContent.trim() ? customContent.trim() : null,
      slug: trimmedSlug ? trimmedSlug : null,
    });
  };

  return (
    <div
      className={styles.backdrop}
      role="dialog"
      aria-modal="true"
      aria-labelledby="propose-modal-title"
      onClick={onClose}
    >
      <div className={styles.modal} onClick={(e) => e.stopPropagation()}>
        <div className={styles.header}>
          <div>
            <h2 id="propose-modal-title" className={styles.headerTitle}>
              Propose Knowledge Article
            </h2>
            <div className={styles.headerSubtitle}>
              Synthesizing draft knowledge from incident: {incidentTitle}
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

            <div className={styles.gridTwoCol}>
              <div className={styles.fieldGroup}>
                <label htmlFor="proposeAuthorId" className={styles.fieldLabel}>
                  Author ID (UUID) <span className={styles.requiredStar}>*</span>
                </label>
                <Input
                  id="proposeAuthorId"
                  value={authorId}
                  onChange={(e) => {
                    setAuthorId(e.target.value);
                    setValidationError(null);
                  }}
                  placeholder="e.g. 550e8400-e29b-41d4-a716-446655440000"
                  disabled={isSubmitting}
                  autoFocus
                  required
                />
                <span className={styles.fieldHint}>Your engineer identity UUID</span>
              </div>

              <div className={styles.fieldGroup}>
                <label htmlFor="knowledgeTypeId" className={styles.fieldLabel}>
                  Knowledge Type ID (UUID) <span className={styles.requiredStar}>*</span>
                </label>
                <Input
                  id="knowledgeTypeId"
                  value={knowledgeTypeId}
                  onChange={(e) => {
                    setKnowledgeTypeId(e.target.value);
                    setValidationError(null);
                  }}
                  placeholder="e.g. 11111111-1111-1111-1111-111111111111"
                  disabled={isSubmitting}
                  required
                />
                <span className={styles.fieldHint}>Postmortem or Runbook type UUID</span>
              </div>
            </div>

            <div className={styles.fieldGroup}>
              <label htmlFor="proposeTitle" className={styles.fieldLabel}>
                Article Title (Optional)
              </label>
              <Input
                id="proposeTitle"
                value={title}
                onChange={(e) => setTitle(e.target.value)}
                placeholder="Defaults to incident title if empty"
                disabled={isSubmitting}
              />
            </div>

            <div className={styles.fieldGroup}>
              <label htmlFor="proposeSummary" className={styles.fieldLabel}>
                Executive Summary (Optional)
              </label>
              <textarea
                id="proposeSummary"
                rows={2}
                value={summary}
                onChange={(e) => setSummary(e.target.value)}
                placeholder="Brief summary of learnings and prevention (defaults to symptoms/problem)..."
                disabled={isSubmitting}
                className={styles.textareaInput}
              />
            </div>

            <div className={styles.fieldGroup}>
              <label htmlFor="proposeSlug" className={styles.fieldLabel}>
                URL Slug (Optional)
              </label>
              <Input
                id="proposeSlug"
                value={slug}
                onChange={(e) => {
                  setSlug(e.target.value);
                  setValidationError(null);
                }}
                placeholder="e.g. redis-connection-timeout-runbook"
                disabled={isSubmitting}
              />
            </div>

            <div className={styles.fieldGroup}>
              <label htmlFor="customContent" className={styles.fieldLabel}>
                Custom Content (Optional Markdown)
              </label>
              <textarea
                id="customContent"
                rows={4}
                value={customContent}
                onChange={(e) => setCustomContent(e.target.value)}
                placeholder="Leave blank to auto-synthesize from Problem, Symptoms, Root Cause, and Resolution sections..."
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
              icon={<BookOpen size={16} />}
            >
              {isSubmitting ? 'Synthesizing...' : 'Propose Article'}
            </Button>
          </div>
        </form>
      </div>
    </div>
  );
};
