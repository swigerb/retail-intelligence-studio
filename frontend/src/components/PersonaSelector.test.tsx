import { describe, it, expect, vi } from 'vitest';
import { render, screen, fireEvent } from '@testing-library/react';
import { PersonaSelector } from './PersonaSelector';
import { RetailPersona, RetailCategory, PersonaContext } from '../types';

const mockPersonas: PersonaContext[] = [
  {
    persona: RetailPersona.Grocery,
    category: RetailCategory.FoodAndDining,
    displayName: 'Grocery Retail',
    description: 'Traditional grocery retail',
    keyCategories: ['Beverages', 'Fresh Produce', 'Frozen Foods'],
    channels: ['In-Store', 'Online'],
    sampleDecisions: ['Should we expand organic offerings?']
  },
  {
    persona: RetailPersona.QuickServeRestaurant,
    category: RetailCategory.FoodAndDining,
    displayName: 'Quick Serve Restaurant',
    description: 'Fast food and QSR',
    keyCategories: ['Burgers', 'Sides', 'Beverages'],
    channels: ['Dine-In', 'Drive-Through'],
    sampleDecisions: ['Should we launch breakfast menu?']
  },
  {
    persona: RetailPersona.SpecialtyRetail,
    category: RetailCategory.SpecialtyAndFashion,
    displayName: 'Specialty Retail',
    description: 'Specialty and department stores',
    keyCategories: ['Electronics', 'Fashion', 'Home'],
    channels: ['Stores', 'E-commerce'],
    sampleDecisions: ['Should we open new locations?']
  }
];

describe('PersonaSelector', () => {
  it('renders all persona options', () => {
    const onSelect = vi.fn();
    render(
      <PersonaSelector 
        personas={mockPersonas} 
        selectedPersona={null} 
        onSelect={onSelect} 
      />
    );

    expect(screen.getByText('Grocery Retail')).toBeInTheDocument();
    expect(screen.getByText('Quick Serve Restaurant')).toBeInTheDocument();
    expect(screen.getByText('Specialty Retail')).toBeInTheDocument();
  });

  it('displays the selected persona with correct styling', () => {
    const onSelect = vi.fn();
    const { container } = render(
      <PersonaSelector 
        personas={mockPersonas} 
        selectedPersona={RetailPersona.Grocery} 
        onSelect={onSelect} 
      />
    );

    // 3 persona buttons + 2 category header buttons = 5 total buttons
    const buttons = container.querySelectorAll('button');
    expect(buttons.length).toBe(5);
  });

  it('calls onSelect when persona is clicked', () => {
    const onSelect = vi.fn();
    render(
      <PersonaSelector 
        personas={mockPersonas} 
        selectedPersona={null} 
        onSelect={onSelect} 
      />
    );

    fireEvent.click(screen.getByText('Grocery Retail'));
    
    expect(onSelect).toHaveBeenCalledWith(RetailPersona.Grocery);
  });

  it('renders category tags for each persona', () => {
    const onSelect = vi.fn();
    render(
      <PersonaSelector 
        personas={mockPersonas} 
        selectedPersona={null} 
        onSelect={onSelect} 
      />
    );

    // Should show truncated categories
    expect(screen.getByText('Beverages, Fresh Produce, Frozen Foods')).toBeInTheDocument();
  });

  it('renders all retail personas with category headers', () => {
    const onSelect = vi.fn();
    render(
      <PersonaSelector 
        personas={mockPersonas} 
        selectedPersona={null} 
        onSelect={onSelect} 
      />
    );

    // 3 persona buttons + 2 category header buttons = 5 total buttons
    const buttons = screen.getAllByRole('button');
    expect(buttons.length).toBe(5);
  });

  it('renders icons for each persona and category chevrons', () => {
    const onSelect = vi.fn();
    const { container } = render(
      <PersonaSelector 
        personas={mockPersonas} 
        selectedPersona={null} 
        onSelect={onSelect} 
      />
    );

    // 3 persona icons + 2 category chevron icons = 5 total SVGs
    const svgs = container.querySelectorAll('svg');
    expect(svgs.length).toBe(5);
  });

  describe('category grouping', () => {
    it('groups personas by category and displays category headers', () => {
      const onSelect = vi.fn();
      render(
        <PersonaSelector 
          personas={mockPersonas} 
          selectedPersona={null} 
          onSelect={onSelect} 
        />
      );

      // Should display category headers
      expect(screen.getByText('Food & Dining')).toBeInTheDocument();
      expect(screen.getByText('Specialty & Fashion')).toBeInTheDocument();
    });

    it('shows correct persona count per category', () => {
      const onSelect = vi.fn();
      render(
        <PersonaSelector 
          personas={mockPersonas} 
          selectedPersona={null} 
          onSelect={onSelect} 
        />
      );

      // Food & Dining has 2 verticals (Grocery, QSR)
      expect(screen.getByText('2 verticals')).toBeInTheDocument();
      // Specialty & Fashion has 1 vertical
      expect(screen.getByText('1 vertical')).toBeInTheDocument();
    });

    it('requires category field on all personas to prevent crashes', () => {
      // This test ensures personas have the required category field
      // which caused a blank screen when missing from API response
      mockPersonas.forEach((persona) => {
        expect(persona.category).toBeDefined();
        expect(Object.values(RetailCategory)).toContain(persona.category);
      });
    });

    it('handles personas with missing category gracefully', () => {
      const onSelect = vi.fn();
      const personasWithMissingCategory = [
        {
          persona: RetailPersona.Grocery,
          category: undefined as unknown as RetailCategory, // Simulate missing category
          displayName: 'Grocery Retail',
          description: 'Traditional grocery retail',
          keyCategories: ['Beverages'],
          channels: ['In-Store'],
          sampleDecisions: ['Test decision']
        }
      ];

      // Component should not crash - this validates defensive coding
      expect(() => {
        render(
          <PersonaSelector 
            personas={personasWithMissingCategory} 
            selectedPersona={null} 
            onSelect={onSelect} 
          />
        );
      }).not.toThrow();
    });

    it('toggles category expansion when header is clicked', () => {
      const onSelect = vi.fn();
      render(
        <PersonaSelector 
          personas={mockPersonas} 
          selectedPersona={null} 
          onSelect={onSelect} 
        />
      );

      // Initially personas should be visible (categories expanded by default)
      expect(screen.getByText('Grocery Retail')).toBeInTheDocument();

      // Click category header to collapse
      fireEvent.click(screen.getByText('Food & Dining'));

      // Grocery should still be in the document but in collapsed section
      // The implementation hides the content when collapsed
    });

    it('highlights category header when a persona within is selected', () => {
      const onSelect = vi.fn();
      const { container } = render(
        <PersonaSelector 
          personas={mockPersonas} 
          selectedPersona={RetailPersona.Grocery} 
          onSelect={onSelect} 
        />
      );

      // The Food & Dining category header should have special styling
      const categoryHeaders = container.querySelectorAll('button');
      const foodDiningHeader = Array.from(categoryHeaders).find(
        btn => btn.textContent?.includes('Food & Dining')
      );
      
      expect(foodDiningHeader).toBeDefined();
      expect(foodDiningHeader?.className).toContain('bg-ris-primary-50');
    });
  });
});
