"use client";

import { useState, useEffect } from "react";
import { api } from "@/lib/api";
import { formatCurrency, formatPercent } from "@/lib/formatters";
import type { PortfolioDto } from "@/lib/types";
import BrokerStatusIndicator from "@/components/dashboard/BrokerStatusIndicator";

export default function Header() {
  const [portfolio, setPortfolio] = useState<PortfolioDto | null>(null);

  useEffect(() => {
    api.dashboard.portfolio()
      .then(setPortfolio)
      .catch(() => setPortfolio(null));
  }, []);

  return (
    <header className="flex items-center justify-between border-b border-[hsl(var(--border))] bg-[hsl(var(--card))] px-6 py-3">
      <div className="flex items-center gap-6">
        {portfolio && (
          <>
            <div>
              <p className="text-xs text-[hsl(var(--muted-foreground))]">Portfolio Value</p>
              <p className="text-lg font-bold text-[hsl(var(--foreground))]">{formatCurrency(portfolio.totalEquity)}</p>
            </div>
            <div>
              <p className="text-xs text-[hsl(var(--muted-foreground))]">Daily Change</p>
              <p className={`text-sm font-medium ${portfolio.dailyChange >= 0 ? "text-green-500" : "text-red-500"}`}>
                {portfolio.dailyChange >= 0 ? "+" : ""}{formatCurrency(portfolio.dailyChange)} ({portfolio.dailyChangePercent >= 0 ? "+" : ""}{formatPercent(portfolio.dailyChangePercent)})
              </p>
            </div>
          </>
        )}
      </div>
      <BrokerStatusIndicator />
    </header>
  );
}
