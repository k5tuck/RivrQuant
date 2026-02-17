namespace RivrQuant.Infrastructure.Analysis;

/// <summary>Configuration for the Anthropic Claude API.</summary>
public sealed class ClaudeConfiguration
{
    /// <summary>Configuration section name.</summary>
    public const string SectionName = "Anthropic";

    /// <summary>Anthropic API key.</summary>
    public string ApiKey { get; set; } = string.Empty;

    /// <summary>Claude model to use for analysis.</summary>
    public string Model { get; set; } = "claude-sonnet-4-20250514";

    /// <summary>Maximum tokens in the response.</summary>
    public int MaxTokens { get; set; } = 4096;

    /// <summary>Base URL for the Anthropic API.</summary>
    public string BaseUrl { get; init; } = "https://api.anthropic.com";

    /// <summary>Validates that required configuration values are present.</summary>
    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(ApiKey))
            throw new InvalidOperationException("ANTHROPIC_API_KEY is required. Set it in environment variables or appsettings.");
    }
}
