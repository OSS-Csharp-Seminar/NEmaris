const DEFAULT_NEMARIS_API_BASE_URL = "http://localhost:5199";

export function getApiBaseUrl(override?: string): string {
  return override ?? process.env.NEMARIS_API_BASE_URL ?? DEFAULT_NEMARIS_API_BASE_URL;
}
