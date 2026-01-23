using Microsoft.Extensions.Logging;
using RetailIntelligenceStudio.Agents.Infrastructure;
using RetailIntelligenceStudio.Core.Models;

namespace RetailIntelligenceStudio.Agents.Roles;

/// <summary>
/// Estimates sales and volume impact with ranges and uncertainty.
/// </summary>
public sealed class DemandForecastingRole : IntelligenceRoleBase
{
    public override string RoleName => "demand_forecasting";
    public override string DisplayName => "Demand Forecasting";
    public override string Description => "Estimates sales and volume impact with ranges and uncertainty.";
    public override IReadOnlyList<string> FocusAreas => ["Sales Volume", "Demand Curves", "Seasonality", "Promotional Lift"];
    public override string OutputType => "Volume Forecast";
    public override int WorkflowOrder => 3;

    public DemandForecastingRole(IAgentFactory agentFactory, ILogger<DemandForecastingRole> logger) 
        : base(agentFactory, logger)
    {
    }

    protected override string BuildSystemPrompt(PersonaContext persona, bool useSampleData)
    {
        var assumptions = useSampleData && persona.BaselineAssumptions.TryGetValue("promotional_response", out var response)
            ? $"\n\nBaseline Assumption: {response}"
            : "";

        return $"""
            You are the Demand Forecasting analyst for a {persona.DisplayName} retail intelligence system.
            
            Your role is to:
            1. Estimate sales and volume impact of the proposed decision
            2. Provide forecasts with ranges (low, expected, high scenarios)
            3. Quantify uncertainty and key drivers of variance
            4. Identify leading indicators to monitor
            5. Consider seasonality, trends, and market factors
            
            Retail Context:
            - Key Categories: {string.Join(", ", persona.KeyCategories)}
            - Channels: {string.Join(", ", persona.Channels)}
            - Baseline KPIs:
            {FormatKpis(persona.BaselineKpis)}
            {assumptions}
            
            Provide quantitative analysis with:
            - Expected lift/impact (with % ranges)
            - Volume projections
            - Key assumptions driving the forecast
            - Confidence intervals
            - **Confidence: XX%** (your overall confidence in this forecast, 0-100%)
            
            Use precise numbers and ranges. Be transparent about uncertainty.
            """;
    }

    protected override string BuildUserPrompt(
        DecisionRequest request, 
        PersonaContext persona, 
        IReadOnlyDictionary<string, RoleInsight> priorInsights)
    {
        var priorContext = FormatPriorInsights(priorInsights);

        return $"""
            Forecast the demand impact of this decision:
            
            "{request.DecisionText}"
            
            Prior Analysis:
            {priorContext}
            
            Provide:
            - Unit/volume projections (low/expected/high)
            - Revenue impact estimates
            - Key drivers and sensitivities
            - Cannibalization or halo effects
            - Recommended monitoring metrics
            """;
    }
}
