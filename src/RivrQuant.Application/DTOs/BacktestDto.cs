// Copyright (c) RivrQuant. All rights reserved.
// Licensed under the MIT License.

namespace RivrQuant.Application.DTOs;

/// <summary>
/// Projection of a single backtest trade for display in detail views.
/// </summary>
/// <param name="Id">Unique identifier for the trade.</param>
/// <param name="Symbol">Ticker symbol of the traded instrument.</param>
/// <param name="Side">Direction of the trade (Buy or Sell).</param>
/// <param name="EntryTime">Timestamp when the position was opened.</param>
/// <param name="ExitTime">Timestamp when the position was closed.</param>
/// <param name="EntryPrice">Price at which the position was entered.</param>
/// <param name="ExitPrice">Price at which the position was exited.</param>
/// <param name="Quantity">Number of shares or contracts traded.</param>
/// <param name="ProfitLoss">Absolute profit or loss for the trade.</param>
/// <param name="ProfitLossPercent">Profit or loss as a percentage of entry value.</param>
/// <param name="HoldingPeriod">Duration the position was held.</param>
/// <param name="IsWin">Whether the trade was profitable.</param>
public sealed record BacktestTradeDto(
    Guid Id,
    string Symbol,
    string Side,
    DateTimeOffset EntryTime,
    DateTimeOffset ExitTime,
    decimal EntryPrice,
    decimal ExitPrice,
    decimal Quantity,
    decimal ProfitLoss,
    decimal ProfitLossPercent,
    TimeSpan HoldingPeriod,
    bool IsWin);
