using RetailIntelligenceStudio.Core.Models;

namespace RetailIntelligenceStudio.Core.Abstractions;

/// <summary>
/// Provides access to retail persona configurations.
/// </summary>
public interface IPersonaCatalog
{
    /// <summary>
    /// Gets the context for a specific persona.
    /// </summary>
    PersonaContext GetPersonaContext(RetailPersona persona);

    /// <summary>
    /// Gets all available personas.
    /// </summary>
    IReadOnlyList<PersonaContext> GetAllPersonas();

    /// <summary>
    /// Gets a sample decision for a persona.
    /// </summary>
    string GetSampleDecision(RetailPersona persona, int index = 0);
}
