import React from 'react';
import { FileText, ArrowRight, Inbox } from 'lucide-react';
import { Badge } from '@/components/ui/Badge';
import { Button } from '@/components/ui/Button';
import { Skeleton } from '@/components/ui/Skeleton';
import type { DocumentDto } from '../types/documentTypes';
import {
  normalizeProcessingStatus,
  getProcessingStatusBadgeVariant,
  formatDocumentDate,
} from '../utils/documentFormatters';
import styles from './DocumentListTable.module.css';

export interface DocumentListTableProps {
  documents: DocumentDto[];
  isLoading: boolean;
  onOpenDocument: (documentId: string) => void;
}

export const DocumentListTable: React.FC<DocumentListTableProps> = ({
  documents,
  isLoading,
  onOpenDocument,
}) => {
  if (isLoading) {
    return (
      <div className={styles.tableContainer} aria-busy="true" aria-label="Loading engineering documents">
        <table className={styles.table}>
          <thead>
            <tr>
              <th>File Name</th>
              <th>Content Type</th>
              <th>Status</th>
              <th>Created</th>
              <th>Processed</th>
              <th>Action</th>
            </tr>
          </thead>
          <tbody>
            {[1, 2, 3, 4].map((i) => (
              <tr key={i}>
                <td><Skeleton variant="text" width={180} height={16} /></td>
                <td><Skeleton variant="text" width={80} height={16} /></td>
                <td><Skeleton variant="rectangular" width={75} height={22} /></td>
                <td><Skeleton variant="text" width={120} height={16} /></td>
                <td><Skeleton variant="text" width={120} height={16} /></td>
                <td><Skeleton variant="rectangular" width={60} height={28} /></td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>
    );
  }

  if (documents.length === 0) {
    return (
      <div className={styles.tableContainer}>
        <div className={styles.emptyContainer}>
          <Inbox size={32} />
          <span className={styles.emptyTitle}>No Documents Found</span>
          <p className={styles.emptyDescription}>
            There are no engineering documents registered under this knowledge source yet.
            Register a document to begin ingestion, text extraction, and knowledge proposal.
          </p>
        </div>
      </div>
    );
  }

  return (
    <div className={styles.tableContainer}>
      <table className={styles.table} aria-label="Engineering documents list">
        <thead>
          <tr>
            <th>File Name</th>
            <th>Content Type</th>
            <th>Status</th>
            <th>Created</th>
            <th>Processed</th>
            <th>Action</th>
          </tr>
        </thead>
        <tbody>
          {documents.map((doc) => {
            const statusLabel = normalizeProcessingStatus(doc.processingStatus);
            const badgeVariant = getProcessingStatusBadgeVariant(doc.processingStatus);

            return (
              <tr key={doc.id}>
                <td>
                  <div className={styles.fileCell}>
                    <FileText size={16} className={styles.fileIcon} aria-hidden="true" />
                    <button
                      type="button"
                      className={styles.fileName}
                      onClick={() => onOpenDocument(doc.id)}
                      title={`Open document ${doc.fileName}`}
                    >
                      {doc.fileName}
                    </button>
                  </div>
                </td>
                <td>
                  <span className={styles.contentTypeTag}>{doc.contentType}</span>
                </td>
                <td>
                  <Badge variant={badgeVariant} size="sm">
                    {statusLabel}
                  </Badge>
                </td>
                <td>{formatDocumentDate(doc.createdAt)}</td>
                <td>{formatDocumentDate(doc.processedAt)}</td>
                <td>
                  <Button
                    variant="subtle"
                    size="sm"
                    icon={<ArrowRight size={13} />}
                    onClick={() => onOpenDocument(doc.id)}
                    aria-label={`Open details for ${doc.fileName}`}
                  >
                    Open
                  </Button>
                </td>
              </tr>
            );
          })}
        </tbody>
      </table>
    </div>
  );
};
