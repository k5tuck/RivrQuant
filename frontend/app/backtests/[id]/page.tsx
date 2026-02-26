"use client";
import { useState, useEffect } from "react";
import { useParams } from "next/navigation";
import { api } from "@/lib/api";
import { formatCurrency, formatPercent, formatNumber, formatDate, formatDateTime } from "@/lib/formatters";
import type { BacktestDetailDto } from "@/lib/types";

export default function BacktestDetailPage() {
  const params = useParams();
  const [bt, setBt] = useState<BacktestDetailDto | null>(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    if (params.id) api.backtests.get(params.id as string).then(setBt).catch(console.error).finally(() => setLoading(false));
  }, [params.id]);

  if (loading) return <div className="animate-pulse text-[hsl(var(--muted-foreground))]">Loading backtest...</div>;
  if (!bt) return <div>Backtest not found</div>;

  const metrics = [
    { label: "Sharpe Ratio", value: formatNumber(bt.sharpeRatio, 3) },
    { label: "Sortino Ratio", value: formatNumber(bt.sortinoRatio, 3) },
    { label: "Max Drawdown", value: formatPercent(bt.maxDrawdown) },
    { label: "Total Return", value: formatPercent(Number(bt.totalReturn)) },
    { label: "Profit Factor", value: formatNumber(bt.profitFactor, 3) },
    { label: "Win Rate", value: formatPercent(bt.winRate) },
  ];

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <a href="/backtests" className="text-sm text-[hsl(var(--muted-foreground))] hover:underline">Backtests</a>
          <h2 className="text-2xl font-bold">{bt.strategyName}</h2>
          <p className="text-sm text-[hsl(var(--muted-foreground))]">{formatDate(bt.dateRun)}</p>
        </div>
        <button onClick={() => api.backtests.analyze(bt.id)} className="px-4 py-2 rounded-lg bg-[hsl(var(--primary))] text-[hsl(var(--primary-foreground))] text-sm">
          Run AI Analysis
        </button>
      </div>

      <div className="grid grid-cols-2 md:grid-cols-3 lg:grid-cols-6 gap-4">
        {metrics.map((m) => (
          <div key={m.label} className="rounded-lg border border-[hsl(var(--border))] bg-[hsl(var(--card))] p-4">
            <p className="text-xs text-[hsl(var(--muted-foreground))]">{m.label}</p>
            <p className="text-lg font-semibold">{m.value}</p>
          </div>
        ))}
      </div>

      {bt.aiReport && (
        <div className="rounded-xl border border-[hsl(var(--border))] bg-[hsl(var(--card))] p-6 space-y-4">
          <h3 className="text-lg font-semibold">AI Analysis</h3>
          <div className="flex items-center gap-4">
            <div className="text-3xl font-bold text-[hsl(var(--primary))]">{bt.aiReport.deploymentReadiness}/10</div>
            <div className="text-sm text-[hsl(var(--muted-foreground))]">Deployment Readiness</div>
          </div>
          <p>{bt.aiReport.overallAssessment}</p>
          {bt.aiReport.strengths.length > 0 && (
            <div>
              <h4 className="font-medium text-profit mb-1">Strengths</h4>
              <ul className="list-disc list-inside text-sm space-y-1">{bt.aiReport.strengths.map((s, i) => <li key={i}>{s}</li>)}</ul>
            </div>
          )}
          {bt.aiReport.weaknesses.length > 0 && (
            <div>
              <h4 className="font-medium text-loss mb-1">Weaknesses</h4>
              <ul className="list-disc list-inside text-sm space-y-1">{bt.aiReport.weaknesses.map((w, i) => <li key={i}>{w}</li>)}</ul>
            </div>
          )}
          {bt.aiReport.criticalWarnings.length > 0 && (
            <div className="rounded-lg bg-loss/10 border border-loss/20 p-4">
              <h4 className="font-medium text-loss mb-1">Critical Warnings</h4>
              {bt.aiReport.criticalWarnings.map((w, i) => <p key={i} className="text-sm text-loss">{w}</p>)}
            </div>
          )}
          <p className="text-sm text-[hsl(var(--muted-foreground))]">{bt.aiReport.summary}</p>
        </div>
      )}

      <div className="rounded-xl border border-[hsl(var(--border))] bg-[hsl(var(--card))] p-6">
        <h3 className="text-lg font-semibold mb-4">Trade Log</h3>
        <table className="w-full text-sm">
          <thead>
            <tr className="border-b border-[hsl(var(--border))] text-[hsl(var(--muted-foreground))]">
              <th className="pb-2 text-left">Symbol</th><th className="pb-2 text-left">Side</th>
              <th className="pb-2 text-left">Entry</th><th className="pb-2 text-left">Exit</th>
              <th className="pb-2 text-right">Qty</th><th className="pb-2 text-right">P&L</th><th className="pb-2 text-right">P&L %</th>
            </tr>
          </thead>
          <tbody>
            {(bt.trades ?? []).slice(0, 50).map((t, i) => (
              <tr key={i} className="border-b border-[hsl(var(--border))]">
                <td className="py-1 font-medium">{t.symbol}</td>
                <td className={t.side === "Buy" ? "text-profit" : "text-loss"}>{t.side}</td>
                <td className="text-xs">{formatDateTime(t.entryTime)}</td>
                <td className="text-xs">{formatDateTime(t.exitTime)}</td>
                <td className="text-right">{t.quantity}</td>
                <td className={`text-right ${t.profitLoss >= 0 ? "text-profit" : "text-loss"}`}>{formatCurrency(t.profitLoss)}</td>
                <td className={`text-right ${t.profitLossPercent >= 0 ? "text-profit" : "text-loss"}`}>{formatPercent(t.profitLossPercent)}</td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>
    </div>
  );
}
