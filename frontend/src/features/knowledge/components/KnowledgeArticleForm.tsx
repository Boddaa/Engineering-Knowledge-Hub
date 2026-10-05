import React, { useState } from 'react';
import { AlertCircle, ArrowLeft, Save, Sparkles } from 'lucide-react';
import { Input } from '@/components/ui/Input';
import { Button } from '@/components/ui/Button';
import styles from './KnowledgeArticleForm.module.css';

export interface KnowledgeArticleFormData {
  title: string;
  slug: string;
  summary: string;
  content: string;
  knowledgeTypeId: string;
  authorId: string;
}

export interface FormValidationErrors {
  title?: string;
  slug?: string;
  summary?: string;
  content?: string;
  knowledgeTypeId?: string;
  authorId?: string;
}

export interface DiscoveredKnowledgeType {
  id: string;
  name?: string;
}

export interface KnowledgeArticleFormProps {
  mode: 'create' | 'edit';
  initialData?: Partial<KnowledgeArticleFormData>;
  isSubmitting: boolean;
  apiError: string | null;
  onSubmit: (data: KnowledgeArticleFormData) => void;
  onCancel: () => void;
  discoveredKnowledgeTypes?: DiscoveredKnowledgeType[];
}

function generateDefaultUuid(): string {
  if (typeof crypto !== 'undefined' && typeof crypto.randomUUID === 'function') {
    return crypto.randomUUID();
  }
  return 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, (c) => {
    const r = (Math.random() * 16) | 0;
    const v = c === 'x' ? r : (r & 0x3) | 0x8;
    return v.toString(16);
  });
}

export function validateArticleForm(
  data: KnowledgeArticleFormData,
  mode: 'create' | 'edit'
): FormValidationErrors {
  const errors: FormValidationErrors = {};

  const trimmedTitle = data.title.trim();
  if (!trimmedTitle) {
    errors.title = 'Article title is required.';
  } else if (trimmedTitle.length > 200) {
    errors.title = 'Article title cannot exceed 200 characters.';
  }

  const trimmedSummary = data.summary.trim();
  if (!trimmedSummary) {
    errors.summary = 'Article summary is required.';
  }

  const trimmedContent = data.content.trim();
  if (!trimmedContent) {
    errors.content = 'Documentation content is required.';
  }

  const trimmedSlug = data.slug.trim();
  if (trimmedSlug) {
    if (trimmedSlug.length > 200) {
      errors.slug = 'Slug cannot exceed 200 characters.';
    } else if (!/^[a-z0-9\u0600-\u06FF]+(?:-[a-z0-9\u0600-\u06FF]+)*$/i.test(trimmedSlug)) {
      errors.slug = 'Slug must contain only letters, numbers, and single hyphens without spaces.';
    }
  }

  if (mode === 'create') {
    const trimmedTypeId = data.knowledgeTypeId.trim();
    if (!trimmedTypeId) {
      errors.knowledgeTypeId = 'Knowledge Type ID is required.';
    } else if (!/^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$/i.test(trimmedTypeId)) {
      errors.knowledgeTypeId = 'Knowledge Type ID must be a valid UUID.';
    }

    const trimmedAuthorId = data.authorId.trim();
    if (!trimmedAuthorId) {
      errors.authorId = 'Author ID is required.';
    } else if (!/^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$/i.test(trimmedAuthorId)) {
      errors.authorId = 'Author ID must be a valid UUID.';
    }
  }

  return errors;
}

export const KnowledgeArticleForm: React.FC<KnowledgeArticleFormProps> = ({
  mode,
  initialData = {},
  isSubmitting,
  apiError,
  onSubmit,
  onCancel,
  discoveredKnowledgeTypes = [],
}) => {
  const [formData, setFormData] = useState<KnowledgeArticleFormData>({
    title: initialData.title ?? '',
    slug: initialData.slug ?? '',
    summary: initialData.summary ?? '',
    content: initialData.content ?? '',
    knowledgeTypeId: initialData.knowledgeTypeId ?? (discoveredKnowledgeTypes[0]?.id || ''),
    authorId: initialData.authorId ?? (mode === 'create' ? generateDefaultUuid() : ''),
  });

  const [errors, setErrors] = useState<FormValidationErrors>({});

  const handleFieldChange = (field: keyof KnowledgeArticleFormData, value: string) => {
    setFormData((prev) => ({ ...prev, [field]: value }));
    // Clear validation error for field on edit
    if (errors[field]) {
      setErrors((prev) => {
        const next = { ...prev };
        delete next[field];
        return next;
      });
    }
  };

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    const validationErrors = validateArticleForm(formData, mode);
    if (Object.keys(validationErrors).length > 0) {
      setErrors(validationErrors);
      return;
    }
    setErrors({});
    onSubmit(formData);
  };

  const handleApplyDiscoveredType = (typeId: string) => {
    handleFieldChange('knowledgeTypeId', typeId);
  };

  const handleGenerateAuthorId = () => {
    handleFieldChange('authorId', generateDefaultUuid());
  };

  return (
    <form className={styles.formCard} onSubmit={handleSubmit} noValidate>
      {/* API Level Error Banner */}
      {apiError && (
        <div className={styles.apiErrorBanner} role="alert" aria-live="polite">
          <AlertCircle size={18} className={styles.apiErrorIcon} aria-hidden="true" />
          <div>
            <strong>Unable to save article</strong>
            <div>{apiError}</div>
          </div>
        </div>
      )}

      {/* Edit Mode Read-Only Metadata */}
      {mode === 'edit' && (initialData.knowledgeTypeId || initialData.authorId) && (
        <div className={styles.metaInfoPanel} aria-label="Immutable article metadata">
          {initialData.knowledgeTypeId && (
            <div className={styles.metaItem}>
              <span className={styles.metaLabel}>Knowledge Type</span>
              <span className={styles.metaValue}>{initialData.knowledgeTypeId}</span>
            </div>
          )}
          {initialData.authorId && (
            <div className={styles.metaItem}>
              <span className={styles.metaLabel}>Author ID</span>
              <span className={styles.metaValue}>{initialData.authorId}</span>
            </div>
          )}
        </div>
      )}

      <div className={styles.formSection}>
        {/* Title */}
        <Input
          id="article-title"
          label="Article Title *"
          value={formData.title}
          onChange={(e) => handleFieldChange('title', e.target.value)}
          error={errors.title}
          placeholder="e.g. Distributed Caching Strategies with Redis and .NET"
          disabled={isSubmitting}
          maxLength={200}
          aria-required="true"
        />

        {/* Slug */}
        <Input
          id="article-slug"
          label="Slug (Optional)"
          value={formData.slug}
          onChange={(e) => handleFieldChange('slug', e.target.value)}
          error={errors.slug}
          helperText={
            mode === 'create'
              ? 'URL-friendly slug. If left blank, the backend automatically generates one from the title.'
              : 'URL-friendly identifier.'
          }
          placeholder="e.g. distributed-caching-strategies"
          disabled={isSubmitting}
          maxLength={200}
        />

        {/* Create Mode: KnowledgeTypeId & AuthorId */}
        {mode === 'create' && (
          <div className={styles.gridTwoCol}>
            <div className={styles.fieldGroup}>
              <Input
                id="article-knowledge-type-id"
                label="Knowledge Type ID *"
                value={formData.knowledgeTypeId}
                onChange={(e) => handleFieldChange('knowledgeTypeId', e.target.value)}
                error={errors.knowledgeTypeId}
                placeholder="xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx"
                disabled={isSubmitting}
                aria-required="true"
              />
              {discoveredKnowledgeTypes.length > 0 && (
                <div className={styles.suggestionsRow}>
                  <span className={styles.fieldHelper}>Known Types:</span>
                  {discoveredKnowledgeTypes.slice(0, 3).map((t) => (
                    <button
                      key={t.id}
                      type="button"
                      className={styles.suggestionBadge}
                      onClick={() => handleApplyDiscoveredType(t.id)}
                      title={`Use type: ${t.id}`}
                    >
                      <Sparkles size={10} style={{ display: 'inline', marginRight: 2 }} />
                      {t.name || t.id.substring(0, 8)}
                    </button>
                  ))}
                </div>
              )}
            </div>

            <div className={styles.fieldGroup}>
              <Input
                id="article-author-id"
                label="Author ID *"
                value={formData.authorId}
                onChange={(e) => handleFieldChange('authorId', e.target.value)}
                error={errors.authorId}
                placeholder="xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx"
                disabled={isSubmitting}
                suffix={
                  <button
                    type="button"
                    className={styles.autoIdBtn}
                    onClick={handleGenerateAuthorId}
                    title="Generate new author UUID"
                  >
                    New UUID
                  </button>
                }
                aria-required="true"
              />
              <span className={styles.fieldHelper}>Author identity associated with this article.</span>
            </div>
          </div>
        )}

        {/* Summary */}
        <div className={styles.fieldGroup}>
          <div className={styles.fieldLabel}>
            <label htmlFor="article-summary">
              Executive Summary <span className={styles.fieldRequired}>*</span>
            </label>
            <span className={styles.charCount}>{formData.summary.length} characters</span>
          </div>
          <textarea
            id="article-summary"
            className={`${styles.summaryTextarea} ${errors.summary ? styles.hasError : ''}`}
            value={formData.summary}
            onChange={(e) => handleFieldChange('summary', e.target.value)}
            placeholder="High-level overview of the solution, architecture, incident, or practice..."
            disabled={isSubmitting}
            rows={3}
            aria-required="true"
            aria-invalid={Boolean(errors.summary)}
          />
          {errors.summary && <span className={styles.fieldError}>{errors.summary}</span>}
          <span className={styles.fieldHelper}>
            A concise description rendered in search results and executive previews.
          </span>
        </div>

        {/* Content */}
        <div className={styles.fieldGroup}>
          <div className={styles.fieldLabel}>
            <label htmlFor="article-content">
              Documentation Content <span className={styles.fieldRequired}>*</span>
            </label>
            <span className={styles.charCount}>{formData.content.length} characters</span>
          </div>
          <textarea
            id="article-content"
            className={`${styles.contentTextarea} ${errors.content ? styles.hasError : ''}`}
            value={formData.content}
            onChange={(e) => handleFieldChange('content', e.target.value)}
            placeholder="Write engineering documentation, technical specifications, runbooks, or implementation notes..."
            disabled={isSubmitting}
            rows={14}
            aria-required="true"
            aria-invalid={Boolean(errors.content)}
          />
          {errors.content && <span className={styles.fieldError}>{errors.content}</span>}
          <span className={styles.fieldHelper}>
            Main technical content of the engineering article.
          </span>
        </div>
      </div>

      {/* Action Bar */}
      <div className={styles.actionBar}>
        <Button
          type="button"
          variant="subtle"
          size="md"
          icon={<ArrowLeft size={15} />}
          onClick={onCancel}
          disabled={isSubmitting}
        >
          {mode === 'create' ? 'Cancel' : 'Back to Article'}
        </Button>

        <Button
          type="submit"
          variant="primary"
          size="md"
          icon={<Save size={15} />}
          isLoading={isSubmitting}
          disabled={isSubmitting}
        >
          {mode === 'create' ? 'Save Draft' : 'Save Changes'}
        </Button>
      </div>
    </form>
  );
};
