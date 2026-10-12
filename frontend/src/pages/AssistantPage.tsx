import React from 'react';
import {
  Sparkles,
  Bot,
  Search,
  ShieldCheck,
  BookOpen,
} from 'lucide-react';
import {
  AiHealthBanner,
  RagQueryWorkspace,
  RagAnswerDisplay,
  SessionHistoryPanel,
  useAiHealth,
  useRagQuery,
} from '@/features/assistant';
import styles from './AssistantPage.module.css';

export const AssistantPage: React.FC = () => {
  const {
    health,
    isLoading: isLoadingHealth,
    error: healthError,
    isConnectionError,
    lastChecked,
    checkHealth,
  } = useAiHealth();

  const {
    activeResponse,
    activeQuestion,
    history,
    isSubmitting,
    error: queryError,
    ask,
    selectHistoryItem,
    clearHistory,
  } = useRagQuery();

  return (
    <div className={styles.pageContainer}>
      <header className={styles.header}>
        <div className={styles.titleRow}>
          <Bot size={28} className={styles.titleIcon} />
          <h1 className={styles.title}>AI Knowledge Assistant</h1>
        </div>
        <p className={styles.subtitle}>
          Context-grounded technical question answering synthesizing verified engineering knowledge articles, documents, and postmortems with citation evidence.
        </p>
      </header>

      {/* Local AI Operational Diagnostics Banner */}
      <AiHealthBanner
        health={health}
        isLoading={isLoadingHealth}
        error={healthError}
        isConnectionError={isConnectionError}
        lastChecked={lastChecked}
        onRefresh={checkHealth}
      />

      <div className={styles.mainLayout}>
        <div className={styles.queryColumn}>
          {/* Query Formulation Input Workspace */}
          <RagQueryWorkspace
            isSubmitting={isSubmitting}
            apiError={queryError}
            onSubmit={ask}
          />

          {/* Active Synthesis Loading Skeleton */}
          {isSubmitting && (
            <div className={styles.loadingSkeletonCard} aria-busy="true" aria-label="Synthesizing response...">
              <div className={styles.skeletonHeader} />
              <div className={styles.loadingMessage}>
                <Sparkles size={16} className="spin" />
                <span>Retrieving semantic chunks and synthesizing grounded answer...</span>
              </div>
              <div className={styles.skeletonLine} />
              <div className={styles.skeletonLine} />
              <div className={styles.skeletonLineShort} />
            </div>
          )}

          {/* Active Synthesized Answer with Grounded Citations */}
          {!isSubmitting && activeResponse && (
            <RagAnswerDisplay
              response={activeResponse}
              activeQuestion={activeQuestion}
            />
          )}

          {/* Initial Clean Guidance Slate */}
          {!isSubmitting && !activeResponse && (
            <div className={styles.emptyStateCard}>
              <div className={styles.emptyIconWrapper}>
                <Sparkles size={24} />
              </div>
              <h2 className={styles.emptyStateTitle}>Grounded Retrieval-Augmented Generation</h2>
              <p className={styles.emptyStateDescription}>
                Ask any question about architecture patterns, system specifications, or resolved incidents. The assistant retrieves relevant chunks from approved knowledge and returns an evidence-backed answer with explicit citations.
              </p>
              <div className={styles.workflowPills}>
                <span className={styles.workflowPill}>
                  <Search size={12} /> Semantic Vector Retrieval
                </span>
                <span className={styles.workflowPill}>
                  <ShieldCheck size={12} /> Grounding Verification
                </span>
                <span className={styles.workflowPill}>
                  <BookOpen size={12} /> Traceable Citations
                </span>
              </div>
            </div>
          )}
        </div>

        {/* In-Memory Session History Panel */}
        <SessionHistoryPanel
          history={history}
          activeQuestion={activeQuestion}
          onSelect={selectHistoryItem}
          onClear={clearHistory}
        />
      </div>
    </div>
  );
};
