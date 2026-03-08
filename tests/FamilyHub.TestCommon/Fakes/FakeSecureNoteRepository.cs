using FamilyHub.Api.Features.FileManagement.Domain.Entities;
using FamilyHub.Api.Features.FileManagement.Domain.Repositories;
using FamilyHub.Api.Features.FileManagement.Domain.ValueObjects;
using FamilyHub.Common.Domain.ValueObjects;

namespace FamilyHub.TestCommon.Fakes;

public class FakeSecureNoteRepository : ISecureNoteRepository
{
    public List<SecureNote> Notes { get; } = [];

    public Task<SecureNote?> GetByIdAsync(SecureNoteId id, CancellationToken cancellationToken = default)
        => Task.FromResult(Notes.FirstOrDefault(n => n.Id == id));

    public Task<bool> ExistsByIdAsync(SecureNoteId id, CancellationToken cancellationToken = default)
        => Task.FromResult(Notes.Any(n => n.Id == id));

    public Task<List<SecureNote>> GetByUserIdAsync(UserId userId, FamilyId familyId, CancellationToken cancellationToken = default)
        => Task.FromResult(Notes
            .Where(n => n.UserId == userId && n.FamilyId == familyId)
            .OrderByDescending(n => n.UpdatedAt)
            .ToList());

    public Task<List<SecureNote>> GetByUserIdAndCategoryAsync(
        UserId userId, FamilyId familyId, NoteCategory category, CancellationToken cancellationToken = default)
        => Task.FromResult(Notes
            .Where(n => n.UserId == userId && n.FamilyId == familyId && n.Category == category)
            .OrderByDescending(n => n.UpdatedAt)
            .ToList());

    public Task AddAsync(SecureNote note, CancellationToken cancellationToken = default)
    {
        Notes.Add(note);
        return Task.CompletedTask;
    }

    public Task RemoveAsync(SecureNote note, CancellationToken cancellationToken = default)
    {
        Notes.Remove(note);
        return Task.CompletedTask;
    }
}
