using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RivrQuant.Domain.Models.Strategies;
using RivrQuant.Infrastructure.Persistence;

namespace RivrQuant.Application.Services;

/// <summary>
/// Application service for managing trading strategy definitions and their parameters.
/// </summary>
public sealed class StrategyService
{
    private readonly RivrQuantDbContext _db;
    private readonly ILogger<StrategyService> _logger;

    /// <summary>Initializes a new instance of <see cref="StrategyService"/>.</summary>
    public StrategyService(RivrQuantDbContext db, ILogger<StrategyService> logger)
    {
        _db = db;
        _logger = logger;
    }

    /// <summary>Lists all strategies with their parameters.</summary>
    public async Task<IReadOnlyList<Strategy>> GetAllAsync(CancellationToken ct)
    {
        return await _db.Strategies
            .Include(s => s.Parameters)
            .OrderByDescending(s => s.CreatedAt)
            .ToListAsync(ct);
    }

    /// <summary>Retrieves a strategy by identifier.</summary>
    public async Task<Strategy?> GetByIdAsync(Guid id, CancellationToken ct)
    {
        return await _db.Strategies
            .Include(s => s.Parameters)
            .Include(s => s.Versions)
            .FirstOrDefaultAsync(s => s.Id == id, ct);
    }

    /// <summary>Creates a new strategy.</summary>
    public async Task<Strategy> CreateAsync(Strategy strategy, CancellationToken ct)
    {
        _db.Strategies.Add(strategy);

        // Create initial version
        var version = new StrategyVersion
        {
            StrategyId = strategy.Id,
            VersionNumber = 1,
            ChangeNotes = "Initial creation",
            ParametersSnapshot = JsonSerializer.Serialize(strategy.Parameters)
        };
        _db.StrategyVersions.Add(version);

        await _db.SaveChangesAsync(ct);
        _logger.LogInformation("Created strategy {StrategyId} ({StrategyName})", strategy.Id, strategy.Name);
        return strategy;
    }

    /// <summary>Updates an existing strategy.</summary>
    public async Task<Strategy> UpdateAsync(Strategy updated, CancellationToken ct)
    {
        var existing = await _db.Strategies
            .Include(s => s.Parameters)
            .Include(s => s.Versions)
            .FirstOrDefaultAsync(s => s.Id == updated.Id, ct)
            ?? throw new KeyNotFoundException($"Strategy {updated.Id} not found.");

        // Update basic properties using the Entry API
        _db.Entry(existing).CurrentValues.SetValues(new
        {
            updated.Name,
            updated.Description,
            updated.AssetClass,
            updated.IsActive,
            LastModifiedAt = DateTimeOffset.UtcNow
        });

        // Remove old parameters and add new ones
        _db.StrategyParameters.RemoveRange(existing.Parameters);
        foreach (var param in updated.Parameters)
        {
            param.GetType().GetProperty("StrategyId")!.SetValue(param, existing.Id);
            _db.StrategyParameters.Add(param);
        }

        // Create new version
        var latestVersion = existing.Versions.OrderByDescending(v => v.VersionNumber).FirstOrDefault();
        var nextVersion = (latestVersion?.VersionNumber ?? 0) + 1;

        _db.StrategyVersions.Add(new StrategyVersion
        {
            StrategyId = existing.Id,
            VersionNumber = nextVersion,
            ChangeNotes = "Strategy updated",
            ParametersSnapshot = JsonSerializer.Serialize(updated.Parameters)
        });

        await _db.SaveChangesAsync(ct);
        _logger.LogInformation("Updated strategy {StrategyId} to version {Version}", existing.Id, nextVersion);
        return existing;
    }

    /// <summary>Deletes a strategy by identifier.</summary>
    public async Task DeleteAsync(Guid id, CancellationToken ct)
    {
        var strategy = await _db.Strategies
            .Include(s => s.Parameters)
            .Include(s => s.Versions)
            .FirstOrDefaultAsync(s => s.Id == id, ct)
            ?? throw new KeyNotFoundException($"Strategy {id} not found.");

        _db.StrategyVersions.RemoveRange(strategy.Versions);
        _db.StrategyParameters.RemoveRange(strategy.Parameters);
        _db.Strategies.Remove(strategy);
        await _db.SaveChangesAsync(ct);
        _logger.LogInformation("Deleted strategy {StrategyId}", id);
    }
}
