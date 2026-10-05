import type { BadgeVariant } from '@/components/ui/Badge';
import { DocumentProcessingStatus } from '../types/documentTypes';

/**
 * Maps numeric or string representation of DocumentProcessingStatus to a human-readable display string.
 */
export function normalizeProcessingStatus(
  status: DocumentProcessingStatus | number | string
): string {
  if (typeof status === 'string') {
    switch (status.toLowerCase()) {
      case 'pending':
        return 'Pending';
      case 'processing':
        return 'Processing';
      case 'processed':
        return 'Processed';
      case 'failed':
        return 'Failed';
      default:
        return status;
    }
  }

  switch (status) {
    case DocumentProcessingStatus.Pending:
      return 'Pending';
    case DocumentProcessingStatus.Processing:
      return 'Processing';
    case DocumentProcessingStatus.Processed:
      return 'Processed';
    case DocumentProcessingStatus.Failed:
      return 'Failed';
    default:
      return `Status ${status}`;
  }
}

/**
 * Returns the appropriate design token BadgeVariant for a given document processing status.
 */
export function getProcessingStatusBadgeVariant(
  status: DocumentProcessingStatus | number | string
): BadgeVariant {
  const normalized = typeof status === 'string'
    ? status.toLowerCase()
    : status;

  if (normalized === 'processed' || normalized === DocumentProcessingStatus.Processed) {
    return 'success';
  }
  if (normalized === 'processing' || normalized === DocumentProcessingStatus.Processing) {
    return 'cyan';
  }
  if (normalized === 'pending' || normalized === DocumentProcessingStatus.Pending) {
    return 'warning';
  }
  if (normalized === 'failed' || normalized === DocumentProcessingStatus.Failed) {
    return 'error';
  }
  return 'default';
}

/**
 * Formats an ISO date string for consistent presentation across documents views.
 */
export function formatDocumentDate(isoDate: string | null | undefined): string {
  if (!isoDate) return '—';
  try {
    const date = new Date(isoDate);
    if (isNaN(date.getTime())) return isoDate;
    return new Intl.DateTimeFormat('en-US', {
      year: 'numeric',
      month: 'short',
      day: 'numeric',
      hour: '2-digit',
      minute: '2-digit',
    }).format(date);
  } catch {
    return isoDate;
  }
}
