using FamilyHub.Common.Domain.ValueObjects;
using FamilyHub.Api.Features.Family.Domain.Repositories;
using FamilyEntity = FamilyHub.Api.Features.Family.Domain.Entities.Family;

namespace FamilyHub.TestCommon.Fakes;

public class FakeFamilyRepository(FamilyEntity? existingFamilyForOwner = null) : IFamilyRepository
{
    public List<FamilyEntity> AddedFamilies { get; } = [];

    public Task<FamilyEntity?> GetByIdAsync(FamilyId id, CancellationToken ct = default) =>
        Task.FromResult<FamilyEntity?>(null);

    public Task<FamilyEntity?> GetByIdWithMembersAsync(FamilyId id, CancellationToken ct = default) =>
        Task.FromResult<FamilyEntity?>(null);

    public Task<FamilyEntity?> GetByOwnerIdAsync(UserId ownerId, CancellationToken ct = default) =>
        Task.FromResult(existingFamilyForOwner);

    public Task<bool> UserHasFamilyAsync(UserId userId, CancellationToken ct = default) =>
        Task.FromResult(existingFamilyForOwner is not null);

    public Task AddAsync(FamilyEntity family, CancellationToken ct = default)
    {
        AddedFamilies.Add(family);
        return Task.CompletedTask;
    }

    public Task UpdateAsync(FamilyEntity family, CancellationToken ct = default) =>
        Task.CompletedTask;

    public Task<int> SaveChangesAsync(CancellationToken ct = default) =>
        Task.FromResult(1);
}
