using FamilyHub.Common.Domain;
using FamilyHub.Common.Domain.ValueObjects;
using FamilyHub.Api.Features.Family.Domain.Entities;
using FamilyHub.Api.Features.Family.Domain.ValueObjects;

namespace FamilyHub.Api.Features.Family.Domain.Repositories;

/// <summary>
/// Repository interface for FamilyMember entities.
/// </summary>
public interface IFamilyMemberRepository : IWriteRepository<FamilyMember, FamilyMemberId>
{
    Task<bool> ExistsByUserAndFamilyAsync(UserId userId, FamilyId familyId, CancellationToken cancellationToken = default);
    Task<FamilyMember?> GetByUserAndFamilyAsync(UserId userId, FamilyId familyId, CancellationToken cancellationToken = default);
    Task<List<FamilyMember>> GetByFamilyIdAsync(FamilyId familyId, CancellationToken cancellationToken = default);
}
