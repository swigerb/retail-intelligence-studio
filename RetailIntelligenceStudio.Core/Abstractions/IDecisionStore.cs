using RetailIntelligenceStudio.Core.Models;

namespace RetailIntelligenceStudio.Core.Abstractions;

/// <summary>
/// Abstraction for storing and streaming decision events.
/// Supports real-time event broadcasting to multiple clients.
/// </summary>
public interface IDecisionStore
{
    /// <summary>
    /// Appends an event to a decision's event stream.
    /// </summary>
    Task AppendEventAsync(DecisionEvent decisionEvent, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all events for a decision.
    /// </summary>
    Task<IReadOnlyList<DecisionEvent>> GetEventsAsync(string decisionId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Streams events for a decision as they arrive.
    /// </summary>
    IAsyncEnumerable<DecisionEvent> StreamEventsAsync(string decisionId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Marks a decision's event stream as complete.
    /// </summary>
    Task CompleteAsync(string decisionId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Marks a decision's event stream as failed with an error.
    /// </summary>
    Task FailAsync(string decisionId, string errorMessage, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a decision's event stream is complete.
    /// </summary>
    Task<bool> IsCompleteAsync(string decisionId, CancellationToken cancellationToken = default);
}
