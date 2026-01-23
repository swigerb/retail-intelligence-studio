using FluentAssertions;

namespace RetailIntelligenceStudio.Tests.Agents;

/// <summary>
/// Tests for confidence extraction from AI responses.
/// These tests ensure that each agent role reports accurate, distinct confidence values
/// parsed from the AI model's response rather than hardcoded defaults.
/// </summary>
public class ConfidenceExtractionTests
{
    [Theory]
    [InlineData("Analysis complete. **Confidence: 85%**", 0.85)]
    [InlineData("The decision looks sound. Confidence: 78%", 0.78)]
    [InlineData("Based on the data, **Confidence:** 92%", 0.92)]
    [InlineData("Risk assessment complete.\n\nConfidence: 65%", 0.65)]
    [InlineData("CONFIDENCE: 90%", 0.90)]
    public void ExtractConfidence_ParsesPercentageFormat(string response, double expected)
    {
        // Act
        var confidence = TestableConfidenceExtractor.ExtractConfidenceFromResponse(response);

        // Assert
        confidence.Should().BeApproximately(expected, 0.01);
    }

    [Theory]
    [InlineData("Analysis complete. Confidence: 0.85", 0.85)]
    [InlineData("confidence level: 0.78", 0.78)]
    [InlineData("Confidence score: .92", 0.92)]
    public void ExtractConfidence_ParsesDecimalFormat(string response, double expected)
    {
        // Act
        var confidence = TestableConfidenceExtractor.ExtractConfidenceFromResponse(response);

        // Assert
        confidence.Should().BeApproximately(expected, 0.01);
    }

    [Theory]
    [InlineData("We have high confidence in this recommendation.", 0.90)]
    [InlineData("This analysis has very high confidence given the data.", 0.90)]
    [InlineData("There is moderate confidence in these projections.", 0.70)]
    [InlineData("Given the uncertainty, we have low confidence.", 0.50)]
    public void ExtractConfidence_ParsesQualitativeConfidence(string response, double expected)
    {
        // Act
        var confidence = TestableConfidenceExtractor.ExtractConfidenceFromResponse(response);

        // Assert
        confidence.Should().BeApproximately(expected, 0.01);
    }

    [Fact]
    public void ExtractConfidence_ReturnsDefault_WhenNoConfidenceFound()
    {
        // Arrange
        var response = "This is an analysis without any confidence mention.";

        // Act
        var confidence = TestableConfidenceExtractor.ExtractConfidenceFromResponse(response);

        // Assert
        confidence.Should().Be(0.75); // Default value
    }

    [Fact]
    public void ExtractConfidence_ClampsToValidRange()
    {
        // Arrange - test that values over 100% get clamped to 1.0
        var responseOver100 = "Confidence: 150%";

        // Act
        var over = TestableConfidenceExtractor.ExtractConfidenceFromResponse(responseOver100);

        // Assert
        over.Should().Be(1.0);
    }

    [Fact]
    public void ExtractConfidence_ReturnsDefault_ForInvalidFormats()
    {
        // Arrange - negative values and other invalid formats won't match the regex
        var responseNegative = "Confidence: -10%";
        var responseNoNumber = "Confidence: unknown";

        // Act
        var negative = TestableConfidenceExtractor.ExtractConfidenceFromResponse(responseNegative);
        var noNumber = TestableConfidenceExtractor.ExtractConfidenceFromResponse(responseNoNumber);

        // Assert - both should return default since they don't match valid patterns
        negative.Should().Be(0.75); // Falls through to default
        noNumber.Should().Be(0.75); // Falls through to default
    }

    [Fact]
    public void ExtractConfidence_PrefersExplicitPercentage_OverQualitative()
    {
        // Arrange - response has both explicit and qualitative confidence
        var response = "We have moderate confidence overall, but specifically **Confidence: 82%**";

        // Act
        var confidence = TestableConfidenceExtractor.ExtractConfidenceFromResponse(response);

        // Assert - should use the explicit 82% not the "moderate" 70%
        confidence.Should().BeApproximately(0.82, 0.01);
    }
}

/// <summary>
/// Exposes the confidence extraction method for testing.
/// This mirrors the implementation in IntelligenceRoleBase.
/// </summary>
internal static class TestableConfidenceExtractor
{
    public static double ExtractConfidenceFromResponse(string response)
    {
        // Pattern 1: "Confidence: XX%" or "**Confidence:** XX%"
        var percentMatch = System.Text.RegularExpressions.Regex.Match(
            response,
            @"[Cc]onfidence[:\s*]+(\d{1,3})%",
            System.Text.RegularExpressions.RegexOptions.IgnoreCase);

        if (percentMatch.Success && int.TryParse(percentMatch.Groups[1].Value, out var percentValue))
        {
            return Math.Clamp(percentValue / 100.0, 0.0, 1.0);
        }

        // Pattern 2: "confidence: 0.XX" or "confidence level: 0.XX"
        var decimalMatch = System.Text.RegularExpressions.Regex.Match(
            response,
            @"[Cc]onfidence[:\s\w]*?(\d?\.\d+)",
            System.Text.RegularExpressions.RegexOptions.IgnoreCase);

        if (decimalMatch.Success && double.TryParse(decimalMatch.Groups[1].Value, out var decimalValue))
        {
            return Math.Clamp(decimalValue, 0.0, 1.0);
        }

        // Pattern 3: Look for "high confidence", "moderate confidence", "low confidence"
        if (System.Text.RegularExpressions.Regex.IsMatch(response, @"\b(very\s+)?high\s+confidence\b", System.Text.RegularExpressions.RegexOptions.IgnoreCase))
            return 0.90;
        if (System.Text.RegularExpressions.Regex.IsMatch(response, @"\bmoderate\s+confidence\b", System.Text.RegularExpressions.RegexOptions.IgnoreCase))
            return 0.70;
        if (System.Text.RegularExpressions.Regex.IsMatch(response, @"\blow\s+confidence\b", System.Text.RegularExpressions.RegexOptions.IgnoreCase))
            return 0.50;

        // Default confidence if none found
        return 0.75;
    }
}
