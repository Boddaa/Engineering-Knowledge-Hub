/**
 * Strongly-typed domain and DTO contracts for the AI Knowledge Assistant and RAG workspace.
 * Mirrors ASP.NET Core backend models from:
 * - EngineeringKnowledgeHub.Api.Models.RagQueryRequest
 * - EngineeringKnowledgeHub.Application.DTOs.RagResponseDto
 * - EngineeringKnowledgeHub.Application.DTOs.RagEvidenceDto
 * - EngineeringKnowledgeHub.Application.Ai.LocalAiHealthDto
 * - EngineeringKnowledgeHub.Application.Ai.LocalAiHealthStatus
 * - EngineeringKnowledgeHub.Api.Models.AiGenerateRequest
 * - EngineeringKnowledgeHub.Application.Rag.AiResponse
 */

/**
 * Request payload for submitting a natural-language question to the RAG knowledge assistant.
 * Endpoint: POST /api/rag/ask or POST /api/rag/query
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
 * Traceable back to internal knowledge articles, document chunks, or resolved incidents.
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
 * Diagnostic payload reporting the operational readiness and model presence of the Local AI subsystem.
 * Endpoint: GET /api/ai/local/health or GET /api/ai/health
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
 * Request payload for direct AI text generation.
 * Endpoint: POST /api/ai/generate
 */
export interface AiGenerateRequest {
  userPrompt: string;
  systemPrompt?: string | null;
  temperature?: number | null;
  maxTokens?: number | null;
}

/**
 * Direct response from the configured AI provider.
 */
export interface AiResponse {
  content: string;
  modelName: string;
  promptTokens: number;
  completionTokens: number;
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
