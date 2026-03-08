using FamilyHub.Common.Domain.ValueObjects;
using FamilyHub.Api.Features.Auth.Domain.Entities;
using FamilyHub.Api.Features.Auth.Domain.Repositories;

namespace FamilyHub.TestCommon.Fakes;

public class FakeUserRepository(User? existingUser = null) : IUserRepository
{
    public User? StoredUser => existingUser;

    public Task<User?> GetByIdAsync(UserId id, CancellationToken cancellationToken = default) =>
        Task.FromResult(existingUser);

    public Task<User?> GetByExternalIdAsync(ExternalUserId externalId, CancellationToken cancellationToken = default) =>
        Task.FromResult(existingUser);

    public Task<User?> GetByEmailAsync(Email email, CancellationToken cancellationToken = default) =>
        Task.FromResult(existingUser?.Email == email ? existingUser : null);

    public Task<bool> ExistsByIdAsync(UserId id, CancellationToken cancellationToken = default) =>
        Task.FromResult(existingUser is not null);

    public Task AddAsync(User user, CancellationToken cancellationToken = default) =>
        Task.CompletedTask;

    public Task UpdateAsync(User user, CancellationToken cancellationToken = default) =>
        Task.CompletedTask;
}
