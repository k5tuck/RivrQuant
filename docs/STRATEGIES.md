# Strategy Guide

## Overview

RivrQuant integrates with QuantConnect LEAN for strategy backtesting. Strategies are written in C# and executed on QuantConnect's cloud infrastructure. RivrQuant polls for completed backtests and runs AI-powered analysis.

## Writing a Strategy

Strategies are developed in the QuantConnect IDE or locally using the LEAN CLI. Here's a minimal example:

```csharp
public class MomentumAlpha : QCAlgorithm
{
    private Symbol _spy;

    public override void Initialize()
    {
        SetStartDate(2020, 1, 1);
        SetEndDate(2024, 1, 1);
        SetCash(100000);

        _spy = AddEquity("SPY", Resolution.Daily).Symbol;

        // Schedule rebalancing
        Schedule.On(DateRules.WeekStart(), TimeRules.AfterMarketOpen(_spy, 30),
            () => Rebalance());
    }

    private void Rebalance()
    {
        var history = History(_spy, 200, Resolution.Daily);
        var returns = history.Select(b => (double)b.Close).ToArray();

        // Simple momentum: buy if 50-day SMA > 200-day SMA
        var sma50 = returns.TakeLast(50).Average();
        var sma200 = returns.Average();

        if (sma50 > sma200)
            SetHoldings(_spy, 1.0);
        else
            Liquidate(_spy);
    }
}
```

## Connecting to RivrQuant

1. Create your strategy on QuantConnect
2. Note your **Project ID** from the QuantConnect dashboard URL
3. Add the Project ID to `QC_PROJECT_IDS` (comma-separated for multiple)
4. RivrQuant automatically polls for new backtest results

## AI Analysis Pipeline

When a new backtest is detected, RivrQuant automatically:

1. **Calculates statistics** — Sharpe, Sortino, Max Drawdown, VaR, Profit Factor, Calmar, Beta
2. **Detects market regimes** — Classifies periods as Trending, Mean-Reverting, High/Low Volatility, or Crisis
3. **Runs AI analysis** — Sends metrics to Claude for interpretation, overfitting assessment, and deployment readiness scoring

## Analysis Report Fields

| Field | Description |
|-------|-------------|
| Overall Assessment | AI's summary judgment (Strong/Moderate/Weak) |
| Deployment Readiness | 1-10 score for live deployment suitability |
| Overfitting Risk | Low/Medium/High assessment |
| Strengths | List of strategy strengths |
| Weaknesses | List of strategy weaknesses |
| Critical Warnings | Urgent issues requiring attention |
| Parameter Suggestions | AI-recommended parameter adjustments |

## Best Practices

1. **Use walk-forward validation** — Check for Sharpe decay between in-sample and out-of-sample periods
2. **Monitor regime performance** — Ensure the strategy performs across different market conditions
3. **Start with paper trading** — Deploy to Alpaca paper or Bybit testnet before live trading
4. **Set alerts** — Configure drawdown and daily loss alerts before going live
5. **Compare backtests** — Use the comparison view to evaluate strategy iterations
