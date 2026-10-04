import React from 'react';
import {
  X,
  Link2,
  ExternalLink,
  Activity,
  Zap,
  Network,
} from 'lucide-react';
import { IconButton } from '../ui/IconButton';
import { Button } from '../ui/Button';
import { mockConnections, mockRecentActivity } from '@/lib/mock/mockData';
import styles from './ContextPanel.module.css';

export interface ContextPanelProps {
  isOpen: boolean;
  onClose: () => void;
}

export const ContextPanel: React.FC<ContextPanelProps> = ({
  isOpen,
  onClose,
}) => {
  return (
    <aside
      className={`${styles.contextPanel} ${!isOpen ? styles.isClosed : ''}`}
      aria-label="Context and Inspector Panel"
    >
      <div className={styles.panelHeader}>
        <div className={styles.titleWrapper}>
          <h2 className={styles.panelTitle}>Context Inspector</h2>
          <span className={styles.mockTag} title="F0 static representation of graph inspector">
            F0 Mock
          </span>
        </div>
        <IconButton
          icon={<X size={16} />}
          label="Close context panel"
          variant="ghost"
          size="sm"
          onClick={onClose}
        />
      </div>

      {/* Quick Actions Section */}
      <section className={styles.section} aria-label="Quick Actions">
        <div className={styles.sectionHeading}>
          <span>Quick Actions</span>
          <Zap size={12} />
        </div>
        <div className={styles.quickActionsGrid}>
          <Button variant="subtle" size="sm" icon={<Link2 size={13} />}>
            Link Node
          </Button>
          <Button variant="subtle" size="sm" icon={<Network size={13} />}>
            Graph View
          </Button>
        </div>
      </section>

      {/* Connections / Graph Relatives */}
      <section className={styles.section} aria-label="Related Connections">
        <div className={styles.sectionHeading}>
          <span>Connections ({mockConnections.length})</span>
          <Network size={12} />
        </div>
        <div className={styles.connectionList}>
          {mockConnections.map((conn) => (
            <div key={conn.id} className={styles.connectionItem}>
              <div className={styles.connectionTarget}>
                <Link2 size={12} />
                <span>{conn.target}</span>
              </div>
              <span className={styles.connectionType}>{conn.type}</span>
            </div>
          ))}
        </div>
      </section>

      {/* Referenced In */}
      <section className={styles.section} aria-label="Referenced In">
        <div className={styles.sectionHeading}>
          <span>Referenced In</span>
          <ExternalLink size={12} />
        </div>
        <div className={styles.connectionList}>
          <div className={styles.connectionItem}>
            <span>Architecture Overview</span>
            <span className={styles.connectionType}>doc</span>
          </div>
          <div className={styles.connectionItem}>
            <span>Incident Postmortem #104</span>
            <span className={styles.connectionType}>incident</span>
          </div>
        </div>
      </section>

      {/* Recent Activity Feed */}
      <section className={styles.section} aria-label="Recent Activity">
        <div className={styles.sectionHeading}>
          <span>Activity Stream</span>
          <Activity size={12} />
        </div>
        <div className={styles.activityList}>
          {mockRecentActivity.slice(0, 4).map((act) => (
            <div key={act.id} className={styles.activityItem}>
              <span className={styles.activityDot} />
              <div className={styles.activityContent}>
                <p className={styles.activityDesc}>
                  <strong>{act.author}</strong> {act.description}
                </p>
                <time className={styles.activityTime}>{act.timestamp}</time>
              </div>
            </div>
          ))}
        </div>
      </section>
    </aside>
  );
};
