namespace RetailIntelligenceStudio.Core.Models;

/// <summary>
/// Represents the retail vertical personas supported by Retail Intelligence Studio.
/// Each persona influences assumptions, sample data, agent reasoning, and insight language.
/// </summary>
public enum RetailPersona
{
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
    /// Specialty retail - electronics or apparel.
    /// Channels: store, web, BOPIS.
    /// KPIs: conversion rate, gross margin, attachment rate, returns %.
    /// </summary>
    SpecialtyRetail = 3
}
