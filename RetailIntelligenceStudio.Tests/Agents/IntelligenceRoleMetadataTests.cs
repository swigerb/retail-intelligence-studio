using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using RetailIntelligenceStudio.Agents.Abstractions;
using RetailIntelligenceStudio.Agents.Infrastructure;
using RetailIntelligenceStudio.Agents.Roles;

namespace RetailIntelligenceStudio.Tests.Agents;

/// <summary>
/// Tests for Intelligence Role metadata properties (FocusAreas, OutputType, WorkflowOrder).
/// Ensures all roles provide proper metadata for the enhanced UX.
/// </summary>
public class IntelligenceRoleMetadataTests
{
    private readonly Mock<IAgentFactory> _mockAgentFactory;
    private readonly List<IIntelligenceRole> _allRoles;

    public IntelligenceRoleMetadataTests()
    {
        _mockAgentFactory = new Mock<IAgentFactory>();
        
        // Create all 8 intelligence roles with mocked dependencies
        _allRoles = new List<IIntelligenceRole>
        {
            CreateRole<DecisionFramerRole>(),
            CreateRole<ShopperInsightsRole>(),
            CreateRole<DemandForecastingRole>(),
            CreateRole<InventoryReadinessRole>(),
            CreateRole<MarginImpactRole>(),
            CreateRole<DigitalMerchandisingRole>(),
            CreateRole<RiskComplianceRole>(),
            CreateRole<ExecutiveRecommendationRole>()
        };
    }

    private T CreateRole<T>() where T : class, IIntelligenceRole
    {
        var loggerMock = new Mock<ILogger<T>>();
        return (T)Activator.CreateInstance(typeof(T), _mockAgentFactory.Object, loggerMock.Object)!;
    }

    [Fact]
    public void AllRoles_HaveFocusAreas()
    {
        foreach (var role in _allRoles)
        {
            role.FocusAreas.Should().NotBeNull($"{role.DisplayName} should have FocusAreas");
            role.FocusAreas.Should().NotBeEmpty($"{role.DisplayName} should have at least one focus area");
        }
    }

    [Fact]
    public void AllRoles_HaveOutputType()
    {
        foreach (var role in _allRoles)
        {
            role.OutputType.Should().NotBeNullOrWhiteSpace($"{role.DisplayName} should have an OutputType");
        }
    }

    [Fact]
    public void AllRoles_HaveWorkflowOrder()
    {
        foreach (var role in _allRoles)
        {
            role.WorkflowOrder.Should().BeGreaterThan(0, $"{role.DisplayName} should have a positive WorkflowOrder");
            role.WorkflowOrder.Should().BeLessThanOrEqualTo(8, $"{role.DisplayName} should have WorkflowOrder <= 8");
        }
    }

    [Fact]
    public void AllRoles_HaveUniqueWorkflowOrders()
    {
        var orders = _allRoles.Select(r => r.WorkflowOrder).ToList();
        orders.Should().OnlyHaveUniqueItems("each role should have a unique WorkflowOrder");
    }

    [Fact]
    public void AllRoles_WorkflowOrdersAreConsecutive()
    {
        var orders = _allRoles.Select(r => r.WorkflowOrder).OrderBy(o => o).ToList();
        orders.Should().BeEquivalentTo(new[] { 1, 2, 3, 4, 5, 6, 7, 8 });
    }

    [Fact]
    public void DecisionFramer_HasCorrectMetadata()
    {
        var role = CreateRole<DecisionFramerRole>();

        role.WorkflowOrder.Should().Be(1);
        role.OutputType.Should().Be("Decision Brief");
        role.FocusAreas.Should().Contain("Business Question");
        role.FocusAreas.Should().Contain("Success Criteria");
        role.FocusAreas.Should().Contain("Scope Definition");
        role.FocusAreas.Should().Contain("Key Assumptions");
    }

    [Fact]
    public void ShopperInsights_HasCorrectMetadata()
    {
        var role = CreateRole<ShopperInsightsRole>();

        role.WorkflowOrder.Should().Be(2);
        role.OutputType.Should().Be("Behavioral Analysis");
        role.FocusAreas.Should().Contain("Customer Segments");
        role.FocusAreas.Should().Contain("Price Sensitivity");
        role.FocusAreas.Should().Contain("Loyalty Impact");
        role.FocusAreas.Should().Contain("Basket Composition");
    }

    [Fact]
    public void DemandForecasting_HasCorrectMetadata()
    {
        var role = CreateRole<DemandForecastingRole>();

        role.WorkflowOrder.Should().Be(3);
        role.OutputType.Should().Be("Volume Forecast");
        role.FocusAreas.Should().NotBeEmpty();
    }

    [Fact]
    public void InventoryReadiness_HasCorrectMetadata()
    {
        var role = CreateRole<InventoryReadinessRole>();

        role.WorkflowOrder.Should().Be(4);
        role.OutputType.Should().Be("Supply Assessment");
        role.FocusAreas.Should().NotBeEmpty();
    }

    [Fact]
    public void MarginImpact_HasCorrectMetadata()
    {
        var role = CreateRole<MarginImpactRole>();

        role.WorkflowOrder.Should().Be(5);
        role.OutputType.Should().Be("Financial Analysis");
        role.FocusAreas.Should().NotBeEmpty();
    }

    [Fact]
    public void DigitalMerchandising_HasCorrectMetadata()
    {
        var role = CreateRole<DigitalMerchandisingRole>();

        role.WorkflowOrder.Should().Be(6);
        role.OutputType.Should().Be("Execution Plan");
        role.FocusAreas.Should().NotBeEmpty();
    }

    [Fact]
    public void RiskCompliance_HasCorrectMetadata()
    {
        var role = CreateRole<RiskComplianceRole>();

        role.WorkflowOrder.Should().Be(7);
        role.OutputType.Should().Be("Risk Assessment");
        role.FocusAreas.Should().NotBeEmpty();
    }

    [Fact]
    public void ExecutiveRecommendation_HasCorrectMetadata()
    {
        var role = CreateRole<ExecutiveRecommendationRole>();

        role.WorkflowOrder.Should().Be(8);
        role.OutputType.Should().Be("Final Recommendation");
        role.FocusAreas.Should().NotBeEmpty();
    }

    [Fact]
    public void AllRoles_HaveExactlyFourFocusAreas()
    {
        foreach (var role in _allRoles)
        {
            role.FocusAreas.Should().HaveCount(4, 
                $"{role.DisplayName} should have exactly 4 focus areas");
        }
    }

    [Fact]
    public void AllRoles_FocusAreasAreNotEmpty()
    {
        foreach (var role in _allRoles)
        {
            foreach (var focusArea in role.FocusAreas)
            {
                focusArea.Should().NotBeNullOrWhiteSpace(
                    $"{role.DisplayName} has an empty focus area");
            }
        }
    }

    [Fact]
    public void RolesSortedByWorkflowOrder_MatchesExpectedSequence()
    {
        var sortedRoles = _allRoles.OrderBy(r => r.WorkflowOrder).ToList();

        sortedRoles[0].Should().BeOfType<DecisionFramerRole>();
        sortedRoles[1].Should().BeOfType<ShopperInsightsRole>();
        sortedRoles[2].Should().BeOfType<DemandForecastingRole>();
        sortedRoles[3].Should().BeOfType<InventoryReadinessRole>();
        sortedRoles[4].Should().BeOfType<MarginImpactRole>();
        sortedRoles[5].Should().BeOfType<DigitalMerchandisingRole>();
        sortedRoles[6].Should().BeOfType<RiskComplianceRole>();
        sortedRoles[7].Should().BeOfType<ExecutiveRecommendationRole>();
    }
}
