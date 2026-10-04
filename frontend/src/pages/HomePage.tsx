import React from 'react';
import {
  BookOpen,
  ShieldCheck,
  AlertTriangle,
  Network,
  Sparkles,
  Plus,
  ArrowRight,
  Layers,
} from 'lucide-react';
import { PageShell } from '@/components/layout/PageShell';
import { Badge } from '@/components/ui/Badge';
import { Button } from '@/components/ui/Button';
import { mockRecentKnowledge, mockCurrentUser } from '@/lib/mock/mockData';
import styles from './HomePage.module.css';

export const HomePage: React.FC = () => {
  return (
    <PageShell
      title="Engineering Cockpit"
      description="Central intelligence for architecture standards, verified runbooks, incident postmortems, and technology taxonomies."
      badge={<Badge variant="cyan" icon={<Sparkles size={11} />}>Phase F0 Shell</Badge>}
      headerActions={
        <div style={{ display: 'flex', gap: 'var(--space-sm)' }}>
          <Button variant="subtle" size="md" icon={<Network size={15} />}>
            Explore Graph
          </Button>
          <Button variant="primary" size="md" icon={<Plus size={15} />}>
            New Entry
          </Button>
        </div>
      }
    >
      <div className={styles.pageContent}>
        {/* Welcome Banner */}
        <section className={styles.welcomeCard} aria-label="Welcome Overview">
          <div className={styles.welcomeLeft}>
            <div style={{ display: 'flex', alignItems: 'center', gap: '8px' }}>
              <Badge variant="violet">Phase F0 Foundation</Badge>
              <span className="text-caption">Presentation Shell</span>
            </div>
            <h2 className={styles.welcomeTitle}>
              Welcome back, {mockCurrentUser.name}
            </h2>
            <p className={styles.welcomeSubtitle}>
              Phase F0 application shell and design system foundation. Operational metrics and activity displays below are simulated presentation mock data pending Phase F1 API integration.
            </p>
          </div>
          <Button
            variant="ghost"
            size="md"
            icon={<ArrowRight size={15} />}
          >
            Review Architecture
          </Button>
        </section>

        {/* Metrics Overview Grid with Clear F0 Mock Presentation Tag */}
        <section aria-label="Knowledge Hub Overview Metrics">
          <div className={styles.sectionHeader} style={{ marginBottom: 'var(--space-sm)' }}>
            <div style={{ display: 'flex', alignItems: 'center', gap: '8px' }}>
              <h2 className={styles.sectionTitle}>Platform Overview</h2>
              <Badge variant="outline">F0 Mock Data</Badge>
            </div>
            <span className="text-caption">Simulated metrics (F0 presentation)</span>
          </div>

          <div className={styles.metricsGrid}>
            <div className={styles.metricCard}>
              <div className={styles.metricHeader}>
                <span className={styles.metricLabel}>Knowledge Nodes</span>
                <BookOpen size={16} />
              </div>
              <div className={styles.metricValue}>142</div>
              <span className={styles.metricMeta}>+12 added (simulated)</span>
            </div>

            <div className={styles.metricCard}>
              <div className={styles.metricHeader}>
                <span className={styles.metricLabel}>Verified Ratio</span>
                <ShieldCheck size={16} />
              </div>
              <div className={styles.metricValue} style={{ color: 'var(--color-status-success)' }}>
                94.2%
              </div>
              <span className={styles.metricMeta}>Simulated governance rule</span>
            </div>

            <div className={styles.metricCard}>
              <div className={styles.metricHeader}>
                <span className={styles.metricLabel}>Active Incidents</span>
                <AlertTriangle size={16} />
              </div>
              <div className={styles.metricValue} style={{ color: 'var(--color-status-warning)' }}>
                3
              </div>
              <span className={styles.metricMeta}>Simulated incident queue</span>
            </div>

            <div className={styles.metricCard}>
              <div className={styles.metricHeader}>
                <span className={styles.metricLabel}>Tech Taxonomy</span>
                <Layers size={16} />
              </div>
              <div className={styles.metricValue} style={{ color: 'var(--color-accent-secondary)' }}>
                28
              </div>
              <span className={styles.metricMeta}>Products & frameworks (mock)</span>
            </div>
          </div>
        </section>

        {/* Two-Column Section: Recent Knowledge & Health Diagnostics */}
        <div className={styles.dashboardLayout}>
          {/* Left Column: Recent Knowledge */}
          <section aria-label="Recent Knowledge">
            <div className={styles.sectionHeader}>
              <h2 className={styles.sectionTitle}>Recent Knowledge Entries</h2>
              <Badge variant="outline">F0 Presentation Mock</Badge>
            </div>
            <div className={styles.knowledgeList}>
              {mockRecentKnowledge.map((item) => (
                <article
                  key={item.id}
                  className={styles.knowledgeItem}
                  tabIndex={0}
                  role="button"
                  aria-label={`Open knowledge article: ${item.title}`}
                >
                  <div className={styles.itemHeader}>
                    <h3 className={styles.itemTitle}>{item.title}</h3>
                    <div style={{ display: 'flex', gap: '6px' }}>
                      <Badge
                        variant={
                          item.verificationStatus === 'Verified'
                            ? 'success'
                            : item.verificationStatus === 'Draft'
                            ? 'default'
                            : 'warning'
                        }
                        dot
                      >
                        {item.verificationStatus}
                      </Badge>
                      <Badge variant="cyan">{item.type}</Badge>
                    </div>
                  </div>
                  <p className={styles.itemSummary}>{item.summary}</p>
                  <div className={styles.itemFooter}>
                    <span>By {item.author}</span>
                    <span>•</span>
                    <span>Updated {item.updatedAt}</span>
                    <span>•</span>
                    <span className="text-code">v{item.version}</span>
                  </div>
                </article>
              ))}
            </div>
          </section>

          {/* Right Column: Health Diagnostics */}
          <section aria-label="Knowledge System Health">
            <div className={styles.sectionHeader}>
              <h2 className={styles.sectionTitle}>System Health & Integrity</h2>
              <Badge variant="outline">F0 Presentation Mock</Badge>
            </div>
            <div className={styles.healthList}>
              <div className={styles.healthCard}>
                <div className={styles.healthRow}>
                  <span style={{ fontWeight: 500, color: 'var(--color-text-primary)' }}>
                    Snapshot Governance
                  </span>
                  <span className="text-code" style={{ color: 'var(--color-accent-primary)' }}>
                    100%
                  </span>
                </div>
                <div className={styles.progressBar}>
                  <div className={styles.progressFill} style={{ width: '100%' }} />
                </div>
                <span className="text-caption">
                  Simulated readiness target (mandatory snapshots on all transitions)
                </span>
              </div>

              <div className={styles.healthCard}>
                <div className={styles.healthRow}>
                  <span style={{ fontWeight: 500, color: 'var(--color-text-primary)' }}>
                    RAG Vector Embeddings
                  </span>
                  <span className="text-code" style={{ color: 'var(--color-status-success)' }}>
                    Indexed
                  </span>
                </div>
                <div className={styles.progressBar}>
                  <div className={styles.progressFill} style={{ width: '92%' }} />
                </div>
                <span className="text-caption">
                  Simulated status (semantic chunking and embedding stores)
                </span>
              </div>

              <div className={styles.healthCard}>
                <div className={styles.healthRow}>
                  <span style={{ fontWeight: 500, color: 'var(--color-text-primary)' }}>
                    REST API Contract Status
                  </span>
                  <span className="text-code" style={{ color: 'var(--color-accent-secondary)' }}>
                    13 Controllers
                  </span>
                </div>
                <span className="text-caption" style={{ marginTop: '4px' }}>
                  Base URL ready for Phase F1+ connection
                </span>
              </div>
            </div>
          </section>
        </div>
      </div>
    </PageShell>
  );
};
