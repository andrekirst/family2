using FamilyHub.Api.Common.Database;
using FamilyHub.Api.Features.FileManagement.Domain.Repositories;
using FamilyHub.Api.Features.FileManagement.Domain.ValueObjects;
using FamilyHub.Common.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Tag = FamilyHub.Api.Features.FileManagement.Domain.Entities.Tag;

namespace FamilyHub.Api.Features.FileManagement.Infrastructure.Repositories;

public sealed class TagRepository(AppDbContext context) : ITagRepository
{
    public async Task<Tag?> GetByIdAsync(TagId id, CancellationToken cancellationToken = default)
        => await context.Set<Tag>().FindAsync([id], cancellationToken: cancellationToken);

    public async Task<bool> ExistsByIdAsync(TagId id, CancellationToken cancellationToken = default)
        => await context.Set<Tag>().AnyAsync(t => t.Id == id, cancellationToken);

    public async Task<List<Tag>> GetByFamilyIdAsync(FamilyId familyId, CancellationToken cancellationToken = default)
        => await context.Set<Tag>()
            .Where(t => t.FamilyId == familyId)
            .OrderBy(t => t.Name)
            .ToListAsync(cancellationToken);

    public async Task<Tag?> GetByNameAsync(TagName name, FamilyId familyId, CancellationToken cancellationToken = default)
        => await context.Set<Tag>()
            .FirstOrDefaultAsync(t => t.FamilyId == familyId && t.Name == name, cancellationToken);

    public async Task AddAsync(Tag tag, CancellationToken cancellationToken = default)
        => await context.Set<Tag>().AddAsync(tag, cancellationToken);

    public Task RemoveAsync(Tag tag, CancellationToken cancellationToken = default)
    {
        context.Set<Tag>().Remove(tag);
        return Task.CompletedTask;
    }
}
