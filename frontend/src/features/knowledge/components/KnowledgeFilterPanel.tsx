import React, { useState, useEffect } from 'react';
import { Filter, RotateCcw, X } from 'lucide-react';
import { Button } from '@/components/ui/Button';
import { Input } from '@/components/ui/Input';
import type { KnowledgeFilterState } from '../hooks/useKnowledgeSearch';
import {
  VALID_KNOWLEDGE_STATUSES,
  VALID_VERIFICATION_STATUSES,
  normalizeFilterStatus,
  normalizeFilterVerificationStatus,
  normalizeFilterUuid,
} from '../utils/knowledgeFormatters';
import styles from './KnowledgeFilterPanel.module.css';

export interface KnowledgeFilterPanelProps {
  isOpen: boolean;
  onClose: () => void;
  filters: KnowledgeFilterState;
  onApplyFilters: (filters: Partial<KnowledgeFilterState>) => void;
  onResetFilters: () => void;
}

export const KnowledgeFilterPanel: React.FC<KnowledgeFilterPanelProps> = ({
  isOpen,
  onClose,
  filters,
  onApplyFilters,
  onResetFilters,
}) => {
  const [localStatus, setLocalStatus] = useState<string>(filters.status || '');
  const [localVerification, setLocalVerification] = useState<string>(
    filters.verificationStatus || ''
  );
  const [localKnowledgeTypeId, setLocalKnowledgeTypeId] = useState<string>(
    filters.knowledgeTypeId || ''
  );
  const [localProductId, setLocalProductId] = useState<string>(
    filters.productId || ''
  );
  const [localTechnologyId, setLocalTechnologyId] = useState<string>(
    filters.technologyId || ''
  );
  const [localTagId, setLocalTagId] = useState<string>(filters.tagId || '');

  // Synchronize when filters change
  useEffect(() => {
    setLocalStatus(filters.status || '');
    setLocalVerification(filters.verificationStatus || '');
    setLocalKnowledgeTypeId(filters.knowledgeTypeId || '');
    setLocalProductId(filters.productId || '');
    setLocalTechnologyId(filters.technologyId || '');
    setLocalTagId(filters.tagId || '');
  }, [filters]);

  if (!isOpen) {
    return null;
  }

  const handleApply = (e: React.FormEvent) => {
    e.preventDefault();

    onApplyFilters({
      status: normalizeFilterStatus(localStatus),
      verificationStatus: normalizeFilterVerificationStatus(localVerification),
      knowledgeTypeId: normalizeFilterUuid(localKnowledgeTypeId),
      productId: normalizeFilterUuid(localProductId),
      technologyId: normalizeFilterUuid(localTechnologyId),
      tagId: normalizeFilterUuid(localTagId),
    });

    onClose();
  };

  const handleReset = () => {
    setLocalStatus('');
    setLocalVerification('');
    setLocalKnowledgeTypeId('');
    setLocalProductId('');
    setLocalTechnologyId('');
    setLocalTagId('');
    onResetFilters();
    onClose();
  };

  return (
    <div
      className={styles.filterPanel}
      role="region"
      aria-label="Knowledge search filters"
    >
      <div className={styles.panelHeader}>
        <h3 className={styles.panelTitle}>
          <Filter size={15} className="text-accent" aria-hidden="true" />
          Filter Knowledge Articles
        </h3>
        <button
          type="button"
          className={styles.chipCloseBtn}
          onClick={onClose}
          aria-label="Close filters panel"
        >
          <X size={16} />
        </button>
      </div>

      <form onSubmit={handleApply} noValidate>
        <div className={styles.filterGrid}>
          {/* Status Filter */}
          <div className={styles.selectGroup}>
            <label htmlFor="filter-status-select" className={styles.selectLabel}>
              Lifecycle Status
            </label>
            <select
              id="filter-status-select"
              className={styles.select}
              value={localStatus}
              onChange={(e) => setLocalStatus(e.target.value)}
            >
              <option value="">All Statuses</option>
              {VALID_KNOWLEDGE_STATUSES.map((status) => (
                <option key={status} value={status}>
                  {status === 'PendingReview' ? 'Pending Review' : status}
                </option>
              ))}
            </select>
          </div>

          {/* Verification Status Filter */}
          <div className={styles.selectGroup}>
            <label htmlFor="filter-verification-select" className={styles.selectLabel}>
              Verification Status
            </label>
            <select
              id="filter-verification-select"
              className={styles.select}
              value={localVerification}
              onChange={(e) => setLocalVerification(e.target.value)}
            >
              <option value="">All Verification States</option>
              {VALID_VERIFICATION_STATUSES.map((vStatus) => (
                <option key={vStatus} value={vStatus}>
                  {vStatus}
                </option>
              ))}
            </select>
          </div>

          {/* Knowledge Type ID */}
          <Input
            id="filter-knowledge-type-id"
            label="Knowledge Type ID"
            size="sm"
            placeholder="UUID (e.g. 00000000-...)"
            value={localKnowledgeTypeId}
            onChange={(e) => setLocalKnowledgeTypeId(e.target.value)}
            helperText="Filter by exact Knowledge Type UUID"
          />

          {/* Product ID */}
          <Input
            id="filter-product-id"
            label="Product ID"
            size="sm"
            placeholder="UUID (e.g. 00000000-...)"
            value={localProductId}
            onChange={(e) => setLocalProductId(e.target.value)}
            helperText="Filter by associated Product UUID"
          />

          {/* Technology ID */}
          <Input
            id="filter-technology-id"
            label="Technology ID"
            size="sm"
            placeholder="UUID (e.g. 00000000-...)"
            value={localTechnologyId}
            onChange={(e) => setLocalTechnologyId(e.target.value)}
            helperText="Filter by associated Technology UUID"
          />

          {/* Tag ID */}
          <Input
            id="filter-tag-id"
            label="Tag ID"
            size="sm"
            placeholder="UUID (e.g. 00000000-...)"
            value={localTagId}
            onChange={(e) => setLocalTagId(e.target.value)}
            helperText="Filter by associated Tag UUID"
          />
        </div>

        <div className={styles.panelFooter}>
          <Button
            type="button"
            variant="ghost"
            size="sm"
            icon={<RotateCcw size={13} />}
            onClick={handleReset}
          >
            Reset All
          </Button>
          <Button type="submit" variant="primary" size="sm">
            Apply Filters
          </Button>
        </div>
      </form>
    </div>
  );
};

export interface ActiveFilterChipsProps {
  searchQuery: string;
  filters: KnowledgeFilterState;
  onClearQuery: () => void;
  onRemoveFilter: (key: keyof KnowledgeFilterState) => void;
  onClearAll: () => void;
}

export const ActiveFilterChips: React.FC<ActiveFilterChipsProps> = ({
  searchQuery,
  filters,
  onClearQuery,
  onRemoveFilter,
  onClearAll,
}) => {
  const hasQuery = searchQuery.trim() !== '';
  const hasFilters = Object.values(filters).some(Boolean);

  if (!hasQuery && !hasFilters) {
    return null;
  }

  const formatUuidDisplay = (id: string) => {
    return `${id.substring(0, 8)}...`;
  };

  return (
    <div
      className={styles.chipsContainer}
      role="region"
      aria-label="Active search and filter criteria"
    >
      <span className={styles.chipsLabel}>Active:</span>

      {/* Query chip */}
      {hasQuery && (
        <span className={styles.chip}>
          <span>Query: "{searchQuery}"</span>
          <button
            type="button"
            className={styles.chipCloseBtn}
            onClick={onClearQuery}
            aria-label="Remove search query filter"
          >
            <X size={12} />
          </button>
        </span>
      )}

      {/* Status chip */}
      {filters.status && (
        <span className={styles.chip}>
          <span>Status: {filters.status === 'PendingReview' ? 'Pending Review' : filters.status}</span>
          <button
            type="button"
            className={styles.chipCloseBtn}
            onClick={() => onRemoveFilter('status')}
            aria-label="Remove status filter"
          >
            <X size={12} />
          </button>
        </span>
      )}

      {/* Verification chip */}
      {filters.verificationStatus && (
        <span className={styles.chip}>
          <span>Verification: {filters.verificationStatus}</span>
          <button
            type="button"
            className={styles.chipCloseBtn}
            onClick={() => onRemoveFilter('verificationStatus')}
            aria-label="Remove verification status filter"
          >
            <X size={12} />
          </button>
        </span>
      )}

      {/* Knowledge Type ID chip */}
      {filters.knowledgeTypeId && (
        <span className={styles.chip}>
          <span>Type ID: {formatUuidDisplay(filters.knowledgeTypeId)}</span>
          <button
            type="button"
            className={styles.chipCloseBtn}
            onClick={() => onRemoveFilter('knowledgeTypeId')}
            aria-label="Remove knowledge type filter"
          >
            <X size={12} />
          </button>
        </span>
      )}

      {/* Product ID chip */}
      {filters.productId && (
        <span className={styles.chip}>
          <span>Product ID: {formatUuidDisplay(filters.productId)}</span>
          <button
            type="button"
            className={styles.chipCloseBtn}
            onClick={() => onRemoveFilter('productId')}
            aria-label="Remove product filter"
          >
            <X size={12} />
          </button>
        </span>
      )}

      {/* Technology ID chip */}
      {filters.technologyId && (
        <span className={styles.chip}>
          <span>Tech ID: {formatUuidDisplay(filters.technologyId)}</span>
          <button
            type="button"
            className={styles.chipCloseBtn}
            onClick={() => onRemoveFilter('technologyId')}
            aria-label="Remove technology filter"
          >
            <X size={12} />
          </button>
        </span>
      )}

      {/* Tag ID chip */}
      {filters.tagId && (
        <span className={styles.chip}>
          <span>Tag ID: {formatUuidDisplay(filters.tagId)}</span>
          <button
            type="button"
            className={styles.chipCloseBtn}
            onClick={() => onRemoveFilter('tagId')}
            aria-label="Remove tag filter"
          >
            <X size={12} />
          </button>
        </span>
      )}

      {/* Clear all action */}
      <button
        type="button"
        className={styles.clearAllBtn}
        onClick={onClearAll}
        aria-label="Clear all active filters"
      >
        Clear all
      </button>
    </div>
  );
};
