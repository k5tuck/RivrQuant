"use client";
import { useState, useEffect } from "react";
import { api } from "@/lib/api";
import { formatDate, formatPercent, formatNumber } from "@/lib/formatters";
import type { BacktestSummaryDto } from "@/lib/types";

export default function BacktestsPage() {
  const [backtests, setBacktests] = useState<BacktestSummaryDto[]>([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    api.backtests.list().then(setBacktests).catch(console.error).finally(() => setLoading(false));
  }, []);

  if (loading) return <div className="animate-pulse text-[hsl(var(--muted-foreground))]">Loading backtests...</div>;

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <h2 className="text-2xl font-bold">Backtests</h2>
        <a href="/backtests/compare" className="px-4 py-2 rounded-lg bg-[hsl(var(--primary))] text-[hsl(var(--primary-foreground))] text-sm">
          Compare Selected
        </a>
      </div>
      <div className="rounded-xl border border-[hsl(var(--border))] bg-[hsl(var(--card))] overflow-hidden">
        <table className="w-full text-sm">
          <thead>
            <tr className="border-b border-[hsl(var(--border))] bg-[hsl(var(--muted))]">
              <th className="p-3 text-left">Strategy</th>
              <th className="p-3 text-left">Date</th>
              <th className="p-3 text-right">Sharpe</th>
              <th className="p-3 text-right">Max DD</th>
              <th className="p-3 text-right">Return</th>
              <th className="p-3 text-right">Win Rate</th>
              <th className="p-3 text-right">AI Score</th>
              <th className="p-3 text-center">Status</th>
            </tr>
          </thead>
          <tbody>
            {backtests.map((bt) => (
              <tr key={bt.id} className="border-b border-[hsl(var(--border))] hover:bg-[hsl(var(--accent))] cursor-pointer"
                onClick={() => window.location.href = `/backtests/${bt.id}`}>
                <td className="p-3 font-medium">{bt.strategyName}</td>
                <td className="p-3 text-[hsl(var(--muted-foreground))]">{formatDate(bt.dateRun)}</td>
                <td className="p-3 text-right">{formatNumber(bt.sharpeRatio, 3)}</td>
                <td className="p-3 text-right text-loss">{formatPercent(bt.maxDrawdown)}</td>
                <td className={`p-3 text-right ${bt.totalReturn >= 0 ? "text-profit" : "text-loss"}`}>
                  {formatPercent(Number(bt.totalReturn))}
                </td>
                <td className="p-3 text-right">{formatPercent(bt.winRate)}</td>
                <td className="p-3 text-right">{bt.aiScore ?? "—"}</td>
                <td className="p-3 text-center">
                  <span className={`text-xs px-2 py-1 rounded-full ${bt.isAnalyzed ? "bg-profit/10 text-profit" : "bg-warning/10 text-warning"}`}>
                    {bt.isAnalyzed ? "Analyzed" : "Pending"}
                  </span>
                </td>
              </tr>
            ))}
            {backtests.length === 0 && (
              <tr><td colSpan={8} className="p-8 text-center text-[hsl(var(--muted-foreground))]">No backtests found</td></tr>
            )}
          </tbody>
        </table>
      </div>
    </div>
  );
}
