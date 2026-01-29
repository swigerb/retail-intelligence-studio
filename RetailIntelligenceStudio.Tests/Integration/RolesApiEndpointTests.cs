using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using RetailIntelligenceStudio.Agents.Abstractions;
using RetailIntelligenceStudio.Agents.Infrastructure;
using RetailIntelligenceStudio.Agents.Roles;

namespace RetailIntelligenceStudio.Tests.Integration;

/// <summary>
/// Integration tests for the /api/roles endpoint.
/// Tests the API returns proper role metadata for the enhanced UX.
/// </summary>
public class RolesApiEndpointTests
{
    private readonly Mock<IAgentFactory> _mockAgentFactory;
    private readonly IYamlRoleFactory _roleFactory;

    public RolesApiEndpointTests()
    {
        _mockAgentFactory = new Mock<IAgentFactory>();
        
        var testBasePath = GetAgentsBasePath();
        var definitionLoader = new AgentDefinitionLoader(testBasePath, Mock.Of<ILogger<AgentDefinitionLoader>>());
        var templateEngine = new PromptTemplateEngine();
        
        var loggerFactory = new Mock<ILoggerFactory>();
        loggerFactory.Setup(f => f.CreateLogger(It.IsAny<string>()))
            .Returns(Mock.Of<ILogger>());
        
        _roleFactory = new YamlRoleFactory(definitionLoader, _mockAgentFactory.Object, templateEngine, loggerFactory.Object);
    }

    private static string GetAgentsBasePath()
    {
        // YAML files are copied to test output directory
        return AppContext.BaseDirectory;
    }

    private IIntelligenceRole[] CreateAllRoles()
    {
        return _roleFactory.CreateAllRoles().ToArray();
    }

    /// <summary>
    /// Test DTO for deserializing the /api/roles response.
    /// </summary>
    private record RoleDto(
        string RoleName, 
        string DisplayName, 
        string Description, 
        string[] FocusAreas, 
        string OutputType, 
        int WorkflowOrder);

    [Fact]
    public void GetRoles_FromDI_ReturnsAllEightRoles()
    {
        // Arrange - Use the helper to create all roles
        var roles = CreateAllRoles();
        
        // Act & Assert
        roles.Should().HaveCount(8);
        roles.Select(r => r.RoleName).Should().BeEquivalentTo(new[]
        {
            "decision_framer",
            "shopper_insights",
            "demand_forecasting",
            "inventory_readiness",
            "margin_impact",
            "digital_merchandising",
            "risk_compliance",
            "executive_recommendation"
        });
    }

    [Fact]
    public void GetRoles_SortedByWorkflowOrder_ReturnsCorrectOrder()
    {
        // Arrange
        var roles = CreateAllRoles();

        // Act - simulate what the endpoint does
        var sortedRoles = roles
            .OrderBy(r => r.WorkflowOrder)
            .Select(r => new
            {
                RoleName = r.RoleName,
                DisplayName = r.DisplayName,
                Description = r.Description,
                FocusAreas = r.FocusAreas.ToArray(),
                OutputType = r.OutputType,
                WorkflowOrder = r.WorkflowOrder
            })
            .ToList();

        // Assert
        sortedRoles.Should().HaveCount(8);
        sortedRoles[0].RoleName.Should().Be("decision_framer");
        sortedRoles[1].RoleName.Should().Be("shopper_insights");
        sortedRoles[2].RoleName.Should().Be("demand_forecasting");
        sortedRoles[3].RoleName.Should().Be("inventory_readiness");
        sortedRoles[4].RoleName.Should().Be("margin_impact");
        sortedRoles[5].RoleName.Should().Be("digital_merchandising");
        sortedRoles[6].RoleName.Should().Be("risk_compliance");
        sortedRoles[7].RoleName.Should().Be("executive_recommendation");
    }

    [Fact]
    public void GetRoles_ResponseStructure_MatchesExpectedFormat()
    {
        // Arrange
        var role = _roleFactory.CreateRole("decision_framer");

        // Act - create the response object as the endpoint does
        var responseItem = new
        {
            RoleName = role.RoleName,
            DisplayName = role.DisplayName,
            Description = role.Description,
            FocusAreas = role.FocusAreas.ToArray(),
            OutputType = role.OutputType,
            WorkflowOrder = role.WorkflowOrder
        };

        // Assert - verify structure
        responseItem.RoleName.Should().Be("decision_framer");
        responseItem.DisplayName.Should().Be("Decision Framer");
        responseItem.Description.Should().NotBeNullOrEmpty();
        responseItem.FocusAreas.Should().HaveCount(4);
        responseItem.OutputType.Should().Be("Decision Brief");
        responseItem.WorkflowOrder.Should().Be(1);
    }

    [Fact]
    public void AllRoles_ProvideCompleteMetadata()
    {
        // Arrange
        var allRoles = CreateAllRoles();

        // Act & Assert
        foreach (var role in allRoles)
        {
            var responseItem = new
            {
                RoleName = role.RoleName,
                DisplayName = role.DisplayName,
                Description = role.Description,
                FocusAreas = role.FocusAreas.ToArray(),
                OutputType = role.OutputType,
                WorkflowOrder = role.WorkflowOrder
            };

            responseItem.RoleName.Should().NotBeNullOrWhiteSpace($"RoleName should be set for {role.DisplayName}");
            responseItem.DisplayName.Should().NotBeNullOrWhiteSpace($"DisplayName should be set");
            responseItem.Description.Should().NotBeNullOrWhiteSpace($"Description should be set for {role.DisplayName}");
            responseItem.FocusAreas.Should().NotBeEmpty($"FocusAreas should be set for {role.DisplayName}");
            responseItem.OutputType.Should().NotBeNullOrWhiteSpace($"OutputType should be set for {role.DisplayName}");
            responseItem.WorkflowOrder.Should().BePositive($"WorkflowOrder should be positive for {role.DisplayName}");
        }
    }

    [Fact]
    public void RoleNames_FollowSnakeCaseConvention()
    {
        var allRoles = CreateAllRoles();

        foreach (var role in allRoles)
        {
            // RoleName should be snake_case (lowercase with underscores)
            role.RoleName.Should().MatchRegex(@"^[a-z]+(_[a-z]+)*$", 
                $"RoleName '{role.RoleName}' should be snake_case");
        }
    }

    [Theory]
    [InlineData("decision_framer", "Decision Framer", 1)]
    [InlineData("shopper_insights", "Shopper Insights", 2)]
    [InlineData("demand_forecasting", "Demand Forecasting", 3)]
    [InlineData("inventory_readiness", "Inventory Readiness", 4)]
    [InlineData("margin_impact", "Margin Impact", 5)]
    [InlineData("digital_merchandising", "Digital Merchandising", 6)]
    [InlineData("risk_compliance", "Risk & Compliance", 7)]
    [InlineData("executive_recommendation", "Executive Recommendation", 8)]
    public void Role_HasExpectedIdentityAndOrder(string expectedRoleName, string expectedDisplayName, int expectedOrder)
    {
        var allRoles = CreateAllRoles();

        var role = allRoles.Single(r => r.RoleName == expectedRoleName);

        role.DisplayName.Should().Be(expectedDisplayName);
        role.WorkflowOrder.Should().Be(expectedOrder);
    }
}
