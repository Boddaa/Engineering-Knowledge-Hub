// API
export { documentApi } from './api/documentApi';

// Types
export type {
  DocumentDto,
  DocumentChunkDto,
  DocumentProcessingStatus,
  UploadDocumentRequest,
  ProcessDocumentRequest,
  ChunkDocumentRequest,
  ProposeArticleFromDocumentRequest,
} from './types/documentTypes';

// Utils
export {
  normalizeProcessingStatus,
  getProcessingStatusBadgeVariant,
  formatDocumentDate,
} from './utils/documentFormatters';

// Hooks
export { useDocuments } from './hooks/useDocuments';
export type { UseDocumentsResult } from './hooks/useDocuments';

export { useDocument } from './hooks/useDocument';
export type { UseDocumentResult } from './hooks/useDocument';

export { useDocumentActions } from './hooks/useDocumentActions';
export type { UseDocumentActionsResult } from './hooks/useDocumentActions';

// Components
export { DocumentListTable } from './components/DocumentListTable';
export type { DocumentListTableProps } from './components/DocumentListTable';

export { DocumentRegisterModal } from './components/DocumentRegisterModal';
export type { DocumentRegisterModalProps } from './components/DocumentRegisterModal';

export { DocumentProcessModal } from './components/DocumentProcessModal';
export type { DocumentProcessModalProps } from './components/DocumentProcessModal';

export { DocumentChunkModal } from './components/DocumentChunkModal';
export type { DocumentChunkModalProps } from './components/DocumentChunkModal';

export { DocumentChunksInspector } from './components/DocumentChunksInspector';
export type { DocumentChunksInspectorProps } from './components/DocumentChunksInspector';

export { DocumentProposeArticleModal } from './components/DocumentProposeArticleModal';
export type { DocumentProposeArticleModalProps } from './components/DocumentProposeArticleModal';
