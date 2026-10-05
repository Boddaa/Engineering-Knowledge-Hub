import React, { useState, useId } from 'react';
import { ChevronDown, ExternalLink, RefreshCw } from 'lucide-react';
import { Badge } from '@/components/ui/Badge';
import { Spinner } from '@/components/ui/Spinner';
import { useSources } from '../hooks/useSources';
import type {
  KnowledgeSourceDto,
  SourceType,
  TrustLevel,
} from '../types/sourceTypes';
import {
  normalizeSourceType,
  normalizeTrustLevel,
  getTrustLevelBadgeVariant,
  getSourceTypeBadgeVariant,
} from '../utils/sourceFormatters';
import styles from './SourceSelect.module.css';

export interface SourceSelectProps {
  id?: string;
  label?: string;
  value: string;
  onChange: (sourceId: string, source?: KnowledgeSourceDto) => void;
  required?: boolean;
  disabled?: boolean;
  filterSourceType?: SourceType | number;
  filterTrustLevel?: TrustLevel | number;
  filterIsApproved?: boolean;
  excludeIds?: string[];
  placeholder?: string;
  error?: string;
  helperText?: string;
  showDetailsPreview?: boolean;
}

export const SourceSelect: React.FC<SourceSelectProps> = ({
  id,
  label = 'Knowledge Source',
  value,
  onChange,
  required = false,
  disabled = false,
  filterSourceType,
  filterTrustLevel,
  filterIsApproved,
  excludeIds = [],
  placeholder = '-- Select a Knowledge Source --',
  error,
  helperText,
  showDetailsPreview = true,
}) => {
  const generatedId = useId();
  const selectId = id || generatedId;
  const [isFocused, setIsFocused] = useState(false);

  const { sources, isLoading, error: fetchError, refetch } = useSources({
    sourceType: filterSourceType,
    trustLevel: filterTrustLevel,
    isApproved: filterIsApproved,
  });

  // Filter out already excluded IDs (e.g. already attached to article)
  const availableSources = sources.filter(
    (src) => src.id === value || !excludeIds.includes(src.id)
  );

  const selectedSource = sources.find((s) => s.id === value);

  const handleChange = (e: React.ChangeEvent<HTMLSelectElement>) => {
    const selectedId = e.target.value;
    const found = sources.find((s) => s.id === selectedId);
    onChange(selectedId, found);
  };

  return (
    <div className={styles.container}>
      {label && (
        <div className={styles.labelRow}>
          <label htmlFor={selectId} className={styles.label}>
            {label}
            {required && <span className={styles.requiredStar} aria-hidden="true">*</span>}
          </label>
        </div>
      )}

      {fetchError && (
        <div className={styles.errorBanner} role="alert">
          <span>Failed to load sources from backend.</span>
          <button
            type="button"
            className={styles.retryButton}
            onClick={refetch}
            aria-label="Retry loading sources"
          >
            <RefreshCw size={12} style={{ marginRight: 4, display: 'inline' }} />
            Retry
          </button>
        </div>
      )}

      <div
        className={`
          ${styles.selectWrapper}
          ${isFocused ? styles.isFocused : ''}
          ${error ? styles.hasError : ''}
          ${disabled || isLoading ? styles.disabled : ''}
        `}
      >
        <select
          id={selectId}
          value={value}
          onChange={handleChange}
          onFocus={() => setIsFocused(true)}
          onBlur={() => setIsFocused(false)}
          disabled={disabled || isLoading}
          required={required}
          aria-required={required}
          aria-invalid={Boolean(error)}
          aria-busy={isLoading}
          className={styles.select}
        >
          {isLoading ? (
            <option value="">Loading knowledge sources...</option>
          ) : (
            <>
              <option value="">{placeholder}</option>
              {availableSources.length === 0 ? (
                <option value="" disabled>
                  No sources available
                </option>
              ) : (
                availableSources.map((source) => (
                  <option key={source.id} value={source.id}>
                    {source.name} ({normalizeTrustLevel(source.trustLevel)} • {normalizeSourceType(source.sourceType)})
                  </option>
                ))
              )}
            </>
          )}
        </select>

        {isLoading ? (
          <div className={styles.spinnerWrapper} aria-hidden="true">
            <Spinner size="sm" />
          </div>
        ) : (
          <ChevronDown size={16} className={styles.chevronIcon} aria-hidden="true" />
        )}
      </div>

      {error && <span className={styles.errorText} role="alert">{error}</span>}
      {!error && helperText && <span className={styles.helperText}>{helperText}</span>}

      {/* Selected Source Details Preview */}
      {showDetailsPreview && selectedSource && (
        <div
          className={styles.previewCard}
          aria-label="Selected knowledge source information"
        >
          <div className={styles.previewHeader}>
            <span className={styles.previewTitle}>{selectedSource.name}</span>
            <div className={styles.badgeGroup}>
              <Badge variant={getSourceTypeBadgeVariant(selectedSource.sourceType)} size="sm">
                {normalizeSourceType(selectedSource.sourceType)}
              </Badge>
              <Badge variant={getTrustLevelBadgeVariant(selectedSource.trustLevel)} size="sm">
                {normalizeTrustLevel(selectedSource.trustLevel)}
              </Badge>
              {selectedSource.isApproved && (
                <Badge variant="success" size="sm">
                  Approved
                </Badge>
              )}
            </div>
          </div>

          {selectedSource.description && (
            <p className={styles.previewDescription}>{selectedSource.description}</p>
          )}

          {selectedSource.url && (
            <a
              href={selectedSource.url}
              target="_blank"
              rel="noopener noreferrer"
              className={styles.previewUrl}
              title={`Open ${selectedSource.url}`}
            >
              <span>{selectedSource.url}</span>
              <ExternalLink size={11} aria-hidden="true" />
            </a>
          )}
        </div>
      )}
    </div>
  );
};
