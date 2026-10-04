import React from 'react';
import { Link } from 'react-router-dom';
import {
  Search,
  Settings,
  Bell,
  PanelRight,
  Sparkles,
} from 'lucide-react';
import { Avatar } from '../ui/Avatar';
import { IconButton } from '../ui/IconButton';
import { mockCurrentUser } from '@/lib/mock/mockData';
import styles from './CommandHeader.module.css';

export interface CommandHeaderProps {
  onOpenCommandPalette?: () => void;
  onToggleContextPanel?: () => void;
  isContextPanelOpen?: boolean;
}

export const CommandHeader: React.FC<CommandHeaderProps> = ({
  onOpenCommandPalette,
  onToggleContextPanel,
  isContextPanelOpen = true,
}) => {
  return (
    <header className={styles.header}>
      <div className={styles.leftSection}>
        <Link to="/" className={styles.brand} aria-label="Engineering Knowledge Hub Home">
          <div className={styles.brandIcon}>
            <Sparkles size={16} />
          </div>
          <div className={styles.brandText}>
            <span>Engineering Hub</span>
            <span className={styles.versionBadge}>v1.0</span>
          </div>
        </Link>
      </div>

      <div className={styles.centerSection}>
        <button
          type="button"
          className={styles.commandTrigger}
          onClick={onOpenCommandPalette}
          aria-label="Search knowledge and trigger command palette (Ctrl+K)"
        >
          <div className={styles.triggerLeft}>
            <Search size={15} />
            <span>Search knowledge, incidents, architecture, or nodes...</span>
          </div>
          <div className={styles.shortcut}>
            <kbd className={styles.kbd}>Ctrl</kbd>
            <kbd className={styles.kbd}>K</kbd>
          </div>
        </button>
      </div>

      <div className={styles.rightSection}>
        <IconButton
          icon={<Bell size={18} />}
          label="Notifications"
          variant="ghost"
          size="sm"
        />
        <IconButton
          icon={<Settings size={18} />}
          label="Settings"
          variant="ghost"
          size="sm"
        />
        <IconButton
          icon={<PanelRight size={18} />}
          label={isContextPanelOpen ? 'Collapse Context Panel' : 'Expand Context Panel'}
          variant={isContextPanelOpen ? 'subtle' : 'ghost'}
          size="sm"
          onClick={onToggleContextPanel}
        />

        <div className={styles.userInfo}>
          <Avatar
            name={mockCurrentUser.name}
            size="sm"
            status="online"
          />
          <div className={styles.userDetails}>
            <span className={styles.userName}>{mockCurrentUser.name}</span>
            <span className={styles.userRole}>{mockCurrentUser.role}</span>
          </div>
        </div>
      </div>
    </header>
  );
};
