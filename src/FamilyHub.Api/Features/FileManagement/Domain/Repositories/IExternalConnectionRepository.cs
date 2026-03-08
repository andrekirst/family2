using FamilyHub.Common.Domain;
using FamilyHub.Api.Features.FileManagement.Domain.Entities;
using FamilyHub.Api.Features.FileManagement.Domain.ValueObjects;
using FamilyHub.Common.Domain.ValueObjects;

namespace FamilyHub.Api.Features.FileManagement.Domain.Repositories;

public interface IExternalConnectionRepository : IWriteRepository<ExternalConnection, ExternalConnectionId>
{
    Task<List<ExternalConnection>> GetByFamilyIdAsync(FamilyId familyId, CancellationToken ct = default);
    Task<ExternalConnection?> GetByFamilyAndProviderAsync(FamilyId familyId, ExternalProviderType providerType, CancellationToken ct = default);
    Task RemoveAsync(ExternalConnection connection, CancellationToken ct = default);
}
