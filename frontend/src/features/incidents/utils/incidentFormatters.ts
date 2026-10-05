import type {
  IncidentStatus,
  IncidentSeverity,
  ArticleRelationshipType,
} from '../types/incidentTypes';
import type { BadgeVariant } from '@/components/ui/Badge';

export const VALID_INCIDENT_STATUSES: readonly IncidentStatus[] = [
  'Open',
  'Investigating',
  'Resolved',
  'Closed',
] as const;

export const VALID_INCIDENT_SEVERITIES: readonly IncidentSeverity[] = [
  'Low',
  'Medium',
  'High',
  'Critical',
] as const;

export const VALID_RELATIONSHIP_TYPES: readonly ArticleRelationshipType[] = [
  'Supports',
  'Resolves',
  'RelatesTo',
  'CausedBy',
] as const;

export function isValidIncidentStatus(value: unknown): value is IncidentStatus {
  return typeof value === 'string' && VALID_INCIDENT_STATUSES.includes(value as IncidentStatus);
}

export function isValidIncidentSeverity(value: unknown): value is IncidentSeverity {
  return typeof value === 'string' && VALID_INCIDENT_SEVERITIES.includes(value as IncidentSeverity);
}

export function isValidRelationshipType(value: unknown): value is ArticleRelationshipType {
  return typeof value === 'string' && VALID_RELATIONSHIP_TYPES.includes(value as ArticleRelationshipType);
}

/**
 * Normalizes backend IncidentStatus which may arrive as either an integer or string.
 * Domain: Open = 1, Investigating = 2, Resolved = 3, Closed = 4
 */
export function normalizeIncidentStatus(status: IncidentStatus | number | string | undefined | null): IncidentStatus {
  if (typeof status === 'string') {
    const matched = VALID_INCIDENT_STATUSES.find((s) => s.toLowerCase() === status.toLowerCase());
    if (matched) return matched;
  }
  switch (status) {
    case 1:
      return 'Open';
    case 2:
      return 'Investigating';
    case 3:
      return 'Resolved';
    case 4:
      return 'Closed';
    default:
      return 'Open';
  }
}

/**
 * Normalizes backend IncidentSeverity which may arrive as either an integer or string.
 * Domain: Low = 1, Medium = 2, High = 3, Critical = 4
 */
export function normalizeIncidentSeverity(severity: IncidentSeverity | number | string | undefined | null): IncidentSeverity {
  if (typeof severity === 'string') {
    const matched = VALID_INCIDENT_SEVERITIES.find((s) => s.toLowerCase() === severity.toLowerCase());
    if (matched) return matched;
  }
  switch (severity) {
    case 1:
      return 'Low';
    case 2:
      return 'Medium';
    case 3:
      return 'High';
    case 4:
      return 'Critical';
    default:
      return 'Medium';
  }
}

/**
 * Normalizes backend ArticleRelationshipType which may arrive as either an integer or string.
 * Domain: Supports = 1, Resolves = 2, RelatesTo = 3, CausedBy = 4
 */
export function normalizeArticleRelationshipType(
  rel: ArticleRelationshipType | number | string | undefined | null
): ArticleRelationshipType {
  if (typeof rel === 'string') {
    const matched = VALID_RELATIONSHIP_TYPES.find((r) => r.toLowerCase() === rel.toLowerCase());
    if (matched) return matched;
  }
  switch (rel) {
    case 1:
      return 'Supports';
    case 2:
      return 'Resolves';
    case 3:
      return 'RelatesTo';
    case 4:
      return 'CausedBy';
    default:
      return 'RelatesTo';
  }
}

/**
 * Converts IncidentSeverity to its canonical backend numeric value.
 */
export function toSeverityNumeric(severity: IncidentSeverity): number {
  switch (severity) {
    case 'Low':
      return 1;
    case 'Medium':
      return 2;
    case 'High':
      return 3;
    case 'Critical':
      return 4;
  }
}

/**
 * Converts ArticleRelationshipType to its canonical backend numeric value.
 */
export function toRelationshipTypeNumeric(type: ArticleRelationshipType): number {
  switch (type) {
    case 'Supports':
      return 1;
    case 'Resolves':
      return 2;
    case 'RelatesTo':
      return 3;
    case 'CausedBy':
      return 4;
  }
}

/**
 * Strictly normalizes URL status filter.
 * Malformed or unparseable values safely fall back to undefined.
 */
export function normalizeStatusFilter(value: string | null | undefined): IncidentStatus | undefined {
  if (!value) return undefined;
  const trimmed = value.trim();

  // If provided as a numeric string (e.g. "1"), strictly check whole string
  if (/^\d+$/.test(trimmed)) {
    const num = Number(trimmed);
    if (num >= 1 && num <= 4) {
      return normalizeIncidentStatus(num);
    }
    return undefined;
  }

  const matched = VALID_INCIDENT_STATUSES.find((s) => s.toLowerCase() === trimmed.toLowerCase());
  return matched || undefined;
}

/**
 * Maps incident status to an appropriate design system badge variant.
 */
export function getIncidentStatusBadgeVariant(status: IncidentStatus | number | undefined | null): BadgeVariant {
  const normalized = normalizeIncidentStatus(status);
  switch (normalized) {
    case 'Open':
      return 'warning';
    case 'Investigating':
      return 'cyan';
    case 'Resolved':
      return 'success';
    case 'Closed':
      return 'default';
  }
}

/**
 * Maps incident severity to an appropriate design system badge variant.
 */
export function getIncidentSeverityBadgeVariant(severity: IncidentSeverity | number | undefined | null): BadgeVariant {
  const normalized = normalizeIncidentSeverity(severity);
  switch (normalized) {
    case 'Critical':
      return 'error';
    case 'High':
      return 'warning';
    case 'Medium':
      return 'violet';
    case 'Low':
      return 'default';
  }
}

/**
 * Maps relationship type to a badge variant.
 */
export function getRelationshipTypeBadgeVariant(rel: ArticleRelationshipType | number | undefined | null): BadgeVariant {
  const normalized = normalizeArticleRelationshipType(rel);
  switch (normalized) {
    case 'Resolves':
      return 'success';
    case 'Supports':
      return 'cyan';
    case 'CausedBy':
      return 'warning';
    case 'RelatesTo':
    default:
      return 'outline';
  }
}

/**
 * Formats ISO date strings into human-readable strings.
 */
export function formatDate(dateString: string | null | undefined): string {
  if (!dateString) return '—';
  try {
    const date = new Date(dateString);
    if (isNaN(date.getTime())) return dateString;
    return new Intl.DateTimeFormat('en-US', {
      year: 'numeric',
      month: 'short',
      day: 'numeric',
      hour: '2-digit',
      minute: '2-digit',
    }).format(date);
  } catch {
    return dateString;
  }
}
