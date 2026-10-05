/**
 * Strongly-typed Document contracts matching the ASP.NET Core backend DTOs.
 * Source of truth:
 * - EngineeringKnowledgeHub.Application.DTOs.DocumentDto
 * - EngineeringKnowledgeHub.Application.DTOs.DocumentChunkDto
 * - EngineeringKnowledgeHub.Domain.Enums.DocumentProcessingStatus
 * - EngineeringKnowledgeHub.Api.Models.DocumentRequests
 */

import type { KnowledgeArticleDto } from '@/features/knowledge/types/knowledgeTypes';

/**
 * Status of an imported document through its ingestion and extraction lifecycle.
 * Domain enum:
 * Pending = 1, Processing = 2, Processed = 3, Failed = 4
 */
export enum DocumentProcessingStatus {
  Pending = 1,
  Processing = 2,
  Processed = 3,
  Failed = 4,
}

export type DocumentId = string;
export type DocumentChunkId = string;

/**
 * Data transfer object representing a registered engineering document.
 * Endpoint: GET /api/documents/{id}, GET /api/documents/source/{sourceId}
 */
export interface DocumentDto {
  id: DocumentId;
  fileName: string;
  storagePath: string;
  contentType: string;
  fileHash: string;
  sourceId: string;
  processingStatus: DocumentProcessingStatus | number;
  uploadedById: string;
  createdAt: string;
  processedAt: string | null;
  failureReason: string | null;
  extractedText: string | null;
}

/**
 * Semantic chunk generated from a processed document.
 * Endpoint: POST /api/documents/{id}/chunk
 */
export interface DocumentChunkDto {
  id: DocumentChunkId;
  documentId: DocumentId;
  chunkIndex: number;
  content: string;
  tokenCount: number;
  embeddingReference: string | null;
  createdAt: string;
}

/**
 * HTTP request contract for registering/uploading a new document.
 * Endpoint: POST /api/documents
 */
export interface UploadDocumentRequest {
  fileName: string;
  contentType: string;
  sourceId: string;
  uploadedById: string;
  content: string;
  storagePath?: string | null;
}

/**
 * HTTP request contract for triggering text extraction and processing.
 * Endpoint: POST /api/documents/{id}/process
 */
export interface ProcessDocumentRequest {
  content?: string | null;
}

/**
 * HTTP request contract for chunking a processed document.
 * Endpoint: POST /api/documents/{id}/chunk
 */
export interface ChunkDocumentRequest {
  targetChunkSize?: number | null;
  overlapTokens?: number | null;
}

/**
 * HTTP request contract for proposing a draft knowledge article synthesized from a document.
 * Endpoint: POST /api/documents/{id}/propose-article
 */
export interface ProposeArticleFromDocumentRequest {
  title: string;
  authorId: string;
  knowledgeTypeId: string;
  summary?: string | null;
  content?: string | null;
  referenceText?: string | null;
  productIds?: string[] | null;
  technologyIds?: string[] | null;
  tagIds?: string[] | null;
}

export type { KnowledgeArticleDto };
