import { useState, useEffect, useCallback, useRef } from 'react';
import { knowledgeApi } from '../api/knowledgeApi';
import type { KnowledgeArticleDto, KnowledgeArticleId } from '../types/knowledgeTypes';
import { ApiError } from '@/services/api/apiErrors';

export interface UseKnowledgeArticleResult {
  article: KnowledgeArticleDto | null;
  isLoading: boolean;
  error: string | null;
  isNotFound: boolean;
  retry: () => void;
}

export function useKnowledgeArticle(id: KnowledgeArticleId | undefined): UseKnowledgeArticleResult {
  const [article, setArticle] = useState<KnowledgeArticleDto | null>(null);
  const [isLoading, setIsLoading] = useState<boolean>(true);
  const [error, setError] = useState<string | null>(null);
  const [isNotFound, setIsNotFound] = useState<boolean>(false);

  const abortControllerRef = useRef<AbortController | null>(null);
  const requestIdRef = useRef<number>(0);

  const fetchArticle = useCallback(async () => {
    // Generate new request sequence identity
    const currentRequestId = ++requestIdRef.current;

    if (!id || id.trim() === '') {
      setArticle(null);
      setIsLoading(false);
      setIsNotFound(true);
      setError('Article ID is missing or invalid.');
      return;
    }

    if (abortControllerRef.current) {
      abortControllerRef.current.abort();
    }
    const controller = new AbortController();
    abortControllerRef.current = controller;

    setIsLoading(true);
    setError(null);
    setIsNotFound(false);
    setArticle(null);

    try {
      const data = await knowledgeApi.getArticle(id, { signal: controller.signal });

      // Guard: Discard results if an obsolete request
      if (requestIdRef.current !== currentRequestId) {
        return;
      }

      setArticle(data);
    } catch (err: unknown) {
      // Guard: Discard errors if an obsolete request
      if (requestIdRef.current !== currentRequestId) {
        return;
      }

      if (err instanceof ApiError) {
        if (err.status === 408) {
          // Ignored aborted request
          return;
        }
        if (err.status === 404) {
          setIsNotFound(true);
          setError(`Knowledge article with ID "${id}" was not found.`);
          return;
        }
        setError(err.message);
      } else {
        setError('An unexpected error occurred while loading article details.');
      }
    } finally {
      // Guard: Only the active/latest request is permitted to turn off loading state
      if (requestIdRef.current === currentRequestId) {
        setIsLoading(false);
      }
    }
  }, [id]);

  useEffect(() => {
    fetchArticle();
    return () => {
      // Invalidate active request sequence and abort in-flight fetch
      requestIdRef.current++;
      if (abortControllerRef.current) {
        abortControllerRef.current.abort();
      }
    };
  }, [fetchArticle]);

  return {
    article,
    isLoading,
    error,
    isNotFound,
    retry: fetchArticle,
  };
}
