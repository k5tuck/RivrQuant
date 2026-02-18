// Copyright (c) RivrQuant. All rights reserved.
// Licensed under the MIT License.

namespace RivrQuant.Application.DTOs;

/// <summary>
/// Aggregate portfolio snapshot combining positions and balances from all connected brokers,
/// suitable for dashboard display and API responses.
/// </summary>
/// <param name="TotalEquity">Total portfolio equity across all broker accounts.</param>
/// <param name="Cash">Total available cash balance.</param>
/// <param name="BuyingPower">Total buying power including margin.</param>
/// <param name="UnrealizedPnl">Total unrealized profit or loss across all open positions.</param>
/// <param name="DailyChange">Absolute change in portfolio value for the current trading day.</param>
/// <param name="DailyChangePercent">Percentage change in portfolio value for the current trading day.</param>
public sealed record PortfolioDto(
    decimal TotalEquity,
    decimal Cash,
    decimal BuyingPower,
    decimal UnrealizedPnl,
    decimal DailyChange,
    decimal DailyChangePercent);

/// <summary>
/// Projection of an individual open position with broker attribution,
/// including real-time unrealized P&amp;L calculations.
/// </summary>
/// <param name="Symbol">Ticker symbol of the held instrument.</param>
/// <param name="Side">Direction of the position (Buy/Long or Sell/Short).</param>
/// <param name="Qty">Current position quantity.</param>
/// <param name="EntryPrice">Volume-weighted average entry price.</param>
/// <param name="CurrentPrice">Most recent market price.</param>
/// <param name="UnrealizedPnl">Unrealized profit or loss in currency.</param>
/// <param name="PnlPercent">Unrealized P&amp;L as a percentage of cost basis.</param>
/// <param name="Broker">Broker through which the position is held.</param>
/// <param name="AssetClass">Asset class of the instrument (Stock, Crypto).</param>
public sealed record PositionDto(
    string Symbol,
    string Side,
    decimal Qty,
    decimal EntryPrice,
    decimal CurrentPrice,
    decimal UnrealizedPnl,
    decimal PnlPercent,
    string Broker,
    string AssetClass);
