"use client";

import { useState, useEffect } from "react";
import { api } from "@/lib/api";
import { formatDateTime, severityColor } from "@/lib/formatters";
import type { AlertEventDto } from "@/lib/types";

export default function AlertHistoryPage() {
  const [events, setEvents] = useState<AlertEventDto[]>([]);
  const [loading, setLoading] = useState(true);

  async function fetchEvents() {
    setLoading(true);
    try {
      const data = await api.alerts.history();
      setEvents(data);
    } finally {
      setLoading(false);
    }
  }

  useEffect(() => {
    fetchEvents();
  }, []);

  async function handleAcknowledge(id: string) {
    await api.alerts.acknowledge(id);
    await fetchEvents();
  }

  return (
    <div className="p-6 space-y-6">
      <div className="flex items-center justify-between">
        <h2 className="text-2xl font-semibold" style={{ color: "hsl(var(--foreground))" }}>
          Alert History
        </h2>
        <a
          href="/alerts"
          className="px-4 py-2 rounded-md text-sm font-medium border transition-colors"
          style={{
            color: "hsl(var(--foreground))",
            borderColor: "hsl(var(--border))",
            backgroundColor: "hsl(var(--background))",
          }}
        >
          &larr; Back to Rules
        </a>
      </div>

      <div
        className="rounded-lg border overflow-hidden"
        style={{
          backgroundColor: "hsl(var(--card))",
          borderColor: "hsl(var(--border))",
        }}
      >
        {loading ? (
          <div className="p-8 text-center text-sm" style={{ color: "hsl(var(--muted-foreground))" }}>
            Loading alert history...
          </div>
        ) : events.length === 0 ? (
          <div className="p-8 text-center text-sm" style={{ color: "hsl(var(--muted-foreground))" }}>
            No alert events
          </div>
        ) : (
          <div className="overflow-x-auto">
            <table className="w-full text-sm">
              <thead>
                <tr style={{ borderBottomColor: "hsl(var(--border))", borderBottomWidth: 1 }}>
                  {["Rule", "Severity", "Message", "Value", "Threshold", "Acknowledged", "Time", "Actions"].map((col) => (
                    <th
                      key={col}
                      className="px-4 py-3 text-left text-xs font-semibold uppercase tracking-wide"
                      style={{ color: "hsl(var(--muted-foreground))" }}
                    >
                      {col}
                    </th>
                  ))}
                </tr>
              </thead>
              <tbody>
                {events.map((event, idx) => (
                  <tr
                    key={event.id}
                    style={{
                      borderTopColor: idx === 0 ? "transparent" : "hsl(var(--border))",
                      borderTopWidth: idx === 0 ? 0 : 1,
                    }}
                  >
                    <td className="px-4 py-3 font-medium" style={{ color: "hsl(var(--foreground))" }}>
                      {event.ruleName}
                    </td>
                    <td className="px-4 py-3">
                      <span
                        className={`inline-flex items-center px-2 py-0.5 rounded-full text-xs font-semibold ${severityColor(event.severity)}`}
                      >
                        {event.severity}
                      </span>
                    </td>
                    <td
                      className="px-4 py-3 max-w-xs truncate"
                      style={{ color: "hsl(var(--muted-foreground))" }}
                      title={event.message}
                    >
                      {event.message}
                    </td>
                    <td className="px-4 py-3 tabular-nums" style={{ color: "hsl(var(--foreground))" }}>
                      {event.currentValue}
                    </td>
                    <td className="px-4 py-3 tabular-nums" style={{ color: "hsl(var(--foreground))" }}>
                      {event.threshold}
                    </td>
                    <td className="px-4 py-3 text-center">
                      {event.isAcknowledged ? (
                        <span
                          className="text-base"
                          style={{ color: "hsl(var(--chart-2, 142 76% 36%))" }}
                          aria-label="Acknowledged"
                        >
                          &#10003;
                        </span>
                      ) : (
                        <span
                          className="text-base"
                          style={{ color: "hsl(var(--muted-foreground))" }}
                          aria-label="Not acknowledged"
                        >
                          &#8212;
                        </span>
                      )}
                    </td>
                    <td
                      className="px-4 py-3 whitespace-nowrap tabular-nums"
                      style={{ color: "hsl(var(--muted-foreground))" }}
                    >
                      {formatDateTime(event.triggeredAt)}
                    </td>
                    <td className="px-4 py-3">
                      {!event.isAcknowledged && (
                        <button
                          onClick={() => handleAcknowledge(event.id)}
                          className="px-3 py-1 rounded-md text-xs font-medium transition-colors"
                          style={{
                            backgroundColor: "hsl(var(--primary) / 0.1)",
                            color: "hsl(var(--primary))",
                          }}
                        >
                          Acknowledge
                        </button>
                      )}
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}
      </div>
    </div>
  );
}
