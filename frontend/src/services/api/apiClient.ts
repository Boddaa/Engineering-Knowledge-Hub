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

    const controller = new AbortController();
    const timeoutId = setTimeout(() => controller.abort(), timeoutMs);

    const mergedSignal = fetchOptions.signal
      ? anySignal([fetchOptions.signal, controller.signal])
      : controller.signal;

    try {
      const response = await fetch(url, {
        ...fetchOptions,
        signal: mergedSignal,
        headers: {
          ...API_CONFIG.defaultHeaders,
          ...fetchOptions.headers,
        },
      });

      clearTimeout(timeoutId);

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
      clearTimeout(timeoutId);

      if (err instanceof ApiError) {
        throw err;
      }

      if (err instanceof DOMException && err.name === 'AbortError') {
        throw new ApiError(408, 'Request timed out or was aborted.');
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

/**
 * Combines multiple abort signals into one.
 */
function anySignal(signals: AbortSignal[]): AbortSignal {
  const controller = new AbortController();
  for (const signal of signals) {
    if (signal.aborted) {
      controller.abort();
      return signal;
    }
    signal.addEventListener('abort', () => controller.abort(), { once: true });
  }
  return controller.signal;
}

export const apiClient = new ApiClient();
export { ApiClient };
