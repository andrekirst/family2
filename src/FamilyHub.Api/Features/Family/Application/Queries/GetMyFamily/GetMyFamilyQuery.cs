using FamilyHub.Api.Common.Application;
using FamilyHub.Api.Features.Auth.Domain.ValueObjects;
using FamilyHub.Api.Features.Family.Models;

namespace FamilyHub.Api.Features.Family.Application.Queries.GetMyFamily;

/// <summary>
/// Query to get the current user's family.
/// </summary>
public sealed record GetMyFamilyQuery(
    ExternalUserId ExternalUserId
) : IQuery<FamilyDto?>;
