"use client";

import { useState } from "react";
import { api } from "@/lib/api";

export default function OrderForm() {
  const [symbol, setSymbol] = useState("");
  const [side, setSide] = useState<"Buy" | "Sell">("Buy");
  const [orderType, setOrderType] = useState("Market");
  const [quantity, setQuantity] = useState("");
  const [limitPrice, setLimitPrice] = useState("");
  const [assetClass, setAssetClass] = useState("Stock");
  const [submitting, setSubmitting] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [success, setSuccess] = useState(false);

  async function handleSubmit(e: React.FormEvent) {
    e.preventDefault();
    setError(null);
    setSuccess(false);
    setSubmitting(true);

    try {
      await api.trading.placeOrder({
        symbol: symbol.toUpperCase(),
        side,
        type: orderType,
        quantity: parseFloat(quantity),
        limitPrice: limitPrice ? parseFloat(limitPrice) : undefined,
        assetClass,
      });
      setSuccess(true);
      setSymbol("");
      setQuantity("");
      setLimitPrice("");
    } catch (err) {
      setError(err instanceof Error ? err.message : "Order failed");
    } finally {
      setSubmitting(false);
    }
  }

  return (
    <form onSubmit={handleSubmit} className="rounded-lg border border-[hsl(var(--border))] bg-[hsl(var(--card))] p-4">
      <h3 className="mb-4 text-sm font-medium text-[hsl(var(--foreground))]">Place Order</h3>

      <div className="space-y-3">
        <div className="grid grid-cols-2 gap-3">
          <div>
            <label className="block text-xs text-[hsl(var(--muted-foreground))] mb-1">Symbol</label>
            <input
              type="text"
              value={symbol}
              onChange={(e) => setSymbol(e.target.value)}
              placeholder="AAPL"
              required
              className="w-full rounded-md border border-[hsl(var(--border))] bg-[hsl(var(--background))] px-3 py-2 text-sm text-[hsl(var(--foreground))] placeholder:text-[hsl(var(--muted-foreground))]"
            />
          </div>
          <div>
            <label className="block text-xs text-[hsl(var(--muted-foreground))] mb-1">Asset Class</label>
            <select
              value={assetClass}
              onChange={(e) => setAssetClass(e.target.value)}
              className="w-full rounded-md border border-[hsl(var(--border))] bg-[hsl(var(--background))] px-3 py-2 text-sm text-[hsl(var(--foreground))]"
            >
              <option value="Stock">Stock (Alpaca)</option>
              <option value="Crypto">Crypto (Bybit)</option>
            </select>
          </div>
        </div>

        <div className="grid grid-cols-2 gap-3">
          <div>
            <label className="block text-xs text-[hsl(var(--muted-foreground))] mb-1">Side</label>
            <div className="flex rounded-md border border-[hsl(var(--border))]">
              <button
                type="button"
                onClick={() => setSide("Buy")}
                className={`flex-1 py-2 text-sm font-medium rounded-l-md ${side === "Buy" ? "bg-profit/20 text-profit" : "text-[hsl(var(--muted-foreground))]"}`}
              >
                Buy
              </button>
              <button
                type="button"
                onClick={() => setSide("Sell")}
                className={`flex-1 py-2 text-sm font-medium rounded-r-md ${side === "Sell" ? "bg-loss/20 text-loss" : "text-[hsl(var(--muted-foreground))]"}`}
              >
                Sell
              </button>
            </div>
          </div>
          <div>
            <label className="block text-xs text-[hsl(var(--muted-foreground))] mb-1">Order Type</label>
            <select
              value={orderType}
              onChange={(e) => setOrderType(e.target.value)}
              className="w-full rounded-md border border-[hsl(var(--border))] bg-[hsl(var(--background))] px-3 py-2 text-sm text-[hsl(var(--foreground))]"
            >
              <option value="Market">Market</option>
              <option value="Limit">Limit</option>
              <option value="StopLoss">Stop Loss</option>
              <option value="StopLimit">Stop Limit</option>
            </select>
          </div>
        </div>

        <div className="grid grid-cols-2 gap-3">
          <div>
            <label className="block text-xs text-[hsl(var(--muted-foreground))] mb-1">Quantity</label>
            <input
              type="number"
              value={quantity}
              onChange={(e) => setQuantity(e.target.value)}
              placeholder="0"
              required
              min="0"
              step="any"
              className="w-full rounded-md border border-[hsl(var(--border))] bg-[hsl(var(--background))] px-3 py-2 text-sm text-[hsl(var(--foreground))] placeholder:text-[hsl(var(--muted-foreground))]"
            />
          </div>
          {(orderType === "Limit" || orderType === "StopLimit") && (
            <div>
              <label className="block text-xs text-[hsl(var(--muted-foreground))] mb-1">Limit Price</label>
              <input
                type="number"
                value={limitPrice}
                onChange={(e) => setLimitPrice(e.target.value)}
                placeholder="0.00"
                step="any"
                min="0"
                className="w-full rounded-md border border-[hsl(var(--border))] bg-[hsl(var(--background))] px-3 py-2 text-sm text-[hsl(var(--foreground))] placeholder:text-[hsl(var(--muted-foreground))]"
              />
            </div>
          )}
        </div>

        {error && (
          <div className="rounded-md bg-loss/10 border border-loss/20 px-3 py-2 text-sm text-loss">{error}</div>
        )}
        {success && (
          <div className="rounded-md bg-profit/10 border border-profit/20 px-3 py-2 text-sm text-profit">Order placed successfully</div>
        )}

        <button
          type="submit"
          disabled={submitting}
          className="w-full rounded-md bg-[hsl(var(--primary))] py-2 text-sm font-medium text-[hsl(var(--primary-foreground))] hover:opacity-90 disabled:opacity-50"
        >
          {submitting ? "Placing..." : `Place ${side} Order`}
        </button>
      </div>
    </form>
  );
}
