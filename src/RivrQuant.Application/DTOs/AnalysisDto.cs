// Copyright (c) RivrQuant. All rights reserved.
// Licensed under the MIT License.

namespace RivrQuant.Application.DTOs;

/// <summary>
/// Projection of an AI-generated analysis report for a backtest result,
/// containing strategy assessment, risk evaluation, and deployment readiness scoring.
/// </summary>
/// <param name="Id">Unique identifier for the analysis report.</param>
/// <param name="BacktestId">Identifier of the analyzed backtest result.</param>
/// <param name="OverallAssessment">High-level narrative assessment of strategy viability.</param>
/// <param name="Strengths">Identified strengths in the strategy's performance and design.</param>
/// <param name="Weaknesses">Identified weaknesses or areas of concern.</param>
/// <param name="OverfittingRisk">Assessed overfitting risk level (low, medium, high).</param>
/// <param name="DeploymentReadiness">Deployment readiness score (1-10).</param>
/// <param name="Summary">Plain-English summary suitable for non-technical stakeholders.</param>
/// <param name="CriticalWarnings">Critical warnings to address before live deployment.</param>
/// <param name="CreatedAt">Timestamp when the analysis was generated.</param>
public sealed record AnalysisReportDto(
    Guid Id,
    Guid BacktestId,
    string OverallAssessment,
    IReadOnlyList<string> Strengths,
    IReadOnlyList<string> Weaknesses,
    string OverfittingRisk,
    int DeploymentReadiness,
    string Summary,
    IReadOnlyList<string> CriticalWarnings,
    DateTimeOffset CreatedAt);
