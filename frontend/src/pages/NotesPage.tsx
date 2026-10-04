import React from 'react';
import { FileText, Plus } from 'lucide-react';
import { PageShell } from '@/components/layout/PageShell';
import { Badge } from '@/components/ui/Badge';
import { Button } from '@/components/ui/Button';
import { EmptyState } from '@/components/ui/EmptyState';

export const NotesPage: React.FC = () => {
  return (
    <PageShell
      title="Engineering Notes"
      description="Quick scratchpads, meeting takeaways, architectural spikes, and unverified knowledge drafts."
      badge={<Badge variant="default">Phase F0 Workspace</Badge>}
      headerActions={
        <Button variant="primary" size="md" icon={<Plus size={15} />}>
          New Note
        </Button>
      }
    >
      <EmptyState
        icon={<FileText size={28} />}
        title="Personal and Team Engineering Notes"
        description="Draft notes can be converted to formal knowledge entries when approved by governance workflows in Phase F1."
        action={
          <Button variant="subtle" size="md" icon={<Plus size={14} />}>
            Create First Note
          </Button>
        }
      />
    </PageShell>
  );
};
