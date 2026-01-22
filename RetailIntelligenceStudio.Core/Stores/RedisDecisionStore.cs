using RetailIntelligenceStudio.Core.Abstractions;
using RetailIntelligenceStudio.Core.Models;

namespace RetailIntelligenceStudio.Core.Stores;

/// <summary>
/// Stub implementation for Redis-based decision event streaming.
/// To be implemented for enterprise deployments with distributed streaming.
/// </summary>
public sealed class RedisDecisionStore : IDecisionStore
{
    private const string NotImplementedMessage = 
        "RedisDecisionStore is a stub for enterprise deployments. " +
        "Configure Redis connection and implement this class for distributed streaming.";

    public Task AppendEventAsync(DecisionEvent decisionEvent, CancellationToken cancellationToken = default)
        => throw new NotImplementedException(NotImplementedMessage);

    public Task<IReadOnlyList<DecisionEvent>> GetEventsAsync(string decisionId, CancellationToken cancellationToken = default)
        => throw new NotImplementedException(NotImplementedMessage);

    public IAsyncEnumerable<DecisionEvent> StreamEventsAsync(string decisionId, CancellationToken cancellationToken = default)
        => throw new NotImplementedException(NotImplementedMessage);

    public Task CompleteAsync(string decisionId, CancellationToken cancellationToken = default)
        => throw new NotImplementedException(NotImplementedMessage);

    public Task FailAsync(string decisionId, string errorMessage, CancellationToken cancellationToken = default)
        => throw new NotImplementedException(NotImplementedMessage);

    public Task<bool> IsCompleteAsync(string decisionId, CancellationToken cancellationToken = default)
        => throw new NotImplementedException(NotImplementedMessage);
}
