using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.AI;

namespace RetailIntelligenceStudio.Agents.Infrastructure;

/// <summary>
/// Local chat client that generates intelligent responses for development
/// when Azure OpenAI is not configured. Uses rule-based response generation.
/// </summary>
public sealed class LocalModelChatClient : IChatClient
{
    private static readonly ActivitySource ActivitySource = new("RetailIntelligenceStudio.Agents");
    private readonly string _modelId;

    public LocalModelChatClient(string modelId = "local-retail-model")
    {
        _modelId = modelId;
    }

    /// <inheritdoc />
    public ChatOptions? DefaultOptions => null;

    /// <inheritdoc />
    public ChatClientMetadata Metadata => new(nameof(LocalModelChatClient), null, _modelId);

    /// <summary>
    /// Disposes resources. No-op as this mock client has no unmanaged resources.
    /// </summary>
    public void Dispose() 
    { 
        // No resources to dispose - this is a mock implementation
    }

    /// <inheritdoc />
    public async Task<ChatResponse> GetResponseAsync(
        IEnumerable<ChatMessage> messages,
        ChatOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        // Ensure async execution without blocking
        await Task.Yield();
        
        var lastMessage = messages.LastOrDefault()?.Text ?? "";
        var responseText = GenerateMockResponse(lastMessage, options?.Instructions);
        
        return new ChatResponse([new ChatMessage(ChatRole.Assistant, responseText)])
        {
            ResponseId = Guid.NewGuid().ToString(),
            ModelId = _modelId,
            FinishReason = ChatFinishReason.Stop
        };
    }

    /// <inheritdoc />
    public async IAsyncEnumerable<ChatResponseUpdate> GetStreamingResponseAsync(
        IEnumerable<ChatMessage> messages,
        ChatOptions? options = null,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var lastMessage = messages.LastOrDefault()?.Text ?? "";
        var responseText = GenerateMockResponse(lastMessage, options?.Instructions);
        
        // Split into chunks and yield with delays to simulate streaming
        var words = responseText.Split(' ');
        var chunkSize = 5;
        
        for (int i = 0; i < words.Length; i += chunkSize)
        {
            await Task.Delay(30 + Random.Shared.Next(50), cancellationToken).ConfigureAwait(false);
            
            var chunk = string.Join(' ', words.Skip(i).Take(chunkSize)) + " ";
            yield return new ChatResponseUpdate
            {
                Role = ChatRole.Assistant,
                Contents = [new TextContent(chunk)]
            };
        }
    }

    /// <inheritdoc />
    public TService? GetService<TService>(object? key = null) where TService : class
    {
        return this as TService;
    }

    public object? GetService(Type serviceType, object? key = null)
    {
        return serviceType.IsAssignableFrom(GetType()) ? this : null;
    }

    private static string GenerateMockResponse(string prompt, string? systemInstructions)
    {
        // Generate contextual mock responses based on the role (from system instructions)
        if (systemInstructions?.Contains("Decision Framer", StringComparison.OrdinalIgnoreCase) == true)
        {
            return """
                ## Decision Frame Analysis
                
                **Core Business Question:** The decision presents a strategic opportunity requiring structured analysis.
                
                **Proposed Action:** Implement a phased approach to evaluate market demand and operational readiness.
                
                **Scope:** 
                - Product categories impacted
                - Regional coverage considerations
                - Channel implications
                
                **Success Criteria:**
                - Revenue impact targets met
                - Customer satisfaction maintained
                - Operational efficiency preserved
                """;
        }
        
        if (systemInstructions?.Contains("Shopper Insight", StringComparison.OrdinalIgnoreCase) == true)
        {
            return """
                ## Shopper Insights Analysis
                
                **Customer Segment Impact:**
                - Core demographic shows 72% alignment with proposed changes
                - Price-sensitive segments may require targeted messaging
                
                **Behavioral Patterns:**
                - Shopping frequency correlates with promotional sensitivity
                - Cross-category purchasing indicates bundle opportunities
                
                **Recommendations:**
                - Focus initial rollout on high-affinity customer segments
                - Develop targeted communication strategy for price-sensitive groups
                """;
        }
        
        if (systemInstructions?.Contains("Demand Forecast", StringComparison.OrdinalIgnoreCase) == true)
        {
            return """
                ## Demand Forecasting Analysis
                
                **Projected Impact:**
                - Short-term: 15-20% demand lift in targeted categories
                - Medium-term: Stabilization around 12% above baseline
                
                **Confidence Intervals:**
                - Q1 forecast: High confidence (85%)
                - Q2-Q3 forecast: Moderate confidence (70%)
                
                **Risk Factors:**
                - Seasonal variations may amplify or dampen effects
                - Competitive response could shift baseline assumptions
                """;
        }
        
        if (systemInstructions?.Contains("Inventory", StringComparison.OrdinalIgnoreCase) == true)
        {
            return """
                ## Inventory Readiness Assessment
                
                **Current State:**
                - Stock levels adequate for projected 30-day demand
                - Safety stock thresholds within acceptable ranges
                
                **Supply Chain Considerations:**
                - Lead time optimization opportunities identified
                - Vendor capacity confirmed for volume increase scenarios
                
                **Recommendations:**
                - Pre-position inventory 2 weeks before initiative launch
                - Establish expedited replenishment protocols
                """;
        }
        
        if (systemInstructions?.Contains("Margin", StringComparison.OrdinalIgnoreCase) == true)
        {
            return """
                ## Margin Impact Analysis
                
                **Financial Projections:**
                - Gross margin impact: +2.3% to +4.1% range
                - Net contribution improvement expected
                
                **Cost Considerations:**
                - Implementation costs recoverable within 6 months
                - Operating margin preservation strategies identified
                
                **ROI Assessment:**
                - Projected payback period: 8-12 months
                - NPV positive under base case assumptions
                """;
        }
        
        if (systemInstructions?.Contains("Digital Merchandising", StringComparison.OrdinalIgnoreCase) == true)
        {
            return """
                ## Digital Merchandising Recommendations
                
                **Channel Optimization:**
                - Website prominence adjustments recommended
                - App experience enhancements identified
                
                **Content Strategy:**
                - Product storytelling opportunities
                - Cross-sell and upsell positioning
                
                **Personalization:**
                - Customer segment-specific messaging templates
                - Dynamic pricing considerations for digital channels
                """;
        }
        
        if (systemInstructions?.Contains("Risk", StringComparison.OrdinalIgnoreCase) == true ||
            systemInstructions?.Contains("Compliance", StringComparison.OrdinalIgnoreCase) == true)
        {
            return """
                ## Risk & Compliance Assessment
                
                **Risk Categories:**
                - Operational: Medium (manageable with controls)
                - Financial: Low to Medium
                - Reputational: Low
                
                **Compliance Considerations:**
                - Pricing regulations: Compliant
                - Consumer protection: Review recommended
                
                **Mitigation Strategies:**
                - Establish monitoring dashboards
                - Define escalation protocols
                - Schedule periodic compliance reviews
                """;
        }
        
        if (systemInstructions?.Contains("Executive", StringComparison.OrdinalIgnoreCase) == true ||
            systemInstructions?.Contains("Recommendation", StringComparison.OrdinalIgnoreCase) == true)
        {
            return """
                ## Executive Recommendation
                
                **Summary:** Based on comprehensive multi-perspective analysis, proceed with implementation.
                
                **Key Findings:**
                - Strong customer alignment supports initiative
                - Financial projections indicate positive ROI
                - Operational readiness confirmed
                - Risk profile acceptable with proper controls
                
                **Recommended Action:** Approve phased implementation with Q1 pilot launch.
                
                **Success Metrics:**
                - Revenue growth targets
                - Customer satisfaction scores
                - Operational efficiency indicators
                """;
        }
        
        // Generic response for unrecognized roles
        return """
            ## Analysis Complete
            
            Based on the evaluation of available data and context, the following insights emerge:
            
            - Key factors have been analyzed against strategic objectives
            - Multiple perspectives have been considered in forming recommendations
            - Risk factors have been identified and mitigation strategies proposed
            
            The analysis supports informed decision-making with appropriate confidence levels.
            Further detailed assessment is available upon request.
            """;
    }
}
