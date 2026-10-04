import React from 'react';
import { Link } from 'react-router-dom';
import { ArrowRight, Calendar, Tag } from 'lucide-react';
import type { SearchArticleResultDto } from '../types/knowledgeTypes';
import {
  normalizeKnowledgeStatus,
  normalizeVerificationStatus,
  getStatusBadgeVariant,
  getVerificationBadgeVariant,
  formatDate,
} from '../utils/knowledgeFormatters';
import { Badge } from '@/components/ui/Badge';
import styles from './KnowledgeCard.module.css';

export interface KnowledgeCardProps {
  article: SearchArticleResultDto;
}

export const KnowledgeCard: React.FC<KnowledgeCardProps> = ({ article }) => {
  const normalizedStatus = normalizeKnowledgeStatus(article.status);
  const normalizedVerification = normalizeVerificationStatus(article.verificationStatus);
  const displayDate = formatDate(article.updatedAt || article.createdAt);

  return (
    <Link
      to={`/knowledge/${article.id}`}
      className={styles.card}
      aria-label={`View knowledge article: ${article.title}`}
    >
      <div className={styles.header}>
        <h3 className={styles.title}>{article.title}</h3>
        <div className={styles.badgeGroup}>
          <Badge variant={getStatusBadgeVariant(article.status)} size="sm">
            {normalizedStatus}
          </Badge>
          <Badge
            variant={getVerificationBadgeVariant(article.verificationStatus)}
            size="sm"
            dot
          >
            {normalizedVerification}
          </Badge>
        </div>
      </div>

      <p className={styles.summary}>
        {article.summary || article.excerpt || 'No description provided.'}
      </p>

      <div className={styles.footer}>
        <div className={styles.metaGroup}>
          <div className={styles.metaItem}>
            <Calendar size={13} aria-hidden="true" />
            <span>{displayDate}</span>
          </div>

          {article.tagIds && article.tagIds.length > 0 && (
            <div className={styles.metaItem}>
              <Tag size={13} aria-hidden="true" />
              <span>{article.tagIds.length} tag{article.tagIds.length > 1 ? 's' : ''}</span>
            </div>
          )}

          {article.slug && (
            <span className={styles.slugText}>
              /{article.slug}
            </span>
          )}
        </div>

        <span className={styles.viewAction}>
          Inspect <ArrowRight size={13} aria-hidden="true" />
        </span>
      </div>
    </Link>
  );
};
