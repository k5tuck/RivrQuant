namespace RivrQuant.Infrastructure;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RivrQuant.Domain.Enums;
using RivrQuant.Domain.Interfaces;
using RivrQuant.Infrastructure.Allocation;
using RivrQuant.Infrastructure.Analysis;
using RivrQuant.Infrastructure.Alerts;
using RivrQuant.Infrastructure.Brokers;
using RivrQuant.Infrastructure.Brokers.Alpaca;
using RivrQuant.Infrastructure.Brokers.Bybit;
using RivrQuant.Infrastructure.Execution;
using RivrQuant.Infrastructure.Exposure;
using RivrQuant.Infrastructure.Persistence;
using RivrQuant.Infrastructure.QuantConnect;
using RivrQuant.Infrastructure.Risk;
using RivrQuant.Infrastructure.Risk.PositionSizing;
using RivrQuant.Infrastructure.Stubs;

/// <summary>Service registration for the Infrastructure layer.</summary>
public static class DependencyInjection
{
    /// <summary>Adds all infrastructure services to the DI container.</summary>
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var databaseProvider = configuration["DATABASE_PROVIDER"] ?? "Sqlite";
        var connectionString = configuration["DATABASE_CONNECTION"] ?? "Data Source=rivrquant.db";

        services.AddDbContext<RivrQuantDbContext>(options =>
        {
            if (databaseProvider.Equals("PostgreSQL", StringComparison.OrdinalIgnoreCase))
                options.UseNpgsql(connectionString);
            else
                options.UseSqlite(connectionString);
        });

        services.Configure<QcConfiguration>(opts =>
        {
            opts.UserId = configuration["QC_USER_ID"] ?? string.Empty;
            opts.ApiToken = configuration["QC_API_TOKEN"] ?? string.Empty;
            var projectIds = configuration["QC_PROJECT_IDS"] ?? string.Empty;
            opts.ProjectIds = projectIds.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList();
            if (int.TryParse(configuration["QC_POLL_INTERVAL_SECONDS"], out var interval))
                opts.PollIntervalSeconds = interval;
        });

        services.Configure<AlpacaConfiguration>(opts =>
        {
            opts.ApiKey    = configuration["ALPACA_API_KEY"]    ?? string.Empty;
            opts.ApiSecret = configuration["ALPACA_API_SECRET"] ?? string.Empty;
            opts.IsPaper   = bool.TryParse(configuration["ALPACA_PAPER"], out var p) && p;
            opts.BaseUrl   = configuration["ALPACA_BASE_URL"]   ?? "https://paper-api.alpaca.markets";
        });

        services.Configure<BybitConfiguration>(opts =>
        {
            opts.ApiKey     = configuration["BYBIT_API_KEY"]    ?? string.Empty;
            opts.ApiSecret  = configuration["BYBIT_API_SECRET"] ?? string.Empty;
            opts.UseTestnet = !bool.TryParse(configuration["BYBIT_USE_TESTNET"], out var t) || t;
        });

        services.Configure<ClaudeConfiguration>(opts =>
        {
            opts.ApiKey    = configuration["ANTHROPIC_API_KEY"]   ?? string.Empty;
            opts.Model     = configuration["ANTHROPIC_MODEL"]     ?? "claude-sonnet-4-20250514";
            if (int.TryParse(configuration["ANTHROPIC_MAX_TOKENS"], out var tokens))
                opts.MaxTokens = tokens;
        });

        services.Configure<SendGridConfiguration>(opts =>
        {
            opts.ApiKey    = configuration["SENDGRID_API_KEY"]      ?? string.Empty;
            opts.FromEmail = configuration["SENDGRID_FROM_EMAIL"]   ?? string.Empty;
            opts.FromName  = configuration["SENDGRID_FROM_NAME"]    ?? "RivrQuant Alerts";
            var recipients = configuration["ALERT_EMAIL_RECIPIENTS"] ?? string.Empty;
            opts.Recipients = recipients.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList();
        });

        services.Configure<TwilioConfiguration>(opts =>
        {
            opts.AccountSid  = configuration["TWILIO_ACCOUNT_SID"]   ?? string.Empty;
            opts.AuthToken   = configuration["TWILIO_AUTH_TOKEN"]     ?? string.Empty;
            opts.FromNumber  = configuration["TWILIO_FROM_NUMBER"]    ?? string.Empty;
            var recipients   = configuration["ALERT_SMS_RECIPIENTS"]  ?? string.Empty;
            opts.Recipients  = recipients.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList();
        });

        services.AddSingleton<QcResultParser>();
        services.AddHttpClient<QcApiClient>();
        services.AddHttpClient<ClaudeAiAnalyzer>();

        services.AddSingleton<IStatisticsEngine, MathNetStatisticsEngine>();
        services.AddSingleton<ClaudePromptBuilder>();
        services.AddSingleton<RegimeDetector>();
        services.AddSingleton<WalkForwardAnalyzer>();
        services.AddSingleton<ParameterSweepRunner>();

        services.AddScoped<IBacktestProvider, QcApiClient>();
        services.AddScoped<IAiAnalyzer, ClaudeAiAnalyzer>();
        services.AddScoped<IAlertService, StubAlertService>();
        services.AddScoped<QcBacktestPoller>();

        // --- Broker clients ---
        // AlpacaBrokerClient uses the Alpaca.Markets SDK (no raw HttpClient needed).
        services.AddScoped<AlpacaBrokerClient>();

        // BybitBrokerClient requires an HttpClient. AddHttpClient<T> registers it as
        // transient with a properly managed HttpMessageHandler from IHttpClientFactory.
        // This fixes the previous bug where AddScoped<BybitBrokerClient>() was used,
        // which could not satisfy the HttpClient constructor parameter.
        services.AddHttpClient<BybitBrokerClient>();

        // Keyed IBrokerClient registrations allow factory and direct keyed resolution.
        // The delegate form ensures the keyed resolution re-uses the same scoped instance
        // that was already constructed by AlpacaBrokerClient's scoped registration.
        services.AddKeyedScoped<IBrokerClient>(BrokerType.Alpaca,
            (sp, _) => sp.GetRequiredService<AlpacaBrokerClient>());
        services.AddKeyedScoped<IBrokerClient>(BrokerType.Bybit,
            (sp, _) => sp.GetRequiredService<BybitBrokerClient>());

        // Factory resolves broker clients by BrokerType.
        services.AddScoped<IBrokerClientFactory, BrokerClientFactory>();

        // IPortfolioTracker: LivePortfolioTracker aggregates across both brokers.
        services.AddScoped<IPortfolioTracker, LivePortfolioTracker>();

        services.AddScoped<SendGridEmailSender>();
        services.AddScoped<TwilioSmsSender>();
        services.AddScoped<AlertDispatcher>();
        services.AddScoped<AlertRuleEvaluator>();

        // --- Risk & Execution Engine ---

        // Execution cost modeling
        services.AddSingleton<SimpleSlippageModel>();
        services.AddSingleton<SpreadEstimator>();
        services.AddSingleton<CommissionCalculator>();
        services.AddScoped<IExecutionCostModel, ExecutionCostAggregator>();
        services.AddScoped<IFillAnalyzer, PostTradeFillAnalyzer>();

        // Position sizing
        services.AddScoped<KellyPositionSizer>();
        services.AddScoped<VolatilityTargetSizer>();
        services.AddScoped<FixedFractionalSizer>();
        services.AddScoped<IPositionSizer, CompositePositionSizer>();

        // Drawdown management
        services.AddScoped<IDrawdownManager, DrawdownManager>();

        // Volatility targeting
        services.AddScoped<IVolatilityTargetEngine, VolatilityTargetEngine>();

        // Ruin probability
        services.AddSingleton<RuinProbabilityCalculator>();

        // Exposure tracking
        services.AddSingleton<SectorMapper>();
        services.AddScoped<CorrelationEngine>();
        services.AddScoped<IExposureTracker, ExposureTracker>();

        // Capital allocation
        services.AddScoped<ICapitalAllocator, EqualWeightAllocator>();
        services.AddScoped<AllocationOrchestrator>();

        return services;
    }
}
