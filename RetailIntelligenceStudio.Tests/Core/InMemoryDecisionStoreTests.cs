using FluentAssertions;
using RetailIntelligenceStudio.Core.Models;
using RetailIntelligenceStudio.Core.Stores;

namespace RetailIntelligenceStudio.Tests.Core;

public class InMemoryDecisionStoreTests
{
    private readonly InMemoryDecisionStore _sut;

    public InMemoryDecisionStoreTests()
    {
        _sut = new InMemoryDecisionStore();
    }

    [Fact]
    public async Task AppendEventAsync_StoresEvent()
    {
        // Arrange
        var decisionEvent = CreateTestEvent("test-decision-1");

        // Act
        await _sut.AppendEventAsync(decisionEvent);
        var events = await _sut.GetEventsAsync("test-decision-1");

        // Assert
        events.Should().ContainSingle();
        events[0].Message.Should().Be(decisionEvent.Message);
    }

    [Fact]
    public async Task GetEventsAsync_ReturnsEmptyForNonExistentDecision()
    {
        // Act
        var events = await _sut.GetEventsAsync("non-existent");

        // Assert
        events.Should().BeEmpty();
    }

    [Fact]
    public async Task AppendEventAsync_AppendsMultipleEvents()
    {
        // Arrange
        var decisionId = "test-decision-2";
        var event1 = CreateTestEvent(decisionId, "Message 1");
        var event2 = CreateTestEvent(decisionId, "Message 2");
        var event3 = CreateTestEvent(decisionId, "Message 3");

        // Act
        await _sut.AppendEventAsync(event1);
        await _sut.AppendEventAsync(event2);
        await _sut.AppendEventAsync(event3);
        var events = await _sut.GetEventsAsync(decisionId);

        // Assert
        events.Should().HaveCount(3);
        events.Select(e => e.Message).Should().ContainInOrder("Message 1", "Message 2", "Message 3");
    }

    [Fact]
    public async Task StreamEventsAsync_YieldsExistingEvents()
    {
        // Arrange
        var decisionId = "test-stream-1";
        await _sut.AppendEventAsync(CreateTestEvent(decisionId, "Existing event"));
        await _sut.CompleteAsync(decisionId); // Mark complete to end the stream

        // Act
        var events = new List<DecisionEvent>();
        await foreach (var evt in _sut.StreamEventsAsync(decisionId))
        {
            events.Add(evt);
        }

        // Assert
        events.Should().ContainSingle();
        events[0].Message.Should().Be("Existing event");
    }

    [Fact]
    public async Task CompleteAsync_MarksDecisionAsComplete()
    {
        // Arrange
        var decisionId = "test-complete";
        await _sut.AppendEventAsync(CreateTestEvent(decisionId));

        // Act
        await _sut.CompleteAsync(decisionId);
        var isComplete = await _sut.IsCompleteAsync(decisionId);

        // Assert
        isComplete.Should().BeTrue();
    }

    [Fact]
    public async Task IsCompleteAsync_ReturnsFalseForNewDecision()
    {
        // Act
        var isComplete = await _sut.IsCompleteAsync("new-decision");

        // Assert
        isComplete.Should().BeFalse();
    }

    private static DecisionEvent CreateTestEvent(string decisionId, string message = "Test message")
    {
        return new DecisionEvent
        {
            DecisionId = decisionId,
            Persona = RetailPersona.Grocery,
            RoleName = "TestRole",
            Phase = AnalysisPhase.Analyzing,
            Message = message
        };
    }
}
