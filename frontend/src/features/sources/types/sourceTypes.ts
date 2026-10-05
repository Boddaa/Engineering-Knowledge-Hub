/**
 * Strongly-typed Source API Contracts matching the ASP.NET Core backend DTOs.
 * Source of truth:
 * - EngineeringKnowledgeHub.Application.DTOs.KnowledgeSourceDto
 * - EngineeringKnowledgeHub.Domain.Enums.SourceType
 * - EngineeringKnowledgeHub.Domain.Enums.TrustLevel
 * - EngineeringKnowledgeHub.Api.Models.SourceRequests
 */

/**
 * Origin type for a knowledge source or supporting reference.
 * Domain enum:
 * Internal = 1, External = 2, Incident = 3, EngineerExperience = 4,
 * VendorDocumentation = 5, SystemGenerated = 6, AISuggested = 7
 */
export enum SourceType {
  Internal = 1,
  External = 2,
  Incident = 3,
  EngineerExperience = 4,
  VendorDocumentation = 5,
  SystemGenerated = 6,
  AISuggested = 7,
}

/**
 * Trust tier for knowledge sources to differentiate verified internal assets
 * from unvetted external sources.
 * Domain enum:
 * Untrusted = 1, Community = 2, Verified = 3, Official = 4
 */
export enum TrustLevel {
  Untrusted = 1,
  Community = 2,
  Verified = 3,
  Official = 4,
}

/**
 * Unique identifier alias for a knowledge source.
 */
export type SourceId = string;

/**
 * Knowledge Source projection returned by GET /api/sources and GET /api/sources/{id}.
 */
export interface KnowledgeSourceDto {
  id: SourceId;
  name: string;
  sourceType: SourceType | number;
  url: string | null;
  description: string | null;
  trustLevel: TrustLevel | number;
  isApproved: boolean;
  createdAt: string;
  updatedAt: string | null;
}

/**
 * Query parameters for filtering knowledge sources list.
 * Endpoint: GET /api/sources
 */
export interface GetSourcesQueryParams {
  sourceType?: SourceType | number;
  trustLevel?: TrustLevel | number;
  isApproved?: boolean;
}

/**
 * HTTP request contract for attaching a source to a knowledge article.
 * Endpoint: POST /api/knowledge/articles/{articleId}/sources/{sourceId}
 */
export interface AttachSourceRequest {
  referenceText?: string | null;
}

/**
 * HTTP request contract for registering a new knowledge source.
 * Endpoint: POST /api/sources
 */
export interface CreateKnowledgeSourceRequest {
  name: string;
  sourceType: SourceType | number;
  url?: string | null;
  description?: string | null;
  trustLevel?: TrustLevel | number;
}

/**
 * HTTP request contract for updating an existing knowledge source.
 * Endpoint: PUT /api/sources/{id}
 */
export interface UpdateKnowledgeSourceRequest {
  name: string;
  url?: string | null;
  description?: string | null;
}

/**
 * HTTP request contract for updating the trust level of a knowledge source.
 * Endpoint: PUT /api/sources/{id}/trust-level
 */
export interface UpdateTrustLevelRequest {
  trustLevel: TrustLevel | number;
}
