using FamilyHub.SharedKernel.Application.CQRS;
using FamilyHub.SharedKernel.Domain.ValueObjects;

namespace FamilyHub.Modules.Family.Application.Queries.GetUserFamilies;

/// <summary>
/// Query to get all families that a user belongs to.
/// </summary>
public sealed record GetUserFamiliesQuery(
    UserId UserId
) : IQuery<GetUserFamiliesResult>;
