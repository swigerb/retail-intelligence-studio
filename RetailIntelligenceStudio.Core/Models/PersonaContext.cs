namespace RetailIntelligenceStudio.Core.Models;

/// <summary>
/// Provides persona-specific context for intelligence role reasoning.
/// Contains baseline assumptions, categories, channels, and KPIs relevant to the selected retail vertical.
/// </summary>
public sealed class PersonaContext
{
    /// <summary>
    /// The retail persona this context represents.
    /// </summary>
    public required RetailPersona Persona { get; init; }

    /// <summary>
    /// The category grouping this persona belongs to.
    /// Used for organizing personas in the UI.
    /// </summary>
    public required RetailCategory Category { get; init; }

    /// <summary>
    /// Human-friendly display name for the persona.
    /// </summary>
    public required string DisplayName { get; init; }

    /// <summary>
    /// Brief description of the retail vertical.
    /// </summary>
    public required string Description { get; init; }

    /// <summary>
    /// Key product categories relevant to this persona.
    /// </summary>
    public required string[] KeyCategories { get; init; }

    /// <summary>
    /// Sales and fulfillment channels for this persona.
    /// </summary>
    public required string[] Channels { get; init; }

    /// <summary>
    /// Baseline KPIs with typical industry values for demo/simulation.
    /// </summary>
    public required Dictionary<string, double> BaselineKpis { get; init; }

    /// <summary>
    /// Sample decision templates users can select for quick demos.
    /// </summary>
    public required string[] SampleDecisionTemplates { get; init; }

    /// <summary>
    /// Industry-typical assumptions for agent reasoning when sample data is enabled.
    /// </summary>
    public required Dictionary<string, string> BaselineAssumptions { get; init; }
}
