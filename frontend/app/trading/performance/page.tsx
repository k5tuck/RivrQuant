"use client";
import { useState, useEffect } from "react";
import { api } from "@/lib/api";
import { formatNumber, formatCurrency } from "@/lib/formatters";

interface SharpeComparison {
  liveSharpe30d: number;
  backtestSharpe: number;
  deviationPercent: number;
}

interface EquityPoint {
  date: string;
  liveEquity: number;
  backtestEquity: number;
}

interface RollingSharpePoint {
  date: string;
  liveSharpe: number;
  backtestSharpe: number;
}

function deviationBarColor(deviation: number): string {
  const abs = Math.abs(deviation);
  if (abs <= 10) return "bg-profit";
  if (abs <= 25) return "bg-warning";
  return "bg-loss";
}

function deviationTextColor(deviation: number): string {
  const abs = Math.abs(deviation);
  if (abs <= 10) return "text-profit";
  if (abs <= 25) return "text-warning";
  return "text-loss";
}

function sharpeLabel(sharpe: number): string {
  if (sharpe >= 2) return "Excellent";
  if (sharpe >= 1) return "Good";
  if (sharpe >= 0) return "Marginal";
  return "Negative";
}

function sharpeLabelColor(sharpe: number): string {
  if (sharpe >= 2) return "bg-profit/10 text-profit";
  if (sharpe >= 1) return "bg-blue-500/10 text-blue-500";
  if (sharpe >= 0) return "bg-warning/10 text-warning";
  return "bg-loss/10 text-loss";
}

export default function PerformancePage() {
  const [sharpeData, setSharpeData] = useState<SharpeComparison>({
    liveSharpe30d: 1.42,
    backtestSharpe: 1.87,
    deviationPercent: -24.06,
  });
  const [equityCurveData, setEquityCurveData] = useState<EquityPoint[]>([]);
  const [rollingSharpeData, setRollingSharpeData] = useState<RollingSharpePoint[]>([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    // Load dashboard metrics for live Sharpe; backtest Sharpe would come from
    // the most recently deployed backtest. Chart data would be populated via
    // SignalR in production — arrays remain empty as placeholders.
    api.dashboard
      .get()
      .then((dashboard) => {
        setSharpeData((prev) => ({
          ...prev,
          liveSharpe30d: dashboard.metrics.liveSharpe30d,
        }));
      })
      .catch(console.error)
      .finally(() => setLoading(false));
  }, []);

  const deviationAbs = Math.min(100, Math.abs(sharpeData.deviationPercent));
  const deviationFill = deviationAbs;

  if (loading) {
    return (
      <div className="animate-pulse text-[hsl(var(--muted-foreground))]">
        Loading performance data...
      </div>
    );
  }

  return (
    <div className="space-y-6">
      <h2 className="text-2xl font-bold">Performance: Live vs Backtest</h2>

      {/* Sharpe Ratio Comparison Card */}
      <div className="rounded-xl border border-[hsl(var(--border))] bg-[hsl(var(--card))] p-6">
        <h3 className="text-lg font-semibold mb-4">Rolling 30-Day Sharpe Ratio</h3>
        <div className="grid grid-cols-2 gap-6">
          {/* Live Sharpe */}
          <div className="space-y-2">
            <p className="text-xs font-medium text-[hsl(var(--muted-foreground))] uppercase tracking-wide">
              Live (30d)
            </p>
            <p className="text-3xl font-bold tabular-nums">
              {formatNumber(sharpeData.liveSharpe30d)}
            </p>
            <span
              className={`inline-block text-xs px-2 py-0.5 rounded-full font-medium ${sharpeLabelColor(sharpeData.liveSharpe30d)}`}
            >
              {sharpeLabel(sharpeData.liveSharpe30d)}
            </span>
          </div>

          {/* Backtest Sharpe */}
          <div className="space-y-2">
            <p className="text-xs font-medium text-[hsl(var(--muted-foreground))] uppercase tracking-wide">
              Backtest (expected)
            </p>
            <p className="text-3xl font-bold tabular-nums text-[hsl(var(--muted-foreground))]">
              {formatNumber(sharpeData.backtestSharpe)}
            </p>
            <span
              className={`inline-block text-xs px-2 py-0.5 rounded-full font-medium ${sharpeLabelColor(sharpeData.backtestSharpe)}`}
            >
              {sharpeLabel(sharpeData.backtestSharpe)}
            </span>
          </div>
        </div>
      </div>

      {/* Deviation Tracker */}
      <div className="rounded-xl border border-[hsl(var(--border))] bg-[hsl(var(--card))] p-6">
        <div className="flex items-center justify-between mb-3">
          <h3 className="text-lg font-semibold">Live vs Expected Deviation</h3>
          <span
            className={`text-sm font-bold tabular-nums ${deviationTextColor(sharpeData.deviationPercent)}`}
          >
            {sharpeData.deviationPercent > 0 ? "+" : ""}
            {formatNumber(sharpeData.deviationPercent)}%
          </span>
        </div>

        <p className="text-xs text-[hsl(var(--muted-foreground))] mb-3">
          How far live Sharpe deviates from backtest expectation. Deviation beyond
          ±25% may indicate regime change or strategy degradation.
        </p>

        <div className="space-y-2">
          {/* Bar track */}
          <div className="relative h-4 rounded-full bg-[hsl(var(--muted))] overflow-hidden">
            {/* Center marker */}
            <div className="absolute inset-y-0 left-1/2 w-px bg-[hsl(var(--border))] z-10" />
            {/* Deviation fill — anchored at center, extends left (negative) or right (positive) */}
            <div
              className={`absolute inset-y-0 rounded-full ${deviationBarColor(sharpeData.deviationPercent)}`}
              style={{
                left:
                  sharpeData.deviationPercent < 0
                    ? `${50 - deviationFill / 2}%`
                    : "50%",
                width: `${deviationFill / 2}%`,
              }}
            />
          </div>
          <div className="flex justify-between text-xs text-[hsl(var(--muted-foreground))]">
            <span>−100%</span>
            <span>0%</span>
            <span>+100%</span>
          </div>
        </div>

        {/* Threshold labels */}
        <div className="mt-3 flex gap-4 text-xs">
          <span className="flex items-center gap-1.5">
            <span className="inline-block w-2 h-2 rounded-full bg-profit" />
            Within ±10% — On track
          </span>
          <span className="flex items-center gap-1.5">
            <span className="inline-block w-2 h-2 rounded-full bg-warning" />
            ±10–25% — Monitor
          </span>
          <span className="flex items-center gap-1.5">
            <span className="inline-block w-2 h-2 rounded-full bg-loss" />
            {">"} ±25% — Review strategy
          </span>
        </div>
      </div>

      {/* Equity Curve Comparison */}
      <div className="rounded-xl border border-[hsl(var(--border))] bg-[hsl(var(--card))] p-6">
        <div className="flex items-center justify-between mb-4">
          <h3 className="text-lg font-semibold">Equity Curve Comparison</h3>
          <div className="flex items-center gap-4 text-xs text-[hsl(var(--muted-foreground))]">
            <span className="flex items-center gap-1.5">
              <span className="inline-block w-4 h-0.5 bg-blue-500" />
              Live
            </span>
            <span className="flex items-center gap-1.5">
              <span
                className="inline-block w-4 h-0.5 bg-[hsl(var(--muted-foreground))]"
                style={{ borderTop: "2px dashed" }}
              />
              Backtest
            </span>
          </div>
        </div>

        {equityCurveData.length === 0 ? (
          <div className="flex items-center justify-center h-64 rounded-lg bg-[hsl(var(--muted))] border border-dashed border-[hsl(var(--border))]">
            <div className="text-center space-y-1">
              <p className="text-sm text-[hsl(var(--muted-foreground))]">
                Equity curve chart
              </p>
              <p className="text-xs text-[hsl(var(--muted-foreground))]">
                Populated via SignalR in production
              </p>
            </div>
          </div>
        ) : (
          // Chart component would render equityCurveData here
          <div className="h-64 rounded-lg bg-[hsl(var(--muted))]" />
        )}
      </div>

      {/* Rolling Sharpe Comparison */}
      <div className="rounded-xl border border-[hsl(var(--border))] bg-[hsl(var(--card))] p-6">
        <div className="flex items-center justify-between mb-4">
          <h3 className="text-lg font-semibold">Rolling Sharpe Comparison</h3>
          <div className="flex items-center gap-4 text-xs text-[hsl(var(--muted-foreground))]">
            <span className="flex items-center gap-1.5">
              <span className="inline-block w-4 h-0.5 bg-profit" />
              Live 30d Sharpe
            </span>
            <span className="flex items-center gap-1.5">
              <span
                className="inline-block w-4 h-0.5 bg-[hsl(var(--muted-foreground))]"
                style={{ borderTop: "2px dashed" }}
              />
              Backtest Sharpe
            </span>
          </div>
        </div>

        {rollingSharpeData.length === 0 ? (
          <div className="flex items-center justify-center h-64 rounded-lg bg-[hsl(var(--muted))] border border-dashed border-[hsl(var(--border))]">
            <div className="text-center space-y-1">
              <p className="text-sm text-[hsl(var(--muted-foreground))]">
                Rolling Sharpe chart
              </p>
              <p className="text-xs text-[hsl(var(--muted-foreground))]">
                Populated via SignalR in production
              </p>
            </div>
          </div>
        ) : (
          // Chart component would render rollingSharpeData here
          <div className="h-64 rounded-lg bg-[hsl(var(--muted))]" />
        )}
      </div>
    </div>
  );
}
