import { useState, useEffect, useCallback, useMemo, useRef } from 'react';
import { useSearchParams } from 'react-router-dom';
import { knowledgeApi } from '../api/knowledgeApi';
import type {
  SearchArticleResultDto,
  PagedResult,
  SearchSortOption,
  KnowledgeStatus,
  VerificationStatus,
} from '../types/knowledgeTypes';
import {
  normalizeSearchSortOption,
  normalizeFilterStatus,
  normalizeFilterVerificationStatus,
  normalizeFilterUuid,
  normalizePage,
  normalizePageSize,
} from '../utils/knowledgeFormatters';
import { ApiError } from '@/services/api/apiErrors';

export interface KnowledgeFilterState {
  status?: KnowledgeStatus;
  verificationStatus?: VerificationStatus;
  knowledgeTypeId?: string;
  productId?: string;
  technologyId?: string;
  tagId?: string;
}

export interface UseKnowledgeSearchResult {
  articles: SearchArticleResultDto[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
  hasPreviousPage: boolean;
  hasNextPage: boolean;
  isLoading: boolean;
  error: string | null;
  searchQuery: string;
  sortBy: SearchSortOption;
  filters: KnowledgeFilterState;
  activeFilterCount: number;
  hasActiveSearchOrFilters: boolean;
  setSearchQuery: (query: string) => void;
  executeSearch: (newQuery?: string) => void;
  applyFilter: <K extends keyof KnowledgeFilterState>(key: K, value: KnowledgeFilterState[K]) => void;
  applyFilters: (newFilters: Partial<KnowledgeFilterState>) => void;
  removeFilter: (key: keyof KnowledgeFilterState) => void;
  clearAllFilters: () => void;
  goToPage: (targetPage: number) => void;
  changeSortBy: (sort: SearchSortOption) => void;
  retry: () => void;
}

const DEFAULT_PAGE_SIZE = 10;

export function useKnowledgeSearch(): UseKnowledgeSearchResult {
  const [searchParams, setSearchParams] = useSearchParams();

  // Read and safely normalize URL parameters as source of truth
  const urlQuery = searchParams.get('q') || '';
  const urlStatus = normalizeFilterStatus(searchParams.get('status'));
  const urlVerificationStatus = normalizeFilterVerificationStatus(searchParams.get('verificationStatus'));
  const urlKnowledgeTypeId = normalizeFilterUuid(searchParams.get('knowledgeTypeId'));
  const urlProductId = normalizeFilterUuid(searchParams.get('productId'));
  const urlTechnologyId = normalizeFilterUuid(searchParams.get('technologyId'));
  const urlTagId = normalizeFilterUuid(searchParams.get('tagId'));
  const urlSortBy = normalizeSearchSortOption(searchParams.get('sortBy'));
  const urlPage = normalizePage(searchParams.get('page'));
  const urlPageSize = normalizePageSize(searchParams.get('pageSize'), DEFAULT_PAGE_SIZE);

  // Local query state for the input field before user executes search
  const [searchQuery, setSearchQuery] = useState(urlQuery);

  // Keep local query in sync if URL changes (e.g. browser back/forward or direct URL navigation)
  useEffect(() => {
    setSearchQuery(urlQuery);
  }, [urlQuery]);

  // Structured active filter state
  const filters: KnowledgeFilterState = useMemo(
    () => ({
      status: urlStatus,
      verificationStatus: urlVerificationStatus,
      knowledgeTypeId: urlKnowledgeTypeId,
      productId: urlProductId,
      technologyId: urlTechnologyId,
      tagId: urlTagId,
    }),
    [
      urlStatus,
      urlVerificationStatus,
      urlKnowledgeTypeId,
      urlProductId,
      urlTechnologyId,
      urlTagId,
    ]
  );

  // Active filter count (excluding query, sortBy, pagination)
  const activeFilterCount = useMemo(() => {
    let count = 0;
    if (filters.status) count++;
    if (filters.verificationStatus) count++;
    if (filters.knowledgeTypeId) count++;
    if (filters.productId) count++;
    if (filters.technologyId) count++;
    if (filters.tagId) count++;
    return count;
  }, [filters]);

  const hasActiveSearchOrFilters = useMemo(
    () => Boolean(urlQuery.trim() || activeFilterCount > 0 || urlSortBy !== 'Relevance' || urlPage > 1),
    [urlQuery, activeFilterCount, urlSortBy, urlPage]
  );

  // Request state
  const [results, setResults] = useState<PagedResult<SearchArticleResultDto> | null>(null);
  const [isLoading, setIsLoading] = useState<boolean>(true);
  const [error, setError] = useState<string | null>(null);

  // Abort controller ref and request sequence guard to eliminate race conditions
  const abortControllerRef = useRef<AbortController | null>(null);
  const requestIdRef = useRef<number>(0);

  const fetchArticles = useCallback(async () => {
    const currentRequestId = ++requestIdRef.current;

    // Abort previous in-flight search
    if (abortControllerRef.current) {
      abortControllerRef.current.abort();
    }
    const controller = new AbortController();
    abortControllerRef.current = controller;

    setIsLoading(true);
    setError(null);

    try {
      const data = await knowledgeApi.searchArticles(
        {
          query: urlQuery || undefined,
          status: urlStatus,
          verificationStatus: urlVerificationStatus,
          knowledgeTypeId: urlKnowledgeTypeId,
          productId: urlProductId,
          technologyId: urlTechnologyId,
          tagId: urlTagId,
          sortBy: urlSortBy,
          page: urlPage,
          pageSize: urlPageSize,
        },
        { signal: controller.signal }
      );

      // Guard: Discard results if a newer request was dispatched
      if (requestIdRef.current !== currentRequestId) {
        return;
      }

      setResults(data);
    } catch (err: unknown) {
      // Guard: Discard error if a newer request was dispatched
      if (requestIdRef.current !== currentRequestId) {
        return;
      }

      if (err instanceof ApiError && err.status === 408) {
        // Ignored aborted request
        return;
      }
      const message = err instanceof ApiError ? err.message : 'Failed to retrieve knowledge articles.';
      setError(message);
    } finally {
      // Guard: Only the active/latest request is permitted to turn off loading state
      if (requestIdRef.current === currentRequestId) {
        setIsLoading(false);
      }
    }
  }, [
    urlQuery,
    urlStatus,
    urlVerificationStatus,
    urlKnowledgeTypeId,
    urlProductId,
    urlTechnologyId,
    urlTagId,
    urlSortBy,
    urlPage,
    urlPageSize,
  ]);

  useEffect(() => {
    fetchArticles();
    return () => {
      // Invalidate active request sequence and abort in-flight fetch
      requestIdRef.current++;
      if (abortControllerRef.current) {
        abortControllerRef.current.abort();
      }
    };
  }, [fetchArticles]);

  const executeSearch = useCallback(
    (newQuery?: string) => {
      const targetQuery = (newQuery !== undefined ? newQuery : searchQuery).trim();
      const params = new URLSearchParams(searchParams);

      if (targetQuery) {
        params.set('q', targetQuery);
      } else {
        params.delete('q');
      }

      // Always reset to page 1 on new query execution
      params.set('page', '1');

      setSearchParams(params);
    },
    [searchQuery, searchParams, setSearchParams]
  );

  const applyFilter = useCallback(
    <K extends keyof KnowledgeFilterState>(key: K, value: KnowledgeFilterState[K]) => {
      const params = new URLSearchParams(searchParams);

      if (value) {
        params.set(key, String(value));
      } else {
        params.delete(key);
      }

      // Reset to page 1 when filter changes
      params.set('page', '1');

      setSearchParams(params);
    },
    [searchParams, setSearchParams]
  );

  const applyFilters = useCallback(
    (newFilters: Partial<KnowledgeFilterState>) => {
      const params = new URLSearchParams(searchParams);

      (Object.keys(newFilters) as Array<keyof KnowledgeFilterState>).forEach((key) => {
        const value = newFilters[key];
        if (value) {
          params.set(key, String(value));
        } else {
          params.delete(key);
        }
      });

      // Reset to page 1 when filters change
      params.set('page', '1');

      setSearchParams(params);
    },
    [searchParams, setSearchParams]
  );

  const removeFilter = useCallback(
    (key: keyof KnowledgeFilterState) => {
      const params = new URLSearchParams(searchParams);
      params.delete(key);
      params.set('page', '1');
      setSearchParams(params);
    },
    [searchParams, setSearchParams]
  );

  const clearAllFilters = useCallback(() => {
    const params = new URLSearchParams();
    setSearchQuery('');
    setSearchParams(params);
  }, [setSearchParams]);

  const goToPage = useCallback(
    (targetPage: number) => {
      const params = new URLSearchParams(searchParams);
      params.set('page', String(Math.max(1, targetPage)));
      setSearchParams(params);
    },
    [searchParams, setSearchParams]
  );

  const changeSortBy = useCallback(
    (newSort: SearchSortOption) => {
      const validatedSort = normalizeSearchSortOption(newSort);
      const params = new URLSearchParams(searchParams);

      if (validatedSort !== 'Relevance') {
        params.set('sortBy', validatedSort);
      } else {
        params.delete('sortBy');
      }

      params.set('page', '1');
      setSearchParams(params);
    },
    [searchParams, setSearchParams]
  );

  const totalCount = results?.totalCount ?? 0;
  const pageSize = results?.pageSize ?? DEFAULT_PAGE_SIZE;
  const totalPages = results?.totalPages ?? (pageSize > 0 ? Math.ceil(totalCount / pageSize) : 0);
  const hasPreviousPage = results?.hasPreviousPage ?? (urlPage > 1 && totalPages > 0);
  const hasNextPage = results?.hasNextPage ?? (urlPage < totalPages);

  return {
    articles: results?.items ?? [],
    totalCount,
    page: urlPage,
    pageSize,
    totalPages,
    hasPreviousPage,
    hasNextPage,
    isLoading,
    error,
    searchQuery,
    sortBy: urlSortBy,
    filters,
    activeFilterCount,
    hasActiveSearchOrFilters,
    setSearchQuery,
    executeSearch,
    applyFilter,
    applyFilters,
    removeFilter,
    clearAllFilters,
    goToPage,
    changeSortBy,
    retry: fetchArticles,
  };
}
