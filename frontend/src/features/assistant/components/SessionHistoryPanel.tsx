import React from 'react';
import { History, Trash2, Clock, BookOpen, Inbox } from 'lucide-react';
import { formatIsoTime } from '../utils/assistantFormatters';
import type { SessionQueryItem } from '../types/assistantTypes';
import styles from './SessionHistoryPanel.module.css';

export interface SessionHistoryPanelProps {
  history: SessionQueryItem[];
  activeQuestion: string | null;
  onSelect: (id: string) => void;
  onClear: () => void;
}

export const SessionHistoryPanel: React.FC<SessionHistoryPanelProps> = ({
  history,
  activeQuestion,
  onSelect,
  onClear,
}) => {
  return (
    <aside className={styles.panel} aria-label="Session Query History">
      <div className={styles.header}>
        <h4 className={styles.title}>
          <History size={15} />
          <span>Session Queries</span>
          <span className={styles.countBadge}>{history.length}</span>
        </h4>

        {history.length > 0 && (
          <button
            type="button"
            className={styles.clearButton}
            onClick={onClear}
            title="Clear in-memory session queries"
          >
            <Trash2 size={12} />
            <span>Clear</span>
          </button>
        )}
      </div>

      {history.length === 0 ? (
        <div className={styles.emptyState}>
          <Inbox size={28} className={styles.emptyIcon} />
          <p className={styles.emptyText}>No queries asked in this session yet.</p>
        </div>
      ) : (
        <div className={styles.list}>
          {history.map((item) => {
            const isActive = activeQuestion === item.question;
            const evidenceCount = item.response.evidence?.length || 0;

            return (
              <button
                key={item.id}
                type="button"
                className={`${styles.itemButton} ${isActive ? styles.itemButtonActive : ''}`}
                onClick={() => onSelect(item.id)}
              >
                <p className={styles.itemQuestion}>{item.question}</p>
                <div className={styles.itemMeta}>
                  <span>
                    <Clock size={10} /> {formatIsoTime(item.timestamp)}
                  </span>
                  <span className={styles.itemEvidenceCount}>
                    <BookOpen size={10} /> {evidenceCount} {evidenceCount === 1 ? 'citation' : 'citations'}
                  </span>
                </div>
              </button>
            );
          })}
        </div>
      )}

      <p className={styles.inMemoryDisclaimer}>
        Session history is maintained in memory only during this browser tab session.
      </p>
    </aside>
  );
};
