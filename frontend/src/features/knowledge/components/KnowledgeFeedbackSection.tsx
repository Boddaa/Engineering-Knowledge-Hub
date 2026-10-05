import React, { useState } from 'react';
import { ThumbsUp, ThumbsDown, CheckCircle2, AlertCircle, MessageSquare } from 'lucide-react';
import { Button } from '@/components/ui/Button';
import { Input } from '@/components/ui/Input';
import { useKnowledgeFeedback } from '../hooks/useKnowledgeFeedback';
import type { FeedbackType, KnowledgeStatus } from '../types/knowledgeTypes';
import { normalizeKnowledgeStatus } from '../utils/knowledgeFormatters';
import styles from './KnowledgeFeedbackSection.module.css';

export interface KnowledgeFeedbackSectionProps {
  articleId: string;
  articleStatus: KnowledgeStatus | number | undefined | null;
}

const UUID_REGEX = /^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$/i;

export const KnowledgeFeedbackSection: React.FC<KnowledgeFeedbackSectionProps> = ({
  articleId,
  articleStatus,
}) => {
  const normalizedStatus = normalizeKnowledgeStatus(articleStatus);
  const isPublished = normalizedStatus === 'Published';

  const {
    feedbackList,
    isSubmitting,
    submitError,
    submitSuccess,
    hasSubmitted,
    submitFeedback,
  } = useKnowledgeFeedback(articleId);

  const [selectedType, setSelectedType] = useState<FeedbackType | null>(null);
  const [userId, setUserId] = useState('');
  const [comment, setComment] = useState('');
  const [validationError, setValidationError] = useState<string | null>(null);

  // Helpfulness counts
  const helpfulCount = feedbackList.filter(
    (f) => f.feedbackType === 'Helpful' || f.feedbackType === 1
  ).length;
  const notHelpfulCount = feedbackList.filter(
    (f) => f.feedbackType === 'NotHelpful' || f.feedbackType === 2
  ).length;

  const handleSelectRating = (type: FeedbackType) => {
    setSelectedType(type);
    setValidationError(null);
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();

    if (!selectedType) {
      setValidationError('Please select whether this article was helpful.');
      return;
    }

    const trimmedUserId = userId.trim();
    if (!trimmedUserId) {
      setValidationError('User ID is required.');
      return;
    }

    if (!UUID_REGEX.test(trimmedUserId)) {
      setValidationError('User ID must be a valid UUID.');
      return;
    }

    setValidationError(null);
    const numericType = selectedType === 'Helpful' ? 1 : 2;

    await submitFeedback({
      userId: trimmedUserId,
      feedbackType: numericType,
      comment: comment.trim() ? comment.trim() : undefined,
    });
  };

  return (
    <section className={styles.feedbackCard} aria-label="Knowledge article feedback">
      <div className={styles.header}>
        <div className={styles.titleGroup}>
          <MessageSquare size={16} aria-hidden="true" />
          <h3 className={styles.title}>Was this article helpful?</h3>
        </div>

        {feedbackList.length > 0 && (
          <span className={styles.feedbackSummary}>
            {helpfulCount} found helpful · {notHelpfulCount} found unhelpful
          </span>
        )}
      </div>

      {!isPublished ? (
        <p className={styles.statusNotice}>
          Helpfulness feedback is only enabled for published knowledge articles.
        </p>
      ) : submitSuccess ? (
        <div className={styles.successBanner} role="status">
          <CheckCircle2 size={16} />
          <span>Thank you for your feedback! It helps improve this engineering knowledge.</span>
        </div>
      ) : (
        <>
          <div className={styles.ratingButtons} role="group" aria-label="Article helpfulness rating">
            <button
              type="button"
              className={`${styles.ratingButton} ${
                selectedType === 'Helpful' ? styles.isSelected : ''
              }`}
              onClick={() => handleSelectRating('Helpful')}
              disabled={isSubmitting || hasSubmitted}
              aria-pressed={selectedType === 'Helpful'}
            >
              <ThumbsUp size={14} aria-hidden="true" />
              <span>Yes</span>
            </button>

            <button
              type="button"
              className={`${styles.ratingButton} ${
                selectedType === 'NotHelpful' ? styles.isNotHelpfulSelected : ''
              }`}
              onClick={() => handleSelectRating('NotHelpful')}
              disabled={isSubmitting || hasSubmitted}
              aria-pressed={selectedType === 'NotHelpful'}
            >
              <ThumbsDown size={14} aria-hidden="true" />
              <span>No</span>
            </button>
          </div>

          {selectedType && (
            <form className={styles.formBody} onSubmit={handleSubmit}>
              {(validationError || submitError) && (
                <div className={styles.errorBanner} role="alert">
                  <AlertCircle size={15} />
                  <span>{validationError || submitError}</span>
                </div>
              )}

              <div className={styles.fieldGroup}>
                <label htmlFor="feedbackUserId" className={styles.fieldLabel}>
                  Your User ID (UUID) <span className={styles.requiredStar}>*</span>
                </label>
                <Input
                  id="feedbackUserId"
                  value={userId}
                  onChange={(e) => {
                    setUserId(e.target.value);
                    setValidationError(null);
                  }}
                  placeholder="e.g. 550e8400-e29b-41d4-a716-446655440000"
                  disabled={isSubmitting}
                  required
                />
                <span className={styles.fieldHint}>
                  Required by the backend feedback audit service
                </span>
              </div>

              <div className={styles.fieldGroup}>
                <label htmlFor="feedbackComment" className={styles.fieldLabel}>
                  Optional comment
                </label>
                <textarea
                  id="feedbackComment"
                  rows={2}
                  value={comment}
                  onChange={(e) => setComment(e.target.value)}
                  placeholder="What could be improved? Any missing steps or outdated parameters?"
                  disabled={isSubmitting}
                  className={styles.commentTextarea}
                />
              </div>

              <div className={styles.actionsBar}>
                <Button
                  type="submit"
                  variant="primary"
                  size="sm"
                  disabled={isSubmitting}
                  isLoading={isSubmitting}
                >
                  {isSubmitting ? 'Submitting...' : 'Submit Feedback'}
                </Button>
              </div>
            </form>
          )}
        </>
      )}
    </section>
  );
};
