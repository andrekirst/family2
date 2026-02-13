using FamilyHub.Api.Features.GoogleIntegration.Domain.Entities;
using FamilyHub.Api.Features.GoogleIntegration.Domain.Repositories;

namespace FamilyHub.TestCommon.Fakes;

public class FakeOAuthStateRepository(OAuthState? existingState = null) : IOAuthStateRepository
{
    public List<OAuthState> AddedStates { get; } = [];
    public List<OAuthState> DeletedStates { get; } = [];
    private int _saveChangesCount;
    public int SaveChangesCount => _saveChangesCount;

    public Task<OAuthState?> GetByStateAsync(string state, CancellationToken ct = default)
    {
        var result = existingState?.State == state ? existingState
            : AddedStates.FirstOrDefault(s => s.State == state);
        return Task.FromResult(result);
    }

    public Task AddAsync(OAuthState oauthState, CancellationToken ct = default)
    {
        AddedStates.Add(oauthState);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(OAuthState oauthState, CancellationToken ct = default)
    {
        DeletedStates.Add(oauthState);
        return Task.CompletedTask;
    }

    public Task DeleteExpiredAsync(CancellationToken ct = default) =>
        Task.CompletedTask;

    public Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        _saveChangesCount++;
        return Task.FromResult(1);
    }
}
