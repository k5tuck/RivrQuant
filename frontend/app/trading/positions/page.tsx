"use client";
import { useState, useEffect } from "react";
import { api } from "@/lib/api";
import { formatCurrency, formatPercent } from "@/lib/formatters";
import type { PositionDto } from "@/lib/types";

export default function PositionsPage() {
  const [positions, setPositions] = useState<PositionDto[]>([]);
  const [loading, setLoading] = useState(true);
  useEffect(() => { api.trading.positions().then(setPositions).catch(console.error).finally(() => setLoading(false)); }, []);
  if (loading) return <div className="animate-pulse text-[hsl(var(--muted-foreground))]">Loading...</div>;
  return (
    <div className="space-y-6">
      <h2 className="text-2xl font-bold">Current Positions</h2>
      <div className="rounded-xl border border-[hsl(var(--border))] bg-[hsl(var(--card))] overflow-hidden">
        <table className="w-full text-sm">
          <thead><tr className="border-b border-[hsl(var(--border))] bg-[hsl(var(--muted))]">
            <th className="p-3 text-left">Symbol</th><th className="p-3 text-left">Side</th><th className="p-3 text-right">Qty</th>
            <th className="p-3 text-right">Entry</th><th className="p-3 text-right">Current</th><th className="p-3 text-right">P&L</th>
            <th className="p-3 text-right">P&L %</th><th className="p-3 text-left">Broker</th>
          </tr></thead>
          <tbody>
            {positions.map((p) => (
              <tr key={`${p.symbol}-${p.broker}`} className="border-b border-[hsl(var(--border))]">
                <td className="p-3 font-medium">{p.symbol}</td>
                <td className={`p-3 ${p.side === "Buy" ? "text-green-500" : "text-red-500"}`}>{p.side}</td>
                <td className="p-3 text-right">{p.qty}</td>
                <td className="p-3 text-right">{formatCurrency(p.entryPrice)}</td>
                <td className="p-3 text-right">{formatCurrency(p.currentPrice)}</td>
                <td className={`p-3 text-right ${p.unrealizedPnl >= 0 ? "text-green-500" : "text-red-500"}`}>{formatCurrency(p.unrealizedPnl)}</td>
                <td className={`p-3 text-right ${p.pnlPercent >= 0 ? "text-green-500" : "text-red-500"}`}>{formatPercent(p.pnlPercent / 100)}</td>
                <td className="p-3">{p.broker}</td>
              </tr>
            ))}
            {positions.length === 0 && <tr><td colSpan={8} className="p-8 text-center text-[hsl(var(--muted-foreground))]">No positions</td></tr>}
          </tbody>
        </table>
      </div>
    </div>
  );
}
