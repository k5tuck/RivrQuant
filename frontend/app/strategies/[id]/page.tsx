"use client";
import { useState, useEffect } from "react";
import { useParams } from "next/navigation";
import { api } from "@/lib/api";
import { formatDate } from "@/lib/formatters";
import type { StrategyDto } from "@/lib/types";

export default function StrategyDetailPage() {
  const params = useParams();
  const [strategy, setStrategy] = useState<StrategyDto | null>(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    if (params.id) {
      api.strategies.get(params.id as string).then(setStrategy).catch(console.error).finally(() => setLoading(false));
    }
  }, [params.id]);

  if (loading) return <div className="animate-pulse text-[hsl(var(--muted-foreground))]">Loading strategy...</div>;
  if (!strategy) return <div className="text-[hsl(var(--muted-foreground))]">Strategy not found</div>;

  return (
    <div className="space-y-6">
      <div>
        <a href="/strategies" className="text-sm text-[hsl(var(--muted-foreground))] hover:underline">
          Strategies
        </a>
        <h2 className="text-2xl font-bold mt-1">{strategy.name}</h2>
      </div>

      <div className="rounded-xl border border-[hsl(var(--border))] bg-[hsl(var(--card))] p-6 space-y-6">
        <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
          <div className="space-y-1">
            <p className="text-xs text-[hsl(var(--muted-foreground))] uppercase tracking-wide font-medium">Name</p>
            <p className="text-base font-semibold">{strategy.name}</p>
          </div>

          <div className="space-y-1">
            <p className="text-xs text-[hsl(var(--muted-foreground))] uppercase tracking-wide font-medium">Asset Class</p>
            <span className="inline-block text-sm px-2 py-1 rounded-md bg-[hsl(var(--muted))] text-[hsl(var(--muted-foreground))] font-medium">
              {strategy.assetClass}
            </span>
          </div>

          <div className="space-y-1">
            <p className="text-xs text-[hsl(var(--muted-foreground))] uppercase tracking-wide font-medium">Status</p>
            <span
              className={`inline-block text-sm px-2 py-1 rounded-full font-medium ${
                strategy.isActive
                  ? "bg-green-500/10 text-green-500"
                  : "bg-red-500/10 text-red-500"
              }`}
            >
              {strategy.isActive ? "Active" : "Inactive"}
            </span>
          </div>

          <div className="space-y-1">
            <p className="text-xs text-[hsl(var(--muted-foreground))] uppercase tracking-wide font-medium">Created</p>
            <p className="text-base">{formatDate(strategy.createdAt)}</p>
          </div>
        </div>

        {strategy.description && (
          <div className="space-y-1 border-t border-[hsl(var(--border))] pt-6">
            <p className="text-xs text-[hsl(var(--muted-foreground))] uppercase tracking-wide font-medium">Description</p>
            <p className="text-sm text-[hsl(var(--foreground))] leading-relaxed">{strategy.description}</p>
          </div>
        )}
      </div>
    </div>
  );
}
