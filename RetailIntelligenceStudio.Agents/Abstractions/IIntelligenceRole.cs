using RetailIntelligenceStudio.Core.Models;

namespace RetailIntelligenceStudio.Agents.Abstractions;

/// <summary>
/// Defines an intelligence role that analyzes retail decisions.
/// Each role has a specific responsibility and produces streaming insights.
/// </summary>
public interface IIntelligenceRole
{
    /// <summary>
    /// Unique name of the intelligence role.
    /// </summary>
    string RoleName { get; }

    /// <summary>
    /// Human-friendly display name for the UI.
    /// </summary>
    string DisplayName { get; }

    /// <summary>
    /// Brief description of the role's responsibility.
    /// </summary>
    string Description { get; }

    /// <summary>
    /// Key areas this role focuses on during analysis.
    /// </summary>
    IReadOnlyList<string> FocusAreas { get; }

    /// <summary>
    /// The type of output this role produces.
    /// </summary>
    string OutputType { get; }

    /// <summary>
    /// Order in the workflow (1-8, where 1 is first).
    /// </summary>
    int WorkflowOrder { get; }

    /// <summary>
    /// Analyzes the decision and streams events as insights are generated.
    /// </summary>
    /// <param name="decisionId">Unique decision identifier.</param>
    /// <param name="request">The decision request.</param>
    /// <param name="personaContext">Persona-specific context.</param>
    /// <param name="priorInsights">Insights from roles that ran before this one.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Async enumerable of decision events.</returns>
    IAsyncEnumerable<DecisionEvent> AnalyzeAsync(
        string decisionId,
        DecisionRequest request,
        PersonaContext personaContext,
        IReadOnlyDictionary<string, RoleInsight> priorInsights,
        CancellationToken cancellationToken = default);
}
