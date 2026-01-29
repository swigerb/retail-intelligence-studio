using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using RetailIntelligenceStudio.Agents.Abstractions;
using RetailIntelligenceStudio.Agents.Infrastructure;
using RetailIntelligenceStudio.Agents.Roles;

namespace RetailIntelligenceStudio.Tests.Agents;

/// <summary>
/// Tests for Intelligence Role metadata properties (FocusAreas, OutputType, WorkflowOrder).
/// Ensures all YAML-driven roles provide proper metadata for the enhanced UX.
/// </summary>
public class IntelligenceRoleMetadataTests
{
    private readonly Mock<IAgentFactory> _mockAgentFactory;
    private readonly IPromptTemplateEngine _templateEngine;
    private readonly IAgentDefinitionLoader _definitionLoader;
    private readonly IYamlRoleFactory _roleFactory;
    private readonly List<IIntelligenceRole> _allRoles;

    public IntelligenceRoleMetadataTests()
    {
        _mockAgentFactory = new Mock<IAgentFactory>();
        _templateEngine = new PromptTemplateEngine();
        
        // Use the test directory that contains the YAML files
        var testBasePath = GetAgentsBasePath();
        _definitionLoader = new AgentDefinitionLoader(testBasePath, Mock.Of<ILogger<AgentDefinitionLoader>>());
        
        var loggerFactory = new Mock<ILoggerFactory>();
        loggerFactory.Setup(f => f.CreateLogger(It.IsAny<string>()))
            .Returns(Mock.Of<ILogger>());
        
        _roleFactory = new YamlRoleFactory(_definitionLoader, _mockAgentFactory.Object, _templateEngine, loggerFactory.Object);
        _allRoles = _roleFactory.CreateAllRoles().ToList();
    }

    private static string GetAgentsBasePath()
    {
        // YAML files are copied to test output directory
        return AppContext.BaseDirectory;
    }

    private IIntelligenceRole GetRole(string roleName)
    {
        return _roleFactory.CreateRole(roleName);
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
        var role = GetRole("decision_framer");

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
        var role = GetRole("shopper_insights");

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
        var role = GetRole("demand_forecasting");

        role.WorkflowOrder.Should().Be(3);
        role.OutputType.Should().Be("Volume Forecast");
        role.FocusAreas.Should().NotBeEmpty();
    }

    [Fact]
    public void InventoryReadiness_HasCorrectMetadata()
    {
        var role = GetRole("inventory_readiness");

        role.WorkflowOrder.Should().Be(4);
        role.OutputType.Should().Be("Supply Assessment");
        role.FocusAreas.Should().NotBeEmpty();
    }

    [Fact]
    public void MarginImpact_HasCorrectMetadata()
    {
        var role = GetRole("margin_impact");

        role.WorkflowOrder.Should().Be(5);
        role.OutputType.Should().Be("Financial Analysis");
        role.FocusAreas.Should().NotBeEmpty();
    }

    [Fact]
    public void DigitalMerchandising_HasCorrectMetadata()
    {
        var role = GetRole("digital_merchandising");

        role.WorkflowOrder.Should().Be(6);
        role.OutputType.Should().Be("Execution Plan");
        role.FocusAreas.Should().NotBeEmpty();
    }

    [Fact]
    public void RiskCompliance_HasCorrectMetadata()
    {
        var role = GetRole("risk_compliance");

        role.WorkflowOrder.Should().Be(7);
        role.OutputType.Should().Be("Risk Assessment");
        role.FocusAreas.Should().NotBeEmpty();
    }

    [Fact]
    public void ExecutiveRecommendation_HasCorrectMetadata()
    {
        var role = GetRole("executive_recommendation");

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

        sortedRoles[0].RoleName.Should().Be("decision_framer");
        sortedRoles[1].RoleName.Should().Be("shopper_insights");
        sortedRoles[2].RoleName.Should().Be("demand_forecasting");
        sortedRoles[3].RoleName.Should().Be("inventory_readiness");
        sortedRoles[4].RoleName.Should().Be("margin_impact");
        sortedRoles[5].RoleName.Should().Be("digital_merchandising");
        sortedRoles[6].RoleName.Should().Be("risk_compliance");
        sortedRoles[7].RoleName.Should().Be("executive_recommendation");
    }
}
