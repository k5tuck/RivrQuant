"use client";
import { AreaChart, Area, XAxis, YAxis, CartesianGrid, Tooltip, ResponsiveContainer } from "recharts";

interface Props {
  data: { date: string; drawdown: number }[];
}

export default function DrawdownChart({ data }: Props) {
  return (
    <div className="h-80 w-full">
      <ResponsiveContainer width="100%" height="100%">
        <AreaChart data={data} margin={{ top: 5, right: 20, left: 10, bottom: 5 }}>
          <CartesianGrid strokeDasharray="3 3" stroke="hsl(var(--border))" />
          <XAxis dataKey="date" tick={{ fontSize: 12, fill: "hsl(var(--muted-foreground))" }} />
          <YAxis tick={{ fontSize: 12, fill: "hsl(var(--muted-foreground))" }} />
          <Tooltip
            contentStyle={{ backgroundColor: "hsl(var(--card))", border: "1px solid hsl(var(--border))", borderRadius: "0.5rem" }}
            labelStyle={{ color: "hsl(var(--foreground))" }}
          />
          <Area
            type="monotone"
            dataKey="drawdown"
            stroke="#ef4444"
            strokeWidth={2}
            fill="rgba(239,68,68,0.3)"
            dot={false}
          />
        </AreaChart>
      </ResponsiveContainer>
    </div>
  );
}
