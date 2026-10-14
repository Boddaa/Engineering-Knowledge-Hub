import { API_CONFIG } from './apiConfig';
import { ApiError, type ProblemDetails } from './apiErrors';

export interface RequestOptions extends Omit<RequestInit, 'body'> {
  params?: Record<string, string | number | boolean | undefined | null>;
  timeoutMs?: number;
}

class ApiClient {
  private readonly baseUrl: string;

  constructor(baseUrl: string = API_CONFIG.baseUrl) {
    this.baseUrl = baseUrl.replace(/\/+$/, '');
  }

  private buildUrl(path: string, params?: RequestOptions['params']): string {
    const cleanPath = path.startsWith('/') ? path : `/${path}`;

    // Normalize path to avoid /api/api when baseUrl already contains /api
    let fullPath: string;
    if (this.baseUrl.endsWith('/api') && (cleanPath.startsWith('/api/') || cleanPath === '/api')) {
      const strippedPath = cleanPath.slice(4);
      fullPath = `${this.baseUrl}${strippedPath}`;
    } else {
      fullPath = `${this.baseUrl}${cleanPath}`;
    }

    const isAbsolute = /^https?:\/\//i.test(fullPath);
    const url = isAbsolute
      ? new URL(fullPath)
      : new URL(fullPath, window.location.origin);

    if (params) {
      Object.entries(params).forEach(([key, value]) => {
        if (value !== undefined && value !== null && value !== '') {
          url.searchParams.append(key, String(value));
        }
      });
    }

    return isAbsolute ? url.toString() : url.pathname + url.search;
  }

  private async request<T>(
    path: string,
    options: RequestInit & { timeoutMs?: number; params?: RequestOptions['params'] } = {}
  ): Promise<T> {
    const { timeoutMs = API_CONFIG.timeoutMs, params, ...fetchOptions } = options;
    const url = this.buildUrl(path, params);

    // Pre-flight check: if caller's signal is already aborted, fail immediately without network dispatch
    if (fetchOptions.signal?.aborted) {
      throw new ApiError(408, 'Request was aborted.', undefined, true, false);
    }

    const timeoutController = new AbortController();
    let didTimeout = false;
    const timeoutId = setTimeout(() => {
      didTimeout = true;
      timeoutController.abort();
    }, timeoutMs);

    // Merge caller signal and internal timeout controller with explicit lifecycle cleanup
    const mergedController = new AbortController();
    const onCallerAbort = () => mergedController.abort();
    const onTimeoutAbort = () => mergedController.abort();

    if (fetchOptions.signal) {
      fetchOptions.signal.addEventListener('abort', onCallerAbort, { once: true });
    }
    timeoutController.signal.addEventListener('abort', onTimeoutAbort, { once: true });

    const cleanupSignals = () => {
      clearTimeout(timeoutId);
      if (fetchOptions.signal) {
        fetchOptions.signal.removeEventListener('abort', onCallerAbort);
      }
      timeoutController.signal.removeEventListener('abort', onTimeoutAbort);
    };

    try {
      const response = await fetch(url, {
        ...fetchOptions,
        signal: mergedController.signal,
        headers: {
          ...API_CONFIG.defaultHeaders,
          ...fetchOptions.headers,
        },
      });

      cleanupSignals();

      if (!response.ok) {
        let problemDetails: ProblemDetails | undefined;
        let errorMessage = `HTTP ${response.status} ${response.statusText}`;

        try {
          const data = await response.json();
          problemDetails = data as ProblemDetails;
          errorMessage = problemDetails.detail || problemDetails.title || errorMessage;
        } catch {
          // If response body is not JSON, use default error message
        }

        throw new ApiError(response.status, errorMessage, problemDetails);
      }

      if (response.status === 204) {
        return undefined as unknown as T;
      }

      return (await response.json()) as T;
    } catch (err: unknown) {
      cleanupSignals();

      if (err instanceof ApiError) {
        throw err;
      }

      const isAbort =
        (err instanceof DOMException && err.name === 'AbortError') ||
        (err instanceof Error && err.name === 'AbortError');

      if (isAbort) {
        if (didTimeout) {
          throw new ApiError(408, 'Request timed out.', undefined, false, true);
        }
        // User/caller aborted via signal
        throw new ApiError(408, 'Request was aborted.', undefined, true, false);
      }

      const message = err instanceof Error ? err.message : 'Network error occurred.';
      throw new ApiError(0, message);
    }
  }

  public get<T>(path: string, options?: RequestOptions): Promise<T> {
    return this.request<T>(path, { ...options, method: 'GET' });
  }

  public post<T>(path: string, body?: unknown, options?: RequestOptions): Promise<T> {
    return this.request<T>(path, {
      ...options,
      method: 'POST',
      body: body !== undefined ? JSON.stringify(body) : undefined,
    });
  }

  public put<T>(path: string, body?: unknown, options?: RequestOptions): Promise<T> {
    return this.request<T>(path, {
      ...options,
      method: 'PUT',
      body: body !== undefined ? JSON.stringify(body) : undefined,
    });
  }

  public delete<T>(path: string, options?: RequestOptions): Promise<T> {
    return this.request<T>(path, { ...options, method: 'DELETE' });
  }
}

export const apiClient = new ApiClient();
export { ApiClient };
