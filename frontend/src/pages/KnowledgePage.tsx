import React from 'react';
import { useNavigate } from 'react-router-dom';
import { Search, X, BookOpen, Plus } from 'lucide-react';
import { PageShell } from '@/components/layout/PageShell';
import { Input } from '@/components/ui/Input';
import { Button } from '@/components/ui/Button';
import { Skeleton } from '@/components/ui/Skeleton';
import { EmptyState } from '@/components/ui/EmptyState';
import { ErrorState } from '@/components/ui/ErrorState';
import {
  useKnowledgeSearch,
  KnowledgeCard,
  KnowledgePagination,
  type SearchSortOption,
} from '@/features/knowledge';
import styles from './KnowledgePage.module.css';

export const KnowledgePage: React.FC = () => {
  const navigate = useNavigate();
  const {
    articles,
    totalCount,
    page,
    totalPages,
    hasPreviousPage,
    hasNextPage,
    isLoading,
    error,
    searchQuery,
    sortBy,
    setSearchQuery,
    executeSearch,
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

  return (
    <PageShell
      title="Knowledge"
      description="Find engineering knowledge, solutions, incidents, and verified technical guidance."
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
        {/* Search Toolbar */}
        <div className={styles.searchToolbar} role="search" aria-label="Search knowledge articles">
          <form className={styles.searchForm} onSubmit={handleFormSubmit}>
            <div className={styles.searchInputWrapper}>
              <Input
                size="sm"
                value={searchQuery}
                onChange={(e) => setSearchQuery(e.target.value)}
                placeholder="Search knowledge articles..."
                prefix={<Search size={14} aria-hidden="true" />}
                aria-label="Search knowledge articles"
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

          {/* Sort Selector */}
          <div className={styles.filterControls}>
            <label htmlFor="knowledge-sort-select" className={styles.sortLabel}>
              Sort by:
            </label>
            <select
              id="knowledge-sort-select"
              className={styles.sortSelect}
              value={sortBy}
              onChange={(e) => changeSortBy(e.target.value as SearchSortOption)}
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

        {/* Active Query Banner */}
        {searchQuery.trim() !== '' && !isLoading && (
          <div className={styles.activeQueryBanner}>
            <span className={styles.activeQueryText}>
              Showing results for query: <strong>"{searchQuery}"</strong> ({totalCount} found)
            </span>
            <button
              type="button"
              className={styles.clearSearchButton}
              onClick={handleClearQuery}
              aria-label="Clear active search query"
            >
              <X size={12} className={styles.clearSearchIcon} /> Clear filter
            </button>
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

        {/* Empty State */}
        {!isLoading && !error && articles.length === 0 && (
          <EmptyState
            icon={<BookOpen size={32} />}
            title="No knowledge articles found"
            description={
              searchQuery.trim() !== ''
                ? `No articles matched "${searchQuery}". Try a different search term or clear the filter.`
                : 'No knowledge articles are currently available in the knowledge repository.'
            }
            action={
              searchQuery.trim() !== '' ? (
                <Button variant="subtle" size="sm" onClick={handleClearQuery}>
                  Clear Search
                </Button>
              ) : undefined
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
