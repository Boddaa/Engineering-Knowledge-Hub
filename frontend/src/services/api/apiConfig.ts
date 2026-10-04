/**
 * API configuration settings.
 * Defaults to the local ASP.NET Core API endpoint or Vite reverse proxy.
 */
export const API_CONFIG = {
  baseUrl: (import.meta.env.VITE_API_BASE_URL as string) || '/api',
  timeoutMs: 30000,
  defaultHeaders: {
    'Content-Type': 'application/json',
    'Accept': 'application/json, application/problem+json',
  },
};
