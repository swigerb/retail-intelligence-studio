using RetailIntelligenceStudio.Core.Models;

namespace RetailIntelligenceStudio.Core.Abstractions;

/// <summary>
/// Abstraction for storing and retrieving decision evaluation state.
/// Implementations can use in-memory, Redis, Cosmos DB, etc.
/// </summary>
public interface IStateStore
{
    /// <summary>
    /// Saves a decision result to the store.
    /// </summary>
    Task SaveDecisionAsync(DecisionResult decision, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a decision result by ID.
    /// </summary>
    Task<DecisionResult?> GetDecisionAsync(string decisionId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing decision result.
    /// </summary>
    Task UpdateDecisionAsync(DecisionResult decision, CancellationToken cancellationToken = default);

    /// <summary>
    /// Lists recent decisions with pagination.
    /// </summary>
    Task<IReadOnlyList<DecisionResult>> ListDecisionsAsync(int skip = 0, int take = 20, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a decision by ID.
    /// </summary>
    Task DeleteDecisionAsync(string decisionId, CancellationToken cancellationToken = default);
}
