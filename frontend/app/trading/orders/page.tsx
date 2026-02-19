"use client";

import { useEffect, useState } from "react";
import { api } from "@/lib/api";
import { formatCurrency, formatDateTime } from "@/lib/formatters";
import type { OrderDto } from "@/lib/types";

function getStatusColor(status: string): string {
  switch (status) {
    case "Filled":
      return "text-green-500";
    case "PartiallyFilled":
      return "text-yellow-500";
    case "Cancelled":
    case "Rejected":
      return "text-red-500";
    default:
      return "text-[hsl(var(--muted-foreground))]";
  }
}

function getSideColor(side: string): string {
  if (side.toLowerCase() === "buy") return "text-green-500";
  if (side.toLowerCase() === "sell") return "text-red-500";
  return "";
}

function canCancel(status: string): boolean {
  return status === "New" || status === "PartiallyFilled";
}

export default function OrdersPage() {
  const [orders, setOrders] = useState<OrderDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [cancelling, setCancelling] = useState<Set<string>>(new Set());

  async function fetchOrders() {
    try {
      const data = await api.trading.orders();
      setOrders(data);
    } finally {
      setLoading(false);
    }
  }

  useEffect(() => {
    fetchOrders();
  }, []);

  async function handleCancel(id: string) {
    setCancelling((prev) => new Set(prev).add(id));
    try {
      await api.trading.cancelOrder(id);
      await fetchOrders();
    } finally {
      setCancelling((prev) => {
        const next = new Set(prev);
        next.delete(id);
        return next;
      });
    }
  }

  if (loading) {
    return (
      <div className="p-6 space-y-3 animate-pulse">
        <div className="h-6 w-32 rounded bg-[hsl(var(--muted))]" />
        <div className="h-10 rounded-xl bg-[hsl(var(--muted))]" />
        {Array.from({ length: 5 }).map((_, i) => (
          <div key={i} className="h-12 rounded bg-[hsl(var(--muted))]" />
        ))}
      </div>
    );
  }

  return (
    <div className="p-6">
      <h1 className="text-2xl font-semibold mb-6">Orders</h1>

      {orders.length === 0 ? (
        <div className="flex items-center justify-center rounded-xl border border-[hsl(var(--border))] bg-[hsl(var(--card))] p-16">
          <p className="text-[hsl(var(--muted-foreground))]">No orders</p>
        </div>
      ) : (
        <div className="rounded-xl border border-[hsl(var(--border))] bg-[hsl(var(--card))] overflow-hidden">
          <div className="overflow-x-auto">
            <table className="w-full text-sm">
              <thead className="bg-[hsl(var(--muted))]">
                <tr>
                  <th className="p-3 text-left font-medium text-[hsl(var(--muted-foreground))]">
                    Symbol
                  </th>
                  <th className="p-3 text-left font-medium text-[hsl(var(--muted-foreground))]">
                    Side
                  </th>
                  <th className="p-3 text-left font-medium text-[hsl(var(--muted-foreground))]">
                    Type
                  </th>
                  <th className="p-3 text-left font-medium text-[hsl(var(--muted-foreground))]">
                    Status
                  </th>
                  <th className="p-3 text-right font-medium text-[hsl(var(--muted-foreground))]">
                    Qty
                  </th>
                  <th className="p-3 text-right font-medium text-[hsl(var(--muted-foreground))]">
                    Limit Price
                  </th>
                  <th className="p-3 text-right font-medium text-[hsl(var(--muted-foreground))]">
                    Filled Qty
                  </th>
                  <th className="p-3 text-right font-medium text-[hsl(var(--muted-foreground))]">
                    Filled Price
                  </th>
                  <th className="p-3 text-left font-medium text-[hsl(var(--muted-foreground))]">
                    Broker
                  </th>
                  <th className="p-3 text-left font-medium text-[hsl(var(--muted-foreground))]">
                    Date
                  </th>
                  <th className="p-3 text-left font-medium text-[hsl(var(--muted-foreground))]">
                    Actions
                  </th>
                </tr>
              </thead>
              <tbody className="divide-y divide-[hsl(var(--border))]">
                {orders.map((order) => (
                  <tr
                    key={order.id}
                    className="hover:bg-[hsl(var(--muted))] transition-colors"
                  >
                    <td className="p-3 font-medium">{order.symbol}</td>
                    <td className={`p-3 font-medium ${getSideColor(order.side)}`}>
                      {order.side}
                    </td>
                    <td className="p-3 text-[hsl(var(--muted-foreground))]">
                      {order.type}
                    </td>
                    <td className={`p-3 font-medium ${getStatusColor(order.status)}`}>
                      {order.status}
                    </td>
                    <td className="p-3 text-right tabular-nums">{order.qty}</td>
                    <td className="p-3 text-right tabular-nums">
                      {order.limitPrice != null
                        ? formatCurrency(order.limitPrice)
                        : <span className="text-[hsl(var(--muted-foreground))]">—</span>}
                    </td>
                    <td className="p-3 text-right tabular-nums">
                      {order.filledQty != null
                        ? order.filledQty
                        : <span className="text-[hsl(var(--muted-foreground))]">—</span>}
                    </td>
                    <td className="p-3 text-right tabular-nums">
                      {order.filledPrice != null
                        ? formatCurrency(order.filledPrice)
                        : <span className="text-[hsl(var(--muted-foreground))]">—</span>}
                    </td>
                    <td className="p-3 text-[hsl(var(--muted-foreground))]">
                      {order.broker}
                    </td>
                    <td className="p-3 text-[hsl(var(--muted-foreground))] whitespace-nowrap">
                      {formatDateTime(order.createdAt)}
                    </td>
                    <td className="p-3">
                      {canCancel(order.status) ? (
                        <button
                          onClick={() => handleCancel(order.id)}
                          disabled={cancelling.has(order.id)}
                          className="rounded-md border border-red-500 px-3 py-1 text-xs font-medium text-red-500 hover:bg-red-500 hover:text-white transition-colors disabled:opacity-50 disabled:cursor-not-allowed"
                        >
                          {cancelling.has(order.id) ? "Cancelling…" : "Cancel"}
                        </button>
                      ) : (
                        <span className="text-[hsl(var(--muted-foreground))]">—</span>
                      )}
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </div>
      )}
    </div>
  );
}
