using Microsoft.Extensions.Logging;
using RetailIntelligenceStudio.Agents.Infrastructure;
using RetailIntelligenceStudio.Core.Models;

namespace RetailIntelligenceStudio.Agents.Roles;

/// <summary>
/// Flags legal, pricing, brand, or operational risks.
/// </summary>
public sealed class RiskComplianceRole : IntelligenceRoleBase
{
    public override string RoleName => "risk_compliance";
    public override string DisplayName => "Risk & Compliance";
    public override string Description => "Flags legal, pricing, brand, or operational risks.";
    public override IReadOnlyList<string> FocusAreas => ["Legal Risk", "Pricing Rules", "Brand Impact", "Operational Risk"];
    public override string OutputType => "Risk Assessment";
    public override int WorkflowOrder => 7;

    public RiskComplianceRole(IAgentFactory agentFactory, ILogger<RiskComplianceRole> logger) 
        : base(agentFactory, logger)
    {
    }

    protected override string BuildSystemPrompt(PersonaContext persona, bool useSampleData)
    {
        return $"""
            You are the Risk & Compliance analyst for a {persona.DisplayName} retail intelligence system.
            
            Your role is to:
            1. Identify legal and regulatory risks
            2. Flag pricing compliance and competitive concerns
            3. Assess brand reputation and PR risks
            4. Evaluate operational and execution risks
            5. Highlight vendor and contractual considerations
            
            Retail Context:
            - Key Categories: {string.Join(", ", persona.KeyCategories)}
            - Channels: {string.Join(", ", persona.Channels)}
            
            Provide risk assessment with:
            - Risk category and severity (Low/Medium/High/Critical)
            - Specific risk descriptions
            - Mitigation recommendations
            - Monitoring requirements
            
            Be thorough but proportionate. Focus on material risks.
            """;
    }

    protected override string BuildUserPrompt(
        DecisionRequest request, 
        PersonaContext persona, 
        IReadOnlyDictionary<string, RoleInsight> priorInsights)
    {
        var priorContext = FormatPriorInsights(priorInsights);

        return $"""
            Assess risks and compliance considerations for this decision:
            
            "{request.DecisionText}"
            
            Prior Analysis:
            {priorContext}
            
            Evaluate:
            - Legal and regulatory compliance (pricing laws, advertising regulations)
            - Competitive and antitrust considerations
            - Brand and reputation risks
            - Operational execution risks
            - Vendor and contractual obligations
            - Customer communication risks
            """;
    }
}
