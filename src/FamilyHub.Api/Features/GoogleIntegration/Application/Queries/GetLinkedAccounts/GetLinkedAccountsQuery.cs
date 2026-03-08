using FamilyHub.Api.Common.Infrastructure.FamilyScope;
using FamilyHub.Common.Application;
using FamilyHub.Common.Domain.ValueObjects;
using FamilyHub.Api.Features.GoogleIntegration.Models;

namespace FamilyHub.Api.Features.GoogleIntegration.Application.Queries.GetLinkedAccounts;

public sealed record GetLinkedAccountsQuery(
    UserId UserId
) : IReadOnlyQuery<List<LinkedAccountDto>>, IIgnoreFamilyMembership;
