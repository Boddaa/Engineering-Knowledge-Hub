import { apiClient, type RequestOptions } from '@/services/api/apiClient';
import type { KnowledgeArticleDto } from '@/features/knowledge/types/knowledgeTypes';
import type {
  DocumentDto,
  DocumentChunkDto,
  UploadDocumentRequest,
  ProcessDocumentRequest,
  ChunkDocumentRequest,
  ProposeArticleFromDocumentRequest,
  DocumentId,
} from '../types/documentTypes';

/**
 * Documents Feature API Service.
 * Strongly-typed API client routing through the shared apiClient to consume ASP.NET Core Documents endpoints.
 */
export const documentApi = {
  /**
   * Registers/uploads a new document attached to a knowledge source.
   * Endpoint: POST /api/documents
   */
  uploadDocument(
    request: UploadDocumentRequest,
    options?: RequestOptions
  ): Promise<DocumentDto> {
    return apiClient.post<DocumentDto>('/api/documents', request, options);
  },

  /**
   * Retrieves a document by its unique identifier.
   * Endpoint: GET /api/documents/{id}
   */
  getDocumentById(id: DocumentId, options?: RequestOptions): Promise<DocumentDto> {
    return apiClient.get<DocumentDto>(`/api/documents/${id}`, options);
  },

  /**
   * Retrieves all documents registered under a specific knowledge source.
   * Endpoint: GET /api/documents/source/{sourceId}
   */
  getDocumentsBySourceId(
    sourceId: string,
    options?: RequestOptions
  ): Promise<DocumentDto[]> {
    return apiClient.get<DocumentDto[]>(`/api/documents/source/${sourceId}`, options);
  },

  /**
   * Triggers text extraction and processing on an uploaded document.
   * Endpoint: POST /api/documents/{id}/process
   */
  processDocument(
    id: DocumentId,
    request?: ProcessDocumentRequest,
    options?: RequestOptions
  ): Promise<DocumentDto> {
    return apiClient.post<DocumentDto>(
      `/api/documents/${id}/process`,
      request ?? {},
      options
    );
  },

  /**
   * Chunks a processed document into semantic chunks for vector indexing and search.
   * Endpoint: POST /api/documents/{id}/chunk
   */
  chunkDocument(
    id: DocumentId,
    request?: ChunkDocumentRequest,
    options?: RequestOptions
  ): Promise<DocumentChunkDto[]> {
    return apiClient.post<DocumentChunkDto[]>(
      `/api/documents/${id}/chunk`,
      request ?? {},
      options
    );
  },

  /**
   * Proposes a new draft knowledge article synthesized from an imported document.
   * Endpoint: POST /api/documents/{id}/propose-article
   */
  proposeArticle(
    id: DocumentId,
    request: ProposeArticleFromDocumentRequest,
    options?: RequestOptions
  ): Promise<KnowledgeArticleDto> {
    return apiClient.post<KnowledgeArticleDto>(
      `/api/documents/${id}/propose-article`,
      request,
      options
    );
  },
};
