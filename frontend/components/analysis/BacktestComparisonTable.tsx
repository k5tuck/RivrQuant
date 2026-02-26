"use client";

import { formatNumber, formatPercent, formatCurrency } from "@/lib/formatters";
import type { BacktestDetailDto } from "@/lib/types";

interface BacktestComparisonTableProps {
  backtests: BacktestDetailDto[];
}

export default function BacktestComparisonTable({ backtests }: BacktestComparisonTableProps) {
  if (backtests.length === 0) {
    return (
      <div className="rounded-lg border border-[hsl(var(--border))] bg-[hsl(var(--card))] p-6 text-center text-sm text-[hsl(var(--muted-foreground))]">
        Select backtests to compare
      </div>
    );
  }

  const metrics = [
    { label: "Strategy", getValue: (b: BacktestDetailDto) => b.strategyName },
    { label: "Sharpe Ratio", getValue: (b: BacktestDetailDto) => formatNumber(b.sharpeRatio) },
    { label: "Sortino Ratio", getValue: (b: BacktestDetailDto) => formatNumber(b.sortinoRatio) },
    { label: "Total Return", getValue: (b: BacktestDetailDto) => formatPercent(b.totalReturn) },
    { label: "Max Drawdown", getValue: (b: BacktestDetailDto) => formatPercent(b.maxDrawdown) },
    { label: "Win Rate", getValue: (b: BacktestDetailDto) => formatPercent(b.winRate) },
    { label: "Profit Factor", getValue: (b: BacktestDetailDto) => formatNumber(b.profitFactor) },
    { label: "Calmar Ratio", getValue: (b: BacktestDetailDto) => formatNumber(b.calmarRatio) },
    { label: "Total Trades", getValue: (b: BacktestDetailDto) => String(b.totalTrades) },
    { label: "Avg Win", getValue: (b: BacktestDetailDto) => formatCurrency(b.avgWin) },
    { label: "Avg Loss", getValue: (b: BacktestDetailDto) => formatCurrency(b.avgLoss) },
    { label: "AI Score", getValue: (b: BacktestDetailDto) => b.aiScore !== undefined && b.aiScore !== null ? formatNumber(b.aiScore) : "\u2014" },
  ];

  const colors = ["text-blue-400", "text-profit", "text-warning", "text-purple-400"];

  return (
    <div className="rounded-lg border border-[hsl(var(--border))] bg-[hsl(var(--card))] overflow-hidden">
      <table className="w-full text-sm">
        <thead>
          <tr className="border-b border-[hsl(var(--border))]">
            <th className="px-4 py-3 text-left text-xs text-[hsl(var(--muted-foreground))]">Metric</th>
            {backtests.map((b, i) => (
              <th key={b.id} className={`px-4 py-3 text-right text-xs ${colors[i % colors.length]}`}>
                {b.strategyName}
              </th>
            ))}
          </tr>
        </thead>
        <tbody>
          {metrics.map((metric) => (
            <tr key={metric.label} className="border-b border-[hsl(var(--border))] last:border-0">
              <td className="px-4 py-2.5 text-[hsl(var(--muted-foreground))]">{metric.label}</td>
              {backtests.map((b, i) => (
                <td key={b.id} className={`px-4 py-2.5 text-right font-medium ${colors[i % colors.length]}`}>
                  {metric.getValue(b)}
                </td>
              ))}
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
}
