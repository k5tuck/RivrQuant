"use client";

import { formatCurrency, formatDateTime } from "@/lib/formatters";
import type { OrderDto } from "@/lib/types";

interface RecentTradesTableProps {
  trades: OrderDto[];
}

export default function RecentTradesTable({ trades }: RecentTradesTableProps) {
  if (trades.length === 0) {
    return (
      <div className="rounded-lg border border-[hsl(var(--border))] bg-[hsl(var(--card))] p-6 text-center text-sm text-[hsl(var(--muted-foreground))]">
        No recent trades
      </div>
    );
  }

  return (
    <div className="rounded-lg border border-[hsl(var(--border))] bg-[hsl(var(--card))] overflow-hidden">
      <div className="px-4 py-3 border-b border-[hsl(var(--border))]">
        <h3 className="text-sm font-medium text-[hsl(var(--foreground))]">Recent Trades</h3>
      </div>
      <table className="w-full text-sm">
        <thead>
          <tr className="border-b border-[hsl(var(--border))] text-xs text-[hsl(var(--muted-foreground))]">
            <th className="px-4 py-2 text-left">Time</th>
            <th className="px-4 py-2 text-left">Symbol</th>
            <th className="px-4 py-2 text-left">Side</th>
            <th className="px-4 py-2 text-right">Qty</th>
            <th className="px-4 py-2 text-right">Price</th>
            <th className="px-4 py-2 text-left">Broker</th>
          </tr>
        </thead>
        <tbody>
          {trades.slice(0, 20).map((trade) => (
            <tr key={trade.id} className="border-b border-[hsl(var(--border))] last:border-0 hover:bg-[hsl(var(--accent))]">
              <td className="px-4 py-2 text-[hsl(var(--muted-foreground))]">{formatDateTime(trade.createdAt)}</td>
              <td className="px-4 py-2 font-medium text-[hsl(var(--foreground))]">{trade.symbol}</td>
              <td className="px-4 py-2">
                <span className={`text-xs font-medium px-1.5 py-0.5 rounded ${trade.side === "Buy" ? "bg-profit/10 text-profit" : "bg-loss/10 text-loss"}`}>
                  {trade.side}
                </span>
              </td>
              <td className="px-4 py-2 text-right text-[hsl(var(--foreground))]">{trade.qty}</td>
              <td className="px-4 py-2 text-right text-[hsl(var(--foreground))]">
                {trade.filledPrice ? formatCurrency(trade.filledPrice) : "\u2014"}
              </td>
              <td className="px-4 py-2 text-[hsl(var(--muted-foreground))]">{trade.broker}</td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
}
