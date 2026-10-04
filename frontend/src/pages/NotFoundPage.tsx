import React from 'react';
import { useNavigate } from 'react-router-dom';
import { FileQuestion, ArrowLeft } from 'lucide-react';
import { PageShell } from '@/components/layout/PageShell';
import { Button } from '@/components/ui/Button';
import { EmptyState } from '@/components/ui/EmptyState';

export const NotFoundPage: React.FC = () => {
  const navigate = useNavigate();

  return (
    <PageShell
      title="404 — Page Not Found"
      description="The requested route does not exist in the Engineering Knowledge Hub."
    >
      <EmptyState
        icon={<FileQuestion size={32} />}
        title="Destination Not Found"
        description="The navigation route you requested is invalid or has moved. Return to the engineering cockpit to resume navigation."
        action={
          <Button
            variant="primary"
            size="md"
            icon={<ArrowLeft size={16} />}
            onClick={() => navigate('/')}
          >
            Return to Cockpit
          </Button>
        }
      />
    </PageShell>
  );
};
