"use client";

import { severityColor, formatDateTime } from "@/lib/formatters";
import type { AlertEventDto } from "@/lib/types";

interface AlertsBannerProps {
  alerts: AlertEventDto[];
}

export default function AlertsBanner({ alerts }: AlertsBannerProps) {
  const recent = alerts.filter((a) => !a.isAcknowledged).slice(0, 3);

  if (recent.length === 0) {
    return null;
  }

  return (
    <div className="rounded-lg border border-yellow-500/30 bg-yellow-500/5 p-3">
      <div className="flex items-center justify-between mb-2">
        <h3 className="text-sm font-medium text-yellow-500">Active Alerts</h3>
        <a href="/alerts/history" className="text-xs text-[hsl(var(--muted-foreground))] hover:text-[hsl(var(--foreground))]">
          View all &rarr;
        </a>
      </div>
      <div className="space-y-1.5">
        {recent.map((alert) => (
          <div key={alert.id} className="flex items-center gap-3 rounded-md bg-[hsl(var(--card))] px-3 py-2 text-sm">
            <span className={`shrink-0 rounded px-1.5 py-0.5 text-xs font-medium ${severityColor(alert.severity)}`}>
              {alert.severity}
            </span>
            <span className="flex-1 truncate text-[hsl(var(--foreground))]">{alert.message}</span>
            <span className="shrink-0 text-xs text-[hsl(var(--muted-foreground))]">
              {formatDateTime(alert.triggeredAt)}
            </span>
          </div>
        ))}
      </div>
    </div>
  );
}
