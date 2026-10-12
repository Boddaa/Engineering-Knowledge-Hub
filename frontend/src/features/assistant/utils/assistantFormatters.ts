import type {
  LocalAiHealthStatus,
  RagEvidenceDto,
} from '../types/assistantTypes';

const UUID_REGEX = /^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$/i;
const EMPTY_GUID = '00000000-0000-0000-0000-000000000000';

/**
 * Formats a normalized decimal relevance score (0.0 to 1.0) into a clean percentage string.
 * Clamps input between 0 and 100 for visual display without mutating the underlying value.
 */
export function formatScorePercent(score: number): string {
  if (typeof score !== 'number' || Number.isNaN(score)) {
    return '0%';
  }
  const clamped = Math.max(0, Math.min(1, score));
  return `${Math.round(clamped * 100)}%`;
}

/**
 * Presentation configuration for the actual backend evidence sufficiency state (IsSufficientEvidence).
 * Does not fabricate a numeric confidence score.
 */
export interface SufficiencyPresentation {
  label: string;
  variant: 'success' | 'warning';
  description: string;
}

/**
 * Maps the boolean isSufficientEvidence response property into an honest sufficiency status.
 */
export function formatSufficiencyStatus(isSufficientEvidence: boolean): SufficiencyPresentation {
  if (isSufficientEvidence) {
    return {
      label: 'Sufficiently Grounded',
      variant: 'success',
      description: 'The answer was synthesized with sufficient retrieval evidence context.',
    };
  }

  return {
    label: 'Insufficient Grounding / Tentative',
    variant: 'warning',
    description: 'Retrieval context was insufficient to fully ground the response. The answer may rely on fallback heuristics.',
  };
}

/**
 * Presentation configuration for Local AI health status.
 */
export interface HealthStatusPresentation {
  label: string;
  variant: 'success' | 'warning' | 'error' | 'neutral';
  description: string;
}

/**
 * Normalizes raw health status values (whether returned as string or enum integer) to a typed LocalAiHealthStatus.
 */
export function normalizeHealthStatus(raw: unknown): LocalAiHealthStatus {
  if (raw === 0 || raw === '0' || raw === 'Disabled') return 'Disabled';
  if (raw === 1 || raw === '1' || raw === 'Healthy') return 'Healthy';
  if (raw === 2 || raw === '2' || raw === 'Unavailable') return 'Unavailable';
  if (raw === 3 || raw === '3' || raw === 'Misconfigured') return 'Misconfigured';
  if (raw === 4 || raw === '4' || raw === 'ModelUnavailable') return 'ModelUnavailable';
  return 'Unavailable';
}

/**
 * Maps LocalAiHealthStatus into presentation tokens and descriptive messages.
 */
export function formatHealthStatus(status: LocalAiHealthStatus): HealthStatusPresentation {
  switch (status) {
    case 'Healthy':
      return {
        label: 'Local AI Operational',
        variant: 'success',
        description: 'Runtime daemon is reachable and required AI models are loaded and ready.',
      };
    case 'Disabled':
      return {
        label: 'Local AI Disabled',
        variant: 'neutral',
        description: 'Local AI is disabled in configuration. Deterministic answer synthesis is active.',
      };
    case 'Unavailable':
      return {
        label: 'Local AI Offline',
        variant: 'warning',
        description: 'Local AI daemon is currently unreachable. Fallback deterministic provider is engaged.',
      };
    case 'ModelUnavailable':
      return {
        label: 'Models Missing',
        variant: 'warning',
        description: 'Local AI daemon is running, but one or more configured models are missing or uninstalled.',
      };
    case 'Misconfigured':
      return {
        label: 'AI Misconfigured',
        variant: 'error',
        description: 'Local AI service has invalid configuration parameters or missing settings.',
      };
    default:
      return {
        label: 'Status Unknown',
        variant: 'neutral',
        description: 'Operational health status could not be categorized.',
      };
  }
}

/**
 * Resolves linking targets and origin metadata for an individual citation evidence item.
 * Links to /knowledge/{articleId}, /documents/{documentId}, or /incidents/{incidentId}
 * ONLY when valid non-empty UUID identifiers exist. If missing or empty GUID, returns linkPath: null.
 */
export function formatEvidenceOrigin(evidence: RagEvidenceDto): {
  linkPath: string | null;
  linkLabel: string;
  originType: string;
  isExternal: boolean;
  provenanceName: string;
} {
  const isExternal =
    evidence.metadata?.IsExternal === 'true' ||
    evidence.sourceType?.toLowerCase().includes('external') ||
    evidence.sourceType?.toLowerCase().includes('vendor') ||
    false;

  const provenanceName =
    evidence.metadata?.SourceName ||
    evidence.metadata?.SourceType ||
    evidence.sourceType ||
    'Unknown Source';

  let linkPath: string | null = null;
  let linkLabel = '';
  let originType = 'Reference';

  const refId = evidence.referenceId;

  if (refId && typeof refId === 'string') {
    const trimmed = refId.trim();

    if (UUID_REGEX.test(trimmed) && trimmed !== EMPTY_GUID) {
      if (evidence.sourceType === 'KnowledgeArticle') {
        linkPath = `/knowledge/${trimmed}`;
        linkLabel = 'View Knowledge Article';
        originType = 'Knowledge Article';
      } else if (evidence.sourceType === 'DocumentChunk') {
        linkPath = `/documents/${trimmed}`;
        linkLabel = 'View Source Document';
        originType = 'Document Chunk';
      } else if (evidence.sourceType === 'Incident') {
        linkPath = `/incidents/${trimmed}`;
        linkLabel = 'View Related Incident';
        originType = 'Incident Postmortem';
      }
    }
  }

  return {
    linkPath,
    linkLabel,
    originType,
    isExternal,
    provenanceName,
  };
}

/**
 * Formats ISO date timestamps to a clean locale presentation.
 */
export function formatIsoTime(isoDate: string | undefined): string {
  if (!isoDate) return 'Just now';
  try {
    const date = new Date(isoDate);
    if (Number.isNaN(date.getTime())) return 'Just now';
    return date.toLocaleTimeString(undefined, {
      hour: '2-digit',
      minute: '2-digit',
      second: '2-digit',
    });
  } catch {
    return 'Just now';
  }
}
