import { useState, useEffect, useCallback, useRef } from 'react';
import { useSearchParams } from 'react-router-dom';
import { knowledgeApi } from '../api/knowledgeApi';
import type {
  SearchArticleResultDto,
  PagedResult,
  SearchSortOption,
} from '../types/knowledgeTypes';
import { normalizeSearchSortOption } from '../utils/knowledgeFormatters';
import { ApiError } from '@/services/api/apiErrors';

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
  setSearchQuery: (query: string) => void;
  executeSearch: (queryToExecute?: string) => void;
  goToPage: (targetPage: number) => void;
  changeSortBy: (sort: SearchSortOption) => void;
  retry: () => void;
}

const DEFAULT_PAGE_SIZE = 10;

export function useKnowledgeSearch(): UseKnowledgeSearchResult {
  const [searchParams, setSearchParams] = useSearchParams();

  // Read URL parameters as source of truth with strict sort normalization
  const urlQuery = searchParams.get('q') || '';
  const urlPage = Math.max(1, parseInt(searchParams.get('page') || '1', 10));
  const rawSortBy = searchParams.get('sortBy');
  const urlSortBy = normalizeSearchSortOption(rawSortBy);

  // Local query state for the input field before submission
  const [searchQuery, setSearchQuery] = useState(urlQuery);

  // Keep local query in sync if URL changes (e.g. browser back/forward)
  useEffect(() => {
    setSearchQuery(urlQuery);
  }, [urlQuery]);

  // Request state
  const [results, setResults] = useState<PagedResult<SearchArticleResultDto> | null>(null);
  const [isLoading, setIsLoading] = useState<boolean>(true);
  const [error, setError] = useState<string | null>(null);

  // Abort controller ref and request sequence guard to eliminate race conditions
  const abortControllerRef = useRef<AbortController | null>(null);
  const requestIdRef = useRef<number>(0);

  const fetchArticles = useCallback(async () => {
    // Generate new request sequence identity
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
          page: urlPage,
          pageSize: DEFAULT_PAGE_SIZE,
          sortBy: urlSortBy,
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
  }, [urlQuery, urlPage, urlSortBy]);

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

  const executeSearch = useCallback((newQuery?: string) => {
    const targetQuery = (newQuery !== undefined ? newQuery : searchQuery).trim();
    const params = new URLSearchParams();

    if (targetQuery) {
      params.set('q', targetQuery);
    }
    if (urlSortBy && urlSortBy !== 'Relevance') {
      params.set('sortBy', urlSortBy);
    }
    // Always reset to page 1 on new search
    params.set('page', '1');

    setSearchParams(params);
  }, [searchQuery, urlSortBy, setSearchParams]);

  const goToPage = useCallback((targetPage: number) => {
    const params = new URLSearchParams(searchParams);
    params.set('page', String(Math.max(1, targetPage)));
    setSearchParams(params);
  }, [searchParams, setSearchParams]);

  const changeSortBy = useCallback((newSort: SearchSortOption) => {
    const validatedSort = normalizeSearchSortOption(newSort);
    const params = new URLSearchParams(searchParams);
    params.set('sortBy', validatedSort);
    params.set('page', '1');
    setSearchParams(params);
  }, [searchParams, setSearchParams]);

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
    setSearchQuery,
    executeSearch,
    goToPage,
    changeSortBy,
    retry: fetchArticles,
  };
}
