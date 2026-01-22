using System.Diagnostics;
using Azure.AI.OpenAI;
using Azure.Identity;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using OpenAI.Responses;

namespace RetailIntelligenceStudio.Agents.Infrastructure;

/// <summary>
/// Factory for creating AIAgent instances using Microsoft Agent Framework.
/// </summary>
public interface IAgentFactory
{
    /// <summary>
    /// Creates an AIAgent with the specified instructions and name.
    /// </summary>
    ChatClientAgent CreateAgent(string name, string instructions);
}

/// <summary>
/// Factory implementation using Azure OpenAI Responses API.
/// </summary>
public sealed class AzureOpenAIAgentFactory : IAgentFactory
{
    private static readonly ActivitySource ActivitySource = new("RetailIntelligenceStudio.Agents");
    private readonly ResponsesClient _responsesClient;
    private readonly ILoggerFactory _loggerFactory;
    private readonly ILogger<AzureOpenAIAgentFactory> _logger;
    private readonly string _deploymentName;

    public AzureOpenAIAgentFactory(
        string endpoint, 
        string deploymentName,
        ILoggerFactory loggerFactory)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(endpoint);
        ArgumentException.ThrowIfNullOrWhiteSpace(deploymentName);

        _deploymentName = deploymentName;
        _loggerFactory = loggerFactory;
        _logger = loggerFactory.CreateLogger<AzureOpenAIAgentFactory>();

        var client = new AzureOpenAIClient(
            new Uri(endpoint),
            new DefaultAzureCredential());

        _responsesClient = client.GetResponsesClient(deploymentName);
        
        _logger.LogInformation("Initialized Azure OpenAI agent factory with deployment: {DeploymentName}", deploymentName);
    }

    public ChatClientAgent CreateAgent(string name, string instructions)
    {
        using var activity = ActivitySource.StartActivity($"CreateAgent:{name}");
        activity?.SetTag("agent.name", name);
        activity?.SetTag("agent.type", "AzureOpenAI");
        activity?.SetTag("agent.deployment", _deploymentName);
        
        _logger.LogInformation("Creating Azure OpenAI agent: {AgentName} using deployment {Deployment}", name, _deploymentName);
        
        return _responsesClient.AsAIAgent(
            name: name,
            instructions: instructions,
            loggerFactory: _loggerFactory);
    }
}

/// <summary>
/// Local agent factory that creates agents using rule-based response generation
/// for development without Azure OpenAI.
/// </summary>
public sealed class LocalAgentFactory : IAgentFactory
{
    private static readonly ActivitySource ActivitySource = new("RetailIntelligenceStudio.Agents");
    private readonly ILoggerFactory _loggerFactory;
    private readonly ILogger<LocalAgentFactory> _logger;

    public LocalAgentFactory(ILoggerFactory loggerFactory)
    {
        _loggerFactory = loggerFactory;
        _logger = loggerFactory.CreateLogger<LocalAgentFactory>();
    }

    public ChatClientAgent CreateAgent(string name, string instructions)
    {
        using var activity = ActivitySource.StartActivity($"CreateAgent:{name}");
        activity?.SetTag("agent.name", name);
        activity?.SetTag("agent.type", "LocalModel");
        
        _logger.LogInformation("Creating local agent: {AgentName}", name);
        
        var chatClient = new LocalModelChatClient();
        return chatClient.AsAIAgent(
            name: name,
            instructions: instructions,
            loggerFactory: _loggerFactory);
    }
}
