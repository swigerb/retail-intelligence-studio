import { describe, it, expect, vi, beforeEach, afterEach } from 'vitest';
import { render, screen, fireEvent, waitFor } from '@testing-library/react';
import { IntelligenceGrid } from './IntelligenceGrid';
import type { DecisionEvent } from '../types';
import { RetailPersona, AnalysisPhase, INTELLIGENCE_ROLES } from '../types';
import * as rolesApi from '../api/roles';

// Mock the roles API
vi.mock('../api/roles', () => ({
  fetchRoles: vi.fn()
}));

const mockFetchRoles = rolesApi.fetchRoles as ReturnType<typeof vi.fn>;

describe('IntelligenceGrid', () => {
  beforeEach(() => {
    // Default: API returns empty and component uses static fallback
    mockFetchRoles.mockRejectedValue(new Error('API unavailable'));
  });

  afterEach(() => {
    vi.clearAllMocks();
  });

  it('renders all 8 intelligence role cards', () => {
    render(<IntelligenceGrid events={[]} isAnalyzing={false} />);

    // Check for all role names
    expect(screen.getByText('Decision Framer')).toBeInTheDocument();
    expect(screen.getByText('Shopper Insights')).toBeInTheDocument();
    expect(screen.getByText('Demand Forecasting')).toBeInTheDocument();
    expect(screen.getByText('Inventory Readiness')).toBeInTheDocument();
    expect(screen.getByText('Margin Impact')).toBeInTheDocument();
    expect(screen.getByText('Digital Merchandising')).toBeInTheDocument();
    expect(screen.getByText('Risk & Compliance')).toBeInTheDocument();
    expect(screen.getByText('Executive Recommendation')).toBeInTheDocument();
  });

  it('renders role descriptions', () => {
    render(<IntelligenceGrid events={[]} isAnalyzing={false} />);

    // Static fallback uses longer descriptions
    expect(screen.getByText(/Clarifies the business question/)).toBeInTheDocument();
    expect(screen.getByText(/Evaluates customer behavior/)).toBeInTheDocument();
  });

  it('shows correct styling when analyzing', () => {
    const events: DecisionEvent[] = [
      {
        decisionId: 'test-1',
        persona: RetailPersona.Grocery,
        roleName: 'shopper_insights',
        phase: AnalysisPhase.Analyzing,
        message: 'Analyzing customer behavior...',
        timestamp: new Date().toISOString(),
        sequenceNumber: 1
      }
    ];

    const { container } = render(<IntelligenceGrid events={events} isAnalyzing={true} />);
    
    // The component should render with events
    expect(container.querySelector('.grid')).toBeInTheDocument();
  });

  it('handles events correctly', () => {
    const events: DecisionEvent[] = [
      {
        decisionId: 'test-1',
        persona: RetailPersona.Grocery,
        roleName: 'demand_forecasting',
        phase: AnalysisPhase.Analyzing,
        message: 'Projecting sales impact...',
        timestamp: new Date().toISOString(),
        sequenceNumber: 1
      }
    ];

    const { container } = render(<IntelligenceGrid events={events} isAnalyzing={true} />);
    
    // The component should render with the events
    expect(container.querySelector('.grid')).toBeInTheDocument();
    // And show "Analysis in progress" indicator
    expect(screen.getByText('Analysis in progress')).toBeInTheDocument();
  });

  it('handles completed phase events', () => {
    const events: DecisionEvent[] = [
      {
        decisionId: 'test-1',
        persona: RetailPersona.Grocery,
        roleName: 'decision_framer',
        phase: AnalysisPhase.Completed,
        message: 'Decision framed successfully',
        timestamp: new Date().toISOString(),
        sequenceNumber: 1
      }
    ];

    const { container } = render(<IntelligenceGrid events={events} isAnalyzing={false} />);
    
    // Should render with completed event
    expect(container).toBeInTheDocument();
  });

  it('renders with proper grid layout', () => {
    const { container } = render(<IntelligenceGrid events={[]} isAnalyzing={false} />);

    // Should have grid container
    const grid = container.querySelector('.grid');
    expect(grid).toBeInTheDocument();
  });

  it('shows icons for each role', () => {
    const { container } = render(<IntelligenceGrid events={[]} isAnalyzing={false} />);

    // Each role card should have an icon (SVG)
    const svgs = container.querySelectorAll('svg');
    expect(svgs.length).toBeGreaterThanOrEqual(8);
  });

  // ========== NEW TESTS FOR HOVER EXPANSION AND METADATA ==========

  describe('hover expansion behavior', () => {
    it('shows workflow order badge on hover', async () => {
      render(<IntelligenceGrid events={[]} isAnalyzing={false} />);

      // Find the Decision Framer card and hover
      const decisionFramerCard = screen.getByText('Decision Framer').closest('div[class*="rounded-xl"]');
      expect(decisionFramerCard).toBeInTheDocument();
      
      fireEvent.mouseEnter(decisionFramerCard!);

      // Should show workflow order badge #1
      await waitFor(() => {
        expect(screen.getByText('#1')).toBeInTheDocument();
      });
    });

    it('shows output type pill on hover', async () => {
      render(<IntelligenceGrid events={[]} isAnalyzing={false} />);

      // Find and hover the Shopper Insights card
      const shopperCard = screen.getByText('Shopper Insights').closest('div[class*="rounded-xl"]');
      fireEvent.mouseEnter(shopperCard!);

      // Should show output type
      await waitFor(() => {
        expect(screen.getByText('Behavioral Analysis')).toBeInTheDocument();
      });
    });

    it('shows focus area tags on hover', async () => {
      render(<IntelligenceGrid events={[]} isAnalyzing={false} />);

      // Find and hover the Shopper Insights card
      const shopperCard = screen.getByText('Shopper Insights').closest('div[class*="rounded-xl"]');
      fireEvent.mouseEnter(shopperCard!);

      // Should show focus areas
      await waitFor(() => {
        expect(screen.getByText('Customer Segments')).toBeInTheDocument();
        expect(screen.getByText('Price Sensitivity')).toBeInTheDocument();
      });
    });

    it('hides expanded content on mouse leave', async () => {
      render(<IntelligenceGrid events={[]} isAnalyzing={false} />);

      const shopperCard = screen.getByText('Shopper Insights').closest('div[class*="rounded-xl"]');
      
      // Hover to show expanded content
      fireEvent.mouseEnter(shopperCard!);
      await waitFor(() => {
        expect(screen.getByText('Behavioral Analysis')).toBeInTheDocument();
      });

      // Leave to hide expanded content
      fireEvent.mouseLeave(shopperCard!);
      await waitFor(() => {
        expect(screen.queryByText('Behavioral Analysis')).not.toBeInTheDocument();
      });
    });

    it('does not show expanded content when card is in analyzing state', async () => {
      const events: DecisionEvent[] = [
        {
          decisionId: 'test-1',
          persona: RetailPersona.Grocery,
          roleName: 'shopper_insights',
          phase: AnalysisPhase.Analyzing,
          message: 'Analyzing...',
          timestamp: new Date().toISOString(),
          sequenceNumber: 1
        }
      ];

      render(<IntelligenceGrid events={events} isAnalyzing={true} />);

      const shopperCard = screen.getByText('Shopper Insights').closest('div[class*="rounded-xl"]');
      fireEvent.mouseEnter(shopperCard!);

      // Wait a bit and verify expanded content is NOT shown
      await new Promise(resolve => setTimeout(resolve, 100));
      expect(screen.queryByText('Behavioral Analysis')).not.toBeInTheDocument();
    });
  });

  describe('API integration', () => {
    it('fetches roles from API on mount', async () => {
      mockFetchRoles.mockResolvedValueOnce([
        {
          roleName: 'decision_framer',
          displayName: 'Decision Framer',
          description: 'Clarifies the business question',
          focusAreas: ['Problem Definition', 'Scope Setting'],
          outputType: 'Strategic Framework',
          workflowOrder: 1
        }
      ]);

      render(<IntelligenceGrid events={[]} isAnalyzing={false} />);

      await waitFor(() => {
        expect(mockFetchRoles).toHaveBeenCalledTimes(1);
      });
    });

    it('uses static fallback when API fails', async () => {
      mockFetchRoles.mockRejectedValueOnce(new Error('Network error'));

      render(<IntelligenceGrid events={[]} isAnalyzing={false} />);

      // Should still render all roles from static fallback
      await waitFor(() => {
        expect(screen.getByText('Decision Framer')).toBeInTheDocument();
        expect(screen.getByText('Shopper Insights')).toBeInTheDocument();
      });
    });

    it('updates roles when API returns data', async () => {
      const apiRoles = [
        {
          roleName: 'decision_framer',
          displayName: 'Decision Framer',
          description: 'API Updated Description',
          focusAreas: ['API Focus Area 1', 'API Focus Area 2'],
          outputType: 'API Output Type',
          workflowOrder: 1
        }
      ];

      mockFetchRoles.mockResolvedValueOnce(apiRoles);

      render(<IntelligenceGrid events={[]} isAnalyzing={false} />);

      // Wait for API to be called and component to update
      await waitFor(() => {
        expect(mockFetchRoles).toHaveBeenCalled();
      });

      // The description should be updated from API
      await waitFor(() => {
        expect(screen.getByText('API Updated Description')).toBeInTheDocument();
      });
    });
  });

  describe('static fallback data', () => {
    it('has correct structure for all static roles', () => {
      // Validate static fallback data
      expect(INTELLIGENCE_ROLES).toHaveLength(8);
      
      INTELLIGENCE_ROLES.forEach(role => {
        expect(role.name).toBeDefined();
        expect(role.displayName).toBeDefined();
        expect(role.description).toBeDefined();
        expect(role.focusAreas).toBeDefined();
        expect(Array.isArray(role.focusAreas)).toBe(true);
        expect(role.outputType).toBeDefined();
        expect(role.workflowOrder).toBeDefined();
        expect(typeof role.workflowOrder).toBe('number');
      });
    });

    it('has focus areas for each role in static data', () => {
      INTELLIGENCE_ROLES.forEach(role => {
        expect(role.focusAreas.length).toBeGreaterThan(0);
      });
    });

    it('has correct workflow order sequence', () => {
      const orders = INTELLIGENCE_ROLES.map(r => r.workflowOrder).sort((a, b) => a - b);
      expect(orders).toEqual([1, 2, 3, 4, 5, 6, 7, 8]);
    });
  });

  describe('role metadata display', () => {
    it('renders all focus areas when expanded', async () => {
      render(<IntelligenceGrid events={[]} isAnalyzing={false} />);

      // Find Decision Framer and hover
      const card = screen.getByText('Decision Framer').closest('div[class*="rounded-xl"]');
      fireEvent.mouseEnter(card!);

      // Check that all focus areas are rendered
      await waitFor(() => {
        // Decision Framer's focus areas from static fallback data
        expect(screen.getByText('Business Question')).toBeInTheDocument();
        expect(screen.getByText('Success Criteria')).toBeInTheDocument();
        expect(screen.getByText('Scope Definition')).toBeInTheDocument();
        expect(screen.getByText('Key Assumptions')).toBeInTheDocument();
      });
    });

    it('displays "Output:" label in expanded view', async () => {
      render(<IntelligenceGrid events={[]} isAnalyzing={false} />);

      const card = screen.getByText('Decision Framer').closest('div[class*="rounded-xl"]');
      fireEvent.mouseEnter(card!);

      await waitFor(() => {
        expect(screen.getByText('Output:')).toBeInTheDocument();
      });
    });
  });
});
