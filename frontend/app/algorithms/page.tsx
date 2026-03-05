"use client";

import { useEffect, useState } from "react";
import { api } from "@/lib/api";
import type { AlgorithmSummaryDto } from "@/lib/types";
import { formatPercent, formatNumber } from "@/lib/formatters";

function SharpeBar({ value }: { value?: number }) {
  if (value == null) return <span className="text-xs text-[hsl(var(--muted-foreground))]">—</span>;
  const pct = Math.min(Math.max((value / 3) * 100, 0), 100);
  const color = value >= 1.5 ? "bg-profit" : value >= 0.5 ? "bg-warning" : "bg-loss";
  return (
    <div className="flex items-center gap-2">
      <div className="flex-1 h-1.5 rounded-full bg-[hsl(var(--muted))]">
        <div className={`h-1.5 rounded-full ${color}`} style={{ width: `${pct}%` }} />
      </div>
      <span className={`text-sm font-semibold tabular-nums ${value >= 1.5 ? "text-profit" : value >= 0.5 ? "text-warning" : "text-loss"}`}>
        {formatNumber(value)}
      </span>
    </div>
  );
}

export default function AlgorithmsPage() {
  const [algorithms, setAlgorithms] = useState<AlgorithmSummaryDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    api.backtests.projects()
      .then(setAlgorithms)
      .catch((e) => setError(e.message))
      .finally(() => setLoading(false));
  }, []);

  if (loading) {
    return (
      <div className="flex items-center justify-center h-64">
        <div className="text-sm text-[hsl(var(--muted-foreground))]">Loading algorithms…</div>
      </div>
    );
  }

  if (error) {
    return (
      <div className="rounded-xl border border-loss/20 bg-loss/5 p-6 text-sm text-loss">{error}</div>
    );
  }

  return (
    <div className="space-y-6">
      {/* Header */}
      <div>
        <h1 className="text-2xl font-bold text-[hsl(var(--foreground))]">Algorithms</h1>
        <p className="text-sm text-[hsl(var(--muted-foreground))] mt-1">
          One row per QuantConnect project. Each project is a distinct algorithm or bot you've
          built and backtested. Click a row to see all of its backtest runs.
        </p>
      </div>

      {/* How it works callout */}
      <div className="rounded-xl border border-profit/20 bg-profit/5 p-4 text-sm">
        <p className="font-semibold text-profit mb-1">How to see your algorithms here</p>
        <ol className="list-decimal list-inside space-y-1 text-[hsl(var(--foreground))]">
          <li>Add each algorithm's <strong>Project ID</strong> (from the QuantConnect URL) to <code className="bg-[hsl(var(--muted))] px-1 rounded text-xs">QC_PROJECT_IDS</code> in your <code className="bg-[hsl(var(--muted))] px-1 rounded text-xs">.env</code> file.</li>
          <li>The poller fetches the project name automatically from QuantConnect.</li>
          <li>All completed backtests import every 5 minutes — or trigger one now in the Hangfire dashboard (<code className="bg-[hsl(var(--muted))] px-1 rounded text-xs">/hangfire</code>).</li>
        </ol>
      </div>

      {algorithms.length === 0 ? (
        <div className="rounded-xl border border-[hsl(var(--border))] bg-[hsl(var(--card))] p-12 text-center">
          <p className="text-[hsl(var(--muted-foreground))] text-sm">No algorithms imported yet.</p>
          <p className="text-[hsl(var(--muted-foreground))] text-xs mt-1">
            Add project IDs to <code className="bg-[hsl(var(--muted))] px-1 rounded">QC_PROJECT_IDS</code> and restart the API.
          </p>
        </div>
      ) : (
        <div className="grid gap-4 sm:grid-cols-2 xl:grid-cols-3">
          {algorithms.map((algo) => (
            <a
              key={algo.projectId}
              href={`/backtests?project=${algo.projectId}`}
              className="block rounded-xl border border-[hsl(var(--border))] bg-[hsl(var(--card))] p-5 hover:border-profit/40 hover:shadow-sm transition-all duration-150 group"
            >
              {/* Name + status */}
              <div className="flex items-start justify-between gap-2 mb-4">
                <div>
                  <h2 className="font-semibold text-[hsl(var(--foreground))] group-hover:text-profit transition-colors leading-snug">
                    {algo.projectName}
                  </h2>
                  <p className="text-xs text-[hsl(var(--muted-foreground))] mt-0.5">
                    QC Project <span className="font-mono">{algo.projectId}</span>
                  </p>
                </div>
                <span className={`shrink-0 text-xs font-medium px-2 py-0.5 rounded-full ${
                  algo.analyzedCount === algo.backtestCount && algo.backtestCount > 0
                    ? "bg-profit/10 text-profit"
                    : algo.analyzedCount > 0
                    ? "bg-warning/10 text-warning"
                    : "bg-[hsl(var(--muted))] text-[hsl(var(--muted-foreground))]"
                }`}>
                  {algo.analyzedCount}/{algo.backtestCount} analyzed
                </span>
              </div>

              {/* Sharpe */}
              <div className="mb-4">
                <p className="text-xs text-[hsl(var(--muted-foreground))] mb-1.5">Best Sharpe</p>
                <SharpeBar value={algo.bestSharpe} />
              </div>

              {/* Stats row */}
              <div className="grid grid-cols-3 gap-2 pt-3 border-t border-[hsl(var(--border))]">
                <div>
                  <p className="text-[10px] text-[hsl(var(--muted-foreground))] uppercase tracking-wide">Runs</p>
                  <p className="text-sm font-bold text-[hsl(var(--foreground))]">{algo.backtestCount}</p>
                </div>
                <div>
                  <p className="text-[10px] text-[hsl(var(--muted-foreground))] uppercase tracking-wide">Best Return</p>
                  <p className={`text-sm font-bold ${algo.bestTotalReturn >= 0 ? "text-profit" : "text-loss"}`}>
                    {formatPercent(algo.bestTotalReturn)}
                  </p>
                </div>
                <div>
                  <p className="text-[10px] text-[hsl(var(--muted-foreground))] uppercase tracking-wide">Latest</p>
                  <p className="text-sm font-bold text-[hsl(var(--foreground))]">
                    {new Date(algo.latestBacktest).toLocaleDateString("en-US", { month: "short", day: "numeric" })}
                  </p>
                </div>
              </div>
            </a>
          ))}
        </div>
      )}
    </div>
  );
}
