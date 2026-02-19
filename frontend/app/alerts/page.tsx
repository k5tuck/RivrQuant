"use client";

import { useState, useEffect } from "react";
import { api } from "@/lib/api";
import type { AlertRuleDto } from "@/lib/types";
import { severityColor } from "@/lib/formatters";

const defaultForm = {
  name: "",
  conditionType: "DrawdownExceedsPercent",
  threshold: 0,
  severity: "Info",
  sendEmail: false,
  sendSms: false,
};

export default function AlertRulesPage() {
  const [rules, setRules] = useState<AlertRuleDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [showForm, setShowForm] = useState(false);
  const [form, setForm] = useState(defaultForm);
  const [submitting, setSubmitting] = useState(false);

  async function fetchRules() {
    setLoading(true);
    try {
      const data = await api.alerts.rules();
      setRules(data);
    } finally {
      setLoading(false);
    }
  }

  useEffect(() => {
    fetchRules();
  }, []);

  async function handleDelete(id: string) {
    await api.alerts.deleteRule(id);
    await fetchRules();
  }

  async function handleToggle(id: string) {
    await api.alerts.toggleRule(id);
    await fetchRules();
  }

  async function handleSubmit(e: React.FormEvent) {
    e.preventDefault();
    setSubmitting(true);
    try {
      await api.alerts.createRule(form);
      setForm(defaultForm);
      setShowForm(false);
      await fetchRules();
    } finally {
      setSubmitting(false);
    }
  }

  return (
    <div className="p-6 space-y-6">
      <div className="flex items-center justify-between">
        <h2 className="text-2xl font-semibold" style={{ color: "hsl(var(--foreground))" }}>
          Alert Rules
        </h2>
        <div className="flex items-center gap-3">
          <a
            href="/alerts/history"
            className="px-4 py-2 rounded-md text-sm font-medium border transition-colors"
            style={{
              color: "hsl(var(--foreground))",
              borderColor: "hsl(var(--border))",
              backgroundColor: "hsl(var(--background))",
            }}
          >
            View History
          </a>
          <button
            onClick={() => setShowForm((prev) => !prev)}
            className="px-4 py-2 rounded-md text-sm font-medium transition-colors"
            style={{
              backgroundColor: "hsl(var(--primary))",
              color: "hsl(var(--primary-foreground))",
            }}
          >
            {showForm ? "Cancel" : "Add Rule"}
          </button>
        </div>
      </div>

      {showForm && (
        <form
          onSubmit={handleSubmit}
          className="rounded-lg border p-5 space-y-4"
          style={{
            backgroundColor: "hsl(var(--card))",
            borderColor: "hsl(var(--border))",
          }}
        >
          <h3 className="text-base font-semibold" style={{ color: "hsl(var(--foreground))" }}>
            New Alert Rule
          </h3>

          <div className="grid grid-cols-1 gap-4 sm:grid-cols-2">
            <div className="flex flex-col gap-1">
              <label className="text-sm font-medium" style={{ color: "hsl(var(--muted-foreground))" }}>
                Name
              </label>
              <input
                type="text"
                required
                value={form.name}
                onChange={(e) => setForm((f) => ({ ...f, name: e.target.value }))}
                className="rounded-md border px-3 py-2 text-sm outline-none focus:ring-2"
                style={{
                  backgroundColor: "hsl(var(--background))",
                  borderColor: "hsl(var(--border))",
                  color: "hsl(var(--foreground))",
                }}
              />
            </div>

            <div className="flex flex-col gap-1">
              <label className="text-sm font-medium" style={{ color: "hsl(var(--muted-foreground))" }}>
                Condition Type
              </label>
              <select
                value={form.conditionType}
                onChange={(e) => setForm((f) => ({ ...f, conditionType: e.target.value }))}
                className="rounded-md border px-3 py-2 text-sm outline-none focus:ring-2"
                style={{
                  backgroundColor: "hsl(var(--background))",
                  borderColor: "hsl(var(--border))",
                  color: "hsl(var(--foreground))",
                }}
              >
                <option value="DrawdownExceedsPercent">Drawdown Exceeds Percent</option>
                <option value="DailyLossExceedsAmount">Daily Loss Exceeds Amount</option>
                <option value="PositionSizeExceedsPercent">Position Size Exceeds Percent</option>
              </select>
            </div>

            <div className="flex flex-col gap-1">
              <label className="text-sm font-medium" style={{ color: "hsl(var(--muted-foreground))" }}>
                Threshold
              </label>
              <input
                type="number"
                required
                value={form.threshold}
                onChange={(e) => setForm((f) => ({ ...f, threshold: Number(e.target.value) }))}
                className="rounded-md border px-3 py-2 text-sm outline-none focus:ring-2"
                style={{
                  backgroundColor: "hsl(var(--background))",
                  borderColor: "hsl(var(--border))",
                  color: "hsl(var(--foreground))",
                }}
              />
            </div>

            <div className="flex flex-col gap-1">
              <label className="text-sm font-medium" style={{ color: "hsl(var(--muted-foreground))" }}>
                Severity
              </label>
              <select
                value={form.severity}
                onChange={(e) => setForm((f) => ({ ...f, severity: e.target.value }))}
                className="rounded-md border px-3 py-2 text-sm outline-none focus:ring-2"
                style={{
                  backgroundColor: "hsl(var(--background))",
                  borderColor: "hsl(var(--border))",
                  color: "hsl(var(--foreground))",
                }}
              >
                <option value="Info">Info</option>
                <option value="Warning">Warning</option>
                <option value="Critical">Critical</option>
              </select>
            </div>
          </div>

          <div className="flex items-center gap-6">
            <label className="flex items-center gap-2 text-sm cursor-pointer" style={{ color: "hsl(var(--foreground))" }}>
              <input
                type="checkbox"
                checked={form.sendEmail}
                onChange={(e) => setForm((f) => ({ ...f, sendEmail: e.target.checked }))}
                className="h-4 w-4 rounded"
              />
              Send Email
            </label>
            <label className="flex items-center gap-2 text-sm cursor-pointer" style={{ color: "hsl(var(--foreground))" }}>
              <input
                type="checkbox"
                checked={form.sendSms}
                onChange={(e) => setForm((f) => ({ ...f, sendSms: e.target.checked }))}
                className="h-4 w-4 rounded"
              />
              Send SMS
            </label>
          </div>

          <div className="flex justify-end gap-3 pt-2">
            <button
              type="button"
              onClick={() => { setShowForm(false); setForm(defaultForm); }}
              className="px-4 py-2 rounded-md text-sm font-medium border transition-colors"
              style={{
                color: "hsl(var(--foreground))",
                borderColor: "hsl(var(--border))",
                backgroundColor: "hsl(var(--background))",
              }}
            >
              Cancel
            </button>
            <button
              type="submit"
              disabled={submitting}
              className="px-4 py-2 rounded-md text-sm font-medium transition-colors disabled:opacity-50"
              style={{
                backgroundColor: "hsl(var(--primary))",
                color: "hsl(var(--primary-foreground))",
              }}
            >
              {submitting ? "Saving..." : "Create Rule"}
            </button>
          </div>
        </form>
      )}

      <div
        className="rounded-lg border overflow-hidden"
        style={{
          backgroundColor: "hsl(var(--card))",
          borderColor: "hsl(var(--border))",
        }}
      >
        {loading ? (
          <div className="p-8 text-center text-sm" style={{ color: "hsl(var(--muted-foreground))" }}>
            Loading alert rules...
          </div>
        ) : rules.length === 0 ? (
          <div className="p-8 text-center text-sm" style={{ color: "hsl(var(--muted-foreground))" }}>
            No alert rules configured
          </div>
        ) : (
          <div className="overflow-x-auto">
            <table className="w-full text-sm">
              <thead>
                <tr style={{ borderBottomColor: "hsl(var(--border))", borderBottomWidth: 1 }}>
                  {["Name", "Condition", "Threshold", "Severity", "Email", "SMS", "Active", "Actions"].map((col) => (
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
                {rules.map((rule, idx) => (
                  <tr
                    key={rule.id}
                    style={{
                      borderTopColor: idx === 0 ? "transparent" : "hsl(var(--border))",
                      borderTopWidth: idx === 0 ? 0 : 1,
                    }}
                  >
                    <td className="px-4 py-3 font-medium" style={{ color: "hsl(var(--foreground))" }}>
                      {rule.name}
                    </td>
                    <td className="px-4 py-3" style={{ color: "hsl(var(--muted-foreground))" }}>
                      {rule.conditionType}
                    </td>
                    <td className="px-4 py-3" style={{ color: "hsl(var(--foreground))" }}>
                      {rule.threshold}
                    </td>
                    <td className="px-4 py-3">
                      <span
                        className={`inline-flex items-center px-2 py-0.5 rounded-full text-xs font-semibold ${severityColor(rule.severity)}`}
                      >
                        {rule.severity}
                      </span>
                    </td>
                    <td className="px-4 py-3 text-center">
                      {rule.sendEmail ? (
                        <span style={{ color: "hsl(var(--foreground))" }}>&#10003;</span>
                      ) : (
                        <span style={{ color: "hsl(var(--muted-foreground))" }}>&#8212;</span>
                      )}
                    </td>
                    <td className="px-4 py-3 text-center">
                      {rule.sendSms ? (
                        <span style={{ color: "hsl(var(--foreground))" }}>&#10003;</span>
                      ) : (
                        <span style={{ color: "hsl(var(--muted-foreground))" }}>&#8212;</span>
                      )}
                    </td>
                    <td className="px-4 py-3">
                      <button
                        onClick={() => handleToggle(rule.id)}
                        className="relative inline-flex h-5 w-9 items-center rounded-full transition-colors focus:outline-none focus:ring-2 focus:ring-offset-1"
                        style={{
                          backgroundColor: rule.isActive
                            ? "hsl(var(--primary))"
                            : "hsl(var(--muted))",
                        }}
                        aria-label={rule.isActive ? "Deactivate rule" : "Activate rule"}
                      >
                        <span
                          className="inline-block h-3.5 w-3.5 transform rounded-full transition-transform"
                          style={{
                            backgroundColor: "hsl(var(--primary-foreground))",
                            transform: rule.isActive ? "translateX(18px)" : "translateX(3px)",
                          }}
                        />
                      </button>
                    </td>
                    <td className="px-4 py-3">
                      <button
                        onClick={() => handleDelete(rule.id)}
                        className="px-3 py-1 rounded-md text-xs font-medium transition-colors"
                        style={{
                          backgroundColor: "hsl(var(--destructive) / 0.1)",
                          color: "hsl(var(--destructive))",
                        }}
                      >
                        Delete
                      </button>
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
