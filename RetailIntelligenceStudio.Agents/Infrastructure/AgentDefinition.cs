using YamlDotNet.Serialization;

namespace RetailIntelligenceStudio.Agents.Infrastructure;

/// <summary>
/// Represents an agent definition loaded from YAML configuration.
/// Contains all metadata and prompt templates for an intelligence role.
/// </summary>
public sealed class AgentDefinition
{
    /// <summary>
    /// Unique identifier for the agent (e.g., "decision_framer").
    /// </summary>
    [YamlMember(Alias = "name")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Human-friendly display name (e.g., "Decision Framer").
    /// </summary>
    [YamlMember(Alias = "display_name")]
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>
    /// Brief description of the agent's responsibility.
    /// </summary>
    [YamlMember(Alias = "description")]
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Type of output this agent produces (e.g., "Decision Brief", "Volume Forecast").
    /// </summary>
    [YamlMember(Alias = "output_type")]
    public string OutputType { get; set; } = string.Empty;

    /// <summary>
    /// Order in the workflow (1-8, where 1 is first).
    /// </summary>
    [YamlMember(Alias = "workflow_order")]
    public int WorkflowOrder { get; set; }

    /// <summary>
    /// Key areas this agent focuses on during analysis.
    /// </summary>
    [YamlMember(Alias = "focus_areas")]
    public List<string> FocusAreas { get; set; } = [];

    /// <summary>
    /// Prompt templates for the agent.
    /// </summary>
    [YamlMember(Alias = "prompts")]
    public AgentPrompts Prompts { get; set; } = new();

    /// <summary>
    /// Optional baseline assumptions to use when sample data is enabled.
    /// Key is the assumption name, value is the assumption template.
    /// </summary>
    [YamlMember(Alias = "baseline_assumptions")]
    public Dictionary<string, string> BaselineAssumptions { get; set; } = [];
}

/// <summary>
/// Contains prompt templates for system and user prompts.
/// Templates support placeholders like {{persona.DisplayName}}, {{request.DecisionText}}, etc.
/// </summary>
public sealed class AgentPrompts
{
    /// <summary>
    /// System prompt template that defines the agent's role and behavior.
    /// Supports placeholders: {{persona.DisplayName}}, {{persona.KeyCategories}}, 
    /// {{persona.Channels}}, {{persona.BaselineKpis}}, {{dataContext}}, {{assumptions}}
    /// </summary>
    [YamlMember(Alias = "system")]
    public string System { get; set; } = string.Empty;

    /// <summary>
    /// User prompt template for the analysis request.
    /// Supports placeholders: {{request.DecisionText}}, {{additionalContext}}, 
    /// {{priorInsights}}, {{specificContext}}
    /// </summary>
    [YamlMember(Alias = "user")]
    public string User { get; set; } = string.Empty;
}
