using FamilyHub.Common.Domain.ValueObjects;
using FamilyHub.Api.Features.Family.Domain.Entities;
using FamilyHub.Api.Features.Family.Domain.ValueObjects;

namespace FamilyHub.Api.Features.Family.Domain.Repositories;

/// <summary>
/// Repository interface for FamilyMember entities.
/// </summary>
public interface IFamilyMemberRepository
{
    Task<FamilyMember?> GetByUserAndFamilyAsync(UserId userId, FamilyId familyId, CancellationToken ct = default);
    Task<List<FamilyMember>> GetByFamilyIdAsync(FamilyId familyId, CancellationToken ct = default);
    Task AddAsync(FamilyMember member, CancellationToken ct = default);
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
