import React from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import {
  ArrowLeft,
  Calendar,
  User,
  CheckCircle,
  FileQuestion,
  Tag,
  Layers,
  Cpu,
  BookOpen,
} from 'lucide-react';
import { PageShell } from '@/components/layout/PageShell';
import { Button } from '@/components/ui/Button';
import { Badge } from '@/components/ui/Badge';
import { Skeleton } from '@/components/ui/Skeleton';
import { EmptyState } from '@/components/ui/EmptyState';
import { ErrorState } from '@/components/ui/ErrorState';
import {
  useKnowledgeArticle,
  normalizeKnowledgeStatus,
  normalizeVerificationStatus,
  getStatusBadgeVariant,
  getVerificationBadgeVariant,
  formatDate,
} from '@/features/knowledge';
import styles from './KnowledgeArticlePage.module.css';

export const KnowledgeArticlePage: React.FC = () => {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();

  const { article, isLoading, error, isNotFound, retry } = useKnowledgeArticle(id);

  if (isLoading) {
    return (
      <PageShell title="Loading Article...">
        <div className={styles.skeletonDetail} aria-busy="true" aria-label="Loading knowledge article details">
          <Skeleton variant="text" width={140} height={32} />
          <Skeleton variant="text" width="75%" height={36} />
          <div style={{ display: 'flex', gap: '12px' }}>
            <Skeleton variant="rectangular" width={90} height={24} />
            <Skeleton variant="rectangular" width={90} height={24} />
          </div>
          <Skeleton variant="rectangular" height={100} />
          <Skeleton variant="rectangular" height={320} />
        </div>
      </PageShell>
    );
  }

  if (isNotFound) {
    return (
      <PageShell title="Article Not Found">
        <EmptyState
          icon={<FileQuestion size={36} />}
          title="Knowledge Article Not Found"
          description={`No knowledge article exists with ID "${id}". It may have been moved, archived, or deleted.`}
          action={
            <Button
              variant="primary"
              size="md"
              icon={<ArrowLeft size={16} />}
              onClick={() => navigate('/knowledge')}
            >
              Back to Knowledge
            </Button>
          }
        />
      </PageShell>
    );
  }

  if (error || !article) {
    return (
      <PageShell title="Error Loading Article">
        <ErrorState
          title="Unable to Load Article"
          message={error || 'An error occurred while fetching the knowledge article.'}
          onRetry={retry}
          actionText="Try Again"
        />
      </PageShell>
    );
  }

  const normalizedStatus = normalizeKnowledgeStatus(article.status);
  const normalizedVerification = normalizeVerificationStatus(article.verificationStatus);

  return (
    <PageShell
      title={article.title}
      description={article.summary}
      badge={
        <div className={styles.badgeGroup}>
          <Badge variant={getStatusBadgeVariant(article.status)}>
            {normalizedStatus}
          </Badge>
          <Badge
            variant={getVerificationBadgeVariant(article.verificationStatus)}
            dot
          >
            {normalizedVerification}
          </Badge>
          {article.slug && (
            <span className="text-code" style={{ fontSize: '11px' }}>
              /{article.slug}
            </span>
          )}
        </div>
      }
      headerActions={
        <Button
          variant="subtle"
          size="md"
          icon={<ArrowLeft size={15} />}
          onClick={() => navigate('/knowledge')}
          aria-label="Back to knowledge repository"
        >
          Back to Knowledge
        </Button>
      }
    >
      <div className={styles.container}>
        {/* Metadata Grid */}
        <section className={styles.metaGrid} aria-label="Article metadata">
          <div className={styles.metaItem}>
            <span className={styles.metaLabel}>Created</span>
            <span className={styles.metaValue}>
              <Calendar size={13} style={{ display: 'inline', verticalAlign: 'middle', marginRight: 4 }} />
              {formatDate(article.createdAt)}
            </span>
          </div>

          <div className={styles.metaItem}>
            <span className={styles.metaLabel}>Last Updated</span>
            <span className={styles.metaValue}>
              {formatDate(article.updatedAt)}
            </span>
          </div>

          <div className={styles.metaItem}>
            <span className={styles.metaLabel}>Published</span>
            <span className={styles.metaValue}>
              {formatDate(article.publishedAt)}
            </span>
          </div>

          <div className={styles.metaItem}>
            <span className={styles.metaLabel}>Author</span>
            <span className={styles.metaValue} title={article.authorId}>
              <User size={13} style={{ display: 'inline', verticalAlign: 'middle', marginRight: 4 }} />
              <span className="text-code">{article.authorId.substring(0, 8)}...</span>
            </span>
          </div>

          {article.reviewerId && (
            <div className={styles.metaItem}>
              <span className={styles.metaLabel}>Reviewer</span>
              <span className={styles.metaValue} title={article.reviewerId}>
                <CheckCircle size={13} style={{ display: 'inline', verticalAlign: 'middle', marginRight: 4 }} />
                <span className="text-code">{article.reviewerId.substring(0, 8)}...</span>
              </span>
            </div>
          )}
        </section>

        {/* Summary Box */}
        {article.summary && (
          <section className={styles.summaryBox} aria-label="Executive summary">
            <strong style={{ display: 'block', marginBottom: '4px', fontSize: '13px', color: 'var(--color-accent-primary)' }}>
              Executive Summary
            </strong>
            <p style={{ margin: 0 }}>{article.summary}</p>
          </section>
        )}

        {/* Main Content Body */}
        <section className={styles.contentBox} aria-label="Article content">
          <h2 className={styles.sectionHeading}>Documentation Content</h2>
          <div className={styles.textContent}>{article.content}</div>
        </section>

        {/* Relationships and Taxonomy Identifiers (Metadata only, no extra API calls) */}
        {((article.tagIds && article.tagIds.length > 0) ||
          (article.productIds && article.productIds.length > 0) ||
          (article.technologyIds && article.technologyIds.length > 0) ||
          (article.sourceIds && article.sourceIds.length > 0)) && (
          <section className={styles.relationshipSection} aria-label="Associated metadata and relationships">
            <h3 className={styles.sectionHeading} style={{ fontSize: '14px' }}>
              Linked Taxonomy & Architecture Relationships
            </h3>

            {article.tagIds && article.tagIds.length > 0 && (
              <div style={{ display: 'flex', flexDirection: 'column', gap: '4px' }}>
                <span className="text-caption">
                  <Tag size={12} style={{ display: 'inline', verticalAlign: 'middle', marginRight: 4 }} />
                  Tags ({article.tagIds.length})
                </span>
                <div className={styles.idList}>
                  {article.tagIds.map((tagId) => (
                    <span key={tagId} className="text-code" style={{ padding: '2px 6px', background: 'var(--color-surface-base)', borderRadius: '4px', fontSize: '11px' }}>
                      {tagId}
                    </span>
                  ))}
                </div>
              </div>
            )}

            {article.productIds && article.productIds.length > 0 && (
              <div style={{ display: 'flex', flexDirection: 'column', gap: '4px' }}>
                <span className="text-caption">
                  <Layers size={12} style={{ display: 'inline', verticalAlign: 'middle', marginRight: 4 }} />
                  Products ({article.productIds.length})
                </span>
                <div className={styles.idList}>
                  {article.productIds.map((prodId) => (
                    <span key={prodId} className="text-code" style={{ padding: '2px 6px', background: 'var(--color-surface-base)', borderRadius: '4px', fontSize: '11px' }}>
                      {prodId}
                    </span>
                  ))}
                </div>
              </div>
            )}

            {article.technologyIds && article.technologyIds.length > 0 && (
              <div style={{ display: 'flex', flexDirection: 'column', gap: '4px' }}>
                <span className="text-caption">
                  <Cpu size={12} style={{ display: 'inline', verticalAlign: 'middle', marginRight: 4 }} />
                  Technologies ({article.technologyIds.length})
                </span>
                <div className={styles.idList}>
                  {article.technologyIds.map((techId) => (
                    <span key={techId} className="text-code" style={{ padding: '2px 6px', background: 'var(--color-surface-base)', borderRadius: '4px', fontSize: '11px' }}>
                      {techId}
                    </span>
                  ))}
                </div>
              </div>
            )}

            {article.sourceIds && article.sourceIds.length > 0 && (
              <div style={{ display: 'flex', flexDirection: 'column', gap: '4px' }}>
                <span className="text-caption">
                  <BookOpen size={12} style={{ display: 'inline', verticalAlign: 'middle', marginRight: 4 }} />
                  Sources ({article.sourceIds.length})
                </span>
                <div className={styles.idList}>
                  {article.sourceIds.map((srcId) => (
                    <span key={srcId} className="text-code" style={{ padding: '2px 6px', background: 'var(--color-surface-base)', borderRadius: '4px', fontSize: '11px' }}>
                      {srcId}
                    </span>
                  ))}
                </div>
              </div>
            )}
          </section>
        )}
      </div>
    </PageShell>
  );
};
