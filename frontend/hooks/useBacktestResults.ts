import { useState, useEffect, useCallback, useRef } from "react";
import { api } from "@/lib/api";
import { getConnection, startConnection } from "@/lib/signalr";
import type { BacktestSummaryDto } from "@/lib/types";

interface UseBacktestResultsResult {
  backtests: BacktestSummaryDto[];
  loading: boolean;
  error: string | null;
  refresh: () => void;
}

export function useBacktestResults(): UseBacktestResultsResult {
  const [backtests, setBacktests] = useState<BacktestSummaryDto[]>([]);
  const [loading, setLoading] = useState<boolean>(true);
  const [error, setError] = useState<string | null>(null);
  const refreshTrigger = useRef<number>(0);
  const [, forceRefresh] = useState<number>(0);

  const refresh = useCallback(() => {
    refreshTrigger.current += 1;
    forceRefresh((n) => n + 1);
  }, []);

  const fetchBacktests = useCallback(async () => {
    try {
      setLoading(true);
      setError(null);
      const data = await api.backtests.list();
      setBacktests(data);
    } catch (err) {
      setError(
        err instanceof Error ? err.message : "Failed to fetch backtest results"
      );
    } finally {
      setLoading(false);
    }
  }, []);

  // Re-fetch whenever refresh() is called
  useEffect(() => {
    fetchBacktests();
  }, [fetchBacktests, refreshTrigger.current]); // eslint-disable-line react-hooks/exhaustive-deps

  useEffect(() => {
    let mounted = true;

    const setupSignalR = async () => {
      try {
        await startConnection();
        const conn = getConnection();

        const handleBacktestDetected = (_payload: unknown) => {
          if (mounted) {
            fetchBacktests();
          }
        };

        const handleAnalysisComplete = (_payload: unknown) => {
          if (mounted) {
            fetchBacktests();
          }
        };

        conn.on("BacktestDetected", handleBacktestDetected);
        conn.on("AnalysisComplete", handleAnalysisComplete);

        return () => {
          conn.off("BacktestDetected", handleBacktestDetected);
          conn.off("AnalysisComplete", handleAnalysisComplete);
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
  }, [fetchBacktests]);

  return { backtests, loading, error, refresh };
}
