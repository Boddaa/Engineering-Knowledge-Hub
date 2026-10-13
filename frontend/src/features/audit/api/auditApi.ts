import { apiClient, type RequestOptions } from '@/services/api/apiClient';
import type { AuditLogDto } from '../types/auditTypes';

/**
 * Audit Feature API Service.
 * Consumes ASP.NET Core AuditController endpoints.
 */
export const auditApi = {
  /**
   * Retrieves all system audit logs in descending chronological order.
   * Endpoint: GET /api/audit
   */
  getAll(options?: RequestOptions): Promise<AuditLogDto[]> {
    return apiClient.get<AuditLogDto[]>('/api/audit', options);
  },

  /**
   * Retrieves audit logs associated with a specific entity type and entity ID.
   * Endpoint: GET /api/audit/entities/{entityName}/{entityId:guid}
   */
  getByEntity(
    entityName: string,
    entityId: string,
    options?: RequestOptions
  ): Promise<AuditLogDto[]> {
    const encodedEntity = encodeURIComponent(entityName.trim());
    return apiClient.get<AuditLogDto[]>(
      `/api/audit/entities/${encodedEntity}/${entityId.trim()}`,
      options
    );
  },

  /**
   * Retrieves audit logs recorded for a specific user ID.
   * Endpoint: GET /api/audit/users/{userId:guid}
   */
  getByUser(userId: string, options?: RequestOptions): Promise<AuditLogDto[]> {
    return apiClient.get<AuditLogDto[]>(
      `/api/audit/users/${userId.trim()}`,
      options
    );
  },
};
