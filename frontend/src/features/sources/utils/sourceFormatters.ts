import type { BadgeVariant } from '@/components/ui/Badge';
import { SourceType, TrustLevel } from '../types/sourceTypes';

/**
 * Maps numeric or string representation of SourceType to a human-readable display string.
 */
export function normalizeSourceType(sourceType: SourceType | number | string): string {
  if (typeof sourceType === 'string') {
    switch (sourceType.toLowerCase()) {
      case 'internal':
        return 'Internal';
      case 'external':
        return 'External';
      case 'incident':
        return 'Incident';
      case 'engineerexperience':
        return 'Engineer Experience';
      case 'vendordocumentation':
        return 'Vendor Documentation';
      case 'systemgenerated':
        return 'System Generated';
      case 'aisuggested':
        return 'AI Suggested';
      default:
        return sourceType;
    }
  }

  switch (sourceType) {
    case SourceType.Internal:
      return 'Internal';
    case SourceType.External:
      return 'External';
    case SourceType.Incident:
      return 'Incident';
    case SourceType.EngineerExperience:
      return 'Engineer Experience';
    case SourceType.VendorDocumentation:
      return 'Vendor Documentation';
    case SourceType.SystemGenerated:
      return 'System Generated';
    case SourceType.AISuggested:
      return 'AI Suggested';
    default:
      return `Type ${sourceType}`;
  }
}

/**
 * Maps numeric or string representation of TrustLevel to a human-readable display string.
 */
export function normalizeTrustLevel(trustLevel: TrustLevel | number | string): string {
  if (typeof trustLevel === 'string') {
    switch (trustLevel.toLowerCase()) {
      case 'official':
        return 'Official';
      case 'verified':
        return 'Verified';
      case 'community':
        return 'Community';
      case 'untrusted':
        return 'Untrusted';
      default:
        return trustLevel;
    }
  }

  switch (trustLevel) {
    case TrustLevel.Official:
      return 'Official';
    case TrustLevel.Verified:
      return 'Verified';
    case TrustLevel.Community:
      return 'Community';
    case TrustLevel.Untrusted:
      return 'Untrusted';
    default:
      return `Level ${trustLevel}`;
  }
}

/**
 * Returns the appropriate design token BadgeVariant for a given source trust level.
 */
export function getTrustLevelBadgeVariant(trustLevel: TrustLevel | number | string): BadgeVariant {
  const normalized = typeof trustLevel === 'string'
    ? trustLevel.toLowerCase()
    : trustLevel;

  if (normalized === 'official' || normalized === TrustLevel.Official) {
    return 'cyan';
  }
  if (normalized === 'verified' || normalized === TrustLevel.Verified) {
    return 'success';
  }
  if (normalized === 'community' || normalized === TrustLevel.Community) {
    return 'violet';
  }
  return 'warning';
}

/**
 * Returns the appropriate design token BadgeVariant for a given source origin type.
 */
export function getSourceTypeBadgeVariant(sourceType: SourceType | number | string): BadgeVariant {
  const normalized = typeof sourceType === 'string'
    ? sourceType.toLowerCase()
    : sourceType;

  if (normalized === 'internal' || normalized === SourceType.Internal) {
    return 'cyan';
  }
  if (normalized === 'engineerexperience' || normalized === SourceType.EngineerExperience) {
    return 'violet';
  }
  if (normalized === 'incident' || normalized === SourceType.Incident) {
    return 'warning';
  }
  if (normalized === 'vendordocumentation' || normalized === SourceType.VendorDocumentation) {
    return 'outline';
  }
  return 'default';
}
