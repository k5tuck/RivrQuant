"use client";

import { useState, useEffect, useCallback } from "react";
import { startConnection, SignalREvent } from "@/lib/signalr";
import { formatCurrency } from "@/lib/formatters";

interface LivePnlTickerProps {
  initialPnl?: number;
}

export default function LivePnlTicker({ initialPnl = 0 }: LivePnlTickerProps) {
  const [pnl, setPnl] = useState(initialPnl);
  const [flash, setFlash] = useState<"up" | "down" | null>(null);

  const handleUpdate = useCallback((event: SignalREvent) => {
    if (event.type === "PortfolioUpdate" && event.data?.dailyChange !== undefined) {
      const newPnl = event.data.dailyChange as number;
      setFlash(newPnl > pnl ? "up" : "down");
      setPnl(newPnl);
      setTimeout(() => setFlash(null), 600);
    }
  }, [pnl]);

  useEffect(() => {
    const connection = startConnection();
    connection.on("PortfolioUpdate", (data: Record<string, unknown>) => {
      handleUpdate({ type: "PortfolioUpdate", data });
    });
    return () => {
      connection.off("PortfolioUpdate");
    };
  }, [handleUpdate]);

  const isPositive = pnl >= 0;

  return (
    <div
      className={`inline-flex items-center gap-2 rounded-lg border px-4 py-2 font-mono text-lg font-bold transition-all duration-300 ${
        flash === "up"
          ? "border-green-500/50 bg-green-500/10"
          : flash === "down"
          ? "border-red-500/50 bg-red-500/10"
          : "border-[hsl(var(--border))] bg-[hsl(var(--card))]"
      }`}
    >
      <span className="text-xs text-[hsl(var(--muted-foreground))]">Today</span>
      <span className={isPositive ? "text-green-500" : "text-red-500"}>
        {isPositive ? "+" : ""}
        {formatCurrency(pnl)}
      </span>
    </div>
  );
}
