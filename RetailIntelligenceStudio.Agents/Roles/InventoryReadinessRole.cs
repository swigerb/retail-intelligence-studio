using Microsoft.Extensions.Logging;
using RetailIntelligenceStudio.Agents.Infrastructure;
using RetailIntelligenceStudio.Core.Models;

namespace RetailIntelligenceStudio.Agents.Roles;

/// <summary>
/// Assesses supply, lead times, and fulfillment feasibility.
/// </summary>
public sealed class InventoryReadinessRole : IntelligenceRoleBase
{
    public override string RoleName => "inventory_readiness";
    public override string DisplayName => "Inventory Readiness";
    public override string Description => "Assesses supply, lead times, and fulfillment feasibility.";
    public override IReadOnlyList<string> FocusAreas => ["Stock Levels", "Lead Times", "Supplier Capacity", "Fulfillment Risk"];
    public override string OutputType => "Supply Assessment";
    public override int WorkflowOrder => 4;

    public InventoryReadinessRole(IAgentFactory agentFactory, ILogger<InventoryReadinessRole> logger) 
        : base(agentFactory, logger)
    {
    }

    protected override string BuildSystemPrompt(PersonaContext persona, bool useSampleData)
    {
        var assumptions = useSampleData && persona.BaselineAssumptions.TryGetValue("inventory_lead_time", out var leadTime)
            ? $"\n\nBaseline Assumption: {leadTime}"
            : "";

        return $"""
            You are the Inventory Readiness analyst for a {persona.DisplayName} retail intelligence system.
            
            Your role is to:
            1. Assess supply chain readiness for the proposed decision
            2. Evaluate inventory levels, lead times, and replenishment capacity
            3. Identify fulfillment risks and constraints
            4. Recommend inventory positioning strategies
            5. Consider warehouse, distribution, and store-level impacts
            
            Retail Context:
            - Key Categories: {string.Join(", ", persona.KeyCategories)}
            - Channels: {string.Join(", ", persona.Channels)}
            - Baseline KPIs:
            {FormatKpis(persona.BaselineKpis)}
            {assumptions}
            
            Provide operational analysis with:
            - Supply readiness assessment (Ready/At Risk/Not Ready)
            - Lead time considerations
            - Stock-out risk evaluation
            - Recommended safety stock adjustments
            
            Focus on execution feasibility and operational risks.
            """;
    }

    protected override string BuildUserPrompt(
        DecisionRequest request, 
        PersonaContext persona, 
        IReadOnlyDictionary<string, RoleInsight> priorInsights)
    {
        var demandContext = priorInsights.TryGetValue("demand_forecasting", out var demand)
            ? $"\n\nDemand Forecast:\n{demand.Summary}\nKey Findings:\n{string.Join("\n", demand.KeyFindings.Select(f => $"- {f}"))}"
            : "";

        return $"""
            Assess inventory readiness for this decision:
            
            "{request.DecisionText}"
            {demandContext}
            
            Evaluate:
            - Current inventory positions and coverage
            - Supplier lead times and capacity constraints
            - Distribution network readiness
            - Store-level fulfillment capabilities
            - Risk of stock-outs during promotion period
            """;
    }
}
