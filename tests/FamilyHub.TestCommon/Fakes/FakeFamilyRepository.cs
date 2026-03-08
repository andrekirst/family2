using FamilyHub.Common.Domain.ValueObjects;
using FamilyHub.Api.Features.Family.Domain.Repositories;
using FamilyEntity = FamilyHub.Api.Features.Family.Domain.Entities.Family;

namespace FamilyHub.TestCommon.Fakes;

public class FakeFamilyRepository(FamilyEntity? existingFamilyForOwner = null) : IFamilyRepository
{
    public List<FamilyEntity> AddedFamilies { get; } = [];
    private readonly List<FamilyEntity> _seeded = [];

    public void Seed(FamilyEntity family) => _seeded.Add(family);

    public Task<FamilyEntity?> GetByIdAsync(FamilyId id, CancellationToken ct = default) =>
        Task.FromResult(_seeded.FirstOrDefault(f => f.Id == id));

    public Task<FamilyEntity?> GetByIdWithMembersAsync(FamilyId id, CancellationToken ct = default) =>
        Task.FromResult(_seeded.FirstOrDefault(f => f.Id == id));

    public Task<FamilyEntity?> GetByOwnerIdAsync(UserId ownerId, CancellationToken ct = default) =>
        Task.FromResult(existingFamilyForOwner ?? _seeded.FirstOrDefault(f => f.OwnerId == ownerId));

    public Task<bool> ExistsByIdAsync(FamilyId id, CancellationToken ct = default) =>
        Task.FromResult(_seeded.Concat(AddedFamilies).Any(f => f.Id == id));

    public Task<bool> UserHasFamilyAsync(UserId userId, CancellationToken ct = default) =>
        Task.FromResult(existingFamilyForOwner is not null);

    public Task AddAsync(FamilyEntity family, CancellationToken ct = default)
    {
        AddedFamilies.Add(family);
        return Task.CompletedTask;
    }

    public Task UpdateAsync(FamilyEntity family, CancellationToken ct = default) =>
        Task.CompletedTask;
}
