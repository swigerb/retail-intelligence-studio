import { describe, it, expect, vi } from 'vitest';
import { render, screen, fireEvent } from '@testing-library/react';
import { PersonaSelector } from './PersonaSelector';
import { RetailPersona, PersonaContext } from '../types';

const mockPersonas: PersonaContext[] = [
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

    // The selected button should have the primary border class
    const buttons = container.querySelectorAll('button');
    expect(buttons.length).toBe(3);
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

  it('renders all retail personas', () => {
    const onSelect = vi.fn();
    render(
      <PersonaSelector 
        personas={mockPersonas} 
        selectedPersona={null} 
        onSelect={onSelect} 
      />
    );

    // Should have 3 buttons for 3 personas
    const buttons = screen.getAllByRole('button');
    expect(buttons.length).toBe(3);
  });

  it('renders icons for each persona', () => {
    const onSelect = vi.fn();
    const { container } = render(
      <PersonaSelector 
        personas={mockPersonas} 
        selectedPersona={null} 
        onSelect={onSelect} 
      />
    );

    // Each persona should have an icon (SVG)
    const svgs = container.querySelectorAll('svg');
    expect(svgs.length).toBe(3);
  });
});
