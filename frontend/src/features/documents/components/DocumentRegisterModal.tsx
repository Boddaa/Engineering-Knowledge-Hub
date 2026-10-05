import React, { useState, useEffect, useRef } from 'react';
import { X, UploadCloud, AlertCircle, FileText, CheckCircle2 } from 'lucide-react';
import { Button } from '@/components/ui/Button';
import { Input } from '@/components/ui/Input';
import { SourceSelect } from '@/features/sources';
import { documentApi } from '../api/documentApi';
import type { DocumentDto, UploadDocumentRequest } from '../types/documentTypes';
import { ApiError } from '@/services/api/apiErrors';
import styles from './DocumentRegisterModal.module.css';

const UUID_REGEX = /^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$/i;

export interface DocumentRegisterModalProps {
  isOpen: boolean;
  initialSourceId?: string;
  onClose: () => void;
  onSuccess: (document: DocumentDto) => void;
}

export const DocumentRegisterModal: React.FC<DocumentRegisterModalProps> = ({
  isOpen,
  initialSourceId = '',
  onClose,
  onSuccess,
}) => {
  const [sourceId, setSourceId] = useState<string>(initialSourceId);
  const [fileName, setFileName] = useState<string>('');
  const [contentType, setContentType] = useState<string>('text/markdown');
  const [uploadedById, setUploadedById] = useState<string>('');
  const [storagePath, setStoragePath] = useState<string>('');
  const [content, setContent] = useState<string>('');

  const [isSubmitting, setIsSubmitting] = useState<boolean>(false);
  const [validationError, setValidationError] = useState<string | null>(null);
  const [apiError, setApiError] = useState<string | null>(null);

  const fileInputRef = useRef<HTMLInputElement | null>(null);

  useEffect(() => {
    if (isOpen) {
      setSourceId(initialSourceId);
      setFileName('');
      setContentType('text/markdown');
      setUploadedById('');
      setStoragePath('');
      setContent('');
      setValidationError(null);
      setApiError(null);
    }
  }, [isOpen, initialSourceId]);

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

  const handleFileChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const file = e.target.files?.[0];
    if (!file) return;

    setFileName(file.name);
    setContentType(file.type || 'text/plain');

    const reader = new FileReader();
    reader.onload = (event) => {
      const result = event.target?.result;
      if (typeof result === 'string') {
        setContent(result);
      }
    };
    // If text file, read as text; otherwise read as data URL / text
    if (
      file.type.startsWith('text/') ||
      file.name.endsWith('.md') ||
      file.name.endsWith('.json') ||
      file.name.endsWith('.txt') ||
      file.name.endsWith('.yaml') ||
      file.name.endsWith('.yml')
    ) {
      reader.readAsText(file);
    } else {
      reader.readAsDataURL(file);
    }
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();

    const trimmedFileName = fileName.trim();
    if (!trimmedFileName) {
      setValidationError('File name is required.');
      return;
    }

    const trimmedContentType = contentType.trim();
    if (!trimmedContentType) {
      setValidationError('Content type is required.');
      return;
    }

    const trimmedSourceId = sourceId.trim();
    if (!trimmedSourceId) {
      setValidationError('A Knowledge Source must be selected.');
      return;
    }
    if (!UUID_REGEX.test(trimmedSourceId)) {
      setValidationError('Knowledge Source ID must be a valid UUID.');
      return;
    }

    const trimmedUploadedById = uploadedById.trim();
    if (!trimmedUploadedById) {
      setValidationError('Uploaded By ID is required.');
      return;
    }
    if (!UUID_REGEX.test(trimmedUploadedById)) {
      setValidationError('Uploaded By ID must be a valid UUID.');
      return;
    }

    const trimmedContent = content.trim();
    if (!trimmedContent) {
      setValidationError('Document content is required.');
      return;
    }

    setValidationError(null);
    setApiError(null);
    setIsSubmitting(true);

    try {
      const request: UploadDocumentRequest = {
        fileName: trimmedFileName,
        contentType: trimmedContentType,
        sourceId: trimmedSourceId,
        uploadedById: trimmedUploadedById,
        content: trimmedContent,
        storagePath: storagePath.trim() || null,
      };

      const result = await documentApi.uploadDocument(request);
      onSuccess(result);
      onClose();
    } catch (err: unknown) {
      if (err instanceof ApiError) {
        setApiError(err.message);
      } else {
        setApiError('Failed to register document.');
      }
    } finally {
      setIsSubmitting(false);
    }
  };

  return (
    <div
      className={styles.backdrop}
      role="dialog"
      aria-modal="true"
      aria-labelledby="register-document-title"
      onClick={onClose}
    >
      <div className={styles.modal} onClick={(e) => e.stopPropagation()}>
        <div className={styles.header}>
          <div>
            <h2 id="register-document-title" className={styles.headerTitle}>
              <UploadCloud size={18} className={styles.headerIcon} aria-hidden="true" />
              Register Engineering Document
            </h2>
            <p className={styles.headerSubtitle}>
              Register documentation, technical specs, or reference content to ingest and synthesize into knowledge.
            </p>
          </div>
          <button
            type="button"
            className={styles.closeButton}
            onClick={onClose}
            aria-label="Close register document dialog"
            disabled={isSubmitting}
          >
            <X size={18} />
          </button>
        </div>

        <form onSubmit={handleSubmit} noValidate>
          <div className={styles.body}>
            {(validationError || apiError) && (
              <div className={styles.errorBanner} role="alert">
                <AlertCircle size={16} />
                <span>{validationError || apiError}</span>
              </div>
            )}

            {/* Knowledge Source Selection */}
            <SourceSelect
              id="doc-source-id"
              label="Knowledge Source *"
              value={sourceId}
              onChange={(newSourceId) => {
                setSourceId(newSourceId);
                setValidationError(null);
              }}
              required
              disabled={isSubmitting}
              showDetailsPreview={false}
              helperText="The source under which this document belongs."
            />

            {/* File Picker Dropzone */}
            <div
              className={styles.fileDropZone}
              onClick={() => fileInputRef.current?.click()}
              role="button"
              tabIndex={0}
              onKeyDown={(e) => {
                if (e.key === 'Enter' || e.key === ' ') {
                  fileInputRef.current?.click();
                }
              }}
              aria-label="Select file to upload"
            >
              <FileText size={24} className={styles.dropZoneIcon} aria-hidden="true" />
              <span className={styles.dropZoneText}>
                {fileName ? `Selected: ${fileName}` : 'Choose local file or paste text below'}
              </span>
              <span className={styles.dropZoneSubtext}>
                Supports Markdown, JSON, Text, or Technical Documentation
              </span>
              <input
                ref={fileInputRef}
                type="file"
                className={styles.fileInput}
                onChange={handleFileChange}
                disabled={isSubmitting}
              />
            </div>

            <div className={styles.gridTwoCol}>
              <Input
                id="doc-file-name"
                label="File Name *"
                value={fileName}
                onChange={(e) => {
                  setFileName(e.target.value);
                  setValidationError(null);
                }}
                placeholder="e.g. redis-caching-guide.md"
                required
                disabled={isSubmitting}
              />

              <Input
                id="doc-content-type"
                label="Content Type *"
                value={contentType}
                onChange={(e) => {
                  setContentType(e.target.value);
                  setValidationError(null);
                }}
                placeholder="e.g. text/markdown"
                required
                disabled={isSubmitting}
              />
            </div>

            <div className={styles.gridTwoCol}>
              <Input
                id="doc-uploader-id"
                label="Uploaded By ID (UUID) *"
                value={uploadedById}
                onChange={(e) => {
                  setUploadedById(e.target.value);
                  setValidationError(null);
                }}
                placeholder="e.g. 550e8400-e29b-41d4-a716-446655440000"
                required
                disabled={isSubmitting}
                helperText="Engineer identity UUID registering this asset."
              />

              <Input
                id="doc-storage-path"
                label="Storage Path (Optional)"
                value={storagePath}
                onChange={(e) => setStoragePath(e.target.value)}
                placeholder="e.g. s3://eng-docs/redis/spec.md"
                disabled={isSubmitting}
                helperText="External storage URI or archive locator."
              />
            </div>

            <div className={styles.fieldGroup}>
              <div className={styles.fieldLabel}>
                <label htmlFor="doc-content">
                  Document Content <span className={styles.requiredStar}>*</span>
                </label>
              </div>
              <textarea
                id="doc-content"
                className={styles.textarea}
                value={content}
                onChange={(e) => {
                  setContent(e.target.value);
                  setValidationError(null);
                }}
                placeholder="Paste raw documentation text, markdown, or Base64 encoded file payload..."
                rows={7}
                disabled={isSubmitting}
                required
              />
              <span className={styles.fieldHelper}>
                Text content to be extracted and indexed for engineering knowledge synthesis.
              </span>
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
              icon={<CheckCircle2 size={15} />}
              isLoading={isSubmitting}
              disabled={isSubmitting}
            >
              Register Document
            </Button>
          </div>
        </form>
      </div>
    </div>
  );
};
