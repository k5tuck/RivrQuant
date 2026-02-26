"use client";
import { useState, useEffect } from "react";
import { api } from "@/lib/api";
import { formatCurrency, formatPercent } from "@/lib/formatters";
import type { PositionDto, PortfolioDto } from "@/lib/types";

export default function TradingPage() {
  const [positions, setPositions] = useState<PositionDto[]>([]);
  const [portfolio, setPortfolio] = useState<PortfolioDto | null>(null);
  const [loading, setLoading] = useState(true);
  const [killConfirm, setKillConfirm] = useState(false);

  useEffect(() => {
    Promise.all([api.trading.positions(), api.dashboard.portfolio()])
      .then(([pos, port]) => { setPositions(pos); setPortfolio(port); })
      .catch(console.error).finally(() => setLoading(false));
  }, []);

  const handleKillSwitch = async () => {
    if (!killConfirm) { setKillConfirm(true); return; }
    await api.trading.closeAll();
    setKillConfirm(false);
    setPositions([]);
  };

  if (loading) return <div className="animate-pulse text-[hsl(var(--muted-foreground))]">Loading...</div>;

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <h2 className="text-2xl font-bold">Live Trading</h2>
        <button onClick={handleKillSwitch}
          className={`px-6 py-2 rounded-lg font-bold text-white ${killConfirm ? "bg-red-700 animate-pulse" : "bg-red-600 hover:bg-red-700"}`}>
          {killConfirm ? "CONFIRM: Close ALL Positions" : "Kill Switch"}
        </button>
      </div>

      {portfolio && (
        <div className="grid grid-cols-4 gap-4">
          <div className="rounded-lg border border-[hsl(var(--border))] bg-[hsl(var(--card))] p-4">
            <p className="text-xs text-[hsl(var(--muted-foreground))]">Equity</p>
            <p className="text-xl font-bold">{formatCurrency(portfolio.totalEquity)}</p>
          </div>
          <div className="rounded-lg border border-[hsl(var(--border))] bg-[hsl(var(--card))] p-4">
            <p className="text-xs text-[hsl(var(--muted-foreground))]">Unrealized P&L</p>
            <p className={`text-xl font-bold ${portfolio.unrealizedPnl >= 0 ? "text-profit" : "text-loss"}`}>
              {formatCurrency(portfolio.unrealizedPnl)}
            </p>
          </div>
          <div className="rounded-lg border border-[hsl(var(--border))] bg-[hsl(var(--card))] p-4">
            <p className="text-xs text-[hsl(var(--muted-foreground))]">Buying Power</p>
            <p className="text-xl font-bold">{formatCurrency(portfolio.buyingPower)}</p>
          </div>
          <div className="rounded-lg border border-[hsl(var(--border))] bg-[hsl(var(--card))] p-4">
            <p className="text-xs text-[hsl(var(--muted-foreground))]">Positions</p>
            <p className="text-xl font-bold">{positions.length}</p>
          </div>
        </div>
      )}

      <div className="space-y-4">
        <h3 className="text-lg font-semibold">Open Positions</h3>
        <div className="grid gap-3">
          {positions.map((p) => (
            <div key={`${p.symbol}-${p.broker}`} className="rounded-lg border border-[hsl(var(--border))] bg-[hsl(var(--card))] p-4 flex items-center justify-between">
              <div>
                <span className="font-bold text-lg">{p.symbol}</span>
                <span className="ml-2 text-xs text-[hsl(var(--muted-foreground))]">{p.broker} | {p.assetClass}</span>
                <div className="text-sm text-[hsl(var(--muted-foreground))]">{p.side} {p.qty} @ {formatCurrency(p.entryPrice)}</div>
              </div>
              <div className="text-right">
                <p className={`text-lg font-bold ${p.unrealizedPnl >= 0 ? "text-profit" : "text-loss"}`}>
                  {formatCurrency(p.unrealizedPnl)}
                </p>
                <p className={`text-sm ${p.pnlPercent >= 0 ? "text-profit" : "text-loss"}`}>{formatPercent(p.pnlPercent / 100)}</p>
              </div>
            </div>
          ))}
          {positions.length === 0 && <p className="text-center text-[hsl(var(--muted-foreground))] py-8">No open positions</p>}
        </div>
      </div>

      <div className="flex gap-4">
        <a href="/trading/positions" className="text-sm text-[hsl(var(--primary))] hover:underline">All Positions</a>
        <a href="/trading/orders" className="text-sm text-[hsl(var(--primary))] hover:underline">Order History</a>
        <a href="/trading/performance" className="text-sm text-[hsl(var(--primary))] hover:underline">Performance</a>
      </div>
    </div>
  );
}
