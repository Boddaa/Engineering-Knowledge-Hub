import React, { useState, useEffect } from 'react';
import { Filter, RotateCcw, X } from 'lucide-react';
import { Button } from '@/components/ui/Button';
import { useTaxonomyCatalog } from '@/features/taxonomy';
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
  const { types, products, technologies, tags } = useTaxonomyCatalog();
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

          {/* Knowledge Type */}
          <div className={styles.selectGroup}>
            <label htmlFor="filter-knowledge-type-select" className={styles.selectLabel}>
              Knowledge Type
            </label>
            <select
              id="filter-knowledge-type-select"
              className={styles.select}
              value={localKnowledgeTypeId}
              onChange={(e) => setLocalKnowledgeTypeId(e.target.value)}
            >
              <option value="">All Knowledge Types</option>
              {types.map((type) => (
                <option key={type.id} value={type.id}>
                  {type.name}
                </option>
              ))}
            </select>
          </div>

          {/* Product */}
          <div className={styles.selectGroup}>
            <label htmlFor="filter-product-select" className={styles.selectLabel}>
              Product
            </label>
            <select
              id="filter-product-select"
              className={styles.select}
              value={localProductId}
              onChange={(e) => setLocalProductId(e.target.value)}
            >
              <option value="">All Products</option>
              {products.map((product) => (
                <option key={product.id} value={product.id}>
                  {product.name} ({product.code})
                </option>
              ))}
            </select>
          </div>

          {/* Technology */}
          <div className={styles.selectGroup}>
            <label htmlFor="filter-technology-select" className={styles.selectLabel}>
              Technology
            </label>
            <select
              id="filter-technology-select"
              className={styles.select}
              value={localTechnologyId}
              onChange={(e) => setLocalTechnologyId(e.target.value)}
            >
              <option value="">All Technologies</option>
              {technologies.map((tech) => (
                <option key={tech.id} value={tech.id}>
                  {tech.name}
                </option>
              ))}
            </select>
          </div>

          {/* Tag */}
          <div className={styles.selectGroup}>
            <label htmlFor="filter-tag-select" className={styles.selectLabel}>
              Tag
            </label>
            <select
              id="filter-tag-select"
              className={styles.select}
              value={localTagId}
              onChange={(e) => setLocalTagId(e.target.value)}
            >
              <option value="">All Tags</option>
              {tags.map((tag) => (
                <option key={tag.id} value={tag.id}>
                  {tag.name}
                </option>
              ))}
            </select>
          </div>
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
  const { types, products, technologies, tags } = useTaxonomyCatalog();
  const hasQuery = searchQuery.trim() !== '';
  const hasFilters = Object.values(filters).some(Boolean);

  if (!hasQuery && !hasFilters) {
    return null;
  }

  const formatUuidDisplay = (id: string) => {
    return `${id.substring(0, 8)}...`;
  };

  const getTypeName = (id: string) => types.find((t) => t.id === id)?.name;
  const getProductName = (id: string) => products.find((p) => p.id === id)?.name;
  const getTechName = (id: string) => technologies.find((t) => t.id === id)?.name;
  const getTagName = (id: string) => tags.find((t) => t.id === id)?.name;

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

      {/* Knowledge Type chip */}
      {filters.knowledgeTypeId && (
        <span className={styles.chip}>
          <span>Type: {getTypeName(filters.knowledgeTypeId) ?? formatUuidDisplay(filters.knowledgeTypeId)}</span>
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

      {/* Product chip */}
      {filters.productId && (
        <span className={styles.chip}>
          <span>Product: {getProductName(filters.productId) ?? formatUuidDisplay(filters.productId)}</span>
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

      {/* Technology chip */}
      {filters.technologyId && (
        <span className={styles.chip}>
          <span>Tech: {getTechName(filters.technologyId) ?? formatUuidDisplay(filters.technologyId)}</span>
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

      {/* Tag chip */}
      {filters.tagId && (
        <span className={styles.chip}>
          <span>Tag: {getTagName(filters.tagId) ?? formatUuidDisplay(filters.tagId)}</span>
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
