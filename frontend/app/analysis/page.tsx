"use client";
import { useState, useEffect } from "react";
import { api } from "@/lib/api";
import { formatDate } from "@/lib/formatters";
import type { AnalysisReportDto } from "@/lib/types";

function assessmentStyle(assessment: string): string {
  const val = assessment.toLowerCase();
  if (val.includes("strong")) return "bg-profit/10 text-profit";
  if (val.includes("moderate")) return "bg-warning/10 text-warning";
  if (val.includes("weak")) return "bg-loss/10 text-loss";
  return "bg-[hsl(var(--muted))] text-[hsl(var(--muted-foreground))]";
}

function overfittingStyle(risk: string): string {
  const val = risk.toLowerCase();
  if (val === "low") return "bg-profit/10 text-profit";
  if (val === "moderate") return "bg-warning/10 text-warning";
  if (val === "high") return "bg-loss/10 text-loss";
  return "bg-[hsl(var(--muted))] text-[hsl(var(--muted-foreground))]";
}

function deploymentBarColor(score: number): string {
  if (score >= 70) return "bg-profit";
  if (score >= 40) return "bg-warning";
  return "bg-loss";
}

export default function AnalysisPage() {
  const [reports, setReports] = useState<AnalysisReportDto[]>([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    api.analysis.list().then(setReports).catch(console.error).finally(() => setLoading(false));
  }, []);

  if (loading) return <div className="animate-pulse text-[hsl(var(--muted-foreground))]">Loading analysis reports...</div>;

  return (
    <div className="space-y-6">
      <h2 className="text-2xl font-bold">AI Analysis Reports</h2>

      <div className="rounded-xl border border-[hsl(var(--border))] bg-[hsl(var(--card))] overflow-hidden">
        <table className="w-full text-sm">
          <thead>
            <tr className="border-b border-[hsl(var(--border))] bg-[hsl(var(--muted))]">
              <th className="p-3 text-left">Backtest</th>
              <th className="p-3 text-left">Assessment</th>
              <th className="p-3 text-left">Deployment Score</th>
              <th className="p-3 text-left">Overfitting Risk</th>
              <th className="p-3 text-right">Warnings</th>
              <th className="p-3 text-left">Date</th>
            </tr>
          </thead>
          <tbody>
            {reports.map((report) => (
              <tr
                key={report.id}
                className="border-b border-[hsl(var(--border))] hover:bg-[hsl(var(--accent))] cursor-pointer"
                onClick={() => (window.location.href = `/analysis/${report.id}`)}
              >
                <td className="p-3 font-medium font-mono text-xs text-[hsl(var(--muted-foreground))]">
                  {report.backtestId.slice(0, 8)}…
                </td>
                <td className="p-3">
                  <span className={`text-xs px-2 py-1 rounded-full capitalize ${assessmentStyle(report.overallAssessment)}`}>
                    {report.overallAssessment}
                  </span>
                </td>
                <td className="p-3">
                  <div className="flex items-center gap-2 min-w-[120px]">
                    <div className="flex-1 h-2 rounded-full bg-[hsl(var(--muted))] overflow-hidden">
                      <div
                        className={`h-full rounded-full ${deploymentBarColor(report.deploymentReadiness)}`}
                        style={{ width: `${Math.min(100, Math.max(0, report.deploymentReadiness))}%` }}
                      />
                    </div>
                    <span className="text-xs font-medium w-8 text-right">{report.deploymentReadiness}</span>
                  </div>
                </td>
                <td className="p-3">
                  <span className={`text-xs px-2 py-1 rounded-full capitalize ${overfittingStyle(report.overfittingRisk)}`}>
                    {report.overfittingRisk}
                  </span>
                </td>
                <td className="p-3 text-right">
                  {report.criticalWarnings.length > 0 ? (
                    <span className="text-xs px-2 py-1 rounded-full bg-loss/10 text-loss font-medium">
                      {report.criticalWarnings.length}
                    </span>
                  ) : (
                    <span className="text-xs text-[hsl(var(--muted-foreground))]">—</span>
                  )}
                </td>
                <td className="p-3 text-[hsl(var(--muted-foreground))]">{formatDate(report.createdAt)}</td>
              </tr>
            ))}
            {reports.length === 0 && (
              <tr>
                <td colSpan={6} className="p-8 text-center text-[hsl(var(--muted-foreground))]">
                  No analysis reports
                </td>
              </tr>
            )}
          </tbody>
        </table>
      </div>
    </div>
  );
}
