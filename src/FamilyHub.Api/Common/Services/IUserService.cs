using System.Security.Claims;
using FamilyHub.Api.Common.Infrastructure;
using FamilyHub.Api.Features.Auth.Domain.Repositories;
using FamilyHub.Api.Features.Auth.Domain.ValueObjects;

namespace FamilyHub.Api.Common.Services;

public interface IUserService
{
    Task<Features.Auth.Domain.Entities.User> GetCurrentUser(
        ClaimsPrincipal claimsPrincipal,
        IUserRepository userRepository,
        CancellationToken cancellationToken);
}

public class UserService : IUserService
{
    public async Task<Features.Auth.Domain.Entities.User> GetCurrentUser(
        ClaimsPrincipal claimsPrincipal,
        IUserRepository userRepository,
        CancellationToken cancellationToken)
    {
        var externalUserIdString = claimsPrincipal.FindFirst(ClaimNames.Sub)?.Value
                                   ?? throw new UnauthorizedAccessException("User not authenticated");

        var externalUserId = ExternalUserId.From(externalUserIdString);
        return await userRepository.GetByExternalIdAsync(externalUserId, cancellationToken)
               ?? throw new UnauthorizedAccessException("User not found");
    }
}
