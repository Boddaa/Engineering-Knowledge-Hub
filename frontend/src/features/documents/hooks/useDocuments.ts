import { useState, useEffect, useCallback, useRef } from 'react';
import { documentApi } from '../api/documentApi';
import type { DocumentDto } from '../types/documentTypes';
import { ApiError } from '@/services/api/apiErrors';

export interface UseDocumentsResult {
  documents: DocumentDto[];
  isLoading: boolean;
  error: string | null;
  refetch: () => void;
}

/**
 * Custom hook for fetching and managing documents belonging to a knowledge source.
 * Provides race condition protection, request cancellation, and stale-response prevention.
 */
export function useDocuments(sourceId: string | undefined): UseDocumentsResult {
  const [documents, setDocuments] = useState<DocumentDto[]>([]);
  const [isLoading, setIsLoading] = useState<boolean>(Boolean(sourceId));
  const [error, setError] = useState<string | null>(null);

  const abortControllerRef = useRef<AbortController | null>(null);
  const requestIdRef = useRef<number>(0);

  const fetchDocuments = useCallback(async () => {
    if (!sourceId) {
      setDocuments([]);
      setIsLoading(false);
      setError(null);
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
      const data = await documentApi.getDocumentsBySourceId(sourceId, {
        signal: controller.signal,
      });

      if (requestIdRef.current !== currentRequestId) {
        return;
      }

      setDocuments(data);
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
        setError('Failed to load documents for the selected source.');
      }
    } finally {
      if (requestIdRef.current === currentRequestId) {
        setIsLoading(false);
      }
    }
  }, [sourceId]);

  useEffect(() => {
    fetchDocuments();
    return () => {
      requestIdRef.current++;
      if (abortControllerRef.current) {
        abortControllerRef.current.abort();
      }
    };
  }, [fetchDocuments]);

  return {
    documents,
    isLoading,
    error,
    refetch: fetchDocuments,
  };
}
