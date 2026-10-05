import { useState, useEffect, useCallback, useRef } from 'react';
import { sourceApi } from '../api/sourceApi';
import type { KnowledgeSourceDto, GetSourcesQueryParams } from '../types/sourceTypes';
import { ApiError } from '@/services/api/apiErrors';

export interface UseSourcesResult {
  sources: KnowledgeSourceDto[];
  isLoading: boolean;
  error: string | null;
  refetch: () => void;
}

/**
 * Custom hook for fetching and managing knowledge sources list from the backend.
 * Provides race condition protection, request cancellation, and stale-response prevention.
 */
export function useSources(params?: GetSourcesQueryParams): UseSourcesResult {
  const [sources, setSources] = useState<KnowledgeSourceDto[]>([]);
  const [isLoading, setIsLoading] = useState<boolean>(true);
  const [error, setError] = useState<string | null>(null);

  const abortControllerRef = useRef<AbortController | null>(null);
  const requestIdRef = useRef<number>(0);

  const sourceType = params?.sourceType;
  const trustLevel = params?.trustLevel;
  const isApproved = params?.isApproved;

  const fetchSources = useCallback(async () => {
    const currentRequestId = ++requestIdRef.current;

    if (abortControllerRef.current) {
      abortControllerRef.current.abort();
    }
    const controller = new AbortController();
    abortControllerRef.current = controller;

    setIsLoading(true);
    setError(null);

    try {
      const data = await sourceApi.getSources(
        {
          sourceType,
          trustLevel,
          isApproved,
        },
        { signal: controller.signal }
      );

      // Discard stale responses
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
          return; // Aborted request
        }
        setError(err.message);
      } else {
        setError('An unexpected error occurred while loading knowledge sources.');
      }
    } finally {
      if (requestIdRef.current === currentRequestId) {
        setIsLoading(false);
      }
    }
  }, [sourceType, trustLevel, isApproved]);

  useEffect(() => {
    fetchSources();
    return () => {
      requestIdRef.current++;
      if (abortControllerRef.current) {
        abortControllerRef.current.abort();
      }
    };
  }, [fetchSources]);

  return {
    sources,
    isLoading,
    error,
    refetch: fetchSources,
  };
}
