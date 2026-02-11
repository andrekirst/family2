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

    public Task<FamilyInvitation?> GetByIdAsync(InvitationId id, CancellationToken ct = default) =>
        Task.FromResult(existingById);

    public Task<FamilyInvitation?> GetByTokenHashAsync(InvitationToken tokenHash, CancellationToken ct = default) =>
        Task.FromResult(existingByTokenHash);

    public Task<List<FamilyInvitation>> GetPendingByFamilyIdAsync(FamilyId familyId, CancellationToken ct = default) =>
        Task.FromResult(new List<FamilyInvitation>());

    public Task<FamilyInvitation?> GetByEmailAndFamilyAsync(Email email, FamilyId familyId, CancellationToken ct = default) =>
        Task.FromResult(existingByEmail);

    public Task<List<FamilyInvitation>> GetPendingByEmailAsync(Email email, CancellationToken ct = default) =>
        Task.FromResult(new List<FamilyInvitation>());

    public Task AddAsync(FamilyInvitation invitation, CancellationToken ct = default)
    {
        AddedInvitations.Add(invitation);
        return Task.CompletedTask;
    }

    public Task<int> SaveChangesAsync(CancellationToken ct = default) =>
        Task.FromResult(1);
}
