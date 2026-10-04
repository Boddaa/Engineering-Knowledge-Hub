import React from 'react';
import { FolderKanban, Plus } from 'lucide-react';
import { PageShell } from '@/components/layout/PageShell';
import { Badge } from '@/components/ui/Badge';
import { Button } from '@/components/ui/Button';
import { EmptyState } from '@/components/ui/EmptyState';

export const ProjectsPage: React.FC = () => {
  return (
    <PageShell
      title="Engineering Projects"
      description="Track project architecture diagrams, technical decisions, runbooks, and active product milestones."
      badge={<Badge variant="default">Phase F0 Workspace</Badge>}
      headerActions={
        <Button variant="primary" size="md" icon={<Plus size={15} />}>
          New Project
        </Button>
      }
    >
      <EmptyState
        icon={<FolderKanban size={28} />}
        title="Project Workspaces"
        description="Project management and knowledge mapping will be connected in subsequent phases. Use the sidebar to inspect Suez Toll System and API Docs runbooks."
        action={
          <Button variant="subtle" size="md" icon={<Plus size={14} />}>
            Create Project Space
          </Button>
        }
      />
    </PageShell>
  );
};
