using FamilyHub.Common.Domain.ValueObjects;
using FamilyHub.Api.Common.Infrastructure.Avatar;

namespace FamilyHub.TestCommon.Fakes;

public class FakeAvatarRepository(AvatarAggregate? existingAvatar = null) : IAvatarRepository
{
    public List<AvatarAggregate> AddedAvatars { get; } = [];
    public List<AvatarId> DeletedAvatarIds { get; } = [];

    public Task<AvatarAggregate?> GetByIdAsync(AvatarId id, CancellationToken ct = default)
    {
        var added = AddedAvatars.FirstOrDefault(a => a.Id == id);
        if (added is not null) return Task.FromResult<AvatarAggregate?>(added);

        return Task.FromResult(existingAvatar?.Id == id ? existingAvatar : null);
    }

    public Task AddAsync(AvatarAggregate avatar, CancellationToken ct = default)
    {
        AddedAvatars.Add(avatar);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(AvatarId id, CancellationToken ct = default)
    {
        DeletedAvatarIds.Add(id);
        return Task.CompletedTask;
    }
}
