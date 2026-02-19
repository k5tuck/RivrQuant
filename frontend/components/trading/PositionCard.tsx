"use client";

import { formatCurrency, formatPercent } from "@/lib/formatters";
import type { PositionDto } from "@/lib/types";

interface PositionCardProps {
  position: PositionDto;
}

export default function PositionCard({ position }: PositionCardProps) {
  const isProfit = position.unrealizedPnl >= 0;

  return (
    <div className="rounded-lg border border-[hsl(var(--border))] bg-[hsl(var(--card))] p-4">
      <div className="flex items-center justify-between">
        <div className="flex items-center gap-2">
          <span className="text-base font-bold text-[hsl(var(--foreground))]">{position.symbol}</span>
          <span className={`text-xs font-medium px-1.5 py-0.5 rounded ${position.side === "Long" ? "bg-green-500/10 text-green-500" : "bg-red-500/10 text-red-500"}`}>
            {position.side}
          </span>
        </div>
        <span className="text-xs rounded bg-[hsl(var(--muted))] px-2 py-0.5 text-[hsl(var(--muted-foreground))]">
          {position.broker}
        </span>
      </div>
      <div className="mt-3 grid grid-cols-2 gap-3 text-sm">
        <div>
          <p className="text-xs text-[hsl(var(--muted-foreground))]">Qty</p>
          <p className="font-medium text-[hsl(var(--foreground))]">{position.qty}</p>
        </div>
        <div>
          <p className="text-xs text-[hsl(var(--muted-foreground))]">Entry Price</p>
          <p className="font-medium text-[hsl(var(--foreground))]">{formatCurrency(position.entryPrice)}</p>
        </div>
        <div>
          <p className="text-xs text-[hsl(var(--muted-foreground))]">Current Price</p>
          <p className="font-medium text-[hsl(var(--foreground))]">{formatCurrency(position.currentPrice)}</p>
        </div>
        <div>
          <p className="text-xs text-[hsl(var(--muted-foreground))]">Unrealized P&L</p>
          <p className={`font-medium ${isProfit ? "text-green-500" : "text-red-500"}`}>
            {isProfit ? "+" : ""}{formatCurrency(position.unrealizedPnl)}
            <span className="ml-1 text-xs">({isProfit ? "+" : ""}{formatPercent(position.pnlPercent)})</span>
          </p>
        </div>
      </div>
    </div>
  );
}
