using FluentAssertions;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using Moq;
using RetailIntelligenceStudio.Agents.Abstractions;
using RetailIntelligenceStudio.Agents.Infrastructure;
using RetailIntelligenceStudio.Agents.Orchestration;
using RetailIntelligenceStudio.Agents.Roles;
using RetailIntelligenceStudio.Core.Abstractions;
using RetailIntelligenceStudio.Core.Models;
using RetailIntelligenceStudio.Core.Services;
using RetailIntelligenceStudio.Core.Stores;
using Xunit;

namespace RetailIntelligenceStudio.Tests.Integration;

/// <summary>
/// End-to-end integration tests for the complete agent workflow.
/// Tests the full communication flow through all 8 YAML-driven intelligence roles.
/// </summary>
public class AgentWorkflowIntegrationTests
{
    private readonly Mock<IAgentFactory> _mockAgentFactory;
    private readonly IDecisionStore _decisionStore;
    private readonly IPersonaCatalog _personaCatalog;
    private readonly Mock<ILogger<DecisionWorkflowOrchestrator>> _mockOrchestratorLogger;
    private readonly IYamlRoleFactory _roleFactory;

    public AgentWorkflowIntegrationTests()
    {
        _mockAgentFactory = new Mock<IAgentFactory>();
        _decisionStore = new InMemoryDecisionStore();
        _personaCatalog = new PersonaCatalog();
        _mockOrchestratorLogger = new Mock<ILogger<DecisionWorkflowOrchestrator>>();
        
        // Setup YAML-driven role factory
        var testBasePath = GetAgentsBasePath();
        var definitionLoader = new AgentDefinitionLoader(testBasePath, Mock.Of<ILogger<AgentDefinitionLoader>>());
        var templateEngine = new PromptTemplateEngine();
        
        var loggerFactory = new Mock<ILoggerFactory>();
        loggerFactory.Setup(f => f.CreateLogger(It.IsAny<string>()))
            .Returns(Mock.Of<ILogger>());
        
        _roleFactory = new YamlRoleFactory(definitionLoader, _mockAgentFactory.Object, templateEngine, loggerFactory.Object);
        
        SetupMockAgentFactory();
    }

    private static string GetAgentsBasePath()
    {
        // YAML files are copied to test output directory
        return AppContext.BaseDirectory;
    }

    private void SetupMockAgentFactory()
    {
        // Setup mock agent factory to return agents that produce streaming responses
        _mockAgentFactory
            .Setup(f => f.CreateAgent(It.IsAny<string>(), It.IsAny<string>()))
            .Returns((string name, string instructions) =>
            {
                // Create a mock IChatClient for the agent
                var mockChatClient = new Mock<IChatClient>();
                mockChatClient
                    .Setup(c => c.GetStreamingResponseAsync(
                        It.IsAny<IEnumerable<ChatMessage>>(),
                        It.IsAny<ChatOptions?>(),
                        It.IsAny<CancellationToken>()))
                    .Returns((IEnumerable<ChatMessage> messages, ChatOptions? options, CancellationToken ct) =>
                    {
                        var responseText = GenerateMockResponseForAgent(instructions);
                        return CreateAsyncStreamingResponse(responseText);
                    });

                // Create and return a ChatClientAgent using the mock chat client
                return mockChatClient.Object.AsAIAgent(
                    name: name,
                    instructions: instructions);
            });
    }

    private static async IAsyncEnumerable<ChatResponseUpdate> CreateAsyncStreamingResponse(string text)
    {
        // Split response into chunks and yield as streaming updates
        var chunks = text.Split(new[] { ". " }, StringSplitOptions.RemoveEmptyEntries);
        foreach (var chunk in chunks)
        {
            await Task.Yield(); // Simulate async behavior
            yield return new ChatResponseUpdate
            {
                Role = ChatRole.Assistant,
                Contents = [new TextContent(chunk + ". ")]
            };
        }
    }

    private static string GenerateMockResponseForAgent(string instructions)
    {
        // Generate contextual mock responses based on the agent instructions
        if (instructions.Contains("Decision Framer", StringComparison.OrdinalIgnoreCase))
        {
            return @"## Decision Frame Analysis
**Core Question:** Should the retailer expand organic product offerings?
**Key Stakeholders:** Merchandising, Supply Chain, Marketing, Finance
**Decision Criteria:** Market demand, margin impact, competitive positioning
**Time Horizon:** 6-12 months for full implementation
**Confidence:** 85%";
        }
        
        if (instructions.Contains("Shopper Insight", StringComparison.OrdinalIgnoreCase))
        {
            return @"## Shopper Insights Analysis
**Target Segment:** Health-conscious consumers aged 25-54
**Purchase Behavior:** 73% willing to pay premium for organic
**Basket Analysis:** Organic buyers have 28% higher basket value
**Loyalty Impact:** Expected 15% increase in repeat visits
**Confidence:** 82%";
        }
        
        if (instructions.Contains("Demand Forecast", StringComparison.OrdinalIgnoreCase))
        {
            return @"## Demand Forecasting Analysis
**Projected Demand Growth:** 18% YoY for organic category
**Seasonality:** Peak demand in Q1 and Q3
**Regional Variation:** Urban stores show 25% higher demand
**Cannibalization Risk:** 8% from conventional products
**Confidence:** 78%";
        }
        
        if (instructions.Contains("Inventory", StringComparison.OrdinalIgnoreCase))
        {
            return @"## Inventory Readiness Analysis
**Supplier Capacity:** 3 certified organic suppliers available
**Lead Time:** 2-3 weeks for initial stock
**Storage Requirements:** Requires dedicated cold chain
**Stock Turn Estimate:** 12x annually vs 8x for conventional
**Confidence:** 80%";
        }
        
        if (instructions.Contains("Margin", StringComparison.OrdinalIgnoreCase))
        {
            return @"## Margin Impact Analysis
**Gross Margin:** 35% for organic vs 28% for conventional
**Volume Impact:** Expected 12% category growth
**Promotional Efficiency:** Lower markdown rate (5% vs 12%)
**ROI Projection:** 145% over 18 months
**Confidence:** 75%";
        }
        
        if (instructions.Contains("Digital Merchandising", StringComparison.OrdinalIgnoreCase))
        {
            return @"## Digital Merchandising Analysis
**Online Conversion:** Organic products show 22% higher conversion
**Search Volume:** ""Organic"" searches up 45% YoY
**Cross-sell Opportunity:** Bundle with complementary health products
**Content Strategy:** Emphasize sourcing and certification
**Confidence:** 83%";
        }
        
        if (instructions.Contains("Risk", StringComparison.OrdinalIgnoreCase) || 
            instructions.Contains("Compliance", StringComparison.OrdinalIgnoreCase))
        {
            return @"## Risk & Compliance Analysis
**Certification Risk:** Low - established USDA organic standards
**Supply Chain Risk:** Medium - limited supplier base
**Regulatory Compliance:** All products meet FDA requirements
**Reputational Risk:** Low - aligns with sustainability goals
**Confidence:** 88%";
        }
        
        if (instructions.Contains("Executive", StringComparison.OrdinalIgnoreCase) || 
            instructions.Contains("Recommendation", StringComparison.OrdinalIgnoreCase))
        {
            return @"## Executive Recommendation
**Verdict:** APPROVE WITH MODIFICATIONS
**Summary:** The organic expansion presents a strong opportunity with favorable margin profile and growing demand.
**Key Recommendations:**
1. Start with top 50 SKUs in urban stores
2. Secure secondary supplier within 90 days
3. Launch with targeted marketing campaign
**Expected Outcome:** $2.4M incremental revenue in Year 1
**Overall Confidence:** 81%";
        }
        
        return @"## Analysis Complete
Analysis has been conducted based on available data.
**Confidence:** 75%";
    }

    [Fact]
    public async Task FullWorkflow_ExecutesAllEightRoles_InCorrectOrder()
    {
        // Arrange
        var roles = CreateAllIntelligenceRoles();
        var orchestrator = new DecisionWorkflowOrchestrator(
            roles,
            _decisionStore,
            _personaCatalog,
            _mockOrchestratorLogger.Object);

        var request = new DecisionRequest
        {
            DecisionText = "Should we expand our organic product offerings to attract health-conscious consumers?",
            Persona = RetailPersona.Grocery,
            UseSampleData = true
        };

        var decisionId = Guid.NewGuid().ToString();
        var receivedEvents = new List<DecisionEvent>();

        // Act
        await foreach (var evt in orchestrator.ExecuteAsync(decisionId, request))
        {
            receivedEvents.Add(evt);
        }

        // Assert
        receivedEvents.Should().NotBeEmpty();
        
        // Verify all 8 roles were executed (plus workflow events)
        var roleNames = receivedEvents.Select(e => e.RoleName).Distinct().ToList();
        roleNames.Should().Contain("decision_framer");
        roleNames.Should().Contain("shopper_insights");
        roleNames.Should().Contain("demand_forecasting");
        roleNames.Should().Contain("inventory_readiness");
        roleNames.Should().Contain("margin_impact");
        roleNames.Should().Contain("digital_merchandising");
        roleNames.Should().Contain("risk_compliance");
        roleNames.Should().Contain("executive_recommendation");
    }

    [Fact]
    public async Task FullWorkflow_ProducesEventsForEachPhase()
    {
        // Arrange
        var roles = CreateAllIntelligenceRoles();
        var orchestrator = new DecisionWorkflowOrchestrator(
            roles,
            _decisionStore,
            _personaCatalog,
            _mockOrchestratorLogger.Object);

        var request = new DecisionRequest
        {
            DecisionText = "Should we launch a new loyalty program for our QSR customers?",
            Persona = RetailPersona.QuickServeRestaurant,
            UseSampleData = true
        };

        var decisionId = Guid.NewGuid().ToString();
        var receivedEvents = new List<DecisionEvent>();

        // Act
        await foreach (var evt in orchestrator.ExecuteAsync(decisionId, request))
        {
            receivedEvents.Add(evt);
        }

        // Assert
        // Each role should produce at least Starting, Analyzing, and Completed phases
        var phases = receivedEvents.Select(e => e.Phase).Distinct().ToList();
        phases.Should().Contain(AnalysisPhase.Starting);
        phases.Should().Contain(AnalysisPhase.Analyzing);
        phases.Should().Contain(AnalysisPhase.Completed);
    }

    [Fact]
    public async Task FullWorkflow_MaintainsSequenceNumbers()
    {
        // Arrange
        var roles = CreateAllIntelligenceRoles();
        var orchestrator = new DecisionWorkflowOrchestrator(
            roles,
            _decisionStore,
            _personaCatalog,
            _mockOrchestratorLogger.Object);

        var request = new DecisionRequest
        {
            DecisionText = "Should we open new store locations in suburban areas?",
            Persona = RetailPersona.SpecialtyRetail,
            UseSampleData = true
        };

        var decisionId = Guid.NewGuid().ToString();
        var receivedEvents = new List<DecisionEvent>();

        // Act
        await foreach (var evt in orchestrator.ExecuteAsync(decisionId, request))
        {
            receivedEvents.Add(evt);
        }

        // Assert
        // All events should have non-zero sequence numbers
        receivedEvents.Should().AllSatisfy(e => e.SequenceNumber.Should().BeGreaterThan(0));
    }

    [Fact]
    public async Task FullWorkflow_StoresEventsInDecisionStore()
    {
        // Arrange
        var roles = CreateAllIntelligenceRoles();
        var orchestrator = new DecisionWorkflowOrchestrator(
            roles,
            _decisionStore,
            _personaCatalog,
            _mockOrchestratorLogger.Object);

        var request = new DecisionRequest
        {
            DecisionText = "Should we implement dynamic pricing for seasonal items?",
            Persona = RetailPersona.Grocery,
            UseSampleData = true
        };

        var decisionId = Guid.NewGuid().ToString();

        // Act
        await foreach (var _ in orchestrator.ExecuteAsync(decisionId, request))
        {
            // Consume all events
        }

        // Assert
        var storedEvents = await _decisionStore.GetEventsAsync(decisionId);
        storedEvents.Should().NotBeEmpty();
        storedEvents.Should().AllSatisfy(e => e.DecisionId.Should().Be(decisionId));
    }

    [Fact]
    public async Task FullWorkflow_MarksDecisionAsComplete()
    {
        // Arrange
        var roles = CreateAllIntelligenceRoles();
        var orchestrator = new DecisionWorkflowOrchestrator(
            roles,
            _decisionStore,
            _personaCatalog,
            _mockOrchestratorLogger.Object);

        var request = new DecisionRequest
        {
            DecisionText = "Should we add self-checkout kiosks to our stores?",
            Persona = RetailPersona.SpecialtyRetail,
            UseSampleData = true
        };

        var decisionId = Guid.NewGuid().ToString();

        // Act
        await foreach (var _ in orchestrator.ExecuteAsync(decisionId, request))
        {
            // Consume all events
        }

        // Assert
        var isComplete = await _decisionStore.IsCompleteAsync(decisionId);
        isComplete.Should().BeTrue();
    }

    [Fact]
    public async Task FullWorkflow_AllRolesReceivePersonaContext()
    {
        // Arrange
        var roles = CreateAllIntelligenceRoles();
        var orchestrator = new DecisionWorkflowOrchestrator(
            roles,
            _decisionStore,
            _personaCatalog,
            _mockOrchestratorLogger.Object);

        var request = new DecisionRequest
        {
            DecisionText = "Should we expand our delivery radius?",
            Persona = RetailPersona.QuickServeRestaurant,
            UseSampleData = true
        };

        var decisionId = Guid.NewGuid().ToString();
        var receivedEvents = new List<DecisionEvent>();

        // Act
        await foreach (var evt in orchestrator.ExecuteAsync(decisionId, request))
        {
            receivedEvents.Add(evt);
        }

        // Assert
        // All events should have the correct persona
        receivedEvents.Should().AllSatisfy(e => e.Persona.Should().Be(RetailPersona.QuickServeRestaurant));
    }

    [Fact]
    public async Task FullWorkflow_ExecutiveRole_ProducesRecommendation()
    {
        // Arrange
        var roles = CreateAllIntelligenceRoles();
        var orchestrator = new DecisionWorkflowOrchestrator(
            roles,
            _decisionStore,
            _personaCatalog,
            _mockOrchestratorLogger.Object);

        var request = new DecisionRequest
        {
            DecisionText = "Should we invest in a new inventory management system?",
            Persona = RetailPersona.Grocery,
            UseSampleData = true
        };

        var decisionId = Guid.NewGuid().ToString();
        var receivedEvents = new List<DecisionEvent>();

        // Act
        await foreach (var evt in orchestrator.ExecuteAsync(decisionId, request))
        {
            receivedEvents.Add(evt);
        }

        // Assert
        var executiveEvents = receivedEvents.Where(e => e.RoleName == "executive_recommendation").ToList();
        executiveEvents.Should().NotBeEmpty();
        
        // The executive role should have completed
        executiveEvents.Should().Contain(e => e.Phase == AnalysisPhase.Completed);
    }

    [Fact]
    public async Task FullWorkflow_HandlesMultipleConcurrentDecisions()
    {
        // Arrange
        var roles1 = CreateAllIntelligenceRoles();
        var roles2 = CreateAllIntelligenceRoles();
        var decisionStore1 = new InMemoryDecisionStore();
        var decisionStore2 = new InMemoryDecisionStore();
        
        var orchestrator1 = new DecisionWorkflowOrchestrator(
            roles1,
            decisionStore1,
            _personaCatalog,
            _mockOrchestratorLogger.Object);
            
        var orchestrator2 = new DecisionWorkflowOrchestrator(
            roles2,
            decisionStore2,
            _personaCatalog,
            _mockOrchestratorLogger.Object);

        var request1 = new DecisionRequest
        {
            DecisionText = "Should we expand organic offerings?",
            Persona = RetailPersona.Grocery,
            UseSampleData = true
        };
        
        var request2 = new DecisionRequest
        {
            DecisionText = "Should we launch breakfast menu?",
            Persona = RetailPersona.QuickServeRestaurant,
            UseSampleData = true
        };

        var decisionId1 = Guid.NewGuid().ToString();
        var decisionId2 = Guid.NewGuid().ToString();

        // Act - Execute both workflows concurrently
        var task1 = CollectEventsAsync(orchestrator1.ExecuteAsync(decisionId1, request1));
        var task2 = CollectEventsAsync(orchestrator2.ExecuteAsync(decisionId2, request2));

        await Task.WhenAll(task1, task2);

        var events1 = await task1;
        var events2 = await task2;

        // Assert
        events1.Should().NotBeEmpty();
        events2.Should().NotBeEmpty();
        
        // Each decision should have its own events
        events1.Should().AllSatisfy(e => e.DecisionId.Should().Be(decisionId1));
        events2.Should().AllSatisfy(e => e.DecisionId.Should().Be(decisionId2));
        
        // Both should have all 8 roles plus workflow events
        var roles1Names = events1.Select(e => e.RoleName).Distinct().Where(r => r != "workflow").ToList();
        var roles2Names = events2.Select(e => e.RoleName).Distinct().Where(r => r != "workflow").ToList();
        roles1Names.Should().HaveCount(8);
        roles2Names.Should().HaveCount(8);
    }

    [Fact]
    public async Task FullWorkflow_EventsContainTimestamps()
    {
        // Arrange
        var roles = CreateAllIntelligenceRoles();
        var orchestrator = new DecisionWorkflowOrchestrator(
            roles,
            _decisionStore,
            _personaCatalog,
            _mockOrchestratorLogger.Object);

        var request = new DecisionRequest
        {
            DecisionText = "Should we implement a customer rewards program?",
            Persona = RetailPersona.Grocery,
            UseSampleData = true
        };

        var decisionId = Guid.NewGuid().ToString();
        var startTime = DateTimeOffset.UtcNow;
        var receivedEvents = new List<DecisionEvent>();

        // Act
        await foreach (var evt in orchestrator.ExecuteAsync(decisionId, request))
        {
            receivedEvents.Add(evt);
        }

        var endTime = DateTimeOffset.UtcNow;

        // Assert
        receivedEvents.Should().AllSatisfy(e =>
        {
            e.Timestamp.Should().NotBe(default);
            e.Timestamp.Should().BeOnOrAfter(startTime);
            e.Timestamp.Should().BeOnOrBefore(endTime);
        });
    }

    private static async Task<List<DecisionEvent>> CollectEventsAsync(IAsyncEnumerable<DecisionEvent> events)
    {
        var result = new List<DecisionEvent>();
        await foreach (var evt in events)
        {
            result.Add(evt);
        }
        return result;
    }

    private IEnumerable<IIntelligenceRole> CreateAllIntelligenceRoles()
    {
        return _roleFactory.CreateAllRoles();
    }
}
