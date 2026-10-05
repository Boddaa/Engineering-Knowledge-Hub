import React, { useState, useEffect } from 'react';
import { X, Layers, AlertCircle } from 'lucide-react';
import { Button } from '@/components/ui/Button';
import { Input } from '@/components/ui/Input';
import styles from './DocumentChunkModal.module.css';

export interface DocumentChunkModalProps {
  isOpen: boolean;
  documentFileName: string;
  isSubmitting: boolean;
  apiError: string | null;
  onClose: () => void;
  onConfirm: (targetChunkSize?: number, overlapTokens?: number) => void;
}

export const DocumentChunkModal: React.FC<DocumentChunkModalProps> = ({
  isOpen,
  documentFileName,
  isSubmitting,
  apiError,
  onClose,
  onConfirm,
}) => {
  const [targetChunkSize, setTargetChunkSize] = useState<string>('500');
  const [overlapTokens, setOverlapTokens] = useState<string>('50');
  const [validationError, setValidationError] = useState<string | null>(null);

  useEffect(() => {
    if (isOpen) {
      setTargetChunkSize('500');
      setOverlapTokens('50');
      setValidationError(null);
    }
  }, [isOpen]);

  useEffect(() => {
    const handleKeyDown = (e: KeyboardEvent) => {
      if (e.key === 'Escape' && isOpen && !isSubmitting) {
        onClose();
      }
    };
    window.addEventListener('keydown', handleKeyDown);
    return () => window.removeEventListener('keydown', handleKeyDown);
  }, [isOpen, isSubmitting, onClose]);

  if (!isOpen) return null;

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();

    const chunkSizeNum = parseInt(targetChunkSize, 10);
    if (isNaN(chunkSizeNum) || chunkSizeNum < 50 || chunkSizeNum > 4000) {
      setValidationError('Target chunk size must be a number between 50 and 4000 tokens.');
      return;
    }

    const overlapNum = parseInt(overlapTokens, 10);
    if (isNaN(overlapNum) || overlapNum < 0 || overlapNum >= chunkSizeNum) {
      setValidationError('Overlap tokens must be a number between 0 and less than chunk size.');
      return;
    }

    setValidationError(null);
    onConfirm(chunkSizeNum, overlapNum);
  };

  return (
    <div
      className={styles.backdrop}
      role="dialog"
      aria-modal="true"
      aria-labelledby="chunk-doc-title"
      onClick={onClose}
    >
      <div className={styles.modal} onClick={(e) => e.stopPropagation()}>
        <div className={styles.header}>
          <div>
            <h2 id="chunk-doc-title" className={styles.headerTitle}>
              <Layers size={18} className={styles.headerIcon} aria-hidden="true" />
              Chunk Document
            </h2>
            <p className={styles.headerSubtitle}>
              Partition &ldquo;{documentFileName}&rdquo; into semantic segments for indexing.
            </p>
          </div>
          <button
            type="button"
            className={styles.closeButton}
            onClick={onClose}
            aria-label="Close chunk document dialog"
            disabled={isSubmitting}
          >
            <X size={18} />
          </button>
        </div>

        <form onSubmit={handleSubmit}>
          <div className={styles.body}>
            {(validationError || apiError) && (
              <div className={styles.errorBanner} role="alert">
                <AlertCircle size={16} />
                <span>{validationError || apiError}</span>
              </div>
            )}

            <div className={styles.infoBox}>
              <span>
                Document chunking breaks the extracted text into overlapping token windows suitable
                for retrieval and semantic indexing. Default values are calibrated for technical documents.
              </span>
            </div>

            <div className={styles.gridTwoCol}>
              <Input
                id="chunk-target-size"
                label="Target Chunk Size (Tokens) *"
                type="number"
                min={50}
                max={4000}
                value={targetChunkSize}
                onChange={(e) => {
                  setTargetChunkSize(e.target.value);
                  setValidationError(null);
                }}
                disabled={isSubmitting}
                required
                helperText="Approximate tokens per chunk."
              />

              <Input
                id="chunk-overlap"
                label="Overlap Tokens *"
                type="number"
                min={0}
                max={1000}
                value={overlapTokens}
                onChange={(e) => {
                  setOverlapTokens(e.target.value);
                  setValidationError(null);
                }}
                disabled={isSubmitting}
                required
                helperText="Sliding window token overlap."
              />
            </div>
          </div>

          <div className={styles.footer}>
            <Button
              type="button"
              variant="subtle"
              size="md"
              onClick={onClose}
              disabled={isSubmitting}
            >
              Cancel
            </Button>
            <Button
              type="submit"
              variant="primary"
              size="md"
              icon={<Layers size={14} />}
              isLoading={isSubmitting}
              disabled={isSubmitting}
            >
              Generate Chunks
            </Button>
          </div>
        </form>
      </div>
    </div>
  );
};
