import { useState, useEffect, useCallback, useRef } from 'react';
import { useSearchParams } from 'react-router-dom';
import { incidentApi } from '../api/incidentApi';
import type { IncidentDto, IncidentStatus } from '../types/incidentTypes';
import { normalizeStatusFilter } from '../utils/incidentFormatters';
import { ApiError } from '@/services/api/apiErrors';

export interface UseIncidentsResult {
  incidents: IncidentDto[];
  isLoading: boolean;
  error: string | null;
  selectedStatus: IncidentStatus | undefined;
  changeStatusFilter: (status?: IncidentStatus) => void;
  retry: () => void;
}

export function useIncidents(): UseIncidentsResult {
  const [searchParams, setSearchParams] = useSearchParams();

  // Read and safely normalize status from URL search params
  const selectedStatus = normalizeStatusFilter(searchParams.get('status'));

  const [incidents, setIncidents] = useState<IncidentDto[]>([]);
  const [isLoading, setIsLoading] = useState<boolean>(true);
  const [error, setError] = useState<string | null>(null);

  const abortControllerRef = useRef<AbortController | null>(null);
  const requestIdRef = useRef<number>(0);

  const fetchIncidents = useCallback(async () => {
    const currentRequestId = ++requestIdRef.current;

    if (abortControllerRef.current) {
      abortControllerRef.current.abort();
    }
    const controller = new AbortController();
    abortControllerRef.current = controller;

    setIsLoading(true);
    setError(null);

    try {
      const data = await incidentApi.getIncidents(selectedStatus, {
        signal: controller.signal,
      });

      // Discard if obsolete response
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
          return; // Aborted request
        }
        setError(err.message);
      } else {
        setError('An unexpected error occurred while loading incidents.');
      }
    } finally {
      if (requestIdRef.current === currentRequestId) {
        setIsLoading(false);
      }
    }
  }, [selectedStatus]);

  useEffect(() => {
    fetchIncidents();
    return () => {
      requestIdRef.current++;
      if (abortControllerRef.current) {
        abortControllerRef.current.abort();
      }
    };
  }, [fetchIncidents]);

  const changeStatusFilter = useCallback(
    (newStatus?: IncidentStatus) => {
      setSearchParams(
        (prev) => {
          const next = new URLSearchParams(prev);
          if (newStatus) {
            next.set('status', newStatus);
          } else {
            next.delete('status');
          }
          return next;
        },
        { replace: false }
      );
    },
    [setSearchParams]
  );

  return {
    incidents,
    isLoading,
    error,
    selectedStatus,
    changeStatusFilter,
    retry: fetchIncidents,
  };
}
