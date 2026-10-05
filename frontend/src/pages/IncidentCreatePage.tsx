import React, { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { ArrowLeft } from 'lucide-react';
import { PageShell } from '@/components/layout/PageShell';
import { IncidentForm, incidentApi, type CreateIncidentRequest } from '@/features/incidents';
import { ApiError } from '@/services/api/apiErrors';
import styles from './IncidentCreatePage.module.css';

export const IncidentCreatePage: React.FC = () => {
  const navigate = useNavigate();
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [apiError, setApiError] = useState<string | null>(null);

  const handleSubmit = async (request: CreateIncidentRequest) => {
    setIsSubmitting(true);
    setApiError(null);

    try {
      const created = await incidentApi.createIncident(request);
      navigate(`/incidents/${created.id}`);
    } catch (err: unknown) {
      if (err instanceof ApiError) {
        setApiError(err.detail || err.message);
      } else {
        setApiError('An unexpected network error occurred while recording the incident.');
      }
    } finally {
      setIsSubmitting(false);
    }
  };

  const handleCancel = () => {
    navigate('/incidents');
  };

  return (
    <PageShell
      title="Report Operational Incident"
      description="Record an ongoing outage, performance degradation, or security event to coordinate resolution."
    >
      <div className={styles.container}>
        <div className={styles.backBar}>
          <button
            type="button"
            className={styles.backLink}
            onClick={handleCancel}
            aria-label="Back to incidents list"
          >
            <ArrowLeft size={16} />
            <span>Back to Incidents</span>
          </button>
        </div>

        <IncidentForm
          isSubmitting={isSubmitting}
          apiError={apiError}
          onSubmit={handleSubmit}
          onCancel={handleCancel}
        />
      </div>
    </PageShell>
  );
};
