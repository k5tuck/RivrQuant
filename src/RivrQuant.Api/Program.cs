using Hangfire;
using Hangfire.InMemory;
using Hangfire.PostgreSql;
using Microsoft.EntityFrameworkCore;
using RivrQuant.Api.Hubs;
using RivrQuant.Api.Middleware;
using RivrQuant.Application;
using RivrQuant.Application.BackgroundJobs;
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

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<RivrQuantDbContext>();
    // EnsureCreated creates the full schema from the current model on first run
    // (safe for both SQLite dev and fresh PostgreSQL prod).
    // Once EF migrations are scaffolded (dotnet ef migrations add Initial),
    // switch this to db.Database.Migrate() so future schema changes apply automatically.
    db.Database.EnsureCreated();
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
RecurringJob.AddOrUpdate<BacktestPollingJob>("backtest-polling", job => job.ExecuteAsync(), $"*/{Math.Max(pollInterval / 60, 1)} * * * *");
RecurringJob.AddOrUpdate<PortfolioSnapshotJob>("portfolio-snapshot", job => job.ExecuteAsync(), "* * * * *");
RecurringJob.AddOrUpdate<AlertEvaluationJob>("alert-evaluation", job => job.ExecuteAsync(), "*/1 * * * *");
RecurringJob.AddOrUpdate<LivePerformanceComparisonJob>("live-performance", job => job.ExecuteAsync(), "*/5 * * * *");

app.Run();
