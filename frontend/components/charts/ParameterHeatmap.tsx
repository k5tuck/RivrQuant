"use client";

import { useMemo } from "react";

interface ParameterPoint {
  parameterValue: number;
  sharpeRatio: number;
  totalReturn: number;
}

interface ParameterHeatmapProps {
  data: ParameterPoint[];
  parameterName: string;
}

function valueToColor(value: number, min: number, max: number): string {
  const ratio = max === min ? 0.5 : (value - min) / (max - min);
  if (ratio >= 0.5) {
    const g = Math.round(100 + 155 * ((ratio - 0.5) * 2));
    return `rgb(50, ${g}, 80)`;
  }
  const r = Math.round(100 + 155 * ((0.5 - ratio) * 2));
  return `rgb(${r}, 50, 60)`;
}

export default function ParameterHeatmap({ data, parameterName }: ParameterHeatmapProps) {
  const { sorted, minSharpe, maxSharpe } = useMemo(() => {
    const sorted = [...data].sort((a, b) => a.parameterValue - b.parameterValue);
    const sharpes = sorted.map((d) => d.sharpeRatio);
    return {
      sorted,
      minSharpe: Math.min(...sharpes, 0),
      maxSharpe: Math.max(...sharpes, 1),
    };
  }, [data]);

  if (data.length === 0) {
    return (
      <div className="rounded-lg border border-[hsl(var(--border))] bg-[hsl(var(--card))] p-6 text-center text-[hsl(var(--muted-foreground))]">
        No parameter sensitivity data available
      </div>
    );
  }

  return (
    <div className="rounded-lg border border-[hsl(var(--border))] bg-[hsl(var(--card))] p-4">
      <h3 className="mb-4 text-sm font-medium text-[hsl(var(--foreground))]">
        Parameter Sensitivity: {parameterName}
      </h3>
      <div className="flex gap-1 items-end" style={{ height: 200 }}>
        {sorted.map((point, i) => {
          const height = maxSharpe === minSharpe
            ? 50
            : ((point.sharpeRatio - minSharpe) / (maxSharpe - minSharpe)) * 100;
          return (
            <div key={i} className="flex-1 flex flex-col items-center gap-1">
              <div
                className="w-full rounded-t transition-all"
                style={{
                  height: `${Math.max(height, 4)}%`,
                  backgroundColor: valueToColor(point.sharpeRatio, minSharpe, maxSharpe),
                }}
                title={`${parameterName}=${point.parameterValue} | Sharpe: ${point.sharpeRatio.toFixed(2)} | Return: ${(point.totalReturn * 100).toFixed(1)}%`}
              />
              <span className="text-[10px] text-[hsl(var(--muted-foreground))] truncate w-full text-center">
                {point.parameterValue}
              </span>
            </div>
          );
        })}
      </div>
      <div className="mt-3 flex items-center justify-between text-xs text-[hsl(var(--muted-foreground))]">
        <span>Low Sharpe</span>
        <div className="flex gap-0.5">
          {[0, 0.25, 0.5, 0.75, 1].map((r) => (
            <div
              key={r}
              className="h-3 w-6 rounded-sm"
              style={{ backgroundColor: valueToColor(minSharpe + r * (maxSharpe - minSharpe), minSharpe, maxSharpe) }}
            />
          ))}
        </div>
        <span>High Sharpe</span>
      </div>
    </div>
  );
}
