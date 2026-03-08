using FamilyHub.Common.Domain.ValueObjects;
using FamilyHub.Api.Features.Family.Domain.Entities;
using FamilyHub.Api.Features.Family.Domain.Repositories;
using FamilyHub.Api.Features.Family.Domain.ValueObjects;

namespace FamilyHub.TestCommon.Fakes;

public class FakeFamilyInvitationRepository(
    FamilyInvitation? existingByEmail = null,
    FamilyInvitation? existingByTokenHash = null,
    FamilyInvitation? existingById = null)
    : IFamilyInvitationRepository
{
    public List<FamilyInvitation> AddedInvitations { get; } = [];
    private readonly List<FamilyInvitation> _seeded = [];

    public void Seed(FamilyInvitation invitation) => _seeded.Add(invitation);

    public Task<FamilyInvitation?> GetByIdAsync(InvitationId id, CancellationToken cancellationToken = default) =>
        Task.FromResult(existingById ?? _seeded.FirstOrDefault(i => i.Id == id));

    public Task<bool> ExistsByIdAsync(InvitationId id, CancellationToken cancellationToken = default) =>
        Task.FromResult(existingById is not null || _seeded.Any(i => i.Id == id));

    public Task<FamilyInvitation?> GetByTokenHashAsync(InvitationToken tokenHash, CancellationToken cancellationToken = default) =>
        Task.FromResult(existingByTokenHash);

    public Task<List<FamilyInvitation>> GetPendingByFamilyIdAsync(FamilyId familyId, CancellationToken cancellationToken = default) =>
        Task.FromResult(_seeded.Where(i => i.FamilyId == familyId).ToList());

    public Task<FamilyInvitation?> GetByEmailAndFamilyAsync(Email email, FamilyId familyId, CancellationToken cancellationToken = default) =>
        Task.FromResult(existingByEmail ?? _seeded.FirstOrDefault(i => i.InviteeEmail == email && i.FamilyId == familyId));

    public Task<List<FamilyInvitation>> GetPendingByEmailAsync(Email email, CancellationToken cancellationToken = default) =>
        Task.FromResult(_seeded.Where(i => i.InviteeEmail == email).ToList());

    public Task AddAsync(FamilyInvitation invitation, CancellationToken cancellationToken = default)
    {
        AddedInvitations.Add(invitation);
        return Task.CompletedTask;
    }
}
