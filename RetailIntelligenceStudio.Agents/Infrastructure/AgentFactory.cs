using System.ClientModel;
using System.Diagnostics;
using Azure.AI.OpenAI;
using Azure.Identity;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;

namespace RetailIntelligenceStudio.Agents.Infrastructure;

/// <summary>
/// Configuration options for Azure OpenAI.
/// </summary>
public sealed class AzureOpenAIOptions
{
    public const string SectionName = "AzureOpenAI";
    
    /// <summary>
    /// Azure OpenAI endpoint URL (e.g., https://your-resource.openai.azure.com/).
    /// </summary>
    public string Endpoint { get; set; } = string.Empty;
    
    /// <summary>
    /// The model deployment name (e.g., gpt-4o).
    /// </summary>
    public string DeploymentName { get; set; } = "gpt-4o";
    
    /// <summary>
    /// Optional API key for authentication. If not provided, DefaultAzureCredential is used.
    /// </summary>
    public string? ApiKey { get; set; }
    
    /// <summary>
    /// Validates the configuration and returns any error messages.
    /// </summary>
    public IReadOnlyList<string> Validate()
    {
        var errors = new List<string>();
        
        if (string.IsNullOrWhiteSpace(Endpoint))
        {
            errors.Add("AzureOpenAI:Endpoint is required. Set it in appsettings.json or via environment variable AzureOpenAI__Endpoint.");
        }
        else if (Endpoint.Contains("your-resource", StringComparison.OrdinalIgnoreCase))
        {
            errors.Add("AzureOpenAI:Endpoint contains placeholder value. Please configure your actual Azure OpenAI endpoint.");
        }
        else if (!Uri.TryCreate(Endpoint, UriKind.Absolute, out var uri) || 
                 (uri.Scheme != "https" && uri.Scheme != "http"))
        {
            errors.Add($"AzureOpenAI:Endpoint '{Endpoint}' is not a valid URL.");
        }
        
        if (string.IsNullOrWhiteSpace(DeploymentName))
        {
            errors.Add("AzureOpenAI:DeploymentName is required.");
        }
        
        return errors;
    }
    
    /// <summary>
    /// Returns true if API key authentication should be used.
    /// </summary>
    public bool UseApiKeyAuth => !string.IsNullOrWhiteSpace(ApiKey);
}

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
/// Factory implementation using Azure OpenAI Chat Completions API.
/// Supports both API key and DefaultAzureCredential (Entra ID) authentication.
/// </summary>
public sealed class AzureOpenAIAgentFactory : IAgentFactory
{
    private static readonly ActivitySource ActivitySource = new("RetailIntelligenceStudio.Agents");
    private readonly IChatClient _chatClient;
    private readonly ILoggerFactory _loggerFactory;
    private readonly ILogger<AzureOpenAIAgentFactory> _logger;
    private readonly string _deploymentName;

    /// <summary>
    /// Creates a new Azure OpenAI agent factory with the specified options.
    /// </summary>
    /// <param name="options">Azure OpenAI configuration options.</param>
    /// <param name="loggerFactory">Logger factory for creating loggers.</param>
    /// <exception cref="InvalidOperationException">Thrown when configuration is invalid.</exception>
    public AzureOpenAIAgentFactory(
        AzureOpenAIOptions options,
        ILoggerFactory loggerFactory)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(loggerFactory);

        _loggerFactory = loggerFactory;
        _logger = loggerFactory.CreateLogger<AzureOpenAIAgentFactory>();
        
        // Validate configuration
        var errors = options.Validate();
        if (errors.Count > 0)
        {
            var errorMessage = string.Join(Environment.NewLine, errors);
            _logger.LogError("❌ Azure OpenAI configuration is invalid:\n{Errors}", errorMessage);
            throw new InvalidOperationException(
                $"Azure OpenAI configuration is invalid:\n{errorMessage}\n\n" +
                "To fix this, either:\n" +
                "1. Configure appsettings.json or appsettings.Development.json with your Azure OpenAI settings\n" +
                "2. Set environment variables: AzureOpenAI__Endpoint, AzureOpenAI__DeploymentName, and optionally AzureOpenAI__ApiKey\n" +
                "3. If using DefaultAzureCredential (no API key), ensure you've run 'az login' and have the 'Cognitive Services OpenAI User' role");
        }
        
        _deploymentName = options.DeploymentName;

        AzureOpenAIClient client;
        
        if (options.UseApiKeyAuth)
        {
            _logger.LogInformation(
                "🔑 Initializing Azure OpenAI with API key authentication. Endpoint: {Endpoint}, Deployment: {DeploymentName}",
                options.Endpoint, _deploymentName);
            
            client = new AzureOpenAIClient(
                new Uri(options.Endpoint),
                new ApiKeyCredential(options.ApiKey!));
        }
        else
        {
            _logger.LogInformation(
                "🔐 Initializing Azure OpenAI with DefaultAzureCredential (Entra ID). Endpoint: {Endpoint}, Deployment: {DeploymentName}",
                options.Endpoint, _deploymentName);
            _logger.LogInformation(
                "💡 If authentication fails, ensure you've run 'az login' and have the 'Cognitive Services OpenAI User' role on the Azure OpenAI resource.");
            
            try
            {
                client = new AzureOpenAIClient(
                    new Uri(options.Endpoint),
                    new DefaultAzureCredential());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, 
                    "❌ Failed to initialize DefaultAzureCredential. " +
                    "Ensure you're logged into Azure CLI ('az login') or provide an API key via AzureOpenAI:ApiKey configuration.");
                throw new InvalidOperationException(
                    "Failed to initialize Azure authentication. " +
                    "Either run 'az login' to authenticate with Azure CLI, or provide an API key in configuration (AzureOpenAI:ApiKey).", ex);
            }
        }

        // Use Chat Completions API (supported by Azure OpenAI)
        // NOT Responses API (OpenAI-only feature)
        _chatClient = client.GetChatClient(_deploymentName).AsIChatClient();
        
        _logger.LogInformation("✅ Azure OpenAI agent factory initialized successfully");
    }

    /// <summary>
    /// Legacy constructor for backward compatibility.
    /// </summary>
    [Obsolete("Use the constructor with AzureOpenAIOptions instead.")]
    public AzureOpenAIAgentFactory(
        string endpoint, 
        string deploymentName,
        ILoggerFactory loggerFactory)
        : this(new AzureOpenAIOptions { Endpoint = endpoint, DeploymentName = deploymentName }, loggerFactory)
    {
    }

    public ChatClientAgent CreateAgent(string name, string instructions)
    {
        using var activity = ActivitySource.StartActivity($"CreateAgent:{name}");
        activity?.SetTag("agent.name", name);
        activity?.SetTag("agent.type", "AzureOpenAI");
        activity?.SetTag("agent.deployment", _deploymentName);
        
        _logger.LogInformation("Creating Azure OpenAI agent: {AgentName} using deployment {Deployment}", name, _deploymentName);
        
        return _chatClient.AsAIAgent(
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
