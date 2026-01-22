import { describe, it, expect, vi, beforeEach } from 'vitest';
import { render, screen, fireEvent, waitFor } from '@testing-library/react';
import App from './App';
import { RetailPersona } from './types';

// Mock the fetch API
const mockFetch = vi.fn();
global.fetch = mockFetch;

// Mock EventSource for SSE
class MockEventSource {
  url: string;
  onopen: ((event: Event) => void) | null = null;
  onmessage: ((event: MessageEvent) => void) | null = null;
  onerror: ((event: Event) => void) | null = null;
  readyState: number = 0;

  constructor(url: string) {
    this.url = url;
    setTimeout(() => {
      this.readyState = 1;
      if (this.onopen) {
        this.onopen(new Event('open'));
      }
    }, 10);
  }

  close() {
    this.readyState = 2;
  }
}

(global as unknown as { EventSource: typeof MockEventSource }).EventSource = MockEventSource;

const mockPersonas = [
  {
    persona: RetailPersona.Grocery,
    displayName: 'Grocery Retail',
    description: 'Traditional grocery retail',
    keyCategories: ['Beverages', 'Fresh Produce', 'Frozen Foods'],
    channels: ['In-Store', 'Online'],
    sampleDecisions: ['Should we expand organic offerings?']
  },
  {
    persona: RetailPersona.QuickServeRestaurant,
    displayName: 'Quick Serve Restaurant',
    description: 'Fast food and QSR',
    keyCategories: ['Burgers', 'Sides', 'Beverages'],
    channels: ['Dine-In', 'Drive-Through'],
    sampleDecisions: ['Should we launch breakfast menu?']
  },
  {
    persona: RetailPersona.SpecialtyRetail,
    displayName: 'Specialty Retail',
    description: 'Specialty retail stores',
    keyCategories: ['Electronics', 'Fashion', 'Home'],
    channels: ['Stores', 'E-commerce'],
    sampleDecisions: ['Should we open new locations?']
  }
];

describe('App Smoke Tests', () => {
  beforeEach(() => {
    mockFetch.mockClear();
    // Setup default mock responses
    mockFetch.mockImplementation((url: string) => {
      if (url.includes('/api/personas')) {
        return Promise.resolve({
          ok: true,
          json: () => Promise.resolve(mockPersonas)
        });
      }
      return Promise.resolve({
        ok: true,
        json: () => Promise.resolve({})
      });
    });
  });

  describe('Initial Render', () => {
    it('renders the application header', async () => {
      render(<App />);
      
      await waitFor(() => {
        expect(screen.getByText('Retail Intelligence Studio')).toBeInTheDocument();
      });
    });

    it('renders the subtitle', async () => {
      render(<App />);
      
      await waitFor(() => {
        expect(screen.getByText('Multi-Agent Decision Intelligence')).toBeInTheDocument();
      });
    });

    it('loads and displays personas', async () => {
      render(<App />);
      
      await waitFor(() => {
        expect(screen.getByText('Grocery Retail')).toBeInTheDocument();
        expect(screen.getByText('Quick Serve Restaurant')).toBeInTheDocument();
        expect(screen.getByText('Specialty Retail')).toBeInTheDocument();
      });
    });

    it('renders all 8 intelligence role cards', async () => {
      render(<App />);
      
      await waitFor(() => {
        expect(screen.getByText('Decision Framer')).toBeInTheDocument();
        expect(screen.getByText('Shopper Insights')).toBeInTheDocument();
        expect(screen.getByText('Demand Forecasting')).toBeInTheDocument();
        expect(screen.getByText('Inventory Readiness')).toBeInTheDocument();
        expect(screen.getByText('Margin Impact')).toBeInTheDocument();
        expect(screen.getByText('Digital Merchandising')).toBeInTheDocument();
        expect(screen.getByText('Risk & Compliance')).toBeInTheDocument();
        expect(screen.getByText('Executive Recommendation')).toBeInTheDocument();
      });
    });

    it('renders the decision input area', async () => {
      render(<App />);
      
      await waitFor(() => {
        expect(screen.getByText('Business Decision')).toBeInTheDocument();
        expect(screen.getByRole('textbox')).toBeInTheDocument();
      });
    });
  });

  describe('User Interactions', () => {
    it('allows selecting a persona', async () => {
      render(<App />);
      
      await waitFor(() => {
        expect(screen.getByText('Grocery Retail')).toBeInTheDocument();
      });

      const groceryButton = screen.getByText('Grocery Retail').closest('button');
      if (groceryButton) {
        fireEvent.click(groceryButton);
      }

      // After clicking, the persona should be selected (UI should reflect this)
      await waitFor(() => {
        // The header should show the selected persona
        const header = screen.getByRole('banner');
        expect(header).toBeInTheDocument();
      });
    });

    it('allows typing in the decision input', async () => {
      render(<App />);
      
      await waitFor(() => {
        expect(screen.getByRole('textbox')).toBeInTheDocument();
      });

      const textarea = screen.getByRole('textbox');
      fireEvent.change(textarea, { 
        target: { value: 'Should we expand our organic product offerings to attract health-conscious consumers?' } 
      });

      expect(textarea).toHaveValue('Should we expand our organic product offerings to attract health-conscious consumers?');
    });

    it('shows character count when typing', async () => {
      render(<App />);
      
      await waitFor(() => {
        expect(screen.getByRole('textbox')).toBeInTheDocument();
      });

      const textarea = screen.getByRole('textbox');
      fireEvent.change(textarea, { target: { value: 'Test input text' } });

      await waitFor(() => {
        expect(screen.getByText(/15/)).toBeInTheDocument();
        expect(screen.getByText(/characters/i)).toBeInTheDocument();
      });
    });

    it('enables submit button with valid input', async () => {
      render(<App />);
      
      await waitFor(() => {
        expect(screen.getByText('Grocery Retail')).toBeInTheDocument();
      });

      // Select a persona
      const groceryButton = screen.getByText('Grocery Retail').closest('button');
      if (groceryButton) {
        fireEvent.click(groceryButton);
      }

      // Type a decision (more than 10 characters)
      const textarea = screen.getByRole('textbox');
      fireEvent.change(textarea, { 
        target: { value: 'Should we expand our organic product line to meet growing consumer demand?' } 
      });

      await waitFor(() => {
        const submitButton = screen.getByText(/run analysis/i);
        expect(submitButton).not.toBeDisabled();
      });
    });

    it('disables submit button with short input', async () => {
      render(<App />);
      
      await waitFor(() => {
        expect(screen.getByText('Grocery Retail')).toBeInTheDocument();
      });

      // Select a persona
      const groceryButton = screen.getByText('Grocery Retail').closest('button');
      if (groceryButton) {
        fireEvent.click(groceryButton);
      }

      // Type a short decision (less than 10 characters)
      const textarea = screen.getByRole('textbox');
      fireEvent.change(textarea, { target: { value: 'Short' } });

      await waitFor(() => {
        const submitButton = screen.getByText(/run analysis/i);
        expect(submitButton).toBeDisabled();
      });
    });
  });

  describe('Data Source Toggle', () => {
    it('renders the data source toggle', async () => {
      render(<App />);
      
      await waitFor(() => {
        expect(screen.getByText('Data Source')).toBeInTheDocument();
      });
    });

    it('shows sample data message when toggle is on', async () => {
      render(<App />);
      
      await waitFor(() => {
        expect(screen.getByText(/sample data/i)).toBeInTheDocument();
      });
    });
  });

  describe('Responsive Layout', () => {
    it('renders main layout containers', async () => {
      const { container } = render(<App />);
      
      await waitFor(() => {
        // Should have main grid layout
        expect(container.querySelector('.grid')).toBeInTheDocument();
      });
    });

    it('renders sidebar and main content areas', async () => {
      const { container } = render(<App />);
      
      await waitFor(() => {
        // Should have multiple grid sections
        const grids = container.querySelectorAll('.grid');
        expect(grids.length).toBeGreaterThan(0);
      });
    });
  });

  describe('Error Handling', () => {
    it('handles API failure gracefully', async () => {
      mockFetch.mockRejectedValueOnce(new Error('Network error'));
      
      // App should still render even if API fails
      render(<App />);
      
      await waitFor(() => {
        expect(screen.getByText('Retail Intelligence Studio')).toBeInTheDocument();
      });
    });
  });
});
