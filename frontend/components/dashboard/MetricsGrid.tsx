"use client";

import { formatCurrency, formatPercent, formatNumber } from "@/lib/formatters";
import type { MetricsDto } from "@/lib/types";

interface MetricsGridProps {
  metrics: MetricsDto;
}

export default function MetricsGrid({ metrics }: MetricsGridProps) {
  const items = [
    { label: "Live Sharpe (30d)", value: formatNumber(metrics.liveSharpe30d), color: metrics.liveSharpe30d >= 1 ? "text-profit" : metrics.liveSharpe30d >= 0 ? "text-warning" : "text-loss" },
    { label: "Win Rate (30)", value: formatPercent(metrics.winRate30), color: metrics.winRate30 >= 0.5 ? "text-profit" : "text-loss" },
    { label: "Current Drawdown", value: formatPercent(metrics.currentDrawdown), color: metrics.currentDrawdown > -0.05 ? "text-profit" : metrics.currentDrawdown > -0.1 ? "text-warning" : "text-loss" },
    { label: "Open Positions", value: String(metrics.openPositions), color: "text-[hsl(var(--foreground))]" },
    { label: "Today's P&L", value: formatCurrency(metrics.todaysPnl), color: metrics.todaysPnl >= 0 ? "text-profit" : "text-loss" },
    { label: "Available Cash", value: formatCurrency(metrics.availableCash), color: "text-[hsl(var(--foreground))]" },
  ];

  return (
    <div className="grid grid-cols-2 gap-3 sm:grid-cols-3 lg:grid-cols-6">
      {items.map((item) => (
        <div key={item.label} className="rounded-lg border border-[hsl(var(--border))] bg-[hsl(var(--card))] p-4">
          <p className="text-xs text-[hsl(var(--muted-foreground))]">{item.label}</p>
          <p className={`mt-1 text-lg font-bold ${item.color}`}>{item.value}</p>
        </div>
      ))}
    </div>
  );
}
