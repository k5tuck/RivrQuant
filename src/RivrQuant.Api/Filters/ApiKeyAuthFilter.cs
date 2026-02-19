namespace RivrQuant.Api.Filters;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

/// <summary>Action filter that validates the X-Api-Key header for API authentication.</summary>
public sealed class ApiKeyAuthFilter : IAsyncActionFilter
{
    private const string ApiKeyHeaderName = "X-Api-Key";
    private readonly IConfiguration _configuration;
    private readonly IWebHostEnvironment _environment;

    /// <summary>Initializes a new instance of <see cref="ApiKeyAuthFilter"/>.</summary>
    public ApiKeyAuthFilter(IConfiguration configuration, IWebHostEnvironment environment)
    {
        _configuration = configuration;
        _environment = environment;
    }

    /// <inheritdoc />
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        if (_environment.IsDevelopment())
        {
            await next();
            return;
        }

        if (!context.HttpContext.Request.Headers.TryGetValue(ApiKeyHeaderName, out var providedKey))
        {
            context.Result = new UnauthorizedObjectResult(new { error = "API key is required. Provide it via X-Api-Key header." });
            return;
        }

        var configuredKey = _configuration["API_KEY"];
        if (string.IsNullOrEmpty(configuredKey) || providedKey != configuredKey)
        {
            context.Result = new UnauthorizedObjectResult(new { error = "Invalid API key." });
            return;
        }

        await next();
    }
}
