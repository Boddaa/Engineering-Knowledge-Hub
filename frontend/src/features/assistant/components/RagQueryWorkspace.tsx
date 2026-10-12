import React, { useState } from 'react';
import {
  Send,
  Sliders,
  ChevronDown,
  ChevronUp,
  AlertCircle,
  HelpCircle,
  Sparkles,
} from 'lucide-react';
import { Button } from '@/components/ui/Button';
import type { SubmitQueryParams } from '../hooks/useRagQuery';
import styles from './RagQueryWorkspace.module.css';

const STARTER_PROMPTS = [
  'How does the ASP.NET Core middleware pipeline handle HTTP requests?',
  'What is the document chunking and token strategy in the knowledge repository?',
  'How are incident postmortems converted into draft knowledge articles?',
  'What governance rules govern the transition from unverified draft to verified publication?',
];

export interface RagQueryWorkspaceProps {
  isSubmitting: boolean;
  apiError: string | null;
  onSubmit: (params: SubmitQueryParams) => void;
}

export const RagQueryWorkspace: React.FC<RagQueryWorkspaceProps> = ({
  isSubmitting,
  apiError,
  onSubmit,
}) => {
  const [question, setQuestion] = useState('');
  const [maxEvidence, setMaxEvidence] = useState(5);
  const [minRelevanceScore, setMinRelevanceScore] = useState(0.25);
  const [showOptions, setShowOptions] = useState(false);
  const [validationError, setValidationError] = useState<string | null>(null);

  const trimmed = question.trim();
  const charCount = question.length;
  const isLengthValid = trimmed.length >= 1 && charCount <= 1000;
  const isLimitValid = maxEvidence >= 1 && maxEvidence <= 20;
  const isScoreValid = minRelevanceScore >= 0.0 && minRelevanceScore <= 1.0;
  const canSubmit = isLengthValid && isLimitValid && isScoreValid && !isSubmitting;

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();

    if (!trimmed) {
      setValidationError('Please enter a question for the assistant.');
      return;
    }

    if (charCount > 1000) {
      setValidationError('Question cannot exceed 1,000 characters.');
      return;
    }

    if (!isLimitValid) {
      setValidationError('Evidence limit must be between 1 and 20.');
      return;
    }

    if (!isScoreValid) {
      setValidationError('Minimum relevance score must be between 0.0 and 1.0.');
      return;
    }

    setValidationError(null);
    onSubmit({
      question: trimmed,
      maxEvidence,
      minRelevanceScore,
    });
  };

  const handleSelectStarter = (promptText: string) => {
    if (isSubmitting) return;
    setQuestion(promptText);
    setValidationError(null);
  };

  return (
    <div className={styles.workspace}>
      <form onSubmit={handleSubmit} className={styles.promptForm}>
        <div className={styles.inputGroup}>
          <div className={styles.labelRow}>
            <label htmlFor="rag-question-input" className={styles.label}>
              <HelpCircle size={14} />
              <span>Ask Knowledge Assistant</span>
            </label>
            <span
              className={`${styles.charCounter} ${
                charCount > 1000 ? styles.charCounterExceeded : ''
              }`}
            >
              {charCount} / 1000 chars
            </span>
          </div>

          <textarea
            id="rag-question-input"
            rows={3}
            className={styles.textarea}
            value={question}
            onChange={(e) => {
              setQuestion(e.target.value);
              setValidationError(null);
            }}
            placeholder="Ask a technical architecture question, incident diagnosis inquiry, or domain query..."
            disabled={isSubmitting}
            maxLength={1100}
            required
            autoFocus
          />
        </div>

        {(validationError || apiError) && (
          <div className={styles.errorBanner} role="alert">
            <AlertCircle size={16} />
            <span>{validationError || apiError}</span>
          </div>
        )}

        <div>
          <button
            type="button"
            className={styles.optionsToggle}
            onClick={() => setShowOptions(!showOptions)}
            aria-expanded={showOptions}
          >
            <Sliders size={13} />
            <span>Retrieval Grounding Options</span>
            {showOptions ? <ChevronUp size={13} /> : <ChevronDown size={13} />}
          </button>

          {showOptions && (
            <div className={styles.optionsPanel}>
              <div className={styles.optionItem}>
                <label htmlFor="evidence-limit" className={styles.optionLabel}>
                  <span>Max Evidence Items</span>
                  <span className={styles.optionValueDisplay}>{maxEvidence}</span>
                </label>
                <input
                  id="evidence-limit"
                  type="range"
                  min={1}
                  max={20}
                  step={1}
                  value={maxEvidence}
                  onChange={(e) => setMaxEvidence(Number(e.target.value))}
                  className={styles.sliderInput}
                  disabled={isSubmitting}
                />
              </div>

              <div className={styles.optionItem}>
                <label htmlFor="min-relevance" className={styles.optionLabel}>
                  <span>Min Relevance Score</span>
                  <span className={styles.optionValueDisplay}>
                    {Math.round(minRelevanceScore * 100)}%
                  </span>
                </label>
                <input
                  id="min-relevance"
                  type="range"
                  min={0}
                  max={1}
                  step={0.05}
                  value={minRelevanceScore}
                  onChange={(e) => setMinRelevanceScore(Number(e.target.value))}
                  className={styles.sliderInput}
                  disabled={isSubmitting}
                />
              </div>
            </div>
          )}
        </div>

        <div className={styles.footerRow}>
          <div className={styles.starterSection}>
            <p className={styles.starterTitle}>Suggested Prompts</p>
            <div className={styles.chipsContainer}>
              {STARTER_PROMPTS.map((promptText, i) => (
                <button
                  key={i}
                  type="button"
                  className={styles.starterChip}
                  onClick={() => handleSelectStarter(promptText)}
                  disabled={isSubmitting}
                >
                  {promptText}
                </button>
              ))}
            </div>
          </div>

          <Button
            type="submit"
            variant="primary"
            disabled={!canSubmit}
            isLoading={isSubmitting}
            icon={isSubmitting ? <Sparkles size={16} /> : <Send size={16} />}
          >
            {isSubmitting ? 'Synthesizing...' : 'Ask Assistant'}
          </Button>
        </div>
      </form>
    </div>
  );
};
