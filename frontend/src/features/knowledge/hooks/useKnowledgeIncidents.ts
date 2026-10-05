import { useState, useEffect, useCallback, useRef } from 'react';
import { knowledgeApi } from '../api/knowledgeApi';
import type { KnowledgeArticleId } from '../types/knowledgeTypes';
import type { IncidentDto } from '@/features/incidents/types/incidentTypes';
import { ApiError } from '@/services/api/apiErrors';

export interface UseKnowledgeIncidentsResult {
  incidents: IncidentDto[];
  isLoading: boolean;
  error: string | null;
  retry: () => void;
}

export function useKnowledgeIncidents(
  articleId: KnowledgeArticleId | undefined
): UseKnowledgeIncidentsResult {
  const [incidents, setIncidents] = useState<IncidentDto[]>([]);
  const [isLoading, setIsLoading] = useState<boolean>(true);
  const [error, setError] = useState<string | null>(null);

  const abortControllerRef = useRef<AbortController | null>(null);
  const requestIdRef = useRef<number>(0);

  const fetchIncidents = useCallback(async () => {
    const currentRequestId = ++requestIdRef.current;

    if (!articleId || articleId.trim() === '') {
      setIncidents([]);
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
      const data = await knowledgeApi.getIncidentsForArticle(articleId, {
        signal: controller.signal,
      });

      if (requestIdRef.current !== currentRequestId) {
        return;
      }

      setIncidents(data);
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
        setError('An unexpected error occurred while loading linked incidents.');
      }
    } finally {
      if (requestIdRef.current === currentRequestId) {
        setIsLoading(false);
      }
    }
  }, [articleId]);

  useEffect(() => {
    fetchIncidents();
    return () => {
      requestIdRef.current++;
      if (abortControllerRef.current) {
        abortControllerRef.current.abort();
      }
    };
  }, [fetchIncidents]);

  return {
    incidents,
    isLoading,
    error,
    retry: fetchIncidents,
  };
}
