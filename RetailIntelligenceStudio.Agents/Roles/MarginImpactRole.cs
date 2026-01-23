using Microsoft.Extensions.Logging;
using RetailIntelligenceStudio.Agents.Infrastructure;
using RetailIntelligenceStudio.Core.Models;

namespace RetailIntelligenceStudio.Agents.Roles;

/// <summary>
/// Evaluates profitability and financial trade-offs.
/// </summary>
public sealed class MarginImpactRole : IntelligenceRoleBase
{
    public override string RoleName => "margin_impact";
    public override string DisplayName => "Margin Impact";
    public override string Description => "Evaluates profitability and financial trade-offs.";
    public override IReadOnlyList<string> FocusAreas => ["Gross Margin", "Promo ROI", "Cost Structure", "Mix Effects"];
    public override string OutputType => "Financial Analysis";
    public override int WorkflowOrder => 5;

    public MarginImpactRole(IAgentFactory agentFactory, ILogger<MarginImpactRole> logger) 
        : base(agentFactory, logger)
    {
    }

    protected override string BuildSystemPrompt(PersonaContext persona, bool useSampleData)
    {
        var assumptions = useSampleData && persona.BaselineAssumptions.TryGetValue("margin_structure", out var margins)
            ? $"\n\nBaseline Assumption: {margins}"
            : "";

        return $"""
            You are the Margin Impact analyst for a {persona.DisplayName} retail intelligence system.
            
            Your role is to:
            1. Evaluate the profitability impact of the proposed decision
            2. Calculate margin effects across products and categories
            3. Identify financial trade-offs and break-even points
            4. Assess vendor funding and promotional allowances
            5. Consider total profit contribution vs. margin rate
            
            Retail Context:
            - Key Categories: {string.Join(", ", persona.KeyCategories)}
            - Channels: {string.Join(", ", persona.Channels)}
            - Baseline KPIs:
            {FormatKpis(persona.BaselineKpis)}
            {assumptions}
            
            Provide financial analysis with:
            - Gross margin impact ($ and %)
            - Break-even volume analysis
            - Vendor funding opportunities
            - Total profit contribution outlook
            - **Confidence: XX%** (your confidence in this financial analysis, 0-100%)
            
            Be precise with financial impacts. Highlight trade-offs clearly.
            """;
    }

    protected override string BuildUserPrompt(
        DecisionRequest request, 
        PersonaContext persona, 
        IReadOnlyDictionary<string, RoleInsight> priorInsights)
    {
        var priorContext = FormatPriorInsights(priorInsights);

        return $"""
            Analyze the margin and profitability impact of this decision:
            
            "{request.DecisionText}"
            
            Prior Analysis:
            {priorContext}
            
            Calculate:
            - Direct margin impact from price/promotion changes
            - Volume-driven profit contribution changes
            - Break-even point (what volume lift is needed to maintain profit?)
            - Vendor funding or trade promotion considerations
            - Net financial outcome under different scenarios
            """;
    }
}
