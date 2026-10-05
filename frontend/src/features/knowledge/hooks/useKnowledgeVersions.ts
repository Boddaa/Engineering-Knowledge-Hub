import { useState, useEffect, useCallback, useRef } from 'react';
import { knowledgeApi } from '../api/knowledgeApi';
import type { KnowledgeVersionDto, KnowledgeArticleId } from '../types/knowledgeTypes';
import { ApiError } from '@/services/api/apiErrors';

export interface UseKnowledgeVersionsResult {
  versions: KnowledgeVersionDto[];
  isLoading: boolean;
  error: string | null;
  refetch: () => void;
}

export function useKnowledgeVersions(articleId: KnowledgeArticleId | undefined): UseKnowledgeVersionsResult {
  const [versions, setVersions] = useState<KnowledgeVersionDto[]>([]);
  const [isLoading, setIsLoading] = useState<boolean>(true);
  const [error, setError] = useState<string | null>(null);

  const abortControllerRef = useRef<AbortController | null>(null);
  const requestIdRef = useRef<number>(0);

  const fetchVersions = useCallback(async () => {
    const currentRequestId = ++requestIdRef.current;

    if (!articleId || articleId.trim() === '') {
      setVersions([]);
      setIsLoading(false);
      setError(null);
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
      const data = await knowledgeApi.getVersionHistory(articleId, { signal: controller.signal });

      if (requestIdRef.current !== currentRequestId) {
        return;
      }

      // Sort newest versions first
      const sorted = [...data].sort((a, b) => b.versionNumber - a.versionNumber);
      setVersions(sorted);
    } catch (err: unknown) {
      if (requestIdRef.current !== currentRequestId) {
        return;
      }

      if (err instanceof ApiError) {
        if (err.status === 408) {
          // Aborted request
          return;
        }
        setError(err.problemDetails?.detail || err.message);
      } else {
        setError('Failed to load version history.');
      }
    } finally {
      if (requestIdRef.current === currentRequestId) {
        setIsLoading(false);
      }
    }
  }, [articleId]);

  useEffect(() => {
    fetchVersions();
    return () => {
      requestIdRef.current++;
      if (abortControllerRef.current) {
        abortControllerRef.current.abort();
      }
    };
  }, [fetchVersions]);

  return {
    versions,
    isLoading,
    error,
    refetch: fetchVersions,
  };
}
