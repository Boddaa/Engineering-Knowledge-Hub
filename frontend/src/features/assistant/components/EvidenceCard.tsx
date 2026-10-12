import React from 'react';
import { Link } from 'react-router-dom';
import { ExternalLink, CheckCircle, Globe, FileText, AlertCircle } from 'lucide-react';
import { formatScorePercent, formatEvidenceOrigin } from '../utils/assistantFormatters';
import type { RagEvidenceDto } from '../types/assistantTypes';
import styles from './EvidenceCard.module.css';

export interface EvidenceCardProps {
  evidence: RagEvidenceDto;
}

export const EvidenceCard: React.FC<EvidenceCardProps> = ({ evidence }) => {
  const origin = formatEvidenceOrigin(evidence);

  return (
    <div className={styles.card}>
      <div className={styles.header}>
        <div className={styles.citationMeta}>
          <span className={styles.citationBadge}>
            #{evidence.citationNumber > 0 ? evidence.citationNumber : '—'}
          </span>
          <span className={styles.typeBadge}>{origin.originType}</span>
          {origin.isExternal && (
            <span className={styles.externalBadge} title="External or vendor reference source">
              <Globe size={11} /> External
            </span>
          )}
          {evidence.verificationStatus === 'Verified' && (
            <span className={styles.verifiedBadge} title="Human-verified organizational knowledge">
              <CheckCircle size={11} /> Verified
            </span>
          )}
          {evidence.verificationStatus === 'Unverified' && (
            <span className={styles.externalBadge} title="Draft / Unverified knowledge">
              <AlertCircle size={11} /> Unverified
            </span>
          )}
        </div>

        <div className={styles.scorePill} title="Semantic and keyword relevance match">
          <span>Relevance:</span>
          <strong>{formatScorePercent(evidence.relevanceScore)}</strong>
        </div>
      </div>

      <h4 className={styles.title}>{evidence.title || 'Untitled Evidence'}</h4>

      <p className={styles.excerpt}>{evidence.excerpt}</p>

      <div className={styles.footer}>
        <div className={styles.provenance}>
          <FileText size={12} />
          <span>Source: {origin.provenanceName}</span>
        </div>

        {origin.linkPath ? (
          <Link to={origin.linkPath} className={styles.linkButton} title={origin.linkLabel}>
            <span>{origin.linkLabel}</span>
            <ExternalLink size={11} />
          </Link>
        ) : (
          <span className={styles.noLinkNote}>Direct entity link unavailable</span>
        )}
      </div>
    </div>
  );
};
