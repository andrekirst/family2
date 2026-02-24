using FamilyHub.Api.Features.GoogleIntegration.Domain.Entities;
using FamilyHub.Api.Features.GoogleIntegration.Models;

namespace FamilyHub.Api.Features.GoogleIntegration.Application.Mappers;

public static class GoogleIntegrationMapper
{
    public static LinkedAccountDto ToLinkedAccountDto(GoogleAccountLink link) => new()
    {
        GoogleAccountId = link.GoogleAccountId.Value,
        GoogleEmail = link.GoogleEmail.Value,
        Status = link.Status.Value,
        GrantedScopes = link.GrantedScopes.Value,
        LastSyncAt = link.LastSyncAt,
        CreatedAt = link.CreatedAt
    };

    public static GoogleCalendarSyncStatusDto ToCalendarSyncStatusDto(GoogleAccountLink? link)
    {
        if (link is null)
        {
            return new GoogleCalendarSyncStatusDto
            {
                IsLinked = false,
                Status = "NotLinked"
            };
        }

        return new GoogleCalendarSyncStatusDto
        {
            IsLinked = true,
            LastSyncAt = link.LastSyncAt,
            HasCalendarScope = link.GrantedScopes.HasCalendarScope(),
            Status = link.Status.Value,
            ErrorMessage = link.LastError
        };
    }
}
