"use client";

import { useState, useEffect } from "react";
import { api } from "@/lib/api";
import type { StrategyDto } from "@/lib/types";

export default function ActiveStrategiesCard() {
  const [strategies, setStrategies] = useState<StrategyDto[]>([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    api.strategies.list()
      .then(setStrategies)
      .catch(() => setStrategies([]))
      .finally(() => setLoading(false));
  }, []);

  return (
    <div className="rounded-lg border border-[hsl(var(--border))] bg-[hsl(var(--card))] p-4">
      <h3 className="text-sm font-medium text-[hsl(var(--foreground))]">Active Strategies</h3>
      {loading ? (
        <div className="mt-3 space-y-2">
          {[1, 2, 3].map((i) => (
            <div key={i} className="h-8 animate-pulse rounded bg-[hsl(var(--muted))]" />
          ))}
        </div>
      ) : strategies.length === 0 ? (
        <p className="mt-3 text-sm text-[hsl(var(--muted-foreground))]">No strategies configured</p>
      ) : (
        <div className="mt-3 space-y-2">
          {strategies.map((s) => (
            <div key={s.id} className="flex items-center justify-between rounded-md border border-[hsl(var(--border))] px-3 py-2">
              <div className="flex items-center gap-2">
                <span className={`h-2 w-2 rounded-full ${s.isActive ? "bg-profit" : "bg-zinc-500"}`} />
                <span className="text-sm text-[hsl(var(--foreground))]">{s.name}</span>
              </div>
              <span className={`text-xs px-2 py-0.5 rounded ${s.isActive ? "bg-profit/10 text-profit" : "bg-zinc-500/10 text-zinc-400"}`}>
                {s.isActive ? "Running" : "Paused"}
              </span>
            </div>
          ))}
        </div>
      )}
    </div>
  );
}
