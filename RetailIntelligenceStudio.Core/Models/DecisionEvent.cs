using System.Text.Json.Serialization;

namespace RetailIntelligenceStudio.Core.Models;

/// <summary>
/// Represents a streaming event emitted during decision evaluation.
/// These events are sent to clients via Server-Sent Events (SSE).
/// </summary>
public sealed record DecisionEvent
{
    /// <summary>
    /// Unique identifier for the decision being evaluated.
    /// </summary>
    public required string DecisionId { get; init; }

    /// <summary>
    /// The retail persona context for this decision.
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public required RetailPersona Persona { get; init; }

    /// <summary>
    /// The intelligence role that generated this event.
    /// </summary>
    public required string RoleName { get; init; }

    /// <summary>
    /// The current phase of the role's analysis.
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public required AnalysisPhase Phase { get; init; }

    /// <summary>
    /// The insight or status message from the intelligence role.
    /// </summary>
    public required string Message { get; init; }

    /// <summary>
    /// Optional confidence score (0.0 to 1.0) for quantitative insights.
    /// </summary>
    public double? Confidence { get; init; }

    /// <summary>
    /// Optional structured data payload for rich insights.
    /// </summary>
    public Dictionary<string, object>? Data { get; init; }

    /// <summary>
    /// UTC timestamp when this event was generated.
    /// </summary>
    public DateTimeOffset Timestamp { get; init; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// Sequence number for ordering events within a decision.
    /// </summary>
    public int SequenceNumber { get; init; }
}

/// <summary>
/// Phases of analysis for an intelligence role.
/// </summary>
public enum AnalysisPhase
{
    /// <summary>Role is initializing.</summary>
    Starting,
    
    /// <summary>Role is actively analyzing.</summary>
    Analyzing,
    
    /// <summary>Role is generating insights.</summary>
    Reporting,
    
    /// <summary>Role has completed analysis.</summary>
    Completed,
    
    /// <summary>Role encountered an error.</summary>
    Error
}
