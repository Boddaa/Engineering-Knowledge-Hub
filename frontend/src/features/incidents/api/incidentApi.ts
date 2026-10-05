import { apiClient, type RequestOptions } from '@/services/api/apiClient';
import type {
  IncidentDto,
  IncidentId,
  IncidentStatus,
  CreateIncidentRequest,
  ResolveIncidentRequest,
  LinkArticleRequest,
  CreateKnowledgeFromIncidentRequest,
  KnowledgeArticleDto,
} from '../types/incidentTypes';

/**
 * Incidents Feature API Client.
 * Routes directly through the centralized apiClient without duplicating network or error logic.
 * Source of truth: IncidentsController in ASP.NET Core API.
 */
export const incidentApi = {
  /**
   * Lists all incidents, optionally filtered by lifecycle status.
   * Endpoint: GET /api/incidents or GET /api/incidents?status={status}
   */
  getIncidents(status?: IncidentStatus, options?: RequestOptions): Promise<IncidentDto[]> {
    const query = status ? `?status=${encodeURIComponent(status)}` : '';
    return apiClient.get<IncidentDto[]>(`/api/incidents${query}`, options);
  },

  /**
   * Retrieves a single incident by its unique ID.
   * Endpoint: GET /api/incidents/{id}
   */
  getIncident(id: IncidentId, options?: RequestOptions): Promise<IncidentDto> {
    return apiClient.get<IncidentDto>(`/api/incidents/${id}`, options);
  },

  /**
   * Creates and records a new operational incident.
   * Endpoint: POST /api/incidents
   */
  createIncident(request: CreateIncidentRequest, options?: RequestOptions): Promise<IncidentDto> {
    return apiClient.post<IncidentDto>('/api/incidents', request, options);
  },

  /**
   * Starts investigation on an incident, transitioning status to Investigating.
   * Endpoint: POST /api/incidents/{id}/investigate
   */
  startInvestigation(id: IncidentId, options?: RequestOptions): Promise<IncidentDto> {
    return apiClient.post<IncidentDto>(`/api/incidents/${id}/investigate`, undefined, options);
  },

  /**
   * Resolves an active incident with resolution details and optional root cause analysis.
   * Endpoint: POST /api/incidents/{id}/resolve
   */
  resolveIncident(
    id: IncidentId,
    request: ResolveIncidentRequest,
    options?: RequestOptions
  ): Promise<IncidentDto> {
    return apiClient.post<IncidentDto>(`/api/incidents/${id}/resolve`, request, options);
  },

  /**
   * Closes a resolved incident.
   * Endpoint: POST /api/incidents/{id}/close
   */
  closeIncident(id: IncidentId, options?: RequestOptions): Promise<IncidentDto> {
    return apiClient.post<IncidentDto>(`/api/incidents/${id}/close`, undefined, options);
  },

  /**
   * Links a knowledge article to an incident with a specified relationship type.
   * Endpoint: POST /api/incidents/{id}/articles
   */
  linkArticle(
    id: IncidentId,
    request: LinkArticleRequest,
    options?: RequestOptions
  ): Promise<IncidentDto> {
    return apiClient.post<IncidentDto>(`/api/incidents/${id}/articles`, request, options);
  },

  /**
   * Removes a linked article association from an incident.
   * Endpoint: DELETE /api/incidents/{id}/articles/{articleId}
   */
  unlinkArticle(
    id: IncidentId,
    articleId: string,
    options?: RequestOptions
  ): Promise<IncidentDto> {
    return apiClient.delete<IncidentDto>(`/api/incidents/${id}/articles/${articleId}`, options);
  },

  /**
   * Proposes a draft knowledge article synthesized from a resolved or closed incident.
   * Endpoint: POST /api/incidents/{id}/propose-article
   */
  proposeArticle(
    id: IncidentId,
    request: CreateKnowledgeFromIncidentRequest,
    options?: RequestOptions
  ): Promise<KnowledgeArticleDto> {
    return apiClient.post<KnowledgeArticleDto>(`/api/incidents/${id}/propose-article`, request, options);
  },

  /**
   * Retrieves all incidents linked to a specific knowledge article.
   * Endpoint: GET /api/incidents/article/{articleId}
   */
  getIncidentsByArticle(articleId: string, options?: RequestOptions): Promise<IncidentDto[]> {
    return apiClient.get<IncidentDto[]>(`/api/incidents/article/${articleId}`, options);
  },
};
