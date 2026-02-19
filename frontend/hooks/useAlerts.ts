import { useState, useEffect, useCallback, useMemo } from "react";
import { api } from "@/lib/api";
import { getConnection, startConnection } from "@/lib/signalr";
import type { AlertEventDto } from "@/lib/types";

interface UseAlertsResult {
  alerts: AlertEventDto[];
  loading: boolean;
  error: string | null;
  unacknowledgedCount: number;
}

export function useAlerts(): UseAlertsResult {
  const [alerts, setAlerts] = useState<AlertEventDto[]>([]);
  const [loading, setLoading] = useState<boolean>(true);
  const [error, setError] = useState<string | null>(null);

  const fetchAlerts = useCallback(async () => {
    try {
      setError(null);
      const data = await api.alerts.history();
      setAlerts(data);
    } catch (err) {
      setError(
        err instanceof Error ? err.message : "Failed to fetch alert history"
      );
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    fetchAlerts();

    let mounted = true;

    const setupSignalR = async () => {
      try {
        await startConnection();
        const conn = getConnection();

        const handleAlertTriggered = (newAlert: AlertEventDto) => {
          if (mounted) {
            setAlerts((prev) => [newAlert, ...prev]);
          }
        };

        conn.on("AlertTriggered", handleAlertTriggered);

        return () => {
          conn.off("AlertTriggered", handleAlertTriggered);
        };
      } catch (err) {
        if (mounted) {
          setError(
            err instanceof Error
              ? err.message
              : "Failed to connect to real-time updates"
          );
        }
        return () => {};
      }
    };

    let cleanup: (() => void) | undefined;

    setupSignalR().then((cleanupFn) => {
      cleanup = cleanupFn;
    });

    return () => {
      mounted = false;
      cleanup?.();
    };
  }, [fetchAlerts]);

  const unacknowledgedCount = useMemo(
    () => alerts.filter((alert) => !alert.isAcknowledged).length,
    [alerts]
  );

  return { alerts, loading, error, unacknowledgedCount };
}
