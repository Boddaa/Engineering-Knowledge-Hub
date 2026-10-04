/**
 * Core domain status enums matching backend definitions
 */
export type KnowledgeStatus = 'Draft' | 'PendingReview' | 'Published' | 'Archived' | 'Deprecated';
export type VerificationStatus = 'Unverified' | 'Verified' | 'Rejected' | 'Draft';
export type IncidentSeverity = 'Low' | 'Medium' | 'High' | 'Critical';
export type IncidentStatus = 'Reported' | 'Investigating' | 'Identified' | 'Resolved' | 'Closed';

export interface UserSummary {
  id: string;
  name: string;
  email: string;
  role: string;
  avatarUrl?: string;
  initials?: string;
}

export interface NavItem {
  id: string;
  label: string;
  path: string;
  icon: string;
  badge?: string | number;
}

export interface WorkspaceItem {
  id: string;
  title: string;
  count: number;
  path?: string;
}

export interface GraphItem {
  id: string;
  name: string;
  references: number;
  path?: string;
}

export interface ConnectionItem {
  id: string;
  target: string;
  type: string;
}

export interface QuickActionItem {
  id: string;
  label: string;
  iconName: string;
  onClick?: () => void;
  shortcut?: string;
}

export interface RecentActivityItem {
  id: string;
  title?: string;
  description: string;
  timestamp: string;
  author: string;
  actor?: string;
  iconName?: string;
}

export interface KnowledgeSummaryItem {
  id: string;
  title: string;
  summary: string;
  status: KnowledgeStatus;
  verificationStatus: VerificationStatus;
  type: string;
  category: string;
  tags: string[];
  updatedAt: string;
  author: string;
  version: number;
}
