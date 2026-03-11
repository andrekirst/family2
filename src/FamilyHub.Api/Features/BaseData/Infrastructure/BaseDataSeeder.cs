using System.Reflection;
using System.Text.Json;
using FamilyHub.Api.Features.BaseData.Domain.Entities;
using FamilyHub.Api.Features.BaseData.Domain.Repositories;
using FamilyHub.Api.Features.BaseData.Domain.ValueObjects;

namespace FamilyHub.Api.Features.BaseData.Infrastructure;

/// <summary>
/// Seeds federal state reference data from embedded JSON resource on application startup.
/// Runs in ALL environments. Idempotent: skips if any federal states already exist.
/// </summary>
public sealed class BaseDataSeeder(
    IServiceScopeFactory scopeFactory,
    ILogger<BaseDataSeeder> logger) : IHostedService
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = scopeFactory.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IFederalStateRepository>();

        try
        {
            var hasData = await repository.AnyAsync(cancellationToken);
            if (hasData)
            {
                logger.LogInformation("BaseDataSeeder: Federal states already seeded, skipping");
                return;
            }

            var seedData = LoadSeedData();
            if (seedData is null || seedData.Count == 0)
            {
                logger.LogWarning("BaseDataSeeder: No seed data found in embedded resource");
                return;
            }

            var entities = seedData.Select(s =>
                FederalState.Create(
                    FederalStateName.From(s.Name),
                    Iso3166Code.From(s.Iso3166Code)))
                .ToList();

            await repository.AddRangeAsync(entities, cancellationToken);

            logger.LogInformation("BaseDataSeeder: Seeded {Count} federal states", entities.Count);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "BaseDataSeeder: Failed to seed federal states");
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    private static List<FederalStateSeedEntry>? LoadSeedData()
    {
        var assembly = Assembly.GetExecutingAssembly();
        const string resourceName = "FamilyHub.Api.Features.BaseData.Data.Seeds.federal-states.json";

        using var stream = assembly.GetManifestResourceStream(resourceName);
        if (stream is null)
        {
            return null;
        }

        return JsonSerializer.Deserialize<List<FederalStateSeedEntry>>(stream, JsonOptions);
    }

    private sealed record FederalStateSeedEntry(string Name, string Iso3166Code);
}
