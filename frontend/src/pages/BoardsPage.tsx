import React from 'react';
import { LayoutGrid, Plus } from 'lucide-react';
import { PageShell } from '@/components/layout/PageShell';
import { Badge } from '@/components/ui/Badge';
import { Button } from '@/components/ui/Button';
import { EmptyState } from '@/components/ui/EmptyState';

export const BoardsPage: React.FC = () => {
  return (
    <PageShell
      title="Engineering Boards"
      description="Visual triage boards for knowledge verification review queues, active incident resolutions, and RFC workflows."
      badge={<Badge variant="default">Phase F0 Workspace</Badge>}
      headerActions={
        <Button variant="primary" size="md" icon={<Plus size={15} />}>
          New Board
        </Button>
      }
    >
      <EmptyState
        icon={<LayoutGrid size={28} />}
        title="Review & Triage Boards"
        description="Verification queues and incident resolution boards will be connected to the Governance API in Phase F1."
        action={
          <Button variant="subtle" size="md" icon={<Plus size={14} />}>
            Configure Board
          </Button>
        }
      />
    </PageShell>
  );
};
