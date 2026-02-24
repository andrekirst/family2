using FamilyHub.Api.Common.Database;
using FamilyHub.Api.Features.FileManagement.Domain.Repositories;
using FamilyHub.Api.Features.FileManagement.Domain.ValueObjects;
using FamilyHub.Common.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Tag = FamilyHub.Api.Features.FileManagement.Domain.Entities.Tag;

namespace FamilyHub.Api.Features.FileManagement.Infrastructure.Repositories;

public sealed class TagRepository(AppDbContext context) : ITagRepository
{
    public async Task<Tag?> GetByIdAsync(TagId id, CancellationToken ct = default)
        => await context.Set<Tag>().FindAsync([id], cancellationToken: ct);

    public async Task<List<Tag>> GetByFamilyIdAsync(FamilyId familyId, CancellationToken ct = default)
        => await context.Set<Tag>()
            .Where(t => t.FamilyId == familyId)
            .OrderBy(t => t.Name)
            .ToListAsync(ct);

    public async Task<Tag?> GetByNameAsync(TagName name, FamilyId familyId, CancellationToken ct = default)
        => await context.Set<Tag>()
            .FirstOrDefaultAsync(t => t.FamilyId == familyId && t.Name == name, ct);

    public async Task AddAsync(Tag tag, CancellationToken ct = default)
        => await context.Set<Tag>().AddAsync(tag, ct);

    public Task RemoveAsync(Tag tag, CancellationToken ct = default)
    {
        context.Set<Tag>().Remove(tag);
        return Task.CompletedTask;
    }
}
