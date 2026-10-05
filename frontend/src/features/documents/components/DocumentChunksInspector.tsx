import React from 'react';
import { Layers } from 'lucide-react';
import type { DocumentChunkDto } from '../types/documentTypes';
import styles from './DocumentChunksInspector.module.css';

export interface DocumentChunksInspectorProps {
  chunks: DocumentChunkDto[];
  title?: string;
}

export const DocumentChunksInspector: React.FC<DocumentChunksInspectorProps> = ({
  chunks,
  title = 'Document Chunks',
}) => {
  return (
    <section className={styles.container} aria-labelledby="document-chunks-title">
      <div className={styles.header}>
        <div className={styles.titleRow}>
          <Layers size={18} className={styles.titleIcon} aria-hidden="true" />
          <h2 id="document-chunks-title" className={styles.title}>{title}</h2>
          <span className={styles.countBadge}>
            {chunks.length} {chunks.length === 1 ? 'chunk' : 'chunks'}
          </span>
        </div>
      </div>

      {chunks.length === 0 ? (
        <div className={styles.emptyBox}>
          <span className={styles.emptyTitle}>No chunks available</span>
          <p className={styles.emptyDesc}>
            Chunk this document using the action bar to generate and inspect text segments.
          </p>
        </div>
      ) : (
        <div className={styles.chunkList}>
          {chunks.map((chunk) => (
            <div
              key={chunk.id || `chunk-${chunk.chunkIndex}`}
              className={styles.chunkCard}
            >
              <div className={styles.chunkHeader}>
                <span className={styles.chunkIndex}>Chunk {chunk.chunkIndex}</span>
                <span className={styles.tokenBadge}>
                  {chunk.tokenCount.toLocaleString()} {chunk.tokenCount === 1 ? 'token' : 'tokens'}
                </span>
              </div>
              <pre className={styles.chunkContent}>{chunk.content}</pre>
            </div>
          ))}
        </div>
      )}
    </section>
  );
};
