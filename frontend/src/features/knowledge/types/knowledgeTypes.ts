/**
 * Strongly-typed Knowledge API Contracts matching the ASP.NET Core backend DTOs.
 * Source of truth:
 * - EngineeringKnowledgeHub.Application.DTOs.KnowledgeArticleDto
 * - EngineeringKnowledgeHub.Application.DTOs.ArticleSourceLinkDto
 * - EngineeringKnowledgeHub.Application.DTOs.KnowledgeVersionDto
 * - EngineeringKnowledgeHub.Application.DTOs.AuditLogDto
 * - EngineeringKnowledgeHub.Application.DTOs.SearchArticleResultDto
 * - EngineeringKnowledgeHub.Application.Search.PagedResult<T>
 * - EngineeringKnowledgeHub.Api.Models.ArticleRequests
 */

// Strongly-typed ID aliases
export type KnowledgeArticleId = string;
export type KnowledgeTypeId = string;
export type KnowledgeTagId = string;
export type ProductId = string;
export type TechnologyId = string;
export type SourceId = string;
export type UserId = string;
export type VersionId = string;

/**
 * Editorial and publication state of a knowledge article.
 * Domain enum: Draft = 1, PendingReview = 2, Verified = 3, Published = 4, Deprecated = 5, Archived = 6
 */
export type KnowledgeStatus =
  | 'Draft'
  | 'PendingReview'
  | 'Verified'
  | 'Published'
  | 'Deprecated'
  | 'Archived';

/**
 * Human-validation trust state of a knowledge article.
 * Domain enum: Unverified = 1, Verified = 2, Rejected = 3
 */
export type VerificationStatus =
  | 'Unverified'
  | 'Verified'
  | 'Rejected';

/**
 * Safe sorting dimensions for search and filtering.
 * Domain enum: Relevance = 0, RecentlyUpdated = 1, RecentlyCreated = 2, TitleAsc = 3, TitleDesc = 4
 */
export type SearchSortOption =
  | 'Relevance'
  | 'RecentlyUpdated'
  | 'RecentlyCreated'
  | 'TitleAsc'
  | 'TitleDesc';

/**
 * Source citation attached to a knowledge article.
 */
export interface ArticleSourceLinkDto {
  sourceId: SourceId;
  referenceText: string | null;
}

/**
 * Core Knowledge Article DTO returned by the API.
 */
export interface KnowledgeArticleDto {
  id: KnowledgeArticleId;
  title: string;
  slug: string;
  summary: string;
  content: string;
  knowledgeTypeId: KnowledgeTypeId;
  status: KnowledgeStatus | number;
  verificationStatus: VerificationStatus | number;
  authorId: UserId;
  reviewerId: UserId | null;
  createdAt: string;
  updatedAt: string | null;
  publishedAt: string | null;
  lastReviewedAt: string | null;
  tagIds: KnowledgeTagId[];
  productIds: ProductId[];
  technologyIds: TechnologyId[];
  sourceIds: SourceId[];
  sourceLinks?: ArticleSourceLinkDto[] | null;
}

/**
 * Historical snapshot of an article created upon publication or revision.
 */
export interface KnowledgeVersionDto {
  id: VersionId;
  articleId: KnowledgeArticleId;
  versionNumber: number;
  title: string;
  content: string;
  changedById: UserId;
  changeSummary: string;
  createdAt: string;
}

/**
 * Governance audit trail entry for tracking state transitions and mutations.
 */
export interface AuditLogDto {
  id: string;
  userId: UserId;
  action: string;
  entityName: string;
  entityId: string;
  details: string;
  createdAt: string;
}

/**
 * Result projection for article search and query listing.
 */
export interface SearchArticleResultDto {
  id: KnowledgeArticleId;
  title: string;
  slug: string;
  summary: string;
  excerpt: string;
  knowledgeTypeId: KnowledgeTypeId;
  status: KnowledgeStatus | number;
  verificationStatus: VerificationStatus | number;
  productIds: ProductId[];
  technologyIds: TechnologyId[];
  tagIds: KnowledgeTagId[];
  createdAt: string;
  updatedAt: string | null;
  publishedAt: string | null;
  keywordScore?: number | null;
  semanticScore?: number | null;
  hybridScore?: number | null;
}

/**
 * Bounded pagination container matching Application.Search.PagedResult<T>.
 */
export interface PagedResult<T> {
  items: T[];
  page: number;
  pageSize: number;
  totalCount: number;
  totalPages?: number;
  hasPreviousPage?: boolean;
  hasNextPage?: boolean;
}

// Request DTOs

export interface CreateArticleRequest {
  title: string;
  summary: string;
  content: string;
  knowledgeTypeId: KnowledgeTypeId;
  authorId: UserId;
  slug?: string | null;
}

export interface UpdateArticleRequest {
  title: string;
  summary: string;
  content: string;
  slug?: string | null;
}

export interface SubmitArticleReviewRequest {
  submitterId?: UserId | null;
}

export interface ApproveArticleRequest {
  reviewerId: UserId;
  changeSummary?: string | null;
}

export interface RejectArticleRequest {
  reviewerId: UserId;
}

export interface DeprecateArticleRequest {
  actorId?: UserId | null;
}

export interface ArchiveArticleRequest {
  actorId?: UserId | null;
}

export interface SearchArticleQueryParams {
  query?: string;
  productId?: ProductId;
  technologyId?: TechnologyId;
  knowledgeTypeId?: KnowledgeTypeId;
  tagId?: KnowledgeTagId;
  status?: KnowledgeStatus | number;
  verificationStatus?: VerificationStatus | number;
  sortBy?: SearchSortOption | number;
  page?: number;
  pageSize?: number;
}
