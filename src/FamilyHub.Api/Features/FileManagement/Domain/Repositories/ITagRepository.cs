using FamilyHub.Common.Domain;
using FamilyHub.Api.Features.FileManagement.Domain.ValueObjects;
using FamilyHub.Common.Domain.ValueObjects;

namespace FamilyHub.Api.Features.FileManagement.Domain.Repositories;

public interface ITagRepository : IWriteRepository<Entities.Tag, TagId>
{
    Task<List<Entities.Tag>> GetByFamilyIdAsync(FamilyId familyId, CancellationToken ct = default);
    Task<Entities.Tag?> GetByNameAsync(TagName name, FamilyId familyId, CancellationToken ct = default);
    Task RemoveAsync(Entities.Tag tag, CancellationToken ct = default);
}
