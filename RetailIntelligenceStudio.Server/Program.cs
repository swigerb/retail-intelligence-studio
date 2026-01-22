using System.Text.Json;
using System.Text.Json.Serialization;
using RetailIntelligenceStudio.Agents;
using RetailIntelligenceStudio.Agents.Abstractions;
using RetailIntelligenceStudio.Agents.Orchestration;
using RetailIntelligenceStudio.Core.Abstractions;
using RetailIntelligenceStudio.Core.Models;
using RetailIntelligenceStudio.Core.Services;
using RetailIntelligenceStudio.Core.Stores;

var builder = WebApplication.CreateBuilder(args);

// Add service defaults & Aspire client integrations (includes OpenTelemetry).
builder.AddServiceDefaults();

// Configure JSON serialization
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
    options.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
});

// Add services to the container.
builder.Services.AddProblemDetails();
builder.Services.AddOpenApi();

// Configure CORS for frontend development
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// Add SPA static files for production
builder.Services.AddSpaStaticFiles(config =>
{
    config.RootPath = "wwwroot";
});

// Configure Agent Factory using Microsoft Agent Framework
var config = builder.Configuration;
var endpoint = config["AzureOpenAI:Endpoint"];
var deploymentName = config["AzureOpenAI:DeploymentName"] ?? "gpt-4o";

// Check if Azure OpenAI is configured with a real endpoint
if (string.IsNullOrEmpty(endpoint) || endpoint.Contains("your-resource"))
{
    // Use local agent factory for development without Azure OpenAI
    builder.Services.AddLocalAgentFactory();
}
else
{
    // Use Azure OpenAI with Microsoft Agent Framework
    builder.Services.AddAzureOpenAIAgentFactory(endpoint, deploymentName);
}

// Register Core services - In-memory stores for demo
builder.Services.AddSingleton<IStateStore, InMemoryStateStore>();
builder.Services.AddSingleton<IDecisionStore, InMemoryDecisionStore>();
builder.Services.AddSingleton<IPersonaCatalog, PersonaCatalog>();

// Register Agents and Orchestrator
builder.Services.AddRetailIntelligenceAgents();

var app = builder.Build();

// Log OpenTelemetry configuration status at startup
var startupLogger = app.Services.GetRequiredService<ILoggerFactory>().CreateLogger("Startup");
var otlpEndpoint = app.Configuration["OTEL_EXPORTER_OTLP_ENDPOINT"];
startupLogger.LogInformation("🚀 Application starting - OTEL_EXPORTER_OTLP_ENDPOINT: {OtlpEndpoint}", 
    string.IsNullOrEmpty(otlpEndpoint) ? "(not set)" : otlpEndpoint);

// Configure the HTTP request pipeline.
app.UseExceptionHandler();
app.UseCors();

// Add routing middleware early
app.UseRouting();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// Map default endpoints (includes /health and /alive for Aspire)
app.MapDefaultEndpoints();

// API Routes
var api = app.MapGroup("/api");

// API status endpoint (separate from /health to avoid conflicts)
api.MapGet("status", () => Results.Ok(new { status = "healthy", timestamp = DateTimeOffset.UtcNow }))
   .WithName("ApiHealthCheck");

// Personas endpoint
api.MapGet("personas", (IPersonaCatalog catalog) =>
{
    var personas = catalog.GetAllPersonas().Select(p => new
    {
        p.Persona,
        p.DisplayName,
        p.Description,
        p.KeyCategories,
        p.Channels,
        SampleDecisions = p.SampleDecisionTemplates
    });
    return Results.Ok(personas);
})
.WithName("GetPersonas");

// Intelligence Roles endpoint
api.MapGet("roles", (IEnumerable<IIntelligenceRole> roles) =>
{
    var roleData = roles.OrderBy(r => r.WorkflowOrder).Select(r => new
    {
        r.RoleName,
        r.DisplayName,
        r.Description,
        r.FocusAreas,
        r.OutputType,
        r.WorkflowOrder
    });
    return Results.Ok(roleData);
})
.WithName("GetRoles");

// Get sample decision for persona
api.MapGet("personas/{persona}/sample", (RetailPersona persona, IPersonaCatalog catalog, int index = 0) =>
{
    try
    {
        var sample = catalog.GetSampleDecision(persona, index);
        return Results.Ok(new { decisionText = sample });
    }
    catch (ArgumentException)
    {
        return Results.NotFound();
    }
})
.WithName("GetSampleDecision");

// Submit decision for evaluation
api.MapPost("decisions", async (
    DecisionRequest request,
    DecisionWorkflowOrchestrator orchestrator,
    IStateStore stateStore,
    IPersonaCatalog personaCatalog,
    CancellationToken cancellationToken) =>
{
    var decisionId = Guid.NewGuid().ToString("N")[..12];
    var personaContext = personaCatalog.GetPersonaContext(request.Persona);

    // Create initial decision result
    var result = new DecisionResult
    {
        DecisionId = decisionId,
        Request = request,
        PersonaContext = personaContext,
        Status = DecisionStatus.Running,
        StartedAt = DateTimeOffset.UtcNow
    };

    await stateStore.SaveDecisionAsync(result, cancellationToken);

    // Start workflow execution in background
    _ = Task.Run(async () =>
    {
        try
        {
            await foreach (var evt in orchestrator.ExecuteAsync(decisionId, request, cancellationToken))
            {
                result.Events.Add(evt);
            }
            result.Status = DecisionStatus.Completed;
            result.CompletedAt = DateTimeOffset.UtcNow;
        }
        catch (Exception ex)
        {
            result.Status = DecisionStatus.Failed;
            result.CompletedAt = DateTimeOffset.UtcNow;
            // Log error - would be captured by OpenTelemetry
        }
        finally
        {
            await stateStore.UpdateDecisionAsync(result, CancellationToken.None);
        }
    }, cancellationToken);

    return Results.Accepted($"/api/decisions/{decisionId}", new
    {
        decisionId,
        status = "accepted",
        eventsUrl = $"/api/decisions/{decisionId}/events"
    });
})
.WithName("SubmitDecision");

// Get decision status
api.MapGet("decisions/{decisionId}", async (string decisionId, IStateStore stateStore, CancellationToken cancellationToken) =>
{
    var decision = await stateStore.GetDecisionAsync(decisionId, cancellationToken);
    return decision is null ? Results.NotFound() : Results.Ok(decision);
})
.WithName("GetDecision");

// Stream decision events via Server-Sent Events
api.MapGet("decisions/{decisionId}/events", async (
    string decisionId,
    IDecisionStore decisionStore,
    HttpContext httpContext,
    CancellationToken cancellationToken) =>
{
    httpContext.Response.Headers.ContentType = "text/event-stream";
    httpContext.Response.Headers.CacheControl = "no-cache";
    httpContext.Response.Headers.Connection = "keep-alive";

    var jsonOptions = new JsonSerializerOptions
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Converters = { new JsonStringEnumConverter() }
    };

    try
    {
        await foreach (var evt in decisionStore.StreamEventsAsync(decisionId, cancellationToken))
        {
            var json = JsonSerializer.Serialize(evt, jsonOptions);
            await httpContext.Response.WriteAsync($"data: {json}\n\n", cancellationToken);
            await httpContext.Response.Body.FlushAsync(cancellationToken);
        }

        // Send completion event
        await httpContext.Response.WriteAsync("data: {\"type\":\"complete\"}\n\n", cancellationToken);
        await httpContext.Response.Body.FlushAsync(cancellationToken);
    }
    catch (OperationCanceledException)
    {
        // Client disconnected - this is expected
    }
})
.WithName("StreamDecisionEvents");

// List recent decisions
api.MapGet("decisions", async (IStateStore stateStore, int skip = 0, int take = 20, CancellationToken cancellationToken = default) =>
{
    var decisions = await stateStore.ListDecisionsAsync(skip, take, cancellationToken);
    return Results.Ok(decisions.Select(d => new
    {
        d.DecisionId,
        d.Request.DecisionText,
        d.Request.Persona,
        d.Status,
        d.StartedAt,
        d.CompletedAt
    }));
})
.WithName("ListDecisions");

// In development with Aspire, the frontend is served by Vite directly.
// Only serve static files and SPA fallback in production.
if (!app.Environment.IsDevelopment())
{
    // Serve static files and SPA for production
    app.UseStaticFiles();
    app.UseSpaStaticFiles();

    // SPA fallback - only in production
    app.UseSpa(spa =>
    {
        spa.Options.SourcePath = "../frontend";
    });
}

app.Run();

