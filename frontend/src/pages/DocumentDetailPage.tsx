import React, { useState } from 'react';
import { useParams, useNavigate, Link } from 'react-router-dom';
import {
  ArrowLeft,
  Calendar,
  User,
  Clock,
  Layers,
  Sparkles,
  BookOpen,
  RefreshCw,
  HardDrive,
  Hash,
  X,
  AlertTriangle,
} from 'lucide-react';
import { PageShell } from '@/components/layout/PageShell';
import { Button } from '@/components/ui/Button';
import { Badge } from '@/components/ui/Badge';
import { Skeleton } from '@/components/ui/Skeleton';
import { EmptyState } from '@/components/ui/EmptyState';
import { ErrorState } from '@/components/ui/ErrorState';
import {
  useDocument,
  useDocumentActions,
  DocumentProcessModal,
  DocumentChunkModal,
  DocumentProposeArticleModal,
  DocumentChunksInspector,
  normalizeProcessingStatus,
  getProcessingStatusBadgeVariant,
  formatDocumentDate,
} from '@/features/documents';
import type {
  ProposeArticleFromDocumentRequest,
} from '@/features/documents';
import type { KnowledgeArticleDto } from '@/features/knowledge';
import styles from './DocumentDetailPage.module.css';

export const DocumentDetailPage: React.FC = () => {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();

  const {
    document,
    isLoading,
    error,
    isNotFound,
    refetch,
  } = useDocument(id);

  const {
    isProcessing,
    isChunking,
    isProposing,
    actionError,
    chunks,
    processDocument,
    chunkDocument,
    proposeArticle,
    clearActionError,
  } = useDocumentActions(id);

  // Modals state
  const [isProcessModalOpen, setIsProcessModalOpen] = useState(false);
  const [isChunkModalOpen, setIsChunkModalOpen] = useState(false);
  const [isProposeModalOpen, setIsProposeModalOpen] = useState(false);

  if (isLoading) {
    return (
      <PageShell
        title="Loading document..."
        description="Retrieving document metadata and processing status."
      >
        <div className={styles.container}>
          <Skeleton width="140px" height="20px" />
          <Skeleton width="100%" height="180px" />
          <Skeleton width="100%" height="240px" />
        </div>
      </PageShell>
    );
  }

  if (isNotFound || !document) {
    return (
      <PageShell title="Document Not Found" description="The requested document does not exist.">
        <EmptyState
          icon={<AlertTriangle size={36} />}
          title="Document not found"
          description={`No registered document exists with ID "${id}".`}
          action={
            <Button
              variant="subtle"
              icon={<ArrowLeft size={16} />}
              onClick={() => navigate('/documents')}
            >
              Return to Documents
            </Button>
          }
        />
      </PageShell>
    );
  }

  if (error) {
    return (
      <PageShell title="Document Error" description="Unable to load document record.">
        <ErrorState
          title="Failed to load document"
          message={error}
          onRetry={refetch}
        />
      </PageShell>
    );
  }

  const normalizedStatus = normalizeProcessingStatus(document.processingStatus);
  const statusBadgeVariant = getProcessingStatusBadgeVariant(document.processingStatus);

  // Handlers for lifecycle actions
  const handleConfirmProcess = async (contentOverride?: string) => {
    clearActionError();
    const updated = await processDocument(contentOverride);
    if (updated) {
      refetch();
      setIsProcessModalOpen(false);
    }
  };

  const handleConfirmChunk = async (targetChunkSize?: number, overlapTokens?: number) => {
    clearActionError();
    const resultChunks = await chunkDocument(targetChunkSize, overlapTokens);
    if (resultChunks) {
      refetch();
      setIsChunkModalOpen(false);
    }
  };

  const handleConfirmPropose = async (
    request: ProposeArticleFromDocumentRequest
  ): Promise<KnowledgeArticleDto | null> => {
    clearActionError();
    return await proposeArticle(request);
  };

  return (
    <PageShell
      title={`Document: ${document.fileName}`}
      description={`Engineering document ${document.id}`}
    >
      <div className={styles.container}>
        {/* Navigation Bar */}
        <div className={styles.backBar}>
          <button
            type="button"
            className={styles.backLink}
            onClick={() => navigate(`/documents?sourceId=${document.sourceId}`)}
            aria-label="Back to documents list for this source"
          >
            <ArrowLeft size={16} />
            <span>Back to Source Documents</span>
          </button>
        </div>

        {/* Action Error Banner */}
        {actionError && (
          <div className={styles.actionErrorBanner} role="alert">
            <AlertTriangle size={16} />
            <span>{actionError}</span>
            <button
              type="button"
              className={styles.bannerClose}
              onClick={clearActionError}
              aria-label="Dismiss error"
            >
              <X size={14} />
            </button>
          </div>
        )}

        {/* Lifecycle Actions Toolbar */}
        <div className={styles.actionBar} role="toolbar" aria-label="Document processing actions">
          <Button
            variant="primary"
            size="sm"
            icon={<Sparkles size={14} />}
            onClick={() => {
              clearActionError();
              setIsProcessModalOpen(true);
            }}
            disabled={isProcessing}
            isLoading={isProcessing}
            aria-label="Process document"
          >
            {normalizedStatus === 'Processed' ? 'Re-process Document' : 'Process Document'}
          </Button>

          <Button
            variant="subtle"
            size="sm"
            icon={<Layers size={14} />}
            onClick={() => {
              clearActionError();
              setIsChunkModalOpen(true);
            }}
            disabled={isChunking}
            isLoading={isChunking}
            aria-label="Chunk document"
          >
            Chunk Document
          </Button>

          <Button
            variant="subtle"
            size="sm"
            icon={<BookOpen size={14} />}
            onClick={() => {
              clearActionError();
              setIsProposeModalOpen(true);
            }}
            disabled={isProposing}
            aria-label="Propose Knowledge Article from this document"
          >
            Propose Knowledge Article
          </Button>

          <Button
            variant="ghost"
            size="sm"
            icon={<RefreshCw size={14} />}
            onClick={refetch}
            disabled={isLoading || isProcessing}
            aria-label="Refresh document details"
          >
            Refresh
          </Button>
        </div>

        {/* Overview Header Card */}
        <section className={styles.overviewCard} aria-label="Document overview">
          <div className={styles.titleRow}>
            <h1 className={styles.documentTitle}>{document.fileName}</h1>
            <div className={styles.badgeGroup}>
              <Badge variant={statusBadgeVariant} dot>
                {normalizedStatus}
              </Badge>
              <Badge variant="default">
                {document.contentType}
              </Badge>
            </div>
          </div>

          <div className={styles.metaGrid}>
            <div className={styles.metaItem}>
              <span className={styles.metaLabel}>Source</span>
              <span className={styles.metaValue}>
                <Link
                  to={`/documents?sourceId=${document.sourceId}`}
                  className={styles.linkValue}
                  title="Filter documents by this source"
                >
                  <span className={styles.mono}>{document.sourceId.substring(0, 8)}...</span>
                </Link>
              </span>
            </div>

            <div className={styles.metaItem}>
              <span className={styles.metaLabel}>Storage Path</span>
              <span className={styles.metaValue} title={document.storagePath || '—'}>
                <HardDrive size={13} aria-hidden="true" />
                <span className={styles.mono}>{document.storagePath || '—'}</span>
              </span>
            </div>

            <div className={styles.metaItem}>
              <span className={styles.metaLabel}>Created At</span>
              <span className={styles.metaValue}>
                <Calendar size={13} aria-hidden="true" />
                {formatDocumentDate(document.createdAt)}
              </span>
            </div>

            {document.processedAt && (
              <div className={styles.metaItem}>
                <span className={styles.metaLabel}>Processed At</span>
                <span className={styles.metaValue}>
                  <Clock size={13} aria-hidden="true" />
                  {formatDocumentDate(document.processedAt)}
                </span>
              </div>
            )}

            {document.uploadedById && (
              <div className={styles.metaItem}>
                <span className={styles.metaLabel}>Uploaded By</span>
                <span className={styles.metaValue} title={document.uploadedById}>
                  <User size={13} aria-hidden="true" />
                  <span className={styles.mono}>{document.uploadedById.substring(0, 8)}...</span>
                </span>
              </div>
            )}

            {document.fileHash && (
              <div className={styles.metaItem}>
                <span className={styles.metaLabel}>File Hash</span>
                <span className={styles.metaValue} title={document.fileHash}>
                  <Hash size={13} aria-hidden="true" />
                  <span className={styles.mono}>{document.fileHash.substring(0, 12)}...</span>
                </span>
              </div>
            )}
          </div>

          {/* Failure reason if present */}
          {document.failureReason && (
            <div className={styles.errorMessageCard} role="alert">
              <h2 className={styles.errorMessageTitle}>Processing Error Notice</h2>
              <p className={styles.errorMessageText}>{document.failureReason}</p>
            </div>
          )}
        </section>

        {/* Extracted Text Inspector */}
        <section className={styles.extractedTextSection} aria-label="Extracted document text">
          <div className={styles.sectionHeader}>
            <h2 className={styles.sectionTitle}>Extracted Text Content</h2>
            {document.extractedText && (
              <span className={styles.sectionBadge}>
                {document.extractedText.length.toLocaleString()} characters
              </span>
            )}
          </div>

          {document.extractedText ? (
            <pre className={styles.textViewer}>{document.extractedText}</pre>
          ) : (
            <p className={styles.emptyText}>
              No text content currently extracted for this document. Use &quot;Process Document&quot;
              above to extract readable text.
            </p>
          )}
        </section>

        {/* Chunks Inspector */}
        {chunks.length > 0 && (
          <div className={styles.chunksSection}>
            <DocumentChunksInspector chunks={chunks} />
          </div>
        )}

        {/* Process Modal */}
        <DocumentProcessModal
          isOpen={isProcessModalOpen}
          documentFileName={document.fileName}
          isSubmitting={isProcessing}
          apiError={actionError}
          onClose={() => setIsProcessModalOpen(false)}
          onConfirm={handleConfirmProcess}
        />

        {/* Chunk Modal */}
        <DocumentChunkModal
          isOpen={isChunkModalOpen}
          documentFileName={document.fileName}
          isSubmitting={isChunking}
          apiError={actionError}
          onClose={() => setIsChunkModalOpen(false)}
          onConfirm={handleConfirmChunk}
        />

        {/* Propose Knowledge Article Modal */}
        <DocumentProposeArticleModal
          isOpen={isProposeModalOpen}
          document={document}
          isSubmitting={isProposing}
          apiError={actionError}
          onClose={() => setIsProposeModalOpen(false)}
          onConfirm={handleConfirmPropose}
        />
      </div>
    </PageShell>
  );
};
