/**
 * API client for fetching Intelligence Role metadata.
 */

const API_BASE = '/api';

/**
 * Role information returned from the API.
 */
export interface RoleInfo {
  roleName: string;
  displayName: string;
  description: string;
  focusAreas: string[];
  outputType: string;
  workflowOrder: number;
}

/**
 * Fetches all intelligence roles with their metadata.
 */
export async function fetchRoles(): Promise<RoleInfo[]> {
  const response = await fetch(`${API_BASE}/roles`);
  if (!response.ok) {
    throw new Error('Failed to fetch roles');
  }
  return response.json();
}
