import { useState, useEffect, useCallback, useRef } from 'react';
import { incidentApi } from '../api/incidentApi';
import type { IncidentDto, IncidentId } from '../types/incidentTypes';
import { ApiError } from '@/services/api/apiErrors';

export interface UseIncidentResult {
  incident: IncidentDto | null;
  isLoading: boolean;
  error: string | null;
  isNotFound: boolean;
  retry: () => void;
  setIncident: React.Dispatch<React.SetStateAction<IncidentDto | null>>;
}

export function useIncident(id: IncidentId | undefined): UseIncidentResult {
  const [incident, setIncident] = useState<IncidentDto | null>(null);
  const [isLoading, setIsLoading] = useState<boolean>(true);
  const [error, setError] = useState<string | null>(null);
  const [isNotFound, setIsNotFound] = useState<boolean>(false);

  const abortControllerRef = useRef<AbortController | null>(null);
  const requestIdRef = useRef<number>(0);

  const fetchIncident = useCallback(async () => {
    const currentRequestId = ++requestIdRef.current;

    if (!id || id.trim() === '') {
      setIncident(null);
      setIsLoading(false);
      setIsNotFound(true);
      setError('Incident ID is missing.');
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
    setIncident(null);

    try {
      const data = await incidentApi.getIncident(id, { signal: controller.signal });

      if (requestIdRef.current !== currentRequestId) {
        return;
      }

      setIncident(data);
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
          setError(`Incident with ID "${id}" was not found.`);
          return;
        }
        setError(err.message);
      } else {
        setError('An unexpected error occurred while loading incident details.');
      }
    } finally {
      if (requestIdRef.current === currentRequestId) {
        setIsLoading(false);
      }
    }
  }, [id]);

  useEffect(() => {
    fetchIncident();
    return () => {
      requestIdRef.current++;
      if (abortControllerRef.current) {
        abortControllerRef.current.abort();
      }
    };
  }, [fetchIncident]);

  return {
    incident,
    isLoading,
    error,
    isNotFound,
    retry: fetchIncident,
    setIncident,
  };
}
