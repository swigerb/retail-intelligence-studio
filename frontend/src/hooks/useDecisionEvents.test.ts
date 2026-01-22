import { describe, it, expect, vi, beforeEach, afterEach } from 'vitest';
import { renderHook, act, waitFor } from '@testing-library/react';
import { useDecisionEvents } from './useDecisionEvents';
import { RetailPersona, AnalysisPhase } from '../types';

// Mock EventSource
class MockEventSource {
  static instances: MockEventSource[] = [];
  url: string;
  onopen: ((event: Event) => void) | null = null;
  onmessage: ((event: MessageEvent) => void) | null = null;
  onerror: ((event: Event) => void) | null = null;
  readyState: number = 0;

  constructor(url: string) {
    this.url = url;
    MockEventSource.instances.push(this);
    // Simulate async connection
    setTimeout(() => {
      this.readyState = 1;
      if (this.onopen) {
        this.onopen(new Event('open'));
      }
    }, 0);
  }

  close() {
    this.readyState = 2;
  }

  simulateMessage(data: unknown) {
    if (this.onmessage) {
      this.onmessage(new MessageEvent('message', {
        data: JSON.stringify(data)
      }));
    }
  }

  simulateError() {
    if (this.onerror) {
      this.onerror(new Event('error'));
    }
  }
}

// Store original EventSource
const OriginalEventSource = global.EventSource;

describe('useDecisionEvents', () => {
  beforeEach(() => {
    // Replace global EventSource with mock
    (global as unknown as { EventSource: typeof MockEventSource }).EventSource = MockEventSource;
    MockEventSource.instances = [];
  });

  afterEach(() => {
    // Restore original EventSource
    (global as unknown as { EventSource: typeof EventSource }).EventSource = OriginalEventSource;
    MockEventSource.instances = [];
    vi.clearAllMocks();
  });

  it('initializes with empty state when no decisionId', () => {
    const { result } = renderHook(() => useDecisionEvents(null));

    expect(result.current.events).toEqual([]);
    expect(result.current.isConnected).toBe(false);
    expect(result.current.isComplete).toBe(false);
    expect(result.current.error).toBe(null);
  });

  it('provides connect and disconnect functions', () => {
    const { result } = renderHook(() => useDecisionEvents('test-id'));

    expect(typeof result.current.connect).toBe('function');
    expect(typeof result.current.disconnect).toBe('function');
  });

  it('connects to SSE endpoint when connect is called', async () => {
    const { result } = renderHook(() => useDecisionEvents('test-123'));

    act(() => {
      result.current.connect();
    });

    // May create multiple instances due to React's strict mode or hook behavior
    expect(MockEventSource.instances.length).toBeGreaterThanOrEqual(1);
    expect(MockEventSource.instances[MockEventSource.instances.length - 1].url).toBe('/api/decisions/test-123/events');
  });

  it('updates isConnected when connection opens', async () => {
    const { result } = renderHook(() => useDecisionEvents('test-123'));

    act(() => {
      result.current.connect();
    });

    await waitFor(() => {
      expect(result.current.isConnected).toBe(true);
    });
  });

  it('adds events when receiving messages', async () => {
    const { result } = renderHook(() => useDecisionEvents('test-123'));

    act(() => {
      result.current.connect();
    });

    await waitFor(() => {
      expect(result.current.isConnected).toBe(true);
    });

    const testEvent = {
      decisionId: 'test-123',
      persona: RetailPersona.Grocery,
      roleName: 'decision_framer',
      phase: AnalysisPhase.Analyzing,
      message: 'Analyzing decision...',
      timestamp: new Date().toISOString(),
      sequenceNumber: 1
    };

    act(() => {
      MockEventSource.instances[0].simulateMessage(testEvent);
    });

    expect(result.current.events.length).toBe(1);
    expect(result.current.events[0].message).toBe('Analyzing decision...');
  });

  it('disconnects and closes EventSource', async () => {
    const { result } = renderHook(() => useDecisionEvents('test-123'));

    act(() => {
      result.current.connect();
    });

    await waitFor(() => {
      expect(result.current.isConnected).toBe(true);
    });

    act(() => {
      result.current.disconnect();
    });

    expect(result.current.isConnected).toBe(false);
    // The last instance should be closed
    const lastInstance = MockEventSource.instances[MockEventSource.instances.length - 1];
    expect(lastInstance.readyState).toBe(2);
  });

  it('sets error state on connection error', async () => {
    const { result } = renderHook(() => useDecisionEvents('test-123'));

    act(() => {
      result.current.connect();
    });

    await waitFor(() => {
      expect(result.current.isConnected).toBe(true);
    });

    act(() => {
      MockEventSource.instances[0].simulateError();
    });

    expect(result.current.error).toBe('Connection lost. Retrying...');
  });

  it('handles completion signal', async () => {
    const { result } = renderHook(() => useDecisionEvents('test-123'));

    act(() => {
      result.current.connect();
    });

    await waitFor(() => {
      expect(result.current.isConnected).toBe(true);
    });

    act(() => {
      MockEventSource.instances[0].simulateMessage({ type: 'complete' });
    });

    expect(result.current.isComplete).toBe(true);
  });
});
