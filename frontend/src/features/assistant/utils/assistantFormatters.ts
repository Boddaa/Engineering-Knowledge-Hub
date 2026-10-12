import type {
  LocalAiHealthStatus,
  RagEvidenceDto,
  RagResponseDto,
} from '../types/assistantTypes';

/**
 * Formats a normalized decimal score (0.0 to 1.0) into a clean percentage string.
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
 * Resolves the synthesis confidence metric from a RAG response.
 * Keeps confidenceScore distinct from individual evidence relevance scores.
 * Uses explicit backend confidenceScore if present, or derives an aggregate evidence
 * grounding confidence score while providing clear context that confidence is not a guarantee.
 */
export function resolveSynthesisConfidence(response: RagResponseDto): {
  score: number;
  formatted: string;
  disclaimer: string;
} {
  // Check if response has an explicit confidenceScore from the backend
  const explicitScore = (response as unknown as { confidenceScore?: number }).confidenceScore;

  let finalScore = 0;
  if (typeof explicitScore === 'number' && !Number.isNaN(explicitScore)) {
    finalScore = Math.max(0, Math.min(1, explicitScore));
  } else if (response.evidence && response.evidence.length > 0) {
    // Derive weighted average of top evidence relevance scores
    const topScores = response.evidence
      .map((e) => e.relevanceScore)
      .filter((s) => typeof s === 'number' && !Number.isNaN(s))
      .sort((a, b) => b - a)
      .slice(0, 3);

    if (topScores.length > 0) {
      const avg = topScores.reduce((acc, curr) => acc + curr, 0) / topScores.length;
      finalScore = response.isSufficientEvidence ? avg : avg * 0.5;
    }
  } else {
    finalScore = response.isSufficientEvidence ? 0.5 : 0.0;
  }

  return {
    score: finalScore,
    formatted: formatScorePercent(finalScore),
    disclaimer:
      'Confidence indicates retrieval context grounding density, not an absolute guarantee of correctness. Always verify source citations.',
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
 * ONLY when valid identifiers exist. If missing or undefined, returns linkPath: null.
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
    evidence.sourceType.toLowerCase().includes('external') ||
    evidence.sourceType.toLowerCase().includes('vendor');

  const provenanceName =
    evidence.metadata?.SourceName ||
    evidence.metadata?.SourceType ||
    evidence.sourceType ||
    'Unknown Source';

  let linkPath: string | null = null;
  let linkLabel = '';
  let originType = 'Reference';

  const refId = evidence.referenceId;

  if (refId && typeof refId === 'string' && refId.trim()) {
    const cleanId = refId.trim();

    if (evidence.sourceType === 'KnowledgeArticle') {
      linkPath = `/knowledge/${cleanId}`;
      linkLabel = 'View Knowledge Article';
      originType = 'Knowledge Article';
    } else if (evidence.sourceType === 'DocumentChunk') {
      linkPath = `/documents/${cleanId}`;
      linkLabel = 'View Source Document';
      originType = 'Document Chunk';
    } else if (evidence.sourceType === 'Incident') {
      linkPath = `/incidents/${cleanId}`;
      linkLabel = 'View Related Incident';
      originType = 'Incident Postmortem';
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
