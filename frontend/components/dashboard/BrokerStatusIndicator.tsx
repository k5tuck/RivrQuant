"use client";

import { useState, useEffect } from "react";
import { startConnection } from "@/lib/signalr";

interface BrokerStatus {
  name: string;
  connected: boolean;
  latencyMs: number | null;
}

export default function BrokerStatusIndicator() {
  const [brokers, setBrokers] = useState<BrokerStatus[]>([
    { name: "Alpaca", connected: false, latencyMs: null },
    { name: "Bybit", connected: false, latencyMs: null },
  ]);

  useEffect(() => {
    const connection = startConnection();
    connection.on("BrokerStatusChange", (data: { broker: string; connected: boolean; latencyMs: number }) => {
      setBrokers((prev) =>
        prev.map((b) =>
          b.name.toLowerCase() === data.broker.toLowerCase()
            ? { ...b, connected: data.connected, latencyMs: data.latencyMs }
            : b
        )
      );
    });
    return () => {
      connection.off("BrokerStatusChange");
    };
  }, []);

  return (
    <div className="flex items-center gap-4">
      {brokers.map((b) => (
        <div key={b.name} className="flex items-center gap-2">
          <span className={`h-2 w-2 rounded-full ${b.connected ? "bg-green-500" : "bg-red-500"}`} />
          <span className="text-xs text-[hsl(var(--muted-foreground))]">{b.name}</span>
          {b.latencyMs !== null && (
            <span className={`text-xs ${b.latencyMs < 100 ? "text-green-500" : b.latencyMs < 500 ? "text-yellow-500" : "text-red-500"}`}>
              {b.latencyMs}ms
            </span>
          )}
        </div>
      ))}
    </div>
  );
}
