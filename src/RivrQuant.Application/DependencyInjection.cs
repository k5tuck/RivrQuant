namespace RivrQuant.Application;

using Microsoft.Extensions.DependencyInjection;
using RivrQuant.Application.Services;
using RivrQuant.Domain.Interfaces;

/// <summary>Service registration for the Application layer.</summary>
public static class DependencyInjection
{
    /// <summary>Adds all application services to the DI container.</summary>
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<BacktestService>();
        services.AddScoped<TradingService>();
        services.AddScoped<DashboardService>();
        services.AddScoped<AnalysisService>();
        services.AddScoped<AlertAppService>();
        services.AddScoped<StrategyService>();
        return services;
    }
}
