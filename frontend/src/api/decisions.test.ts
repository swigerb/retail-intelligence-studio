import { describe, it, expect, vi, beforeEach, afterEach } from 'vitest';
import { submitDecision, fetchPersonas, fetchSampleDecision, fetchDecision } from './decisions';
import { RetailPersona } from '../types';

// Mock fetch globally
const mockFetch = vi.fn();
global.fetch = mockFetch;

describe('decisions API', () => {
  beforeEach(() => {
    mockFetch.mockClear();
  });

  afterEach(() => {
    vi.clearAllMocks();
  });

  describe('submitDecision', () => {
    it('submits a decision successfully', async () => {
      const mockResponse = {
        decisionId: 'test-123',
        status: 'Running',
        eventsUrl: '/api/decisions/test-123/events'
      };

      mockFetch.mockResolvedValueOnce({
        ok: true,
        json: () => Promise.resolve(mockResponse)
      });

      const request = {
        decisionText: 'Should we expand our product line?',
        persona: RetailPersona.Grocery,
        useSampleData: true
      };

      const result = await submitDecision(request);

      expect(mockFetch).toHaveBeenCalledWith('/api/decisions', {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify(request)
      });
      expect(result).toEqual(mockResponse);
    });

    it('throws error on failed submission', async () => {
      mockFetch.mockResolvedValueOnce({
        ok: false,
        status: 500
      });

      const request = {
        decisionText: 'Test decision',
        persona: RetailPersona.Grocery,
        useSampleData: false
      };

      await expect(submitDecision(request)).rejects.toThrow('Failed to submit decision');
    });
  });

  describe('fetchPersonas', () => {
    it('fetches all personas', async () => {
      const mockPersonas = [
        { persona: RetailPersona.Grocery, displayName: 'Grocery Retail' }
      ];

      mockFetch.mockResolvedValueOnce({
        ok: true,
        json: () => Promise.resolve(mockPersonas)
      });

      const result = await fetchPersonas();

      expect(mockFetch).toHaveBeenCalledWith('/api/personas');
      expect(result).toEqual(mockPersonas);
    });

    it('throws error on failure', async () => {
      mockFetch.mockResolvedValueOnce({
        ok: false
      });

      await expect(fetchPersonas()).rejects.toThrow('Failed to fetch personas');
    });
  });

  describe('fetchSampleDecision', () => {
    it('fetches sample decision for a persona', async () => {
      mockFetch.mockResolvedValueOnce({
        ok: true,
        json: () => Promise.resolve({ decisionText: 'Sample decision text' })
      });

      const result = await fetchSampleDecision(RetailPersona.Grocery);

      expect(mockFetch).toHaveBeenCalledWith('/api/personas/Grocery/sample?index=0');
      expect(result).toBe('Sample decision text');
    });

    it('respects index parameter', async () => {
      mockFetch.mockResolvedValueOnce({
        ok: true,
        json: () => Promise.resolve({ decisionText: 'Another sample' })
      });

      await fetchSampleDecision(RetailPersona.Grocery, 2);

      expect(mockFetch).toHaveBeenCalledWith('/api/personas/Grocery/sample?index=2');
    });

    it('throws error on failure', async () => {
      mockFetch.mockResolvedValueOnce({
        ok: false
      });

      await expect(fetchSampleDecision(RetailPersona.Grocery)).rejects.toThrow('Failed to fetch sample decision');
    });
  });

  describe('fetchDecision', () => {
    it('fetches decision by ID', async () => {
      const mockDecision = {
        decisionId: 'test-123',
        status: 'Completed'
      };

      mockFetch.mockResolvedValueOnce({
        ok: true,
        json: () => Promise.resolve(mockDecision)
      });

      const result = await fetchDecision('test-123');

      expect(mockFetch).toHaveBeenCalledWith('/api/decisions/test-123');
      expect(result).toEqual(mockDecision);
    });

    it('throws error on failure', async () => {
      mockFetch.mockResolvedValueOnce({
        ok: false
      });

      await expect(fetchDecision('test-123')).rejects.toThrow('Failed to fetch decision');
    });
  });
});
