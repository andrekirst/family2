using FamilyHub.Api.Features.FileManagement.Domain.Entities;
using FamilyHub.Api.Features.FileManagement.Domain.Repositories;
using FamilyHub.Api.Features.FileManagement.Domain.ValueObjects;
using FamilyHub.Common.Domain.ValueObjects;

namespace FamilyHub.TestCommon.Fakes;

public class FakeTagRepository : ITagRepository
{
    public List<Tag> Tags { get; } = [];

    public Task<Tag?> GetByIdAsync(TagId id, CancellationToken ct = default)
        => Task.FromResult(Tags.FirstOrDefault(t => t.Id == id));

    public Task<List<Tag>> GetByFamilyIdAsync(FamilyId familyId, CancellationToken ct = default)
        => Task.FromResult(Tags.Where(t => t.FamilyId == familyId).OrderBy(t => t.Name).ToList());

    public Task<Tag?> GetByNameAsync(TagName name, FamilyId familyId, CancellationToken ct = default)
        => Task.FromResult(Tags.FirstOrDefault(t => t.FamilyId == familyId && t.Name == name));

    public Task AddAsync(Tag tag, CancellationToken ct = default)
    {
        Tags.Add(tag);
        return Task.CompletedTask;
    }

    public Task RemoveAsync(Tag tag, CancellationToken ct = default)
    {
        Tags.Remove(tag);
        return Task.CompletedTask;
    }
}
