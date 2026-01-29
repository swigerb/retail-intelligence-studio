using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace RetailIntelligenceStudio.Agents.Infrastructure;

/// <summary>
/// Service for loading and caching agent definitions from YAML files.
/// Provides runtime access to agent configuration without hardcoding in C#.
/// </summary>
public interface IAgentDefinitionLoader
{
    /// <summary>
    /// Gets an agent definition by its name.
    /// </summary>
    /// <param name="agentName">The agent name (e.g., "decision_framer").</param>
    /// <returns>The agent definition, or throws if not found.</returns>
    AgentDefinition GetDefinition(string agentName);

    /// <summary>
    /// Gets all loaded agent definitions.
    /// </summary>
    IReadOnlyDictionary<string, AgentDefinition> GetAllDefinitions();

    /// <summary>
    /// Reloads all agent definitions from YAML files.
    /// </summary>
    void Reload();
}

/// <summary>
/// Loads agent definitions from YAML files in the Workflows/Agents directory.
/// </summary>
public sealed class AgentDefinitionLoader : IAgentDefinitionLoader
{
    private readonly string _agentsDirectory;
    private readonly ILogger<AgentDefinitionLoader> _logger;
    private readonly IDeserializer _deserializer;
    private Dictionary<string, AgentDefinition> _definitions = new(StringComparer.OrdinalIgnoreCase);
    private readonly object _lock = new();

    public AgentDefinitionLoader(string basePath, ILogger<AgentDefinitionLoader> logger)
    {
        _agentsDirectory = Path.Combine(basePath, "Workflows", "Agents");
        _logger = logger;
        _deserializer = new DeserializerBuilder()
            .WithNamingConvention(UnderscoredNamingConvention.Instance)
            .IgnoreUnmatchedProperties()
            .Build();

        LoadDefinitions();
    }

    public AgentDefinition GetDefinition(string agentName)
    {
        lock (_lock)
        {
            if (_definitions.TryGetValue(agentName, out var definition))
            {
                return definition;
            }
        }

        throw new KeyNotFoundException($"Agent definition not found: '{agentName}'. " +
            $"Ensure a YAML file exists at '{Path.Combine(_agentsDirectory, agentName + ".yaml")}'.");
    }

    public IReadOnlyDictionary<string, AgentDefinition> GetAllDefinitions()
    {
        lock (_lock)
        {
            return new Dictionary<string, AgentDefinition>(_definitions, StringComparer.OrdinalIgnoreCase);
        }
    }

    public void Reload()
    {
        _logger.LogInformation("Reloading agent definitions from {Directory}", _agentsDirectory);
        LoadDefinitions();
    }

    private void LoadDefinitions()
    {
        var newDefinitions = new Dictionary<string, AgentDefinition>(StringComparer.OrdinalIgnoreCase);

        if (!Directory.Exists(_agentsDirectory))
        {
            _logger.LogWarning("Agent definitions directory not found: {Directory}. No agents will be loaded.", _agentsDirectory);
            lock (_lock)
            {
                _definitions = newDefinitions;
            }
            return;
        }

        var yamlFiles = Directory.GetFiles(_agentsDirectory, "*.yaml");
        _logger.LogInformation("Found {Count} agent definition files in {Directory}", yamlFiles.Length, _agentsDirectory);

        foreach (var filePath in yamlFiles)
        {
            try
            {
                var yaml = File.ReadAllText(filePath);
                var definition = _deserializer.Deserialize<AgentDefinition>(yaml);

                if (string.IsNullOrEmpty(definition.Name))
                {
                    _logger.LogWarning("Agent definition in {File} is missing 'name' property, skipping.", filePath);
                    continue;
                }

                newDefinitions[definition.Name] = definition;
                _logger.LogDebug("Loaded agent definition: {Name} from {File}", definition.Name, filePath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load agent definition from {File}", filePath);
            }
        }

        lock (_lock)
        {
            _definitions = newDefinitions;
        }

        _logger.LogInformation("Loaded {Count} agent definitions: {Names}", 
            newDefinitions.Count, 
            string.Join(", ", newDefinitions.Keys));
    }
}
