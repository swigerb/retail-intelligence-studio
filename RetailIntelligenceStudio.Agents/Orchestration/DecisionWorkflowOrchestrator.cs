using System.Collections.Concurrent;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;
using RetailIntelligenceStudio.Agents.Abstractions;
using RetailIntelligenceStudio.Core.Abstractions;
using RetailIntelligenceStudio.Core.Models;

namespace RetailIntelligenceStudio.Agents.Orchestration;

/// <summary>
/// Orchestrates the decision evaluation workflow using a fan-out/fan-in pattern.
/// Executes Decision Framer first, then runs 6 analysis roles in parallel,
/// and finally synthesizes results with Executive Recommendation.
/// Role definitions are loaded from YAML configuration files.
/// </summary>
public sealed class DecisionWorkflowOrchestrator
{
    private static readonly ActivitySource ActivitySource = new("RetailIntelligenceStudio.Orchestration");
    private readonly IReadOnlyDictionary<string, IIntelligenceRole> _roles;
    private readonly IDecisionStore _decisionStore;
    private readonly IPersonaCatalog _personaCatalog;
    private readonly ILogger<DecisionWorkflowOrchestrator> _logger;

    // Role execution order - roles are loaded from YAML definitions
    // These constants define the workflow structure
    private const string FramerRole = "decision_framer";
    private static readonly string[] ParallelAnalysisRoles =
    [
        "shopper_insights",
        "demand_forecasting",
        "inventory_readiness",
        "margin_impact",
        "digital_merchandising",
        "risk_compliance"
    ];
    private const string SynthesisRole = "executive_recommendation";

    public DecisionWorkflowOrchestrator(
        IEnumerable<IIntelligenceRole> roles,
        IDecisionStore decisionStore,
        IPersonaCatalog personaCatalog,
        ILogger<DecisionWorkflowOrchestrator> logger)
    {
        _roles = roles.ToDictionary(r => r.RoleName, r => r);
        _decisionStore = decisionStore;
        _personaCatalog = personaCatalog;
        _logger = logger;
        
        _logger.LogInformation(
            "🧠 Orchestrator initialized with {RoleCount} YAML-driven intelligence roles: [{RoleNames}]", 
            _roles.Count,
            string.Join(", ", _roles.Keys.OrderBy(k => _roles[k].WorkflowOrder)));
    }

    /// <summary>
    /// Executes the decision evaluation workflow and streams events.
    /// </summary>
    public async IAsyncEnumerable<DecisionEvent> ExecuteAsync(
        string decisionId,
        DecisionRequest request,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        using var activity = ActivitySource.StartActivity("ExecuteWorkflow");
        activity?.SetTag("decision.id", decisionId);
        activity?.SetTag("persona", request.Persona.ToString());
        
        var workflowStart = Stopwatch.GetTimestamp();
        var personaContext = _personaCatalog.GetPersonaContext(request.Persona);
        var allInsights = new ConcurrentDictionary<string, RoleInsight>();
        var sequenceCounter = 0;

        _logger.LogInformation(
            "🚀 Starting decision workflow {DecisionId} for persona {Persona}",
            decisionId, request.Persona);

        // Emit workflow start event
        var startEvent = CreateWorkflowEvent(
            decisionId, request.Persona, "workflow",
            AnalysisPhase.Starting, "Retail Intelligence Studio is evaluating your decision...",
            Interlocked.Increment(ref sequenceCounter));
        
        await _decisionStore.AppendEventAsync(startEvent, cancellationToken);
        yield return startEvent;

        // Stage 1: Decision Framer
        _logger.LogInformation("📋 Stage 1: Executing Decision Framer for {DecisionId}", decisionId);
        using (var stage1 = ActivitySource.StartActivity("Stage1:DecisionFramer"))
        {
            stage1?.SetTag("stage", 1);
            stage1?.SetTag("role", FramerRole);
            
            await foreach (var evt in ExecuteRoleAsync(
                FramerRole, decisionId, request, personaContext, allInsights, sequenceCounter, cancellationToken))
            {
                sequenceCounter = evt.SequenceNumber;
                await _decisionStore.AppendEventAsync(evt, cancellationToken);
                yield return evt;
            }
        }

        // Stage 2: Parallel Analysis Roles (fan-out)
        _logger.LogInformation("⚡ Stage 2: Executing {RoleCount} parallel analysis roles for {DecisionId}", 
            ParallelAnalysisRoles.Length, decisionId);
        
        using (var stage2 = ActivitySource.StartActivity("Stage2:ParallelAnalysis"))
        {
            stage2?.SetTag("stage", 2);
            stage2?.SetTag("parallel_roles", string.Join(",", ParallelAnalysisRoles));
            
            var parallelEvent = CreateWorkflowEvent(
                decisionId, request.Persona, "workflow",
                AnalysisPhase.Analyzing, "Running specialized analysis in parallel...",
                Interlocked.Increment(ref sequenceCounter));
            
            await _decisionStore.AppendEventAsync(parallelEvent, cancellationToken);
            yield return parallelEvent;

            // Execute parallel roles and merge their event streams
            await foreach (var evt in ExecuteParallelRolesAsync(
                ParallelAnalysisRoles, decisionId, request, personaContext, allInsights, sequenceCounter, cancellationToken))
            {
                sequenceCounter = Math.Max(sequenceCounter, evt.SequenceNumber);
                await _decisionStore.AppendEventAsync(evt, cancellationToken);
                yield return evt;
            }
        }

        // Stage 3: Executive Recommendation (fan-in)
        _logger.LogInformation("📊 Stage 3: Executing Executive Recommendation for {DecisionId}", decisionId);
        
        using (var stage3 = ActivitySource.StartActivity("Stage3:ExecutiveRecommendation"))
        {
            stage3?.SetTag("stage", 3);
            stage3?.SetTag("role", SynthesisRole);
            stage3?.SetTag("insights_available", allInsights.Count);
            
            var synthesisEvent = CreateWorkflowEvent(
                decisionId, request.Persona, "workflow",
                AnalysisPhase.Reporting, "Synthesizing insights for executive recommendation...",
                Interlocked.Increment(ref sequenceCounter));
            
            await _decisionStore.AppendEventAsync(synthesisEvent, cancellationToken);
            yield return synthesisEvent;

            await foreach (var evt in ExecuteRoleAsync(
                SynthesisRole, decisionId, request, personaContext, allInsights, sequenceCounter, cancellationToken))
            {
                sequenceCounter = evt.SequenceNumber;
                await _decisionStore.AppendEventAsync(evt, cancellationToken);
                yield return evt;
            }
        }

        // Emit workflow complete event
        var workflowDurationMs = Stopwatch.GetElapsedTime(workflowStart).TotalMilliseconds;
        activity?.SetTag("workflow.duration_ms", workflowDurationMs);
        activity?.SetTag("workflow.insights_count", allInsights.Count);
        
        _logger.LogInformation(
            "🎉 Workflow completed for {DecisionId} in {DurationMs:F0}ms with {InsightCount} insights",
            decisionId, workflowDurationMs, allInsights.Count);
        
        var completeEvent = CreateWorkflowEvent(
            decisionId, request.Persona, "workflow",
            AnalysisPhase.Completed, "Decision evaluation complete.",
            Interlocked.Increment(ref sequenceCounter),
            data: new Dictionary<string, object>
            {
                ["totalRoles"] = _roles.Count,
                ["completedRoles"] = allInsights.Count,
                ["durationMs"] = workflowDurationMs
            });
        
        await _decisionStore.AppendEventAsync(completeEvent, cancellationToken);
        await _decisionStore.CompleteAsync(decisionId, cancellationToken);
        yield return completeEvent;

        _logger.LogInformation(
            "Completed decision workflow {DecisionId} with {InsightCount} role insights",
            decisionId, allInsights.Count);
    }

    private async IAsyncEnumerable<DecisionEvent> ExecuteRoleAsync(
        string roleName,
        string decisionId,
        DecisionRequest request,
        PersonaContext personaContext,
        ConcurrentDictionary<string, RoleInsight> allInsights,
        int baseSequence,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        if (!_roles.TryGetValue(roleName, out var role))
        {
            _logger.LogWarning("Role {RoleName} not found", roleName);
            yield break;
        }

        var sequence = baseSequence;
        var eventQueue = new Queue<DecisionEvent>();
        Exception? executionException = null;

        try
        {
            await foreach (var evt in role.AnalyzeAsync(
                decisionId, request, personaContext, allInsights.ToDictionary(), cancellationToken))
            {
                var sequencedEvent = evt with { SequenceNumber = Interlocked.Increment(ref sequence) };
                
                // Capture completed insights for downstream roles
                if (evt.Phase == AnalysisPhase.Completed && evt.Data != null)
                {
                    var insight = new RoleInsight
                    {
                        RoleName = roleName,
                        Summary = evt.Message,
                        KeyFindings = evt.Data.TryGetValue("keyFindings", out var findings) && findings is string[] findingsArray
                            ? findingsArray
                            : [evt.Message],
                        Confidence = evt.Confidence ?? 0.75
                    };
                    allInsights[roleName] = insight;
                }

                eventQueue.Enqueue(sequencedEvent);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing role {RoleName} for decision {DecisionId}", roleName, decisionId);
            executionException = ex;
        }

        // Yield all queued events
        while (eventQueue.TryDequeue(out var evt))
        {
            yield return evt;
        }

        // If there was an exception, yield error event
        if (executionException != null)
        {
            yield return CreateWorkflowEvent(
                decisionId, request.Persona, roleName,
                AnalysisPhase.Error, $"Analysis error: {executionException.Message}",
                Interlocked.Increment(ref sequence));
        }
    }

    private async IAsyncEnumerable<DecisionEvent> ExecuteParallelRolesAsync(
        string[] roleNames,
        string decisionId,
        DecisionRequest request,
        PersonaContext personaContext,
        ConcurrentDictionary<string, RoleInsight> allInsights,
        int baseSequence,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var sequence = baseSequence;
        var eventChannel = System.Threading.Channels.Channel.CreateUnbounded<DecisionEvent>();

        // Start all roles in parallel
        var tasks = roleNames.Select(async roleName =>
        {
            await foreach (var evt in ExecuteRoleAsync(
                roleName, decisionId, request, personaContext, allInsights, 
                Interlocked.Increment(ref sequence) * 100, // Spread sequences to avoid collisions
                cancellationToken))
            {
                await eventChannel.Writer.WriteAsync(evt, cancellationToken);
            }
        }).ToList();

        // Complete the channel when all tasks finish
        _ = Task.WhenAll(tasks).ContinueWith(_ => eventChannel.Writer.Complete(), cancellationToken);

        // Yield events as they arrive from any role
        await foreach (var evt in eventChannel.Reader.ReadAllAsync(cancellationToken))
        {
            yield return evt;
        }
    }

    private static DecisionEvent CreateWorkflowEvent(
        string decisionId,
        RetailPersona persona,
        string roleName,
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
            RoleName = roleName,
            Phase = phase,
            Message = message,
            Confidence = confidence,
            Data = data,
            SequenceNumber = sequenceNumber,
            Timestamp = DateTimeOffset.UtcNow
        };
    }
}
