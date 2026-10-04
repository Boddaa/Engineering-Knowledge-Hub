import type { KnowledgeStatus, VerificationStatus } from '../types/knowledgeTypes';
import type { BadgeVariant } from '@/components/ui/Badge';

/**
 * Normalizes backend KnowledgeStatus which may arrive as either an integer or string.
 * Domain: Draft = 1, PendingReview = 2, Verified = 3, Published = 4, Deprecated = 5, Archived = 6
 */
export function normalizeKnowledgeStatus(status: KnowledgeStatus | number | undefined | null): KnowledgeStatus {
  if (typeof status === 'string') {
    return status;
  }
  switch (status) {
    case 1:
      return 'Draft';
    case 2:
      return 'PendingReview';
    case 3:
      return 'Verified';
    case 4:
      return 'Published';
    case 5:
      return 'Deprecated';
    case 6:
      return 'Archived';
    default:
      return 'Draft';
  }
}

/**
 * Normalizes backend VerificationStatus which may arrive as either an integer or string.
 * Domain: Unverified = 1, Verified = 2, Rejected = 3
 */
export function normalizeVerificationStatus(status: VerificationStatus | number | undefined | null): VerificationStatus {
  if (typeof status === 'string') {
    return status;
  }
  switch (status) {
    case 1:
      return 'Unverified';
    case 2:
      return 'Verified';
    case 3:
      return 'Rejected';
    default:
      return 'Unverified';
  }
}

/**
 * Maps knowledge status to an appropriate design system badge variant.
 */
export function getStatusBadgeVariant(status: KnowledgeStatus | number | undefined | null): BadgeVariant {
  const normalized = normalizeKnowledgeStatus(status);
  switch (normalized) {
    case 'Published':
      return 'cyan';
    case 'Verified':
      return 'success';
    case 'PendingReview':
      return 'warning';
    case 'Draft':
      return 'default';
    case 'Deprecated':
      return 'error';
    case 'Archived':
      return 'outline';
    default:
      return 'default';
  }
}

/**
 * Maps verification status to an appropriate design system badge variant.
 */
export function getVerificationBadgeVariant(status: VerificationStatus | number | undefined | null): BadgeVariant {
  const normalized = normalizeVerificationStatus(status);
  switch (normalized) {
    case 'Verified':
      return 'success';
    case 'Unverified':
      return 'warning';
    case 'Rejected':
      return 'error';
    default:
      return 'outline';
  }
}

/**
 * Formats ISO date strings gracefully into human-readable strings.
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
