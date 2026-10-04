import { apiClient, type RequestOptions } from '@/services/api/apiClient';
import type {
  KnowledgeArticleDto,
  KnowledgeVersionDto,
  AuditLogDto,
  SearchArticleResultDto,
  PagedResult,
  CreateArticleRequest,
  UpdateArticleRequest,
  SubmitArticleReviewRequest,
  ApproveArticleRequest,
  RejectArticleRequest,
  DeprecateArticleRequest,
  ArchiveArticleRequest,
  SearchArticleQueryParams,
  KnowledgeArticleId,
  KnowledgeTagId,
  ProductId,
  TechnologyId,
  VersionId,
} from '../types/knowledgeTypes';

/**
 * Knowledge Feature API Service.
 * Thin, strongly-typed client consuming the ASP.NET Core Knowledge API endpoints.
 * Routes directly through the centralized apiClient without duplicating network or error logic.
 */
export const knowledgeApi = {
  // --- Article CRUD & Lifecycle Operations (/api/knowledge/articles) ---

  /**
   * Retrieves a knowledge article by its unique ID.
   * Endpoint: GET /api/knowledge/articles/{id}
   */
  getArticle(id: KnowledgeArticleId, options?: RequestOptions): Promise<KnowledgeArticleDto> {
    return apiClient.get<KnowledgeArticleDto>(`/api/knowledge/articles/${id}`, options);
  },

  /**
   * Creates a new knowledge article in Draft status.
   * Endpoint: POST /api/knowledge/articles
   */
  createArticle(request: CreateArticleRequest, options?: RequestOptions): Promise<KnowledgeArticleDto> {
    return apiClient.post<KnowledgeArticleDto>('/api/knowledge/articles', request, options);
  },

  /**
   * Updates an existing draft article's title, summary, content, or slug.
   * Endpoint: PUT /api/knowledge/articles/{id}
   */
  updateArticle(
    id: KnowledgeArticleId,
    request: UpdateArticleRequest,
    options?: RequestOptions
  ): Promise<KnowledgeArticleDto> {
    return apiClient.put<KnowledgeArticleDto>(`/api/knowledge/articles/${id}`, request, options);
  },

  /**
   * Submits a draft knowledge article for peer review.
   * Endpoint: POST /api/knowledge/articles/{id}/submit-review
   */
  submitForReview(
    id: KnowledgeArticleId,
    request?: SubmitArticleReviewRequest,
    options?: RequestOptions
  ): Promise<KnowledgeArticleDto> {
    return apiClient.post<KnowledgeArticleDto>(
      `/api/knowledge/articles/${id}/submit-review`,
      request ?? {},
      options
    );
  },

  /**
   * Approves an article in review, atomically publishing it and generating a version snapshot.
   * Endpoint: POST /api/knowledge/articles/{id}/approve
   */
  approveArticle(
    id: KnowledgeArticleId,
    request: ApproveArticleRequest,
    options?: RequestOptions
  ): Promise<KnowledgeArticleDto> {
    return apiClient.post<KnowledgeArticleDto>(`/api/knowledge/articles/${id}/approve`, request, options);
  },

  /**
   * Rejects an article in review, returning it to Draft status.
   * Endpoint: POST /api/knowledge/articles/{id}/reject
   */
  rejectArticle(
    id: KnowledgeArticleId,
    request: RejectArticleRequest,
    options?: RequestOptions
  ): Promise<KnowledgeArticleDto> {
    return apiClient.post<KnowledgeArticleDto>(`/api/knowledge/articles/${id}/reject`, request, options);
  },

  /**
   * Deprecates a published article.
   * Endpoint: POST /api/knowledge/articles/{id}/deprecate
   */
  deprecateArticle(
    id: KnowledgeArticleId,
    request?: DeprecateArticleRequest,
    options?: RequestOptions
  ): Promise<KnowledgeArticleDto> {
    return apiClient.post<KnowledgeArticleDto>(
      `/api/knowledge/articles/${id}/deprecate`,
      request ?? {},
      options
    );
  },

  /**
   * Archives a knowledge article.
   * Endpoint: POST /api/knowledge/articles/{id}/archive
   */
  archiveArticle(
    id: KnowledgeArticleId,
    request?: ArchiveArticleRequest,
    options?: RequestOptions
  ): Promise<KnowledgeArticleDto> {
    return apiClient.post<KnowledgeArticleDto>(
      `/api/knowledge/articles/${id}/archive`,
      request ?? {},
      options
    );
  },

  // --- Metadata Associations ---

  /**
   * Associates a tag with an article.
   * Endpoint: POST /api/knowledge/articles/{id}/tags/{tagId}
   */
  addTag(
    id: KnowledgeArticleId,
    tagId: KnowledgeTagId,
    options?: RequestOptions
  ): Promise<KnowledgeArticleDto> {
    return apiClient.post<KnowledgeArticleDto>(`/api/knowledge/articles/${id}/tags/${tagId}`, undefined, options);
  },

  /**
   * Removes a tag association from an article.
   * Endpoint: DELETE /api/knowledge/articles/{id}/tags/{tagId}
   */
  removeTag(
    id: KnowledgeArticleId,
    tagId: KnowledgeTagId,
    options?: RequestOptions
  ): Promise<KnowledgeArticleDto> {
    return apiClient.delete<KnowledgeArticleDto>(`/api/knowledge/articles/${id}/tags/${tagId}`, options);
  },

  /**
   * Associates a product with an article.
   * Endpoint: POST /api/knowledge/articles/{id}/products/{productId}
   */
  addProduct(
    id: KnowledgeArticleId,
    productId: ProductId,
    options?: RequestOptions
  ): Promise<KnowledgeArticleDto> {
    return apiClient.post<KnowledgeArticleDto>(
      `/api/knowledge/articles/${id}/products/${productId}`,
      undefined,
      options
    );
  },

  /**
   * Removes a product association from an article.
   * Endpoint: DELETE /api/knowledge/articles/{id}/products/{productId}
   */
  removeProduct(
    id: KnowledgeArticleId,
    productId: ProductId,
    options?: RequestOptions
  ): Promise<KnowledgeArticleDto> {
    return apiClient.delete<KnowledgeArticleDto>(
      `/api/knowledge/articles/${id}/products/${productId}`,
      options
    );
  },

  /**
   * Associates a technology with an article.
   * Endpoint: POST /api/knowledge/articles/{id}/technologies/{technologyId}
   */
  addTechnology(
    id: KnowledgeArticleId,
    technologyId: TechnologyId,
    options?: RequestOptions
  ): Promise<KnowledgeArticleDto> {
    return apiClient.post<KnowledgeArticleDto>(
      `/api/knowledge/articles/${id}/technologies/${technologyId}`,
      undefined,
      options
    );
  },

  /**
   * Removes a technology association from an article.
   * Endpoint: DELETE /api/knowledge/articles/{id}/technologies/{technologyId}
   */
  removeTechnology(
    id: KnowledgeArticleId,
    technologyId: TechnologyId,
    options?: RequestOptions
  ): Promise<KnowledgeArticleDto> {
    return apiClient.delete<KnowledgeArticleDto>(
      `/api/knowledge/articles/${id}/technologies/${technologyId}`,
      options
    );
  },

  // --- Governance & Version History (/api/knowledge/articles/{id}/versions) ---

  /**
   * Retrieves the immutable version history of a knowledge article.
   * Endpoint: GET /api/knowledge/articles/{articleId}/versions
   */
  getVersionHistory(
    articleId: KnowledgeArticleId,
    options?: RequestOptions
  ): Promise<KnowledgeVersionDto[]> {
    return apiClient.get<KnowledgeVersionDto[]>(`/api/knowledge/articles/${articleId}/versions`, options);
  },

  /**
   * Retrieves a specific historical snapshot by version number.
   * Endpoint: GET /api/knowledge/articles/{articleId}/versions/{versionNumber}
   */
  getVersionByNumber(
    articleId: KnowledgeArticleId,
    versionNumber: number,
    options?: RequestOptions
  ): Promise<KnowledgeVersionDto> {
    return apiClient.get<KnowledgeVersionDto>(
      `/api/knowledge/articles/${articleId}/versions/${versionNumber}`,
      options
    );
  },

  /**
   * Retrieves a historical version snapshot by its unique version ID.
   * Endpoint: GET /api/knowledge/versions/{versionId}
   */
  getVersionById(
    versionId: VersionId,
    options?: RequestOptions
  ): Promise<KnowledgeVersionDto> {
    return apiClient.get<KnowledgeVersionDto>(`/api/knowledge/versions/${versionId}`, options);
  },

  /**
   * Retrieves the governance audit trail for a knowledge article.
   * Endpoint: GET /api/knowledge/articles/{articleId}/audit
   */
  getArticleAuditTrail(
    articleId: KnowledgeArticleId,
    options?: RequestOptions
  ): Promise<AuditLogDto[]> {
    return apiClient.get<AuditLogDto[]>(`/api/knowledge/articles/${articleId}/audit`, options);
  },

  // --- Article Search & Listing (/api/search/articles) ---

  /**
   * Queries and filters knowledge articles with pagination and sorting.
   * Endpoint: GET /api/search/articles
   * Note: The backend exposes article listing through the Search service.
   */
  searchArticles(
    params?: SearchArticleQueryParams,
    options?: RequestOptions
  ): Promise<PagedResult<SearchArticleResultDto>> {
    return apiClient.get<PagedResult<SearchArticleResultDto>>('/api/search/articles', {
      ...options,
      params: params as Record<string, string | number | boolean | undefined | null>,
    });
  },
};
