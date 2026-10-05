import { useState, useEffect, useCallback, useRef } from 'react';
import { sourceApi } from '../api/sourceApi';
import type { KnowledgeSourceDto } from '../types/sourceTypes';
import { ApiError } from '@/services/api/apiErrors';

export interface UseArticleSourcesResult {
  sources: KnowledgeSourceDto[];
  isLoading: boolean;
  error: string | null;
  isAttaching: boolean;
  isDetaching: boolean;
  actionError: string | null;
  attachSource: (sourceId: string, referenceText?: string) => Promise<boolean>;
  detachSource: (sourceId: string) => Promise<boolean>;
  refetch: () => void;
  clearActionError: () => void;
}

/**
 * Hook for managing the knowledge sources attached to a specific article.
 * Supports race-safe fetching, attaching, and detaching with granular mutation state.
 */
export function useArticleSources(articleId: string | undefined): UseArticleSourcesResult {
  const [sources, setSources] = useState<KnowledgeSourceDto[]>([]);
  const [isLoading, setIsLoading] = useState<boolean>(true);
  const [error, setError] = useState<string | null>(null);

  const [isAttaching, setIsAttaching] = useState<boolean>(false);
  const [isDetaching, setIsDetaching] = useState<boolean>(false);
  const [actionError, setActionError] = useState<string | null>(null);

  const abortControllerRef = useRef<AbortController | null>(null);
  const requestIdRef = useRef<number>(0);

  const fetchArticleSources = useCallback(async () => {
    if (!articleId) {
      setSources([]);
      setIsLoading(false);
      return;
    }

    const currentRequestId = ++requestIdRef.current;

    if (abortControllerRef.current) {
      abortControllerRef.current.abort();
    }
    const controller = new AbortController();
    abortControllerRef.current = controller;

    setIsLoading(true);
    setError(null);

    try {
      const data = await sourceApi.getSourcesByArticleId(articleId, {
        signal: controller.signal,
      });

      if (requestIdRef.current !== currentRequestId) {
        return;
      }

      setSources(data);
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
        setError('Failed to load article sources.');
      }
    } finally {
      if (requestIdRef.current === currentRequestId) {
        setIsLoading(false);
      }
    }
  }, [articleId]);

  useEffect(() => {
    fetchArticleSources();
    return () => {
      requestIdRef.current++;
      if (abortControllerRef.current) {
        abortControllerRef.current.abort();
      }
    };
  }, [fetchArticleSources]);

  const attachSource = useCallback(
    async (sourceId: string, referenceText?: string): Promise<boolean> => {
      if (!articleId || !sourceId) return false;

      setIsAttaching(true);
      setActionError(null);

      try {
        await sourceApi.attachSourceToArticle(articleId, sourceId, {
          referenceText: referenceText?.trim() || null,
        });

        await fetchArticleSources();
        return true;
      } catch (err: unknown) {
        if (err instanceof ApiError) {
          setActionError(err.message);
        } else {
          setActionError('Failed to attach source to article.');
        }
        return false;
      } finally {
        setIsAttaching(false);
      }
    },
    [articleId, fetchArticleSources]
  );

  const detachSource = useCallback(
    async (sourceId: string): Promise<boolean> => {
      if (!articleId || !sourceId) return false;

      setIsDetaching(true);
      setActionError(null);

      try {
        await sourceApi.detachSourceFromArticle(articleId, sourceId);
        await fetchArticleSources();
        return true;
      } catch (err: unknown) {
        if (err instanceof ApiError) {
          setActionError(err.message);
        } else {
          setActionError('Failed to detach source from article.');
        }
        return false;
      } finally {
        setIsDetaching(false);
      }
    },
    [articleId, fetchArticleSources]
  );

  const clearActionError = useCallback(() => {
    setActionError(null);
  }, []);

  return {
    sources,
    isLoading,
    error,
    isAttaching,
    isDetaching,
    actionError,
    attachSource,
    detachSource,
    refetch: fetchArticleSources,
    clearActionError,
  };
}
