import React, { useState } from 'react';
import { useSearchParams, useNavigate } from 'react-router-dom';
import { RefreshCw, Plus, FolderSearch } from 'lucide-react';
import { PageShell } from '@/components/layout/PageShell';
import { Button } from '@/components/ui/Button';
import { ErrorState } from '@/components/ui/ErrorState';
import { SourceSelect } from '@/features/sources';
import {
  useDocuments,
  DocumentListTable,
  DocumentRegisterModal,
} from '@/features/documents';
import type { DocumentDto } from '@/features/documents';
import styles from './DocumentsPage.module.css';

export const DocumentsPage: React.FC = () => {
  const [searchParams, setSearchParams] = useSearchParams();
  const navigate = useNavigate();

  const selectedSourceId = searchParams.get('sourceId') || '';

  const {
    documents,
    isLoading,
    error,
    refetch,
  } = useDocuments(selectedSourceId);

  // Document registration modal state
  const [isRegisterModalOpen, setIsRegisterModalOpen] = useState(false);

  const handleSourceChange = (sourceId: string) => {
    if (sourceId) {
      setSearchParams({ sourceId });
    } else {
      setSearchParams({});
    }
  };

  const handleOpenRegisterModal = () => {
    setIsRegisterModalOpen(true);
  };

  const handleCloseRegisterModal = () => {
    setIsRegisterModalOpen(false);
  };

  const handleDocumentRegistered = (created: DocumentDto) => {
    setIsRegisterModalOpen(false);
    if (created.sourceId === selectedSourceId) {
      refetch();
    } else {
      setSearchParams({ sourceId: created.sourceId });
    }
  };

  return (
    <PageShell
      title="Documents"
      description="Discover, register, process, and chunk engineering documents partitioned by Knowledge Source."
      headerActions={
        <Button
          variant="primary"
          icon={<Plus size={16} />}
          onClick={handleOpenRegisterModal}
          aria-label="Register new document"
        >
          Register Document
        </Button>
      }
    >
      <div className={styles.container}>
        {/* Source Selector & Controls Bar */}
        <section className={styles.filterBar} aria-label="Document filters and actions">
          <div className={styles.sourceSelectWrapper}>
            <SourceSelect
              label="Filter by Knowledge Source"
              value={selectedSourceId}
              onChange={handleSourceChange}
              placeholder="-- Select a Knowledge Source to browse documents --"
              helperText="The backend partitions document discovery by source repository or origin."
            />
          </div>

          <div className={styles.actionButtons}>
            <Button
              variant="subtle"
              icon={<RefreshCw size={15} />}
              onClick={refetch}
              disabled={isLoading || !selectedSourceId}
              aria-label="Refresh documents list"
            >
              Refresh
            </Button>
          </div>
        </section>

        {/* Error State */}
        {error && (
          <ErrorState
            title="Failed to Load Documents"
            message={error}
            onRetry={refetch}
            actionText="Retry"
          />
        )}

        {/* Prompt when no source selected */}
        {!selectedSourceId && !error && (
          <div className={styles.emptyPromptCard}>
            <FolderSearch size={40} className={styles.emptyPromptIcon} aria-hidden="true" />
            <h2 className={styles.emptyPromptTitle}>Select a Knowledge Source</h2>
            <p className={styles.emptyPromptText}>
              Documents are indexed under specific knowledge sources. Select an approved or managed
              source from the dropdown above to discover and inspect its registered documents.
            </p>
          </div>
        )}

        {/* Document Table when source is selected */}
        {selectedSourceId && !error && (
          <div className={styles.contentCard}>
            <div className={styles.summaryRow}>
              <span className={styles.summaryText}>
                Showing <span className={styles.summaryHighlight}>{documents.length}</span>{' '}
                {documents.length === 1 ? 'document' : 'documents'} for selected source
              </span>
            </div>
            <DocumentListTable
              documents={documents}
              isLoading={isLoading}
              onOpenDocument={(docId) => navigate(`/documents/${docId}`)}
            />
          </div>
        )}

        {/* Register Document Modal */}
        <DocumentRegisterModal
          isOpen={isRegisterModalOpen}
          initialSourceId={selectedSourceId}
          onClose={handleCloseRegisterModal}
          onSuccess={handleDocumentRegistered}
        />
      </div>
    </PageShell>
  );
};
