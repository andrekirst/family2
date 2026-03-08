using FamilyHub.Api.Common.Infrastructure.FamilyScope;
using FamilyHub.Common.Application;
using FamilyHub.Common.Domain.ValueObjects;
using FamilyHub.Api.Features.GoogleIntegration.Models;

namespace FamilyHub.Api.Features.GoogleIntegration.Application.Queries.GetCalendarSyncStatus;

public sealed record GetCalendarSyncStatusQuery(
    UserId UserId
) : IReadOnlyQuery<GoogleCalendarSyncStatusDto>, IIgnoreFamilyMembership;
