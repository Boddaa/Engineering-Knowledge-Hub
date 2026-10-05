import React, { useState } from 'react';
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
  Edit3,
} from 'lucide-react';
import { PageShell } from '@/components/layout/PageShell';
import { Button } from '@/components/ui/Button';
import { Badge } from '@/components/ui/Badge';
import { Skeleton } from '@/components/ui/Skeleton';
import { EmptyState } from '@/components/ui/EmptyState';
import { ErrorState } from '@/components/ui/ErrorState';
import {
  useKnowledgeArticle,
  useKnowledgeVersions,
  useKnowledgeAudit,
  useKnowledgeGovernance,
  GovernanceActionBar,
  GovernanceConfirmModal,
  KnowledgeVersionsList,
  KnowledgeAuditTrail,
  KnowledgeFeedbackSection,
  KnowledgeLinkedIncidents,
  type GovernanceActionType,
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

  const {
    versions,
    isLoading: isLoadingVersions,
    error: versionsError,
    refetch: refetchVersions,
  } = useKnowledgeVersions(id);

  const {
    auditLogs,
    isLoading: isLoadingAudit,
    error: auditError,
    refetch: refetchAudit,
  } = useKnowledgeAudit(id);

  const {
    isSubmitting: isGovernanceSubmitting,
    executingAction,
    actionError,
    successFeedback,
    submitReview,
    approve,
    reject,
    deprecate,
    archive,
    clearActionError,
    clearSuccessFeedback,
  } = useKnowledgeGovernance();

  const [activeModalAction, setActiveModalAction] = useState<GovernanceActionType | null>(null);

  const handleOpenAction = (action: GovernanceActionType) => {
    clearActionError();
    setActiveModalAction(action);
  };

  const handleCloseModal = () => {
    if (!isGovernanceSubmitting) {
      clearActionError();
      setActiveModalAction(null);
    }
  };

  const handleConfirmAction = async (payload: {
    reviewerId?: string;
    submitterId?: string;
    actorId?: string;
    changeSummary?: string;
  }) => {
    if (!article || !activeModalAction) return;

    let success = false;
    switch (activeModalAction) {
      case 'submit-review':
        success = await submitReview(article.id, payload.submitterId);
        break;
      case 'approve':
        if (payload.reviewerId) {
          success = await approve(article.id, payload.reviewerId, payload.changeSummary);
        }
        break;
      case 'reject':
        if (payload.reviewerId) {
          success = await reject(article.id, payload.reviewerId);
        }
        break;
      case 'deprecate':
        success = await deprecate(article.id, payload.actorId);
        break;
      case 'archive':
        success = await archive(article.id, payload.actorId);
        break;
    }

    if (success) {
      setActiveModalAction(null);
      retry();
      refetchVersions();
      refetchAudit();
    }
  };

  if (isLoading) {
    return (
      <PageShell title="Loading Article...">
        <div className={styles.skeletonDetail} aria-busy="true" aria-label="Loading knowledge article details">
          <Skeleton variant="text" width={140} height={32} />
          <Skeleton variant="text" width="75%" height={36} />
          <div className={styles.skeletonBadges}>
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
            <span className={styles.slugBadge}>
              /{article.slug}
            </span>
          )}
        </div>
      }
      headerActions={
        <div className={styles.headerActionGroup}>
          <Button
            variant="subtle"
            size="md"
            icon={<ArrowLeft size={15} />}
            onClick={() => navigate('/knowledge')}
            aria-label="Back to knowledge repository"
          >
            Back to Knowledge
          </Button>
          <Button
            variant="primary"
            size="md"
            icon={<Edit3 size={15} />}
            onClick={() => navigate(`/knowledge/${article.id}/edit`)}
            aria-label="Edit knowledge article"
            disabled={normalizedStatus === 'Archived'}
            title={normalizedStatus === 'Archived' ? 'Archived articles cannot be edited.' : undefined}
          >
            Edit
          </Button>
        </div>
      }
    >
      <div className={styles.container}>
        {/* Governance Lifecycle Action Bar */}
        <GovernanceActionBar
          status={article.status}
          verificationStatus={article.verificationStatus}
          isSubmitting={isGovernanceSubmitting}
          executingAction={executingAction}
          onOpenAction={handleOpenAction}
          successFeedback={successFeedback}
          onDismissFeedback={clearSuccessFeedback}
        />
        {/* Metadata Grid */}
        <section className={styles.metaGrid} aria-label="Article metadata">
          <div className={styles.metaItem}>
            <span className={styles.metaLabel}>Created</span>
            <span className={styles.metaValue}>
              <Calendar size={13} className={styles.metaIcon} />
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
              <User size={13} className={styles.metaIcon} />
              <span className="text-code">{article.authorId.substring(0, 8)}...</span>
            </span>
          </div>

          {article.reviewerId && (
            <div className={styles.metaItem}>
              <span className={styles.metaLabel}>Reviewer</span>
              <span className={styles.metaValue} title={article.reviewerId}>
                <CheckCircle size={13} className={styles.metaIcon} />
                <span className="text-code">{article.reviewerId.substring(0, 8)}...</span>
              </span>
            </div>
          )}
        </section>

        {/* Summary Box */}
        {article.summary && (
          <section className={styles.summaryBox} aria-label="Executive summary">
            <strong className={styles.summaryTitle}>
              Executive Summary
            </strong>
            <p className={styles.summaryText}>{article.summary}</p>
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
            <h3 className={styles.subSectionHeading}>
              Linked Taxonomy & Architecture Relationships
            </h3>

            {article.tagIds && article.tagIds.length > 0 && (
              <div className={styles.relationshipGroup}>
                <span className="text-caption">
                  <Tag size={12} className={styles.relationshipIcon} />
                  Tags ({article.tagIds.length})
                </span>
                <div className={styles.idList}>
                  {article.tagIds.map((tagId) => (
                    <span key={tagId} className={styles.idTag}>
                      {tagId}
                    </span>
                  ))}
                </div>
              </div>
            )}

            {article.productIds && article.productIds.length > 0 && (
              <div className={styles.relationshipGroup}>
                <span className="text-caption">
                  <Layers size={12} className={styles.relationshipIcon} />
                  Products ({article.productIds.length})
                </span>
                <div className={styles.idList}>
                  {article.productIds.map((prodId) => (
                    <span key={prodId} className={styles.idTag}>
                      {prodId}
                    </span>
                  ))}
                </div>
              </div>
            )}

            {article.technologyIds && article.technologyIds.length > 0 && (
              <div className={styles.relationshipGroup}>
                <span className="text-caption">
                  <Cpu size={12} className={styles.relationshipIcon} />
                  Technologies ({article.technologyIds.length})
                </span>
                <div className={styles.idList}>
                  {article.technologyIds.map((techId) => (
                    <span key={techId} className={styles.idTag}>
                      {techId}
                    </span>
                  ))}
                </div>
              </div>
            )}

            {article.sourceIds && article.sourceIds.length > 0 && (
              <div className={styles.relationshipGroup}>
                <span className="text-caption">
                  <BookOpen size={12} className={styles.relationshipIcon} />
                  Sources ({article.sourceIds.length})
                </span>
                <div className={styles.idList}>
                  {article.sourceIds.map((srcId) => (
                    <span key={srcId} className={styles.idTag}>
                      {srcId}
                    </span>
                  ))}
                </div>
              </div>
            )}
          </section>
        )}

        {/* Linked Operational Incidents Section */}
        <KnowledgeLinkedIncidents articleId={article.id} />

        {/* Article Helpfulness Feedback Section */}
        <KnowledgeFeedbackSection
          articleId={article.id}
          articleStatus={article.status}
        />

        {/* Versions History Section */}
        <KnowledgeVersionsList
          versions={versions}
          isLoading={isLoadingVersions}
          error={versionsError}

          onRetry={refetchVersions}
        />

        {/* Governance Audit Trail Section */}
        <KnowledgeAuditTrail
          auditLogs={auditLogs}
          isLoading={isLoadingAudit}
          error={auditError}
          onRetry={refetchAudit}
        />

        {/* Governance Lifecycle Confirmation Modal */}
        <GovernanceConfirmModal
          isOpen={activeModalAction !== null}
          action={activeModalAction}
          articleTitle={article.title}
          isSubmitting={isGovernanceSubmitting}
          apiError={actionError}
          onClose={handleCloseModal}
          onConfirm={handleConfirmAction}
        />
      </div>
    </PageShell>
  );
};
