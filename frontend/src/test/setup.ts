import '@testing-library/jest-dom';

// Mock EventSource for SSE testing
class MockEventSource {
  static instances: MockEventSource[] = [];
  
  url: string;
  onmessage: ((event: MessageEvent) => void) | null = null;
  onerror: ((event: Event) => void) | null = null;
  onopen: ((event: Event) => void) | null = null;
  readyState: number = 0;

  static readonly CONNECTING = 0;
  static readonly OPEN = 1;
  static readonly CLOSED = 2;

  constructor(url: string) {
    this.url = url;
    this.readyState = MockEventSource.CONNECTING;
    MockEventSource.instances.push(this);
    
    // Simulate connection opening
    setTimeout(() => {
      this.readyState = MockEventSource.OPEN;
      this.onopen?.(new Event('open'));
    }, 0);
  }

  close() {
    this.readyState = MockEventSource.CLOSED;
    const index = MockEventSource.instances.indexOf(this);
    if (index > -1) {
      MockEventSource.instances.splice(index, 1);
    }
  }

  // Helper to simulate receiving a message
  simulateMessage(data: unknown) {
    if (this.onmessage) {
      const event = new MessageEvent('message', {
        data: typeof data === 'string' ? data : JSON.stringify(data),
      });
      this.onmessage(event);
    }
  }

  // Helper to simulate an error
  simulateError() {
    this.onerror?.(new Event('error'));
  }

  static clearInstances() {
    MockEventSource.instances.forEach(instance => instance.close());
    MockEventSource.instances = [];
  }
}

// Make it available globally
(globalThis as unknown as { EventSource: typeof MockEventSource }).EventSource = MockEventSource;

// Export for use in tests
export { MockEventSource };

// Clean up after each test
afterEach(() => {
  MockEventSource.clearInstances();
});
