"use client";

import { formatCurrency, formatPercent } from "@/lib/formatters";
import type { PortfolioDto } from "@/lib/types";

interface PortfolioSummaryCardProps {
  portfolio: PortfolioDto;
}

export default function PortfolioSummaryCard({ portfolio }: PortfolioSummaryCardProps) {
  const isPositive = portfolio.dailyChange >= 0;

  return (
    <div className="rounded-lg border border-[hsl(var(--border))] bg-[hsl(var(--card))] p-6">
      <p className="text-sm text-[hsl(var(--muted-foreground))]">Total Portfolio Value</p>
      <p className="mt-1 text-3xl font-bold text-[hsl(var(--foreground))]">
        {formatCurrency(portfolio.totalEquity)}
      </p>
      <div className="mt-2 flex items-center gap-2">
        <span className={`text-sm font-medium ${isPositive ? "text-profit" : "text-loss"}`}>
          {isPositive ? "+" : ""}
          {formatCurrency(portfolio.dailyChange)}
        </span>
        <span className={`rounded px-1.5 py-0.5 text-xs font-medium ${isPositive ? "bg-profit/10 text-profit" : "bg-loss/10 text-loss"}`}>
          {isPositive ? "+" : ""}
          {formatPercent(portfolio.dailyChangePercent)}
        </span>
      </div>
      <div className="mt-4 grid grid-cols-3 gap-4 border-t border-[hsl(var(--border))] pt-4">
        <div>
          <p className="text-xs text-[hsl(var(--muted-foreground))]">Cash</p>
          <p className="text-sm font-medium text-[hsl(var(--foreground))]">{formatCurrency(portfolio.cash)}</p>
        </div>
        <div>
          <p className="text-xs text-[hsl(var(--muted-foreground))]">Buying Power</p>
          <p className="text-sm font-medium text-[hsl(var(--foreground))]">{formatCurrency(portfolio.buyingPower)}</p>
        </div>
        <div>
          <p className="text-xs text-[hsl(var(--muted-foreground))]">Unrealized P&L</p>
          <p className={`text-sm font-medium ${portfolio.unrealizedPnl >= 0 ? "text-profit" : "text-loss"}`}>
            {portfolio.unrealizedPnl >= 0 ? "+" : ""}{formatCurrency(portfolio.unrealizedPnl)}
          </p>
        </div>
      </div>
    </div>
  );
}
