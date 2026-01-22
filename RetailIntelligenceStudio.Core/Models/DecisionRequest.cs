using System.Text.Json.Serialization;

namespace RetailIntelligenceStudio.Core.Models;

/// <summary>
/// Represents a request to evaluate a retail business decision.
/// </summary>
public sealed class DecisionRequest
{
    /// <summary>
    /// The business decision to evaluate (e.g., "Should we run a 20% off promotion on 12-pack sparkling water?").
    /// </summary>
    public required string DecisionText { get; init; }

    /// <summary>
    /// The retail persona context to use for evaluation.
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public required RetailPersona Persona { get; init; }

    /// <summary>
    /// When true, agents use persona-specific sample data and assumptions.
    /// When false, agents reason from user input and general industry patterns.
    /// </summary>
    public bool UseSampleData { get; init; } = true;

    /// <summary>
    /// Optional geographic region for the decision (e.g., "Southeast", "West Coast").
    /// </summary>
    public string? Region { get; init; }

    /// <summary>
    /// Optional product category (e.g., "beverages", "frozen", "combos").
    /// </summary>
    public string? Category { get; init; }

    /// <summary>
    /// Optional timeframe for the decision (e.g., "4 weeks", "Q2 2026").
    /// </summary>
    public string? Timeframe { get; init; }
}
