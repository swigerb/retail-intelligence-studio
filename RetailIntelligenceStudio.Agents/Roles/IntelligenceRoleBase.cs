using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using RetailIntelligenceStudio.Agents.Abstractions;
using RetailIntelligenceStudio.Agents.Infrastructure;
using RetailIntelligenceStudio.Core.Models;

namespace RetailIntelligenceStudio.Agents.Roles;

/// <summary>
/// Base class for intelligence roles providing common AI interaction patterns
/// using Microsoft Agent Framework.
/// </summary>
public abstract class IntelligenceRoleBase : IIntelligenceRole
{
    private static readonly ActivitySource ActivitySource = new("RetailIntelligenceStudio.Agents");
    protected readonly IAgentFactory AgentFactory;
    protected readonly ILogger Logger;

    public abstract string RoleName { get; }
    public abstract string DisplayName { get; }
    public abstract string Description { get; }
    public abstract IReadOnlyList<string> FocusAreas { get; }
    public abstract string OutputType { get; }
    public abstract int WorkflowOrder { get; }

    protected IntelligenceRoleBase(IAgentFactory agentFactory, ILogger logger)
    {
        AgentFactory = agentFactory;
        Logger = logger;
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

        Logger.LogInformation(
            "🔍 [{Role}] Starting analysis for decision {DecisionId} (Persona: {Persona})",
            DisplayName, decisionId, request.Persona);

        // Emit starting event
        yield return CreateEvent(decisionId, request.Persona, AnalysisPhase.Starting, 
            $"{DisplayName} is beginning analysis...", sequenceNumber++);

        yield return CreateEvent(decisionId, request.Persona, AnalysisPhase.Analyzing,
            $"Evaluating decision parameters for {personaContext.DisplayName} context...", sequenceNumber++);

        var systemPrompt = BuildSystemPrompt(personaContext, request.UseSampleData);
        var userPrompt = BuildUserPrompt(request, personaContext, priorInsights);

        Logger.LogDebug(
            "[{Role}] Creating agent with {PriorInsightsCount} prior insights available",
            RoleName, priorInsights.Count);

        // Create an AIAgent using Microsoft Agent Framework's CreateAIAgent pattern
        var agent = AgentFactory.CreateAgent(
            name: $"{RoleName}-agent",
            instructions: systemPrompt);

        // Collect events from streaming into a queue
        var eventQueue = new Queue<DecisionEvent>();
        Exception? streamingException = null;

        // Process streaming response using AIAgent.RunStreamingAsync()
        var responseBuilder = new StringBuilder();
        var insightBuilder = new StringBuilder(); // Accumulates text for meaningful insights
        var tokenCount = 0;
        const int MinInsightLength = 50; // Minimum chars before emitting an insight
        
        try
        {
            Logger.LogInformation(
                "⚡ [{Role}] Invoking AI agent for decision {DecisionId}",
                DisplayName, decisionId);
            
            await foreach (var update in agent.RunStreamingAsync(userPrompt, cancellationToken: cancellationToken))
            {
                if (!string.IsNullOrEmpty(update.Text))
                {
                    responseBuilder.Append(update.Text);
                    insightBuilder.Append(update.Text);
                    tokenCount += update.Text.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length;

                    // Queue intermediate insights when we have a complete sentence of meaningful length
                    if (insightBuilder.Length >= MinInsightLength && 
                        (update.Text.EndsWith('.') || update.Text.EndsWith('!') || update.Text.EndsWith('?') || update.Text.Contains("\n\n")))
                    {
                        var insightText = insightBuilder.ToString().Trim();
                        // Extract the last complete sentence or paragraph for cleaner display
                        var lastSentenceEnd = Math.Max(
                            insightText.LastIndexOf(". ", StringComparison.Ordinal),
                            Math.Max(insightText.LastIndexOf("! ", StringComparison.Ordinal),
                                     insightText.LastIndexOf("? ", StringComparison.Ordinal)));
                        
                        if (lastSentenceEnd > MinInsightLength)
                        {
                            var cleanInsight = insightText[..(lastSentenceEnd + 1)].Trim();
                            // Remove markdown formatting for cleaner display
                            cleanInsight = cleanInsight.Replace("**", "").Replace("##", "").Trim();
                            if (!string.IsNullOrWhiteSpace(cleanInsight))
                            {
                                eventQueue.Enqueue(CreateEvent(decisionId, request.Persona, AnalysisPhase.Reporting,
                                    cleanInsight, sequenceNumber++));
                            }
                            // Keep the remainder for the next insight
                            insightBuilder.Clear();
                            insightBuilder.Append(insightText[(lastSentenceEnd + 1)..]);
                        }
                    }
                }
            }

            // Parse the final response and queue completion
            var finalResponse = responseBuilder.ToString();
            var insight = ParseInsight(finalResponse);
            
            var elapsedMs = Stopwatch.GetElapsedTime(startTime).TotalMilliseconds;
            activity?.SetTag("analysis.duration_ms", elapsedMs);
            activity?.SetTag("analysis.token_count", tokenCount);
            activity?.SetTag("analysis.confidence", insight.Confidence);

            Logger.LogInformation(
                "✅ [{Role}] Completed analysis for decision {DecisionId} in {ElapsedMs:F0}ms (Confidence: {Confidence:P0})",
                DisplayName, decisionId, elapsedMs, insight.Confidence);

            // Build the data dictionary, including any role-specific data from ParseInsight
            var eventData = new Dictionary<string, object>
            {
                ["keyFindings"] = insight.KeyFindings,
                ["fullAnalysis"] = finalResponse
            };
            
            // Merge any role-specific data (like verdict from ExecutiveRecommendation)
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
            Logger.LogError(ex, 
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

    protected abstract string BuildSystemPrompt(PersonaContext persona, bool useSampleData);
    
    protected abstract string BuildUserPrompt(
        DecisionRequest request, 
        PersonaContext persona, 
        IReadOnlyDictionary<string, RoleInsight> priorInsights);

    protected virtual RoleInsight ParseInsight(string response)
    {
        // Default parsing - subclasses can override for structured extraction
        var lines = response.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        var findings = lines
            .Where(l => l.TrimStart().StartsWith('-') || l.TrimStart().StartsWith('•') || l.TrimStart().StartsWith("*"))
            .Select(l => l.TrimStart('-', '•', '*', ' '))
            .Take(5)
            .ToArray();

        return new RoleInsight
        {
            RoleName = RoleName,
            Summary = lines.FirstOrDefault() ?? "Analysis complete.",
            KeyFindings = findings.Length > 0 ? findings : ["Analysis completed successfully."],
            Confidence = 0.75 // Default confidence
        };
    }

    protected DecisionEvent CreateEvent(
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

    protected static string FormatKpis(Dictionary<string, double> kpis)
    {
        return string.Join("\n", kpis.Select(kv => $"  - {kv.Key}: {kv.Value:F2}"));
    }

    protected static string FormatPriorInsights(IReadOnlyDictionary<string, RoleInsight> insights)
    {
        if (insights.Count == 0) return "No prior analysis available.";

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
}
