using FamilyHub.Modules.Auth.Domain.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace FamilyHub.Modules.Auth.Application.Queries.GetUserFamilies;

/// <summary>
/// Handler for GetUserFamiliesQuery.
/// Retrieves all families that a user belongs to.
/// </summary>
public sealed partial class GetUserFamiliesQueryHandler(
    IFamilyRepository familyRepository,
    ILogger<GetUserFamiliesQueryHandler> logger)
    : IRequestHandler<GetUserFamiliesQuery, GetUserFamiliesResult>
{
    public async Task<GetUserFamiliesResult> Handle(
        GetUserFamiliesQuery request,
        CancellationToken cancellationToken)
    {
        LogRetrievingFamiliesForUser(request.UserId.Value);

        // 1. Query family from repository (users now have only one family)
        var family = await familyRepository.GetFamilyByUserIdAsync(request.UserId, cancellationToken);

        if (family == null)
        {
            LogFoundFamilycountFamilyIesForUserUserid(0, request.UserId.Value);
            return new GetUserFamiliesResult
            {
                Families = []
            };
        }

        LogFoundFamilycountFamilyIesForUserUserid(1, request.UserId.Value);

        // 2. Map to DTO
        var familyDto = new FamilyDto
        {
            FamilyId = family.Id,
            Name = family.Name.Value,
            OwnerId = family.OwnerId,
            CreatedAt = family.CreatedAt,
            MemberCount = family.GetMemberCount()
        };

        // 3. Return result (single family in list for backwards compatibility)
        return new GetUserFamiliesResult
        {
            Families = [familyDto]
        };
    }

    [LoggerMessage(LogLevel.Information, "Retrieving families for user {userId}")]
    partial void LogRetrievingFamiliesForUser(Guid userId);

    [LoggerMessage(LogLevel.Information, "Found {familyCount} family(ies) for user {userId}")]
    partial void LogFoundFamilycountFamilyIesForUserUserid(int familyCount, Guid userId);
}
