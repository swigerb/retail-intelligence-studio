using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using System.Threading.Channels;
using RetailIntelligenceStudio.Core.Abstractions;
using RetailIntelligenceStudio.Core.Models;

namespace RetailIntelligenceStudio.Core.Stores;

/// <summary>
/// In-memory implementation of IDecisionStore for local development and demos.
/// Supports real-time event streaming via Channels with proper multi-subscriber support.
/// </summary>
public sealed class InMemoryDecisionStore : IDecisionStore
{
    private readonly ConcurrentDictionary<string, List<DecisionEvent>> _events = new();
    private readonly ConcurrentDictionary<string, List<Channel<DecisionEvent>>> _subscribers = new();
    private readonly ConcurrentDictionary<string, bool> _completed = new();
    private readonly ConcurrentDictionary<string, string?> _errors = new();
    
    // Lock objects to ensure atomic operations between append and stream
    private readonly ConcurrentDictionary<string, object> _locks = new();

    public Task AppendEventAsync(DecisionEvent decisionEvent, CancellationToken cancellationToken = default)
    {
        var lockObj = _locks.GetOrAdd(decisionEvent.DecisionId, _ => new object());
        var events = _events.GetOrAdd(decisionEvent.DecisionId, _ => []);
        var subscribers = _subscribers.GetOrAdd(decisionEvent.DecisionId, _ => []);

        // Atomic: add to list AND broadcast to ALL subscribers under same lock
        lock (lockObj)
        {
            events.Add(decisionEvent);
            
            // Broadcast to all active subscribers
            foreach (var channel in subscribers)
            {
                channel.Writer.TryWrite(decisionEvent);
            }
        }

        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<DecisionEvent>> GetEventsAsync(string decisionId, CancellationToken cancellationToken = default)
    {
        if (_events.TryGetValue(decisionId, out var events))
        {
            var lockObj = _locks.GetOrAdd(decisionId, _ => new object());
            lock (lockObj)
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
        var lockObj = _locks.GetOrAdd(decisionId, _ => new object());
        var subscribers = _subscribers.GetOrAdd(decisionId, _ => []);
        var events = _events.GetOrAdd(decisionId, _ => []);
        
        // Create a dedicated channel for THIS subscriber
        var subscriberChannel = Channel.CreateUnbounded<DecisionEvent>();
        
        // Track sequence numbers we've already yielded to avoid duplicates
        var yieldedSequences = new HashSet<int>();
        
        // Take snapshot AND register subscriber UNDER LOCK atomically
        // This ensures no events are lost between snapshot and subscription
        List<DecisionEvent> snapshot;
        bool alreadyComplete;
        lock (lockObj)
        {
            snapshot = events.ToList();
            alreadyComplete = _completed.TryGetValue(decisionId, out var c) && c;
            
            // Only subscribe if not already complete
            if (!alreadyComplete)
            {
                subscribers.Add(subscriberChannel);
            }
        }

        try
        {
            // Yield all existing events from snapshot
            foreach (var evt in snapshot)
            {
                if (yieldedSequences.Add(evt.SequenceNumber))
                {
                    yield return evt;
                }
            }

            // If already complete when we started, we have everything
            if (alreadyComplete)
            {
                yield break;
            }

            // Wait for new events from our dedicated subscriber channel
            await foreach (var evt in subscriberChannel.Reader.ReadAllAsync(cancellationToken))
            {
                // Only yield events we haven't already yielded from the snapshot
                if (yieldedSequences.Add(evt.SequenceNumber))
                {
                    yield return evt;
                }
            }
        }
        finally
        {
            // Always unsubscribe when done (client disconnect, cancellation, or completion)
            lock (lockObj)
            {
                subscribers.Remove(subscriberChannel);
            }
        }
    }

    public Task CompleteAsync(string decisionId, CancellationToken cancellationToken = default)
    {
        var lockObj = _locks.GetOrAdd(decisionId, _ => new object());
        
        lock (lockObj)
        {
            _completed[decisionId] = true;

            // Complete ALL subscriber channels
            if (_subscribers.TryGetValue(decisionId, out var subscribers))
            {
                foreach (var channel in subscribers)
                {
                    channel.Writer.TryComplete();
                }
            }
        }

        return Task.CompletedTask;
    }

    public Task FailAsync(string decisionId, string errorMessage, CancellationToken cancellationToken = default)
    {
        var lockObj = _locks.GetOrAdd(decisionId, _ => new object());
        
        lock (lockObj)
        {
            _completed[decisionId] = true;
            _errors[decisionId] = errorMessage;

            // Complete ALL subscriber channels with error
            if (_subscribers.TryGetValue(decisionId, out var subscribers))
            {
                var exception = new InvalidOperationException(errorMessage);
                foreach (var channel in subscribers)
                {
                    channel.Writer.TryComplete(exception);
                }
            }
        }

        return Task.CompletedTask;
    }

    public Task<bool> IsCompleteAsync(string decisionId, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_completed.TryGetValue(decisionId, out var isComplete) && isComplete);
    }
}
