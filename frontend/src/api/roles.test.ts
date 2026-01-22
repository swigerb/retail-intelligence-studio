import { describe, it, expect, vi, beforeEach, afterEach } from 'vitest';
import { fetchRoles, type RoleInfo } from './roles';

// Mock fetch globally
const mockFetch = vi.fn();
global.fetch = mockFetch;

describe('roles API', () => {
  beforeEach(() => {
    mockFetch.mockClear();
  });

  afterEach(() => {
    vi.clearAllMocks();
  });

  describe('fetchRoles', () => {
    it('fetches all roles successfully', async () => {
      const mockRoles: RoleInfo[] = [
        {
          roleName: 'decision_framer',
          displayName: 'Decision Framer',
          description: 'Clarifies the business question',
          focusAreas: ['Problem Definition', 'Scope Setting', 'Success Criteria', 'Stakeholder Alignment'],
          outputType: 'Strategic Framework',
          workflowOrder: 1
        },
        {
          roleName: 'shopper_insights',
          displayName: 'Shopper Insights',
          description: 'Evaluates customer behavior',
          focusAreas: ['Customer Segments', 'Price Sensitivity', 'Loyalty Impact', 'Basket Composition'],
          outputType: 'Behavioral Analysis',
          workflowOrder: 2
        }
      ];

      mockFetch.mockResolvedValueOnce({
        ok: true,
        json: () => Promise.resolve(mockRoles)
      });

      const result = await fetchRoles();

      expect(mockFetch).toHaveBeenCalledWith('/api/roles');
      expect(result).toEqual(mockRoles);
      expect(result).toHaveLength(2);
    });

    it('returns all 8 intelligence roles when API succeeds', async () => {
      const mockRoles: RoleInfo[] = [
        { roleName: 'decision_framer', displayName: 'Decision Framer', description: 'Test', focusAreas: ['A', 'B'], outputType: 'Framework', workflowOrder: 1 },
        { roleName: 'shopper_insights', displayName: 'Shopper Insights', description: 'Test', focusAreas: ['A', 'B'], outputType: 'Analysis', workflowOrder: 2 },
        { roleName: 'demand_forecasting', displayName: 'Demand Forecasting', description: 'Test', focusAreas: ['A', 'B'], outputType: 'Forecast', workflowOrder: 3 },
        { roleName: 'inventory_readiness', displayName: 'Inventory Readiness', description: 'Test', focusAreas: ['A', 'B'], outputType: 'Assessment', workflowOrder: 4 },
        { roleName: 'margin_impact', displayName: 'Margin Impact', description: 'Test', focusAreas: ['A', 'B'], outputType: 'Financial', workflowOrder: 5 },
        { roleName: 'digital_merchandising', displayName: 'Digital Merchandising', description: 'Test', focusAreas: ['A', 'B'], outputType: 'Strategy', workflowOrder: 6 },
        { roleName: 'risk_compliance', displayName: 'Risk & Compliance', description: 'Test', focusAreas: ['A', 'B'], outputType: 'Risk Report', workflowOrder: 7 },
        { roleName: 'executive_recommendation', displayName: 'Executive Recommendation', description: 'Test', focusAreas: ['A', 'B'], outputType: 'Recommendation', workflowOrder: 8 }
      ];

      mockFetch.mockResolvedValueOnce({
        ok: true,
        json: () => Promise.resolve(mockRoles)
      });

      const result = await fetchRoles();

      expect(result).toHaveLength(8);
      expect(result.map(r => r.roleName)).toEqual([
        'decision_framer',
        'shopper_insights',
        'demand_forecasting',
        'inventory_readiness',
        'margin_impact',
        'digital_merchandising',
        'risk_compliance',
        'executive_recommendation'
      ]);
    });

    it('throws error on failed fetch', async () => {
      mockFetch.mockResolvedValueOnce({
        ok: false,
        status: 500
      });

      await expect(fetchRoles()).rejects.toThrow('Failed to fetch roles');
    });

    it('throws error on network failure', async () => {
      mockFetch.mockRejectedValueOnce(new Error('Network error'));

      await expect(fetchRoles()).rejects.toThrow('Network error');
    });

    it('validates role data structure', async () => {
      const mockRole: RoleInfo = {
        roleName: 'shopper_insights',
        displayName: 'Shopper Insights',
        description: 'Evaluates customer behavior',
        focusAreas: ['Customer Segments', 'Price Sensitivity', 'Loyalty Impact', 'Basket Composition'],
        outputType: 'Behavioral Analysis',
        workflowOrder: 2
      };

      mockFetch.mockResolvedValueOnce({
        ok: true,
        json: () => Promise.resolve([mockRole])
      });

      const result = await fetchRoles();
      const role = result[0];

      expect(role.roleName).toBe('shopper_insights');
      expect(role.displayName).toBe('Shopper Insights');
      expect(role.description).toBe('Evaluates customer behavior');
      expect(role.focusAreas).toHaveLength(4);
      expect(role.focusAreas).toContain('Customer Segments');
      expect(role.outputType).toBe('Behavioral Analysis');
      expect(role.workflowOrder).toBe(2);
    });

    it('returns roles in the order provided by the API', async () => {
      // Simulate API returning pre-sorted roles (backend sorts by workflowOrder)
      const mockRoles: RoleInfo[] = [
        { roleName: 'decision_framer', displayName: 'Decision Framer', description: 'Test', focusAreas: [], outputType: 'Test', workflowOrder: 1 },
        { roleName: 'shopper_insights', displayName: 'Shopper Insights', description: 'Test', focusAreas: [], outputType: 'Test', workflowOrder: 2 },
        { roleName: 'margin_impact', displayName: 'Margin Impact', description: 'Test', focusAreas: [], outputType: 'Test', workflowOrder: 5 }
      ];

      mockFetch.mockResolvedValueOnce({
        ok: true,
        json: () => Promise.resolve(mockRoles)
      });

      const result = await fetchRoles();

      // Client should return roles in the order provided by backend
      expect(result).toEqual(mockRoles);
      expect(result[0].workflowOrder).toBe(1);
      expect(result[1].workflowOrder).toBe(2);
    });
  });
});
