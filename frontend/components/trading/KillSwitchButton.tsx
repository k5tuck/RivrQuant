"use client";

import { useState } from "react";
import { api } from "@/lib/api";

export default function KillSwitchButton() {
  const [confirming, setConfirming] = useState(false);
  const [executing, setExecuting] = useState(false);
  const [result, setResult] = useState<"success" | "error" | null>(null);

  async function handleKillSwitch() {
    setExecuting(true);
    setResult(null);
    try {
      await api.trading.closeAll();
      setResult("success");
      setConfirming(false);
    } catch {
      setResult("error");
    } finally {
      setExecuting(false);
    }
  }

  if (confirming) {
    return (
      <div className="rounded-lg border-2 border-loss bg-loss/10 p-4">
        <p className="mb-3 text-sm font-medium text-loss">
          This will close ALL positions across ALL brokers. Are you sure?
        </p>
        <div className="flex gap-2">
          <button
            onClick={handleKillSwitch}
            disabled={executing}
            className="rounded-md bg-red-600 px-4 py-2 text-sm font-bold text-white hover:bg-red-700 disabled:opacity-50"
          >
            {executing ? "Closing all..." : "CONFIRM CLOSE ALL"}
          </button>
          <button
            onClick={() => setConfirming(false)}
            disabled={executing}
            className="rounded-md border border-[hsl(var(--border))] px-4 py-2 text-sm text-[hsl(var(--muted-foreground))] hover:bg-[hsl(var(--accent))]"
          >
            Cancel
          </button>
        </div>
        {result === "success" && (
          <p className="mt-2 text-sm text-profit">All positions closed successfully.</p>
        )}
        {result === "error" && (
          <p className="mt-2 text-sm text-loss">Failed to close positions. Check broker connections.</p>
        )}
      </div>
    );
  }

  return (
    <button
      onClick={() => setConfirming(true)}
      className="rounded-md border-2 border-loss bg-loss/10 px-6 py-3 text-sm font-bold text-loss hover:bg-loss/20 transition-colors"
    >
      KILL SWITCH — Close All Positions
    </button>
  );
}
