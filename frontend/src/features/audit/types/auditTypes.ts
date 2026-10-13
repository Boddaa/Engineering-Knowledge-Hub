/**
 * F1 Phase 10: System Governance Audit & Compliance Observability Types.
 * Strictly adheres to backend AuditLogDto serialization contract.
 */

export interface AuditLogDto {
  /** System identifier of the audit log record (UUID) */
  id: string;
  /** Identifier of the user who initiated the action (UUID) */
  userId: string;
  /** Action performed, e.g. 'Created', 'Updated', 'Approved', 'Rejected', etc. */
  action: string;
  /** Name of the entity, e.g. 'KnowledgeArticle', 'KnowledgeVersion' */
  entityName: string;
  /** Target entity identifier (UUID) */
  entityId: string;
  /** Plain text narrative describing the event */
  details: string;
  /** UTC timestamp of the recorded action (ISO-8601 string) */
  createdAt: string;
}

export type AuditFilterMode = 'all' | 'entity' | 'user';

export interface AuditFilterState {
  mode: AuditFilterMode;
  entityName: string;
  entityId: string;
  userId: string;
  searchTerm: string;
}

export interface AuditPaginationState {
  page: number;
  pageSize: number;
}
