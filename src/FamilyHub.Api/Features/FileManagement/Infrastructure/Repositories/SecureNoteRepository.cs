using FamilyHub.Api.Common.Database;
using FamilyHub.Api.Features.FileManagement.Domain.Entities;
using FamilyHub.Api.Features.FileManagement.Domain.Repositories;
using FamilyHub.Api.Features.FileManagement.Domain.ValueObjects;
using FamilyHub.Common.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace FamilyHub.Api.Features.FileManagement.Infrastructure.Repositories;

public sealed class SecureNoteRepository(AppDbContext context) : ISecureNoteRepository
{
    public async Task<SecureNote?> GetByIdAsync(SecureNoteId id, CancellationToken cancellationToken = default)
        => await context.Set<SecureNote>().FirstOrDefaultAsync(n => n.Id == id, cancellationToken);

    public async Task<bool> ExistsByIdAsync(SecureNoteId id, CancellationToken cancellationToken = default)
        => await context.Set<SecureNote>().AnyAsync(n => n.Id == id, cancellationToken);

    public async Task<List<SecureNote>> GetByUserIdAsync(UserId userId, FamilyId familyId, CancellationToken cancellationToken = default)
        => await context.Set<SecureNote>()
            .Where(n => n.UserId == userId && n.FamilyId == familyId)
            .OrderByDescending(n => n.UpdatedAt)
            .ToListAsync(cancellationToken);

    public async Task<List<SecureNote>> GetByUserIdAndCategoryAsync(
        UserId userId, FamilyId familyId, NoteCategory category, CancellationToken cancellationToken = default)
        => await context.Set<SecureNote>()
            .Where(n => n.UserId == userId && n.FamilyId == familyId && n.Category == category)
            .OrderByDescending(n => n.UpdatedAt)
            .ToListAsync(cancellationToken);

    public async Task AddAsync(SecureNote note, CancellationToken cancellationToken = default)
        => await context.Set<SecureNote>().AddAsync(note, cancellationToken);

    public Task RemoveAsync(SecureNote note, CancellationToken cancellationToken = default)
    {
        context.Set<SecureNote>().Remove(note);
        return Task.CompletedTask;
    }
}
