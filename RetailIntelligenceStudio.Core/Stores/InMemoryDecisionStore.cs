using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using System.Threading.Channels;
using RetailIntelligenceStudio.Core.Abstractions;
using RetailIntelligenceStudio.Core.Models;

namespace RetailIntelligenceStudio.Core.Stores;

/// <summary>
/// In-memory implementation of IDecisionStore for local development and demos.
/// Supports real-time event streaming via Channels.
/// </summary>
public sealed class InMemoryDecisionStore : IDecisionStore
{
    private readonly ConcurrentDictionary<string, List<DecisionEvent>> _events = new();
    private readonly ConcurrentDictionary<string, Channel<DecisionEvent>> _channels = new();
    private readonly ConcurrentDictionary<string, bool> _completed = new();
    private readonly ConcurrentDictionary<string, string?> _errors = new();

    public Task AppendEventAsync(DecisionEvent decisionEvent, CancellationToken cancellationToken = default)
    {
        var events = _events.GetOrAdd(decisionEvent.DecisionId, _ => []);
        
        lock (events)
        {
            events.Add(decisionEvent);
        }

        // Ensure channel exists and broadcast to any listeners
        var channel = _channels.GetOrAdd(decisionEvent.DecisionId, _ => Channel.CreateUnbounded<DecisionEvent>());
        channel.Writer.TryWrite(decisionEvent);

        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<DecisionEvent>> GetEventsAsync(string decisionId, CancellationToken cancellationToken = default)
    {
        if (_events.TryGetValue(decisionId, out var events))
        {
            lock (events)
            {
                return Task.FromResult<IReadOnlyList<DecisionEvent>>(events.ToList());
            }
        }

        return Task.FromResult<IReadOnlyList<DecisionEvent>>([]);
    }

    public async IAsyncEnumerable<DecisionEvent> StreamEventsAsync(
        string decisionId, 
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        // Get or create the channel first - this ensures we don't miss any events
        // that are written between when we take the snapshot and when we subscribe
        var channel = _channels.GetOrAdd(decisionId, _ => Channel.CreateUnbounded<DecisionEvent>());
        
        // Track sequence numbers we've already yielded to avoid duplicates
        var yieldedSequences = new HashSet<int>();
        
        // First, yield any existing events from the list
        if (_events.TryGetValue(decisionId, out var existingEvents))
        {
            List<DecisionEvent> snapshot;
            lock (existingEvents)
            {
                snapshot = existingEvents.ToList();
            }

            foreach (var evt in snapshot)
            {
                if (yieldedSequences.Add(evt.SequenceNumber))
                {
                    yield return evt;
                }
            }
        }

        // If already complete, get the final list state and yield any missing events
        if (_completed.TryGetValue(decisionId, out var isComplete) && isComplete)
        {
            // Re-read the list to get any events we missed
            if (_events.TryGetValue(decisionId, out existingEvents))
            {
                List<DecisionEvent> finalSnapshot;
                lock (existingEvents)
                {
                    finalSnapshot = existingEvents.ToList();
                }
                
                foreach (var evt in finalSnapshot)
                {
                    if (yieldedSequences.Add(evt.SequenceNumber))
                    {
                        yield return evt;
                    }
                }
            }
            yield break;
        }

        // Subscribe to new events from the channel
        // ReadAllAsync will complete when the channel writer is completed
        await foreach (var evt in channel.Reader.ReadAllAsync(cancellationToken))
        {
            // Only yield events we haven't already yielded from the snapshot
            if (yieldedSequences.Add(evt.SequenceNumber))
            {
                yield return evt;
            }
        }
        
        // After channel completes, do a final check for any events in the list
        // that might have been added but not yet read from the channel
        if (_events.TryGetValue(decisionId, out existingEvents))
        {
            List<DecisionEvent> finalSnapshot;
            lock (existingEvents)
            {
                finalSnapshot = existingEvents.ToList();
            }
            
            foreach (var evt in finalSnapshot)
            {
                if (yieldedSequences.Add(evt.SequenceNumber))
                {
                    yield return evt;
                }
            }
        }
    }

    public Task CompleteAsync(string decisionId, CancellationToken cancellationToken = default)
    {
        _completed[decisionId] = true;

        if (_channels.TryGetValue(decisionId, out var channel))
        {
            channel.Writer.TryComplete();
        }

        return Task.CompletedTask;
    }

    public Task FailAsync(string decisionId, string errorMessage, CancellationToken cancellationToken = default)
    {
        _completed[decisionId] = true;
        _errors[decisionId] = errorMessage;

        if (_channels.TryGetValue(decisionId, out var channel))
        {
            channel.Writer.TryComplete(new InvalidOperationException(errorMessage));
        }

        return Task.CompletedTask;
    }

    public Task<bool> IsCompleteAsync(string decisionId, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_completed.TryGetValue(decisionId, out var isComplete) && isComplete);
    }
}
