import { useState, useEffect, useCallback, useRef } from 'react';
import { DecisionEvent, AnalysisPhase } from '../types';

export interface UseDecisionEventsResult {
  events: DecisionEvent[];
  isConnected: boolean;
  isComplete: boolean;
  error: string | null;
  connect: () => void;
  disconnect: () => void;
}

/**
 * Custom hook for streaming decision events via Server-Sent Events (SSE).
 */
export function useDecisionEvents(decisionId: string | null): UseDecisionEventsResult {
  const [events, setEvents] = useState<DecisionEvent[]>([]);
  const [isConnected, setIsConnected] = useState(false);
  const [isComplete, setIsComplete] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const eventSourceRef = useRef<EventSource | null>(null);

  const disconnect = useCallback(() => {
    if (eventSourceRef.current) {
      eventSourceRef.current.close();
      eventSourceRef.current = null;
      setIsConnected(false);
    }
  }, []);

  const connect = useCallback(() => {
    if (!decisionId) return;

    // Close any existing connection
    disconnect();

    // Reset state
    setEvents([]);
    setError(null);
    setIsComplete(false);

    const eventSource = new EventSource(`/api/decisions/${decisionId}/events`);
    eventSourceRef.current = eventSource;

    eventSource.onopen = () => {
      setIsConnected(true);
      setError(null);
      console.log('[useDecisionEvents] ✅ SSE connection opened');
    };

    eventSource.onmessage = (event) => {
      try {
        const data = JSON.parse(event.data);
        
        // Check for completion signal
        if (data.type === 'complete') {
          console.log('[useDecisionEvents] 🏁 Received completion signal');
          setIsComplete(true);
          disconnect();
          return;
        }

        const decisionEvent = data as DecisionEvent;
        
        // Log completed events prominently
        if (decisionEvent.phase === 'Completed' || decisionEvent.phase === AnalysisPhase.Completed) {
          console.log(`[useDecisionEvents] ✅ ${decisionEvent.roleName} COMPLETED`, {
            phase: decisionEvent.phase,
            confidence: decisionEvent.confidence,
            seq: decisionEvent.sequenceNumber
          });
        }
        
        setEvents(prev => {
          const newEvents = [...prev, decisionEvent];
          // Log role summary periodically
          if (newEvents.length % 50 === 0) {
            const roles = [...new Set(newEvents.map(e => e.roleName))];
            console.log(`[useDecisionEvents] 📊 ${newEvents.length} events, roles: ${roles.join(', ')}`);
          }
          return newEvents;
        });

        // Check if the workflow is complete
        if (decisionEvent.roleName === 'workflow' && 
            decisionEvent.phase === AnalysisPhase.Completed) {
          setIsComplete(true);
          disconnect();
        }
      } catch (e) {
        console.error('[useDecisionEvents] ❌ Failed to parse event:', e, event.data);
      }
    };

    eventSource.onerror = () => {
      setError('Connection lost. Retrying...');
      setIsConnected(false);
      
      // EventSource will automatically retry
      // After max retries, close the connection
      setTimeout(() => {
        if (eventSourceRef.current?.readyState === EventSource.CLOSED) {
          setError('Connection failed. Please try again.');
        }
      }, 10000);
    };

  }, [decisionId, disconnect]);

  // Auto-connect when decisionId changes
  useEffect(() => {
    if (decisionId) {
      connect();
    }
    
    return () => disconnect();
  }, [decisionId, connect, disconnect]);

  return {
    events,
    isConnected,
    isComplete,
    error,
    connect,
    disconnect
  };
}
