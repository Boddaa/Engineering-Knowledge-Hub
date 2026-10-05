import { apiClient, type RequestOptions } from '@/services/api/apiClient';
import type { KnowledgeArticleDto } from '@/features/knowledge/types/knowledgeTypes';
import type {
  KnowledgeSourceDto,
  GetSourcesQueryParams,
  AttachSourceRequest,
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
};

