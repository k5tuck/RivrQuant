"use client";
import { useState, useEffect } from "react";
import { useParams } from "next/navigation";
import { api } from "@/lib/api";
import { formatDate } from "@/lib/formatters";
import type { AnalysisReportDto } from "@/lib/types";

function assessmentBadgeStyle(assessment: string): string {
  const val = assessment.toLowerCase();
  if (val.includes("strong")) return "bg-profit/10 text-profit border border-profit/20";
  if (val.includes("moderate")) return "bg-warning/10 text-warning border border-warning/20";
  if (val.includes("weak")) return "bg-loss/10 text-loss border border-loss/20";
  return "bg-[hsl(var(--muted))] text-[hsl(var(--muted-foreground))] border border-[hsl(var(--border))]";
}

function overfittingBadgeStyle(risk: string): string {
  const val = risk.toLowerCase();
  if (val === "low") return "bg-profit/10 text-profit border border-profit/20";
  if (val === "moderate") return "bg-warning/10 text-warning border border-warning/20";
  if (val === "high") return "bg-loss/10 text-loss border border-loss/20";
  return "bg-[hsl(var(--muted))] text-[hsl(var(--muted-foreground))] border border-[hsl(var(--border))]";
}

function deploymentBarColor(score: number): string {
  if (score >= 70) return "bg-profit";
  if (score >= 40) return "bg-warning";
  return "bg-loss";
}

function deploymentTextColor(score: number): string {
  if (score >= 70) return "text-profit";
  if (score >= 40) return "text-warning";
  return "text-loss";
}

export default function AnalysisDetailPage() {
  const params = useParams();
  const [report, setReport] = useState<AnalysisReportDto | null>(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    if (params.id) {
      api.analysis.get(params.id as string).then(setReport).catch(console.error).finally(() => setLoading(false));
    }
  }, [params.id]);

  if (loading) return <div className="animate-pulse text-[hsl(var(--muted-foreground))]">Loading analysis report...</div>;
  if (!report) return <div className="text-[hsl(var(--muted-foreground))]">Analysis report not found.</div>;

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="space-y-1">
        <a href="/analysis" className="text-sm text-[hsl(var(--muted-foreground))] hover:underline">
          ← AI Analysis Reports
        </a>
        <div className="flex items-center gap-3 mt-2">
          <h2 className="text-2xl font-bold">Overall Assessment</h2>
          <span className={`text-sm px-3 py-1 rounded-full capitalize font-medium ${assessmentBadgeStyle(report.overallAssessment)}`}>
            {report.overallAssessment}
          </span>
        </div>
        <p className="text-xs text-[hsl(var(--muted-foreground))]">
          Created {formatDate(report.createdAt)}
        </p>
      </div>

      {/* Summary */}
      <div className="rounded-xl border border-[hsl(var(--border))] bg-[hsl(var(--card))] p-6">
        <h3 className="text-sm font-semibold text-[hsl(var(--muted-foreground))] uppercase tracking-wide mb-2">Summary</h3>
        <p className="text-sm leading-relaxed">{report.summary}</p>
      </div>

      {/* Deployment Readiness + Overfitting Risk */}
      <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
        {/* Deployment Readiness */}
        <div className="rounded-xl border border-[hsl(var(--border))] bg-[hsl(var(--card))] p-6 space-y-3">
          <h3 className="text-sm font-semibold text-[hsl(var(--muted-foreground))] uppercase tracking-wide">Deployment Readiness</h3>
          <div className="flex items-end gap-2">
            <span className={`text-4xl font-bold ${deploymentTextColor(report.deploymentReadiness)}`}>
              {report.deploymentReadiness}
            </span>
            <span className="text-[hsl(var(--muted-foreground))] text-sm pb-1">/ 100</span>
          </div>
          <div className="h-3 rounded-full bg-[hsl(var(--muted))] overflow-hidden">
            <div
              className={`h-full rounded-full transition-all ${deploymentBarColor(report.deploymentReadiness)}`}
              style={{ width: `${Math.min(100, Math.max(0, report.deploymentReadiness))}%` }}
            />
          </div>
          <p className="text-xs text-[hsl(var(--muted-foreground))]">
            {report.deploymentReadiness >= 70
              ? "Ready for deployment consideration"
              : report.deploymentReadiness >= 40
              ? "Needs improvement before deployment"
              : "Not ready for deployment"}
          </p>
        </div>

        {/* Overfitting Risk */}
        <div className="rounded-xl border border-[hsl(var(--border))] bg-[hsl(var(--card))] p-6 space-y-3">
          <h3 className="text-sm font-semibold text-[hsl(var(--muted-foreground))] uppercase tracking-wide">Overfitting Risk</h3>
          <div className="flex items-center gap-3">
            <span className={`text-sm px-3 py-1.5 rounded-full capitalize font-medium ${overfittingBadgeStyle(report.overfittingRisk)}`}>
              {report.overfittingRisk}
            </span>
          </div>
          <p className="text-xs text-[hsl(var(--muted-foreground))]">
            {report.overfittingRisk.toLowerCase() === "low"
              ? "Strategy shows good generalization characteristics"
              : report.overfittingRisk.toLowerCase() === "moderate"
              ? "Some signs of curve fitting detected; validate on out-of-sample data"
              : "High likelihood of overfitting; strategy may underperform live"}
          </p>
        </div>
      </div>

      {/* Strengths & Weaknesses */}
      <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
        {/* Strengths */}
        <div className="rounded-xl border border-[hsl(var(--border))] bg-[hsl(var(--card))] p-6 space-y-3">
          <h3 className="text-sm font-semibold text-profit uppercase tracking-wide">Strengths</h3>
          {report.strengths.length > 0 ? (
            <ul className="space-y-2">
              {report.strengths.map((strength, i) => (
                <li key={i} className="flex items-start gap-2 text-sm">
                  <span className="text-profit mt-0.5 shrink-0">&#10003;</span>
                  <span>{strength}</span>
                </li>
              ))}
            </ul>
          ) : (
            <p className="text-sm text-[hsl(var(--muted-foreground))]">No strengths identified.</p>
          )}
        </div>

        {/* Weaknesses */}
        <div className="rounded-xl border border-[hsl(var(--border))] bg-[hsl(var(--card))] p-6 space-y-3">
          <h3 className="text-sm font-semibold text-loss uppercase tracking-wide">Weaknesses</h3>
          {report.weaknesses.length > 0 ? (
            <ul className="space-y-2">
              {report.weaknesses.map((weakness, i) => (
                <li key={i} className="flex items-start gap-2 text-sm">
                  <span className="text-loss mt-0.5 shrink-0">&#10007;</span>
                  <span>{weakness}</span>
                </li>
              ))}
            </ul>
          ) : (
            <p className="text-sm text-[hsl(var(--muted-foreground))]">No weaknesses identified.</p>
          )}
        </div>
      </div>

      {/* Critical Warnings */}
      {report.criticalWarnings.length > 0 && (
        <div className="rounded-xl border border-warning/30 bg-warning/5 p-6 space-y-3">
          <h3 className="text-sm font-semibold text-warning uppercase tracking-wide flex items-center gap-2">
            <span>&#9888;</span>
            Critical Warnings
          </h3>
          <ul className="space-y-2">
            {report.criticalWarnings.map((warning, i) => (
              <li key={i} className="flex items-start gap-2 text-sm">
                <span className="text-warning mt-0.5 shrink-0">&#9888;</span>
                <span className="text-warning/90">{warning}</span>
              </li>
            ))}
          </ul>
        </div>
      )}
    </div>
  );
}
