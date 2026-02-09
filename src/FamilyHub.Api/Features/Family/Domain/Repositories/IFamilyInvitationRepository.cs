using FamilyHub.Api.Common.Domain.ValueObjects;
using FamilyHub.Api.Features.Family.Domain.Entities;
using FamilyHub.Api.Features.Family.Domain.ValueObjects;

namespace FamilyHub.Api.Features.Family.Domain.Repositories;

/// <summary>
/// Repository interface for FamilyInvitation aggregate.
/// </summary>
public interface IFamilyInvitationRepository
{
    Task<FamilyInvitation?> GetByIdAsync(InvitationId id, CancellationToken ct = default);
    Task<FamilyInvitation?> GetByTokenHashAsync(InvitationToken tokenHash, CancellationToken ct = default);
    Task<List<FamilyInvitation>> GetPendingByFamilyIdAsync(FamilyId familyId, CancellationToken ct = default);
    Task<FamilyInvitation?> GetByEmailAndFamilyAsync(Email email, FamilyId familyId, CancellationToken ct = default);
    Task AddAsync(FamilyInvitation invitation, CancellationToken ct = default);
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
