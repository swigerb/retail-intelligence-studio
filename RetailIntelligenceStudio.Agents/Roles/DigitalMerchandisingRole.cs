using Microsoft.Extensions.Logging;
using RetailIntelligenceStudio.Agents.Infrastructure;
using RetailIntelligenceStudio.Core.Models;

namespace RetailIntelligenceStudio.Agents.Roles;

/// <summary>
/// Recommends execution strategy across digital and physical channels.
/// </summary>
public sealed class DigitalMerchandisingRole : IntelligenceRoleBase
{
    public override string RoleName => "digital_merchandising";
    public override string DisplayName => "Digital Merchandising";
    public override string Description => "Recommends execution strategy across digital and physical channels.";
    public override IReadOnlyList<string> FocusAreas => ["Channel Strategy", "Placement", "Timing", "Creative Execution"];
    public override string OutputType => "Execution Plan";
    public override int WorkflowOrder => 6;

    public DigitalMerchandisingRole(IAgentFactory agentFactory, ILogger<DigitalMerchandisingRole> logger) 
        : base(agentFactory, logger)
    {
    }

    protected override string BuildSystemPrompt(PersonaContext persona, bool useSampleData)
    {
        return $"""
            You are the Digital Merchandising strategist for a {persona.DisplayName} retail intelligence system.
            
            Your role is to:
            1. Develop execution strategy across all channels
            2. Recommend digital placement and visibility tactics
            3. Plan in-store merchandising and signage
            4. Coordinate omnichannel customer experience
            5. Suggest marketing and promotional messaging
            
            Retail Context:
            - Key Categories: {string.Join(", ", persona.KeyCategories)}
            - Channels: {string.Join(", ", persona.Channels)}
            - Baseline KPIs:
            {FormatKpis(persona.BaselineKpis)}
            
            Provide execution recommendations with:
            - Channel-specific tactics
            - Placement and visibility strategy
            - Messaging and creative direction
            - Timing and phasing recommendations
            - **Confidence: XX%** (your confidence in this strategy, 0-100%)
            
            Focus on actionable execution plans.
            """;
    }

    protected override string BuildUserPrompt(
        DecisionRequest request, 
        PersonaContext persona, 
        IReadOnlyDictionary<string, RoleInsight> priorInsights)
    {
        var shopperContext = priorInsights.TryGetValue("shopper_insights", out var shopper)
            ? $"\n\nShopper Insights:\n{shopper.Summary}"
            : "";

        return $"""
            Develop the merchandising and execution strategy for this decision:
            
            "{request.DecisionText}"
            {shopperContext}
            
            Recommend:
            - Digital channel tactics (website, app, email, social)
            - In-store execution (placement, signage, displays)
            - Omnichannel coordination approach
            - Marketing message and creative themes
            - Launch timing and duration
            """;
    }
}
