using Hangfire;
using Hangfire.InMemory;
using Hangfire.PostgreSql;
using Microsoft.EntityFrameworkCore;
using RivrQuant.Api.Hubs;
using RivrQuant.Api.Middleware;
using RivrQuant.Api.Notifications;
using RivrQuant.Application;
using RivrQuant.Application.BackgroundJobs;
using RivrQuant.Application.Notifications;
using RivrQuant.Infrastructure;
using RivrQuant.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "RivrQuant API", Version = "v1" });
});

builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddApplication();

builder.Services.AddSignalR();

var isProduction = builder.Environment.IsProduction();
var hangfireConnectionString = builder.Configuration["DATABASE_CONNECTION"];
builder.Services.AddHangfire(config =>
{
    if (isProduction && !string.IsNullOrEmpty(hangfireConnectionString))
        config.UsePostgreSqlStorage(c => c.UseNpgsqlConnection(hangfireConnectionString));
    else
        config.UseInMemoryStorage();
});
builder.Services.AddHangfireServer();

var allowedOrigins = builder.Configuration["CORS_ORIGINS"]?.Split(',')
    ?? new[] { "http://localhost:3000", "http://localhost:3001" };
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins(allowedOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

builder.Services.AddHealthChecks();

// Real-time event publisher — implemented in API layer using IHubContext<TradingHub>.
builder.Services.AddScoped<IRealtimeEventPublisher, SignalREventPublisher>();

// DrawdownMonitorService runs every 15 seconds as a hosted service, bypassing
// Hangfire's 1-minute cron resolution limit. The old Hangfire DrawdownMonitorJob
// registration is intentionally omitted below.
builder.Services.AddHostedService<DrawdownMonitorService>();

var app = builder.Build();

// Apply EF Core migrations on startup so schema changes deploy automatically
// when a new container version starts. Migrate() is idempotent — it only applies
// pending migrations and is safe to call on every startup.
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<RivrQuantDbContext>();
    db.Database.Migrate();
    app.Logger.LogInformation("Database migrations applied successfully");
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<RequestLoggingMiddleware>();
app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseCors();

app.MapControllers();
app.MapHub<TradingHub>("/hubs/trading");
app.MapHangfireDashboard("/hangfire");
app.MapHealthChecks("/health");

var pollInterval = builder.Configuration.GetValue("QC_POLL_INTERVAL_SECONDS", 300);
RecurringJob.AddOrUpdate<BacktestPollingJob>("backtest-polling",  job => job.ExecuteAsync(), $"*/{Math.Max(pollInterval / 60, 1)} * * * *");
RecurringJob.AddOrUpdate<PortfolioSnapshotJob>("portfolio-snapshot", job => job.ExecuteAsync(), "* * * * *");
RecurringJob.AddOrUpdate<AlertEvaluationJob>("alert-evaluation",  job => job.ExecuteAsync(), "*/1 * * * *");
RecurringJob.AddOrUpdate<LivePerformanceComparisonJob>("live-performance", job => job.ExecuteAsync(), "*/5 * * * *");

// Risk & Execution Engine jobs (DrawdownMonitorJob removed — replaced by DrawdownMonitorService hosted service)
RecurringJob.AddOrUpdate<VolatilityUpdateJob>("volatility-update",  job => job.ExecuteAsync(), "*/5 * * * *");
RecurringJob.AddOrUpdate<ExposureSnapshotJob>("exposure-snapshot",  job => job.ExecuteAsync(), "* * * * *");
RecurringJob.AddOrUpdate<CorrelationUpdateJob>("correlation-update", job => job.ExecuteAsync(), "0 * * * *");
RecurringJob.AddOrUpdate<DecayTrackingJob>("decay-tracking",         job => job.ExecuteAsync(), "30 16 * * 1-5");
RecurringJob.AddOrUpdate<CriticalJobWatchdog>("critical-watchdog",   job => job.ExecuteAsync(), "*/2 * * * *");

app.Run();
