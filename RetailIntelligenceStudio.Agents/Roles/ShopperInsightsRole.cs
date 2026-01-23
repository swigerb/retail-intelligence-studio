using Microsoft.Extensions.Logging;
using RetailIntelligenceStudio.Agents.Infrastructure;
using RetailIntelligenceStudio.Core.Models;

namespace RetailIntelligenceStudio.Agents.Roles;

/// <summary>
/// Evaluates customer behavior, price sensitivity, loyalty, and basket impact.
/// </summary>
public sealed class ShopperInsightsRole : IntelligenceRoleBase
{
    public override string RoleName => "shopper_insights";
    public override string DisplayName => "Shopper Insights";
    public override string Description => "Evaluates customer behavior, price sensitivity, loyalty, and basket impact.";
    public override IReadOnlyList<string> FocusAreas => ["Customer Segments", "Price Sensitivity", "Loyalty Impact", "Basket Composition"];
    public override string OutputType => "Behavioral Analysis";
    public override int WorkflowOrder => 2;

    public ShopperInsightsRole(IAgentFactory agentFactory, ILogger<ShopperInsightsRole> logger) 
        : base(agentFactory, logger)
    {
    }

    protected override string BuildSystemPrompt(PersonaContext persona, bool useSampleData)
    {
        var assumptions = useSampleData && persona.BaselineAssumptions.TryGetValue("price_elasticity", out var elasticity)
            ? $"\n\nBaseline Assumption: {elasticity}"
            : "";

        return $"""
            You are the Shopper Insights analyst for a {persona.DisplayName} retail intelligence system.
            
            Your role is to analyze:
            1. Customer behavior patterns and shopping habits
            2. Price sensitivity and elasticity considerations
            3. Loyalty program impact and customer lifetime value
            4. Basket composition and cross-category effects
            5. Channel preferences and shopping journey
            
            Retail Context:
            - Key Categories: {string.Join(", ", persona.KeyCategories)}
            - Channels: {string.Join(", ", persona.Channels)}
            - Baseline KPIs:
            {FormatKpis(persona.BaselineKpis)}
            {assumptions}
            
            Provide actionable insights with:
            - Customer segment impact analysis
            - Expected behavioral changes
            - Loyalty and retention implications
            - **Confidence: XX%** (your overall confidence in this analysis, 0-100%)
            
            Be data-driven, specific, and business-focused.
            """;
    }

    protected override string BuildUserPrompt(
        DecisionRequest request, 
        PersonaContext persona, 
        IReadOnlyDictionary<string, RoleInsight> priorInsights)
    {
        var framerContext = priorInsights.TryGetValue("decision_framer", out var framer)
            ? $"\n\nDecision Brief:\n{framer.Summary}\n\nKey Points:\n{string.Join("\n", framer.KeyFindings.Select(f => $"- {f}"))}"
            : "";

        return $"""
            Analyze the shopper implications of this decision:
            
            "{request.DecisionText}"
            {framerContext}
            
            Consider:
            - How will different customer segments respond?
            - What is the expected impact on basket size and trip frequency?
            - Are there loyalty or retention risks?
            - How might this affect customer perception and brand equity?
            """;
    }
}
