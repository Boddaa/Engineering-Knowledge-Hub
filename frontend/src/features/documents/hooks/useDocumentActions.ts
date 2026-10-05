import { useState, useCallback } from 'react';
import { documentApi } from '../api/documentApi';
import type {
  DocumentId,
  DocumentDto,
  DocumentChunkDto,
  ProposeArticleFromDocumentRequest,
  KnowledgeArticleDto,
} from '../types/documentTypes';
import { ApiError } from '@/services/api/apiErrors';

export interface UseDocumentActionsResult {
  isProcessing: boolean;
  isChunking: boolean;
  isProposing: boolean;
  actionError: string | null;
  chunks: DocumentChunkDto[];
  lastProposedArticle: KnowledgeArticleDto | null;
  processDocument: (contentOverride?: string) => Promise<DocumentDto | null>;
  chunkDocument: (targetChunkSize?: number, overlapTokens?: number) => Promise<DocumentChunkDto[] | null>;
  proposeArticle: (request: ProposeArticleFromDocumentRequest) => Promise<KnowledgeArticleDto | null>;
  clearActionError: () => void;
  clearProposedArticle: () => void;
}

/**
 * Hook for executing document lifecycle mutations (process, chunk, propose article).
 * Manages operational states, errors, and chunk/article artifacts.
 */
export function useDocumentActions(documentId: DocumentId | undefined): UseDocumentActionsResult {
  const [isProcessing, setIsProcessing] = useState<boolean>(false);
  const [isChunking, setIsChunking] = useState<boolean>(false);
  const [isProposing, setIsProposing] = useState<boolean>(false);
  const [actionError, setActionError] = useState<string | null>(null);

  const [chunks, setChunks] = useState<DocumentChunkDto[]>([]);
  const [lastProposedArticle, setLastProposedArticle] = useState<KnowledgeArticleDto | null>(null);

  const clearActionError = useCallback(() => {
    setActionError(null);
  }, []);

  const clearProposedArticle = useCallback(() => {
    setLastProposedArticle(null);
  }, []);

  const processDocument = useCallback(
    async (contentOverride?: string): Promise<DocumentDto | null> => {
      if (!documentId) return null;

      setIsProcessing(true);
      setActionError(null);

      try {
        const result = await documentApi.processDocument(documentId, {
          content: contentOverride?.trim() || null,
        });
        return result;
      } catch (err: unknown) {
        if (err instanceof ApiError) {
          setActionError(err.message);
        } else {
          setActionError('Failed to process document.');
        }
        return null;
      } finally {
        setIsProcessing(false);
      }
    },
    [documentId]
  );

  const chunkDocument = useCallback(
    async (targetChunkSize?: number, overlapTokens?: number): Promise<DocumentChunkDto[] | null> => {
      if (!documentId) return null;

      setIsChunking(true);
      setActionError(null);

      try {
        const result = await documentApi.chunkDocument(documentId, {
          targetChunkSize: targetChunkSize ?? null,
          overlapTokens: overlapTokens ?? null,
        });
        setChunks(result);
        return result;
      } catch (err: unknown) {
        if (err instanceof ApiError) {
          setActionError(err.message);
        } else {
          setActionError('Failed to chunk document.');
        }
        return null;
      } finally {
        setIsChunking(false);
      }
    },
    [documentId]
  );

  const proposeArticle = useCallback(
    async (request: ProposeArticleFromDocumentRequest): Promise<KnowledgeArticleDto | null> => {
      if (!documentId) return null;

      setIsProposing(true);
      setActionError(null);

      try {
        const result = await documentApi.proposeArticle(documentId, request);
        setLastProposedArticle(result);
        return result;
      } catch (err: unknown) {
        if (err instanceof ApiError) {
          setActionError(err.message);
        } else {
          setActionError('Failed to propose knowledge article from document.');
        }
        return null;
      } finally {
        setIsProposing(false);
      }
    },
    [documentId]
  );

  return {
    isProcessing,
    isChunking,
    isProposing,
    actionError,
    chunks,
    lastProposedArticle,
    processDocument,
    chunkDocument,
    proposeArticle,
    clearActionError,
    clearProposedArticle,
  };
}
