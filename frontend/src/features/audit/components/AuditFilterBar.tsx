import React from 'react';
import { Layers, Database, User, Search, RotateCcw, Info } from 'lucide-react';
import { Button } from '@/components/ui/Button';
import type {
  AuditFilterState,
  AuditFilterMode,
  AuditPaginationState,
} from '../types/auditTypes';
import styles from './AuditFilterBar.module.css';

export interface AuditFilterBarProps {
  filters: AuditFilterState;
  onFilterChange: React.Dispatch<React.SetStateAction<AuditFilterState>>;
  pagination: AuditPaginationState;
  onPageSizeChange: (pageSize: number) => void;
  onReset: () => void;
  isLoading: boolean;
}

const COMMON_ENTITIES = [
  { value: 'KnowledgeArticle', label: 'KnowledgeArticle' },
  { value: 'KnowledgeVersion', label: 'KnowledgeVersion' },
];

export const AuditFilterBar: React.FC<AuditFilterBarProps> = ({
  filters,
  onFilterChange,
  pagination,
  onPageSizeChange,
  onReset,
  isLoading,
}) => {
  const handleModeSwitch = (mode: AuditFilterMode) => {
    onFilterChange((prev) => ({
      ...prev,
      mode,
      // If switching to all, clear specific target IDs
      ...(mode === 'all' ? { entityId: '', userId: '' } : {}),
      // If switching to entity, set default entityName if empty
      ...(mode === 'entity' && !prev.entityName
        ? { entityName: 'KnowledgeArticle' }
        : {}),
    }));
  };

  const handleEntityNameChange = (e: React.ChangeEvent<HTMLSelectElement | HTMLInputElement>) => {
    const value = e.target.value;
    onFilterChange((prev) => ({ ...prev, entityName: value }));
  };

  const handleEntityIdChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const value = e.target.value;
    onFilterChange((prev) => ({ ...prev, entityId: value }));
  };

  const handleUserIdChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const value = e.target.value;
    onFilterChange((prev) => ({ ...prev, userId: value }));
  };

  const handleSearchChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const value = e.target.value;
    onFilterChange((prev) => ({ ...prev, searchTerm: value }));
  };

  const handlePageSizeChange = (e: React.ChangeEvent<HTMLSelectElement>) => {
    const size = parseInt(e.target.value, 10);
    if (!Number.isNaN(size) && size > 0) {
      onPageSizeChange(size);
    }
  };

  return (
    <div className={styles.filterBarContainer} role="search" aria-label="Audit log filters">
      <div className={styles.modeTabs}>
        <button
          type="button"
          className={`${styles.modeTab} ${filters.mode === 'all' ? styles.modeTabActive : ''}`}
          onClick={() => handleModeSwitch('all')}
          aria-pressed={filters.mode === 'all'}
        >
          <Layers size={14} />
          All System Logs
        </button>

        <button
          type="button"
          className={`${styles.modeTab} ${filters.mode === 'entity' ? styles.modeTabActive : ''}`}
          onClick={() => handleModeSwitch('entity')}
          aria-pressed={filters.mode === 'entity'}
        >
          <Database size={14} />
          Filter by Entity
        </button>

        <button
          type="button"
          className={`${styles.modeTab} ${filters.mode === 'user' ? styles.modeTabActive : ''}`}
          onClick={() => handleModeSwitch('user')}
          aria-pressed={filters.mode === 'user'}
        >
          <User size={14} />
          Filter by User
        </button>
      </div>

      <div className={styles.controlsRow}>
        {filters.mode === 'entity' && (
          <>
            <div className={styles.inputGroup}>
              <label htmlFor="audit-entity-name" className={styles.inputLabel}>
                Entity Type
              </label>
              <select
                id="audit-entity-name"
                className={styles.selectField}
                value={filters.entityName}
                onChange={handleEntityNameChange}
              >
                {COMMON_ENTITIES.map((item) => (
                  <option key={item.value} value={item.value}>
                    {item.label}
                  </option>
                ))}
              </select>
            </div>

            <div className={styles.inputGroup}>
              <label htmlFor="audit-entity-id" className={styles.inputLabel}>
                Entity ID (UUID)
              </label>
              <input
                id="audit-entity-id"
                type="text"
                placeholder="e.g. 3fa85f64-5717-4562-b3fc-2c963f66afa6"
                className={`${styles.inputField} ${styles.monoInput}`}
                value={filters.entityId}
                onChange={handleEntityIdChange}
              />
            </div>
          </>
        )}

        {filters.mode === 'user' && (
          <div className={styles.inputGroup}>
            <label htmlFor="audit-user-id" className={styles.inputLabel}>
              User ID (UUID)
            </label>
            <input
              id="audit-user-id"
              type="text"
              placeholder="e.g. 3fa85f64-5717-4562-b3fc-2c963f66afa6"
              className={`${styles.inputField} ${styles.monoInput}`}
              value={filters.userId}
              onChange={handleUserIdChange}
            />
          </div>
        )}

        <div className={styles.searchGroup}>
          <label htmlFor="audit-quick-search" className={styles.inputLabel}>
            Search Across Logs
          </label>
          <div className={styles.searchWrapper}>
            <Search size={16} className={styles.searchIcon} aria-hidden="true" />
            <input
              id="audit-quick-search"
              type="search"
              placeholder="Filter by action, details text, or IDs..."
              className={styles.searchInput}
              value={filters.searchTerm}
              onChange={handleSearchChange}
            />
          </div>
        </div>

        <div className={styles.inputGroup} style={{ flex: 0, minWidth: '100px' }}>
          <label htmlFor="audit-page-size" className={styles.inputLabel}>
            Page Size
          </label>
          <select
            id="audit-page-size"
            className={styles.selectField}
            value={pagination.pageSize}
            onChange={handlePageSizeChange}
          >
            <option value={10}>10</option>
            <option value={25}>25</option>
            <option value={50}>50</option>
            <option value={100}>100</option>
          </select>
        </div>

        <div className={styles.actionsGroup}>
          <Button
            type="button"
            variant="subtle"
            size="md"
            icon={<RotateCcw size={14} />}
            onClick={onReset}
            disabled={isLoading}
            title="Reset filters to default"
          >
            Reset
          </Button>
        </div>
      </div>

      <div className={styles.infoBanner} role="note">
        <Info size={14} />
        <span>
          <strong>Client-side pagination & search:</strong> The audit API returns full historical logs for the selected scope. Pagination and keyword filtering are processed in-memory for instant responsive browsing.
        </span>
      </div>
    </div>
  );
};
