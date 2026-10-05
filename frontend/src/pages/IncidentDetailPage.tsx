import React, { useState } from 'react';
import { useParams, useNavigate, Link } from 'react-router-dom';
import {
  ArrowLeft,
  Calendar,
  User,
  CheckCircle2,
  AlertTriangle,
  Play,
  Check,
  Lock,
  BookOpen,
  Link2,
  Trash2,
  ExternalLink,
  X,
} from 'lucide-react';
import { PageShell } from '@/components/layout/PageShell';
import { Button } from '@/components/ui/Button';
import { Badge } from '@/components/ui/Badge';
import { Skeleton } from '@/components/ui/Skeleton';
import { EmptyState } from '@/components/ui/EmptyState';
import { ErrorState } from '@/components/ui/ErrorState';
import {
  useIncident,
  useIncidentActions,
  IncidentResolveModal,
  IncidentProposeArticleModal,
  IncidentLinkArticleModal,
  normalizeIncidentStatus,
  normalizeIncidentSeverity,
  normalizeArticleRelationshipType,
  getIncidentStatusBadgeVariant,
  getIncidentSeverityBadgeVariant,
  getRelationshipTypeBadgeVariant,
  formatDate,
  type ResolveIncidentRequest,
  type LinkArticleRequest,
  type CreateKnowledgeFromIncidentRequest,
} from '@/features/incidents';
import styles from './IncidentDetailPage.module.css';

export const IncidentDetailPage: React.FC = () => {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();

  const { incident, isLoading, error, isNotFound, retry, setIncident } = useIncident(id);

  const {
    isSubmitting,
    actionError,
    successFeedback,
    startInvestigation,
    resolveIncident,
    closeIncident,
    linkArticle,
    unlinkArticle,
    proposeArticle,
    clearActionError,
    clearSuccessFeedback,
  } = useIncidentActions();

  // Modal dialog visibility state
  const [isResolveModalOpen, setIsResolveModalOpen] = useState(false);
  const [isProposeModalOpen, setIsProposeModalOpen] = useState(false);
  const [isLinkModalOpen, setIsLinkModalOpen] = useState(false);

  if (isLoading) {
    return (
      <PageShell
        title="Loading incident..."
        description="Retrieving operational incident details and verified knowledge linkages."
      >
        <div className={styles.container}>
          <Skeleton width="120px" height="18px" />
          <Skeleton width="100%" height="160px" />
          <Skeleton width="100%" height="120px" />
          <Skeleton width="100%" height="80px" />
        </div>
      </PageShell>
    );
  }

  if (isNotFound || !incident) {
    return (
      <PageShell title="Incident Not Found" description="The requested incident could not be found.">
        <EmptyState
          icon={<AlertTriangle size={36} />}
          title="Incident not found"
          description={`No operational incident exists with ID "${id}".`}
          action={
            <Button
              variant="subtle"
              icon={<ArrowLeft size={16} />}
              onClick={() => navigate('/incidents')}
            >
              Return to Incidents
            </Button>
          }
        />
      </PageShell>
    );
  }

  if (error) {
    return (
      <PageShell title="Incident Error" description="Unable to load incident record.">
        <ErrorState
          title="Failed to load incident"
          message={error}
          onRetry={retry}
        />
      </PageShell>
    );
  }

  const normalizedStatus = normalizeIncidentStatus(incident.status);
  const normalizedSeverity = normalizeIncidentSeverity(incident.severity);

  // Lifecycle Action Handlers
  const handleStartInvestigation = async () => {
    clearActionError();
    const updated = await startInvestigation(incident.id);
    if (updated) {
      setIncident(updated);
    }
  };

  const handleConfirmResolve = async (request: ResolveIncidentRequest) => {
    clearActionError();
    const updated = await resolveIncident(incident.id, request);
    if (updated) {
      setIncident(updated);
      setIsResolveModalOpen(false);
    }
  };

  const handleConfirmClose = async () => {
    clearActionError();
    const updated = await closeIncident(incident.id);
    if (updated) {
      setIncident(updated);
    }
  };

  const handleConfirmLink = async (request: LinkArticleRequest) => {
    clearActionError();
    const updated = await linkArticle(incident.id, request);
    if (updated) {
      setIncident(updated);
      setIsLinkModalOpen(false);
    }
  };

  const handleUnlink = async (articleId: string) => {
    clearActionError();
    const updated = await unlinkArticle(incident.id, articleId);
    if (updated) {
      setIncident(updated);
    }
  };

  const handleConfirmPropose = async (request: CreateKnowledgeFromIncidentRequest) => {
    clearActionError();
    const createdArticle = await proposeArticle(incident.id, request);
    if (createdArticle) {
      setIsProposeModalOpen(false);
      navigate(`/knowledge/${createdArticle.id}`);
    }
  };

  return (
    <PageShell
      title={`Incident: ${incident.title}`}
      description={`Operational incident ${incident.id}`}
    >
      <div className={styles.container}>
        {/* Navigation Bar */}
        <div className={styles.backBar}>
          <button
            type="button"
            className={styles.backLink}
            onClick={() => navigate('/incidents')}
            aria-label="Back to incidents list"
          >
            <ArrowLeft size={16} />
            <span>Back to Incidents</span>
          </button>
        </div>

        {/* Action Success / Error Banners */}
        {actionError && (
          <div className={styles.actionErrorBanner} role="alert">
            <AlertTriangle size={16} />
            <span>{actionError}</span>
            <button
              type="button"
              className={styles.bannerClose}
              onClick={clearActionError}
              aria-label="Dismiss error"
            >
              <X size={14} />
            </button>
          </div>
        )}

        {successFeedback && (
          <div className={styles.actionSuccessBanner} role="status">
            <CheckCircle2 size={16} />
            <span>{successFeedback}</span>
            <button
              type="button"
              className={styles.bannerClose}
              onClick={clearSuccessFeedback}
              aria-label="Dismiss notification"
            >
              <X size={14} />
            </button>
          </div>
        )}

        {/* Lifecycle Actions Toolbar */}
        <div className={styles.actionBar} role="toolbar" aria-label="Incident lifecycle actions">
          {/* Open -> Start Investigation */}
          {normalizedStatus === 'Open' && (
            <Button
              variant="subtle"
              size="sm"
              icon={<Play size={14} />}
              onClick={handleStartInvestigation}
              disabled={isSubmitting}
            >
              Start Investigation
            </Button>
          )}

          {/* Open or Investigating -> Resolve */}
          {(normalizedStatus === 'Open' || normalizedStatus === 'Investigating') && (
            <Button
              variant="primary"
              size="sm"
              icon={<Check size={14} />}
              onClick={() => {
                clearActionError();
                setIsResolveModalOpen(true);
              }}
              disabled={isSubmitting}
            >
              Resolve Incident
            </Button>
          )}

          {/* Resolved -> Close */}
          {normalizedStatus === 'Resolved' && (
            <Button
              variant="subtle"
              size="sm"
              icon={<Lock size={14} />}
              onClick={handleConfirmClose}
              disabled={isSubmitting}
            >
              Close Incident
            </Button>
          )}

          {/* Resolved or Closed -> Propose Knowledge Article */}
          {(normalizedStatus === 'Resolved' || normalizedStatus === 'Closed') && (
            <Button
              variant="subtle"
              size="sm"
              icon={<BookOpen size={14} />}
              onClick={() => {
                clearActionError();
                setIsProposeModalOpen(true);
              }}
              disabled={isSubmitting}
            >
              Propose Knowledge Article
            </Button>
          )}

          {/* Link Article Action */}
          <Button
            variant="ghost"
            size="sm"
            icon={<Link2 size={14} />}
            onClick={() => {
              clearActionError();
              setIsLinkModalOpen(true);
            }}
            disabled={isSubmitting}
          >
            Link Article
          </Button>
        </div>

        {/* Overview Header Card */}
        <section className={styles.overviewCard} aria-label="Incident summary">
          <div className={styles.titleRow}>
            <h1 className={styles.incidentTitle}>{incident.title}</h1>
            <div className={styles.badgeGroup}>
              <Badge
                variant={getIncidentSeverityBadgeVariant(incident.severity)}
                icon={<AlertTriangle size={13} aria-hidden="true" />}
              >
                {normalizedSeverity} Severity
              </Badge>
              <Badge
                variant={getIncidentStatusBadgeVariant(incident.status)}
                dot
              >
                {normalizedStatus}
              </Badge>
            </div>
          </div>

          <div className={styles.metaGrid}>
            <div className={styles.metaItem}>
              <span className={styles.metaLabel}>Created At</span>
              <span className={styles.metaValue}>
                <Calendar size={13} aria-hidden="true" />
                {formatDate(incident.createdAt)}
              </span>
            </div>

            <div className={styles.metaItem}>
              <span className={styles.metaLabel}>Created By</span>
              <span className={styles.metaValue} title={incident.createdById}>
                <User size={13} aria-hidden="true" />
                <span className={styles.mono}>{incident.createdById.substring(0, 8)}...</span>
              </span>
            </div>

            {incident.resolvedAt && (
              <div className={styles.metaItem}>
                <span className={styles.metaLabel}>Resolved At</span>
                <span className={styles.metaValue}>
                  <CheckCircle2 size={13} aria-hidden="true" />
                  {formatDate(incident.resolvedAt)}
                </span>
              </div>
            )}

            {incident.resolvedById && (
              <div className={styles.metaItem}>
                <span className={styles.metaLabel}>Resolved By</span>
                <span className={styles.metaValue} title={incident.resolvedById}>
                  <User size={13} aria-hidden="true" />
                  <span className={styles.mono}>{incident.resolvedById.substring(0, 8)}...</span>
                </span>
              </div>
            )}
          </div>
        </section>

        {/* Problem Description */}
        <section className={styles.sectionBox} aria-label="Problem description">
          <h2 className={styles.sectionHeading}>Problem Description</h2>
          <p className={styles.sectionText}>{incident.problemDescription}</p>
        </section>

        {/* Symptoms (only if present) */}
        {incident.symptoms && (
          <section className={styles.sectionBox} aria-label="Symptoms">
            <h2 className={styles.sectionHeading}>Symptoms & Monitoring Signals</h2>
            <p className={styles.sectionText}>{incident.symptoms}</p>
          </section>
        )}

        {/* Root Cause (only if present) */}
        {incident.rootCause && (
          <section className={styles.sectionBox} aria-label="Root cause analysis">
            <h2 className={styles.sectionHeading}>Root Cause Analysis</h2>
            <p className={styles.sectionText}>{incident.rootCause}</p>
          </section>
        )}

        {/* Resolution (only if present) */}
        {incident.resolution && (
          <section className={styles.sectionBox} aria-label="Resolution details">
            <h2 className={styles.sectionHeading}>Applied Resolution</h2>
            <p className={styles.sectionText}>{incident.resolution}</p>
          </section>
        )}

        {/* Related Knowledge Articles */}
        <section className={styles.relatedSection} aria-label="Related knowledge articles">
          <div className={styles.relatedHeader}>
            <h2 className={styles.sectionHeading}>
              <BookOpen size={16} aria-hidden="true" />
              Related Knowledge Articles ({incident.articleLinks?.length || 0})
            </h2>
            <Button
              variant="subtle"
              size="sm"
              icon={<Link2 size={14} />}
              onClick={() => {
                clearActionError();
                setIsLinkModalOpen(true);
              }}
              disabled={isSubmitting}
            >
              Add Link
            </Button>
          </div>

          {incident.articleLinks && incident.articleLinks.length > 0 ? (
            <div className={styles.linkList}>
              {incident.articleLinks.map((link) => {
                const relSemantic = normalizeArticleRelationshipType(link.relationshipType);

                return (
                  <div key={link.articleId} className={styles.linkItem}>
                    <Link
                      to={`/knowledge/${link.articleId}`}
                      className={styles.linkInfo}
                      aria-label={`View linked knowledge article: ${link.articleId}`}
                    >
                      <Badge
                        variant={getRelationshipTypeBadgeVariant(link.relationshipType)}
                        size="sm"
                      >
                        {relSemantic}
                      </Badge>
                      <span className={styles.linkTitle}>Article: {link.articleId}</span>
                      <ExternalLink size={13} aria-hidden="true" />
                    </Link>

                    <button
                      type="button"
                      className={styles.unlinkButton}
                      onClick={() => handleUnlink(link.articleId)}
                      disabled={isSubmitting}
                      title="Unlink article"
                      aria-label={`Unlink article ${link.articleId}`}
                    >
                      <Trash2 size={14} />
                    </button>
                  </div>
                );
              })}
            </div>
          ) : (
            <p className={styles.emptyLinksText}>
              No knowledge articles currently linked to this incident. Use &quot;Add Link&quot; to associate
              troubleshooting guides, runbooks, or postmortems.
            </p>
          )}
        </section>

        {/* Action Modals */}
        <IncidentResolveModal
          isOpen={isResolveModalOpen}
          incidentTitle={incident.title}
          isSubmitting={isSubmitting}
          apiError={actionError}
          onClose={() => setIsResolveModalOpen(false)}
          onConfirm={handleConfirmResolve}
        />

        <IncidentProposeArticleModal
          isOpen={isProposeModalOpen}
          incidentTitle={incident.title}
          isSubmitting={isSubmitting}
          apiError={actionError}
          onClose={() => setIsProposeModalOpen(false)}
          onConfirm={handleConfirmPropose}
        />

        <IncidentLinkArticleModal
          isOpen={isLinkModalOpen}
          incidentTitle={incident.title}
          isSubmitting={isSubmitting}
          apiError={actionError}
          onClose={() => setIsLinkModalOpen(false)}
          onConfirm={handleConfirmLink}
        />
      </div>
    </PageShell>
  );
};
