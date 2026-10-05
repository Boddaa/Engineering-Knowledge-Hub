import React, { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { X, BookOpen, AlertCircle, CheckCircle2, ExternalLink } from 'lucide-react';
import { Button } from '@/components/ui/Button';
import { Input } from '@/components/ui/Input';
import type { DocumentDto, ProposeArticleFromDocumentRequest } from '../types/documentTypes';
import type { KnowledgeArticleDto } from '@/features/knowledge';
import styles from './DocumentProposeArticleModal.module.css';

export interface DocumentProposeArticleModalProps {
  isOpen: boolean;
  document: DocumentDto | null;
  isSubmitting: boolean;
  apiError: string | null;
  onClose: () => void;
  onConfirm: (request: ProposeArticleFromDocumentRequest) => Promise<KnowledgeArticleDto | null>;
}

const UUID_REGEX = /^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$/i;

export const DocumentProposeArticleModal: React.FC<DocumentProposeArticleModalProps> = ({
  isOpen,
  document,
  isSubmitting,
  apiError,
  onClose,
  onConfirm,
}) => {
  const navigate = useNavigate();
  const [authorId, setAuthorId] = useState('');
  const [knowledgeTypeId, setKnowledgeTypeId] = useState('');
  const [title, setTitle] = useState('');
  const [summary, setSummary] = useState('');
  const [content, setContent] = useState('');
  const [referenceText, setReferenceText] = useState('');
  const [validationError, setValidationError] = useState<string | null>(null);
  const [proposedArticle, setProposedArticle] = useState<KnowledgeArticleDto | null>(null);

  useEffect(() => {
    if (isOpen && document) {
      // Suggest clean title from document filename without extension
      const cleanTitle = document.fileName.replace(/\.[^/.]+$/, '').replace(/[-_]/g, ' ');
      setTitle(cleanTitle.charAt(0).toUpperCase() + cleanTitle.slice(1));
      setAuthorId(document.uploadedById || '');
      setKnowledgeTypeId('');
      setSummary('');
      setContent(document.extractedText || '');
      setReferenceText(`Extracted from document: ${document.fileName} (ID: ${document.id})`);
      setValidationError(null);
      setProposedArticle(null);
    }
  }, [isOpen, document]);

  if (!isOpen || !document) return null;

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();

    const trimmedTitle = title.trim();
    if (!trimmedTitle) {
      setValidationError('Article title is required.');
      return;
    }
    if (trimmedTitle.length > 200) {
      setValidationError('Article title cannot exceed 200 characters.');
      return;
    }

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

    setValidationError(null);

    const request: ProposeArticleFromDocumentRequest = {
      title: trimmedTitle,
      authorId: trimmedAuthorId,
      knowledgeTypeId: trimmedTypeId,
      summary: summary.trim() ? summary.trim() : null,
      content: content.trim() ? content.trim() : null,
      referenceText: referenceText.trim() ? referenceText.trim() : null,
    };

    const result = await onConfirm(request);
    if (result) {
      setProposedArticle(result);
    }
  };

  const handleNavigateToArticle = () => {
    if (proposedArticle?.id) {
      onClose();
      navigate(`/knowledge/${proposedArticle.id}`);
    }
  };

  const isTitleValid = Boolean(title.trim()) && title.trim().length <= 200;
  const isAuthorValid = Boolean(authorId.trim()) && UUID_REGEX.test(authorId.trim());
  const isKnowledgeTypeValid = Boolean(knowledgeTypeId.trim()) && UUID_REGEX.test(knowledgeTypeId.trim());
  const isSubmitDisabled = isSubmitting || !isTitleValid || !isAuthorValid || !isKnowledgeTypeValid;

  return (
    <div
      className={styles.backdrop}
      role="dialog"
      aria-modal="true"
      aria-labelledby="propose-document-article-title"
      onClick={isSubmitting ? undefined : onClose}
    >
      <div className={styles.modal} onClick={(e) => e.stopPropagation()}>
        <div className={styles.header}>
          <div>
            <h2 id="propose-document-article-title" className={styles.headerTitle}>
              Propose Knowledge Article
            </h2>
            <div className={styles.headerSubtitle}>
              Deriving draft knowledge from: {document.fileName}
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

        {proposedArticle ? (
          <div className={styles.body}>
            <div className={styles.successCard}>
              <div className={styles.successHeader}>
                <CheckCircle2 size={20} aria-hidden="true" />
                <span>Knowledge Article Successfully Proposed</span>
              </div>
              <h3 className={styles.successTitle}>{proposedArticle.title}</h3>
              <div className={styles.successMeta}>
                <div className={styles.metaRow}>
                  <span className={styles.metaLabel}>Article ID:</span>
                  <span className={styles.metaValue}>{proposedArticle.id}</span>
                </div>
                {proposedArticle.slug && (
                  <div className={styles.metaRow}>
                    <span className={styles.metaLabel}>Slug:</span>
                    <span className={styles.metaValue}>{proposedArticle.slug}</span>
                  </div>
                )}
                <div className={styles.metaRow}>
                  <span className={styles.metaLabel}>Lifecycle Status:</span>
                  <span>{proposedArticle.status} (Draft / Pending Review)</span>
                </div>
              </div>
              <div className={styles.successRuleBanner}>
                Document → Draft / Unverified Article → Human Review:
                The article has been created as an unverified draft for peer inspection and review.
              </div>
            </div>
            <div className={styles.footer}>
              <Button type="button" variant="subtle" onClick={onClose}>
                Close
              </Button>
              <Button
                type="button"
                variant="primary"
                icon={<ExternalLink size={16} />}
                onClick={handleNavigateToArticle}
              >
                View Knowledge Article
              </Button>
            </div>
          </div>
        ) : (
          <form onSubmit={handleSubmit}>
            <div className={styles.body}>
              {(validationError || apiError) && (
                <div className={styles.errorBanner} role="alert">
                  <AlertCircle size={16} />
                  <span>{validationError || apiError}</span>
                </div>
              )}

              <div className={styles.fieldGroup}>
                <label htmlFor="articleTitle" className={styles.fieldLabel}>
                  Article Title <span className={styles.requiredStar}>*</span>
                </label>
                <Input
                  id="articleTitle"
                  value={title}
                  onChange={(e) => {
                    setTitle(e.target.value);
                    setValidationError(null);
                  }}
                  placeholder="e.g. Architecture Overview from Technical Specification"
                  disabled={isSubmitting}
                  maxLength={200}
                  required
                  autoFocus
                />
              </div>

              <div className={styles.gridTwoCol}>
                <div className={styles.fieldGroup}>
                  <label htmlFor="articleAuthorId" className={styles.fieldLabel}>
                    Author ID (UUID) <span className={styles.requiredStar}>*</span>
                  </label>
                  <Input
                    id="articleAuthorId"
                    value={authorId}
                    onChange={(e) => {
                      setAuthorId(e.target.value);
                      setValidationError(null);
                    }}
                    placeholder="e.g. 550e8400-e29b-41d4-a716-446655440000"
                    disabled={isSubmitting}
                    required
                    error={
                      authorId.trim() && !UUID_REGEX.test(authorId.trim())
                        ? 'Author ID must be a valid UUID'
                        : undefined
                    }
                    helperText="Author/Engineer identity UUID"
                  />
                </div>

                <div className={styles.fieldGroup}>
                  <label htmlFor="articleKnowledgeTypeId" className={styles.fieldLabel}>
                    Knowledge Type ID (UUID) <span className={styles.requiredStar}>*</span>
                  </label>
                  <Input
                    id="articleKnowledgeTypeId"
                    value={knowledgeTypeId}
                    onChange={(e) => {
                      setKnowledgeTypeId(e.target.value);
                      setValidationError(null);
                    }}
                    placeholder="e.g. 11111111-1111-1111-1111-111111111111"
                    disabled={isSubmitting}
                    required
                    error={
                      knowledgeTypeId.trim() && !UUID_REGEX.test(knowledgeTypeId.trim())
                        ? 'Knowledge Type ID must be a valid UUID'
                        : undefined
                    }
                    helperText="Required taxonomy Knowledge Type UUID"
                  />
                </div>
              </div>

              <div className={styles.fieldGroup}>
                <label htmlFor="articleSummary" className={styles.fieldLabel}>
                  Executive Summary (Optional)
                </label>
                <textarea
                  id="articleSummary"
                  rows={2}
                  value={summary}
                  onChange={(e) => setSummary(e.target.value)}
                  placeholder="Brief synopsis of the technical specification or guide..."
                  disabled={isSubmitting}
                  className={styles.textareaInput}
                />
              </div>

              <div className={styles.fieldGroup}>
                <label htmlFor="articleContent" className={styles.fieldLabel}>
                  Article Content (Optional Markdown)
                </label>
                <textarea
                  id="articleContent"
                  rows={6}
                  value={content}
                  onChange={(e) => setContent(e.target.value)}
                  placeholder="Pre-populated from extracted text if available..."
                  disabled={isSubmitting}
                  className={styles.textareaInput}
                />
                <span className={styles.fieldHint}>
                  Initial content pre-populated from document extraction text
                </span>
              </div>

              <div className={styles.fieldGroup}>
                <label htmlFor="articleReferenceText" className={styles.fieldLabel}>
                  Reference / Provenance Note (Optional)
                </label>
                <Input
                  id="articleReferenceText"
                  value={referenceText}
                  onChange={(e) => setReferenceText(e.target.value)}
                  placeholder="Reference or source provenance metadata..."
                  disabled={isSubmitting}
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
                disabled={isSubmitDisabled}
                isLoading={isSubmitting}
                icon={<BookOpen size={16} />}
              >
                {isSubmitting ? 'Proposing...' : 'Propose Article'}
              </Button>
            </div>
          </form>
        )}
      </div>
    </div>
  );
};
