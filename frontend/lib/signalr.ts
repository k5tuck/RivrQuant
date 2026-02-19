import * as signalR from "@microsoft/signalr";

const SIGNALR_URL = process.env.NEXT_PUBLIC_SIGNALR_URL || "http://localhost:5000/hubs/trading";

let connection: signalR.HubConnection | null = null;

export function getConnection(): signalR.HubConnection {
  if (!connection) {
    connection = new signalR.HubConnectionBuilder()
      .withUrl(SIGNALR_URL)
      .withAutomaticReconnect([0, 1000, 2000, 5000, 10000, 30000])
      .configureLogging(signalR.LogLevel.Warning)
      .build();
  }
  return connection;
}

export async function startConnection(): Promise<void> {
  const conn = getConnection();
  if (conn.state === signalR.HubConnectionState.Disconnected) {
    try {
      await conn.start();
    } catch (err) {
      console.error("SignalR connection failed:", err);
    }
  }
}

export type SignalREvent =
  | "PortfolioUpdate"
  | "PositionUpdate"
  | "OrderUpdate"
  | "TradeExecuted"
  | "BacktestDetected"
  | "AnalysisComplete"
  | "AlertTriggered"
  | "BrokerStatusChange";
