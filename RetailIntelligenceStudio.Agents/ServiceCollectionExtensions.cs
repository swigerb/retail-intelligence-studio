using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RetailIntelligenceStudio.Agents.Abstractions;
using RetailIntelligenceStudio.Agents.Infrastructure;
using RetailIntelligenceStudio.Agents.Orchestration;
using RetailIntelligenceStudio.Agents.Roles;

namespace RetailIntelligenceStudio.Agents;

/// <summary>
/// Extension methods for registering Retail Intelligence Studio agents with DI.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds all intelligence roles and the workflow orchestrator to the service collection.
    /// Uses YAML-based agent definitions for versionable configuration.
    /// Requires IAgentFactory to be registered separately.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="agentsBasePath">Base path where the Workflows/Agents YAML files are located. 
    /// Defaults to AppContext.BaseDirectory.</param>
    public static IServiceCollection AddRetailIntelligenceAgents(
        this IServiceCollection services, 
        string? agentsBasePath = null)
    {
        var basePath = agentsBasePath ?? AppContext.BaseDirectory;

        // Register infrastructure services
        services.AddSingleton<IAgentDefinitionLoader>(sp =>
        {
            var logger = sp.GetRequiredService<ILogger<AgentDefinitionLoader>>();
            return new AgentDefinitionLoader(basePath, logger);
        });

        services.AddSingleton<IPromptTemplateEngine, PromptTemplateEngine>();
        services.AddSingleton<IYamlRoleFactory, YamlRoleFactory>();

        // Register all intelligence roles from YAML definitions
        services.AddSingleton<IEnumerable<IIntelligenceRole>>(sp =>
        {
            var factory = sp.GetRequiredService<IYamlRoleFactory>();
            return factory.CreateAllRoles().ToList();
        });

        // Register workflow orchestrator
        services.AddSingleton<DecisionWorkflowOrchestrator>();

        return services;
    }

    /// <summary>
    /// Adds the Azure OpenAI agent factory using the Microsoft Agent Framework.
    /// Supports both API key and DefaultAzureCredential (Entra ID) authentication.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="options">Azure OpenAI configuration options.</param>
    public static IServiceCollection AddAzureOpenAIAgentFactory(
        this IServiceCollection services,
        AzureOpenAIOptions options)
    {
        services.AddSingleton<IAgentFactory>(sp =>
        {
            var loggerFactory = sp.GetRequiredService<ILoggerFactory>();
            return new AzureOpenAIAgentFactory(options, loggerFactory);
        });

        return services;
    }

    /// <summary>
    /// Adds the Azure OpenAI agent factory using the Microsoft Agent Framework.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="endpoint">Azure OpenAI endpoint URL.</param>
    /// <param name="deploymentName">The model deployment name.</param>
    [Obsolete("Use AddAzureOpenAIAgentFactory(AzureOpenAIOptions) instead for API key support.")]
    public static IServiceCollection AddAzureOpenAIAgentFactory(
        this IServiceCollection services,
        string endpoint,
        string deploymentName)
    {
        return services.AddAzureOpenAIAgentFactory(new AzureOpenAIOptions 
        { 
            Endpoint = endpoint, 
            DeploymentName = deploymentName 
        });
    }

    /// <summary>
    /// Adds a local agent factory for development without Azure OpenAI.
    /// Uses rule-based response generation.
    /// </summary>
    public static IServiceCollection AddLocalAgentFactory(this IServiceCollection services)
    {
        services.AddSingleton<IAgentFactory>(sp =>
        {
            var loggerFactory = sp.GetRequiredService<ILoggerFactory>();
            return new LocalAgentFactory(loggerFactory);
        });

        return services;
    }
}
