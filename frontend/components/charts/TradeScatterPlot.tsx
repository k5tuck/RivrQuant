"use client";

import { ScatterChart, Scatter, XAxis, YAxis, CartesianGrid, Tooltip, ResponsiveContainer } from "recharts";

interface TradePoint {
  entryTime: string;
  profitLoss: number;
  quantity: number;
  side: string;
}

interface TradeScatterPlotProps {
  trades: TradePoint[];
}

export default function TradeScatterPlot({ trades }: TradeScatterPlotProps) {
  const data = trades.map((t, i) => ({
    index: i,
    pnl: t.profitLoss,
    qty: t.quantity,
    time: t.entryTime,
    side: t.side,
  }));

  const wins = data.filter((d) => d.pnl >= 0);
  const losses = data.filter((d) => d.pnl < 0);

  if (trades.length === 0) {
    return (
      <div className="rounded-lg border border-[hsl(var(--border))] bg-[hsl(var(--card))] p-6 text-center text-[hsl(var(--muted-foreground))]">
        No trade data available
      </div>
    );
  }

  return (
    <div className="rounded-lg border border-[hsl(var(--border))] bg-[hsl(var(--card))] p-4">
      <h3 className="mb-4 text-sm font-medium text-[hsl(var(--foreground))]">Trade P&L Scatter</h3>
      <ResponsiveContainer width="100%" height={300}>
        <ScatterChart margin={{ top: 10, right: 20, bottom: 10, left: 10 }}>
          <CartesianGrid strokeDasharray="3 3" stroke="hsl(var(--border))" />
          <XAxis dataKey="index" type="number" name="Trade #" stroke="hsl(var(--muted-foreground))" fontSize={11} />
          <YAxis dataKey="pnl" type="number" name="P&L" stroke="hsl(var(--muted-foreground))" fontSize={11} />
          <Tooltip
            contentStyle={{ backgroundColor: "hsl(var(--card))", border: "1px solid hsl(var(--border))", borderRadius: 8, fontSize: 12 }}
            formatter={(value: number) => [`$${value.toFixed(2)}`, "P&L"]}
          />
          <Scatter name="Wins" data={wins} fill="#22c55e" fillOpacity={0.7} />
          <Scatter name="Losses" data={losses} fill="#ef4444" fillOpacity={0.7} />
        </ScatterChart>
      </ResponsiveContainer>
    </div>
  );
}
