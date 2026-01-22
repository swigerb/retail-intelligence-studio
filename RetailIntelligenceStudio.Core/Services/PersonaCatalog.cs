using RetailIntelligenceStudio.Core.Abstractions;
using RetailIntelligenceStudio.Core.Models;

namespace RetailIntelligenceStudio.Core.Services;

/// <summary>
/// Provides hard-coded persona configurations for demo purposes.
/// </summary>
public sealed class PersonaCatalog : IPersonaCatalog
{
    private readonly Dictionary<RetailPersona, PersonaContext> _personas;

    public PersonaCatalog()
    {
        _personas = new Dictionary<RetailPersona, PersonaContext>
        {
            [RetailPersona.Grocery] = CreateGroceryPersona(),
            [RetailPersona.QuickServeRestaurant] = CreateQsrPersona(),
            [RetailPersona.SpecialtyRetail] = CreateSpecialtyRetailPersona()
        };
    }

    public PersonaContext GetPersonaContext(RetailPersona persona)
    {
        return _personas.TryGetValue(persona, out var context)
            ? context
            : throw new ArgumentException($"Unknown persona: {persona}", nameof(persona));
    }

    public IReadOnlyList<PersonaContext> GetAllPersonas()
    {
        return _personas.Values.ToList();
    }

    public string GetSampleDecision(RetailPersona persona, int index = 0)
    {
        var context = GetPersonaContext(persona);
        var safeIndex = Math.Abs(index) % context.SampleDecisionTemplates.Length;
        return context.SampleDecisionTemplates[safeIndex];
    }

    private static PersonaContext CreateGroceryPersona()
    {
        return new PersonaContext
        {
            Persona = RetailPersona.Grocery,
            DisplayName = "Grocery Retail",
            Description = "Traditional grocery retail including supermarkets, neighborhood stores, and online grocery fulfillment.",
            KeyCategories =
            [
                "Beverages",
                "Fresh Produce",
                "Frozen Foods",
                "Center Store",
                "Dairy",
                "Bakery",
                "Deli & Prepared Foods"
            ],
            Channels =
            [
                "In-Store",
                "Curbside Pickup",
                "Home Delivery",
                "Click & Collect"
            ],
            BaselineKpis = new Dictionary<string, double>
            {
                ["units_per_transaction"] = 12.5,
                ["average_basket_size"] = 48.50,
                ["gross_margin_percent"] = 28.5,
                ["trip_frequency_weekly"] = 1.8,
                ["category_penetration"] = 0.42,
                ["promotional_lift"] = 1.35,
                ["shrink_percent"] = 2.1,
                ["out_of_stock_percent"] = 3.5
            },
            SampleDecisionTemplates =
            [
                "Should we run a 20% off promotion on 12-pack sparkling water in Southeast stores for the next 4 weeks?",
                "Should we expand our organic produce section by 25% and reduce conventional produce space in urban stores?",
                "Should we introduce a $5 meal deal combining deli sandwich, chips, and drink for lunch daypart?",
                "Should we implement dynamic pricing for dairy products within 3 days of expiration to reduce shrink?",
                "Should we add a third-party delivery partnership to supplement our existing delivery fleet in suburban markets?"
            ],
            BaselineAssumptions = new Dictionary<string, string>
            {
                ["price_elasticity"] = "Grocery customers show moderate price sensitivity with elasticity around -1.2 to -1.5 for staples",
                ["promotional_response"] = "Typical promotional lift ranges from 25-50% for beverages, with some cannibalization of adjacent products",
                ["basket_impact"] = "Strong promotions on traffic-driving categories can increase overall basket by 8-12%",
                ["inventory_lead_time"] = "Standard replenishment cycles are 2-3 days for DSD, 5-7 days for warehouse items",
                ["margin_structure"] = "Beverage margins average 30-35%, fresh items 25-30%, center store 22-28%"
            }
        };
    }

    private static PersonaContext CreateQsrPersona()
    {
        return new PersonaContext
        {
            Persona = RetailPersona.QuickServeRestaurant,
            DisplayName = "Quick-Serve Restaurant",
            Description = "Fast food and quick-service restaurant operations including drive-thru, in-store, and mobile ordering.",
            KeyCategories =
            [
                "Combo Meals",
                "Add-Ons & Sides",
                "Beverages",
                "Limited Time Offers (LTOs)",
                "Breakfast",
                "Value Menu",
                "Desserts"
            ],
            Channels =
            [
                "In-Store",
                "Drive-Thru",
                "Mobile App",
                "Third-Party Delivery",
                "Catering"
            ],
            BaselineKpis = new Dictionary<string, double>
            {
                ["average_check"] = 9.75,
                ["throughput_per_hour"] = 85,
                ["attachment_rate"] = 0.45,
                ["daypart_breakfast_share"] = 0.22,
                ["daypart_lunch_share"] = 0.38,
                ["daypart_dinner_share"] = 0.28,
                ["mobile_order_percent"] = 0.32,
                ["drive_thru_time_seconds"] = 195
            },
            SampleDecisionTemplates =
            [
                "Should we launch a limited-time spicy chicken sandwich at $6.99 for 8 weeks during summer?",
                "Should we extend breakfast hours from 10:30am to 11:30am to capture late morning traffic?",
                "Should we offer a 20% discount on mobile app orders over $15 to drive app adoption?",
                "Should we add a premium burger line at $2 higher price point to compete with fast-casual?",
                "Should we implement surge pricing during peak drive-thru hours to manage throughput?"
            ],
            BaselineAssumptions = new Dictionary<string, string>
            {
                ["lto_performance"] = "Successful LTOs typically drive 15-25% traffic lift in first 2 weeks, declining to 5-10% thereafter",
                ["mobile_behavior"] = "Mobile app customers have 20% higher average check and 35% higher visit frequency",
                ["daypart_dynamics"] = "Breakfast represents highest margins at 65%, lunch at 58%, dinner at 52%",
                ["drive_thru_economics"] = "Each 10-second reduction in drive-thru time correlates with 2-3% revenue increase",
                ["attachment_patterns"] = "Beverage attachment drives 40% of profit; dessert attachment at 15% but growing"
            }
        };
    }

    private static PersonaContext CreateSpecialtyRetailPersona()
    {
        return new PersonaContext
        {
            Persona = RetailPersona.SpecialtyRetail,
            DisplayName = "Specialty Retail",
            Description = "Specialty retail including electronics, apparel, and lifestyle brands with omnichannel presence.",
            KeyCategories =
            [
                "Consumer Electronics",
                "Apparel & Accessories",
                "Footwear",
                "Home & Lifestyle",
                "Personal Care",
                "Sporting Goods"
            ],
            Channels =
            [
                "Retail Stores",
                "E-Commerce",
                "Buy Online Pickup In-Store (BOPIS)",
                "Ship from Store",
                "Marketplace"
            ],
            BaselineKpis = new Dictionary<string, double>
            {
                ["conversion_rate"] = 0.032,
                ["average_order_value"] = 125.00,
                ["gross_margin_percent"] = 42.5,
                ["attachment_rate"] = 0.28,
                ["return_rate_percent"] = 12.5,
                ["bopis_percent"] = 0.18,
                ["inventory_turns"] = 4.2,
                ["customer_acquisition_cost"] = 45.00
            },
            SampleDecisionTemplates =
            [
                "Should we offer free shipping on orders over $50 instead of $75 to improve conversion?",
                "Should we launch a premium loyalty tier with 10% discount for customers spending $500+ annually?",
                "Should we implement endless aisle in stores to offer full online assortment for in-store customers?",
                "Should we reduce return window from 60 to 30 days to decrease return rate and improve inventory?",
                "Should we expand BOPIS capacity by 50% and offer 2-hour pickup guarantee?"
            ],
            BaselineAssumptions = new Dictionary<string, string>
            {
                ["channel_economics"] = "E-commerce margins 3-5% lower than store due to shipping; BOPIS bridges the gap",
                ["return_dynamics"] = "Apparel returns average 25-30%; electronics 8-10%; free returns increase orders by 15%",
                ["loyalty_impact"] = "Loyalty members spend 2.5x more annually; tier programs drive additional 15% lift",
                ["conversion_factors"] = "Free shipping is #1 conversion driver; each $10 threshold reduction lifts conversion 8-12%",
                ["inventory_strategy"] = "Endless aisle can capture 15-20% of lost sales from out-of-stocks"
            }
        };
    }
}
