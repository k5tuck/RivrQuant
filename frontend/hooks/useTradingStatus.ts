import { useState, useEffect, useCallback } from "react";
import { startConnection } from "@/lib/signalr";

interface BrokerConnection {
  name: string;
  connected: boolean;
  latencyMs: number | null;
}

interface TradingStatus {
  brokers: BrokerConnection[];
  isAnyConnected: boolean;
  isAllConnected: boolean;
}

export function useTradingStatus(): TradingStatus {
  const [brokers, setBrokers] = useState<BrokerConnection[]>([
    { name: "Alpaca", connected: false, latencyMs: null },
    { name: "Bybit", connected: false, latencyMs: null },
  ]);

  const handleStatusChange = useCallback(
    (data: { broker: string; connected: boolean; latencyMs: number }) => {
      setBrokers((prev) =>
        prev.map((b) =>
          b.name.toLowerCase() === data.broker.toLowerCase()
            ? { ...b, connected: data.connected, latencyMs: data.latencyMs }
            : b
        )
      );
    },
    []
  );

  useEffect(() => {
    const connection = startConnection();
    connection.on("BrokerStatusChange", handleStatusChange);
    return () => {
      connection.off("BrokerStatusChange");
    };
  }, [handleStatusChange]);

  return {
    brokers,
    isAnyConnected: brokers.some((b) => b.connected),
    isAllConnected: brokers.every((b) => b.connected),
  };
}
