import { useState, useEffect, useCallback } from "react";
import { api } from "@/lib/api";
import { getConnection, startConnection } from "@/lib/signalr";
import type { PortfolioDto } from "@/lib/types";

interface UsePortfolioResult {
  portfolio: PortfolioDto | null;
  loading: boolean;
  error: string | null;
}

export function usePortfolio(): UsePortfolioResult {
  const [portfolio, setPortfolio] = useState<PortfolioDto | null>(null);
  const [loading, setLoading] = useState<boolean>(true);
  const [error, setError] = useState<string | null>(null);

  const fetchPortfolio = useCallback(async () => {
    try {
      setError(null);
      const data = await api.dashboard.portfolio();
      setPortfolio(data);
    } catch (err) {
      setError(err instanceof Error ? err.message : "Failed to fetch portfolio");
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    fetchPortfolio();

    let mounted = true;

    const setupSignalR = async () => {
      try {
        await startConnection();
        const conn = getConnection();

        const handlePortfolioUpdate = (updatedPortfolio: PortfolioDto) => {
          if (mounted) {
            setPortfolio(updatedPortfolio);
          }
        };

        conn.on("PortfolioUpdate", handlePortfolioUpdate);

        return () => {
          conn.off("PortfolioUpdate", handlePortfolioUpdate);
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
  }, [fetchPortfolio]);

  return { portfolio, loading, error };
}
