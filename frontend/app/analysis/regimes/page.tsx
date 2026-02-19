"use client";

import { useState } from "react";
import { formatDate, formatPercent, formatNumber } from "@/lib/formatters";
import type { RegimeDto } from "@/lib/types";

const regimeColors: Record<string, { bg: string; border: string; text: string; label: string }> = {
  Trending: { bg: "bg-green-500", border: "border-green-500/30", text: "text-green-500", label: "Trending (Up)" },
  TrendingDown: { bg: "bg-blue-500", border: "border-blue-500/30", text: "text-blue-500", label: "Trending (Down)" },
  MeanReverting: { bg: "bg-yellow-500", border: "border-yellow-500/30", text: "text-yellow-500", label: "Mean Reverting" },
  HighVolatility: { bg: "bg-orange-500", border: "border-orange-500/30", text: "text-orange-500", label: "High Volatility" },
  LowVolatility: { bg: "bg-zinc-400", border: "border-zinc-400/30", text: "text-zinc-400", label: "Low Volatility" },
  Crisis: { bg: "bg-red-500", border: "border-red-500/30", text: "text-red-500", label: "Crisis" },
};

function getRegimeStyle(regime: string) {
  return regimeColors[regime] || regimeColors["LowVolatility"];
}

export default function RegimesPage() {
  const [regimes] = useState<RegimeDto[]>([]);

  const totalDays = regimes.reduce((sum, r) => {
    const start = new Date(r.startDate).getTime();
    const end = new Date(r.endDate).getTime();
    return sum + (end - start) / (1000 * 60 * 60 * 24);
  }, 0);

  return (
    <div className="space-y-6">
      <div>
        <h2 className="text-2xl font-bold text-[hsl(var(--foreground))]">Market Regime Timeline</h2>
        <p className="mt-1 text-sm text-[hsl(var(--muted-foreground))]">
          Color-coded market regime classification over time
        </p>
      </div>

      {regimes.length === 0 ? (
        <div className="rounded-lg border border-[hsl(var(--border))] bg-[hsl(var(--card))] p-12 text-center">
          <p className="text-[hsl(var(--muted-foreground))]">No regime data available</p>
          <p className="mt-1 text-xs text-[hsl(var(--muted-foreground))]">
            Run a backtest analysis to generate regime classifications
          </p>
        </div>
      ) : (
        <>
          <div className="rounded-lg border border-[hsl(var(--border))] bg-[hsl(var(--card))] p-6">
            <h3 className="mb-4 text-sm font-medium text-[hsl(var(--foreground))]">Timeline</h3>
            <div className="flex h-10 w-full overflow-hidden rounded-lg">
              {regimes.map((r, i) => {
                const start = new Date(r.startDate).getTime();
                const end = new Date(r.endDate).getTime();
                const days = (end - start) / (1000 * 60 * 60 * 24);
                const width = totalDays > 0 ? (days / totalDays) * 100 : 0;
                const style = getRegimeStyle(r.regime);

                return (
                  <div
                    key={i}
                    className={`${style.bg} relative group`}
                    style={{ width: `${width}%` }}
                    title={`${style.label}: ${formatDate(r.startDate)} — ${formatDate(r.endDate)}`}
                  >
                    <div className="absolute inset-0 opacity-0 group-hover:opacity-100 bg-white/10 transition-opacity" />
                  </div>
                );
              })}
            </div>
            {regimes.length > 0 && (
              <div className="mt-2 flex justify-between text-xs text-[hsl(var(--muted-foreground))]">
                <span>{formatDate(regimes[0].startDate)}</span>
                <span>{formatDate(regimes[regimes.length - 1].endDate)}</span>
              </div>
            )}
          </div>

          <div className="flex flex-wrap gap-4 rounded-lg border border-[hsl(var(--border))] bg-[hsl(var(--card))] p-4">
            {Object.entries(regimeColors).map(([key, style]) => (
              <div key={key} className="flex items-center gap-2">
                <div className={`h-3 w-6 rounded-sm ${style.bg}`} />
                <span className="text-xs text-[hsl(var(--muted-foreground))]">{style.label}</span>
              </div>
            ))}
          </div>

          <div className="rounded-lg border border-[hsl(var(--border))] bg-[hsl(var(--card))] overflow-hidden">
            <table className="w-full text-sm">
              <thead>
                <tr className="border-b border-[hsl(var(--border))] text-xs text-[hsl(var(--muted-foreground))]">
                  <th className="px-4 py-3 text-left">Regime</th>
                  <th className="px-4 py-3 text-left">Start</th>
                  <th className="px-4 py-3 text-left">End</th>
                  <th className="px-4 py-3 text-right">Return</th>
                  <th className="px-4 py-3 text-right">Sharpe</th>
                </tr>
              </thead>
              <tbody>
                {regimes.map((r, i) => {
                  const style = getRegimeStyle(r.regime);
                  return (
                    <tr key={i} className="border-b border-[hsl(var(--border))] last:border-0">
                      <td className="px-4 py-2.5">
                        <div className="flex items-center gap-2">
                          <div className={`h-2.5 w-2.5 rounded-full ${style.bg}`} />
                          <span className={style.text}>{style.label}</span>
                        </div>
                      </td>
                      <td className="px-4 py-2.5 text-[hsl(var(--muted-foreground))]">{formatDate(r.startDate)}</td>
                      <td className="px-4 py-2.5 text-[hsl(var(--muted-foreground))]">{formatDate(r.endDate)}</td>
                      <td className={`px-4 py-2.5 text-right font-medium ${r.returnInRegime >= 0 ? "text-green-500" : "text-red-500"}`}>
                        {formatPercent(r.returnInRegime)}
                      </td>
                      <td className="px-4 py-2.5 text-right font-medium text-[hsl(var(--foreground))]">
                        {formatNumber(r.sharpeInRegime)}
                      </td>
                    </tr>
                  );
                })}
              </tbody>
            </table>
          </div>
        </>
      )}
    </div>
  );
}
