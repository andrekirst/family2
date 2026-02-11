using FamilyHub.Common.Domain.ValueObjects;
using FamilyHub.Api.Features.Family.Domain.Entities;
using FamilyHub.Api.Features.Family.Domain.Repositories;

namespace FamilyHub.TestCommon.Fakes;

public class FakeFamilyMemberRepository(FamilyMember? existingMember = null) : IFamilyMemberRepository
{
    public List<FamilyMember> AddedMembers { get; } = [];

    public Task<FamilyMember?> GetByUserAndFamilyAsync(UserId userId, FamilyId familyId, CancellationToken ct = default) =>
        Task.FromResult(existingMember);

    public Task<List<FamilyMember>> GetByFamilyIdAsync(FamilyId familyId, CancellationToken ct = default) =>
        Task.FromResult(existingMember is not null ? [existingMember] : new List<FamilyMember>());

    public Task AddAsync(FamilyMember member, CancellationToken ct = default)
    {
        AddedMembers.Add(member);
        return Task.CompletedTask;
    }

    public Task<int> SaveChangesAsync(CancellationToken ct = default) =>
        Task.FromResult(1);
}
