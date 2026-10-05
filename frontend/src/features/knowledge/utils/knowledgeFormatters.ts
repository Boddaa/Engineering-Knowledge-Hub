import type { KnowledgeStatus, VerificationStatus, SearchSortOption } from '../types/knowledgeTypes';
import type { BadgeVariant } from '@/components/ui/Badge';

/**
 * Valid sort dimensions supported by the search API contract.
 */
export const VALID_SEARCH_SORT_OPTIONS: readonly SearchSortOption[] = [
  'Relevance',
  'RecentlyUpdated',
  'RecentlyCreated',
  'TitleAsc',
  'TitleDesc',
] as const;

/**
 * Type guard verifying if a value matches a supported SearchSortOption.
 */
export function isValidSearchSortOption(value: unknown): value is SearchSortOption {
  return typeof value === 'string' && VALID_SEARCH_SORT_OPTIONS.includes(value as SearchSortOption);
}

/**
 * Normalizes URL sort parameters safely to prevent passing invalid values to the API.
 * Defaults safely to 'Relevance'.
 */
export function normalizeSearchSortOption(value: string | null | undefined): SearchSortOption {
  if (value && isValidSearchSortOption(value)) {
    return value;
  }
  return 'Relevance';
}

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

/**
 * Valid knowledge statuses supported by the backend search contract.
 */
export const VALID_KNOWLEDGE_STATUSES: readonly KnowledgeStatus[] = [
  'Draft',
  'PendingReview',
  'Verified',
  'Published',
  'Deprecated',
  'Archived',
] as const;

/**
 * Type guard verifying if a value matches a supported KnowledgeStatus.
 */
export function isValidKnowledgeStatus(value: unknown): value is KnowledgeStatus {
  return typeof value === 'string' && VALID_KNOWLEDGE_STATUSES.includes(value as KnowledgeStatus);
}

/**
 * Safely parses and normalizes status parameter from URL search params.
 * Returns undefined if missing or invalid.
 */
export function normalizeFilterStatus(value: string | null | undefined): KnowledgeStatus | undefined {
  if (!value) return undefined;
  if (isValidKnowledgeStatus(value)) return value;
  // If numeric string
  const num = parseInt(value, 10);
  if (!isNaN(num) && num >= 1 && num <= 6) {
    return normalizeKnowledgeStatus(num);
  }
  return undefined;
}

/**
 * Valid verification statuses supported by the backend search contract.
 */
export const VALID_VERIFICATION_STATUSES: readonly VerificationStatus[] = [
  'Unverified',
  'Verified',
  'Rejected',
] as const;

/**
 * Type guard verifying if a value matches a supported VerificationStatus.
 */
export function isValidVerificationStatus(value: unknown): value is VerificationStatus {
  return typeof value === 'string' && VALID_VERIFICATION_STATUSES.includes(value as VerificationStatus);
}

/**
 * Safely parses and normalizes verification status parameter from URL search params.
 * Returns undefined if missing or invalid.
 */
export function normalizeFilterVerificationStatus(value: string | null | undefined): VerificationStatus | undefined {
  if (!value) return undefined;
  if (isValidVerificationStatus(value)) return value;
  const num = parseInt(value, 10);
  if (!isNaN(num) && num >= 1 && num <= 3) {
    return normalizeVerificationStatus(num);
  }
  return undefined;
}

/**
 * Standard UUID regex for validating identifier filters.
 */
export const UUID_REGEX = /^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$/i;

/**
 * Type guard verifying if a string is a valid UUID format.
 */
export function isValidUuid(value: unknown): value is string {
  return typeof value === 'string' && UUID_REGEX.test(value.trim());
}

/**
 * Safely normalizes UUID filter parameters from URL search params.
 * Returns undefined if missing or invalid UUID format.
 */
export function normalizeFilterUuid(value: string | null | undefined): string | undefined {
  if (!value) return undefined;
  const trimmed = value.trim();
  return UUID_REGEX.test(trimmed) ? trimmed : undefined;
}

/**
 * Safely normalizes page number from URL search params.
 * Ensures page is at least 1.
 */
export function normalizePage(value: string | null | undefined): number {
  if (!value) return 1;
  const parsed = parseInt(value, 10);
  return isNaN(parsed) || parsed < 1 ? 1 : parsed;
}

/**
 * Safely normalizes page size from URL search params.
 * Ensures page size is between 1 and 100, defaulting to defaultSize.
 */
export function normalizePageSize(value: string | null | undefined, defaultSize = 10): number {
  if (!value) return defaultSize;
  const parsed = parseInt(value, 10);
  return isNaN(parsed) || parsed < 1 || parsed > 100 ? defaultSize : parsed;
}
