namespace RivrQuant.Infrastructure.QuantConnect;

using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RivrQuant.Domain.Exceptions;
using RivrQuant.Domain.Interfaces;
using RivrQuant.Domain.Models.Backtests;

/// <summary>HTTP client for the QuantConnect REST API. Implements <see cref="IBacktestProvider"/>.</summary>
public sealed class QcApiClient : IBacktestProvider
{
    private readonly HttpClient _httpClient;
    private readonly QcConfiguration _config;
    private readonly QcResultParser _parser;
    private readonly ILogger<QcApiClient> _logger;

    /// <summary>Initializes a new instance of <see cref="QcApiClient"/>.</summary>
    public QcApiClient(HttpClient httpClient, IOptions<QcConfiguration> config, QcResultParser parser, ILogger<QcApiClient> logger)
    {
        _httpClient = httpClient;
        _config = config.Value;
        _parser = parser;
        _logger = logger;

        var credentials = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{_config.UserId}:{_config.ApiToken}"));
        _httpClient.BaseAddress = new Uri(_config.BaseUrl);
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", credentials);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<(string Id, string Name)>> GetProjectsAsync(CancellationToken ct)
    {
        _logger.LogInformation("Fetching project list from QuantConnect");
        try
        {
            var response = await _httpClient.GetAsync("/api/v2/projects/read", ct);
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync(ct);
            var doc = JsonDocument.Parse(json);
            var projects = new List<(string Id, string Name)>();
            if (doc.RootElement.TryGetProperty("projects", out var projectsArray))
            {
                foreach (var project in projectsArray.EnumerateArray())
                {
                    if (project.TryGetProperty("projectId", out var id))
                    {
                        var projectId = id.GetInt64().ToString();
                        var projectName = project.TryGetProperty("name", out var nameEl)
                            ? nameEl.GetString() ?? projectId
                            : projectId;
                        projects.Add((projectId, projectName));
                    }
                }
            }
            _logger.LogInformation("Found {ProjectCount} projects on QuantConnect", projects.Count);
            return projects;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Failed to fetch projects from QuantConnect API");
            throw new BacktestRetrievalException("Failed to fetch project list from QuantConnect API.", ex)
            {
                ProjectId = "all"
            };
        }
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<string>> GetProjectIdsAsync(CancellationToken ct)
    {
        var projects = await GetProjectsAsync(ct);
        return projects.Select(p => p.Id).ToList();
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<BacktestResult>> GetBacktestsForProjectAsync(string projectId, CancellationToken ct)
    {
        _logger.LogInformation("Fetching backtests for project {ProjectId}", projectId);
        try
        {
            var response = await _httpClient.GetAsync($"/api/v2/backtests/read?projectId={projectId}", ct);
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync(ct);
            var doc = JsonDocument.Parse(json);
            var results = new List<BacktestResult>();
            if (doc.RootElement.TryGetProperty("backtests", out var backtestsArray))
            {
                foreach (var bt in backtestsArray.EnumerateArray())
                {
                    results.Add(_parser.ParseBacktestResult(bt, projectId));
                }
            }
            _logger.LogInformation("Found {BacktestCount} backtests for project {ProjectId}", results.Count, projectId);
            return results;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Failed to fetch backtests for project {ProjectId}", projectId);
            throw new BacktestRetrievalException($"Failed to fetch backtests for project {projectId}.", ex)
            {
                ProjectId = projectId
            };
        }
    }

    /// <inheritdoc />
    public async Task<BacktestResult> GetBacktestDetailAsync(string projectId, string backtestId, CancellationToken ct)
    {
        _logger.LogInformation("Fetching backtest detail {BacktestId} for project {ProjectId}", backtestId, projectId);
        try
        {
            var response = await _httpClient.GetAsync($"/api/v2/backtests/read?projectId={projectId}&backtestId={backtestId}", ct);
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync(ct);
            var doc = JsonDocument.Parse(json);
            if (!doc.RootElement.TryGetProperty("backtest", out var backtestElement))
            {
                throw new BacktestRetrievalException($"Backtest {backtestId} not found in response for project {projectId}.")
                {
                    BacktestId = backtestId,
                    ProjectId = projectId
                };
            }
            var result = _parser.ParseBacktestResult(backtestElement, projectId);
            _logger.LogInformation("Successfully fetched backtest {BacktestId} with {TradeCount} trades", backtestId, result.Trades.Count);
            return result;
        }
        catch (BacktestRetrievalException)
        {
            throw;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Failed to fetch backtest detail {BacktestId} for project {ProjectId}", backtestId, projectId);
            throw new BacktestRetrievalException($"Failed to fetch backtest {backtestId} for project {projectId}.", ex)
            {
                BacktestId = backtestId,
                ProjectId = projectId
            };
        }
    }
}
