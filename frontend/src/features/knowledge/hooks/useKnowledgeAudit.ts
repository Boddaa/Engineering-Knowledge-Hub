import { useState, useEffect, useCallback, useRef } from 'react';
import { knowledgeApi } from '../api/knowledgeApi';
import type { AuditLogDto, KnowledgeArticleId } from '../types/knowledgeTypes';
import { ApiError } from '@/services/api/apiErrors';

export interface UseKnowledgeAuditResult {
  auditLogs: AuditLogDto[];
  isLoading: boolean;
  error: string | null;
  refetch: () => void;
}

export function useKnowledgeAudit(articleId: KnowledgeArticleId | undefined): UseKnowledgeAuditResult {
  const [auditLogs, setAuditLogs] = useState<AuditLogDto[]>([]);
  const [isLoading, setIsLoading] = useState<boolean>(true);
  const [error, setError] = useState<string | null>(null);

  const abortControllerRef = useRef<AbortController | null>(null);
  const requestIdRef = useRef<number>(0);

  const fetchAudit = useCallback(async () => {
    const currentRequestId = ++requestIdRef.current;

    if (!articleId || articleId.trim() === '') {
      setAuditLogs([]);
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
      const data = await knowledgeApi.getArticleAuditTrail(articleId, { signal: controller.signal });

      if (requestIdRef.current !== currentRequestId) {
        return;
      }

      // Sort newest audit entries first
      const sorted = [...data].sort(
        (a, b) => new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime()
      );
      setAuditLogs(sorted);
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
        setError('Failed to load governance audit trail.');
      }
    } finally {
      if (requestIdRef.current === currentRequestId) {
        setIsLoading(false);
      }
    }
  }, [articleId]);

  useEffect(() => {
    fetchAudit();
    return () => {
      requestIdRef.current++;
      if (abortControllerRef.current) {
        abortControllerRef.current.abort();
      }
    };
  }, [fetchAudit]);

  return {
    auditLogs,
    isLoading,
    error,
    refetch: fetchAudit,
  };
}
