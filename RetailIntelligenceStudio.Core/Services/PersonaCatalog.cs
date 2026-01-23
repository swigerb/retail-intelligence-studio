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
            // Food & Dining
            [RetailPersona.Grocery] = CreateGroceryPersona(),
            [RetailPersona.QuickServeRestaurant] = CreateQsrPersona(),
            [RetailPersona.ConvenienceStore] = CreateConvenienceStorePersona(),

            // Mass Market
            [RetailPersona.BigBox] = CreateBigBoxPersona(),
            [RetailPersona.DiscountValue] = CreateDiscountValuePersona(),
            [RetailPersona.WarehouseClub] = CreateWarehouseClubPersona(),

            // Specialty & Fashion
            [RetailPersona.SpecialtyRetail] = CreateSpecialtyRetailPersona(),
            [RetailPersona.ApparelFootwear] = CreateApparelFootwearPersona(),
            [RetailPersona.LuxuryPremium] = CreateLuxuryPremiumPersona(),
            [RetailPersona.DepartmentStore] = CreateDepartmentStorePersona(),

            // Home & Auto
            [RetailPersona.HomeImprovement] = CreateHomeImprovementPersona(),
            [RetailPersona.ConsumerElectronics] = CreateConsumerElectronicsPersona(),
            [RetailPersona.Automotive] = CreateAutomotivePersona(),

            // Health & Wellness
            [RetailPersona.PharmacyHealth] = CreatePharmacyHealthPersona(),

            // Digital & Emerging
            [RetailPersona.DirectToConsumer] = CreateDirectToConsumerPersona(),
            [RetailPersona.Recommerce] = CreateRecommercePersona(),
            [RetailPersona.TravelRetail] = CreateTravelRetailPersona()
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

    // ===== Food & Dining =====

    private static PersonaContext CreateGroceryPersona()
    {
        return new PersonaContext
        {
            Persona = RetailPersona.Grocery,
            Category = RetailCategory.FoodAndDining,
            DisplayName = "Grocery Retail",
            Description = "Traditional grocery retail including supermarkets, neighborhood stores, and online grocery fulfillment.",
            KeyCategories = ["Beverages", "Fresh Produce", "Frozen Foods", "Center Store", "Dairy", "Bakery", "Deli & Prepared Foods"],
            Channels = ["In-Store", "Curbside Pickup", "Home Delivery", "Click & Collect"],
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
            Category = RetailCategory.FoodAndDining,
            DisplayName = "Quick-Serve Restaurant",
            Description = "Fast food and quick-service restaurant operations including drive-thru, in-store, and mobile ordering.",
            KeyCategories = ["Combo Meals", "Add-Ons & Sides", "Beverages", "Limited Time Offers (LTOs)", "Breakfast", "Value Menu", "Desserts"],
            Channels = ["In-Store", "Drive-Thru", "Mobile App", "Third-Party Delivery", "Catering"],
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

    private static PersonaContext CreateConvenienceStorePersona()
    {
        return new PersonaContext
        {
            Persona = RetailPersona.ConvenienceStore,
            Category = RetailCategory.FoodAndDining,
            DisplayName = "Convenience Store",
            Description = "Small-format, high-frequency retailers with quick transactions and foodservice overlap. Examples: 7-Eleven, Circle K, Wawa.",
            KeyCategories = ["Tobacco & Nicotine", "Packaged Beverages", "Foodservice & Fresh", "Snacks & Candy", "Beer & Wine", "Lottery", "General Merchandise"],
            Channels = ["In-Store", "Fuel Forecourt", "Mobile App", "Third-Party Delivery"],
            BaselineKpis = new Dictionary<string, double>
            {
                ["transactions_per_day"] = 850,
                ["average_basket_size"] = 8.75,
                ["fuel_attach_rate"] = 0.35,
                ["foodservice_percent"] = 0.22,
                ["gross_margin_percent"] = 32.5,
                ["shrink_percent"] = 1.8,
                ["inventory_turns"] = 18.5,
                ["labor_cost_percent"] = 8.5
            },
            SampleDecisionTemplates =
            [
                "Should we expand our fresh foodservice offering to include made-to-order sandwiches in top 200 urban stores?",
                "Should we implement a $0.50 discount on fountain drinks for loyalty app users to drive digital adoption?",
                "Should we add EV charging stations at 50 suburban locations and how should we price electricity?",
                "Should we extend operating hours to 24/7 in college town locations during the school year?",
                "Should we launch a subscription program for unlimited fountain drinks at $9.99/month?"
            ],
            BaselineAssumptions = new Dictionary<string, string>
            {
                ["transaction_patterns"] = "Peak transactions occur at morning commute (6-9am) and evening (4-7pm); fuel drives 60% of traffic",
                ["basket_dynamics"] = "Fuel customers who enter store spend 3x more than fuel-only transactions",
                ["foodservice_margins"] = "Fresh foodservice margins 45-55% vs 25-30% for packaged goods",
                ["labor_efficiency"] = "Each additional transaction per labor hour increases store profitability by 2-3%",
                ["loyalty_impact"] = "Loyalty members visit 2.5x more frequently and have 15% higher basket size"
            }
        };
    }

    // ===== Mass Market =====

    private static PersonaContext CreateBigBoxPersona()
    {
        return new PersonaContext
        {
            Persona = RetailPersona.BigBox,
            Category = RetailCategory.MassMarket,
            DisplayName = "Big Box / Mass Merchandiser",
            Description = "Large-format, high-SKU retailers offering broad assortments at scale. Examples: Walmart, Target, Costco.",
            KeyCategories = ["Grocery & Consumables", "Apparel", "Home & Furniture", "Electronics", "Health & Wellness", "Toys & Seasonal", "Automotive"],
            Channels = ["Supercenter", "E-Commerce", "Curbside Pickup", "Ship from Store", "Marketplace"],
            BaselineKpis = new Dictionary<string, double>
            {
                ["sales_per_square_foot"] = 425.00,
                ["average_transaction"] = 52.00,
                ["inventory_turns"] = 8.5,
                ["gross_margin_percent"] = 24.5,
                ["ecommerce_percent"] = 0.18,
                ["out_of_stock_percent"] = 2.8,
                ["labor_cost_percent"] = 10.5,
                ["shrink_percent"] = 1.4
            },
            SampleDecisionTemplates =
            [
                "Should we expand our grocery assortment by 2,000 SKUs in 500 supercenters to compete with traditional grocers?",
                "Should we implement same-day delivery for orders over $35 in top 100 metro markets?",
                "Should we launch a Walmart+ style membership program at $98/year with free delivery and fuel discounts?",
                "Should we reduce private label pricing by 5% to capture inflation-sensitive shoppers from premium brands?",
                "Should we add automated micro-fulfillment centers to 200 stores to improve pickup speed by 40%?"
            ],
            BaselineAssumptions = new Dictionary<string, string>
            {
                ["price_perception"] = "Price leadership is critical; 70% of shoppers cite low prices as primary reason to visit",
                ["omnichannel_behavior"] = "Omnichannel customers spend 2x more annually than single-channel shoppers",
                ["private_label"] = "Private label penetration averages 25-30%; each 1% increase adds 50-75 bps to margin",
                ["fulfillment_economics"] = "Store pickup costs $3-5 per order vs $8-12 for home delivery",
                ["labor_productivity"] = "Automation investments typically yield 15-25% labor cost reduction in fulfillment"
            }
        };
    }

    private static PersonaContext CreateDiscountValuePersona()
    {
        return new PersonaContext
        {
            Persona = RetailPersona.DiscountValue,
            Category = RetailCategory.MassMarket,
            DisplayName = "Discount / Value Retailer",
            Description = "Price-driven retailers emphasizing low cost and fast turnover with limited SKU strategy. Examples: Dollar General, Dollar Tree, Five Below, Aldi.",
            KeyCategories = ["Consumables", "Seasonal & Party", "Home Décor", "Health & Beauty", "Food & Snacks", "Toys & Crafts", "Cleaning Supplies"],
            Channels = ["Small-Format Stores", "E-Commerce", "Buy Online Pickup In-Store"],
            BaselineKpis = new Dictionary<string, double>
            {
                ["sales_per_square_foot"] = 235.00,
                ["average_basket_size"] = 14.50,
                ["gross_margin_percent"] = 30.5,
                ["inventory_turns"] = 5.2,
                ["store_labor_hours"] = 140,
                ["shrink_percent"] = 2.8,
                ["new_store_payback_months"] = 18,
                ["sku_count"] = 4500
            },
            SampleDecisionTemplates =
            [
                "Should we raise the price point ceiling from $1.25 to $1.50 on 30% of consumable SKUs?",
                "Should we add refrigerated and frozen sections to 1,000 rural stores to capture grocery trips?",
                "Should we launch a private label cleaning supplies line at 20% lower price than national brands?",
                "Should we reduce SKU count by 15% to improve in-stock rates and simplify store operations?",
                "Should we implement self-checkout in high-volume urban stores to reduce labor costs?"
            ],
            BaselineAssumptions = new Dictionary<string, string>
            {
                ["price_sensitivity"] = "Core customers are extremely price-sensitive; $0.25 price increases can reduce unit sales 15-20%",
                ["store_economics"] = "Small-format stores (7,500 sq ft) achieve profitability at $1.8M annual sales",
                ["assortment_strategy"] = "Limited SKU model (4,000-5,000) enables faster turns and simpler operations",
                ["rural_opportunity"] = "Rural/exurban locations often have no competition within 15-mile radius",
                ["shrink_challenge"] = "Shrink rates 2-3x higher than traditional retail; organized retail crime a growing concern"
            }
        };
    }

    private static PersonaContext CreateWarehouseClubPersona()
    {
        return new PersonaContext
        {
            Persona = RetailPersona.WarehouseClub,
            Category = RetailCategory.MassMarket,
            DisplayName = "Warehouse Club",
            Description = "Membership-based bulk retail with limited SKUs and high turnover. Examples: Costco, BJ's Wholesale, Sam's Club.",
            KeyCategories = ["Bulk Grocery", "Fresh & Prepared Foods", "Electronics", "Home & Furniture", "Apparel", "Health & Beauty", "Business Supplies"],
            Channels = ["Warehouse Stores", "E-Commerce", "Business Delivery", "Instacart Partnership"],
            BaselineKpis = new Dictionary<string, double>
            {
                ["membership_renewal_rate"] = 0.91,
                ["average_transaction"] = 135.00,
                ["gross_margin_percent"] = 11.5,
                ["membership_revenue_percent"] = 0.70,
                ["sku_count"] = 3800,
                ["inventory_turns"] = 12.5,
                ["sales_per_member"] = 2850.00,
                ["executive_member_percent"] = 0.42
            },
            SampleDecisionTemplates =
            [
                "Should we increase annual membership fee from $60 to $65 given current 91% renewal rate?",
                "Should we expand our Kirkland Signature private label into premium organic categories?",
                "Should we add same-day delivery through Instacart for a $10 fee in top 50 markets?",
                "Should we launch a small-format urban warehouse concept at 50,000 sq ft vs traditional 145,000 sq ft?",
                "Should we introduce dynamic pricing on fresh items approaching sell-by date to reduce shrink?"
            ],
            BaselineAssumptions = new Dictionary<string, string>
            {
                ["membership_economics"] = "Membership fees drive 70%+ of operating profit; renewal rate is critical KPI",
                ["pricing_strategy"] = "14% max markup policy creates trust; Kirkland brand at 20-40% below national brands",
                ["treasure_hunt"] = "25% of SKUs rotate regularly creating 'treasure hunt' experience that drives visits",
                ["member_tiers"] = "Executive members ($120/year) spend 2.5x more and have 95% renewal rate",
                ["bulk_economics"] = "Bulk packaging reduces per-unit cost 20-35% vs traditional retail sizes"
            }
        };
    }

    // ===== Specialty & Fashion =====

    private static PersonaContext CreateSpecialtyRetailPersona()
    {
        return new PersonaContext
        {
            Persona = RetailPersona.SpecialtyRetail,
            Category = RetailCategory.SpecialtyAndFashion,
            DisplayName = "Specialty Retail",
            Description = "Specialty retail including electronics, apparel, and lifestyle brands with omnichannel presence.",
            KeyCategories = ["Consumer Electronics", "Apparel & Accessories", "Footwear", "Home & Lifestyle", "Personal Care", "Sporting Goods"],
            Channels = ["Retail Stores", "E-Commerce", "Buy Online Pickup In-Store (BOPIS)", "Ship from Store", "Marketplace"],
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

    private static PersonaContext CreateApparelFootwearPersona()
    {
        return new PersonaContext
        {
            Persona = RetailPersona.ApparelFootwear,
            Category = RetailCategory.SpecialtyAndFashion,
            DisplayName = "Apparel & Footwear",
            Description = "Fashion-centric retailers with high seasonality and rapid product cycles. Examples: Nike, Gap, Lululemon, Foot Locker.",
            KeyCategories = ["Women's Apparel", "Men's Apparel", "Footwear", "Activewear & Athleisure", "Accessories", "Kids & Baby", "Outerwear"],
            Channels = ["Retail Stores", "E-Commerce", "Mobile App", "Outlet Stores", "Wholesale Partners"],
            BaselineKpis = new Dictionary<string, double>
            {
                ["sell_through_rate"] = 0.65,
                ["full_price_sell_through"] = 0.45,
                ["gross_margin_percent"] = 55.0,
                ["return_rate_percent"] = 28.0,
                ["average_order_value"] = 95.00,
                ["inventory_turns"] = 3.8,
                ["markdown_percent"] = 0.35,
                ["units_per_transaction"] = 2.4
            },
            SampleDecisionTemplates =
            [
                "Should we launch a rental/subscription program for premium activewear at $49/month for 4 items?",
                "Should we reduce our seasonal buy by 20% and increase in-season replenishment to reduce markdowns?",
                "Should we offer virtual try-on technology for footwear to reduce our 28% return rate?",
                "Should we expand our resale program to accept competitor brands and offer store credit?",
                "Should we shift 30% of marketing spend from brand awareness to performance/conversion campaigns?"
            ],
            BaselineAssumptions = new Dictionary<string, string>
            {
                ["seasonality"] = "40% of annual sales occur in Q4; back-to-school and holiday are critical windows",
                ["size_fragmentation"] = "Size/color complexity creates 50-80 SKUs per style; stockouts on popular sizes cost 15-20% of demand",
                ["return_economics"] = "Each return costs $10-15 to process; 30% of returns are non-resalable at full price",
                ["brand_heat"] = "Brand desirability drives 2-3x variance in sell-through between hot and declining styles",
                ["dtc_shift"] = "Direct-to-consumer margins 15-20 points higher than wholesale; driving channel shift"
            }
        };
    }

    private static PersonaContext CreateLuxuryPremiumPersona()
    {
        return new PersonaContext
        {
            Persona = RetailPersona.LuxuryPremium,
            Category = RetailCategory.SpecialtyAndFashion,
            DisplayName = "Luxury & Premium",
            Description = "High-margin, brand-led retail with strong clienteling and in-store experience focus. Examples: Louis Vuitton, Gucci, Rolex, Hermès.",
            KeyCategories = ["Leather Goods & Handbags", "Ready-to-Wear", "Watches & Jewelry", "Shoes", "Fragrances & Beauty", "Accessories", "Home & Lifestyle"],
            Channels = ["Flagship Boutiques", "Department Store Concessions", "E-Commerce", "Private Client", "Travel Retail"],
            BaselineKpis = new Dictionary<string, double>
            {
                ["average_transaction_value"] = 1850.00,
                ["gross_margin_percent"] = 68.0,
                ["conversion_rate"] = 0.18,
                ["clienteling_revenue_percent"] = 0.45,
                ["vip_customer_percent"] = 0.08,
                ["vip_revenue_percent"] = 0.42,
                ["return_rate_percent"] = 5.0,
                ["sales_per_square_foot"] = 2800.00
            },
            SampleDecisionTemplates =
            [
                "Should we launch a by-appointment-only private shopping experience for VIP clients spending $50K+ annually?",
                "Should we increase prices by 8% on iconic leather goods to maintain exclusivity and margins?",
                "Should we open a flagship location in Austin, TX given emerging wealth demographics?",
                "Should we expand our authenticated pre-owned program to capture the resale market?",
                "Should we invest in immersive digital experiences (AR/VR) for online clienteling?"
            ],
            BaselineAssumptions = new Dictionary<string, string>
            {
                ["client_concentration"] = "Top 8% of clients drive 42% of revenue; losing a top client costs $75K+ annually",
                ["price_elasticity"] = "Luxury demand is often inelastic or positively correlated with price for iconic items",
                ["experience_premium"] = "In-store experience drives 3-5x higher conversion than digital; store ambiance critical",
                ["scarcity_value"] = "Limited availability on hero products increases desirability; waitlists can exceed 2 years",
                ["brand_equity"] = "Brand perception is everything; any discounting erodes long-term brand value"
            }
        };
    }

    private static PersonaContext CreateDepartmentStorePersona()
    {
        return new PersonaContext
        {
            Persona = RetailPersona.DepartmentStore,
            Category = RetailCategory.SpecialtyAndFashion,
            DisplayName = "Department Store",
            Description = "Multi-category retailers with branded and private-label assortments across fashion, beauty, and home. Examples: Macy's, Nordstrom, Kohl's.",
            KeyCategories = ["Women's Fashion", "Men's Fashion", "Beauty & Cosmetics", "Home & Furniture", "Kids & Toys", "Shoes & Accessories", "Fine Jewelry"],
            Channels = ["Department Stores", "Off-Price/Outlet", "E-Commerce", "Mobile App", "Personal Styling"],
            BaselineKpis = new Dictionary<string, double>
            {
                ["sales_per_square_foot"] = 165.00,
                ["gross_margin_percent"] = 38.0,
                ["loyalty_penetration"] = 0.55,
                ["private_label_percent"] = 0.18,
                ["ecommerce_percent"] = 0.35,
                ["inventory_turns"] = 3.2,
                ["markdown_percent"] = 0.42,
                ["customer_retention_rate"] = 0.62
            },
            SampleDecisionTemplates =
            [
                "Should we convert 15% of floor space to brand partner 'shop-in-shop' concepts to drive traffic?",
                "Should we expand our off-price Rack/Basement format given stronger economics than full-line stores?",
                "Should we enhance our credit card rewards from 3% to 5% cashback to compete with retailer cards?",
                "Should we add same-day delivery from stores within 10-mile radius at $9.95 per order?",
                "Should we exit underperforming categories (furniture, electronics) to focus on fashion and beauty?"
            ],
            BaselineAssumptions = new Dictionary<string, string>
            {
                ["traffic_trends"] = "Mall traffic declining 5-8% annually; digital and off-mall formats growing",
                ["loyalty_economics"] = "Credit card holders spend 3x more annually; 40% of sales from cardholders",
                ["vendor_relationships"] = "Brand partners increasingly opening own stores; concession model helps retain",
                ["off_price_growth"] = "Off-price format margins lower (32% vs 38%) but inventory turns 2x higher",
                ["beauty_strength"] = "Beauty category growing 8-10% annually; strong traffic driver with high attachment"
            }
        };
    }

    // ===== Home & Auto =====

    private static PersonaContext CreateHomeImprovementPersona()
    {
        return new PersonaContext
        {
            Persona = RetailPersona.HomeImprovement,
            Category = RetailCategory.HomeAndAuto,
            DisplayName = "Home Improvement & DIY",
            Description = "Retailers focused on home, construction, and repair with B2C and Pro customer segments. Examples: Home Depot, Lowe's, Ace Hardware.",
            KeyCategories = ["Lumber & Building Materials", "Tools & Hardware", "Plumbing & Electrical", "Paint & Décor", "Appliances", "Outdoor & Garden", "Flooring"],
            Channels = ["Big Box Stores", "E-Commerce", "Pro Desk", "Tool Rental", "Installation Services"],
            BaselineKpis = new Dictionary<string, double>
            {
                ["average_ticket"] = 78.00,
                ["pro_customer_percent"] = 0.45,
                ["pro_revenue_percent"] = 0.48,
                ["attachment_rate_services"] = 0.12,
                ["gross_margin_percent"] = 34.0,
                ["inventory_turns"] = 5.2,
                ["special_order_percent"] = 0.15,
                ["install_attachment_rate"] = 0.08
            },
            SampleDecisionTemplates =
            [
                "Should we expand Pro loyalty program benefits to include 2-hour jobsite delivery for orders over $500?",
                "Should we launch a tool subscription service at $39/month for DIY customers with unlimited rentals?",
                "Should we add installation services for smart home products (thermostats, doorbells, lighting)?",
                "Should we increase private-label paint pricing by 10% given strong brand loyalty and margin opportunity?",
                "Should we open small-format urban stores (25,000 sq ft) focused on décor and installation services?"
            ],
            BaselineAssumptions = new Dictionary<string, string>
            {
                ["pro_economics"] = "Pro customers have 5x higher annual spend but 8-10 points lower margin than DIY",
                ["project_baskets"] = "Project-based purchases average $450+ vs $45 for fill-in trips; big-ticket drives profitability",
                ["installation_opportunity"] = "Installation services carry 35-45% margins; growing as DIY skills decline",
                ["seasonal_patterns"] = "Spring (Mar-May) drives 35% of annual sales; weather significantly impacts demand",
                ["inventory_complexity"] = "50,000+ SKUs with long-tail demand; 20% of SKUs drive 80% of revenue"
            }
        };
    }

    private static PersonaContext CreateConsumerElectronicsPersona()
    {
        return new PersonaContext
        {
            Persona = RetailPersona.ConsumerElectronics,
            Category = RetailCategory.HomeAndAuto,
            DisplayName = "Consumer Electronics",
            Description = "Tech-focused product retailers with high attach-rate services and post-sale support. Examples: Best Buy, Micro Center.",
            KeyCategories = ["Computing & Tablets", "Mobile Phones", "TV & Home Theater", "Appliances", "Gaming", "Smart Home", "Wearables"],
            Channels = ["Big Box Stores", "E-Commerce", "In-Home Services", "Business Solutions", "Marketplace"],
            BaselineKpis = new Dictionary<string, double>
            {
                ["average_order_value"] = 385.00,
                ["services_attach_rate"] = 0.32,
                ["protection_plan_attach"] = 0.22,
                ["gross_margin_percent"] = 23.5,
                ["services_margin_percent"] = 52.0,
                ["return_rate_percent"] = 8.5,
                ["nps_score"] = 62,
                ["employee_expertise_rating"] = 4.2
            },
            SampleDecisionTemplates =
            [
                "Should we expand Geek Squad in-home services to include smart home installation bundles at $199?",
                "Should we launch a tech trade-in program offering 20% bonus credit toward new purchases?",
                "Should we create 'experience zones' for gaming and smart home at 25% of floor space?",
                "Should we offer price matching against Amazon with instant verification at point of sale?",
                "Should we expand our refurbished/open-box selection to capture price-sensitive customers?"
            ],
            BaselineAssumptions = new Dictionary<string, string>
            {
                ["margin_mix"] = "Hardware margins 15-20%; services/protection plans 45-55%; services critical to profitability",
                ["showrooming"] = "40% of customers research in-store then buy online; price matching essential",
                ["expertise_value"] = "Knowledgeable associates drive 25% higher attachment rates and NPS",
                ["product_lifecycle"] = "Tech products have 6-12 month lifecycles; inventory aging risk is high",
                ["services_growth"] = "Services revenue growing 8-12% annually vs 2-3% for products"
            }
        };
    }

    private static PersonaContext CreateAutomotivePersona()
    {
        return new PersonaContext
        {
            Persona = RetailPersona.Automotive,
            Category = RetailCategory.HomeAndAuto,
            DisplayName = "Automotive Retail",
            Description = "Retail sale and servicing of vehicles, parts, and accessories. Examples: AutoZone, CarMax, dealerships.",
            KeyCategories = ["Parts & Accessories", "Maintenance Items", "Tools & Equipment", "Performance Parts", "Batteries", "Fluids & Chemicals", "Interior & Exterior"],
            Channels = ["Retail Stores", "E-Commerce", "Commercial/Pro", "Service Bays", "Mobile Installation"],
            BaselineKpis = new Dictionary<string, double>
            {
                ["average_ticket"] = 42.00,
                ["commercial_percent"] = 0.28,
                ["gross_margin_percent"] = 52.0,
                ["inventory_turns"] = 1.8,
                ["same_day_availability"] = 0.92,
                ["service_attach_rate"] = 0.15,
                ["loyalty_member_percent"] = 0.35,
                ["special_order_fill_rate"] = 0.85
            },
            SampleDecisionTemplates =
            [
                "Should we expand same-day delivery for commercial accounts to capture more shop business?",
                "Should we add EV-specific parts and accessories as electric vehicle adoption grows?",
                "Should we launch mobile battery installation service at customer locations for $25 fee?",
                "Should we acquire a regional auto parts chain to expand geographic coverage?",
                "Should we offer DIY diagnostic scanning in-store to drive parts recommendations and sales?"
            ],
            BaselineAssumptions = new Dictionary<string, string>
            {
                ["diy_vs_difm"] = "DIY market declining 2% annually; DIFM (commercial) growing 4-5%; commercial is future",
                ["parts_availability"] = "Same-day availability critical; 85%+ in-stock drives 15% sales lift vs competitors",
                ["vehicle_age"] = "Average vehicle age 12.5 years and rising; older vehicles need more maintenance parts",
                ["ev_transition"] = "EV parts demand growing 25%+ but represents <3% of market; watch and prepare",
                ["commercial_relationships"] = "Top commercial accounts worth $50K+ annually; service levels drive loyalty"
            }
        };
    }

    // ===== Health & Wellness =====

    private static PersonaContext CreatePharmacyHealthPersona()
    {
        return new PersonaContext
        {
            Persona = RetailPersona.PharmacyHealth,
            Category = RetailCategory.HealthAndWellness,
            DisplayName = "Pharmacy & Health Retail",
            Description = "Retail with regulated health components, clinics, and wellness services. Examples: CVS, Walgreens, Boots.",
            KeyCategories = ["Prescription Drugs", "OTC Medications", "Health & Wellness", "Beauty & Personal Care", "Consumables", "Photo Services", "Clinic Services"],
            Channels = ["Retail Pharmacy", "E-Commerce", "Drive-Thru", "MinuteClinic/In-Store Health", "Mail Order Pharmacy", "Mobile App"],
            BaselineKpis = new Dictionary<string, double>
            {
                ["scripts_per_day"] = 225,
                ["front_store_attach_rate"] = 0.45,
                ["gross_margin_pharmacy"] = 0.22,
                ["gross_margin_front_store"] = 0.32,
                ["clinic_visits_per_week"] = 85,
                ["immunization_attach_rate"] = 0.12,
                ["loyalty_penetration"] = 0.72,
                ["auto_refill_percent"] = 0.38
            },
            SampleDecisionTemplates =
            [
                "Should we extend pharmacy hours to 24/7 in 500 urban locations to capture overnight demand?",
                "Should we expand MinuteClinic services to include chronic disease management at $89/visit?",
                "Should we launch a prescription delivery subscription at $5.99/month for unlimited free delivery?",
                "Should we convert 20% of front-store space to health services (testing, vaccinations)?",
                "Should we partner with telehealth providers to offer in-store virtual consultations?"
            ],
            BaselineAssumptions = new Dictionary<string, string>
            {
                ["script_economics"] = "Pharmacy gross margins compressed to 18-22%; volume and efficiency critical",
                ["front_store_attach"] = "Each pharmacy customer who shops front store adds $15-20 in margin contribution",
                ["healthcare_shift"] = "Retail healthcare growing 15%+ annually; vaccines, testing, basic care expanding",
                ["payer_dynamics"] = "PBM reimbursement declining 3-5% annually; diversification into services essential",
                ["digital_health"] = "Auto-refill and reminder programs reduce abandonment by 25% and improve adherence"
            }
        };
    }

    // ===== Digital & Emerging =====

    private static PersonaContext CreateDirectToConsumerPersona()
    {
        return new PersonaContext
        {
            Persona = RetailPersona.DirectToConsumer,
            Category = RetailCategory.DigitalAndEmerging,
            DisplayName = "Direct-to-Consumer (DTC)",
            Description = "Born-digital brands selling directly to customers with data-driven personalization. Examples: Warby Parker, Allbirds, Casper.",
            KeyCategories = ["Eyewear", "Footwear & Apparel", "Mattresses & Sleep", "Personal Care", "Pet Products", "Food & Beverage", "Home Goods"],
            Channels = ["E-Commerce", "Mobile App", "Pop-Up Stores", "Showrooms", "Subscription"],
            BaselineKpis = new Dictionary<string, double>
            {
                ["customer_acquisition_cost"] = 65.00,
                ["customer_lifetime_value"] = 285.00,
                ["ltv_cac_ratio"] = 4.4,
                ["gross_margin_percent"] = 62.0,
                ["return_rate_percent"] = 15.0,
                ["subscription_retention_90day"] = 0.72,
                ["repeat_purchase_rate"] = 0.35,
                ["nps_score"] = 68
            },
            SampleDecisionTemplates =
            [
                "Should we open 25 permanent retail showrooms to reduce CAC and increase brand awareness?",
                "Should we launch a subscription replenishment program for consumable products at 15% discount?",
                "Should we expand into wholesale partnerships with Target/Nordstrom to reach new customers?",
                "Should we invest $2M in influencer marketing to reduce Facebook/Google ad dependency?",
                "Should we offer a home try-on program (try 5, keep what you love) to reduce purchase friction?"
            ],
            BaselineAssumptions = new Dictionary<string, string>
            {
                ["cac_trends"] = "Digital CAC increasing 15-20% annually; diversification into retail/wholesale essential",
                ["unit_economics"] = "LTV:CAC ratio must exceed 3:1 for sustainable growth; below 2.5:1 is danger zone",
                ["brand_building"] = "Strong brand enables 30-40% lower CAC vs performance-only marketing",
                ["retail_halo"] = "Physical retail presence reduces online CAC by 20-25% in surrounding geography",
                ["subscription_value"] = "Subscribers have 3x higher LTV; auto-ship reduces churn and improves forecasting"
            }
        };
    }

    private static PersonaContext CreateRecommercePersona()
    {
        return new PersonaContext
        {
            Persona = RetailPersona.Recommerce,
            Category = RetailCategory.DigitalAndEmerging,
            DisplayName = "Recommerce / Circular Retail",
            Description = "Resale and sustainability-focused retail with authentication and reverse logistics. Examples: ThredUp, The RealReal, Back Market.",
            KeyCategories = ["Luxury Resale", "Apparel & Footwear", "Consumer Electronics", "Furniture & Home", "Sporting Goods", "Kids & Baby", "Collectibles"],
            Channels = ["E-Commerce Marketplace", "Mobile App", "Consignment Stores", "Trade-In Programs", "B2B Wholesale"],
            BaselineKpis = new Dictionary<string, double>
            {
                ["gross_merchandise_value"] = 125000000,
                ["take_rate_percent"] = 0.35,
                ["authentication_accuracy"] = 0.995,
                ["average_selling_price"] = 85.00,
                ["supply_acquisition_cost"] = 8.50,
                ["processing_cost_per_item"] = 6.25,
                ["sell_through_rate_90day"] = 0.68,
                ["return_rate_percent"] = 10.0
            },
            SampleDecisionTemplates =
            [
                "Should we invest $5M in AI-powered authentication to reduce expert review costs by 40%?",
                "Should we launch brand partnerships offering instant trade-in credit toward new purchases?",
                "Should we expand into refurbished electronics given higher margins and growing demand?",
                "Should we open processing centers in 3 new regions to reduce shipping costs and time?",
                "Should we offer sellers the option for instant payout at 70% of expected value vs consignment?"
            ],
            BaselineAssumptions = new Dictionary<string, string>
            {
                ["supply_constraint"] = "Quality supply acquisition is primary growth constraint; consignor experience critical",
                ["authentication_trust"] = "Authentication accuracy must exceed 99.5%; one counterfeit incident costs millions in trust",
                ["processing_efficiency"] = "Labor cost per item must stay below $7 for unit economics; automation key to scale",
                ["esg_tailwind"] = "Sustainability concerns driving 25%+ annual growth in resale market",
                ["brand_relationships"] = "Brand partnerships (resale-as-a-service) provide reliable supply and marketing"
            }
        };
    }

    private static PersonaContext CreateTravelRetailPersona()
    {
        return new PersonaContext
        {
            Persona = RetailPersona.TravelRetail,
            Category = RetailCategory.DigitalAndEmerging,
            DisplayName = "Travel Retail / Duty Free",
            Description = "Retail in airports, cruise ports, and travel hubs with cross-border compliance. Examples: Dufry, DFS Group.",
            KeyCategories = ["Fragrances & Cosmetics", "Liquor & Tobacco", "Confectionery", "Luxury Goods", "Electronics", "Travel Essentials", "Local Specialties"],
            Channels = ["Airport Duty Free", "Downtown Duty Free", "Cruise Ships", "Border Shops", "Digital Pre-Order"],
            BaselineKpis = new Dictionary<string, double>
            {
                ["spend_per_passenger"] = 28.50,
                ["conversion_rate"] = 0.12,
                ["average_transaction"] = 95.00,
                ["gross_margin_percent"] = 58.0,
                ["pre_order_percent"] = 0.08,
                ["dwell_time_correlation"] = 0.85,
                ["category_penetration_beauty"] = 0.42,
                ["vip_traveler_percent"] = 0.15
            },
            SampleDecisionTemplates =
            [
                "Should we invest in reserve-and-collect digital platform offering 10% discount for pre-orders?",
                "Should we expand exclusive travel retail products (sizes/sets) to differentiate from local retail?",
                "Should we add Chinese mobile payment options (WeChat, Alipay) at all international locations?",
                "Should we implement dynamic pricing based on flight delays and dwell time predictions?",
                "Should we partner with airlines for loyalty program integration offering duty-free miles bonus?"
            ],
            BaselineAssumptions = new Dictionary<string, string>
            {
                ["dwell_time_impact"] = "Each additional 10 minutes of dwell time increases spend by 15-20%",
                ["passenger_mix"] = "Chinese travelers spend 3x average; nationality mix dramatically impacts sales",
                ["exclusivity_premium"] = "Travel retail exclusives command 15-25% premium; differentiation from domestic retail critical",
                ["concession_economics"] = "Airport rent averages 25-35% of sales; high fixed costs require volume",
                ["pre_order_growth"] = "Digital pre-order growing 25%+ annually; reduces in-store time pressure and increases conversion"
            }
        };
    }
}
