using FamilyHub.Common.Domain;
using FamilyHub.Common.Domain.ValueObjects;
using FamilyHub.Api.Features.Family.Domain.Entities;
using FamilyHub.Api.Features.Family.Domain.ValueObjects;

namespace FamilyHub.Api.Features.Family.Domain.Repositories;

/// <summary>
/// Repository interface for FamilyInvitation aggregate.
/// </summary>
public interface IFamilyInvitationRepository : IWriteRepository<FamilyInvitation, InvitationId>
{
    Task<FamilyInvitation?> GetByTokenHashAsync(InvitationToken tokenHash, CancellationToken cancellationToken = default);
    Task<List<FamilyInvitation>> GetPendingByFamilyIdAsync(FamilyId familyId, CancellationToken cancellationToken = default);
    Task<FamilyInvitation?> GetByEmailAndFamilyAsync(Email email, FamilyId familyId, CancellationToken cancellationToken = default);
    Task<List<FamilyInvitation>> GetPendingByEmailAsync(Email email, CancellationToken cancellationToken = default);
}
