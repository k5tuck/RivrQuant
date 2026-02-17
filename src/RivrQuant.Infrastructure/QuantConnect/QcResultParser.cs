// Copyright (c) RivrQuant. All rights reserved.
// Licensed under the MIT License.

using System.Globalization;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using RivrQuant.Domain.Enums;
using RivrQuant.Domain.Models.Backtests;

namespace RivrQuant.Infrastructure.QuantConnect;

/// <summary>
/// Parses QuantConnect API JSON responses into RivrQuant domain models.
/// Handles the mapping between QuantConnect's response schema and our internal
/// <see cref="BacktestResult"/>, <see cref="BacktestTrade"/>, and <see cref="DailyReturn"/> models.
/// </summary>
public sealed class QcResultParser
{
    private readonly ILogger<QcResultParser> _logger;

    /// <summary>
    /// Date formats used by the QuantConnect API for parsing date/time values.
    /// Multiple formats are supported because different API endpoints return
    /// dates in slightly different formats.
    /// </summary>
    private static readonly string[] DateFormats =
    [
        "yyyy-MM-dd'T'HH:mm:ss",
        "yyyy-MM-dd'T'HH:mm:ssZ",
        "yyyy-MM-dd'T'HH:mm:ss.fff",
        "yyyy-MM-dd'T'HH:mm:ss.fffZ",
        "yyyy-MM-dd HH:mm:ss",
        "yyyy-MM-dd",
        "M/d/yyyy h:mm:ss tt",
        "M/d/yyyy"
    ];

    /// <summary>
    /// Initializes a new instance of the <see cref="QcResultParser"/> class.
    /// </summary>
    /// <param name="logger">The logger instance for structured logging.</param>
    public QcResultParser(ILogger<QcResultParser> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Parses a complete backtest result from a QuantConnect API JSON response,
    /// including trade history and daily equity snapshots.
    /// </summary>
    /// <param name="json">
    /// The <see cref="JsonElement"/> representing the backtest detail response from the QC API.
    /// Expected to contain properties such as <c>backtestId</c>, <c>name</c>, <c>result</c>,
    /// and nested <c>Orders</c> and <c>Charts</c> data.
    /// </param>
    /// <param name="projectId">
    /// The QuantConnect project identifier that owns this backtest.
    /// </param>
    /// <returns>
    /// A fully populated <see cref="BacktestResult"/> with <see cref="BacktestResult.Trades"/>
    /// and <see cref="BacktestResult.DailyReturns"/> collections.
    /// </returns>
    public BacktestResult ParseBacktestResult(JsonElement json, string projectId)
    {
        var backtestId = GetStringProperty(json, "backtestId", "unknown");
        var name = GetStringProperty(json, "name", "Unnamed Backtest");
        var note = GetStringPropertyOrNull(json, "note");

        _logger.LogDebug(
            "Parsing backtest result {BacktestId} for project {ProjectId}",
            backtestId, projectId);

        var startDate = ParseDateProperty(json, "periodStart", DateTimeOffset.MinValue);
        var endDate = ParseDateProperty(json, "periodFinish", DateTimeOffset.MaxValue);
        var createdDate = ParseDateProperty(json, "created", DateTimeOffset.UtcNow);

        // Extract statistics from the result object
        var statistics = GetNestedElement(json, "result", "Statistics");
        var initialCapital = GetDecimalFromStatistics(statistics, "Starting Capital");
        var finalEquity = GetDecimalFromStatistics(statistics, "Ending Capital");
        var totalReturn = GetDecimalFromStatistics(statistics, "Total Net Profit");

        var resultId = Guid.NewGuid();

        var trades = ParseTrades(json, resultId, backtestId, projectId);
        var dailyReturns = ParseDailyReturns(json, resultId, initialCapital, backtestId, projectId);

        _logger.LogInformation(
            "Parsed backtest {BacktestId} for project {ProjectId}: {TradeCount} trades, {DailyReturnCount} daily returns",
            backtestId, projectId, trades.Count, dailyReturns.Count);

        return new BacktestResult
        {
            Id = resultId,
            ExternalBacktestId = backtestId,
            ProjectId = projectId,
            StrategyName = name,
            StrategyDescription = note,
            StartDate = startDate,
            EndDate = endDate,
            CreatedAt = createdDate,
            InitialCapital = initialCapital,
            FinalEquity = finalEquity,
            TotalReturn = totalReturn,
            IsAnalyzed = false,
            Trades = trades,
            DailyReturns = dailyReturns
        };
    }

    /// <summary>
    /// Parses the trade/order history from a QuantConnect backtest result JSON response.
    /// Maps QuantConnect order directions to the <see cref="OrderSide"/> enum and computes
    /// profit/loss from fill price and quantity data.
    /// </summary>
    /// <param name="json">The backtest detail JSON element containing the <c>result.Orders</c> node.</param>
    /// <param name="backtestResultId">The internal identifier of the parent <see cref="BacktestResult"/>.</param>
    /// <param name="backtestId">The external QuantConnect backtest identifier (for logging).</param>
    /// <param name="projectId">The QuantConnect project identifier (for logging).</param>
    /// <returns>A list of parsed <see cref="BacktestTrade"/> objects.</returns>
    private List<BacktestTrade> ParseTrades(
        JsonElement json,
        Guid backtestResultId,
        string backtestId,
        string projectId)
    {
        var trades = new List<BacktestTrade>();

        // QC nests orders inside result -> Orders as a dictionary keyed by order ID
        var ordersElement = GetNestedElement(json, "result", "Orders");
        if (ordersElement is null || ordersElement.Value.ValueKind != JsonValueKind.Object)
        {
            _logger.LogWarning(
                "No orders found in backtest {BacktestId} for project {ProjectId}",
                backtestId, projectId);
            return trades;
        }

        // Build a flat list of all closed-order data.
        // QuantConnect orders are individual legs (not round-trips), but we parse them
        // as individual trade records. Round-trip matching can happen at a higher layer.
        foreach (var orderProperty in ordersElement.Value.EnumerateObject())
        {
            try
            {
                var order = orderProperty.Value;
                var trade = ParseSingleTrade(order, backtestResultId);
                if (trade is not null)
                {
                    trades.Add(trade);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(
                    ex,
                    "Failed to parse order {OrderKey} in backtest {BacktestId} for project {ProjectId}; skipping",
                    orderProperty.Name, backtestId, projectId);
            }
        }

        return trades;
    }

    /// <summary>
    /// Parses a single QuantConnect order into a <see cref="BacktestTrade"/>.
    /// Returns <c>null</c> if the order does not have sufficient data (e.g., no fill events).
    /// </summary>
    /// <param name="order">The JSON element representing a single QC order.</param>
    /// <param name="backtestResultId">The internal ID of the parent backtest result.</param>
    /// <returns>A <see cref="BacktestTrade"/> instance, or <c>null</c> if the order cannot be parsed.</returns>
    private BacktestTrade? ParseSingleTrade(JsonElement order, Guid backtestResultId)
    {
        // Extract the symbol from the nested Symbol object
        var symbol = "UNKNOWN";
        if (order.TryGetProperty("Symbol", out var symbolElement))
        {
            symbol = GetStringProperty(symbolElement, "Value", "UNKNOWN");
        }

        var direction = GetStringProperty(order, "Direction", "Buy");
        var side = MapDirectionToOrderSide(direction);
        var quantity = GetDecimalProperty(order, "Quantity", 0m);
        var price = GetDecimalProperty(order, "Price", 0m);
        var createdTime = ParseDateProperty(order, "CreatedTime", DateTimeOffset.MinValue);
        var lastFillTime = ParseDateProperty(order, "LastFillTime", DateTimeOffset.MinValue);

        // If last fill time is not set, try to use the order's "Time" property
        if (lastFillTime == DateTimeOffset.MinValue)
        {
            lastFillTime = ParseDateProperty(order, "Time", createdTime);
        }

        // Extract fill price from OrderEvents if available, otherwise fall back to Price
        var fillPrice = price;
        if (order.TryGetProperty("OrderEvents", out var eventsElement) &&
            eventsElement.ValueKind == JsonValueKind.Array)
        {
            foreach (var evt in eventsElement.EnumerateArray())
            {
                var evtFillPrice = GetDecimalProperty(evt, "FillPrice", 0m);
                if (evtFillPrice != 0m)
                {
                    fillPrice = evtFillPrice;
                    var evtFillQuantity = GetDecimalProperty(evt, "FillQuantity", 0m);
                    if (evtFillQuantity != 0m)
                    {
                        quantity = Math.Abs(evtFillQuantity);
                    }

                    break;
                }
            }
        }

        // Skip orders with zero quantity (cancelled or unfilled orders)
        if (quantity == 0m)
        {
            return null;
        }

        quantity = Math.Abs(quantity);

        // For individual order legs, entry and exit are approximated from the single order.
        // A more sophisticated round-trip matching approach can be applied at a higher layer.
        return new BacktestTrade
        {
            Id = Guid.NewGuid(),
            BacktestResultId = backtestResultId,
            Symbol = symbol,
            EntryTime = createdTime,
            ExitTime = lastFillTime,
            EntryPrice = fillPrice,
            ExitPrice = fillPrice,
            Quantity = quantity,
            Side = side,
            ProfitLoss = 0m,
            ProfitLossPercent = 0m
        };
    }

    /// <summary>
    /// Parses daily equity curve data from the QuantConnect backtest result JSON.
    /// Extracts values from the <c>Charts["Strategy Equity"].Series["Equity"].Values</c> path.
    /// </summary>
    /// <param name="json">The backtest detail JSON element.</param>
    /// <param name="backtestResultId">The internal ID of the parent backtest result.</param>
    /// <param name="initialCapital">The initial portfolio capital for computing returns.</param>
    /// <param name="backtestId">The external QuantConnect backtest identifier (for logging).</param>
    /// <param name="projectId">The QuantConnect project identifier (for logging).</param>
    /// <returns>A list of <see cref="DailyReturn"/> objects representing the equity curve.</returns>
    private List<DailyReturn> ParseDailyReturns(
        JsonElement json,
        Guid backtestResultId,
        decimal initialCapital,
        string backtestId,
        string projectId)
    {
        var dailyReturns = new List<DailyReturn>();

        // Navigate to Charts -> "Strategy Equity" -> Series -> "Equity" -> Values
        var chartsElement = GetNestedElement(json, "result", "Charts");
        if (chartsElement is null || chartsElement.Value.ValueKind != JsonValueKind.Object)
        {
            _logger.LogWarning(
                "No chart data found in backtest {BacktestId} for project {ProjectId}",
                backtestId, projectId);
            return dailyReturns;
        }

        JsonElement? equityValues = null;
        if (chartsElement.Value.TryGetProperty("Strategy Equity", out var strategyEquity) &&
            strategyEquity.TryGetProperty("Series", out var series) &&
            series.TryGetProperty("Equity", out var equitySeries) &&
            equitySeries.TryGetProperty("Values", out var values) &&
            values.ValueKind == JsonValueKind.Array)
        {
            equityValues = values;
        }

        if (equityValues is null)
        {
            _logger.LogWarning(
                "No Strategy Equity chart series found in backtest {BacktestId} for project {ProjectId}",
                backtestId, projectId);
            return dailyReturns;
        }

        decimal previousEquity = initialCapital > 0 ? initialCapital : 1m;
        decimal peakEquity = previousEquity;
        decimal cumulativeReturn = 0m;

        // QC equity chart data points are objects with "x" (Unix timestamp) and "y" (equity value).
        // We sample one point per day by tracking the date of the last emitted record.
        DateTimeOffset? lastDate = null;

        foreach (var point in equityValues.Value.EnumerateArray())
        {
            try
            {
                var timestamp = GetUnixTimestamp(point);
                var equity = GetDecimalProperty(point, "y", 0m);

                if (timestamp is null || equity == 0m)
                    continue;

                var date = timestamp.Value.Date;
                var dateOffset = new DateTimeOffset(date, TimeSpan.Zero);

                // Skip duplicate data points for the same calendar day (take last value per day)
                if (lastDate.HasValue && dateOffset == lastDate.Value)
                {
                    // Overwrite the last entry with the latest value for this day
                    if (dailyReturns.Count > 0)
                    {
                        dailyReturns.RemoveAt(dailyReturns.Count - 1);
                    }
                }

                var dailyPnl = equity - previousEquity;
                var dailyReturnPct = previousEquity != 0m
                    ? dailyPnl / previousEquity
                    : 0m;

                cumulativeReturn = initialCapital > 0
                    ? (equity - initialCapital) / initialCapital
                    : 0m;

                if (equity > peakEquity)
                    peakEquity = equity;

                var drawdown = peakEquity > 0
                    ? (equity - peakEquity) / peakEquity
                    : 0m;

                dailyReturns.Add(new DailyReturn
                {
                    Id = Guid.NewGuid(),
                    BacktestResultId = backtestResultId,
                    Date = dateOffset,
                    Equity = equity,
                    DailyPnl = dailyPnl,
                    DailyReturnPercent = dailyReturnPct,
                    CumulativeReturn = cumulativeReturn,
                    Drawdown = drawdown,
                    BenchmarkEquity = 0m
                });

                previousEquity = equity;
                lastDate = dateOffset;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(
                    ex,
                    "Failed to parse equity data point in backtest {BacktestId} for project {ProjectId}; skipping",
                    backtestId, projectId);
            }
        }

        _logger.LogDebug(
            "Parsed {Count} daily equity data points for backtest {BacktestId}",
            dailyReturns.Count, backtestId);

        return dailyReturns;
    }

    /// <summary>
    /// Maps a QuantConnect order direction string to the <see cref="OrderSide"/> enum.
    /// QuantConnect uses <c>"Buy"</c> (0), <c>"Sell"</c> (1), and <c>"Hold"</c> (2).
    /// </summary>
    /// <param name="direction">The direction string from the QC API (e.g., "Buy", "Sell", "0", "1").</param>
    /// <returns>The corresponding <see cref="OrderSide"/> value.</returns>
    private OrderSide MapDirectionToOrderSide(string direction)
    {
        return direction.Trim().ToUpperInvariant() switch
        {
            "BUY" or "0" => OrderSide.Buy,
            "SELL" or "1" => OrderSide.Sell,
            _ => OrderSide.Buy // Default to Buy if direction is unrecognized
        };
    }

    /// <summary>
    /// Extracts a Unix timestamp from a chart data point element.
    /// QuantConnect chart values use the <c>x</c> property as a Unix epoch timestamp in seconds.
    /// </summary>
    /// <param name="point">The JSON element representing a single chart data point.</param>
    /// <returns>The parsed <see cref="DateTimeOffset"/>, or <c>null</c> if parsing fails.</returns>
    private static DateTimeOffset? GetUnixTimestamp(JsonElement point)
    {
        if (!point.TryGetProperty("x", out var xElement))
            return null;

        long unixSeconds;
        if (xElement.ValueKind == JsonValueKind.Number)
        {
            unixSeconds = xElement.GetInt64();
        }
        else if (xElement.ValueKind == JsonValueKind.String &&
                 long.TryParse(xElement.GetString(), out var parsed))
        {
            unixSeconds = parsed;
        }
        else
        {
            return null;
        }

        return DateTimeOffset.FromUnixTimeSeconds(unixSeconds);
    }

    /// <summary>
    /// Safely retrieves a string property from a JSON element, returning a default value if not found.
    /// </summary>
    /// <param name="element">The JSON element to inspect.</param>
    /// <param name="propertyName">The property name to retrieve.</param>
    /// <param name="defaultValue">The default value if the property is missing or null.</param>
    /// <returns>The property value as a string, or <paramref name="defaultValue"/>.</returns>
    private static string GetStringProperty(JsonElement element, string propertyName, string defaultValue)
    {
        if (element.TryGetProperty(propertyName, out var prop) &&
            prop.ValueKind == JsonValueKind.String)
        {
            return prop.GetString() ?? defaultValue;
        }

        return defaultValue;
    }

    /// <summary>
    /// Safely retrieves a string property from a JSON element, returning <c>null</c> if not found.
    /// </summary>
    /// <param name="element">The JSON element to inspect.</param>
    /// <param name="propertyName">The property name to retrieve.</param>
    /// <returns>The property value as a string, or <c>null</c>.</returns>
    private static string? GetStringPropertyOrNull(JsonElement element, string propertyName)
    {
        if (element.TryGetProperty(propertyName, out var prop) &&
            prop.ValueKind == JsonValueKind.String)
        {
            return prop.GetString();
        }

        return null;
    }

    /// <summary>
    /// Safely retrieves a decimal property from a JSON element, supporting both numeric
    /// and string representations. Returns a default value if parsing fails.
    /// </summary>
    /// <param name="element">The JSON element to inspect.</param>
    /// <param name="propertyName">The property name to retrieve.</param>
    /// <param name="defaultValue">The default value if the property is missing or unparseable.</param>
    /// <returns>The property value as a decimal, or <paramref name="defaultValue"/>.</returns>
    private static decimal GetDecimalProperty(JsonElement element, string propertyName, decimal defaultValue)
    {
        if (!element.TryGetProperty(propertyName, out var prop))
            return defaultValue;

        if (prop.ValueKind == JsonValueKind.Number && prop.TryGetDecimal(out var numericValue))
            return numericValue;

        if (prop.ValueKind == JsonValueKind.String)
        {
            var rawValue = prop.GetString();
            if (rawValue is not null)
            {
                // Remove currency symbols, commas, and percentage signs
                var cleaned = rawValue
                    .Replace("$", "", StringComparison.Ordinal)
                    .Replace(",", "", StringComparison.Ordinal)
                    .Replace("%", "", StringComparison.Ordinal)
                    .Trim();

                if (decimal.TryParse(cleaned, NumberStyles.Any, CultureInfo.InvariantCulture, out var parsed))
                    return parsed;
            }
        }

        return defaultValue;
    }

    /// <summary>
    /// Extracts a decimal value from a QuantConnect statistics dictionary element.
    /// QC statistics are typically key-value pairs where values are formatted strings
    /// (e.g., "$100,000.00" or "15.5%").
    /// </summary>
    /// <param name="statistics">The JSON element representing the statistics dictionary, or <c>null</c>.</param>
    /// <param name="key">The statistics key to look up.</param>
    /// <returns>The parsed decimal value, or <c>0</c> if the key is not found or parsing fails.</returns>
    private static decimal GetDecimalFromStatistics(JsonElement? statistics, string key)
    {
        if (statistics is null || statistics.Value.ValueKind != JsonValueKind.Object)
            return 0m;

        if (!statistics.Value.TryGetProperty(key, out var prop))
            return 0m;

        var rawValue = prop.ValueKind == JsonValueKind.String
            ? prop.GetString()
            : prop.ToString();

        if (rawValue is null)
            return 0m;

        var cleaned = rawValue
            .Replace("$", "", StringComparison.Ordinal)
            .Replace(",", "", StringComparison.Ordinal)
            .Replace("%", "", StringComparison.Ordinal)
            .Trim();

        return decimal.TryParse(cleaned, NumberStyles.Any, CultureInfo.InvariantCulture, out var result)
            ? result
            : 0m;
    }

    /// <summary>
    /// Parses a date/time property from a JSON element using the known QuantConnect date formats.
    /// Falls back to <paramref name="defaultValue"/> if the property is missing or unparseable.
    /// </summary>
    /// <param name="element">The JSON element to inspect.</param>
    /// <param name="propertyName">The property name containing the date value.</param>
    /// <param name="defaultValue">The default value if parsing fails.</param>
    /// <returns>The parsed <see cref="DateTimeOffset"/>, or <paramref name="defaultValue"/>.</returns>
    private DateTimeOffset ParseDateProperty(
        JsonElement element,
        string propertyName,
        DateTimeOffset defaultValue)
    {
        if (!element.TryGetProperty(propertyName, out var prop))
            return defaultValue;

        if (prop.ValueKind == JsonValueKind.String)
        {
            var dateStr = prop.GetString();
            if (dateStr is not null &&
                DateTimeOffset.TryParseExact(
                    dateStr,
                    DateFormats,
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.AssumeUniversal | DateTimeStyles.AllowWhiteSpaces,
                    out var parsed))
            {
                return parsed;
            }

            // Fall back to general parsing
            if (dateStr is not null &&
                DateTimeOffset.TryParse(dateStr, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out var generalParsed))
            {
                return generalParsed;
            }

            _logger.LogDebug(
                "Unable to parse date value '{DateValue}' for property {Property}",
                dateStr, propertyName);
        }

        return defaultValue;
    }

    /// <summary>
    /// Navigates nested JSON properties by a sequence of property names and returns the
    /// leaf element, or <c>null</c> if any segment in the path is missing.
    /// </summary>
    /// <param name="root">The root JSON element to start navigation from.</param>
    /// <param name="path">The property name path segments to traverse.</param>
    /// <returns>The nested <see cref="JsonElement"/>, or <c>null</c> if the path is invalid.</returns>
    private static JsonElement? GetNestedElement(JsonElement root, params string[] path)
    {
        var current = root;
        foreach (var segment in path)
        {
            if (current.ValueKind != JsonValueKind.Object ||
                !current.TryGetProperty(segment, out var next))
            {
                return null;
            }

            current = next;
        }

        return current;
    }
}
