using FamilyHub.Common.Application;
using FamilyHub.Api.Common.Middleware;
using FamilyHub.Api.Features.Auth.Domain.Repositories;
using Microsoft.Extensions.Caching.Memory;

namespace FamilyHub.Api.Features.Auth.Application.Commands.UpdateUserLocale;

/// <summary>
/// Handler for UpdateUserLocaleCommand.
/// Looks up the user by ID and updates their preferred locale.
/// Invalidates the locale cache so the middleware picks up the new value.
/// </summary>
public sealed class UpdateUserLocaleCommandHandler(
    IUserRepository userRepository,
    IMemoryCache memoryCache)
    : ICommandHandler<UpdateUserLocaleCommand, UpdateUserLocaleResult>
{
    public async ValueTask<UpdateUserLocaleResult> Handle(
        UpdateUserLocaleCommand command,
        CancellationToken cancellationToken)
    {
        var user = (await userRepository.GetByIdAsync(command.UserId, cancellationToken))!;

        user.UpdateLocale(command.Locale);

        memoryCache.Remove($"{RequestLocaleResolutionMiddleware.CacheKeyPrefix}{user.ExternalUserId.Value}");

        return new UpdateUserLocaleResult(true);
    }
}
