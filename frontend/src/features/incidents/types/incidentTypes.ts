import type { KnowledgeArticleId, KnowledgeArticleDto } from '@/features/knowledge/types/knowledgeTypes';

/**
 * Strongly-typed Incident API Contracts matching the ASP.NET Core backend DTOs.
 * Source of truth:
 * - EngineeringKnowledgeHub.Application.DTOs.IncidentDto
 * - EngineeringKnowledgeHub.Application.DTOs.IncidentArticleLinkDto
 * - EngineeringKnowledgeHub.Domain.Enums.IncidentStatus
 * - EngineeringKnowledgeHub.Domain.Enums.IncidentSeverity
 * - EngineeringKnowledgeHub.Domain.Enums.ArticleRelationshipType
 * - EngineeringKnowledgeHub.Api.Models.IncidentRequests
 */

export type IncidentId = string;
export type UserId = string;

/**
 * Lifecycle status of an engineering incident.
 * Backend Domain enum: Open = 1, Investigating = 2, Resolved = 3, Closed = 4
 */
export type IncidentStatus = 'Open' | 'Investigating' | 'Resolved' | 'Closed';

/**
 * Operational severity rating for an engineering problem or incident.
 * Backend Domain enum: Low = 1, Medium = 2, High = 3, Critical = 4
 */
export type IncidentSeverity = 'Low' | 'Medium' | 'High' | 'Critical';

/**
 * Relationship link between an incident and a knowledge article.
 * Backend Domain enum: Supports = 1, Resolves = 2, RelatesTo = 3, CausedBy = 4
 */
export type ArticleRelationshipType = 'Supports' | 'Resolves' | 'RelatesTo' | 'CausedBy';

/**
 * Association link projecting an article connected to an incident.
 */
export interface IncidentArticleLinkDto {
  articleId: KnowledgeArticleId;
  relationshipType: ArticleRelationshipType | number;
}

/**
 * Data transfer object projecting operational incident state.
 */
export interface IncidentDto {
  id: IncidentId;
  title: string;
  problemDescription: string;
  symptoms: string;
  rootCause: string;
  resolution: string;
  severity: IncidentSeverity | number;
  status: IncidentStatus | number;
  createdById: UserId;
  resolvedById: UserId | null;
  createdAt: string;
  resolvedAt: string | null;
  articleLinks: IncidentArticleLinkDto[];
}

/**
 * HTTP request contract for reporting a new operational incident.
 */
export interface CreateIncidentRequest {
  title: string;
  problemDescription: string;
  severity: IncidentSeverity | number;
  createdById: UserId;
  symptoms?: string | null;
}

/**
 * HTTP request contract for resolving an ongoing incident.
 */
export interface ResolveIncidentRequest {
  resolvedById: UserId;
  resolution: string;
  rootCause?: string | null;
}

/**
 * HTTP request contract for linking a knowledge article to an incident.
 */
export interface LinkArticleRequest {
  articleId: KnowledgeArticleId;
  relationshipType: ArticleRelationshipType | number;
}

/**
 * HTTP request contract for proposing a draft knowledge article from a resolved incident.
 */
export interface CreateKnowledgeFromIncidentRequest {
  authorId: UserId;
  knowledgeTypeId: string;
  title?: string | null;
  summary?: string | null;
  customContent?: string | null;
  slug?: string | null;
}

export type { KnowledgeArticleDto };
