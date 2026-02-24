using FamilyHub.Common.Application;
using FamilyHub.Common.Domain.ValueObjects;

namespace FamilyHub.Api.Features.GoogleIntegration.Application.Queries.GetGoogleAuthUrl;

public sealed record GetGoogleAuthUrlQuery(
    UserId UserId
) : IQuery<string>;
