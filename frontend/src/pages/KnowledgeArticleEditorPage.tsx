import React, { useState, useEffect, useCallback, useRef } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { ArrowLeft, FileQuestion } from 'lucide-react';
import { PageShell } from '@/components/layout/PageShell';
import { Skeleton } from '@/components/ui/Skeleton';
import { Button } from '@/components/ui/Button';
import { EmptyState } from '@/components/ui/EmptyState';
import { ErrorState } from '@/components/ui/ErrorState';
import {
  knowledgeApi,
  type KnowledgeArticleDto,
  KnowledgeArticleForm,
  type KnowledgeArticleFormData,
  type DiscoveredKnowledgeType,
} from '@/features/knowledge';
import { ApiError } from '@/services/api/apiErrors';
import styles from './KnowledgeArticleEditorPage.module.css';

export const KnowledgeArticleEditorPage: React.FC = () => {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const isEditMode = Boolean(id);

  // Edit Mode state
  const [existingArticle, setExistingArticle] = useState<KnowledgeArticleDto | null>(null);
  const [isLoadingArticle, setIsLoadingArticle] = useState<boolean>(isEditMode);
  const [articleLoadError, setArticleLoadError] = useState<string | null>(null);
  const [isNotFound, setIsNotFound] = useState<boolean>(false);

  // Form submission & feedback state
  const [isSubmitting, setIsSubmitting] = useState<boolean>(false);
  const [apiError, setApiError] = useState<string | null>(null);

  // Discovered types for create mode helper
  const [discoveredTypes, setDiscoveredTypes] = useState<DiscoveredKnowledgeType[]>([]);

  // Request generation guard for async safety
  const requestIdRef = useRef<number>(0);

  // Load existing article if in edit mode
  const fetchArticleForEditing = useCallback(async () => {
    if (!id || id.trim() === '') {
      setIsNotFound(true);
      setIsLoadingArticle(false);
      return;
    }

    const currentRequestId = ++requestIdRef.current;
    setIsLoadingArticle(true);
    setArticleLoadError(null);
    setIsNotFound(false);

    try {
      const data = await knowledgeApi.getArticle(id);
      if (requestIdRef.current !== currentRequestId) {
        return;
      }
      setExistingArticle(data);
    } catch (err: unknown) {
      if (requestIdRef.current !== currentRequestId) {
        return;
      }
      if (err instanceof ApiError) {
        if (err.status === 404) {
          setIsNotFound(true);
          return;
        }
        setArticleLoadError(err.message);
      } else {
        setArticleLoadError('Failed to load article for editing.');
      }
    } finally {
      if (requestIdRef.current === currentRequestId) {
        setIsLoadingArticle(false);
      }
    }
  }, [id]);

  useEffect(() => {
    if (isEditMode) {
      fetchArticleForEditing();
    } else {
      setIsLoadingArticle(false);
      // Discover known knowledge types to assist user in create mode
      knowledgeApi
        .searchArticles({ pageSize: 10 })
        .then((res) => {
          const uniqueTypes = new Map<string, DiscoveredKnowledgeType>();
          res.items.forEach((item) => {
            if (item.knowledgeTypeId && !uniqueTypes.has(item.knowledgeTypeId)) {
              uniqueTypes.set(item.knowledgeTypeId, { id: item.knowledgeTypeId });
            }
          });
          setDiscoveredTypes(Array.from(uniqueTypes.values()));
        })
        .catch(() => {
          // Non-blocking discovery
        });
    }

    return () => {
      requestIdRef.current++;
    };
  }, [isEditMode, fetchArticleForEditing]);

  const handleCancel = () => {
    if (isEditMode && id) {
      navigate(`/knowledge/${id}`);
    } else {
      navigate('/knowledge');
    }
  };

  const handleSubmit = async (formData: KnowledgeArticleFormData) => {
    setIsSubmitting(true);
    setApiError(null);

    try {
      if (isEditMode && id) {
        await knowledgeApi.updateArticle(id, {
          title: formData.title.trim(),
          summary: formData.summary.trim(),
          content: formData.content,
          slug: formData.slug.trim() || null,
        });

        // Navigate to the updated article detail
        navigate(`/knowledge/${id}`);
      } else {
        const created = await knowledgeApi.createArticle({
          title: formData.title.trim(),
          summary: formData.summary.trim(),
          content: formData.content,
          knowledgeTypeId: formData.knowledgeTypeId.trim(),
          authorId: formData.authorId.trim(),
          slug: formData.slug.trim() || null,
        });

        // Navigate to the newly created article detail
        navigate(`/knowledge/${created.id}`);
      }
    } catch (err: unknown) {
      setIsSubmitting(false);
      if (err instanceof ApiError) {
        setApiError(err.problemDetails?.detail || err.message);
      } else {
        setApiError('An unexpected network or server error occurred while saving.');
      }
    }
  };

  // Loading existing article skeleton
  if (isEditMode && isLoadingArticle) {
    return (
      <PageShell title="Loading Article Editor...">
        <div className={styles.skeletonEditor} aria-busy="true" aria-label="Loading article editor">
          <Skeleton variant="text" width={200} height={28} />
          <Skeleton variant="rectangular" height={52} />
          <Skeleton variant="rectangular" height={90} />
          <Skeleton variant="rectangular" height={320} />
          <div style={{ display: 'flex', justifyContent: 'flex-end', gap: '12px' }}>
            <Skeleton variant="rectangular" width={100} height={40} />
            <Skeleton variant="rectangular" width={120} height={40} />
          </div>
        </div>
      </PageShell>
    );
  }

  // Not Found in Edit Mode
  if (isEditMode && isNotFound) {
    return (
      <PageShell title="Article Not Found">
        <EmptyState
          icon={<FileQuestion size={36} />}
          title="Cannot Edit Article"
          description={`Knowledge article with ID "${id}" does not exist or was deleted.`}
          action={
            <Button
              variant="primary"
              size="md"
              icon={<ArrowLeft size={16} />}
              onClick={() => navigate('/knowledge')}
            >
              Back to Knowledge
            </Button>
          }
        />
      </PageShell>
    );
  }

  // Error Loading in Edit Mode
  if (isEditMode && (articleLoadError || !existingArticle)) {
    return (
      <PageShell title="Editor Error">
        <ErrorState
          title="Unable to Load Article for Editing"
          message={articleLoadError || 'Failed to retrieve the article from the server.'}
          onRetry={fetchArticleForEditing}
          actionText="Try Again"
        />
      </PageShell>
    );
  }

  const pageTitle = isEditMode ? `Edit: ${existingArticle?.title}` : 'New Knowledge Article';
  const pageDescription = isEditMode
    ? 'Update technical content, executive summary, and identifier slug.'
    : 'Author a new technical specification, solution guide, runbook, or incident review.';

  return (
    <PageShell
      title={pageTitle}
      description={pageDescription}
      headerActions={
        <Button
          variant="subtle"
          size="md"
          icon={<ArrowLeft size={15} />}
          onClick={handleCancel}
          disabled={isSubmitting}
          aria-label={isEditMode ? 'Cancel and return to article' : 'Cancel and return to knowledge list'}
        >
          {isEditMode ? 'Back to Article' : 'Back to Knowledge'}
        </Button>
      }
    >
      <div className={styles.container}>
        <KnowledgeArticleForm
          mode={isEditMode ? 'edit' : 'create'}
          initialData={
            isEditMode && existingArticle
              ? {
                  title: existingArticle.title,
                  slug: existingArticle.slug || '',
                  summary: existingArticle.summary,
                  content: existingArticle.content,
                  knowledgeTypeId: existingArticle.knowledgeTypeId,
                  authorId: existingArticle.authorId,
                }
              : undefined
          }
          isSubmitting={isSubmitting}
          apiError={apiError}
          onSubmit={handleSubmit}
          onCancel={handleCancel}
          discoveredKnowledgeTypes={discoveredTypes}
        />
      </div>
    </PageShell>
  );
};
