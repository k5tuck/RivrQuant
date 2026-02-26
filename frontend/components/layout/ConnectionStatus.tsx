"use client";

import { useState, useEffect } from "react";
import { startConnection } from "@/lib/signalr";
import { HubConnectionState } from "@microsoft/signalr";

export default function ConnectionStatus() {
  const [state, setState] = useState<"connected" | "reconnecting" | "disconnected">("disconnected");

  useEffect(() => {
    const connection = startConnection();

    function updateState() {
      switch (connection.state) {
        case HubConnectionState.Connected:
          setState("connected");
          break;
        case HubConnectionState.Reconnecting:
          setState("reconnecting");
          break;
        default:
          setState("disconnected");
          break;
      }
    }

    connection.onreconnecting(() => setState("reconnecting"));
    connection.onreconnected(() => setState("connected"));
    connection.onclose(() => setState("disconnected"));

    updateState();

    return () => {
      connection.onreconnecting(() => {});
      connection.onreconnected(() => {});
      connection.onclose(() => {});
    };
  }, []);

  const statusConfig = {
    connected: { color: "bg-profit", label: "SignalR Connected" },
    reconnecting: { color: "bg-warning animate-pulse", label: "Reconnecting..." },
    disconnected: { color: "bg-loss", label: "Disconnected" },
  };

  const config = statusConfig[state];

  return (
    <div className="flex items-center gap-2">
      <span className={`h-2 w-2 rounded-full ${config.color}`} />
      <span className="text-xs text-[hsl(var(--muted-foreground))]">{config.label}</span>
    </div>
  );
}
