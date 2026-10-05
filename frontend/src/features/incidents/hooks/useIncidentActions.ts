import { useState, useCallback } from 'react';
import { incidentApi } from '../api/incidentApi';
import type {
  IncidentDto,
  IncidentId,
  ResolveIncidentRequest,
  LinkArticleRequest,
  CreateKnowledgeFromIncidentRequest,
  KnowledgeArticleDto,
} from '../types/incidentTypes';
import { ApiError } from '@/services/api/apiErrors';

export type IncidentActionType =
  | 'investigate'
  | 'resolve'
  | 'close'
  | 'link-article'
  | 'unlink-article'
  | 'propose-article';

export interface UseIncidentActionsResult {
  isSubmitting: boolean;
  executingAction: IncidentActionType | null;
  actionError: string | null;
  successFeedback: string | null;
  startInvestigation: (id: IncidentId) => Promise<IncidentDto | null>;
  resolveIncident: (id: IncidentId, request: ResolveIncidentRequest) => Promise<IncidentDto | null>;
  closeIncident: (id: IncidentId) => Promise<IncidentDto | null>;
  linkArticle: (id: IncidentId, request: LinkArticleRequest) => Promise<IncidentDto | null>;
  unlinkArticle: (id: IncidentId, articleId: string) => Promise<IncidentDto | null>;
  proposeArticle: (
    id: IncidentId,
    request: CreateKnowledgeFromIncidentRequest
  ) => Promise<KnowledgeArticleDto | null>;
  clearActionError: () => void;
  clearSuccessFeedback: () => void;
}

export function useIncidentActions(): UseIncidentActionsResult {
  const [isSubmitting, setIsSubmitting] = useState<boolean>(false);
  const [executingAction, setExecutingAction] = useState<IncidentActionType | null>(null);
  const [actionError, setActionError] = useState<string | null>(null);
  const [successFeedback, setSuccessFeedback] = useState<string | null>(null);

  const clearActionError = useCallback(() => setActionError(null), []);
  const clearSuccessFeedback = useCallback(() => setSuccessFeedback(null), []);

  const handleAction = async <T>(
    action: IncidentActionType,
    fn: () => Promise<T>,
    successMessage: string
  ): Promise<T | null> => {
    setIsSubmitting(true);
    setExecutingAction(action);
    setActionError(null);
    setSuccessFeedback(null);

    try {
      const result = await fn();
      setSuccessFeedback(successMessage);
      return result;
    } catch (err: unknown) {
      if (err instanceof ApiError) {
        setActionError(err.detail || err.message);
      } else {
        setActionError('An unexpected error occurred while executing the incident action.');
      }
      return null;
    } finally {
      setIsSubmitting(false);
      setExecutingAction(null);
    }
  };

  const startInvestigation = useCallback((id: IncidentId) => {
    return handleAction(
      'investigate',
      () => incidentApi.startInvestigation(id),
      'Incident transitioned to Investigating status.'
    );
  }, []);

  const resolveIncident = useCallback((id: IncidentId, request: ResolveIncidentRequest) => {
    return handleAction(
      'resolve',
      () => incidentApi.resolveIncident(id, request),
      'Incident successfully marked as Resolved.'
    );
  }, []);

  const closeIncident = useCallback((id: IncidentId) => {
    return handleAction(
      'close',
      () => incidentApi.closeIncident(id),
      'Incident successfully Closed.'
    );
  }, []);

  const linkArticle = useCallback((id: IncidentId, request: LinkArticleRequest) => {
    return handleAction(
      'link-article',
      () => incidentApi.linkArticle(id, request),
      'Knowledge article successfully linked to incident.'
    );
  }, []);

  const unlinkArticle = useCallback((id: IncidentId, articleId: string) => {
    return handleAction(
      'unlink-article',
      () => incidentApi.unlinkArticle(id, articleId),
      'Article link removed from incident.'
    );
  }, []);

  const proposeArticle = useCallback(
    (id: IncidentId, request: CreateKnowledgeFromIncidentRequest) => {
      return handleAction(
        'propose-article',
        () => incidentApi.proposeArticle(id, request),
        'Draft knowledge article successfully created from incident.'
      );
    },
    []
  );

  return {
    isSubmitting,
    executingAction,
    actionError,
    successFeedback,
    startInvestigation,
    resolveIncident,
    closeIncident,
    linkArticle,
    unlinkArticle,
    proposeArticle,
    clearActionError,
    clearSuccessFeedback,
  };
}
