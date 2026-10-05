import { useState, useCallback, useRef } from 'react';
import { knowledgeApi } from '../api/knowledgeApi';
import type { KnowledgeArticleId } from '../types/knowledgeTypes';
import { ApiError } from '@/services/api/apiErrors';

export type GovernanceActionType =
  | 'submit-review'
  | 'approve'
  | 'reject'
  | 'deprecate'
  | 'archive';

export interface UseKnowledgeGovernanceResult {
  isSubmitting: boolean;
  executingAction: GovernanceActionType | null;
  actionError: string | null;
  successFeedback: string | null;
  submitReview: (id: KnowledgeArticleId, submitterId?: string) => Promise<boolean>;
  approve: (id: KnowledgeArticleId, reviewerId: string, changeSummary?: string) => Promise<boolean>;
  reject: (id: KnowledgeArticleId, reviewerId: string) => Promise<boolean>;
  deprecate: (id: KnowledgeArticleId, actorId?: string) => Promise<boolean>;
  archive: (id: KnowledgeArticleId, actorId?: string) => Promise<boolean>;
  clearActionError: () => void;
  clearSuccessFeedback: () => void;
}

export function useKnowledgeGovernance(): UseKnowledgeGovernanceResult {
  const [executingAction, setExecutingAction] = useState<GovernanceActionType | null>(null);
  const [actionError, setActionError] = useState<string | null>(null);
  const [successFeedback, setSuccessFeedback] = useState<string | null>(null);

  const activeArticleIdRef = useRef<KnowledgeArticleId | null>(null);

  const clearActionError = useCallback(() => {
    setActionError(null);
  }, []);

  const clearSuccessFeedback = useCallback(() => {
    setSuccessFeedback(null);
  }, []);

  const submitReview = useCallback(
    async (id: KnowledgeArticleId, submitterId?: string): Promise<boolean> => {
      activeArticleIdRef.current = id;
      setExecutingAction('submit-review');
      setActionError(null);
      setSuccessFeedback(null);

      try {
        await knowledgeApi.submitForReview(id, {
          submitterId: submitterId?.trim() || null,
        });

        if (activeArticleIdRef.current === id) {
          setSuccessFeedback('Article submitted for peer review successfully.');
        }
        return true;
      } catch (err: unknown) {
        if (activeArticleIdRef.current === id) {
          if (err instanceof ApiError) {
            setActionError(err.problemDetails?.detail || err.message);
          } else {
            setActionError('Failed to submit article for review.');
          }
        }
        return false;
      } finally {
        if (activeArticleIdRef.current === id) {
          setExecutingAction(null);
        }
      }
    },
    []
  );

  const approve = useCallback(
    async (
      id: KnowledgeArticleId,
      reviewerId: string,
      changeSummary?: string
    ): Promise<boolean> => {
      activeArticleIdRef.current = id;
      setExecutingAction('approve');
      setActionError(null);
      setSuccessFeedback(null);

      try {
        await knowledgeApi.approveArticle(id, {
          reviewerId: reviewerId.trim(),
          changeSummary: changeSummary?.trim() || null,
        });

        if (activeArticleIdRef.current === id) {
          setSuccessFeedback('Article approved and published. Historical version snapshot created.');
        }
        return true;
      } catch (err: unknown) {
        if (activeArticleIdRef.current === id) {
          if (err instanceof ApiError) {
            setActionError(err.problemDetails?.detail || err.message);
          } else {
            setActionError('Failed to approve article.');
          }
        }
        return false;
      } finally {
        if (activeArticleIdRef.current === id) {
          setExecutingAction(null);
        }
      }
    },
    []
  );

  const reject = useCallback(
    async (id: KnowledgeArticleId, reviewerId: string): Promise<boolean> => {
      activeArticleIdRef.current = id;
      setExecutingAction('reject');
      setActionError(null);
      setSuccessFeedback(null);

      try {
        await knowledgeApi.rejectArticle(id, {
          reviewerId: reviewerId.trim(),
        });

        if (activeArticleIdRef.current === id) {
          setSuccessFeedback('Article rejected and returned to draft for revisions.');
        }
        return true;
      } catch (err: unknown) {
        if (activeArticleIdRef.current === id) {
          if (err instanceof ApiError) {
            setActionError(err.problemDetails?.detail || err.message);
          } else {
            setActionError('Failed to reject article.');
          }
        }
        return false;
      } finally {
        if (activeArticleIdRef.current === id) {
          setExecutingAction(null);
        }
      }
    },
    []
  );

  const deprecate = useCallback(
    async (id: KnowledgeArticleId, actorId?: string): Promise<boolean> => {
      activeArticleIdRef.current = id;
      setExecutingAction('deprecate');
      setActionError(null);
      setSuccessFeedback(null);

      try {
        await knowledgeApi.deprecateArticle(id, {
          actorId: actorId?.trim() || null,
        });

        if (activeArticleIdRef.current === id) {
          setSuccessFeedback('Article has been deprecated.');
        }
        return true;
      } catch (err: unknown) {
        if (activeArticleIdRef.current === id) {
          if (err instanceof ApiError) {
            setActionError(err.problemDetails?.detail || err.message);
          } else {
            setActionError('Failed to deprecate article.');
          }
        }
        return false;
      } finally {
        if (activeArticleIdRef.current === id) {
          setExecutingAction(null);
        }
      }
    },
    []
  );

  const archive = useCallback(
    async (id: KnowledgeArticleId, actorId?: string): Promise<boolean> => {
      activeArticleIdRef.current = id;
      setExecutingAction('archive');
      setActionError(null);
      setSuccessFeedback(null);

      try {
        await knowledgeApi.archiveArticle(id, {
          actorId: actorId?.trim() || null,
        });

        if (activeArticleIdRef.current === id) {
          setSuccessFeedback('Article has been archived.');
        }
        return true;
      } catch (err: unknown) {
        if (activeArticleIdRef.current === id) {
          if (err instanceof ApiError) {
            setActionError(err.problemDetails?.detail || err.message);
          } else {
            setActionError('Failed to archive article.');
          }
        }
        return false;
      } finally {
        if (activeArticleIdRef.current === id) {
          setExecutingAction(null);
        }
      }
    },
    []
  );

  return {
    isSubmitting: executingAction !== null,
    executingAction,
    actionError,
    successFeedback,
    submitReview,
    approve,
    reject,
    deprecate,
    archive,
    clearActionError,
    clearSuccessFeedback,
  };
}
