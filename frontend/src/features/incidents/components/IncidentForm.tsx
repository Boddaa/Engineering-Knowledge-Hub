import React, { useState } from 'react';
import { AlertCircle, ArrowLeft, Save } from 'lucide-react';
import { Input } from '@/components/ui/Input';
import { Button } from '@/components/ui/Button';
import type { CreateIncidentRequest, IncidentSeverity } from '../types/incidentTypes';
import { VALID_INCIDENT_SEVERITIES } from '../utils/incidentFormatters';
import styles from './IncidentForm.module.css';

export interface IncidentFormData {
  title: string;
  problemDescription: string;
  severity: IncidentSeverity;
  createdById: string;
  symptoms: string;
}

export interface IncidentFormValidationErrors {
  title?: string;
  problemDescription?: string;
  severity?: string;
  createdById?: string;
  symptoms?: string;
}

export interface IncidentFormProps {
  isSubmitting: boolean;
  apiError: string | null;
  onSubmit: (request: CreateIncidentRequest) => void;
  onCancel: () => void;
}

const UUID_REGEX = /^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$/i;

export function validateIncidentForm(data: IncidentFormData): IncidentFormValidationErrors {
  const errors: IncidentFormValidationErrors = {};

  const trimmedTitle = data.title.trim();
  if (!trimmedTitle) {
    errors.title = 'Incident title is required.';
  } else if (trimmedTitle.length > 200) {
    errors.title = 'Incident title cannot exceed 200 characters.';
  }

  const trimmedDescription = data.problemDescription.trim();
  if (!trimmedDescription) {
    errors.problemDescription = 'Problem description is required.';
  }

  const trimmedCreatedById = data.createdById.trim();
  if (!trimmedCreatedById) {
    errors.createdById = 'Reporter ID is required.';
  } else if (!UUID_REGEX.test(trimmedCreatedById)) {
    errors.createdById = 'Reporter ID must be a valid UUID.';
  }

  return errors;
}

export const IncidentForm: React.FC<IncidentFormProps> = ({
  isSubmitting,
  apiError,
  onSubmit,
  onCancel,
}) => {
  const [formData, setFormData] = useState<IncidentFormData>({
    title: '',
    problemDescription: '',
    severity: 'Medium',
    createdById: '',
    symptoms: '',
  });

  const [errors, setErrors] = useState<IncidentFormValidationErrors>({});
  const [touched, setTouched] = useState<Record<string, boolean>>({});

  const handleChange = (
    e: React.ChangeEvent<HTMLInputElement | HTMLTextAreaElement | HTMLSelectElement>
  ) => {
    const { name, value } = e.target;
    setFormData((prev) => ({ ...prev, [name]: value }));

    if (errors[name as keyof IncidentFormValidationErrors]) {
      setErrors((prev) => ({ ...prev, [name]: undefined }));
    }
  };

  const handleBlur = (field: keyof IncidentFormData) => {
    setTouched((prev) => ({ ...prev, [field]: true }));
    const validationErrors = validateIncidentForm(formData);
    if (validationErrors[field]) {
      setErrors((prev) => ({ ...prev, [field]: validationErrors[field] }));
    }
  };

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();

    setTouched({
      title: true,
      problemDescription: true,
      severity: true,
      createdById: true,
      symptoms: true,
    });

    const validationErrors = validateIncidentForm(formData);
    if (Object.keys(validationErrors).length > 0) {
      setErrors(validationErrors);
      return;
    }

    const request: CreateIncidentRequest = {
      title: formData.title.trim(),
      problemDescription: formData.problemDescription.trim(),
      severity: formData.severity,
      createdById: formData.createdById.trim(),
      symptoms: formData.symptoms.trim() ? formData.symptoms.trim() : null,
    };

    onSubmit(request);
  };

  return (
    <form className={styles.formCard} onSubmit={handleSubmit} noValidate>
      {apiError && (
        <div className={styles.apiErrorBanner} role="alert">
          <AlertCircle size={18} className={styles.apiErrorIcon} />
          <div>
            <strong>Incident submission failed: </strong>
            <span>{apiError}</span>
          </div>
        </div>
      )}

      <div className={styles.formSection}>
        {/* Title */}
        <div className={styles.fieldGroup}>
          <label htmlFor="title" className={styles.fieldLabel}>
            Incident Title <span className={styles.requiredStar}>*</span>
          </label>
          <Input
            id="title"
            name="title"
            value={formData.title}
            onChange={handleChange}
            onBlur={() => handleBlur('title')}
            placeholder="Brief summary of the incident or operational failure..."
            error={touched.title ? errors.title : undefined}
            disabled={isSubmitting}
            required
            autoFocus
          />
        </div>

        {/* Severity & Reporter ID */}
        <div className={styles.gridTwoCol}>
          <div className={styles.fieldGroup}>
            <label htmlFor="severity" className={styles.fieldLabel}>
              Severity Rating <span className={styles.requiredStar}>*</span>
            </label>
            <select
              id="severity"
              name="severity"
              value={formData.severity}
              onChange={handleChange}
              disabled={isSubmitting}
              className={`${styles.selectInput} ${errors.severity ? styles.hasError : ''}`}
            >
              {VALID_INCIDENT_SEVERITIES.map((s) => (
                <option key={s} value={s}>
                  {s}
                </option>
              ))}
            </select>
            <span className={styles.fieldHint}>
              Select the operational urgency and blast radius
            </span>
          </div>

          <div className={styles.fieldGroup}>
            <label htmlFor="createdById" className={styles.fieldLabel}>
              Reporter / Created By ID (UUID) <span className={styles.requiredStar}>*</span>
            </label>
            <Input
              id="createdById"
              name="createdById"
              value={formData.createdById}
              onChange={handleChange}
              onBlur={() => handleBlur('createdById')}
              placeholder="e.g. 550e8400-e29b-41d4-a716-446655440000"
              error={touched.createdById ? errors.createdById : undefined}
              disabled={isSubmitting}
              required
            />
            <span className={styles.fieldHint}>
              Valid UUID representing the engineer reporting this incident
            </span>
          </div>
        </div>

        {/* Problem Description */}
        <div className={styles.fieldGroup}>
          <label htmlFor="problemDescription" className={styles.fieldLabel}>
            Problem Description <span className={styles.requiredStar}>*</span>
          </label>
          <textarea
            id="problemDescription"
            name="problemDescription"
            rows={4}
            value={formData.problemDescription}
            onChange={handleChange}
            onBlur={() => handleBlur('problemDescription')}
            placeholder="Detailed description of what failed, degraded systems, or customer impact..."
            disabled={isSubmitting}
            className={`${styles.textareaInput} ${
              touched.problemDescription && errors.problemDescription ? styles.hasError : ''
            }`}
            required
          />
          {touched.problemDescription && errors.problemDescription && (
            <span className={styles.errorText} role="alert">
              {errors.problemDescription}
            </span>
          )}
        </div>

        {/* Symptoms */}
        <div className={styles.fieldGroup}>
          <label htmlFor="symptoms" className={styles.fieldLabel}>
            Symptoms (Optional)
          </label>
          <textarea
            id="symptoms"
            name="symptoms"
            rows={3}
            value={formData.symptoms}
            onChange={handleChange}
            onBlur={() => handleBlur('symptoms')}
            placeholder="Observed error codes, metric anomalies, alerts, stack traces..."
            disabled={isSubmitting}
            className={styles.textareaInput}
          />
          <span className={styles.fieldHint}>
            Concrete symptoms that triggered detection or monitoring alarms
          </span>
        </div>
      </div>

      <div className={styles.actionsBar}>
        <Button
          type="button"
          variant="subtle"
          onClick={onCancel}
          disabled={isSubmitting}
          icon={<ArrowLeft size={16} />}
        >
          Cancel
        </Button>
        <Button
          type="submit"
          variant="primary"
          disabled={isSubmitting}
          isLoading={isSubmitting}
          icon={<Save size={16} />}
        >
          {isSubmitting ? 'Recording Incident...' : 'Record Incident'}
        </Button>
      </div>
    </form>
  );
};
