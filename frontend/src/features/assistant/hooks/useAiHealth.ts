import { useState, useEffect, useCallback, useRef } from 'react';
import { assistantApi } from '../api/assistantApi';
import type { LocalAiHealthDto } from '../types/assistantTypes';

export interface UseAiHealthResult {
  health: LocalAiHealthDto | null;
  isLoading: boolean;
  error: string | null;
  isConnectionError: boolean;
  lastChecked: Date | null;
  checkHealth: () => Promise<void>;
}

export function useAiHealth(): UseAiHealthResult {
  const [health, setHealth] = useState<LocalAiHealthDto | null>(null);
  const [isLoading, setIsLoading] = useState<boolean>(true);
  const [error, setError] = useState<string | null>(null);
  const [isConnectionError, setIsConnectionError] = useState<boolean>(false);
  const [lastChecked, setLastChecked] = useState<Date | null>(null);

  const abortControllerRef = useRef<AbortController | null>(null);

  const checkHealth = useCallback(async () => {
    if (abortControllerRef.current) {
      abortControllerRef.current.abort();
    }
    const controller = new AbortController();
    abortControllerRef.current = controller;

    setIsLoading(true);
    setError(null);
    setIsConnectionError(false);

    try {
      const data = await assistantApi.getLocalAiHealth({ signal: controller.signal });
      setHealth(data);
      setLastChecked(new Date());
      setError(null);
      setIsConnectionError(false);
    } catch (err: unknown) {
      if (err instanceof DOMException && err.name === 'AbortError') {
        return;
      }
      setHealth(null);
      setIsConnectionError(true);
      const msg = err instanceof Error ? err.message : 'Unable to connect to Local AI health endpoint.';
      setError(msg);
    } finally {
      setIsLoading(false);
    }
  }, []);

  useEffect(() => {
    checkHealth();

    return () => {
      if (abortControllerRef.current) {
        abortControllerRef.current.abort();
      }
    };
  }, [checkHealth]);

  return {
    health,
    isLoading,
    error,
    isConnectionError,
    lastChecked,
    checkHealth,
  };
}
