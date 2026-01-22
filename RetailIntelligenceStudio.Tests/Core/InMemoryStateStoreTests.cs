using FluentAssertions;
using RetailIntelligenceStudio.Core.Models;
using RetailIntelligenceStudio.Core.Stores;

namespace RetailIntelligenceStudio.Tests.Core;

public class InMemoryStateStoreTests
{
    private readonly InMemoryStateStore _sut;

    public InMemoryStateStoreTests()
    {
        _sut = new InMemoryStateStore();
    }

    [Fact]
    public async Task SaveDecisionAsync_StoresDecision()
    {
        // Arrange
        var decision = CreateTestDecision("test-1");

        // Act
        await _sut.SaveDecisionAsync(decision);
        var retrieved = await _sut.GetDecisionAsync("test-1");

        // Assert
        retrieved.Should().NotBeNull();
        retrieved!.DecisionId.Should().Be("test-1");
    }

    [Fact]
    public async Task GetDecisionAsync_ReturnsNullForNonExistentDecision()
    {
        // Act
        var result = await _sut.GetDecisionAsync("non-existent");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task UpdateDecisionAsync_UpdatesExistingDecision()
    {
        // Arrange
        var decision = CreateTestDecision("test-update");
        await _sut.SaveDecisionAsync(decision);
        
        decision.Status = DecisionStatus.Completed;

        // Act
        await _sut.UpdateDecisionAsync(decision);
        var retrieved = await _sut.GetDecisionAsync("test-update");

        // Assert
        retrieved!.Status.Should().Be(DecisionStatus.Completed);
    }

    [Fact]
    public async Task ListDecisionsAsync_ReturnsDecisionsOrderedByDate()
    {
        // Arrange
        await _sut.SaveDecisionAsync(CreateTestDecision("test-a"));
        await Task.Delay(10); // Small delay for ordering
        await _sut.SaveDecisionAsync(CreateTestDecision("test-b"));
        await Task.Delay(10);
        await _sut.SaveDecisionAsync(CreateTestDecision("test-c"));

        // Act
        var decisions = await _sut.ListDecisionsAsync(0, 10);

        // Assert
        decisions.Should().HaveCount(3);
        // Most recent first
        decisions[0].DecisionId.Should().Be("test-c");
    }

    [Fact]
    public async Task DeleteDecisionAsync_RemovesDecision()
    {
        // Arrange
        var decision = CreateTestDecision("test-delete");
        await _sut.SaveDecisionAsync(decision);

        // Act
        await _sut.DeleteDecisionAsync("test-delete");
        var retrieved = await _sut.GetDecisionAsync("test-delete");

        // Assert
        retrieved.Should().BeNull();
    }

    [Fact]
    public async Task ListDecisionsAsync_RespectsPagination()
    {
        // Arrange
        for (int i = 0; i < 10; i++)
        {
            await _sut.SaveDecisionAsync(CreateTestDecision($"test-{i}"));
        }

        // Act
        var page1 = await _sut.ListDecisionsAsync(0, 3);
        var page2 = await _sut.ListDecisionsAsync(3, 3);

        // Assert
        page1.Should().HaveCount(3);
        page2.Should().HaveCount(3);
        page1.Select(d => d.DecisionId).Should().NotIntersectWith(page2.Select(d => d.DecisionId));
    }

    private static DecisionResult CreateTestDecision(string decisionId)
    {
        return new DecisionResult
        {
            DecisionId = decisionId,
            Request = new DecisionRequest
            {
                DecisionText = "Test decision question",
                Persona = RetailPersona.Grocery
            },
            PersonaContext = CreateTestPersonaContext(),
            Status = DecisionStatus.Pending
        };
    }

    private static PersonaContext CreateTestPersonaContext()
    {
        return new PersonaContext
        {
            Persona = RetailPersona.Grocery,
            DisplayName = "Grocery Retail",
            Description = "Test description",
            KeyCategories = ["Fresh Produce", "Dairy"],
            Channels = ["In-Store", "Online"],
            BaselineKpis = new Dictionary<string, double> { ["Sales"] = 100000 },
            SampleDecisionTemplates = ["Sample decision 1"],
            BaselineAssumptions = new Dictionary<string, string> { ["assumption"] = "value" }
        };
    }
}
