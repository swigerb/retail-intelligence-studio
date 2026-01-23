namespace RetailIntelligenceStudio.Core.Models;

/// <summary>
/// Represents the category groupings for retail verticals.
/// Used to organize personas in the UI and group related retail segments.
/// </summary>
public enum RetailCategory
{
    /// <summary>Food &amp; Dining - Grocery, QSR, Convenience Stores</summary>
    FoodAndDining = 1,

    /// <summary>Mass Market - Big Box, Discount, Warehouse Clubs</summary>
    MassMarket = 2,

    /// <summary>Specialty &amp; Fashion - Specialty Retail, Apparel, Luxury, Department Stores</summary>
    SpecialtyAndFashion = 3,

    /// <summary>Home &amp; Auto - Home Improvement, Consumer Electronics, Automotive</summary>
    HomeAndAuto = 4,

    /// <summary>Health &amp; Wellness - Pharmacy / Health Retail</summary>
    HealthAndWellness = 5,

    /// <summary>Digital &amp; Emerging - DTC, Recommerce, Travel Retail</summary>
    DigitalAndEmerging = 6
}

/// <summary>
/// Represents the retail vertical personas supported by Retail Intelligence Studio.
/// Each persona influences assumptions, sample data, agent reasoning, and insight language.
/// </summary>
public enum RetailPersona
{
    // ===== Food & Dining =====

    /// <summary>
    /// Grocery retail - beverages, fresh, frozen, center store.
    /// Channels: in-store, curbside pickup, delivery.
    /// KPIs: units, revenue, margin %, basket size, trip frequency.
    /// </summary>
    Grocery = 1,

    /// <summary>
    /// Quick-Serve Restaurant - combos, add-ons, beverages, LTOs.
    /// Channels: in-store, drive-thru, mobile app.
    /// KPIs: average check, throughput, attachment rate, daypart performance.
    /// </summary>
    QuickServeRestaurant = 2,

    /// <summary>
    /// Convenience Stores - small-format, high-frequency retailers.
    /// Examples: 7-Eleven, Circle K, Wawa.
    /// KPIs: transaction volume, basket size, foodservice attach rate.
    /// </summary>
    ConvenienceStore = 4,

    // ===== Mass Market =====

    /// <summary>
    /// Big Box / Mass Merchandisers - large-format, high-SKU retailers.
    /// Examples: Walmart, Target, Costco.
    /// KPIs: sales per square foot, inventory turns, omnichannel conversion.
    /// </summary>
    BigBox = 5,

    /// <summary>
    /// Discount / Value Retailers - price-driven, fast turnover.
    /// Examples: Dollar General, Dollar Tree, Five Below, Aldi.
    /// KPIs: basket size, shrink rate, store productivity.
    /// </summary>
    DiscountValue = 6,

    /// <summary>
    /// Warehouse Clubs - membership-based bulk retail.
    /// Examples: Costco, BJ's Wholesale, Sam's Club.
    /// KPIs: membership renewal rate, average transaction, SKU velocity.
    /// </summary>
    WarehouseClub = 7,

    // ===== Specialty & Fashion =====

    /// <summary>
    /// Specialty retail - electronics or apparel.
    /// Channels: store, web, BOPIS.
    /// KPIs: conversion rate, gross margin, attachment rate, returns %.
    /// </summary>
    SpecialtyRetail = 3,

    /// <summary>
    /// Apparel &amp; Footwear Retail - fashion-centric, high seasonality.
    /// Examples: Nike, Gap, Lululemon, Foot Locker.
    /// KPIs: sell-through rate, return rate, inventory turns.
    /// </summary>
    ApparelFootwear = 8,

    /// <summary>
    /// Luxury &amp; Premium Retail - high-margin, brand-led retail.
    /// Examples: Louis Vuitton, Gucci, Rolex, Hermès.
    /// KPIs: average transaction value, clienteling conversion, brand equity.
    /// </summary>
    LuxuryPremium = 9,

    /// <summary>
    /// Department Stores - multi-category with branded and private-label.
    /// Examples: Macy's, Nordstrom, Kohl's.
    /// KPIs: sales per square foot, loyalty penetration, omnichannel share.
    /// </summary>
    DepartmentStore = 10,

    // ===== Home & Auto =====

    /// <summary>
    /// Home Improvement &amp; DIY - home, construction, repair.
    /// Examples: Home Depot, Lowe's, Ace Hardware.
    /// KPIs: project basket size, pro customer share, delivery fulfillment.
    /// </summary>
    HomeImprovement = 11,

    /// <summary>
    /// Consumer Electronics Retail - tech-focused products.
    /// Examples: Best Buy, Micro Center.
    /// KPIs: attach rate (services/accessories), return rate, NPS.
    /// </summary>
    ConsumerElectronics = 12,

    /// <summary>
    /// Automotive Retail - vehicles, parts, and services.
    /// Examples: AutoZone, CarMax, dealerships.
    /// KPIs: average ticket, service attach rate, inventory days.
    /// </summary>
    Automotive = 13,

    // ===== Health & Wellness =====

    /// <summary>
    /// Pharmacy / Health Retail - regulated health components.
    /// Examples: CVS, Walgreens, Boots.
    /// KPIs: script volume, front-store attach, clinic utilization.
    /// </summary>
    PharmacyHealth = 14,

    // ===== Digital & Emerging =====

    /// <summary>
    /// Direct-to-Consumer (DTC) / Digital-Native Retail.
    /// Examples: Warby Parker, Allbirds, Casper.
    /// KPIs: CAC, LTV, subscription retention, NPS.
    /// </summary>
    DirectToConsumer = 15,

    /// <summary>
    /// Secondhand / Recommerce / Circular Retail.
    /// Examples: ThredUp, The RealReal, Back Market.
    /// KPIs: authentication accuracy, reverse logistics cost, GMV.
    /// </summary>
    Recommerce = 16,

    /// <summary>
    /// Travel Retail / Duty Free - airports, cruise ports, travel hubs.
    /// Examples: Dufry, DFS Group.
    /// KPIs: spend per passenger, conversion rate, dwell time correlation.
    /// </summary>
    TravelRetail = 17
}
