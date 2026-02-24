// Copyright (c) RivrQuant. All rights reserved.
// Licensed under the MIT License.

namespace RivrQuant.Application.DTOs;

/// <summary>
/// Projection of a trading order for display in order history and live order views,
/// including fill status and broker attribution.
/// </summary>
/// <param name="Id">Unique internal identifier for the order.</param>
/// <param name="ExternalId">Broker-assigned order identifier.</param>
/// <param name="Symbol">Ticker symbol of the ordered instrument.</param>
/// <param name="Side">Direction of the order (Buy or Sell).</param>
/// <param name="Type">Order type (Market, Limit, StopLoss, StopLimit, TrailingStop).</param>
/// <param name="Status">Current lifecycle status of the order.</param>
/// <param name="Qty">Requested order quantity.</param>
/// <param name="LimitPrice">Limit price for Limit/StopLimit orders, null for Market.</param>
/// <param name="FilledQty">Quantity filled so far, null if no fills.</param>
/// <param name="FilledPrice">Volume-weighted average fill price, null if no fills.</param>
/// <param name="Broker">Broker through which the order was submitted.</param>
/// <param name="CreatedAt">Timestamp when the order was created.</param>
public sealed record OrderDto(
    Guid Id,
    string ExternalId,
    string Symbol,
    string Side,
    string Type,
    string Status,
    decimal Qty,
    decimal? LimitPrice,
    decimal? FilledQty,
    decimal? FilledPrice,
    string Broker,
    DateTimeOffset CreatedAt);

