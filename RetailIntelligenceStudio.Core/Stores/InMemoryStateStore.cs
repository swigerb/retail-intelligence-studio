using System.Collections.Concurrent;
using RetailIntelligenceStudio.Core.Abstractions;
using RetailIntelligenceStudio.Core.Models;

namespace RetailIntelligenceStudio.Core.Stores;

/// <summary>
/// In-memory implementation of IStateStore for local development and demos.
/// </summary>
public sealed class InMemoryStateStore : IStateStore
{
    private readonly ConcurrentDictionary<string, DecisionResult> _decisions = new();

    public Task SaveDecisionAsync(DecisionResult decision, CancellationToken cancellationToken = default)
    {
        _decisions[decision.DecisionId] = decision;
        return Task.CompletedTask;
    }

    public Task<DecisionResult?> GetDecisionAsync(string decisionId, CancellationToken cancellationToken = default)
    {
        _decisions.TryGetValue(decisionId, out var decision);
        return Task.FromResult(decision);
    }

    public Task UpdateDecisionAsync(DecisionResult decision, CancellationToken cancellationToken = default)
    {
        _decisions[decision.DecisionId] = decision;
        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<DecisionResult>> ListDecisionsAsync(int skip = 0, int take = 20, CancellationToken cancellationToken = default)
    {
        var results = _decisions.Values
            .OrderByDescending(d => d.StartedAt)
            .Skip(skip)
            .Take(take)
            .ToList();

        return Task.FromResult<IReadOnlyList<DecisionResult>>(results);
    }

    public Task DeleteDecisionAsync(string decisionId, CancellationToken cancellationToken = default)
    {
        _decisions.TryRemove(decisionId, out _);
        return Task.CompletedTask;
    }
}
