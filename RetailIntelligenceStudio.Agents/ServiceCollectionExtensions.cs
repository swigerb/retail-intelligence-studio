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
    /// Requires IAgentFactory to be registered separately.
    /// </summary>
    public static IServiceCollection AddRetailIntelligenceAgents(this IServiceCollection services)
    {
        // Register all intelligence roles
        services.AddSingleton<IIntelligenceRole, DecisionFramerRole>();
        services.AddSingleton<IIntelligenceRole, ShopperInsightsRole>();
        services.AddSingleton<IIntelligenceRole, DemandForecastingRole>();
        services.AddSingleton<IIntelligenceRole, InventoryReadinessRole>();
        services.AddSingleton<IIntelligenceRole, MarginImpactRole>();
        services.AddSingleton<IIntelligenceRole, DigitalMerchandisingRole>();
        services.AddSingleton<IIntelligenceRole, RiskComplianceRole>();
        services.AddSingleton<IIntelligenceRole, ExecutiveRecommendationRole>();

        // Register workflow orchestrator
        services.AddSingleton<DecisionWorkflowOrchestrator>();

        return services;
    }

    /// <summary>
    /// Adds the Azure OpenAI agent factory using the Microsoft Agent Framework.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="endpoint">Azure OpenAI endpoint URL.</param>
    /// <param name="deploymentName">The model deployment name.</param>
    public static IServiceCollection AddAzureOpenAIAgentFactory(
        this IServiceCollection services,
        string endpoint,
        string deploymentName)
    {
        services.AddSingleton<IAgentFactory>(sp =>
        {
            var loggerFactory = sp.GetRequiredService<ILoggerFactory>();
            return new AzureOpenAIAgentFactory(endpoint, deploymentName, loggerFactory);
        });

        return services;
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
