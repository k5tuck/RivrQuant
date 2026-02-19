"use client";

import { BarChart, Bar, XAxis, YAxis, CartesianGrid, Tooltip, Legend, ResponsiveContainer } from "recharts";

interface WalkForwardWindow {
  windowIndex: number;
  inSampleSharpe: number;
  outOfSampleSharpe: number;
}

interface WalkForwardChartProps {
  windows: WalkForwardWindow[];
}

export default function WalkForwardChart({ windows }: WalkForwardChartProps) {
  if (windows.length === 0) {
    return (
      <div className="rounded-lg border border-[hsl(var(--border))] bg-[hsl(var(--card))] p-6 text-center text-sm text-[hsl(var(--muted-foreground))]">
        No walk-forward data available
      </div>
    );
  }

  const data = windows.map((w) => ({
    name: `Window ${w.windowIndex + 1}`,
    "In-Sample": w.inSampleSharpe,
    "Out-of-Sample": w.outOfSampleSharpe,
  }));

  return (
    <div className="rounded-lg border border-[hsl(var(--border))] bg-[hsl(var(--card))] p-4">
      <h3 className="mb-4 text-sm font-medium text-[hsl(var(--foreground))]">Walk-Forward Analysis</h3>
      <ResponsiveContainer width="100%" height={300}>
        <BarChart data={data} margin={{ top: 10, right: 20, bottom: 10, left: 10 }}>
          <CartesianGrid strokeDasharray="3 3" stroke="hsl(var(--border))" />
          <XAxis dataKey="name" stroke="hsl(var(--muted-foreground))" fontSize={11} />
          <YAxis stroke="hsl(var(--muted-foreground))" fontSize={11} />
          <Tooltip
            contentStyle={{
              backgroundColor: "hsl(var(--card))",
              border: "1px solid hsl(var(--border))",
              borderRadius: 8,
              fontSize: 12,
            }}
          />
          <Legend wrapperStyle={{ fontSize: 12 }} />
          <Bar dataKey="In-Sample" fill="#3b82f6" radius={[4, 4, 0, 0]} />
          <Bar dataKey="Out-of-Sample" fill="#22c55e" radius={[4, 4, 0, 0]} />
        </BarChart>
      </ResponsiveContainer>
      <p className="mt-2 text-xs text-[hsl(var(--muted-foreground))]">
        Sharpe decay indicates potential overfitting when in-sample significantly exceeds out-of-sample performance.
      </p>
    </div>
  );
}
