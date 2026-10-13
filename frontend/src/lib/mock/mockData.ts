import type {
  UserSummary,
  NavItem,
  WorkspaceItem,
  GraphItem,
  ConnectionItem,
  RecentActivityItem,
  KnowledgeSummaryItem,
} from '@/types';

/**
 * Visual shell mock data for Phase F0 demonstration.
 * Intentionally isolated from API services.
 * FUTURE EXTENSION: Will be replaced by feature API calls in subsequent phases.
 */

export const mockCurrentUser: UserSummary = {
  id: 'usr_dev_01',
  name: 'Alex Mercer',
  email: 'alex.mercer@eng.internal',
  role: 'Principal Engineer',
  initials: 'AM',
};

export const mockNavigationItems: NavItem[] = [
  { id: 'home', label: 'Home', path: '/', icon: 'Home' },
  { id: 'knowledge', label: 'Knowledge', path: '/knowledge', icon: 'BookOpen', badge: 142 },
  { id: 'incidents', label: 'Incidents', path: '/incidents', icon: 'AlertTriangle' },
  { id: 'documents', label: 'Documents', path: '/documents', icon: 'Files' },
  { id: 'assistant', label: 'AI Assistant', path: '/assistant', icon: 'Bot' },
  { id: 'audit', label: 'Audit Logs', path: '/audit', icon: 'ScrollText' },
  { id: 'projects', label: 'Projects', path: '/projects', icon: 'FolderKanban', badge: 8 },
  { id: 'notes', label: 'Notes', path: '/notes', icon: 'FileText', badge: 24 },
  { id: 'boards', label: 'Boards', path: '/boards', icon: 'LayoutGrid' },
];

export const mockWorkspaceItems: WorkspaceItem[] = [
  { id: 'ws_emp', title: 'Employee Notes', count: 12, path: '/projects/employee-notes' },
  { id: 'ws_suez', title: 'Suez Toll System', count: 34, path: '/projects/suez-toll' },
  { id: 'ws_api', title: 'API Docs', count: 58, path: '/projects/api-docs' },
];

export const mockGraphItems: GraphItem[] = [
  { id: 'gn_ef', name: 'EF Core', references: 19, path: '/knowledge?tag=ef-core' },
  { id: 'gn_linq', name: 'LINQ', references: 14, path: '/knowledge?tag=linq' },
  { id: 'gn_mongo', name: 'MongoDB', references: 8, path: '/knowledge?tag=mongodb' },
];

export const mockRecentKnowledge: KnowledgeSummaryItem[] = [
  {
    id: 'art_01',
    title: 'ASP.NET Core Middleware Pipeline Architecture',
    summary: 'Middleware is software assembled into an app pipeline to handle requests and responses. Each component chooses whether to pass the request to the next component.',
    status: 'Published',
    verificationStatus: 'Verified',
    type: 'Architecture',
    category: 'Backend Architecture',
    tags: ['csharp', 'dotnet', 'intermediate'],
    updatedAt: '2 hours ago',
    author: 'Alex Mercer',
    version: 3,
  },
  {
    id: 'art_02',
    title: 'Resilient Microservices with Polly Circuit Breaker',
    summary: 'Implementing automated failover, retry budgets, and bulkhead isolation for downstream service calls in high-throughput environments.',
    status: 'Published',
    verificationStatus: 'Verified',
    type: 'Runbook',
    category: 'Distributed Systems',
    tags: ['microservices', 'polly', 'reliability'],
    updatedAt: 'Yesterday',
    author: 'Sarah Chen',
    version: 2,
  },
  {
    id: 'art_03',
    title: 'SQL Server LocalDB Migration Recovery Runbook',
    summary: 'Resolving snapshot isolation conflicts and pending schema migration lockouts during cold container restarts.',
    status: 'Draft',
    verificationStatus: 'Draft',
    type: 'Postmortem',
    category: 'Infrastructure',
    tags: ['sqlserver', 'efcore', 'runbook'],
    updatedAt: '3 days ago',
    author: 'Dave Miller',
    version: 1,
  },
];

export const mockRecentActivity: RecentActivityItem[] = [
  {
    id: 'act_01',
    title: 'Edited ErrorHandling.cs',
    description: 'Updated GlobalExceptionHandler ProblemDetails mapping for production sanitization.',
    timestamp: '2 hours ago',
    author: 'Alex Mercer',
    actor: 'Alex Mercer',
    iconName: 'FileCode',
  },
  {
    id: 'act_02',
    title: 'Resolved Incident INC-2026-08',
    description: 'Fixed memory pool contention in Ollama embedding runtime provider.',
    timestamp: '4 hours ago',
    author: 'Sarah Chen',
    actor: 'Sarah Chen',
    iconName: 'CheckCircle2',
  },
  {
    id: 'act_03',
    title: 'Approved Knowledge Article',
    description: 'Promoted "Vector Search Filtering Strategy" to Published with Version 3 snapshot.',
    timestamp: 'Yesterday',
    author: 'Alex Mercer',
    actor: 'Alex Mercer',
    iconName: 'ShieldCheck',
  },
];

export const mockConnections: ConnectionItem[] = [
  { id: 'conn_di', target: 'Dependency Injection', type: 'Architecture' },
  { id: 'conn_mongo', target: 'MongoDB.Driver', type: 'Technology' },
  { id: 'conn_ef', target: 'Entity Framework Core', type: 'Technology' },
];

export const mockReferencedIn = [
  { id: 'ref_suez', title: 'Suez Toll Collection System UI', subsystem: 'Frontend App' },
  { id: 'ref_auth', title: 'Auth Microservice v2', subsystem: 'Security' },
];

// Backwards-compatible aliases
export const MOCK_USER = mockCurrentUser;
export const MOCK_PRIMARY_NAV = mockNavigationItems;
export const MOCK_WORKSPACE_ITEMS = mockWorkspaceItems;
export const MOCK_GRAPH_NODES = mockGraphItems;
export const MOCK_RECENT_ARTICLES = mockRecentKnowledge;
export const MOCK_ACTIVITIES = mockRecentActivity;
export const MOCK_CONNECTIONS = mockConnections;
export const MOCK_REFERENCED_IN = mockReferencedIn;
