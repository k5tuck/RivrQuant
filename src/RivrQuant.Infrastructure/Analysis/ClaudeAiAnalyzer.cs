namespace RivrQuant.Infrastructure.Analysis;

using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RivrQuant.Domain.Exceptions;
using RivrQuant.Domain.Interfaces;
using RivrQuant.Domain.Models.Analysis;
using RivrQuant.Domain.Models.Backtests;

/// <summary>Implements <see cref="IAiAnalyzer"/> using the Anthropic Claude API.</summary>
public sealed class ClaudeAiAnalyzer : IAiAnalyzer
{
    private readonly HttpClient _httpClient;
    private readonly ClaudeConfiguration _config;
    private readonly ClaudePromptBuilder _promptBuilder;
    private readonly ILogger<ClaudeAiAnalyzer> _logger;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    /// <summary>Initializes a new instance of <see cref="ClaudeAiAnalyzer"/>.</summary>
    public ClaudeAiAnalyzer(
        HttpClient httpClient,
        IOptions<ClaudeConfiguration> config,
        ClaudePromptBuilder promptBuilder,
        ILogger<ClaudeAiAnalyzer> logger)
    {
        _httpClient = httpClient;
        _config = config.Value;
        _promptBuilder = promptBuilder;
        _logger = logger;
        _httpClient.BaseAddress = new Uri(_config.BaseUrl);
    }

    /// <inheritdoc />
    public async Task<AiAnalysisReport> AnalyzeBacktestAsync(
        BacktestResult backtest,
        BacktestMetrics metrics,
        IReadOnlyList<RegimeClassification> regimes,
        CancellationToken ct)
    {
        _logger.LogInformation("Starting AI analysis for backtest {BacktestId} ({StrategyName})", backtest.Id, backtest.StrategyName);
        var systemPrompt = _promptBuilder.BuildSystemPrompt();
        var userPrompt = _promptBuilder.BuildAnalysisPrompt(backtest, metrics, regimes);

        try
        {
            var (responseText, tokensUsed) = await CallClaudeApiAsync(systemPrompt, userPrompt, ct);
            var report = ParseAnalysisResponse(responseText, backtest.Id, tokensUsed);
            _logger.LogInformation(
                "AI analysis complete for backtest {BacktestId}. Deployment readiness: {Score}/10. Tokens used: {Tokens}",
                backtest.Id, report.DeploymentReadiness, tokensUsed);
            return report;
        }
        catch (AiAnalysisException)
        {
            throw;
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "AI analysis failed for backtest {BacktestId}", backtest.Id);
            throw new AiAnalysisException($"AI analysis failed for backtest {backtest.Id}: {ex.Message}", ex)
            {
                BacktestId = backtest.Id.ToString(),
                AnalysisStep = "ClaudeApiCall"
            };
        }
    }

    /// <inheritdoc />
    public async Task<AiAnalysisReport> CompareBacktestsAsync(
        IReadOnlyList<BacktestResult> backtests,
        IReadOnlyList<BacktestMetrics> metrics,
        CancellationToken ct)
    {
        _logger.LogInformation("Starting AI comparison of {BacktestCount} backtests", backtests.Count);
        var systemPrompt = _promptBuilder.BuildSystemPrompt();
        var userPrompt = _promptBuilder.BuildComparisonPrompt(backtests, metrics);

        try
        {
            var (responseText, tokensUsed) = await CallClaudeApiAsync(systemPrompt, userPrompt, ct);
            var report = ParseAnalysisResponse(responseText, backtests.FirstOrDefault()?.Id ?? Guid.Empty, tokensUsed);
            _logger.LogInformation("AI comparison complete. Tokens used: {Tokens}", tokensUsed);
            return report;
        }
        catch (AiAnalysisException)
        {
            throw;
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "AI comparison failed");
            throw new AiAnalysisException($"AI comparison failed: {ex.Message}", ex)
            {
                AnalysisStep = "ClaudeApiCall"
            };
        }
    }

    private async Task<(string ResponseText, int TokensUsed)> CallClaudeApiAsync(string systemPrompt, string userPrompt, CancellationToken ct)
    {
        var requestBody = new
        {
            model = _config.Model,
            max_tokens = _config.MaxTokens,
            system = systemPrompt,
            messages = new[]
            {
                new { role = "user", content = userPrompt }
            }
        };

        var json = JsonSerializer.Serialize(requestBody, JsonOptions);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        using var request = new HttpRequestMessage(HttpMethod.Post, "/v1/messages");
        request.Content = content;
        request.Headers.Add("x-api-key", _config.ApiKey);
        request.Headers.Add("anthropic-version", "2023-06-01");

        var response = await _httpClient.SendAsync(request, ct);
        var responseBody = await response.Content.ReadAsStringAsync(ct);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("Claude API returned {StatusCode}: {ResponseBody}", response.StatusCode, responseBody);
            throw new AiAnalysisException($"Claude API returned {response.StatusCode}: {responseBody}")
            {
                AnalysisStep = "HttpRequest"
            };
        }

        var doc = JsonDocument.Parse(responseBody);
        var text = string.Empty;
        var tokensUsed = 0;

        if (doc.RootElement.TryGetProperty("content", out var contentArray) && contentArray.GetArrayLength() > 0)
        {
            text = contentArray[0].GetProperty("text").GetString() ?? string.Empty;
        }

        if (doc.RootElement.TryGetProperty("usage", out var usage))
        {
            var inputTokens = usage.TryGetProperty("input_tokens", out var it) ? it.GetInt32() : 0;
            var outputTokens = usage.TryGetProperty("output_tokens", out var ot) ? ot.GetInt32() : 0;
            tokensUsed = inputTokens + outputTokens;
        }

        return (text, tokensUsed);
    }

    private AiAnalysisReport ParseAnalysisResponse(string responseText, Guid backtestResultId, int tokensUsed)
    {
        try
        {
            var trimmed = responseText.Trim();
            if (trimmed.StartsWith("```"))
            {
                var startIdx = trimmed.IndexOf('\n') + 1;
                var endIdx = trimmed.LastIndexOf("```");
                if (startIdx > 0 && endIdx > startIdx)
                    trimmed = trimmed[startIdx..endIdx].Trim();
            }

            var parsed = JsonDocument.Parse(trimmed);
            var root = parsed.RootElement;

            return new AiAnalysisReport
            {
                BacktestResultId = backtestResultId,
                OverallAssessment = root.TryGetProperty("overallAssessment", out var oa) ? oa.GetString() ?? string.Empty : string.Empty,
                Strengths = ParseStringArray(root, "strengths"),
                Weaknesses = ParseStringArray(root, "weaknesses"),
                OverfittingRisk = root.TryGetProperty("overfittingRisk", out var or) ? or.GetString() ?? "unknown" : "unknown",
                OverfittingExplanation = root.TryGetProperty("overfittingExplanation", out var oe) ? oe.GetString() ?? string.Empty : string.Empty,
                ParameterSuggestions = ParseStringArray(root, "parameterSuggestions"),
                RegimeAnalysis = root.TryGetProperty("regimeAnalysis", out var ra) ? ra.GetString() ?? string.Empty : string.Empty,
                DeploymentReadiness = root.TryGetProperty("deploymentReadiness", out var dr) ? dr.GetInt32() : 0,
                PlainEnglishSummary = root.TryGetProperty("plainEnglishSummary", out var pes) ? pes.GetString() ?? string.Empty : string.Empty,
                CriticalWarnings = ParseStringArray(root, "criticalWarnings"),
                RawResponse = responseText,
                TokensUsed = tokensUsed,
                EstimatedCost = tokensUsed * 0.000003m
            };
        }
        catch (JsonException ex)
        {
            _logger.LogWarning(ex, "Failed to parse Claude response as JSON. Storing raw response.");
            return new AiAnalysisReport
            {
                BacktestResultId = backtestResultId,
                OverallAssessment = responseText,
                PlainEnglishSummary = responseText,
                RawResponse = responseText,
                TokensUsed = tokensUsed,
                EstimatedCost = tokensUsed * 0.000003m
            };
        }
    }

    private static IReadOnlyList<string> ParseStringArray(JsonElement root, string propertyName)
    {
        if (!root.TryGetProperty(propertyName, out var arr) || arr.ValueKind != JsonValueKind.Array)
            return Array.Empty<string>();

        var items = new List<string>();
        foreach (var item in arr.EnumerateArray())
        {
            var value = item.GetString();
            if (!string.IsNullOrEmpty(value))
                items.Add(value);
        }
        return items;
    }
}
