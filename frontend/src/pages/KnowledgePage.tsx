import {
  Filter,
  Plus,
  Search,
} from 'lucide-react';
import { PageShell } from '@/components/layout/PageShell';
import { Badge } from '@/components/ui/Badge';
import { Button } from '@/components/ui/Button';
import { Input } from '@/components/ui/Input';
import { mockRecentKnowledge } from '@/lib/mock/mockData';
import styles from './HomePage.module.css';

export const KnowledgePage: React.FC = () => {
  return (
    <PageShell
      title="Knowledge Repository"
      description="Authoritative engineering documents, architectural decisions, runbooks, and incident postmortems."
      badge={<Badge variant="default">142 Articles</Badge>}
      headerActions={
        <div style={{ display: 'flex', gap: 'var(--space-sm)' }}>
          <Button variant="subtle" size="md" icon={<Filter size={15} />}>
            Filter
          </Button>
          <Button variant="primary" size="md" icon={<Plus size={15} />}>
            New Knowledge
          </Button>
        </div>
      }
      toolbar={
        <div
          style={{
            display: 'flex',
            alignItems: 'center',
            justifyContent: 'space-between',
            width: '100%',
            gap: 'var(--space-md)',
          }}
        >
          <div style={{ maxWidth: '420px', width: '100%' }}>
            <Input
              size="sm"
              placeholder="Filter knowledge entries..."
              prefix={<Search size={14} />}
            />
          </div>
          <div style={{ display: 'flex', gap: 'var(--space-xs)' }}>
            <Badge variant="cyan">All</Badge>
            <Badge variant="outline">Runbooks</Badge>
            <Badge variant="outline">Architecture</Badge>
            <Badge variant="outline">Incidents</Badge>
          </div>
        </div>
      }
    >
      <div className={styles.pageContent}>
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
                  <Badge variant="violet">{item.type}</Badge>
                </div>
              </div>
              <p className={styles.itemSummary}>{item.summary}</p>
              <div className={styles.itemFooter}>
                <span>Author: {item.author}</span>
                <span>•</span>
                <span>Last updated: {item.updatedAt}</span>
                <span>•</span>
                <span className="text-code">v{item.version}</span>
              </div>
            </article>
          ))}
        </div>
      </div>
    </PageShell>
  );
};
