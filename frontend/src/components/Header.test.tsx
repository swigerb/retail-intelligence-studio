import { describe, it, expect } from 'vitest';
import { render, screen } from '@testing-library/react';
import { Header } from './Header';
import { RetailPersona } from '../types';

describe('Header', () => {
  it('renders the application title', () => {
    render(<Header selectedPersona={null} />);
    
    expect(screen.getByText('Retail Intelligence Studio')).toBeInTheDocument();
  });

  it('renders the subtitle', () => {
    render(<Header selectedPersona={null} />);
    
    expect(screen.getByText('Multi-Agent Decision Intelligence')).toBeInTheDocument();
  });

  it('contains the Activity icon', () => {
    const { container } = render(<Header selectedPersona={null} />);
    
    const svg = container.querySelector('svg');
    expect(svg).toBeInTheDocument();
  });

  it('has proper styling classes', () => {
    const { container } = render(<Header selectedPersona={null} />);
    
    const header = container.querySelector('header');
    expect(header).toHaveClass('bg-white');
    expect(header).toHaveClass('border-b');
  });

  it('displays persona name when selected', () => {
    render(
      <Header 
        selectedPersona={RetailPersona.Grocery} 
        personaDisplayName="Grocery Retail" 
      />
    );
    
    expect(screen.getByText('Grocery Retail')).toBeInTheDocument();
  });

  it('does not display persona badge when no persona selected', () => {
    render(<Header selectedPersona={null} />);
    
    // Should not show a persona badge
    expect(screen.queryByText('Grocery Retail')).not.toBeInTheDocument();
  });
});
