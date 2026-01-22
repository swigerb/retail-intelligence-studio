import { PersonaContext, DecisionRequest, DecisionSubmitResponse, DecisionResult, RetailPersona } from '../types';

const API_BASE = '/api';

/**
 * Fetches all available retail personas.
 */
export async function fetchPersonas(): Promise<PersonaContext[]> {
  const response = await fetch(`${API_BASE}/personas`);
  if (!response.ok) {
    throw new Error('Failed to fetch personas');
  }
  return response.json();
}

/**
 * Fetches a sample decision for a specific persona.
 */
export async function fetchSampleDecision(persona: RetailPersona, index = 0): Promise<string> {
  const response = await fetch(`${API_BASE}/personas/${persona}/sample?index=${index}`);
  if (!response.ok) {
    throw new Error('Failed to fetch sample decision');
  }
  const data = await response.json();
  return data.decisionText;
}

/**
 * Submits a decision for evaluation.
 */
export async function submitDecision(request: DecisionRequest): Promise<DecisionSubmitResponse> {
  const response = await fetch(`${API_BASE}/decisions`, {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json',
    },
    body: JSON.stringify(request),
  });
  
  if (!response.ok) {
    throw new Error('Failed to submit decision');
  }
  
  return response.json();
}

/**
 * Fetches a decision result by ID.
 */
export async function fetchDecision(decisionId: string): Promise<DecisionResult> {
  const response = await fetch(`${API_BASE}/decisions/${decisionId}`);
  if (!response.ok) {
    throw new Error('Failed to fetch decision');
  }
  return response.json();
}

/**
 * Fetches recent decisions.
 */
export async function fetchRecentDecisions(skip = 0, take = 20): Promise<DecisionResult[]> {
  const response = await fetch(`${API_BASE}/decisions?skip=${skip}&take=${take}`);
  if (!response.ok) {
    throw new Error('Failed to fetch decisions');
  }
  return response.json();
}
