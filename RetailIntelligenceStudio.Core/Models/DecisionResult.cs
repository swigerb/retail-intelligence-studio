namespace RetailIntelligenceStudio.Core.Models;

/// <summary>
/// Represents the complete result of a decision evaluation.
/// </summary>
public sealed class DecisionResult
{
    /// <summary>
    /// Unique identifier for the decision.
    /// </summary>
    public required string DecisionId { get; init; }

    /// <summary>
    /// The original decision request.
    /// </summary>
    public required DecisionRequest Request { get; init; }

    /// <summary>
    /// The persona context used for evaluation.
    /// </summary>
    public required PersonaContext PersonaContext { get; init; }

    /// <summary>
    /// Structured decision brief from the Decision Framer.
    /// </summary>
    public DecisionBrief? DecisionBrief { get; set; }

    /// <summary>
    /// All events generated during evaluation.
    /// </summary>
    public List<DecisionEvent> Events { get; init; } = [];

    /// <summary>
    /// Insights organized by intelligence role.
    /// </summary>
    public Dictionary<string, RoleInsight> RoleInsights { get; init; } = [];

    /// <summary>
    /// Final executive recommendation.
    /// </summary>
    public ExecutiveRecommendation? Recommendation { get; set; }

    /// <summary>
    /// When the evaluation started.
    /// </summary>
    public DateTimeOffset StartedAt { get; init; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// When the evaluation completed (null if still running).
    /// </summary>
    public DateTimeOffset? CompletedAt { get; set; }

    /// <summary>
    /// Current status of the evaluation.
    /// </summary>
    public DecisionStatus Status { get; set; } = DecisionStatus.Pending;
}

/// <summary>
/// Structured decision brief produced by the Decision Framer.
/// </summary>
public sealed class DecisionBrief
{
    /// <summary>
    /// The core business question being evaluated.
    /// </summary>
    public required string CoreQuestion { get; init; }

    /// <summary>
    /// The proposed action or change.
    /// </summary>
    public required string ProposedAction { get; init; }

    /// <summary>
    /// Target scope (products, stores, regions).
    /// </summary>
    public required string[] Scope { get; init; }

    /// <summary>
    /// Timeline for the decision.
    /// </summary>
    public required string Timeline { get; init; }

    /// <summary>
    /// Success criteria and metrics.
    /// </summary>
    public required string[] SuccessCriteria { get; init; }

    /// <summary>
    /// Key assumptions made.
    /// </summary>
    public required string[] Assumptions { get; init; }
}

/// <summary>
/// Insight from a specific intelligence role.
/// </summary>
public sealed class RoleInsight
{
    /// <summary>
    /// Name of the intelligence role.
    /// </summary>
    public required string RoleName { get; init; }

    /// <summary>
    /// Summary of the role's analysis.
    /// </summary>
    public required string Summary { get; init; }

    /// <summary>
    /// Key findings from the analysis.
    /// </summary>
    public required string[] KeyFindings { get; init; }

    /// <summary>
    /// Confidence level (0.0 to 1.0).
    /// </summary>
    public required double Confidence { get; init; }

    /// <summary>
    /// Additional structured data from the analysis.
    /// </summary>
    public Dictionary<string, object>? Data { get; init; }
}

/// <summary>
/// Status of a decision evaluation.
/// </summary>
public enum DecisionStatus
{
    /// <summary>Decision is queued but not started.</summary>
    Pending,
    
    /// <summary>Decision is being evaluated.</summary>
    Running,
    
    /// <summary>Decision evaluation completed successfully.</summary>
    Completed,
    
    /// <summary>Decision evaluation failed.</summary>
    Failed,
    
    /// <summary>Decision evaluation was cancelled.</summary>
    Cancelled
}
