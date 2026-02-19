"use client";

import { PieChart, Pie, Cell, Tooltip, ResponsiveContainer, Legend } from "recharts";
import type { PositionDto } from "@/lib/types";

interface PortfolioPieChartProps {
  positions: PositionDto[];
  totalEquity: number;
}

const COLORS = ["#3b82f6", "#22c55e", "#f59e0b", "#ef4444", "#8b5cf6", "#06b6d4", "#ec4899", "#f97316"];

export default function PortfolioPieChart({ positions, totalEquity }: PortfolioPieChartProps) {
  const data = positions.map((p) => ({
    name: p.symbol,
    value: Math.abs(p.currentPrice * p.qty),
    broker: p.broker,
  }));

  const positionTotal = data.reduce((sum, d) => sum + d.value, 0);
  const cashValue = Math.max(totalEquity - positionTotal, 0);

  if (cashValue > 0) {
    data.push({ name: "Cash", value: cashValue, broker: "" });
  }

  if (data.length === 0) {
    return (
      <div className="rounded-lg border border-[hsl(var(--border))] bg-[hsl(var(--card))] p-6 text-center text-[hsl(var(--muted-foreground))]">
        No positions to display
      </div>
    );
  }

  return (
    <div className="rounded-lg border border-[hsl(var(--border))] bg-[hsl(var(--card))] p-4">
      <h3 className="mb-4 text-sm font-medium text-[hsl(var(--foreground))]">Portfolio Allocation</h3>
      <ResponsiveContainer width="100%" height={300}>
        <PieChart>
          <Pie
            data={data}
            cx="50%"
            cy="50%"
            innerRadius={60}
            outerRadius={100}
            paddingAngle={2}
            dataKey="value"
          >
            {data.map((_, i) => (
              <Cell key={i} fill={COLORS[i % COLORS.length]} />
            ))}
          </Pie>
          <Tooltip
            contentStyle={{ backgroundColor: "hsl(var(--card))", border: "1px solid hsl(var(--border))", borderRadius: 8, fontSize: 12 }}
            formatter={(value: number) => [`$${value.toLocaleString("en-US", { minimumFractionDigits: 2, maximumFractionDigits: 2 })}`, "Value"]}
          />
          <Legend
            wrapperStyle={{ fontSize: 11, color: "hsl(var(--muted-foreground))" }}
          />
        </PieChart>
      </ResponsiveContainer>
    </div>
  );
}
