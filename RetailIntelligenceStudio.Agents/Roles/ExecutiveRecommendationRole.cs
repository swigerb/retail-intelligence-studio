using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using RetailIntelligenceStudio.Agents.Abstractions;
using RetailIntelligenceStudio.Agents.Infrastructure;
using RetailIntelligenceStudio.Core.Models;

namespace RetailIntelligenceStudio.Agents.Roles;

/// <summary>
/// Synthesizes all insights into a final recommendation: Approve, Approve with Modifications, or Decline.
/// Final role to execute in the workflow, after all analysis roles complete.
/// </summary>
public sealed class ExecutiveRecommendationRole : IntelligenceRoleBase
{
    public override string RoleName => "executive_recommendation";
    public override string DisplayName => "Executive Recommendation";
    public override string Description => "Synthesizes all insights into a final recommendation with rationale, actions, and KPIs.";
    public override IReadOnlyList<string> FocusAreas => ["Go/No-Go Verdict", "Key Trade-offs", "Action Items", "Success KPIs"];
    public override string OutputType => "Final Recommendation";
    public override int WorkflowOrder => 8;

    public ExecutiveRecommendationRole(IAgentFactory agentFactory, ILogger<ExecutiveRecommendationRole> logger) 
        : base(agentFactory, logger)
    {
    }

    protected override string BuildSystemPrompt(PersonaContext persona, bool useSampleData)
    {
        return $"""
            You are the Executive Recommendation synthesizer for a {persona.DisplayName} retail intelligence system.
            
            Your role is to:
            1. Synthesize insights from all analysis roles
            2. Weigh trade-offs and make a clear recommendation
            3. Provide actionable next steps
            4. Project expected KPI outcomes
            5. Identify risks to monitor
            
            Retail Context:
            - Key Categories: {string.Join(", ", persona.KeyCategories)}
            - Channels: {string.Join(", ", persona.Channels)}
            - Baseline KPIs:
            {FormatKpis(persona.BaselineKpis)}
            
            Your recommendation must be one of:
            - APPROVE: Proceed with the decision as proposed
            - APPROVE WITH MODIFICATIONS: Proceed with specific changes
            - DECLINE: Do not proceed, with clear reasoning
            
            Output Format:
            1. Verdict (APPROVE / APPROVE WITH MODIFICATIONS / DECLINE)
            2. Executive Summary (2-3 sentences)
            3. Key Rationale (3-5 bullet points)
            4. Recommended Actions (if approved)
            5. Suggested Modifications (if applicable)
            6. Risks to Monitor
            7. Projected KPIs
            
            Be decisive, clear, and executive-appropriate in tone.
            """;
    }

    protected override string BuildUserPrompt(
        DecisionRequest request, 
        PersonaContext persona, 
        IReadOnlyDictionary<string, RoleInsight> priorInsights)
    {
        var analysisBuilder = new StringBuilder();
        analysisBuilder.AppendLine("Analysis from Intelligence Roles:");
        analysisBuilder.AppendLine("=".PadRight(50, '='));

        foreach (var (role, insight) in priorInsights)
        {
            analysisBuilder.AppendLine();
            analysisBuilder.AppendLine($"## {role.Replace("_", " ").ToUpperInvariant()}");
            analysisBuilder.AppendLine($"Summary: {insight.Summary}");
            analysisBuilder.AppendLine($"Confidence: {insight.Confidence:P0}");
            analysisBuilder.AppendLine("Key Findings:");
            foreach (var finding in insight.KeyFindings)
            {
                analysisBuilder.AppendLine($"  • {finding}");
            }
        }

        return $"""
            Synthesize the following analysis and provide an executive recommendation:
            
            DECISION BEING EVALUATED:
            "{request.DecisionText}"
            
            {analysisBuilder}
            
            Based on all the analysis above, provide your executive recommendation.
            Weigh the trade-offs, consider the risks, and make a clear decision.
            """;
    }

    protected override RoleInsight ParseInsight(string response)
    {
        // Extract verdict from response
        var verdict = "APPROVE";
        if (response.Contains("DECLINE", StringComparison.OrdinalIgnoreCase))
            verdict = "DECLINE";
        else if (response.Contains("MODIFICATIONS", StringComparison.OrdinalIgnoreCase) ||
                 response.Contains("WITH CHANGES", StringComparison.OrdinalIgnoreCase))
            verdict = "APPROVE WITH MODIFICATIONS";

        var lines = response.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        var findings = lines
            .Where(l => l.TrimStart().StartsWith('-') || l.TrimStart().StartsWith('•') || l.TrimStart().StartsWith("*"))
            .Select(l => l.TrimStart('-', '•', '*', ' '))
            .Take(7)
            .ToArray();

        // Find summary (usually after "Summary" or first substantial paragraph)
        var summaryLine = lines.FirstOrDefault(l => 
            l.Length > 50 && 
            !l.StartsWith('#') && 
            !l.StartsWith('-') && 
            !l.StartsWith('•')) ?? $"Recommendation: {verdict}";

        return new RoleInsight
        {
            RoleName = RoleName,
            Summary = summaryLine.Trim(),
            KeyFindings = findings.Length > 0 ? findings : [$"Verdict: {verdict}"],
            Confidence = verdict == "APPROVE" ? 0.85 : verdict == "DECLINE" ? 0.80 : 0.75,
            Data = new Dictionary<string, object>
            {
                ["verdict"] = verdict
            }
        };
    }
}
