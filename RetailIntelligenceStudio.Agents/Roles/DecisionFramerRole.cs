using Microsoft.Extensions.Logging;
using RetailIntelligenceStudio.Agents.Infrastructure;
using RetailIntelligenceStudio.Core.Models;

namespace RetailIntelligenceStudio.Agents.Roles;

/// <summary>
/// Clarifies the business question and produces a structured Decision Brief.
/// First role to execute in the workflow.
/// </summary>
public sealed class DecisionFramerRole : IntelligenceRoleBase
{
    public override string RoleName => "decision_framer";
    public override string DisplayName => "Decision Framer";
    public override string Description => "Clarifies the business question and produces a structured Decision Brief.";
    public override IReadOnlyList<string> FocusAreas => ["Business Question", "Success Criteria", "Scope Definition", "Key Assumptions"];
    public override string OutputType => "Decision Brief";
    public override int WorkflowOrder => 1;

    public DecisionFramerRole(IAgentFactory agentFactory, ILogger<DecisionFramerRole> logger) 
        : base(agentFactory, logger)
    {
    }

    protected override string BuildSystemPrompt(PersonaContext persona, bool useSampleData)
    {
        var dataContext = useSampleData
            ? $"You have access to baseline industry data for {persona.DisplayName}. Use these assumptions to enrich your analysis."
            : "You are working only with the information provided by the user. Clearly state when you are making general industry assumptions.";

        return $"""
            You are the Decision Framer for a {persona.DisplayName} retail intelligence system.
            
            Your role is to:
            1. Clarify and structure the business decision being evaluated
            2. Identify the core question, proposed action, and scope
            3. Define success criteria and key assumptions
            4. Frame the decision for subsequent analysis by specialized intelligence roles
            
            {dataContext}
            
            Retail Context:
            - Key Categories: {string.Join(", ", persona.KeyCategories)}
            - Channels: {string.Join(", ", persona.Channels)}
            
            Output Format:
            Provide a clear, structured Decision Brief with:
            - Core Business Question
            - Proposed Action
            - Scope (products, stores, regions)
            - Timeline
            - Success Criteria
            - Key Assumptions
            - **Confidence: XX%** (your confidence level in this framing, 0-100%)
            
            Be concise, professional, and business-focused. Avoid technical jargon.
            """;
    }

    protected override string BuildUserPrompt(
        DecisionRequest request, 
        PersonaContext persona, 
        IReadOnlyDictionary<string, RoleInsight> priorInsights)
    {
        var contextDetails = new List<string>();
        
        if (!string.IsNullOrEmpty(request.Region))
            contextDetails.Add($"Region: {request.Region}");
        if (!string.IsNullOrEmpty(request.Category))
            contextDetails.Add($"Category: {request.Category}");
        if (!string.IsNullOrEmpty(request.Timeframe))
            contextDetails.Add($"Timeframe: {request.Timeframe}");

        var additionalContext = contextDetails.Count > 0
            ? $"\n\nAdditional Context:\n{string.Join("\n", contextDetails)}"
            : "";

        return $"""
            Please analyze and frame the following business decision:
            
            "{request.DecisionText}"
            {additionalContext}
            
            Create a structured Decision Brief that clearly articulates what is being decided,
            the scope of the decision, and the criteria for success.
            """;
    }
}
