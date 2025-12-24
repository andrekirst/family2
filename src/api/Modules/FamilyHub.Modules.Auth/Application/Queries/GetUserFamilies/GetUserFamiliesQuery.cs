using FamilyHub.SharedKernel.Domain.ValueObjects;
using MediatR;

namespace FamilyHub.Modules.Auth.Application.Queries.GetUserFamilies;

/// <summary>
/// Query to get all families that a user belongs to.
/// </summary>
public sealed record GetUserFamiliesQuery(
    UserId UserId
) : IRequest<GetUserFamiliesResult>;
