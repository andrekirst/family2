using FamilyHub.Common.Domain.ValueObjects;
using FamilyHub.Api.Features.Auth.Domain.Entities;
using FamilyHub.Api.Features.Auth.Domain.Repositories;

namespace FamilyHub.TestCommon.Fakes;

public class FakeUserRepository(User? existingUser = null) : IUserRepository
{
    public User? StoredUser => existingUser;

    public Task<User?> GetByIdAsync(UserId id, CancellationToken ct = default) =>
        Task.FromResult(existingUser);

    public Task<User?> GetByExternalIdAsync(ExternalUserId externalId, CancellationToken ct = default) =>
        Task.FromResult(existingUser);

    public Task<User?> GetByEmailAsync(Email email, CancellationToken ct = default) =>
        Task.FromResult(existingUser?.Email == email ? existingUser : null);

    public Task AddAsync(User user, CancellationToken ct = default) =>
        Task.CompletedTask;

    public Task UpdateAsync(User user, CancellationToken ct = default) =>
        Task.CompletedTask;

    public Task<int> SaveChangesAsync(CancellationToken ct = default) =>
        Task.FromResult(1);
}
