import { useState, useEffect, useCallback, useMemo, useRef } from 'react';
import { auditApi } from '../api/auditApi';
import type {
  AuditLogDto,
  AuditFilterState,
  AuditPaginationState,
} from '../types/auditTypes';
import { isValidGuid } from '../utils/auditFormatters';

export interface UseAuditLogsResult {
  /** The paginated slice of logs currently displayed */
  paginatedLogs: AuditLogDto[];
  /** Total count of logs returned by the current API query */
  totalRawCount: number;
  /** Total count of logs matching client-side search query */
  totalFilteredCount: number;
  /** Current pagination state */
  pagination: AuditPaginationState;
  /** Total calculated pages */
  totalPages: number;
  /** Loading state */
  isLoading: boolean;
  /** Network or validation error message */
  error: string | null;
  /** Current filter parameters */
  filters: AuditFilterState;
  /** Update filter criteria */
  setFilters: React.Dispatch<React.SetStateAction<AuditFilterState>>;
  /** Update current page */
  setPage: (page: number) => void;
  /** Update page size */
  setPageSize: (pageSize: number) => void;
  /** Re-run the active audit query */
  refetch: () => void;
}

const DEFAULT_FILTERS: AuditFilterState = {
  mode: 'all',
  entityName: '',
  entityId: '',
  userId: '',
  searchTerm: '',
};

const DEFAULT_PAGINATION: AuditPaginationState = {
  page: 1,
  pageSize: 25,
};

export function useAuditLogs(): UseAuditLogsResult {
  const [filters, setFilters] = useState<AuditFilterState>(DEFAULT_FILTERS);
  const [pagination, setPagination] = useState<AuditPaginationState>(DEFAULT_PAGINATION);
  const [rawLogs, setRawLogs] = useState<AuditLogDto[]>([]);
  const [isLoading, setIsLoading] = useState<boolean>(false);
  const [error, setError] = useState<string | null>(null);

  // Keep track of in-flight abort controller
  const abortControllerRef = useRef<AbortController | null>(null);

  // Reset page to 1 when search or filter values change
  const prevFilterSignatureRef = useRef<string>('');
  const currentSignature = `${filters.mode}|${filters.entityName}|${filters.entityId}|${filters.userId}|${filters.searchTerm}|${pagination.pageSize}`;

  if (prevFilterSignatureRef.current !== currentSignature) {
    prevFilterSignatureRef.current = currentSignature;
    if (pagination.page !== 1) {
      setPagination((prev) => ({ ...prev, page: 1 }));
    }
  }

  const fetchLogs = useCallback(async () => {
    // Abort previous in-flight request
    if (abortControllerRef.current) {
      abortControllerRef.current.abort();
    }

    const controller = new AbortController();
    abortControllerRef.current = controller;

    // Validate inputs based on mode
    if (filters.mode === 'entity') {
      const trimmedEntityName = filters.entityName.trim();
      const trimmedEntityId = filters.entityId.trim();

      if (!trimmedEntityName) {
        setError('Please enter or select an Entity Name to filter by entity.');
        setRawLogs([]);
        setIsLoading(false);
        return;
      }

      if (!trimmedEntityId) {
        setError('Please enter an Entity ID (UUID) to filter by entity.');
        setRawLogs([]);
        setIsLoading(false);
        return;
      }

      if (!isValidGuid(trimmedEntityId)) {
        setError('Invalid Entity ID format. Entity ID must be a valid UUID (e.g. 12345678-1234-1234-1234-123456789abc).');
        setRawLogs([]);
        setIsLoading(false);
        return;
      }
    }

    if (filters.mode === 'user') {
      const trimmedUserId = filters.userId.trim();

      if (!trimmedUserId) {
        setError('Please enter a User ID (UUID) to filter by user.');
        setRawLogs([]);
        setIsLoading(false);
        return;
      }

      if (!isValidGuid(trimmedUserId)) {
        setError('Invalid User ID format. User ID must be a valid UUID (e.g. 12345678-1234-1234-1234-123456789abc).');
        setRawLogs([]);
        setIsLoading(false);
        return;
      }
    }

    setIsLoading(true);
    setError(null);

    try {
      let data: AuditLogDto[] = [];

      if (filters.mode === 'all') {
        data = await auditApi.getAll({ signal: controller.signal });
      } else if (filters.mode === 'entity') {
        data = await auditApi.getByEntity(
          filters.entityName.trim(),
          filters.entityId.trim(),
          { signal: controller.signal }
        );
      } else if (filters.mode === 'user') {
        data = await auditApi.getByUser(filters.userId.trim(), {
          signal: controller.signal,
        });
      }

      setRawLogs(Array.isArray(data) ? data : []);
      setError(null);
    } catch (err: unknown) {
      if (err instanceof DOMException && err.name === 'AbortError') {
        // Request was aborted due to new filter; ignore
        return;
      }

      const errorMessage =
        err instanceof Error
          ? err.message
          : 'Failed to retrieve governance audit logs from server.';
      setError(errorMessage);
      setRawLogs([]);
    } finally {
      setIsLoading(false);
    }
  }, [filters.mode, filters.entityName, filters.entityId, filters.userId]);

  // Trigger fetch when query parameters change
  useEffect(() => {
    fetchLogs();

    return () => {
      if (abortControllerRef.current) {
        abortControllerRef.current.abort();
      }
    };
  }, [fetchLogs]);

  // Client-side in-memory search/filtering
  const filteredLogs = useMemo(() => {
    const term = filters.searchTerm.trim().toLowerCase();
    if (!term) return rawLogs;

    return rawLogs.filter((log) => {
      const matchAction = log.action?.toLowerCase().includes(term);
      const matchEntity = log.entityName?.toLowerCase().includes(term);
      const matchEntityId = log.entityId?.toLowerCase().includes(term);
      const matchUserId = log.userId?.toLowerCase().includes(term);
      const matchDetails = log.details?.toLowerCase().includes(term);

      return (
        matchAction ||
        matchEntity ||
        matchEntityId ||
        matchUserId ||
        matchDetails
      );
    });
  }, [rawLogs, filters.searchTerm]);

  // Bounded Client-Side Pagination
  const totalFilteredCount = filteredLogs.length;
  const totalPages = Math.max(1, Math.ceil(totalFilteredCount / pagination.pageSize));

  // Ensure current page does not exceed totalPages
  const validPage = Math.min(Math.max(1, pagination.page), totalPages);

  const paginatedLogs = useMemo(() => {
    const startIndex = (validPage - 1) * pagination.pageSize;
    const endIndex = startIndex + pagination.pageSize;
    return filteredLogs.slice(startIndex, endIndex);
  }, [filteredLogs, validPage, pagination.pageSize]);

  const handleSetPage = useCallback((newPage: number) => {
    setPagination((prev) => ({
      ...prev,
      page: Math.max(1, newPage),
    }));
  }, []);

  const handleSetPageSize = useCallback((newPageSize: number) => {
    setPagination((prev) => ({
      ...prev,
      pageSize: newPageSize,
      page: 1,
    }));
  }, []);

  return {
    paginatedLogs,
    totalRawCount: rawLogs.length,
    totalFilteredCount,
    pagination: {
      page: validPage,
      pageSize: pagination.pageSize,
    },
    totalPages,
    isLoading,
    error,
    filters,
    setFilters,
    setPage: handleSetPage,
    setPageSize: handleSetPageSize,
    refetch: fetchLogs,
  };
}
