import { apiClient, type RequestOptions } from '@/services/api/apiClient';
import { ApiError } from '@/services/api/apiErrors';
import { normalizeHealthStatus } from '../utils/assistantFormatters';
import type {
  LocalAiHealthDto,
  RagQueryRequest,
  RagResponseDto,
} from '../types/assistantTypes';

/**
 * Strongly-typed API client routing through the shared apiClient to consume ASP.NET Core RAG and Local AI endpoints.
 * Endpoints consumed:
 * - POST /api/rag/ask
 * - GET  /api/ai/local/health
 */
export const assistantApi = {
  /**
   * Submits a question to the RAG knowledge assistant for evidence retrieval and grounded synthesis.
   * Route: POST /api/rag/ask
   */
  async askQuestion(
    request: RagQueryRequest,
    options?: RequestOptions
  ): Promise<RagResponseDto> {
    return apiClient.post<RagResponseDto>('/api/rag/ask', request, options);
  },

  /**
   * Inspects operational readiness, daemon connectivity, and model availability of the Local AI subsystem.
   * Route: GET /api/ai/local/health
   * Note: The backend returns HTTP 200 for Healthy/Disabled, HTTP 503 for Unavailable/ModelUnavailable,
   * and HTTP 500 for Misconfigured. When the response body carries a valid LocalAiHealthDto,
   * it is extracted and returned as a confirmed diagnostic state.
   * Real network errors or malformed payloads without health diagnostic properties are rethrown as errors.
   */
  async getLocalAiHealth(options?: RequestOptions): Promise<LocalAiHealthDto> {
    try {
      const data = await apiClient.get<LocalAiHealthDto>('/api/ai/local/health', options);
      return {
        ...data,
        status: normalizeHealthStatus(data.status),
      };
    } catch (err: unknown) {
      if (err instanceof ApiError && err.problemDetails) {
        const raw = err.problemDetails as Record<string, unknown>;

        // Verify if the error payload contains actual LocalAiHealthDto diagnostic properties
        const hasHealthProps =
          'runtimeReachable' in raw ||
          'RuntimeReachable' in raw ||
          'status' in raw ||
          'Status' in raw ||
          'diagnosticMessage' in raw ||
          'DiagnosticMessage' in raw;

        if (hasHealthProps) {
          const rawStatus = raw.status ?? raw.Status;
          const status = normalizeHealthStatus(rawStatus);
          const enabled = Boolean(raw.enabled ?? raw.Enabled ?? false);
          const runtimeReachable = Boolean(raw.runtimeReachable ?? raw.RuntimeReachable ?? false);
          const generationModelConfigured = (raw.generationModelConfigured ?? raw.GenerationModelConfigured ?? null) as string | null;
          const generationModelAvailable = Boolean(raw.generationModelAvailable ?? raw.GenerationModelAvailable ?? false);
          const embeddingModelConfigured = (raw.embeddingModelConfigured ?? raw.EmbeddingModelConfigured ?? null) as string | null;
          const embeddingModelAvailable = Boolean(raw.embeddingModelAvailable ?? raw.EmbeddingModelAvailable ?? false);
          const diagnosticMessage = String(raw.diagnosticMessage ?? raw.DiagnosticMessage ?? err.message ?? '');

          return {
            enabled,
            runtimeReachable,
            generationModelConfigured,
            generationModelAvailable,
            embeddingModelConfigured,
            embeddingModelAvailable,
            status,
            diagnosticMessage,
          };
        }
      }

      // Re-throw if it was a network connectivity failure, timeout, or malformed non-health response
      throw err;
    }
  },
};
