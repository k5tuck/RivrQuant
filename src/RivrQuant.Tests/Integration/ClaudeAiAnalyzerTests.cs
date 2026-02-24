using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using RivrQuant.Infrastructure.Analysis;

namespace RivrQuant.Tests.Integration;

/// <summary>
/// Integration tests for ClaudeAiAnalyzer. These tests require a valid Anthropic API key
/// and are skipped when credentials are not available.
/// </summary>
public class ClaudeAiAnalyzerTests
{
    private static bool HasCredentials()
    {
        var apiKey = Environment.GetEnvironmentVariable("ANTHROPIC_API_KEY");
        return !string.IsNullOrEmpty(apiKey) && apiKey != "demo";
    }

    [Fact]
    public void ClaudeConfiguration_Validate_ThrowsOnMissingApiKey()
    {
        var config = new ClaudeConfiguration
        {
            ApiKey = "",
            Model = "claude-sonnet-4-20250514",
            MaxTokens = 4096
        };

        var act = () => config.Validate();

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void ClaudeConfiguration_Validate_SucceedsWithValidConfig()
    {
        var config = new ClaudeConfiguration
        {
            ApiKey = "sk-ant-test-key",
            Model = "claude-sonnet-4-20250514",
            MaxTokens = 4096
        };

        var act = () => config.Validate();

        act.Should().NotThrow();
    }

    [Fact]
    public void ClaudePromptBuilder_BuildSystemPrompt_ReturnsNonEmptyString()
    {
        var builder = new ClaudePromptBuilder();
        var prompt = builder.BuildSystemPrompt();

        prompt.Should().NotBeNullOrWhiteSpace();
        prompt.Should().Contain("quantitative");
    }

    [Fact]
    public void ClaudePromptBuilder_BuildAnalysisPrompt_IncludesStrategyName()
    {
        var promptBuilder = new ClaudePromptBuilder();
        var backtest = new Domain.Models.Backtests.BacktestResult
        {
            StrategyName = "MomentumAlpha",
            StrategyDescription = "A momentum strategy"
        };
        var metrics = new Domain.Models.Backtests.BacktestMetrics
        {
            SharpeRatio = 1.5,
            MaxDrawdown = 0.15,
            WinRate = 0.55,
            ProfitFactor = 1.8,
            TotalTrades = 150
        };
        var prompt = promptBuilder.BuildAnalysisPrompt(backtest, metrics, new List<Domain.Models.Analysis.RegimeClassification>());

        prompt.Should().Contain("MomentumAlpha");
        prompt.Should().Contain("1.5");
        prompt.Should().Contain("momentum");
    }

    [Fact(Skip = "Requires valid Anthropic API key")]
    public async Task AnalyzeBacktestAsync_WithValidCredentials_ReturnsReport()
    {
        if (!HasCredentials()) return;

        var config = Options.Create(new ClaudeConfiguration
        {
            ApiKey = Environment.GetEnvironmentVariable("ANTHROPIC_API_KEY")!,
            Model = "claude-sonnet-4-20250514",
            MaxTokens = 4096
        });
        var logger = new Mock<ILogger<ClaudeAiAnalyzer>>();
        var httpClient = new HttpClient();

        var promptBuilder = new ClaudePromptBuilder();
        var analyzer = new ClaudeAiAnalyzer(httpClient, config, promptBuilder, logger.Object);

        // This would call the real API — only run with valid credentials
        var report = await analyzer.AnalyzeBacktestAsync(
            new Domain.Models.Backtests.BacktestResult
            {
                StrategyName = "TestStrategy",
                StrategyDescription = "Simple momentum strategy"
            },
            new Domain.Models.Backtests.BacktestMetrics
            {
                SharpeRatio = 1.5,
                MaxDrawdown = 0.15,
                WinRate = 0.55,
                ProfitFactor = 1.8,
                TotalTrades = 100
            },
            new List<Domain.Models.Analysis.RegimeClassification>(),
            CancellationToken.None);

        report.Should().NotBeNull();
        report.OverallAssessment.Should().NotBeNullOrEmpty();
    }
}
