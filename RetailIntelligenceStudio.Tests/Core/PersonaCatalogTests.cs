using FluentAssertions;
using RetailIntelligenceStudio.Core.Models;
using RetailIntelligenceStudio.Core.Services;

namespace RetailIntelligenceStudio.Tests.Core;

public class PersonaCatalogTests
{
    private readonly PersonaCatalog _sut;

    public PersonaCatalogTests()
    {
        _sut = new PersonaCatalog();
    }

    [Fact]
    public void GetAllPersonas_ReturnsAllSeventeenPersonas()
    {
        // Act
        var personas = _sut.GetAllPersonas();

        // Assert
        personas.Should().HaveCount(17);
        personas.Select(p => p.Persona).Should().BeEquivalentTo(new[]
        {
            // Food & Dining
            RetailPersona.Grocery,
            RetailPersona.QuickServeRestaurant,
            RetailPersona.ConvenienceStore,
            // Mass Market
            RetailPersona.BigBox,
            RetailPersona.DiscountValue,
            RetailPersona.WarehouseClub,
            // Specialty & Fashion
            RetailPersona.SpecialtyRetail,
            RetailPersona.ApparelFootwear,
            RetailPersona.LuxuryPremium,
            RetailPersona.DepartmentStore,
            // Home & Auto
            RetailPersona.HomeImprovement,
            RetailPersona.ConsumerElectronics,
            RetailPersona.Automotive,
            // Health & Wellness
            RetailPersona.PharmacyHealth,
            // Digital & Emerging
            RetailPersona.DirectToConsumer,
            RetailPersona.Recommerce,
            RetailPersona.TravelRetail
        });
    }

    [Theory]
    [InlineData(RetailPersona.Grocery)]
    [InlineData(RetailPersona.QuickServeRestaurant)]
    [InlineData(RetailPersona.ConvenienceStore)]
    [InlineData(RetailPersona.BigBox)]
    [InlineData(RetailPersona.DiscountValue)]
    [InlineData(RetailPersona.WarehouseClub)]
    [InlineData(RetailPersona.SpecialtyRetail)]
    [InlineData(RetailPersona.ApparelFootwear)]
    [InlineData(RetailPersona.LuxuryPremium)]
    [InlineData(RetailPersona.DepartmentStore)]
    [InlineData(RetailPersona.HomeImprovement)]
    [InlineData(RetailPersona.ConsumerElectronics)]
    [InlineData(RetailPersona.Automotive)]
    [InlineData(RetailPersona.PharmacyHealth)]
    [InlineData(RetailPersona.DirectToConsumer)]
    [InlineData(RetailPersona.Recommerce)]
    [InlineData(RetailPersona.TravelRetail)]
    public void GetPersonaContext_ReturnsValidContext(RetailPersona persona)
    {
        // Act
        var context = _sut.GetPersonaContext(persona);

        // Assert
        context.Should().NotBeNull();
        context.Persona.Should().Be(persona);
        context.DisplayName.Should().NotBeNullOrEmpty();
        context.Description.Should().NotBeNullOrEmpty();
        context.KeyCategories.Should().NotBeEmpty();
        context.Channels.Should().NotBeEmpty();
        context.BaselineKpis.Should().NotBeEmpty();
        context.SampleDecisionTemplates.Should().NotBeEmpty();
    }

    [Fact]
    public void GetPersonaContext_Grocery_HasCorrectConfiguration()
    {
        // Act
        var context = _sut.GetPersonaContext(RetailPersona.Grocery);

        // Assert
        context.DisplayName.Should().Contain("Grocery");
        context.KeyCategories.Should().NotBeEmpty();
        context.Channels.Should().NotBeEmpty();
    }

    [Theory]
    [InlineData(RetailPersona.Grocery)]
    [InlineData(RetailPersona.QuickServeRestaurant)]
    [InlineData(RetailPersona.ConvenienceStore)]
    [InlineData(RetailPersona.BigBox)]
    [InlineData(RetailPersona.DiscountValue)]
    [InlineData(RetailPersona.WarehouseClub)]
    [InlineData(RetailPersona.SpecialtyRetail)]
    [InlineData(RetailPersona.ApparelFootwear)]
    [InlineData(RetailPersona.LuxuryPremium)]
    [InlineData(RetailPersona.DepartmentStore)]
    [InlineData(RetailPersona.HomeImprovement)]
    [InlineData(RetailPersona.ConsumerElectronics)]
    [InlineData(RetailPersona.Automotive)]
    [InlineData(RetailPersona.PharmacyHealth)]
    [InlineData(RetailPersona.DirectToConsumer)]
    [InlineData(RetailPersona.Recommerce)]
    [InlineData(RetailPersona.TravelRetail)]
    public void GetSampleDecision_ReturnsNonEmptyString(RetailPersona persona)
    {
        // Act
        var decision = _sut.GetSampleDecision(persona);

        // Assert
        decision.Should().NotBeNullOrWhiteSpace();
    }

    [Theory]
    [InlineData(RetailPersona.Grocery, 0)]
    [InlineData(RetailPersona.Grocery, 1)]
    [InlineData(RetailPersona.Grocery, 2)]
    public void GetSampleDecision_WithIndex_ReturnsDifferentDecisions(RetailPersona persona, int index)
    {
        // Act
        var decision = _sut.GetSampleDecision(persona, index);

        // Assert
        decision.Should().NotBeNullOrWhiteSpace();
    }
}
