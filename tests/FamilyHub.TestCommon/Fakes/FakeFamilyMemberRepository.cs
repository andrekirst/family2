using FamilyHub.Common.Domain.ValueObjects;
using FamilyHub.Api.Features.Family.Domain.Entities;
using FamilyHub.Api.Features.Family.Domain.Repositories;
using FamilyHub.Api.Features.Family.Domain.ValueObjects;

namespace FamilyHub.TestCommon.Fakes;

public class FakeFamilyMemberRepository(FamilyMember? existingMember = null, List<FamilyMember>? allMembers = null) : IFamilyMemberRepository
{
    private readonly List<FamilyMember> _allMembers = allMembers ?? (existingMember is not null ? [existingMember] : []);
    public List<FamilyMember> AddedMembers { get; } = [];

    public Task<FamilyMember?> GetByIdAsync(FamilyMemberId id, CancellationToken cancellationToken = default)
    {
        var member = _allMembers.Concat(AddedMembers).FirstOrDefault(m => m.Id == id);
        return Task.FromResult(member);
    }

    public Task<bool> ExistsByIdAsync(FamilyMemberId id, CancellationToken cancellationToken = default) =>
        Task.FromResult(_allMembers.Concat(AddedMembers).Any(m => m.Id == id));

    public Task<bool> ExistsByUserAndFamilyAsync(UserId userId, FamilyId familyId, CancellationToken cancellationToken = default) =>
        Task.FromResult(existingMember is not null);

    public Task<FamilyMember?> GetByUserAndFamilyAsync(UserId userId, FamilyId familyId, CancellationToken cancellationToken = default) =>
        Task.FromResult(existingMember);

    public Task<List<FamilyMember>> GetByFamilyIdAsync(FamilyId familyId, CancellationToken cancellationToken = default) =>
        Task.FromResult(_allMembers.Where(m => m.FamilyId == familyId).ToList());

    public Task AddAsync(FamilyMember member, CancellationToken cancellationToken = default)
    {
        AddedMembers.Add(member);
        return Task.CompletedTask;
    }
}
