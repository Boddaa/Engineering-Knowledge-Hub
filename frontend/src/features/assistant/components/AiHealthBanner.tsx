import React from 'react';
import {
  Activity,
  CheckCircle2,
  AlertTriangle,
  XCircle,
  PowerOff,
  RefreshCw,
  Cpu,
  Layers,
} from 'lucide-react';
import { formatHealthStatus, formatIsoTime } from '../utils/assistantFormatters';
import type { LocalAiHealthDto } from '../types/assistantTypes';
import styles from './AiHealthBanner.module.css';

export interface AiHealthBannerProps {
  health: LocalAiHealthDto | null;
  isLoading: boolean;
  error: string | null;
  isConnectionError: boolean;
  lastChecked: Date | null;
  onRefresh: () => void;
}

export const AiHealthBanner: React.FC<AiHealthBannerProps> = ({
  health,
  isLoading,
  error,
  isConnectionError,
  lastChecked,
  onRefresh,
}) => {
  if (isLoading && !health) {
    return <div className={styles.skeletonBar} aria-label="Checking AI subsystem health..." />;
  }

  // Explicit network error state (per constraint 6: do NOT misrepresent failed request as a confirmed status)
  if (isConnectionError || (!health && error)) {
    return (
      <div className={styles.connectionErrorBanner} role="alert">
        <div className={styles.connectionErrorContent}>
          <AlertTriangle size={20} className={styles.connectionErrorIcon} />
          <div className={styles.connectionErrorText}>
            <h3 className={styles.connectionErrorTitle}>AI Health Endpoint Unreachable</h3>
            <p className={styles.connectionErrorSubtitle}>
              {error || 'Unable to connect to the backend health diagnostics service.'}
            </p>
            <span className={styles.connectionErrorCaveat}>
              Note: This is a network/service connection failure. It does not represent a confirmed backend operational state.
            </span>
          </div>
        </div>
        <div className={styles.actionsArea}>
          <button
            type="button"
            className={styles.refreshButton}
            onClick={onRefresh}
            disabled={isLoading}
            aria-label="Retry health check"
          >
            <RefreshCw size={14} className={isLoading ? 'spin' : ''} />
            <span>Retry Check</span>
          </button>
        </div>
      </div>
    );
  }

  if (!health) {
    return null;
  }

  const presentation = formatHealthStatus(health.status);

  const getContainerClass = () => {
    switch (presentation.variant) {
      case 'success':
        return styles.healthContainerHealthy;
      case 'warning':
        return styles.healthContainerWarning;
      case 'error':
        return styles.healthContainerError;
      default:
        return styles.healthContainerDisabled;
    }
  };

  const getIcon = () => {
    switch (health.status) {
      case 'Healthy':
        return (
          <div className={`${styles.statusIconWrapper} ${styles.iconSuccess}`}>
            <CheckCircle2 size={18} />
          </div>
        );
      case 'Disabled':
        return (
          <div className={`${styles.statusIconWrapper} ${styles.iconNeutral}`}>
            <PowerOff size={18} />
          </div>
        );
      case 'Unavailable':
      case 'ModelUnavailable':
        return (
          <div className={`${styles.statusIconWrapper} ${styles.iconWarning}`}>
            <AlertTriangle size={18} />
          </div>
        );
      case 'Misconfigured':
        return (
          <div className={`${styles.statusIconWrapper} ${styles.iconError}`}>
            <XCircle size={18} />
          </div>
        );
      default:
        return (
          <div className={`${styles.statusIconWrapper} ${styles.iconNeutral}`}>
            <Activity size={18} />
          </div>
        );
    }
  };

  return (
    <div className={`${styles.healthContainer} ${getContainerClass()}`}>
      <div className={styles.headerRow}>
        <div className={styles.statusIndicator}>
          {getIcon()}
          <div className={styles.titleArea}>
            <h3 className={styles.statusTitle}>{presentation.label}</h3>
            <p className={styles.statusDescription}>{presentation.description}</p>
          </div>
        </div>

        <div className={styles.actionsArea}>
          <span className={styles.timestamp}>
            Checked: {formatIsoTime(lastChecked?.toISOString())}
          </span>
          <button
            type="button"
            className={styles.refreshButton}
            onClick={onRefresh}
            disabled={isLoading}
            aria-label="Refresh health status"
          >
            <RefreshCw size={13} />
            <span>{isLoading ? 'Checking...' : 'Check Status'}</span>
          </button>
        </div>
      </div>

      <div className={styles.detailsGrid}>
        <div className={styles.detailItem}>
          <span className={styles.detailLabel}>Runtime Daemon</span>
          <span className={styles.detailValue}>
            {health.runtimeReachable ? (
              <span className={styles.badgePresent}>● Reachable</span>
            ) : (
              <span className={styles.badgeMissing}>○ Unreachable</span>
            )}
          </span>
        </div>

        <div className={styles.detailItem}>
          <span className={styles.detailLabel}>Generation Model</span>
          <span className={styles.detailValue}>
            <Cpu size={14} />
            {health.generationModelConfigured || 'None'}
            {health.generationModelAvailable ? (
              <span className={styles.badgePresent}>(Ready)</span>
            ) : (
              <span className={styles.badgeMissing}>(Missing)</span>
            )}
          </span>
        </div>

        <div className={styles.detailItem}>
          <span className={styles.detailLabel}>Embedding Model</span>
          <span className={styles.detailValue}>
            <Layers size={14} />
            {health.embeddingModelConfigured || 'None'}
            {health.embeddingModelAvailable ? (
              <span className={styles.badgePresent}>(Ready)</span>
            ) : (
              <span className={styles.badgeMissing}>(Missing)</span>
            )}
          </span>
        </div>
      </div>

      {health.diagnosticMessage && (
        <div className={styles.diagnosticMessage}>
          {health.diagnosticMessage}
        </div>
      )}
    </div>
  );
};
