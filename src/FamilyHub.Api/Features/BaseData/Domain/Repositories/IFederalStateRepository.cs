using FamilyHub.Api.Features.BaseData.Domain.Entities;
using FamilyHub.Api.Features.BaseData.Domain.ValueObjects;

namespace FamilyHub.Api.Features.BaseData.Domain.Repositories;

public interface IFederalStateRepository
{
    Task<List<FederalState>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<FederalState?> GetByIso3166CodeAsync(Iso3166Code code, CancellationToken cancellationToken = default);

    Task<bool> AnyAsync(CancellationToken cancellationToken = default);

    Task AddRangeAsync(IEnumerable<FederalState> entities, CancellationToken cancellationToken = default);
}
