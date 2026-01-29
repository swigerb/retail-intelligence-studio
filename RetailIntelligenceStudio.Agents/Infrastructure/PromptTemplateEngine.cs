using System.Text;
using System.Text.RegularExpressions;
using RetailIntelligenceStudio.Core.Models;

namespace RetailIntelligenceStudio.Agents.Infrastructure;

/// <summary>
/// Lightweight template engine for processing agent prompt templates.
/// Supports {{placeholder}} syntax with nested property access.
/// </summary>
public interface IPromptTemplateEngine
{
    /// <summary>
    /// Renders a system prompt template with the provided context.
    /// </summary>
    string RenderSystemPrompt(string template, PromptContext context);

    /// <summary>
    /// Renders a user prompt template with the provided context.
    /// </summary>
    string RenderUserPrompt(string template, PromptContext context);
}

/// <summary>
/// Context object containing all data available for prompt template rendering.
/// </summary>
public sealed class PromptContext
{
    public required PersonaContext Persona { get; init; }
    public required DecisionRequest Request { get; init; }
    public IReadOnlyDictionary<string, RoleInsight> PriorInsights { get; init; } = new Dictionary<string, RoleInsight>();
    public bool UseSampleData { get; init; }
    public Dictionary<string, string> BaselineAssumptions { get; init; } = [];
}

/// <summary>
/// Simple template engine using {{placeholder}} syntax.
/// Supports: {{persona.DisplayName}}, {{request.DecisionText}}, {{priorInsights}}, etc.
/// </summary>
public sealed partial class PromptTemplateEngine : IPromptTemplateEngine
{
    [GeneratedRegex(@"\{\{(\w+(?:\.\w+)*)\}\}", RegexOptions.Compiled)]
    private static partial Regex PlaceholderPattern();

    public string RenderSystemPrompt(string template, PromptContext context)
    {
        var values = BuildSystemPromptValues(context);
        return RenderTemplate(template, values);
    }

    public string RenderUserPrompt(string template, PromptContext context)
    {
        var values = BuildUserPromptValues(context);
        return RenderTemplate(template, values);
    }

    private static string RenderTemplate(string template, Dictionary<string, string> values)
    {
        if (string.IsNullOrEmpty(template))
            return string.Empty;

        return PlaceholderPattern().Replace(template, match =>
        {
            var key = match.Groups[1].Value;
            return values.TryGetValue(key, out var value) ? value : match.Value;
        });
    }

    private static Dictionary<string, string> BuildSystemPromptValues(PromptContext context)
    {
        var values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            // Persona properties
            ["persona.DisplayName"] = context.Persona.DisplayName,
            ["persona.KeyCategories"] = string.Join(", ", context.Persona.KeyCategories),
            ["persona.Channels"] = string.Join(", ", context.Persona.Channels),
            ["persona.BaselineKpis"] = FormatKpis(context.Persona.BaselineKpis),

            // Data context based on sample data flag
            ["dataContext"] = context.UseSampleData
                ? $"You have access to baseline industry data for {context.Persona.DisplayName}. Use these assumptions to enrich your analysis."
                : "You are working only with the information provided by the user. Clearly state when you are making general industry assumptions.",

            // Baseline assumptions (if sample data enabled)
            ["assumptions"] = BuildAssumptionsText(context)
        };

        return values;
    }

    private static Dictionary<string, string> BuildUserPromptValues(PromptContext context)
    {
        var values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            // Request properties
            ["request.DecisionText"] = context.Request.DecisionText,
            ["request.Region"] = context.Request.Region ?? string.Empty,
            ["request.Category"] = context.Request.Category ?? string.Empty,
            ["request.Timeframe"] = context.Request.Timeframe ?? string.Empty,

            // Additional context from request
            ["additionalContext"] = BuildAdditionalContext(context.Request),

            // Prior insights from earlier roles
            ["priorInsights"] = FormatPriorInsights(context.PriorInsights),

            // Specific role contexts
            ["framerContext"] = BuildFramerContext(context.PriorInsights),
            ["demandContext"] = BuildDemandContext(context.PriorInsights),
            ["shopperContext"] = BuildShopperContext(context.PriorInsights),
            ["allRolesContext"] = BuildAllRolesContext(context.PriorInsights)
        };

        return values;
    }

    private static string BuildAssumptionsText(PromptContext context)
    {
        if (!context.UseSampleData || context.BaselineAssumptions.Count == 0)
            return string.Empty;

        var sb = new StringBuilder();
        foreach (var (key, value) in context.BaselineAssumptions)
        {
            // Try to get the assumption from persona's baseline assumptions
            if (context.Persona.BaselineAssumptions.TryGetValue(key, out var assumptionValue))
            {
                sb.AppendLine($"\nBaseline Assumption: {assumptionValue}");
            }
        }
        return sb.ToString();
    }

    private static string BuildAdditionalContext(DecisionRequest request)
    {
        var contextDetails = new List<string>();

        if (!string.IsNullOrEmpty(request.Region))
            contextDetails.Add($"Region: {request.Region}");
        if (!string.IsNullOrEmpty(request.Category))
            contextDetails.Add($"Category: {request.Category}");
        if (!string.IsNullOrEmpty(request.Timeframe))
            contextDetails.Add($"Timeframe: {request.Timeframe}");

        return contextDetails.Count > 0
            ? $"\n\nAdditional Context:\n{string.Join("\n", contextDetails)}"
            : string.Empty;
    }

    private static string BuildFramerContext(IReadOnlyDictionary<string, RoleInsight> insights)
    {
        if (!insights.TryGetValue("decision_framer", out var framer))
            return string.Empty;

        return $"\n\nDecision Brief:\n{framer.Summary}\n\nKey Points:\n{string.Join("\n", framer.KeyFindings.Select(f => $"- {f}"))}";
    }

    private static string BuildDemandContext(IReadOnlyDictionary<string, RoleInsight> insights)
    {
        if (!insights.TryGetValue("demand_forecasting", out var demand))
            return string.Empty;

        return $"\n\nDemand Forecast:\n{demand.Summary}\nKey Findings:\n{string.Join("\n", demand.KeyFindings.Select(f => $"- {f}"))}";
    }

    private static string BuildShopperContext(IReadOnlyDictionary<string, RoleInsight> insights)
    {
        if (!insights.TryGetValue("shopper_insights", out var shopper))
            return string.Empty;

        return $"\n\nShopper Insights:\n{shopper.Summary}";
    }

    private static string BuildAllRolesContext(IReadOnlyDictionary<string, RoleInsight> insights)
    {
        if (insights.Count == 0)
            return "No prior analysis available.";

        var sb = new StringBuilder();
        sb.AppendLine("Analysis from Intelligence Roles:");
        sb.AppendLine("=".PadRight(50, '='));

        foreach (var (role, insight) in insights)
        {
            sb.AppendLine();
            sb.AppendLine($"## {role.Replace("_", " ").ToUpperInvariant()}");
            sb.AppendLine($"Summary: {insight.Summary}");
            sb.AppendLine($"Confidence: {insight.Confidence:P0}");
            sb.AppendLine("Key Findings:");
            foreach (var finding in insight.KeyFindings)
            {
                sb.AppendLine($"  • {finding}");
            }
        }

        return sb.ToString();
    }

    private static string FormatPriorInsights(IReadOnlyDictionary<string, RoleInsight> insights)
    {
        if (insights.Count == 0)
            return "No prior analysis available.";

        var sb = new StringBuilder();
        foreach (var (role, insight) in insights)
        {
            sb.AppendLine($"\n{role}:");
            sb.AppendLine($"  Summary: {insight.Summary}");
            sb.AppendLine($"  Key Findings:");
            foreach (var finding in insight.KeyFindings.Take(3))
            {
                sb.AppendLine($"    - {finding}");
            }
        }
        return sb.ToString();
    }

    private static string FormatKpis(Dictionary<string, double> kpis)
    {
        return string.Join("\n", kpis.Select(kv => $"  - {kv.Key}: {kv.Value:F2}"));
    }
}
