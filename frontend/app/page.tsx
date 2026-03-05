"use client";

import { useState, useEffect } from "react";
import { api } from "@/lib/api";
import { formatCurrency, formatPercent, formatNumber, formatDateTime } from "@/lib/formatters";
import type { DashboardDto } from "@/lib/types";

export default function DashboardPage() {
  const [data, setData] = useState<DashboardDto | null>(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    api.dashboard.get().then(setData).catch(console.error).finally(() => setLoading(false));
  }, []);

  if (loading) return <div className="animate-pulse text-[hsl(var(--muted-foreground))]">Loading dashboard...</div>;

  const portfolio = data?.portfolio;
  const metrics = data?.metrics;

  return (
    <div className="space-y-6">
      <h2 className="text-2xl font-bold">Portfolio Overview</h2>

      {/* Portfolio Summary */}
      <div className="rounded-xl border border-[hsl(var(--border))] bg-[hsl(var(--card))] p-6">
        <p className="text-sm text-[hsl(var(--muted-foreground))]">Total Portfolio Value</p>
        <p className="text-3xl font-bold">{formatCurrency(portfolio?.totalEquity ?? 0)}</p>
        <p className={`text-sm ${(portfolio?.dailyChangePercent ?? 0) >= 0 ? "text-profit" : "text-loss"}`}>
          {formatCurrency(portfolio?.dailyChange ?? 0)} ({formatPercent((portfolio?.dailyChangePercent ?? 0) / 100)}) today
        </p>
      </div>

      {/* Metrics Grid */}
      <div className="grid grid-cols-2 md:grid-cols-3 lg:grid-cols-6 gap-4">
        {[
          { label: "Sharpe (30d)", value: formatNumber(metrics?.liveSharpe30d ?? 0, 3) },
          { label: "Win Rate (30)", value: formatPercent((metrics?.winRate30 ?? 0) / 100) },
          { label: "Max Drawdown", value: formatPercent(metrics?.currentDrawdown ?? 0) },
          { label: "Open Positions", value: String(metrics?.openPositions ?? 0) },
          { label: "Today P&L", value: formatCurrency(metrics?.todaysPnl ?? 0), color: (metrics?.todaysPnl ?? 0) >= 0 },
          { label: "Available Cash", value: formatCurrency(metrics?.availableCash ?? 0) },
        ].map((m) => (
          <div key={m.label} className="rounded-lg border border-[hsl(var(--border))] bg-[hsl(var(--card))] p-4">
            <p className="text-xs text-[hsl(var(--muted-foreground))]">{m.label}</p>
            <p className={`text-lg font-semibold ${m.color !== undefined ? (m.color ? "text-profit" : "text-loss") : ""}`}>
              {m.value}
            </p>
          </div>
        ))}
      </div>

      {/* Recent Trades */}
      <div className="rounded-xl border border-[hsl(var(--border))] bg-[hsl(var(--card))] p-6">
        <h3 className="text-lg font-semibold mb-4">Recent Trades</h3>
        <div className="overflow-x-auto">
          <table className="w-full text-sm">
            <thead>
              <tr className="border-b border-[hsl(var(--border))] text-[hsl(var(--muted-foreground))]">
                <th className="pb-2 text-left">Time</th>
                <th className="pb-2 text-left">Symbol</th>
                <th className="pb-2 text-left">Side</th>
                <th className="pb-2 text-right">Qty</th>
                <th className="pb-2 text-right">Price</th>
                <th className="pb-2 text-left">Broker</th>
                <th className="pb-2 text-left">Status</th>
              </tr>
            </thead>
            <tbody>
              {(data?.recentTrades ?? []).slice(0, 20).map((t) => (
                <tr key={t.id} className="border-b border-[hsl(var(--border))] hover:bg-[hsl(var(--accent))]">
                  <td className="py-2">{formatDateTime(t.createdAt)}</td>
                  <td className="font-medium">{t.symbol}</td>
                  <td className={t.side === "Buy" ? "text-profit" : "text-loss"}>{t.side}</td>
                  <td className="text-right">{t.qty}</td>
                  <td className="text-right">{t.filledPrice ? formatCurrency(t.filledPrice) : "—"}</td>
                  <td>{t.broker}</td>
                  <td><span className="text-xs px-2 py-0.5 rounded-full bg-[hsl(var(--accent))]">{t.status}</span></td>
                </tr>
              ))}
              {(!data?.recentTrades || data.recentTrades.length === 0) && (
                <tr><td colSpan={7} className="py-8 text-center text-[hsl(var(--muted-foreground))]">No recent trades</td></tr>
              )}
            </tbody>
          </table>
        </div>
      </div>

      {/* Recent Alerts */}
      {data?.recentAlerts && data.recentAlerts.length > 0 && (
        <div className="rounded-xl border border-warning/20 bg-warning/5 p-4">
          <h3 className="text-sm font-semibold text-warning mb-2">Recent Alerts</h3>
          {data.recentAlerts.slice(0, 3).map((a) => (
            <div key={a.id} className="text-sm py-1">
              <span className={`font-medium ${a.severity === "Critical" ? "text-loss" : "text-warning"}`}>
                [{a.severity}]
              </span>{" "}
              {a.message}
            </div>
          ))}
        </div>
      )}
    </div>
  );
}
