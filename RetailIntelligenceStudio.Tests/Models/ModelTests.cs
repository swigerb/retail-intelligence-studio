using FluentAssertions;
using RetailIntelligenceStudio.Core.Models;

namespace RetailIntelligenceStudio.Tests.Models;

public class DecisionRequestTests
{
    [Fact]
    public void DecisionRequest_CreatesWithValidDefaults()
    {
        // Act
        var request = new DecisionRequest
        {
            DecisionText = "Test question",
            Persona = RetailPersona.Grocery
        };

        // Assert
        request.DecisionText.Should().Be("Test question");
        request.Persona.Should().Be(RetailPersona.Grocery);
        request.UseSampleData.Should().BeTrue();
        request.Region.Should().BeNull();
        request.Category.Should().BeNull();
        request.Timeframe.Should().BeNull();
    }

    [Theory]
    [InlineData(RetailPersona.Grocery)]
    [InlineData(RetailPersona.QuickServeRestaurant)]
    [InlineData(RetailPersona.SpecialtyRetail)]
    public void DecisionRequest_AcceptsAllPersonas(RetailPersona persona)
    {
        // Act
        var request = new DecisionRequest
        {
            DecisionText = "Test",
            Persona = persona
        };

        // Assert
        request.Persona.Should().Be(persona);
    }

    [Fact]
    public void DecisionRequest_CanSetOptionalFields()
    {
        // Act
        var request = new DecisionRequest
        {
            DecisionText = "Test",
            Persona = RetailPersona.Grocery,
            UseSampleData = false,
            Region = "Southeast",
            Category = "beverages",
            Timeframe = "Q2 2026"
        };

        // Assert
        request.UseSampleData.Should().BeFalse();
        request.Region.Should().Be("Southeast");
        request.Category.Should().Be("beverages");
        request.Timeframe.Should().Be("Q2 2026");
    }
}

public class DecisionResultTests
{
    [Fact]
    public void DecisionResult_InitializesCorrectly()
    {
        // Act
        var result = CreateTestDecisionResult();

        // Assert
        result.DecisionId.Should().NotBeNullOrEmpty();
        result.Request.Should().NotBeNull();
        result.Status.Should().Be(DecisionStatus.Pending);
        result.Events.Should().NotBeNull().And.BeEmpty();
        result.RoleInsights.Should().NotBeNull().And.BeEmpty();
        result.Recommendation.Should().BeNull();
    }

    [Fact]
    public void DecisionResult_CanAddRoleInsights()
    {
        // Arrange
        var result = CreateTestDecisionResult();

        // Act
        result.RoleInsights["ShopperInsights"] = new RoleInsight
        {
            RoleName = "ShopperInsights",
            Summary = "Test insight",
            KeyFindings = ["Finding 1"],
            Confidence = 0.85
        };

        // Assert
        result.RoleInsights.Should().HaveCount(1);
        result.RoleInsights["ShopperInsights"].Summary.Should().Be("Test insight");
    }

    [Theory]
    [InlineData(DecisionStatus.Pending)]
    [InlineData(DecisionStatus.Running)]
    [InlineData(DecisionStatus.Completed)]
    [InlineData(DecisionStatus.Failed)]
    [InlineData(DecisionStatus.Cancelled)]
    public void DecisionResult_SupportsAllStatuses(DecisionStatus status)
    {
        // Act
        var result = CreateTestDecisionResult();
        result.Status = status;

        // Assert
        result.Status.Should().Be(status);
    }

    private static DecisionResult CreateTestDecisionResult()
    {
        return new DecisionResult
        {
            DecisionId = "test-id",
            Request = new DecisionRequest { DecisionText = "Test", Persona = RetailPersona.Grocery },
            PersonaContext = CreateTestPersonaContext(),
            Status = DecisionStatus.Pending
        };
    }

    private static PersonaContext CreateTestPersonaContext()
    {
        return new PersonaContext
        {
            Persona = RetailPersona.Grocery,
            DisplayName = "Grocery",
            Description = "Test",
            KeyCategories = ["Fresh"],
            Channels = ["Store"],
            BaselineKpis = new Dictionary<string, double> { ["Sales"] = 100 },
            SampleDecisionTemplates = ["Sample"],
            BaselineAssumptions = new Dictionary<string, string> { ["key"] = "value" }
        };
    }
}

public class DecisionEventTests
{
    [Fact]
    public void DecisionEvent_CreatesWithRequiredFields()
    {
        // Act
        var evt = new DecisionEvent
        {
            DecisionId = "decision-123",
            Persona = RetailPersona.Grocery,
            RoleName = "ShopperInsights",
            Phase = AnalysisPhase.Analyzing,
            Message = "Analyzing customer behavior..."
        };

        // Assert
        evt.DecisionId.Should().Be("decision-123");
        evt.Persona.Should().Be(RetailPersona.Grocery);
        evt.RoleName.Should().Be("ShopperInsights");
        evt.Phase.Should().Be(AnalysisPhase.Analyzing);
        evt.Message.Should().Be("Analyzing customer behavior...");
        evt.Timestamp.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void DecisionEvent_CanIncludeConfidence()
    {
        // Act
        var evt = new DecisionEvent
        {
            DecisionId = "decision-123",
            Persona = RetailPersona.Grocery,
            RoleName = "MarginImpact",
            Phase = AnalysisPhase.Completed,
            Message = "Analysis complete",
            Confidence = 0.92
        };

        // Assert
        evt.Confidence.Should().Be(0.92);
    }

    [Theory]
    [InlineData(AnalysisPhase.Starting)]
    [InlineData(AnalysisPhase.Analyzing)]
    [InlineData(AnalysisPhase.Reporting)]
    [InlineData(AnalysisPhase.Completed)]
    [InlineData(AnalysisPhase.Error)]
    public void DecisionEvent_SupportsAllPhases(AnalysisPhase phase)
    {
        // Act
        var evt = new DecisionEvent
        {
            DecisionId = "test",
            Persona = RetailPersona.Grocery,
            RoleName = "Test",
            Phase = phase,
            Message = "Test message"
        };

        // Assert
        evt.Phase.Should().Be(phase);
    }
}

public class ExecutiveRecommendationTests
{
    [Fact]
    public void ExecutiveRecommendation_CreatesWithAllFields()
    {
        // Act
        var recommendation = new ExecutiveRecommendation
        {
            Verdict = RecommendationVerdict.Approve,
            Summary = "Launch recommended with modifications",
            Rationale = ["Strong market demand", "Manageable risks"],
            RecommendedActions = ["Pilot program", "Monitor KPIs"],
            RisksToMonitor = ["Supply chain constraints", "Competitor response"],
            ProjectedKpis = new Dictionary<string, KpiProjection>
            {
                ["Revenue"] = new KpiProjection
                {
                    Name = "Revenue",
                    BaselineValue = 100000,
                    ProjectedLow = 110000,
                    ProjectedExpected = 120000,
                    ProjectedHigh = 130000,
                    Unit = "$"
                }
            },
            OverallConfidence = 0.85
        };

        // Assert
        recommendation.Verdict.Should().Be(RecommendationVerdict.Approve);
        recommendation.Summary.Should().Be("Launch recommended with modifications");
        recommendation.Rationale.Should().HaveCount(2);
        recommendation.RecommendedActions.Should().Contain("Pilot program");
        recommendation.RisksToMonitor.Should().HaveCount(2);
        recommendation.OverallConfidence.Should().Be(0.85);
    }

    [Theory]
    [InlineData(RecommendationVerdict.Approve)]
    [InlineData(RecommendationVerdict.ApproveWithModifications)]
    [InlineData(RecommendationVerdict.Decline)]
    public void ExecutiveRecommendation_SupportsAllVerdicts(RecommendationVerdict verdict)
    {
        // Act
        var recommendation = CreateTestRecommendation(verdict);

        // Assert
        recommendation.Verdict.Should().Be(verdict);
    }

    private static ExecutiveRecommendation CreateTestRecommendation(RecommendationVerdict verdict)
    {
        return new ExecutiveRecommendation
        {
            Verdict = verdict,
            Summary = "Test summary",
            Rationale = ["Reason 1"],
            RecommendedActions = ["Action 1"],
            RisksToMonitor = ["Risk 1"],
            ProjectedKpis = new Dictionary<string, KpiProjection>(),
            OverallConfidence = 0.75
        };
    }
}

public class PersonaContextTests
{
    [Fact]
    public void PersonaContext_CreatesWithAllFields()
    {
        // Act
        var context = new PersonaContext
        {
            Persona = RetailPersona.SpecialtyRetail,
            DisplayName = "Specialty Retail",
            Description = "High-end retail focused on customer experience",
            KeyCategories = ["Core Assortment", "Limited Editions"],
            Channels = ["Flagship Stores", "E-commerce"],
            BaselineKpis = new Dictionary<string, double> { ["CLV"] = 500 },
            SampleDecisionTemplates = ["Sample decision"],
            BaselineAssumptions = new Dictionary<string, string> { ["quality"] = "premium" }
        };

        // Assert
        context.Persona.Should().Be(RetailPersona.SpecialtyRetail);
        context.DisplayName.Should().Be("Specialty Retail");
        context.KeyCategories.Should().Contain("Core Assortment");
        context.Channels.Should().Contain("E-commerce");
        context.BaselineKpis.Should().ContainKey("CLV");
    }
}

public class RoleInsightTests
{
    [Fact]
    public void RoleInsight_CreatesWithRequiredFields()
    {
        // Act
        var insight = new RoleInsight
        {
            RoleName = "DemandForecasting",
            Summary = "Sales projected to increase by 15%",
            KeyFindings = ["Strong demand signal", "Seasonal boost expected"],
            Confidence = 0.88
        };

        // Assert
        insight.RoleName.Should().Be("DemandForecasting");
        insight.Summary.Should().Contain("15%");
        insight.KeyFindings.Should().HaveCount(2);
        insight.Confidence.Should().Be(0.88);
    }
}
