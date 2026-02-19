"use client";

import { useState } from "react";

interface RegimePeriod {
  regime: string;
  startDate: string;
  endDate: string;
  returnInRegime: number;
}

interface Props {
  data: RegimePeriod[];
}

const REGIME_COLORS: Record<string, string> = {
  Trending: "bg-green-500",
  MeanReverting: "bg-yellow-500",
  HighVolatility: "bg-orange-500",
  LowVolatility: "bg-gray-500",
  Crisis: "bg-red-500",
};

const REGIME_HEX: Record<string, string> = {
  Trending: "#22c55e",
  MeanReverting: "#eab308",
  HighVolatility: "#f97316",
  LowVolatility: "#6b7280",
  Crisis: "#ef4444",
};

const LEGEND_REGIMES = [
  "Trending",
  "MeanReverting",
  "HighVolatility",
  "LowVolatility",
  "Crisis",
];

interface TooltipState {
  visible: boolean;
  x: number;
  y: number;
  regime: string;
  returnInRegime: number;
}

export default function RegimeTimeline({ data }: Props) {
  const [tooltip, setTooltip] = useState<TooltipState>({
    visible: false,
    x: 0,
    y: 0,
    regime: "",
    returnInRegime: 0,
  });

  if (!data || data.length === 0) {
    return (
      <div className="w-full flex items-center justify-center h-16 text-sm text-muted-foreground">
        No regime data available.
      </div>
    );
  }

  const totalStart = new Date(data[0].startDate).getTime();
  const totalEnd = new Date(data[data.length - 1].endDate).getTime();
  const totalSpan = totalEnd - totalStart;

  return (
    <div className="w-full space-y-3">
      {/* Horizontal bar */}
      <div
        className="h-12 w-full rounded-lg overflow-hidden flex"
        role="img"
        aria-label="Market regime timeline"
      >
        {data.map((period, index) => {
          const start = new Date(period.startDate).getTime();
          const end = new Date(period.endDate).getTime();
          const widthPercent = ((end - start) / totalSpan) * 100;
          const colorClass = REGIME_COLORS[period.regime] ?? "bg-gray-400";

          return (
            <div
              key={index}
              className={`${colorClass} cursor-pointer transition-opacity hover:opacity-80 relative`}
              style={{ width: `${widthPercent}%` }}
              onMouseEnter={(e) => {
                const rect = (e.currentTarget as HTMLElement)
                  .closest(".regime-wrapper")
                  ?.getBoundingClientRect();
                const segRect = (
                  e.currentTarget as HTMLElement
                ).getBoundingClientRect();
                setTooltip({
                  visible: true,
                  x: segRect.left + segRect.width / 2,
                  y: segRect.top,
                  regime: period.regime,
                  returnInRegime: period.returnInRegime,
                });
              }}
              onMouseLeave={() =>
                setTooltip((prev) => ({ ...prev, visible: false }))
              }
            />
          );
        })}
      </div>

      {/* Floating tooltip */}
      {tooltip.visible && (
        <div
          className="fixed z-50 pointer-events-none px-3 py-2 rounded-lg shadow-lg text-xs"
          style={{
            left: tooltip.x,
            top: tooltip.y - 60,
            transform: "translateX(-50%)",
            backgroundColor: "hsl(var(--card))",
            border: "1px solid hsl(var(--border))",
            color: "hsl(var(--foreground))",
          }}
        >
          <div className="font-semibold">{tooltip.regime}</div>
          <div>
            Return:{" "}
            <span
              className={
                tooltip.returnInRegime >= 0 ? "text-green-500" : "text-red-500"
              }
            >
              {tooltip.returnInRegime >= 0 ? "+" : ""}
              {(tooltip.returnInRegime * 100).toFixed(2)}%
            </span>
          </div>
        </div>
      )}

      {/* Legend */}
      <div className="flex flex-wrap gap-4">
        {LEGEND_REGIMES.map((regime) => (
          <div key={regime} className="flex items-center gap-1.5">
            <span
              className="inline-block h-3 w-3 rounded-full flex-shrink-0"
              style={{ backgroundColor: REGIME_HEX[regime] ?? "#9ca3af" }}
            />
            <span className="text-xs text-muted-foreground">{regime}</span>
          </div>
        ))}
      </div>
    </div>
  );
}
