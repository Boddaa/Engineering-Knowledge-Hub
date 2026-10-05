import React from 'react';
import { NavLink } from 'react-router-dom';
import {
  Home,
  BookOpen,
  FolderKanban,
  FileText,
  LayoutGrid,
  AlertTriangle,
  Plus,
  Folder,
  Network,
  ChevronLeft,
  ChevronRight,
} from 'lucide-react';
import { Button } from '../ui/Button';
import {
  mockNavigationItems,
  mockWorkspaceItems,
  mockGraphItems,
} from '@/lib/mock/mockData';
import styles from './Sidebar.module.css';

export interface SidebarProps {
  isCollapsed: boolean;
  onToggleCollapse: () => void;
  onNewNode?: () => void;
}

export const Sidebar: React.FC<SidebarProps> = ({
  isCollapsed,
  onToggleCollapse,
  onNewNode,
}) => {
  const getNavIcon = (iconName: string) => {
    switch (iconName) {
      case 'Home':
        return <Home size={18} />;
      case 'BookOpen':
        return <BookOpen size={18} />;
      case 'AlertTriangle':
        return <AlertTriangle size={18} />;
      case 'FolderKanban':
        return <FolderKanban size={18} />;
      case 'FileText':
        return <FileText size={18} />;
      case 'LayoutGrid':
        return <LayoutGrid size={18} />;
      default:
        return <BookOpen size={18} />;
    }
  };

  return (
    <aside
      className={`${styles.sidebar} ${isCollapsed ? styles.isCollapsed : ''}`}
      aria-label="Main Navigation"
    >
      <div className={styles.actionSection}>
        <Button
          variant="primary"
          size="md"
          fullWidth
          icon={<Plus size={16} />}
          onClick={onNewNode}
          title="New Node / Action"
        >
          {!isCollapsed && 'New Node'}
        </Button>
      </div>

      <nav className={styles.navGroup} aria-label="Primary sections">
        <div className={styles.sectionHeader}>
          <span>Navigation</span>
        </div>
        {mockNavigationItems.map((item) => (
          <NavLink
            key={item.path}
            to={item.path}
            end={item.path === '/'}
            className={({ isActive }) =>
              `${styles.navItem} ${isActive ? styles.isActive : ''}`
            }
            title={item.label}
          >
            <span className={styles.navIcon}>{getNavIcon(item.icon)}</span>
            <span className={styles.navLabel}>{item.label}</span>
            {item.badge && <span className={styles.navBadge}>{item.badge}</span>}
          </NavLink>
        ))}
      </nav>

      <div className={styles.navGroup} aria-label="Workspaces">
        <div className={styles.sectionHeader}>
          <span>Workspaces</span>
          <Folder size={12} />
        </div>
        {mockWorkspaceItems.map((item) => (
          <div
            key={item.id}
            className={styles.subItem}
            title={`${item.title} (${item.count} items)`}
          >
            <span className={styles.itemDot} />
            {!isCollapsed && (
              <>
                <span className={styles.navLabel}>{item.title}</span>
                <span className={styles.navBadge}>{item.count}</span>
              </>
            )}
          </div>
        ))}
      </div>

      <div className={styles.navGroup} aria-label="Graph Nodes">
        <div className={styles.sectionHeader}>
          <span>Graph Nodes</span>
          <Network size={12} />
        </div>
        {mockGraphItems.map((item) => (
          <div
            key={item.id}
            className={styles.subItem}
            title={`${item.name} (${item.references} refs)`}
          >
            <span className={styles.itemDot} />
            {!isCollapsed && (
              <>
                <span className={styles.navLabel}>{item.name}</span>
                <span className={styles.navBadge}>{item.references}</span>
              </>
            )}
          </div>
        ))}
      </div>

      <div className={styles.footer}>
        <button
          type="button"
          onClick={onToggleCollapse}
          className={styles.collapseButton}
          aria-label={isCollapsed ? 'Expand sidebar' : 'Collapse sidebar'}
        >
          {isCollapsed ? <ChevronRight size={16} /> : <ChevronLeft size={16} />}
          {!isCollapsed && <span>Collapse Sidebar</span>}
        </button>
      </div>
    </aside>
  );
};
