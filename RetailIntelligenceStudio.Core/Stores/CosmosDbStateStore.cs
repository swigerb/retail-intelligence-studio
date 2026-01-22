using RetailIntelligenceStudio.Core.Abstractions;
using RetailIntelligenceStudio.Core.Models;

namespace RetailIntelligenceStudio.Core.Stores;

/// <summary>
/// Stub implementation for Azure Cosmos DB state storage.
/// To be implemented for enterprise deployments.
/// </summary>
public sealed class CosmosDbStateStore : IStateStore
{
    private const string NotImplementedMessage = 
        "CosmosDbStateStore is a stub for enterprise deployments. " +
        "Configure Azure Cosmos DB connection and implement this class for production use.";

    public Task SaveDecisionAsync(DecisionResult decision, CancellationToken cancellationToken = default)
        => throw new NotImplementedException(NotImplementedMessage);

    public Task<DecisionResult?> GetDecisionAsync(string decisionId, CancellationToken cancellationToken = default)
        => throw new NotImplementedException(NotImplementedMessage);

    public Task UpdateDecisionAsync(DecisionResult decision, CancellationToken cancellationToken = default)
        => throw new NotImplementedException(NotImplementedMessage);

    public Task<IReadOnlyList<DecisionResult>> ListDecisionsAsync(int skip = 0, int take = 20, CancellationToken cancellationToken = default)
        => throw new NotImplementedException(NotImplementedMessage);

    public Task DeleteDecisionAsync(string decisionId, CancellationToken cancellationToken = default)
        => throw new NotImplementedException(NotImplementedMessage);
}
