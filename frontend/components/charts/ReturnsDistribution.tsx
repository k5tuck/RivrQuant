"use client";
import { BarChart, Bar, XAxis, YAxis, CartesianGrid, Tooltip, ResponsiveContainer, Cell } from "recharts";

interface Props {
  data: { date: string; dailyReturnPercent: number }[];
}

interface Bin {
  rangeLabel: string;
  count: number;
  midpoint: number;
}

function buildHistogram(returns: number[], binCount = 20): Bin[] {
  if (returns.length === 0) return [];

  const min = Math.min(...returns);
  const max = Math.max(...returns);
  // Guard against all-identical values
  const range = max === min ? 1 : max - min;
  const binWidth = range / binCount;

  const bins: Bin[] = Array.from({ length: binCount }, (_, i) => {
    const lower = min + i * binWidth;
    const upper = lower + binWidth;
    const midpoint = (lower + upper) / 2;
    return {
      rangeLabel: `${lower.toFixed(2)}%`,
      count: 0,
      midpoint,
    };
  });

  for (const value of returns) {
    // Clamp the last value into the final bin
    let index = Math.floor((value - min) / binWidth);
    if (index >= binCount) index = binCount - 1;
    bins[index].count += 1;
  }

  return bins;
}

export default function ReturnsDistribution({ data }: Props) {
  const returns = data.map((d) => d.dailyReturnPercent);
  const bins = buildHistogram(returns);

  return (
    <div className="h-80 w-full">
      <ResponsiveContainer width="100%" height="100%">
        <BarChart data={bins} margin={{ top: 5, right: 20, left: 10, bottom: 5 }}>
          <CartesianGrid strokeDasharray="3 3" stroke="hsl(var(--border))" />
          <XAxis
            dataKey="rangeLabel"
            tick={{ fontSize: 11, fill: "hsl(var(--muted-foreground))" }}
            interval="preserveStartEnd"
          />
          <YAxis
            tick={{ fontSize: 12, fill: "hsl(var(--muted-foreground))" }}
            allowDecimals={false}
            label={{
              value: "Count",
              angle: -90,
              position: "insideLeft",
              style: { fontSize: 12, fill: "hsl(var(--muted-foreground))" },
            }}
          />
          <Tooltip
            contentStyle={{
              backgroundColor: "hsl(var(--card))",
              border: "1px solid hsl(var(--border))",
              borderRadius: "0.5rem",
            }}
            labelStyle={{ color: "hsl(var(--foreground))" }}
            formatter={(value: number) => [value, "Count"]}
          />
          <Bar dataKey="count" radius={[2, 2, 0, 0]}>
            {bins.map((bin, index) => (
              <Cell
                key={`cell-${index}`}
                fill={bin.midpoint >= 0 ? "rgba(34,197,94,0.8)" : "rgba(239,68,68,0.8)"}
              />
            ))}
          </Bar>
        </BarChart>
      </ResponsiveContainer>
    </div>
  );
}
