using FamilyHub.Common.Domain;
using FamilyHub.Api.Features.FileManagement.Domain.Entities;
using FamilyHub.Api.Features.FileManagement.Domain.ValueObjects;
using FamilyHub.Common.Domain.ValueObjects;

namespace FamilyHub.Api.Features.FileManagement.Domain.Repositories;

public interface IExternalConnectionRepository : IWriteRepository<ExternalConnection, ExternalConnectionId>
{
    Task<List<ExternalConnection>> GetByFamilyIdAsync(FamilyId familyId, CancellationToken cancellationToken = default);
    Task<ExternalConnection?> GetByFamilyAndProviderAsync(FamilyId familyId, ExternalProviderType providerType, CancellationToken cancellationToken = default);
    Task RemoveAsync(ExternalConnection connection, CancellationToken cancellationToken = default);
}
