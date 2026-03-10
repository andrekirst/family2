using FamilyHub.Api.Features.GoogleIntegration.Domain.Entities;
using FamilyHub.Api.Features.GoogleIntegration.Domain.Repositories;

namespace FamilyHub.TestCommon.Fakes;

public class FakeOAuthStateRepository(OAuthState? existingState = null) : IOAuthStateRepository
{
    public List<OAuthState> AddedStates { get; } = [];
    public List<OAuthState> DeletedStates { get; } = [];
    public Task<OAuthState?> GetByStateAsync(string state, CancellationToken cancellationToken = default)
    {
        var result = existingState?.State == state ? existingState
            : AddedStates.FirstOrDefault(s => s.State == state);
        return Task.FromResult(result);
    }

    public Task AddAsync(OAuthState oauthState, CancellationToken cancellationToken = default)
    {
        AddedStates.Add(oauthState);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(OAuthState oauthState, CancellationToken cancellationToken = default)
    {
        DeletedStates.Add(oauthState);
        return Task.CompletedTask;
    }

    public Task DeleteExpiredAsync(CancellationToken cancellationToken = default) =>
        Task.CompletedTask;

}
