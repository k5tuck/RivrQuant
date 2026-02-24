using Hangfire;
using Hangfire.InMemory;
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

builder.Services.AddHangfire(config => config.UseInMemoryStorage());
builder.Services.AddHangfireServer();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins("http://localhost:3000", "http://localhost:3001")
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
