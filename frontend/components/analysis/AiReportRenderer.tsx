"use client";

import { severityColor } from "@/lib/formatters";
import type { AnalysisReportDto } from "@/lib/types";

interface AiReportRendererProps {
  report: AnalysisReportDto;
}

function assessmentColor(assessment: string): string {
  const lower = assessment.toLowerCase();
  if (lower.includes("strong")) return "bg-profit/10 text-profit border-profit/30";
  if (lower.includes("weak")) return "bg-loss/10 text-loss border-loss/30";
  return "bg-warning/10 text-warning border-warning/30";
}

function overfittingColor(risk: string): string {
  const lower = risk.toLowerCase();
  if (lower === "low") return "bg-profit/10 text-profit";
  if (lower === "high") return "bg-loss/10 text-loss";
  return "bg-warning/10 text-warning";
}

function readinessColor(score: number): string {
  if (score >= 7) return "bg-profit";
  if (score >= 4) return "bg-warning";
  return "bg-loss";
}

export default function AiReportRenderer({ report }: AiReportRendererProps) {
  return (
    <div className="space-y-6">
      <div className="rounded-lg border border-[hsl(var(--border))] bg-[hsl(var(--card))] p-6">
        <div className="flex items-center gap-3 mb-4">
          <h3 className="text-lg font-semibold text-[hsl(var(--foreground))]">AI Assessment</h3>
          <span className={`rounded-full border px-3 py-1 text-xs font-medium ${assessmentColor(report.overallAssessment)}`}>
            {report.overallAssessment}
          </span>
        </div>
        <p className="text-sm leading-relaxed text-[hsl(var(--muted-foreground))]">{report.summary}</p>
      </div>

      <div className="rounded-lg border border-[hsl(var(--border))] bg-[hsl(var(--card))] p-6">
        <h4 className="text-sm font-medium text-[hsl(var(--foreground))] mb-3">Deployment Readiness</h4>
        <div className="flex items-center gap-4">
          <span className="text-3xl font-bold text-[hsl(var(--foreground))]">{report.deploymentReadiness}</span>
          <span className="text-sm text-[hsl(var(--muted-foreground))]">/ 10</span>
          <div className="flex-1">
            <div className="h-3 rounded-full bg-[hsl(var(--muted))]">
              <div
                className={`h-3 rounded-full transition-all ${readinessColor(report.deploymentReadiness)}`}
                style={{ width: `${Math.min(Math.max(report.deploymentReadiness * 10, 0), 100)}%` }}
              />
            </div>
          </div>
        </div>
      </div>

      <div className="rounded-lg border border-[hsl(var(--border))] bg-[hsl(var(--card))] p-6">
        <div className="flex items-center gap-3 mb-3">
          <h4 className="text-sm font-medium text-[hsl(var(--foreground))]">Overfitting Risk</h4>
          <span className={`rounded px-2 py-0.5 text-xs font-medium ${overfittingColor(report.overfittingRisk)}`}>
            {report.overfittingRisk}
          </span>
        </div>
      </div>

      <div className="grid gap-6 md:grid-cols-2">
        <div className="rounded-lg border border-profit/20 bg-profit/5 p-6">
          <h4 className="text-sm font-medium text-profit mb-3">Strengths</h4>
          <ul className="space-y-2">
            {report.strengths.map((s, i) => (
              <li key={i} className="flex items-start gap-2 text-sm text-[hsl(var(--foreground))]">
                <span className="mt-0.5 text-profit">&#10003;</span>
                {s}
              </li>
            ))}
          </ul>
        </div>
        <div className="rounded-lg border border-loss/20 bg-loss/5 p-6">
          <h4 className="text-sm font-medium text-loss mb-3">Weaknesses</h4>
          <ul className="space-y-2">
            {report.weaknesses.map((w, i) => (
              <li key={i} className="flex items-start gap-2 text-sm text-[hsl(var(--foreground))]">
                <span className="mt-0.5 text-loss">&#10007;</span>
                {w}
              </li>
            ))}
          </ul>
        </div>
      </div>

      {report.criticalWarnings.length > 0 && (
        <div className="rounded-lg border border-warning/30 bg-warning/5 p-6">
          <h4 className="text-sm font-medium text-warning mb-3">Critical Warnings</h4>
          <ul className="space-y-2">
            {report.criticalWarnings.map((w, i) => (
              <li key={i} className="flex items-start gap-2 text-sm text-[hsl(var(--foreground))]">
                <span className="mt-0.5 text-warning">&#9888;</span>
                {w}
              </li>
            ))}
          </ul>
        </div>
      )}
    </div>
  );
}
