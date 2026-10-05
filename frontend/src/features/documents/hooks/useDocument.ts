import { useState, useEffect, useCallback, useRef } from 'react';
import { documentApi } from '../api/documentApi';
import type { DocumentDto, DocumentId } from '../types/documentTypes';
import { ApiError } from '@/services/api/apiErrors';

export interface UseDocumentResult {
  document: DocumentDto | null;
  isLoading: boolean;
  error: string | null;
  isNotFound: boolean;
  refetch: () => void;
}

/**
 * Hook for fetching and managing a single engineering document by its ID.
 * Follows established race condition and cancellation patterns.
 */
export function useDocument(id: DocumentId | undefined): UseDocumentResult {
  const [document, setDocument] = useState<DocumentDto | null>(null);
  const [isLoading, setIsLoading] = useState<boolean>(true);
  const [error, setError] = useState<string | null>(null);
  const [isNotFound, setIsNotFound] = useState<boolean>(false);

  const abortControllerRef = useRef<AbortController | null>(null);
  const requestIdRef = useRef<number>(0);

  const fetchDocument = useCallback(async () => {
    if (!id) {
      setDocument(null);
      setIsLoading(false);
      setError('Document ID is missing.');
      setIsNotFound(false);
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
    setIsNotFound(false);

    try {
      const data = await documentApi.getDocumentById(id, {
        signal: controller.signal,
      });

      if (requestIdRef.current !== currentRequestId) {
        return;
      }

      setDocument(data);
    } catch (err: unknown) {
      if (requestIdRef.current !== currentRequestId) {
        return;
      }

      if (err instanceof ApiError) {
        if (err.status === 408) {
          return;
        }
        if (err.status === 404) {
          setIsNotFound(true);
          setError('Document not found.');
          return;
        }
        setError(err.message);
      } else {
        setError('Failed to load document details.');
      }
    } finally {
      if (requestIdRef.current === currentRequestId) {
        setIsLoading(false);
      }
    }
  }, [id]);

  useEffect(() => {
    fetchDocument();
    return () => {
      requestIdRef.current++;
      if (abortControllerRef.current) {
        abortControllerRef.current.abort();
      }
    };
  }, [fetchDocument]);

  return {
    document,
    isLoading,
    error,
    isNotFound,
    refetch: fetchDocument,
  };
}
