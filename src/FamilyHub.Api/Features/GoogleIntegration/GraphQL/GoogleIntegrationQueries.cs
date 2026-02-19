using System.Security.Claims;
using FamilyHub.Common.Application;
using FamilyHub.Api.Common.Infrastructure.GraphQL.NamespaceTypes;
using FamilyHub.Api.Common.Services;
using FamilyHub.Api.Features.Auth.Domain.Repositories;
using FamilyHub.Api.Features.GoogleIntegration.Application.Queries.GetCalendarSyncStatus;
using FamilyHub.Api.Features.GoogleIntegration.Application.Queries.GetGoogleAuthUrl;
using FamilyHub.Api.Features.GoogleIntegration.Application.Queries.GetLinkedAccounts;
using FamilyHub.Api.Features.GoogleIntegration.Models;
using HotChocolate.Authorization;

namespace FamilyHub.Api.Features.GoogleIntegration.GraphQL;

[ExtendObjectType(typeof(GoogleIntegrationQuery))]
public class GoogleIntegrationQueries
{
    [Authorize]
    public async Task<List<LinkedAccountDto>> LinkedAccounts(
        ClaimsPrincipal claimsPrincipal,
        [Service] IQueryBus queryBus,
        [Service] IUserRepository userRepository,
        [Service] IUserService userService,
        CancellationToken cancellationToken)
    {
        var user = await userService.GetCurrentUser(
            claimsPrincipal, userRepository, cancellationToken);

        var query = new GetLinkedAccountsQuery(user.Id);
        return await queryBus.QueryAsync<List<LinkedAccountDto>>(query, cancellationToken);
    }

    [Authorize]
    public async Task<GoogleCalendarSyncStatusDto> CalendarSyncStatus(
        ClaimsPrincipal claimsPrincipal,
        [Service] IQueryBus queryBus,
        [Service] IUserRepository userRepository,
        [Service] IUserService userService,
        CancellationToken cancellationToken)
    {
        var user = await userService.GetCurrentUser(
            claimsPrincipal, userRepository, cancellationToken);

        var query = new GetCalendarSyncStatusQuery(user.Id);
        return await queryBus.QueryAsync<GoogleCalendarSyncStatusDto>(query, cancellationToken);
    }

    [Authorize]
    public async Task<string> AuthUrl(
        ClaimsPrincipal claimsPrincipal,
        [Service] IQueryBus queryBus,
        [Service] IUserRepository userRepository,
        [Service] IUserService userService,
        CancellationToken cancellationToken)
    {
        var user = await userService.GetCurrentUser(
            claimsPrincipal, userRepository, cancellationToken);

        var query = new GetGoogleAuthUrlQuery(user.Id);
        return await queryBus.QueryAsync<string>(query, cancellationToken);
    }
}
