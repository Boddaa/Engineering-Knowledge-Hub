import { apiClient, type RequestOptions } from '@/services/api/apiClient';
import { ApiError } from '@/services/api/apiErrors';
import type {
  AiGenerateRequest,
  AiResponse,
  LocalAiHealthDto,
  RagQueryRequest,
  RagResponseDto,
} from '../types/assistantTypes';

/**
 * Strongly-typed API client routing through the shared apiClient to consume ASP.NET Core RAG and Local AI endpoints.
 * Endpoints consumed:
 * - POST /api/rag/ask
 * - GET  /api/ai/local/health
 * - POST /api/ai/generate
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
   * Inspects the operational readiness, daemon connectivity, and model availability of the Local AI subsystem.
   * Route: GET /api/ai/local/health
   * Note: The backend returns HTTP 200 for Healthy/Disabled, HTTP 503 for Unavailable/ModelUnavailable,
   * and HTTP 500 for Misconfigured. If the response body carries a valid LocalAiHealthDto, we extract it.
   */
  async getLocalAiHealth(options?: RequestOptions): Promise<LocalAiHealthDto> {
    try {
      return await apiClient.get<LocalAiHealthDto>('/api/ai/local/health', options);
    } catch (err: unknown) {
      if (err instanceof ApiError && err.problemDetails) {
        // If the error response body is actually the LocalAiHealthDto serialized by the controller
        const details = err.problemDetails as unknown as Partial<LocalAiHealthDto>;
        if (
          details &&
          typeof details.status === 'string' &&
          (details.status === 'Healthy' ||
            details.status === 'Disabled' ||
            details.status === 'Unavailable' ||
            details.status === 'ModelUnavailable' ||
            details.status === 'Misconfigured')
        ) {
          return details as LocalAiHealthDto;
        }
      }
      // Re-throw if it was a network connectivity failure or unparseable error
      throw err;
    }
  },

  /**
   * Direct text generation fallback through the configured AI provider.
   * Route: POST /api/ai/generate
   */
  async generateText(
    request: AiGenerateRequest,
    options?: RequestOptions
  ): Promise<AiResponse> {
    return apiClient.post<AiResponse>('/api/ai/generate', request, options);
  },
};
