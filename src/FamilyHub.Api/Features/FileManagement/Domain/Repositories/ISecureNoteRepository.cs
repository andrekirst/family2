using FamilyHub.Api.Features.FileManagement.Domain.Entities;
using FamilyHub.Api.Features.FileManagement.Domain.ValueObjects;
using FamilyHub.Common.Domain.ValueObjects;

namespace FamilyHub.Api.Features.FileManagement.Domain.Repositories;

public interface ISecureNoteRepository
{
    Task<SecureNote?> GetByIdAsync(SecureNoteId id, CancellationToken ct = default);
    Task<List<SecureNote>> GetByUserIdAsync(UserId userId, FamilyId familyId, CancellationToken ct = default);
    Task<List<SecureNote>> GetByUserIdAndCategoryAsync(UserId userId, FamilyId familyId, NoteCategory category, CancellationToken ct = default);
    Task AddAsync(SecureNote note, CancellationToken ct = default);
    Task RemoveAsync(SecureNote note, CancellationToken ct = default);
}
