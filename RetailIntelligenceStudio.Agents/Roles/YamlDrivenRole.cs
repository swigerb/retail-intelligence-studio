using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;
using Microsoft.Extensions.Logging;
using RetailIntelligenceStudio.Agents.Abstractions;
using RetailIntelligenceStudio.Agents.Infrastructure;
using RetailIntelligenceStudio.Core.Models;

namespace RetailIntelligenceStudio.Agents.Roles;

/// <summary>
/// YAML-driven intelligence role that loads its definition from configuration.
/// Replaces hardcoded role implementations with versionable YAML definitions.
/// </summary>
public class YamlDrivenRole : IIntelligenceRole
{
    private static readonly ActivitySource ActivitySource = new("RetailIntelligenceStudio.Agents");
    
    private readonly AgentDefinition _definition;
    private readonly IAgentFactory _agentFactory;
    private readonly IPromptTemplateEngine _templateEngine;
    private readonly ILogger _logger;

    public string RoleName => _definition.Name;
    public string DisplayName => _definition.DisplayName;
    public string Description => _definition.Description;
    public IReadOnlyList<string> FocusAreas => _definition.FocusAreas;
    public string OutputType => _definition.OutputType;
    public int WorkflowOrder => _definition.WorkflowOrder;

    public YamlDrivenRole(
        AgentDefinition definition,
        IAgentFactory agentFactory,
        IPromptTemplateEngine templateEngine,
        ILogger logger)
    {
        _definition = definition ?? throw new ArgumentNullException(nameof(definition));
        _agentFactory = agentFactory ?? throw new ArgumentNullException(nameof(agentFactory));
        _templateEngine = templateEngine ?? throw new ArgumentNullException(nameof(templateEngine));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async IAsyncEnumerable<DecisionEvent> AnalyzeAsync(
        string decisionId,
        DecisionRequest request,
        PersonaContext personaContext,
        IReadOnlyDictionary<string, RoleInsight> priorInsights,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        using var activity = ActivitySource.StartActivity($"Analyze:{RoleName}");
        activity?.SetTag("decision.id", decisionId);
        activity?.SetTag("role.name", RoleName);
        activity?.SetTag("role.displayName", DisplayName);
        activity?.SetTag("persona", request.Persona.ToString());

        var sequenceNumber = 0;
        var startTime = Stopwatch.GetTimestamp();

        _logger.LogInformation(
            "🔍 [{Role}] Starting analysis for decision {DecisionId} (Persona: {Persona})",
            DisplayName, decisionId, request.Persona);

        // Emit starting event
        yield return CreateEvent(decisionId, request.Persona, AnalysisPhase.Starting,
            $"{DisplayName} is beginning analysis...", sequenceNumber++);

        yield return CreateEvent(decisionId, request.Persona, AnalysisPhase.Analyzing,
            $"Evaluating decision parameters for {personaContext.DisplayName} context...", sequenceNumber++);

        // Build prompt context for template rendering
        var promptContext = new PromptContext
        {
            Persona = personaContext,
            Request = request,
            PriorInsights = priorInsights,
            UseSampleData = request.UseSampleData,
            BaselineAssumptions = _definition.BaselineAssumptions
        };

        var systemPrompt = _templateEngine.RenderSystemPrompt(_definition.Prompts.System, promptContext);
        var userPrompt = _templateEngine.RenderUserPrompt(_definition.Prompts.User, promptContext);

        _logger.LogDebug(
            "[{Role}] Creating agent with {PriorInsightsCount} prior insights available",
            RoleName, priorInsights.Count);

        // Create an AIAgent using Microsoft Agent Framework
        var agent = _agentFactory.CreateAgent(
            name: $"{RoleName}-agent",
            instructions: systemPrompt);

        // Collect events from streaming into a queue
        var eventQueue = new Queue<DecisionEvent>();
        Exception? streamingException = null;

        // Process streaming response
        var responseBuilder = new StringBuilder();
        var insightBuilder = new StringBuilder();
        var tokenCount = 0;
        const int MinInsightLength = 50;

        try
        {
            _logger.LogInformation(
                "⚡ [{Role}] Invoking AI agent for decision {DecisionId}",
                DisplayName, decisionId);

            await foreach (var update in agent.RunStreamingAsync(userPrompt, cancellationToken: cancellationToken))
            {
                if (!string.IsNullOrEmpty(update.Text))
                {
                    responseBuilder.Append(update.Text);
                    insightBuilder.Append(update.Text);
                    tokenCount += update.Text.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length;

                    // Queue intermediate insights when we have a complete sentence
                    if (insightBuilder.Length >= MinInsightLength &&
                        (update.Text.EndsWith('.') || update.Text.EndsWith('!') || update.Text.EndsWith('?') || update.Text.Contains("\n\n")))
                    {
                        var insightText = insightBuilder.ToString().Trim();
                        var lastSentenceEnd = Math.Max(
                            insightText.LastIndexOf(". ", StringComparison.Ordinal),
                            Math.Max(insightText.LastIndexOf("! ", StringComparison.Ordinal),
                                     insightText.LastIndexOf("? ", StringComparison.Ordinal)));

                        if (lastSentenceEnd > MinInsightLength)
                        {
                            var cleanInsight = insightText[..(lastSentenceEnd + 1)].Trim();
                            cleanInsight = cleanInsight.Replace("**", "").Replace("##", "").Trim();
                            if (!string.IsNullOrWhiteSpace(cleanInsight))
                            {
                                eventQueue.Enqueue(CreateEvent(decisionId, request.Persona, AnalysisPhase.Reporting,
                                    cleanInsight, sequenceNumber++));
                            }
                            insightBuilder.Clear();
                            insightBuilder.Append(insightText[(lastSentenceEnd + 1)..]);
                        }
                    }
                }
            }

            // Parse the final response
            var finalResponse = responseBuilder.ToString();
            var insight = ParseInsight(finalResponse);

            var elapsedMs = Stopwatch.GetElapsedTime(startTime).TotalMilliseconds;
            activity?.SetTag("analysis.duration_ms", elapsedMs);
            activity?.SetTag("analysis.token_count", tokenCount);
            activity?.SetTag("analysis.confidence", insight.Confidence);

            _logger.LogInformation(
                "✅ [{Role}] Completed analysis for decision {DecisionId} in {ElapsedMs:F0}ms (Confidence: {Confidence:P0})",
                DisplayName, decisionId, elapsedMs, insight.Confidence);

            // Build the data dictionary
            var eventData = new Dictionary<string, object>
            {
                ["keyFindings"] = insight.KeyFindings,
                ["fullAnalysis"] = finalResponse
            };

            // Merge any role-specific data
            if (insight.Data != null)
            {
                foreach (var (key, value) in insight.Data)
                {
                    eventData[key] = value;
                }
            }

            eventQueue.Enqueue(CreateEvent(decisionId, request.Persona, AnalysisPhase.Completed,
                insight.Summary, sequenceNumber++, insight.Confidence, eventData));
        }
        catch (Exception ex)
        {
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            _logger.LogError(ex,
                "❌ [{Role}] Error in analysis for decision {DecisionId}: {ErrorMessage}",
                DisplayName, decisionId, ex.Message);
            streamingException = ex;
        }

        // Yield all queued events
        while (eventQueue.TryDequeue(out var evt))
        {
            yield return evt;
        }

        // If there was an exception, yield error event
        if (streamingException != null)
        {
            yield return CreateEvent(decisionId, request.Persona, AnalysisPhase.Error,
                $"Analysis encountered an issue: {streamingException.Message}", sequenceNumber++);
        }
    }

    protected virtual RoleInsight ParseInsight(string response)
    {
        // Special handling for executive_recommendation to extract verdict
        if (RoleName == "executive_recommendation")
        {
            return ParseExecutiveInsight(response);
        }

        // Default parsing for all other roles
        var lines = response.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        var findings = lines
            .Where(l => l.TrimStart().StartsWith('-') || l.TrimStart().StartsWith('•') || l.TrimStart().StartsWith("*"))
            .Select(l => l.TrimStart('-', '•', '*', ' '))
            .Take(5)
            .ToArray();

        var confidence = ExtractConfidenceFromResponse(response);

        return new RoleInsight
        {
            RoleName = RoleName,
            Summary = lines.FirstOrDefault() ?? "Analysis complete.",
            KeyFindings = findings.Length > 0 ? findings : ["Analysis completed successfully."],
            Confidence = confidence
        };
    }

    private RoleInsight ParseExecutiveInsight(string response)
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

    protected static double ExtractConfidenceFromResponse(string response)
    {
        // Pattern 1: "Confidence: XX%" or "**Confidence:** XX%"
        var percentMatch = System.Text.RegularExpressions.Regex.Match(
            response,
            @"[Cc]onfidence[:\s*]+(\d{1,3})%",
            System.Text.RegularExpressions.RegexOptions.IgnoreCase);

        if (percentMatch.Success && int.TryParse(percentMatch.Groups[1].Value, out var percentValue))
        {
            return Math.Clamp(percentValue / 100.0, 0.0, 1.0);
        }

        // Pattern 2: "confidence: 0.XX" or "confidence level: 0.XX"
        var decimalMatch = System.Text.RegularExpressions.Regex.Match(
            response,
            @"[Cc]onfidence[:\s\w]*?(\d?\.\d+)",
            System.Text.RegularExpressions.RegexOptions.IgnoreCase);

        if (decimalMatch.Success && double.TryParse(decimalMatch.Groups[1].Value, out var decimalValue))
        {
            return Math.Clamp(decimalValue, 0.0, 1.0);
        }

        // Pattern 3: Qualitative confidence
        if (System.Text.RegularExpressions.Regex.IsMatch(response, @"\b(very\s+)?high\s+confidence\b", System.Text.RegularExpressions.RegexOptions.IgnoreCase))
            return 0.90;
        if (System.Text.RegularExpressions.Regex.IsMatch(response, @"\bmoderate\s+confidence\b", System.Text.RegularExpressions.RegexOptions.IgnoreCase))
            return 0.70;
        if (System.Text.RegularExpressions.Regex.IsMatch(response, @"\blow\s+confidence\b", System.Text.RegularExpressions.RegexOptions.IgnoreCase))
            return 0.50;

        // Default confidence if none found
        return 0.75;
    }

    private DecisionEvent CreateEvent(
        string decisionId,
        RetailPersona persona,
        AnalysisPhase phase,
        string message,
        int sequenceNumber,
        double? confidence = null,
        Dictionary<string, object>? data = null)
    {
        return new DecisionEvent
        {
            DecisionId = decisionId,
            Persona = persona,
            RoleName = RoleName,
            Phase = phase,
            Message = message,
            Confidence = confidence,
            Data = data,
            SequenceNumber = sequenceNumber,
            Timestamp = DateTimeOffset.UtcNow
        };
    }
}

/// <summary>
/// Factory for creating YAML-driven role instances from definitions.
/// </summary>
public interface IYamlRoleFactory
{
    /// <summary>
    /// Creates all intelligence roles from loaded YAML definitions.
    /// </summary>
    IEnumerable<IIntelligenceRole> CreateAllRoles();

    /// <summary>
    /// Creates a specific role by name.
    /// </summary>
    IIntelligenceRole CreateRole(string roleName);
}

/// <summary>
/// Factory implementation that creates YamlDrivenRole instances.
/// </summary>
public sealed class YamlRoleFactory : IYamlRoleFactory
{
    private readonly IAgentDefinitionLoader _definitionLoader;
    private readonly IAgentFactory _agentFactory;
    private readonly IPromptTemplateEngine _templateEngine;
    private readonly ILoggerFactory _loggerFactory;

    public YamlRoleFactory(
        IAgentDefinitionLoader definitionLoader,
        IAgentFactory agentFactory,
        IPromptTemplateEngine templateEngine,
        ILoggerFactory loggerFactory)
    {
        _definitionLoader = definitionLoader;
        _agentFactory = agentFactory;
        _templateEngine = templateEngine;
        _loggerFactory = loggerFactory;
    }

    public IEnumerable<IIntelligenceRole> CreateAllRoles()
    {
        var definitions = _definitionLoader.GetAllDefinitions();
        foreach (var (name, definition) in definitions.OrderBy(d => d.Value.WorkflowOrder))
        {
            yield return CreateRoleFromDefinition(definition);
        }
    }

    public IIntelligenceRole CreateRole(string roleName)
    {
        var definition = _definitionLoader.GetDefinition(roleName);
        return CreateRoleFromDefinition(definition);
    }

    private YamlDrivenRole CreateRoleFromDefinition(AgentDefinition definition)
    {
        var logger = _loggerFactory.CreateLogger($"YamlDrivenRole.{definition.Name}");
        return new YamlDrivenRole(definition, _agentFactory, _templateEngine, logger);
    }
}
