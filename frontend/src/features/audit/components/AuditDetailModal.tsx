import React, { useState, useEffect } from 'react';
import { Link } from 'react-router-dom';
import { X, Copy, Check, ExternalLink, ShieldCheck } from 'lucide-react';
import { Badge } from '@/components/ui/Badge';
import { Button } from '@/components/ui/Button';
import type { AuditLogDto } from '../types/auditTypes';
import {
  formatAuditDate,
  getActionBadgeVariant,
  isEntityRoutable,
  getEntityRoute,
} from '../utils/auditFormatters';
import styles from './AuditDetailModal.module.css';

export interface AuditDetailModalProps {
  log: AuditLogDto | null;
  isOpen: boolean;
  onClose: () => void;
}

export const AuditDetailModal: React.FC<AuditDetailModalProps> = ({
  log,
  isOpen,
  onClose,
}) => {
  const [copiedJson, setCopiedJson] = useState(false);

  useEffect(() => {
    const handleKeyDown = (e: KeyboardEvent) => {
      if (e.key === 'Escape' && isOpen) {
        onClose();
      }
    };
    window.addEventListener('keydown', handleKeyDown);
    return () => window.removeEventListener('keydown', handleKeyDown);
  }, [isOpen, onClose]);

  if (!isOpen || !log) return null;

  const isRoutable = isEntityRoutable(log.entityName);
  const route = getEntityRoute(log.entityName, log.entityId);

  const handleCopyJson = () => {
    navigator.clipboard.writeText(JSON.stringify(log, null, 2));
    setCopiedJson(true);
    setTimeout(() => setCopiedJson(false), 2000);
  };

  return (
    <div
      className={styles.backdrop}
      onClick={(e) => {
        if (e.target === e.currentTarget) onClose();
      }}
      role="dialog"
      aria-modal="true"
      aria-labelledby="audit-modal-title"
    >
      <div className={styles.modal}>
        <div className={styles.header}>
          <div>
            <h3 id="audit-modal-title" className={styles.headerTitle}>
              Audit Record Inspection
            </h3>
            <div className={styles.headerSubtitle}>
              Log ID: <span className={styles.monoText}>{log.id}</span>
            </div>
          </div>
          <button
            type="button"
            className={styles.closeButton}
            onClick={onClose}
            aria-label="Close modal"
          >
            <X size={18} />
          </button>
        </div>

        <div className={styles.body}>
          <div className={styles.grid}>
            <div className={styles.field}>
              <span className={styles.fieldLabel}>Timestamp</span>
              <span className={styles.fieldValue}>{formatAuditDate(log.createdAt)}</span>
              <span className={`${styles.monoText}`} style={{ color: 'var(--color-text-muted)' }}>
                {log.createdAt}
              </span>
            </div>

            <div className={styles.field}>
              <span className={styles.fieldLabel}>Action</span>
              <span className={styles.fieldValue}>
                <Badge variant={getActionBadgeVariant(log.action)} size="md">
                  {log.action}
                </Badge>
              </span>
            </div>

            <div className={styles.field}>
              <span className={styles.fieldLabel}>Entity Name</span>
              <span className={styles.fieldValue}>{log.entityName || 'Unspecified'}</span>
            </div>

            <div className={styles.field}>
              <span className={styles.fieldLabel}>Target Entity ID</span>
              <span className={`${styles.fieldValue} ${styles.monoText}`}>
                {log.entityId}
              </span>
              {isRoutable && route ? (
                <Link
                  to={route}
                  style={{
                    display: 'inline-flex',
                    alignItems: 'center',
                    gap: '4px',
                    fontSize: '12px',
                    color: 'var(--color-brand-cyan)',
                    marginTop: '2px',
                  }}
                  onClick={onClose}
                >
                  <span>Open Knowledge Article</span>
                  <ExternalLink size={12} />
                </Link>
              ) : (
                <span style={{ fontSize: '11px', color: 'var(--color-text-muted)', marginTop: '2px' }}>
                  No dedicated route for entity type &quot;{log.entityName}&quot;
                </span>
              )}
            </div>

            <div className={styles.field} style={{ gridColumn: 'span 2' }}>
              <span className={styles.fieldLabel}>Initiated By User ID</span>
              <span className={`${styles.fieldValue} ${styles.monoText}`}>{log.userId}</span>
            </div>
          </div>

          <div className={styles.field}>
            <span className={styles.fieldLabel}>Event Details &amp; Audit Narrative</span>
            <div className={styles.narrativeBox}>
              {log.details || 'No additional narrative text provided for this event.'}
            </div>
          </div>

          <div className={styles.field}>
            <span className={styles.fieldLabel}>Raw JSON Payload</span>
            <div className={styles.rawJsonBox}>
              {JSON.stringify(log, null, 2)}
            </div>
          </div>
        </div>

        <div className={styles.footer}>
          <div style={{ display: 'flex', alignItems: 'center', gap: '6px', fontSize: '12px', color: 'var(--color-text-muted)' }}>
            <ShieldCheck size={14} style={{ color: 'var(--color-brand-cyan)' }} />
            <span>System Governance Audit Record</span>
          </div>

          <div style={{ display: 'flex', gap: 'var(--space-xs)' }}>
            <Button
              type="button"
              variant="subtle"
              size="md"
              icon={copiedJson ? <Check size={14} style={{ color: 'var(--color-success)' }} /> : <Copy size={14} />}
              onClick={handleCopyJson}
            >
              {copiedJson ? 'Copied' : 'Copy JSON'}
            </Button>
            <Button
              type="button"
              variant="subtle"
              size="md"
              onClick={onClose}
            >
              Close
            </Button>
          </div>
        </div>
      </div>
    </div>
  );
};
