"use client";
import { useState, useEffect } from "react";
import { api } from "@/lib/api";
import { formatDate } from "@/lib/formatters";
import type { StrategyDto } from "@/lib/types";

export default function StrategiesPage() {
  const [strategies, setStrategies] = useState<StrategyDto[]>([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    api.strategies.list().then(setStrategies).catch(console.error).finally(() => setLoading(false));
  }, []);

  if (loading) return <div className="animate-pulse text-[hsl(var(--muted-foreground))]">Loading strategies...</div>;

  return (
    <div className="space-y-6">
      <h2 className="text-2xl font-bold">Strategies</h2>

      {strategies.length === 0 ? (
        <div className="rounded-xl border border-[hsl(var(--border))] bg-[hsl(var(--card))] p-12 text-center text-[hsl(var(--muted-foreground))]">
          No strategies configured
        </div>
      ) : (
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
          {strategies.map((strategy) => (
            <div
              key={strategy.id}
              className="rounded-xl border border-[hsl(var(--border))] bg-[hsl(var(--card))] p-4 space-y-3"
            >
              <div className="flex items-start justify-between gap-2">
                <a
                  href={`/strategies/${strategy.id}`}
                  className="text-lg font-semibold hover:underline text-[hsl(var(--foreground))]"
                >
                  {strategy.name}
                </a>
                <span
                  className={`shrink-0 text-xs px-2 py-1 rounded-full font-medium ${
                    strategy.isActive
                      ? "bg-profit/10 text-profit"
                      : "bg-loss/10 text-loss"
                  }`}
                >
                  {strategy.isActive ? "Active" : "Inactive"}
                </span>
              </div>

              <div>
                <span className="text-xs px-2 py-1 rounded-md bg-[hsl(var(--muted))] text-[hsl(var(--muted-foreground))] font-medium">
                  {strategy.assetClass}
                </span>
              </div>

              {strategy.description && (
                <p className="text-sm text-[hsl(var(--muted-foreground))] line-clamp-2">
                  {strategy.description}
                </p>
              )}

              <p className="text-xs text-[hsl(var(--muted-foreground))]">
                Created {formatDate(strategy.createdAt)}
              </p>
            </div>
          ))}
        </div>
      )}
    </div>
  );
}
