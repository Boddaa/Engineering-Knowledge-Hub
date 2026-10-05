import React, { useState } from 'react';
import {
  BookOpen,
  Plus,
  ExternalLink,
  Trash2,
  Quote,
  RefreshCw,
  Info,
} from 'lucide-react';
import { Button } from '@/components/ui/Button';
import { Badge } from '@/components/ui/Badge';
import { Spinner } from '@/components/ui/Spinner';
import { useArticleSources } from '../hooks/useArticleSources';
import { AttachSourceModal } from './AttachSourceModal';
import type { ArticleSourceLinkDto } from '@/features/knowledge/types/knowledgeTypes';
import {
  normalizeSourceType,
  normalizeTrustLevel,
  getTrustLevelBadgeVariant,
  getSourceTypeBadgeVariant,
} from '../utils/sourceFormatters';
import styles from './ArticleSourcesSection.module.css';

export interface ArticleSourcesSectionProps {
  articleId: string;
  articleTitle: string;
  sourceLinks?: ArticleSourceLinkDto[] | null;
  canManage?: boolean;
  onSourcesChanged?: () => void;
}

export const ArticleSourcesSection: React.FC<ArticleSourcesSectionProps> = ({
  articleId,
  articleTitle,
  sourceLinks = [],
  canManage = true,
  onSourcesChanged,
}) => {
  const {
    sources,
    isLoading,
    error,
    isAttaching,
    isDetaching,
    actionError,
    attachSource,
    detachSource,
    refetch,
    clearActionError,
  } = useArticleSources(articleId);

  const [isAttachModalOpen, setIsAttachModalOpen] = useState(false);
  const [detachingSourceId, setDetachingSourceId] = useState<string | null>(null);

  // Map reference text for attached sources by sourceId
  const referenceTextMap = React.useMemo(() => {
    const map = new Map<string, string>();
    if (sourceLinks) {
      for (const link of sourceLinks) {
        if (link.referenceText) {
          map.set(link.sourceId, link.referenceText);
        }
      }
    }
    return map;
  }, [sourceLinks]);

  const handleOpenAttachModal = () => {
    clearActionError();
    setIsAttachModalOpen(true);
  };

  const handleCloseAttachModal = () => {
    if (!isAttaching) {
      clearActionError();
      setIsAttachModalOpen(false);
    }
  };

  const handleConfirmAttach = async (sourceId: string, referenceText?: string) => {
    const success = await attachSource(sourceId, referenceText);
    if (success) {
      setIsAttachModalOpen(false);
      onSourcesChanged?.();
    }
  };

  const handleDetach = async (sourceId: string) => {
    setDetachingSourceId(sourceId);
    const success = await detachSource(sourceId);
    setDetachingSourceId(null);
    if (success) {
      onSourcesChanged?.();
    }
  };

  const attachedSourceIds = sources.map((s) => s.id);

  return (
    <section
      className={styles.section}
      aria-label="Supporting evidence and knowledge sources"
    >
      <div className={styles.header}>
        <div className={styles.headerLeft}>
          <BookOpen size={16} className={styles.headerIcon} aria-hidden="true" />
          <h3 className={styles.title}>Supporting Evidence &amp; Sources</h3>
          <span className={styles.countBadge}>
            {isLoading ? '...' : sources.length}
          </span>
        </div>

        {canManage && (
          <Button
            variant="subtle"
            size="sm"
            icon={<Plus size={14} />}
            onClick={handleOpenAttachModal}
            disabled={isLoading || isAttaching}
            aria-label="Attach a knowledge source to this article"
          >
            Attach Source
          </Button>
        )}
      </div>

      {actionError && (
        <div className={styles.errorBanner} role="alert">
          <span>{actionError}</span>
          <button
            type="button"
            className={styles.retryBtn}
            onClick={clearActionError}
          >
            Dismiss
          </button>
        </div>
      )}

      {error && (
        <div className={styles.errorBanner} role="alert">
          <span>{error}</span>
          <button
            type="button"
            className={styles.retryBtn}
            onClick={refetch}
          >
            <RefreshCw size={12} className={styles.retryIcon} />
            Retry
          </button>
        </div>
      )}

      {isLoading && (
        <div className={styles.emptyState} aria-busy="true">
          <Spinner size="md" />
          <span className={styles.loadingText}>
            Loading attached knowledge sources...
          </span>
        </div>
      )}

      {!isLoading && sources.length === 0 && !error && (
        <div className={styles.emptyState}>
          <Info size={24} className={styles.emptyIcon} />
          <span className={styles.emptyTitle}>No Attached Sources</span>
          <p className={styles.emptyDescription}>
            This article does not currently link to supporting architecture docs, external RFCs,
            incident postmortems, or verified vendor references.
          </p>
        </div>
      )}

      {!isLoading && sources.length > 0 && (
        <div className={styles.sourceGrid} role="list" aria-label="Attached knowledge sources">
          {sources.map((source) => {
            const referenceText = referenceTextMap.get(source.id);
            const isThisDetaching = isDetaching && detachingSourceId === source.id;

            return (
              <article
                key={source.id}
                className={styles.sourceCard}
                role="listitem"
                aria-label={`Source: ${source.name}`}
              >
                <div className={styles.cardHeader}>
                  <div className={styles.cardTitleGroup}>
                    <span className={styles.sourceName}>{source.name}</span>
                    <div className={styles.cardBadges}>
                      <Badge
                        variant={getSourceTypeBadgeVariant(source.sourceType)}
                        size="sm"
                      >
                        {normalizeSourceType(source.sourceType)}
                      </Badge>
                      <Badge
                        variant={getTrustLevelBadgeVariant(source.trustLevel)}
                        size="sm"
                      >
                        {normalizeTrustLevel(source.trustLevel)}
                      </Badge>
                      {source.isApproved && (
                        <Badge variant="success" size="sm">
                          Approved
                        </Badge>
                      )}
                    </div>
                  </div>

                  {canManage && (
                    <div className={styles.cardActions}>
                      <button
                        type="button"
                        className={styles.detachButton}
                        onClick={() => handleDetach(source.id)}
                        disabled={isThisDetaching}
                        title="Detach source from article"
                        aria-label={`Detach ${source.name} from article`}
                      >
                        {isThisDetaching ? (
                          <Spinner size="sm" />
                        ) : (
                          <Trash2 size={15} />
                        )}
                      </button>
                    </div>
                  )}
                </div>

                {source.description && (
                  <p className={styles.sourceDescription}>{source.description}</p>
                )}

                {source.url && (
                  <div className={styles.sourceUrlRow}>
                    <a
                      href={source.url}
                      target="_blank"
                      rel="noopener noreferrer"
                      className={styles.sourceUrl}
                      title={`External reference: ${source.url}`}
                    >
                      <span>{source.url}</span>
                      <ExternalLink size={12} aria-hidden="true" />
                    </a>
                  </div>
                )}

                {referenceText && (
                  <div
                    className={styles.citationBox}
                    aria-label={`Citation reference: ${referenceText}`}
                  >
                    <Quote size={12} className={styles.citationIcon} aria-hidden="true" />
                    <span className={styles.citationText}>
                      Citation: {referenceText}
                    </span>
                  </div>
                )}
              </article>
            );
          })}
        </div>
      )}

      {/* Attach Source Modal */}
      <AttachSourceModal
        isOpen={isAttachModalOpen}
        articleTitle={articleTitle}
        excludeSourceIds={attachedSourceIds}
        isSubmitting={isAttaching}
        apiError={actionError}
        onClose={handleCloseAttachModal}
        onConfirm={handleConfirmAttach}
      />
    </section>
  );
};
