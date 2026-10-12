/**
 * Strongly-typed domain and DTO contracts for the AI Knowledge Assistant and RAG workspace.
 * Reconciled against ASP.NET Core backend contracts:
 * - EngineeringKnowledgeHub.Api.Models.RagQueryRequest
 * - EngineeringKnowledgeHub.Application.DTOs.RagResponseDto
 * - EngineeringKnowledgeHub.Application.DTOs.RagEvidenceDto
 * - EngineeringKnowledgeHub.Application.Ai.LocalAiHealthDto
 * - EngineeringKnowledgeHub.Application.Ai.LocalAiHealthStatus
 */

/**
 * Request payload for submitting a question to the RAG knowledge assistant.
 * Endpoint: POST /api/rag/ask (or POST /api/rag/query)
 */
export interface RagQueryRequest {
  question: string;
  maxEvidence?: number | null;
  minRelevanceScore?: number | null;
  productId?: string | null;
  technologyId?: string | null;
  knowledgeTypeId?: string | null;
  tagId?: string | null;
  verificationStatus?: string | null;
  preferVerified?: boolean | null;
}

/**
 * Individual citation evidence item returned with a RAG response.
 * Provides provenance, verification status, and relevant excerpt.
 */
export interface RagEvidenceDto {
  citationNumber: number;
  sourceType: string;
  referenceId: string | null;
  secondaryId: string | null;
  title: string;
  excerpt: string;
  relevanceScore: number;
  verificationStatus: string | null;
  metadata: Record<string, string>;
}

/**
 * Synthesized response returned from the RAG knowledge assistant.
 * Note: Contract contains boolean isSufficientEvidence; no numeric confidenceScore is returned by backend.
 */
export interface RagResponseDto {
  question: string;
  answer: string;
  isSufficientEvidence: boolean;
  evidence: RagEvidenceDto[];
  modelUsed: string;
  answeredAt: string;
}

/**
 * Operational health status states for the Local AI subsystem.
 */
export type LocalAiHealthStatus =
  | 'Healthy'
  | 'Disabled'
  | 'Unavailable'
  | 'ModelUnavailable'
  | 'Misconfigured';

/**
 * Diagnostic payload reporting operational readiness and model presence of the Local AI subsystem.
 * Endpoint: GET /api/ai/local/health (or GET /api/ai/health)
 */
export interface LocalAiHealthDto {
  enabled: boolean;
  runtimeReachable: boolean;
  generationModelConfigured: string | null;
  generationModelAvailable: boolean;
  embeddingModelConfigured: string | null;
  embeddingModelAvailable: boolean;
  status: LocalAiHealthStatus;
  diagnosticMessage: string;
}

/**
 * In-memory session query history item for tracking queries during the current session.
 * Exclusively in memory — no persistent storage introduced.
 */
export interface SessionQueryItem {
  id: string;
  question: string;
  timestamp: string;
  response: RagResponseDto;
}
