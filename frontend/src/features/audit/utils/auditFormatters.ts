import type { BadgeVariant } from '@/components/ui/Badge';

/**
 * Standard representation of an empty GUID (Guid.Empty).
 */
export const EMPTY_GUID = '00000000-0000-0000-0000-000000000000';

/**
 * Determines whether a given string is Guid.Empty.
 */
export function isEmptyGuid(value: string): boolean {
  if (!value || typeof value !== 'string') return false;
  return value.trim().toLowerCase() === EMPTY_GUID;
}

/**
 * Validates whether a given string is a valid non-empty UUID/GUID format.
 * Rejects empty string, invalid format, and Guid.Empty (00000000-0000-0000-0000-000000000000).
 */
export function isValidGuid(value: string): boolean {
  if (!value || typeof value !== 'string') return false;
  const trimmed = value.trim();
  const guidRegex = /^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$/i;
  if (!guidRegex.test(trimmed)) return false;
  if (trimmed.toLowerCase() === EMPTY_GUID) return false;
  return true;
}

/**
 * Formats an ISO-8601 audit timestamp into a standardized readable string.
 */
export function formatAuditDate(isoDate: string): string {
  if (!isoDate) return '—';
  try {
    const date = new Date(isoDate);
    if (Number.isNaN(date.getTime())) return isoDate;
    return new Intl.DateTimeFormat('en-US', {
      month: 'short',
      day: 'numeric',
      year: 'numeric',
      hour: '2-digit',
      minute: '2-digit',
      second: '2-digit',
      hour12: true,
      timeZoneName: 'short',
    }).format(date);
  } catch {
    return isoDate;
  }
}

/**
 * Maps audit action verb to appropriate visual BadgeVariant.
 */
export function getActionBadgeVariant(action: string): BadgeVariant {
  const normalized = (action || '').toLowerCase().trim();

  if (
    normalized.includes('create') ||
    normalized.includes('add') ||
    normalized.includes('register')
  ) {
    return 'success';
  }

  if (
    normalized.includes('update') ||
    normalized.includes('edit') ||
    normalized.includes('modify')
  ) {
    return 'cyan';
  }

  if (
    normalized.includes('approve') ||
    normalized.includes('verify') ||
    normalized.includes('promote') ||
    normalized.includes('publish')
  ) {
    return 'violet';
  }

  if (
    normalized.includes('reject') ||
    normalized.includes('delete') ||
    normalized.includes('remove') ||
    normalized.includes('archive')
  ) {
    return 'error';
  }

  if (
    normalized.includes('warn') ||
    normalized.includes('review') ||
    normalized.includes('flag')
  ) {
    return 'warning';
  }

  return 'default';
}

/**
 * Determines whether a given entity type has a valid frontend route.
 * Only 'KnowledgeArticle' is routable to /knowledge/:id.
 */
export function isEntityRoutable(entityName: string): boolean {
  if (!entityName) return false;
  return entityName.trim().toLowerCase() === 'knowledgearticle';
}

/**
 * Safely resolves the frontend navigation URL for an entity.
 * Returns null if the entity type has no supported route or has an invalid UUID.
 */
export function getEntityRoute(entityName: string, entityId: string): string | null {
  if (!isEntityRoutable(entityName)) return null;
  if (!isValidGuid(entityId)) return null;
  return `/knowledge/${entityId.trim()}`;
}

/**
 * Truncates long narrative strings with an ellipsis.
 */
export function truncateText(text: string, maxLength: number = 80): string {
  if (!text) return '';
  const trimmed = text.trim();
  if (trimmed.length <= maxLength) return trimmed;
  return `${trimmed.slice(0, maxLength)}...`;
}
