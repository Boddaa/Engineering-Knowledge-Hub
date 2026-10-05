import React, { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { Search, X, BookOpen, Plus, SlidersHorizontal } from 'lucide-react';
import { PageShell } from '@/components/layout/PageShell';
import { Input } from '@/components/ui/Input';
import { Button } from '@/components/ui/Button';
import { Badge } from '@/components/ui/Badge';
import { Skeleton } from '@/components/ui/Skeleton';
import { EmptyState } from '@/components/ui/EmptyState';
import { ErrorState } from '@/components/ui/ErrorState';
import {
  useKnowledgeSearch,
  KnowledgeCard,
  KnowledgePagination,
  KnowledgeFilterPanel,
  ActiveFilterChips,
  normalizeSearchSortOption,
} from '@/features/knowledge';
import styles from './KnowledgePage.module.css';

export const KnowledgePage: React.FC = () => {
  const navigate = useNavigate();
  const [isFilterPanelOpen, setIsFilterPanelOpen] = useState(false);

  const {
    articles,
    totalCount,
    page,
    pageSize,
    totalPages,
    hasPreviousPage,
    hasNextPage,
    isLoading,
    error,
    searchQuery,
    sortBy,
    filters,
    activeFilterCount,
    hasActiveSearchOrFilters,
    setSearchQuery,
    executeSearch,
    applyFilters,
    removeFilter,
    clearAllFilters,
    goToPage,
    changeSortBy,
    retry,
  } = useKnowledgeSearch();

  const handleFormSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    executeSearch();
  };

  const handleClearQuery = () => {
    setSearchQuery('');
    executeSearch('');
  };

  // Calculate human-friendly range for result context
  const startItem = totalCount > 0 ? (page - 1) * pageSize + 1 : 0;
  const endItem = totalCount > 0 ? Math.min(page * pageSize, totalCount) : 0;

  return (
    <PageShell
      title="Knowledge"
      description="Discover engineering documentation, technical specifications, runbooks, and verified solutions."
      headerActions={
        <Button
          variant="primary"
          size="sm"
          icon={<Plus size={14} />}
          onClick={() => navigate('/knowledge/new')}
          aria-label="Create new knowledge article"
        >
          New Article
        </Button>
      }
    >
      <div className={styles.pageContent}>
        {/* Search & Discovery Toolbar */}
        <div className={styles.searchToolbar} role="search" aria-label="Search and filter knowledge articles">
          <form className={styles.searchForm} onSubmit={handleFormSubmit}>
            <div className={styles.searchInputWrapper}>
              <Input
                size="sm"
                value={searchQuery}
                onChange={(e) => setSearchQuery(e.target.value)}
                placeholder="Search engineering knowledge..."
                prefix={<Search size={14} aria-hidden="true" />}
                suffix={
                  searchQuery ? (
                    <button
                      type="button"
                      className={styles.clearInputButton}
                      onClick={handleClearQuery}
                      aria-label="Clear search input"
                    >
                      <X size={14} />
                    </button>
                  ) : undefined
                }
                aria-label="Search query"
              />
            </div>
            <Button
              type="submit"
              variant="primary"
              size="sm"
              icon={<Search size={14} />}
              disabled={isLoading}
            >
              Search
            </Button>
          </form>

          {/* Filter Toggle and Sort Selector */}
          <div className={styles.toolbarActions}>
            <Button
              variant={isFilterPanelOpen || activeFilterCount > 0 ? 'subtle' : 'ghost'}
              size="sm"
              icon={<SlidersHorizontal size={14} />}
              onClick={() => setIsFilterPanelOpen((prev) => !prev)}
              aria-expanded={isFilterPanelOpen}
              aria-label="Toggle search filters"
            >
              Filters
              {activeFilterCount > 0 && (
                <Badge variant="cyan" size="sm" style={{ marginLeft: 4 }}>
                  {activeFilterCount}
                </Badge>
              )}
            </Button>

            <div className={styles.filterControls}>
              <label htmlFor="knowledge-sort-select" className={styles.sortLabel}>
                Sort:
              </label>
              <select
                id="knowledge-sort-select"
                className={styles.sortSelect}
                value={sortBy}
                onChange={(e) => changeSortBy(normalizeSearchSortOption(e.target.value))}
                aria-label="Sort knowledge articles"
              >
                <option value="Relevance">Relevance</option>
                <option value="RecentlyUpdated">Recently Updated</option>
                <option value="RecentlyCreated">Recently Created</option>
                <option value="TitleAsc">Title (A–Z)</option>
                <option value="TitleDesc">Title (Z–A)</option>
              </select>
            </div>
          </div>
        </div>

        {/* Collapsible Structured Filter Panel */}
        <KnowledgeFilterPanel
          isOpen={isFilterPanelOpen}
          onClose={() => setIsFilterPanelOpen(false)}
          filters={filters}
          onApplyFilters={applyFilters}
          onResetFilters={clearAllFilters}
        />

        {/* Removable Active Filter Chips */}
        <ActiveFilterChips
          searchQuery={searchQuery}
          filters={filters}
          onClearQuery={handleClearQuery}
          onRemoveFilter={removeFilter}
          onClearAll={clearAllFilters}
        />

        {/* Results Context Count */}
        {!isLoading && !error && totalCount > 0 && (
          <div className={styles.resultsContextBar}>
            <span className={styles.resultsCount}>
              Showing <strong>{startItem}–{endItem}</strong> of <strong>{totalCount}</strong> articles
            </span>
          </div>
        )}

        {/* Loading Skeletons */}
        {isLoading && (
          <div className={styles.skeletonList} aria-busy="true" aria-label="Loading knowledge articles">
            {[1, 2, 3, 4].map((n) => (
              <div key={n} className={styles.skeletonCard}>
                <div className={styles.skeletonHeader}>
                  <Skeleton variant="text" width="60%" height={22} />
                  <Skeleton variant="rectangular" width={80} height={20} />
                </div>
                <Skeleton variant="text" width="95%" height={14} />
                <Skeleton variant="text" width="80%" height={14} />
                <div className={styles.skeletonFooter}>
                  <Skeleton variant="rectangular" width={100} height={16} />
                  <Skeleton variant="rectangular" width={60} height={16} />
                </div>
              </div>
            ))}
          </div>
        )}

        {/* Error State */}
        {!isLoading && error && (
          <ErrorState
            title="Failed to Load Knowledge Articles"
            message={error}
            onRetry={retry}
            actionText="Retry Search"
          />
        )}

        {/* Empty States */}
        {!isLoading && !error && articles.length === 0 && (
          <EmptyState
            icon={<BookOpen size={32} />}
            title={
              hasActiveSearchOrFilters
                ? 'No matching knowledge articles'
                : 'No knowledge articles found yet'
            }
            description={
              hasActiveSearchOrFilters
                ? 'No knowledge articles match your current search and filters. Try adjusting your query or clearing filters.'
                : 'There are currently no engineering knowledge articles in the knowledge repository.'
            }
            action={
              hasActiveSearchOrFilters ? (
                <Button variant="subtle" size="sm" onClick={clearAllFilters}>
                  Clear Filters
                </Button>
              ) : (
                <Button
                  variant="primary"
                  size="sm"
                  icon={<Plus size={14} />}
                  onClick={() => navigate('/knowledge/new')}
                >
                  Create First Article
                </Button>
              )
            }
          />
        )}

        {/* Results List */}
        {!isLoading && !error && articles.length > 0 && (
          <>
            <div className={styles.cardList} role="feed" aria-label="Knowledge article results">
              {articles.map((article) => (
                <KnowledgeCard key={article.id} article={article} />
              ))}
            </div>

            {/* Pagination */}
            <KnowledgePagination
              page={page}
              totalPages={totalPages}
              totalCount={totalCount}
              hasPreviousPage={hasPreviousPage}
              hasNextPage={hasNextPage}
              isLoading={isLoading}
              onPageChange={goToPage}
            />
          </>
        )}
      </div>
    </PageShell>
  );
};
