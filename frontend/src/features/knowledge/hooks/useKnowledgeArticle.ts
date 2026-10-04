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

  const fetchArticle = useCallback(async () => {
    if (!id || id.trim() === '') {
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

    try {
      const data = await knowledgeApi.getArticle(id, { signal: controller.signal });
      setArticle(data);
    } catch (err: unknown) {
      if (err instanceof ApiError) {
        if (err.status === 408) {
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
      setIsLoading(false);
    }
  }, [id]);

  useEffect(() => {
    fetchArticle();
    return () => {
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
