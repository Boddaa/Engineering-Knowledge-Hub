import { useState, useEffect, useCallback, useRef } from 'react';
import { knowledgeApi } from '../api/knowledgeApi';
import type {
  KnowledgeArticleId,
  KnowledgeFeedbackDto,
  FeedbackType,
} from '../types/knowledgeTypes';
import { ApiError } from '@/services/api/apiErrors';

export interface UseKnowledgeFeedbackResult {
  feedbackList: KnowledgeFeedbackDto[];
  isLoading: boolean;
  error: string | null;
  isSubmitting: boolean;
  submitError: string | null;
  submitSuccess: boolean;
  hasSubmitted: boolean;
  submitFeedback: (payload: {
    userId: string;
    feedbackType: FeedbackType | number;
    comment?: string;
  }) => Promise<boolean>;
  clearSubmitState: () => void;
  refetch: () => void;
}

const UUID_REGEX = /^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$/i;

export function useKnowledgeFeedback(
  articleId: KnowledgeArticleId | undefined
): UseKnowledgeFeedbackResult {
  const [feedbackList, setFeedbackList] = useState<KnowledgeFeedbackDto[]>([]);
  const [isLoading, setIsLoading] = useState<boolean>(true);
  const [error, setError] = useState<string | null>(null);

  const [isSubmitting, setIsSubmitting] = useState<boolean>(false);
  const [submitError, setSubmitError] = useState<string | null>(null);
  const [submitSuccess, setSubmitSuccess] = useState<boolean>(false);
  const [hasSubmitted, setHasSubmitted] = useState<boolean>(false);

  const abortControllerRef = useRef<AbortController | null>(null);
  const requestIdRef = useRef<number>(0);

  const fetchFeedback = useCallback(async () => {
    const currentRequestId = ++requestIdRef.current;

    if (!articleId || articleId.trim() === '') {
      setFeedbackList([]);
      setIsLoading(false);
      return;
    }

    if (abortControllerRef.current) {
      abortControllerRef.current.abort();
    }
    const controller = new AbortController();
    abortControllerRef.current = controller;

    setIsLoading(true);
    setError(null);

    try {
      const data = await knowledgeApi.getFeedbackForArticle(articleId, {
        signal: controller.signal,
      });

      if (requestIdRef.current !== currentRequestId) {
        return;
      }

      setFeedbackList(data);
    } catch (err: unknown) {
      if (requestIdRef.current !== currentRequestId) {
        return;
      }

      if (err instanceof ApiError) {
        if (err.status === 408) {
          return;
        }
        setError(err.message);
      } else {
        setError('An unexpected error occurred while loading article feedback.');
      }
    } finally {
      if (requestIdRef.current === currentRequestId) {
        setIsLoading(false);
      }
    }
  }, [articleId]);

  useEffect(() => {
    fetchFeedback();
    return () => {
      requestIdRef.current++;
      if (abortControllerRef.current) {
        abortControllerRef.current.abort();
      }
    };
  }, [fetchFeedback]);

  const submitFeedback = useCallback(
    async (payload: {
      userId: string;
      feedbackType: FeedbackType | number;
      comment?: string;
    }): Promise<boolean> => {
      if (!articleId) {
        setSubmitError('Article ID is missing.');
        return false;
      }

      const trimmedUserId = payload.userId.trim();
      if (!trimmedUserId) {
        setSubmitError('User ID is required.');
        return false;
      }
      if (!UUID_REGEX.test(trimmedUserId)) {
        setSubmitError('User ID must be a valid UUID.');
        return false;
      }

      if (hasSubmitted) {
        setSubmitError('Feedback has already been submitted for this session.');
        return false;
      }

      setIsSubmitting(true);
      setSubmitError(null);
      setSubmitSuccess(false);

      try {
        await knowledgeApi.submitFeedback(articleId, {
          userId: trimmedUserId,
          feedbackType: payload.feedbackType,
          comment: payload.comment?.trim() ? payload.comment.trim() : null,
        });

        setSubmitSuccess(true);
        setHasSubmitted(true);
        // Silently reload feedback list to show updated count
        fetchFeedback();
        return true;
      } catch (err: unknown) {
        if (err instanceof ApiError) {
          setSubmitError(err.detail || err.message);
        } else {
          setSubmitError('Failed to submit article feedback.');
        }
        return false;
      } finally {
        setIsSubmitting(false);
      }
    },
    [articleId, hasSubmitted, fetchFeedback]
  );

  const clearSubmitState = useCallback(() => {
    setSubmitError(null);
    setSubmitSuccess(false);
  }, []);

  return {
    feedbackList,
    isLoading,
    error,
    isSubmitting,
    submitError,
    submitSuccess,
    hasSubmitted,
    submitFeedback,
    clearSubmitState,
    refetch: fetchFeedback,
  };
}
