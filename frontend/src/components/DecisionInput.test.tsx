import { describe, it, expect, vi } from 'vitest';
import { render, screen, fireEvent } from '@testing-library/react';
import { DecisionInput } from './DecisionInput';
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
  }
];

const defaultProps = {
  decisionText: '',
  onDecisionTextChange: vi.fn(),
  useSampleData: true,
  onUseSampleDataChange: vi.fn(),
  selectedPersona: RetailPersona.Grocery,
  personas: mockPersonas,
  onSubmit: vi.fn(),
  onLoadSample: vi.fn(),
  isSubmitting: false,
  isAnalyzing: false
};

describe('DecisionInput', () => {
  it('renders textarea for decision input', () => {
    render(<DecisionInput {...defaultProps} />);
    
    const textarea = screen.getByRole('textbox');
    expect(textarea).toBeInTheDocument();
  });

  it('renders submit button', () => {
    render(<DecisionInput {...defaultProps} />);
    
    const submitButton = screen.getByText(/run analysis/i);
    expect(submitButton).toBeInTheDocument();
  });

  it('allows typing in the textarea', () => {
    const onChange = vi.fn();
    render(<DecisionInput {...defaultProps} onDecisionTextChange={onChange} />);
    
    const textarea = screen.getByRole('textbox');
    fireEvent.change(textarea, { target: { value: 'New decision text' } });
    
    expect(onChange).toHaveBeenCalledWith('New decision text');
  });

  it('calls onSubmit when button is clicked with valid input', () => {
    const onSubmit = vi.fn();
    render(
      <DecisionInput 
        {...defaultProps} 
        decisionText="Should we expand our product line to include organic options?"
        onSubmit={onSubmit} 
      />
    );
    
    const submitButton = screen.getByText(/run analysis/i);
    fireEvent.click(submitButton);
    
    expect(onSubmit).toHaveBeenCalled();
  });

  it('disables submit when processing', () => {
    render(
      <DecisionInput 
        {...defaultProps} 
        decisionText="A valid decision that is long enough to submit"
        isSubmitting={true} 
      />
    );
    
    const submitButton = screen.getByText(/submitting/i);
    expect(submitButton).toBeDisabled();
  });

  it('shows analyzing state when isAnalyzing is true', () => {
    render(
      <DecisionInput 
        {...defaultProps} 
        decisionText="A valid decision that is long enough to submit"
        isAnalyzing={true} 
      />
    );
    
    expect(screen.getByText(/analyzing/i)).toBeInTheDocument();
  });

  it('disables submit when textarea is empty', () => {
    render(<DecisionInput {...defaultProps} decisionText="" />);
    
    const submitButton = screen.getByText(/run analysis/i);
    expect(submitButton).toBeDisabled();
  });

  it('renders data source toggle', () => {
    render(<DecisionInput {...defaultProps} />);
    
    expect(screen.getByText('Data Source')).toBeInTheDocument();
  });

  it('renders load sample button', () => {
    render(<DecisionInput {...defaultProps} />);
    
    // The component has "Use example" button
    expect(screen.getByText(/use example/i)).toBeInTheDocument();
  });
});
