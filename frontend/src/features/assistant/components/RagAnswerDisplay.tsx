import React from 'react';
import {
  Sparkles,
  ShieldCheck,
  AlertTriangle,
  Cpu,
  BookOpen,
} from 'lucide-react';
import { resolveSynthesisConfidence, formatIsoTime } from '../utils/assistantFormatters';
import { EvidenceCard } from './EvidenceCard';
import type { RagResponseDto } from '../types/assistantTypes';
import styles from './RagAnswerDisplay.module.css';

export interface RagAnswerDisplayProps {
  response: RagResponseDto;
  activeQuestion?: string | null;
}

export const RagAnswerDisplay: React.FC<RagAnswerDisplayProps> = ({
  response,
  activeQuestion,
}) => {
  const confidence = resolveSynthesisConfidence(response);
  const displayQuestion = activeQuestion || response.question;

  // Split answer content by markdown code fences to render code blocks cleanly
  const renderFormattedAnswer = (text: string) => {
    if (!text.includes('```')) {
      const paragraphs = text.split('\n\n').filter((p) => p.trim());
      return paragraphs.map((para, i) => (
        <p key={i} className={styles.answerParagraph}>
          {para}
        </p>
      ));
    }

    const segments = text.split(/(```[\s\S]*?```)/g);
    return segments.map((seg, idx) => {
      if (seg.startsWith('```') && seg.endsWith('```')) {
        const cleaned = seg.slice(3, -3).replace(/^[a-zA-Z0-9_-]+\n/, '');
        return (
          <pre key={idx} className={styles.codeBlock}>
            <code>{cleaned.trim()}</code>
          </pre>
        );
      }
      const paragraphs = seg.split('\n\n').filter((p) => p.trim());
      return (
        <React.Fragment key={idx}>
          {paragraphs.map((para, pIdx) => (
            <p key={pIdx} className={styles.answerParagraph}>
              {para}
            </p>
          ))}
        </React.Fragment>
      );
    });
  };

  return (
    <div className={styles.container}>
      <div className={styles.questionHeader}>
        <div className={styles.questionMeta}>
          <span className={styles.questionTag}>
            <Sparkles size={13} /> Synthesized Knowledge Answer
          </span>
          <span className={styles.timestamp}>
            Answered: {formatIsoTime(response.answeredAt)}
          </span>
        </div>
        <h3 className={styles.questionText}>{displayQuestion}</h3>
      </div>

      <div className={styles.metricsBar}>
        <div className={styles.metricItem}>
          <span className={styles.metricLabel}>Grounding Confidence:</span>
          <span className={styles.metricValue}>{confidence.formatted}</span>
        </div>

        <div className={styles.metricItem}>
          <span className={styles.metricLabel}>Model:</span>
          <span className={styles.metricValue}>
            <Cpu size={12} /> {response.modelUsed || 'Deterministic'}
          </span>
        </div>

        <div>
          {response.isSufficientEvidence ? (
            <span className={styles.groundedBadgeSuccess}>
              <ShieldCheck size={12} /> Sufficiently Grounded
            </span>
          ) : (
            <span className={styles.groundedBadgeWarning}>
              <AlertTriangle size={12} /> Tentative / Low Evidence
            </span>
          )}
        </div>
      </div>

      <p className={styles.confidenceDisclaimer}>{confidence.disclaimer}</p>

      {!response.isSufficientEvidence && (
        <div className={styles.warningBanner} role="note">
          <AlertTriangle size={16} />
          <span>
            Retrieval evidence fell below the sufficiency threshold. The synthesized response may rely on fallback heuristic guidance. Verify against source citations below.
          </span>
        </div>
      )}

      <div className={styles.answerBody}>
        {renderFormattedAnswer(response.answer)}
      </div>

      <div className={styles.evidenceSection}>
        <div className={styles.evidenceHeader}>
          <h4 className={styles.evidenceTitle}>
            <BookOpen size={16} />
            <span>Grounded Evidence & Citations</span>
          </h4>
          <span className={styles.evidenceCountBadge}>
            {response.evidence.length} cited {response.evidence.length === 1 ? 'item' : 'items'}
          </span>
        </div>

        {response.evidence.length === 0 ? (
          <div className={styles.noEvidenceMessage}>
            No specific document chunks or knowledge articles were matched for citation in this answer.
          </div>
        ) : (
          <div className={styles.evidenceGrid}>
            {response.evidence.map((ev, index) => (
              <EvidenceCard key={ev.citationNumber || index} evidence={ev} />
            ))}
          </div>
        )}
      </div>
    </div>
  );
};
