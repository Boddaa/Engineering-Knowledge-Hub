/**
 * RFC 7807 Problem Details representation emitted by the backend GlobalExceptionHandler.
 */
export interface ProblemDetails {
  status?: number;
  title?: string;
  type?: string;
  detail?: string;
  instance?: string;
  errors?: Record<string, string[]>;
}

export class ApiError extends Error {
  public readonly status: number;
  public readonly title?: string;
  public readonly detail?: string;
  public readonly problemDetails?: ProblemDetails;

  constructor(status: number, message: string, problemDetails?: ProblemDetails) {
    super(message);
    this.name = 'ApiError';
    this.status = status;
    this.title = problemDetails?.title;
    this.detail = problemDetails?.detail || message;
    this.problemDetails = problemDetails;
  }

  public get isNotFound(): boolean {
    return this.status === 404;
  }

  public get isValidation(): boolean {
    return this.status === 400;
  }

  public get isConflict(): boolean {
    return this.status === 409;
  }

  public get isServerError(): boolean {
    return this.status >= 500;
  }
}
