import React, { useState, useEffect } from 'react';
import { Outlet } from 'react-router-dom';
import { Search, Sparkles, X } from 'lucide-react';
import { CommandHeader } from './CommandHeader';
import { Sidebar } from './Sidebar';
import { ContextPanel } from './ContextPanel';
import { Badge } from '../ui/Badge';
import { IconButton } from '../ui/IconButton';
import styles from './AppLayout.module.css';

export const AppLayout: React.FC = () => {
  const [isSidebarCollapsed, setIsSidebarCollapsed] = useState(false);
  const [isContextPanelOpen, setIsContextPanelOpen] = useState(true);
  const [isCommandPaletteOpen, setIsCommandPaletteOpen] = useState(false);

  // Global Ctrl+K / Cmd+K listener
  useEffect(() => {
    const handleKeyDown = (e: KeyboardEvent) => {
      if ((e.ctrlKey || e.metaKey) && e.key.toLowerCase() === 'k') {
        e.preventDefault();
        setIsCommandPaletteOpen((prev) => !prev);
      } else if (e.key === 'Escape') {
        setIsCommandPaletteOpen(false);
      }
    };

    window.addEventListener('keydown', handleKeyDown);
    return () => window.removeEventListener('keydown', handleKeyDown);
  }, []);

  return (
    <div className={styles.layoutRoot}>
      {/* Top Application Header */}
      <CommandHeader
        onOpenCommandPalette={() => setIsCommandPaletteOpen(true)}
        onToggleContextPanel={() => setIsContextPanelOpen((prev) => !prev)}
        isContextPanelOpen={isContextPanelOpen}
      />

      <div className={styles.bodyContainer}>
        {/* Left Navigation Sidebar */}
        <Sidebar
          isCollapsed={isSidebarCollapsed}
          onToggleCollapse={() => setIsSidebarCollapsed((prev) => !prev)}
          onNewNode={() => setIsCommandPaletteOpen(true)}
        />

        {/* Central Workspace Router Outlet */}
        <div className={styles.workspaceOutlet}>
          <Outlet />
        </div>

        {/* Right Context Inspector Panel */}
        <ContextPanel
          isOpen={isContextPanelOpen}
          onClose={() => setIsContextPanelOpen(false)}
        />
      </div>

      {/* Command Palette Trigger Modal (F0 Shell Prototype) */}
      {isCommandPaletteOpen && (
        <div
          className={styles.paletteBackdrop}
          onClick={() => setIsCommandPaletteOpen(false)}
          role="dialog"
          aria-modal="true"
          aria-label="Command Palette Shell Trigger"
        >
          <div
            className={styles.paletteDialog}
            onClick={(e) => e.stopPropagation()}
          >
            <div className={styles.paletteHeader}>
              <Search size={18} />
              <input
                type="text"
                autoFocus
                placeholder="Type a command, query, or node name..."
                className={styles.paletteInput}
              />
              <IconButton
                icon={<X size={16} />}
                label="Close palette"
                variant="ghost"
                size="sm"
                onClick={() => setIsCommandPaletteOpen(false)}
              />
            </div>

            <div className={styles.paletteBody}>
              <div style={{ display: 'flex', alignItems: 'center', gap: '8px' }}>
                <Badge variant="cyan" icon={<Sparkles size={12} />}>
                  Phase F0 Trigger
                </Badge>
                <span className="text-secondary" style={{ fontSize: '13px' }}>
                  Shell Integration Active
                </span>
              </div>
              <p className={styles.paletteNotice}>
                The global search shortcut and modal trigger are established in the application shell.
                Full semantic search, fast node filtering, and contextual command dispatch are
                scheduled for implementation in subsequent frontend phases.
              </p>
            </div>

            <div className={styles.paletteFooter}>
              <span>Press ESC to close</span>
              <span>Navigation Shell: Ready</span>
            </div>
          </div>
        </div>
      )}
    </div>
  );
};
