namespace RivrQuant.Api.Middleware;

using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using RivrQuant.Domain.Exceptions;

/// <summary>Global exception handling middleware that maps domain exceptions to HTTP status codes.</summary>
public sealed class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    /// <summary>Initializes a new instance of <see cref="ExceptionHandlingMiddleware"/>.</summary>
    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    /// <summary>Invokes the middleware.</summary>
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var (statusCode, title) = exception switch
        {
            BrokerConnectionException => (StatusCodes.Status502BadGateway, "Broker Connection Error"),
            InsufficientFundsException => (StatusCodes.Status422UnprocessableEntity, "Insufficient Funds"),
            BacktestRetrievalException => (StatusCodes.Status502BadGateway, "Backtest Retrieval Error"),
            AiAnalysisException => (StatusCodes.Status502BadGateway, "AI Analysis Error"),
            AlertDeliveryException => (StatusCodes.Status502BadGateway, "Alert Delivery Error"),
            RiskLimitExceededException => (StatusCodes.Status422UnprocessableEntity, "Risk Limit Exceeded"),
            RivrQuantException => (StatusCodes.Status400BadRequest, "Request Error"),
            OperationCanceledException => (StatusCodes.Status499ClientClosedRequest, "Request Cancelled"),
            _ => (StatusCodes.Status500InternalServerError, "Internal Server Error")
        };

        _logger.LogError(exception, "Unhandled exception: {ExceptionType} — {Message}", exception.GetType().Name, exception.Message);

        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = exception.Message,
            Instance = context.Request.Path
        };

        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/problem+json";
        await context.Response.WriteAsync(JsonSerializer.Serialize(problemDetails));
    }
}
