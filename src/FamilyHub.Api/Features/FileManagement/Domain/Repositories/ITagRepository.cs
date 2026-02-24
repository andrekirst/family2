using FamilyHub.Api.Features.FileManagement.Domain.ValueObjects;
using FamilyHub.Common.Domain.ValueObjects;

namespace FamilyHub.Api.Features.FileManagement.Domain.Repositories;

public interface ITagRepository
{
    Task<Entities.Tag?> GetByIdAsync(TagId id, CancellationToken ct = default);
    Task<List<Entities.Tag>> GetByFamilyIdAsync(FamilyId familyId, CancellationToken ct = default);
    Task<Entities.Tag?> GetByNameAsync(TagName name, FamilyId familyId, CancellationToken ct = default);
    Task AddAsync(Entities.Tag tag, CancellationToken ct = default);
    Task RemoveAsync(Entities.Tag tag, CancellationToken ct = default);
}
