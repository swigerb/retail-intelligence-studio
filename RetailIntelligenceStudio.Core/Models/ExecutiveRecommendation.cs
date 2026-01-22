using System.Text.Json.Serialization;

namespace RetailIntelligenceStudio.Core.Models;

/// <summary>
/// Final recommendation from the Executive Recommendation intelligence role.
/// </summary>
public sealed class ExecutiveRecommendation
{
    /// <summary>
    /// The final verdict on the decision.
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public required RecommendationVerdict Verdict { get; init; }

    /// <summary>
    /// Executive summary explaining the recommendation.
    /// </summary>
    public required string Summary { get; init; }

    /// <summary>
    /// Key rationale points supporting the recommendation.
    /// </summary>
    public required string[] Rationale { get; init; }

    /// <summary>
    /// Recommended actions if the decision is approved.
    /// </summary>
    public required string[] RecommendedActions { get; init; }

    /// <summary>
    /// Suggested modifications if verdict is ApproveWithModifications.
    /// </summary>
    public string[]? SuggestedModifications { get; init; }

    /// <summary>
    /// Key risks to monitor.
    /// </summary>
    public required string[] RisksToMonitor { get; init; }

    /// <summary>
    /// Projected KPIs based on the analysis.
    /// </summary>
    public required Dictionary<string, KpiProjection> ProjectedKpis { get; init; }

    /// <summary>
    /// Overall confidence in the recommendation (0.0 to 1.0).
    /// </summary>
    public required double OverallConfidence { get; init; }
}

/// <summary>
/// Possible verdicts for an executive recommendation.
/// </summary>
public enum RecommendationVerdict
{
    /// <summary>Recommend proceeding with the decision as proposed.</summary>
    Approve,
    
    /// <summary>Recommend proceeding with suggested modifications.</summary>
    ApproveWithModifications,
    
    /// <summary>Recommend not proceeding with the decision.</summary>
    Decline
}

/// <summary>
/// A projected KPI with value range and confidence.
/// </summary>
public sealed class KpiProjection
{
    /// <summary>
    /// Name of the KPI.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Current/baseline value.
    /// </summary>
    public required double BaselineValue { get; init; }

    /// <summary>
    /// Projected low estimate.
    /// </summary>
    public required double ProjectedLow { get; init; }

    /// <summary>
    /// Projected expected value.
    /// </summary>
    public required double ProjectedExpected { get; init; }

    /// <summary>
    /// Projected high estimate.
    /// </summary>
    public required double ProjectedHigh { get; init; }

    /// <summary>
    /// Unit of measurement (e.g., "%", "$", "units").
    /// </summary>
    public required string Unit { get; init; }
}
