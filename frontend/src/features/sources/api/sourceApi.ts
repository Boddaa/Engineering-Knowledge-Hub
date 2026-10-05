import { apiClient, type RequestOptions } from '@/services/api/apiClient';
import type { KnowledgeArticleDto } from '@/features/knowledge/types/knowledgeTypes';
import type {
  KnowledgeSourceDto,
  GetSourcesQueryParams,
  AttachSourceRequest,
  CreateKnowledgeSourceRequest,
  UpdateKnowledgeSourceRequest,
  UpdateTrustLevelRequest,
  SourceId,
} from '../types/sourceTypes';

/**
 * Sources Feature API Service.
 * Strongly-typed API client routing through the shared apiClient to consume ASP.NET Core Sources endpoints.
 */
export const sourceApi = {
  /**
   * Lists all knowledge sources, optionally filtered by source type, trust level, and approval status.
   * Endpoint: GET /api/sources
   */
  getSources(
    params?: GetSourcesQueryParams,
    options?: RequestOptions
  ): Promise<KnowledgeSourceDto[]> {
    return apiClient.get<KnowledgeSourceDto[]>('/api/sources', {
      ...options,
      params: params as Record<string, string | number | boolean | undefined | null>,
    });
  },

  /**
   * Retrieves a single knowledge source by its unique identifier.
   * Endpoint: GET /api/sources/{id}
   */
  getSourceById(id: SourceId, options?: RequestOptions): Promise<KnowledgeSourceDto> {
    return apiClient.get<KnowledgeSourceDto>(`/api/sources/${id}`, options);
  },

  /**
   * Retrieves all knowledge sources attached to a specific knowledge article.
   * Endpoint: GET /api/knowledge/articles/{articleId}/sources (or GET /api/sources/article/{articleId})
   */
  getSourcesByArticleId(
    articleId: string,
    options?: RequestOptions
  ): Promise<KnowledgeSourceDto[]> {
    return apiClient.get<KnowledgeSourceDto[]>(
      `/api/knowledge/articles/${articleId}/sources`,
      options
    );
  },

  /**
   * Attaches a knowledge source to a knowledge article with optional citation reference text.
   * Endpoint: POST /api/knowledge/articles/{articleId}/sources/{sourceId}
   */
  attachSourceToArticle(
    articleId: string,
    sourceId: SourceId,
    request?: AttachSourceRequest,
    options?: RequestOptions
  ): Promise<KnowledgeArticleDto> {
    return apiClient.post<KnowledgeArticleDto>(
      `/api/knowledge/articles/${articleId}/sources/${sourceId}`,
      request ?? {},
      options
    );
  },

  /**
   * Detaches a knowledge source from a knowledge article.
   * Endpoint: DELETE /api/knowledge/articles/{articleId}/sources/{sourceId}
   */
  detachSourceFromArticle(
    articleId: string,
    sourceId: SourceId,
    options?: RequestOptions
  ): Promise<KnowledgeArticleDto> {
    return apiClient.delete<KnowledgeArticleDto>(
      `/api/knowledge/articles/${articleId}/sources/${sourceId}`,
      options
    );
  },

  /**
   * Registers a new knowledge source.
   * Endpoint: POST /api/sources
   */
  createSource(
    request: CreateKnowledgeSourceRequest,
    options?: RequestOptions
  ): Promise<KnowledgeSourceDto> {
    return apiClient.post<KnowledgeSourceDto>('/api/sources', request, options);
  },

  /**
   * Updates an existing knowledge source's metadata.
   * Endpoint: PUT /api/sources/{id}
   */
  updateSource(
    id: SourceId,
    request: UpdateKnowledgeSourceRequest,
    options?: RequestOptions
  ): Promise<KnowledgeSourceDto> {
    return apiClient.put<KnowledgeSourceDto>(`/api/sources/${id}`, request, options);
  },

  /**
   * Approves a knowledge source for use in the engineering knowledge hub.
   * Endpoint: POST /api/sources/{id}/approve
   */
  approveSource(id: SourceId, options?: RequestOptions): Promise<KnowledgeSourceDto> {
    return apiClient.post<KnowledgeSourceDto>(`/api/sources/${id}/approve`, undefined, options);
  },

  /**
   * Revokes approval of an approved knowledge source.
   * Endpoint: POST /api/sources/{id}/revoke-approval
   */
  revokeSourceApproval(id: SourceId, options?: RequestOptions): Promise<KnowledgeSourceDto> {
    return apiClient.post<KnowledgeSourceDto>(
      `/api/sources/${id}/revoke-approval`,
      undefined,
      options
    );
  },

  /**
   * Updates the organizational trust level of a knowledge source.
   * Endpoint: PUT /api/sources/{id}/trust-level
   */
  updateTrustLevel(
    id: SourceId,
    request: UpdateTrustLevelRequest,
    options?: RequestOptions
  ): Promise<KnowledgeSourceDto> {
    return apiClient.put<KnowledgeSourceDto>(`/api/sources/${id}/trust-level`, request, options);
  },
};
