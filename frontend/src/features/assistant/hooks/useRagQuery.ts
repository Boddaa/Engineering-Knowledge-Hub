import { useState, useCallback, useRef } from 'react';
import { assistantApi } from '../api/assistantApi';
import type {
  RagQueryRequest,
  RagResponseDto,
  SessionQueryItem,
} from '../types/assistantTypes';

export interface SubmitQueryParams {
  question: string;
  maxEvidence?: number;
  minRelevanceScore?: number;
}

export interface UseRagQueryResult {
  activeResponse: RagResponseDto | null;
  activeQuestion: string | null;
  history: SessionQueryItem[];
  isSubmitting: boolean;
  error: string | null;
  ask: (params: SubmitQueryParams) => Promise<RagResponseDto | null>;
  selectHistoryItem: (id: string) => void;
  clearHistory: () => void;
  resetActive: () => void;
}

export function useRagQuery(): UseRagQueryResult {
  const [activeResponse, setActiveResponse] = useState<RagResponseDto | null>(null);
  const [activeQuestion, setActiveQuestion] = useState<string | null>(null);
  const [history, setHistory] = useState<SessionQueryItem[]>([]);
  const [isSubmitting, setIsSubmitting] = useState<boolean>(false);
  const [error, setError] = useState<string | null>(null);

  const abortControllerRef = useRef<AbortController | null>(null);

  const ask = useCallback(
    async ({
      question,
      maxEvidence = 5,
      minRelevanceScore = 0.25,
    }: SubmitQueryParams): Promise<RagResponseDto | null> => {
      // Prevent duplicate submissions while a request is in progress
      if (isSubmitting) {
        return null;
      }

      const trimmed = question.trim();

      // Enforce validation constraints
      if (!trimmed) {
        setError('Question cannot be empty or whitespace.');
        return null;
      }

      if (trimmed.length > 1000) {
        setError('Question cannot exceed 1,000 characters.');
        return null;
      }

      const clampedLimit = Math.max(1, Math.min(20, Math.round(maxEvidence || 5)));
      const clampedScore = Math.max(0, Math.min(1, minRelevanceScore ?? 0.25));

      // Abort previous inflight request if any
      if (abortControllerRef.current) {
        abortControllerRef.current.abort();
      }
      const controller = new AbortController();
      abortControllerRef.current = controller;

      setIsSubmitting(true);
      setError(null);
      setActiveQuestion(trimmed);

      const requestPayload: RagQueryRequest = {
        question: trimmed,
        maxEvidence: clampedLimit,
        minRelevanceScore: clampedScore,
      };

      try {
        const response = await assistantApi.askQuestion(requestPayload, {
          signal: controller.signal,
        });

        setActiveResponse(response);

        // Add to session-only history (strictly in memory)
        const historyItem: SessionQueryItem = {
          id: `${Date.now()}-${Math.random().toString(36).substring(2, 9)}`,
          question: trimmed,
          timestamp: new Date().toISOString(),
          response,
        };

        setHistory((prev) => [historyItem, ...prev]);
        return response;
      } catch (err: unknown) {
        if (err instanceof DOMException && err.name === 'AbortError') {
          return null;
        }
        const msg = err instanceof Error ? err.message : 'Failed to retrieve answer from AI assistant.';
        setError(msg);
        return null;
      } finally {
        setIsSubmitting(false);
      }
    },
    [isSubmitting]
  );

  const selectHistoryItem = useCallback(
    (id: string) => {
      const found = history.find((h) => h.id === id);
      if (found) {
        setActiveQuestion(found.question);
        setActiveResponse(found.response);
        setError(null);
      }
    },
    [history]
  );

  const clearHistory = useCallback(() => {
    setHistory([]);
  }, []);

  const resetActive = useCallback(() => {
    setActiveResponse(null);
    setActiveQuestion(null);
    setError(null);
  }, []);

  return {
    activeResponse,
    activeQuestion,
    history,
    isSubmitting,
    error,
    ask,
    selectHistoryItem,
    clearHistory,
    resetActive,
  };
}
