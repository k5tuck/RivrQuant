"use client";
import { useState, useEffect } from "react";
import { api } from "@/lib/api";
import { formatNumber, formatPercent } from "@/lib/formatters";
import type { BacktestSummaryDto } from "@/lib/types";

export default function ComparePage() {
  const [backtests, setBacktests] = useState<BacktestSummaryDto[]>([]);
  const [selected, setSelected] = useState<Set<string>>(new Set());
  const [loading, setLoading] = useState(true);

  useEffect(() => { api.backtests.list().then(setBacktests).catch(console.error).finally(() => setLoading(false)); }, []);

  const toggle = (id: string) => {
    const s = new Set(selected);
    s.has(id) ? s.delete(id) : s.add(id);
    setSelected(s);
  };

  if (loading) return <div className="animate-pulse text-[hsl(var(--muted-foreground))]">Loading...</div>;

  return (
    <div className="space-y-6">
      <h2 className="text-2xl font-bold">Compare Backtests</h2>
      <p className="text-sm text-[hsl(var(--muted-foreground))]">Select 2-4 backtests to compare side by side.</p>
      <div className="grid gap-3">
        {backtests.map((bt) => (
          <div key={bt.id} onClick={() => toggle(bt.id)}
            className={`p-4 rounded-lg border cursor-pointer transition ${selected.has(bt.id) ? "border-[hsl(var(--primary))] bg-[hsl(var(--primary))]/10" : "border-[hsl(var(--border))] bg-[hsl(var(--card))]"}`}>
            <div className="flex items-center justify-between">
              <span className="font-medium">{bt.strategyName}</span>
              <div className="flex gap-4 text-sm text-[hsl(var(--muted-foreground))]">
                <span>Sharpe: {formatNumber(bt.sharpeRatio, 3)}</span>
                <span>DD: {formatPercent(bt.maxDrawdown)}</span>
                <span>Return: {formatPercent(Number(bt.totalReturn))}</span>
              </div>
            </div>
          </div>
        ))}
      </div>
      {selected.size >= 2 && (
        <button onClick={() => api.backtests.compare(Array.from(selected))}
          className="px-6 py-2 rounded-lg bg-[hsl(var(--primary))] text-[hsl(var(--primary-foreground))]">
          Compare {selected.size} Backtests
        </button>
      )}
    </div>
  );
}
