using FamilyHub.Api.Features.FileManagement.Domain.Entities;
using FamilyHub.Api.Features.FileManagement.Domain.Repositories;
using FamilyHub.Api.Features.FileManagement.Domain.ValueObjects;
using FamilyHub.Common.Domain.ValueObjects;

namespace FamilyHub.TestCommon.Fakes;

public class FakeExternalConnectionRepository : IExternalConnectionRepository
{
    public List<ExternalConnection> Connections { get; } = [];

    public Task<ExternalConnection?> GetByIdAsync(ExternalConnectionId id, CancellationToken ct = default)
        => Task.FromResult(Connections.FirstOrDefault(c => c.Id == id));

    public Task<List<ExternalConnection>> GetByFamilyIdAsync(FamilyId familyId, CancellationToken ct = default)
        => Task.FromResult(Connections
            .Where(c => c.FamilyId == familyId)
            .OrderBy(c => c.ProviderType)
            .ToList());

    public Task<ExternalConnection?> GetByFamilyAndProviderAsync(
        FamilyId familyId, ExternalProviderType providerType, CancellationToken ct = default)
        => Task.FromResult(Connections.FirstOrDefault(c =>
            c.FamilyId == familyId && c.ProviderType == providerType));

    public Task AddAsync(ExternalConnection connection, CancellationToken ct = default)
    {
        Connections.Add(connection);
        return Task.CompletedTask;
    }

    public Task RemoveAsync(ExternalConnection connection, CancellationToken ct = default)
    {
        Connections.Remove(connection);
        return Task.CompletedTask;
    }
}
