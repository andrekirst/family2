using AutoFixture;

namespace FamilyHub.TestCommon.Fixtures;

/// <summary>
/// AutoFixture customization that registers Vogen value object factories.
/// Call RegisterVogenId() for Guid-based VOs and RegisterVogenString() for string-based VOs.
/// </summary>
public class VogenCustomization : ICustomization
{
    public void Customize(IFixture fixture)
    {
        // Common domain value objects (Guid-based IDs)
        fixture.RegisterVogenId(FamilyHub.Common.Domain.ValueObjects.UserId.From);
        fixture.RegisterVogenId(FamilyHub.Common.Domain.ValueObjects.FamilyId.From);
        fixture.RegisterVogenId(FamilyHub.Common.Domain.ValueObjects.FolderId.From);
        fixture.RegisterVogenId(FamilyHub.Common.Domain.ValueObjects.FileId.From);
        fixture.RegisterVogenId(FamilyHub.Common.Domain.ValueObjects.AvatarId.From);
        fixture.RegisterVogenId(FamilyHub.Common.Domain.ValueObjects.MessageId.From);
        fixture.RegisterVogenId(FamilyHub.Common.Domain.ValueObjects.ConversationId.From);
        fixture.RegisterVogenId(FamilyHub.Common.Domain.ValueObjects.TagId.From);
        fixture.RegisterVogenId(FamilyHub.Common.Domain.ValueObjects.AlbumId.From);
        fixture.RegisterVogenId(FamilyHub.Common.Domain.ValueObjects.FileVersionId.From);
        fixture.RegisterVogenId(FamilyHub.Common.Domain.ValueObjects.FileThumbnailId.From);
        fixture.RegisterVogenId(FamilyHub.Common.Domain.ValueObjects.FilePermissionId.From);
        fixture.RegisterVogenId(FamilyHub.Common.Domain.ValueObjects.ShareLinkId.From);
        fixture.RegisterVogenId(FamilyHub.Common.Domain.ValueObjects.ShareLinkAccessLogId.From);
        fixture.RegisterVogenId(FamilyHub.Common.Domain.ValueObjects.OrganizationRuleId.From);
        fixture.RegisterVogenId(FamilyHub.Common.Domain.ValueObjects.ExternalConnectionId.From);
        fixture.RegisterVogenId(FamilyHub.Common.Domain.ValueObjects.ZipJobId.From);
        fixture.RegisterVogenId(FamilyHub.Common.Domain.ValueObjects.SavedSearchId.From);
        fixture.RegisterVogenId(FamilyHub.Common.Domain.ValueObjects.RecentSearchId.From);
        fixture.RegisterVogenId(FamilyHub.Common.Domain.ValueObjects.SecureNoteId.From);
        fixture.RegisterVogenId(FamilyHub.Common.Domain.ValueObjects.ProcessingLogEntryId.From);

        // Feature-specific Guid-based IDs
        fixture.RegisterVogenId(FamilyHub.Api.Features.Family.Domain.ValueObjects.FamilyMemberId.From);
        fixture.RegisterVogenId(FamilyHub.Api.Features.Family.Domain.ValueObjects.InvitationId.From);
        fixture.RegisterVogenId(FamilyHub.Api.Features.Calendar.Domain.ValueObjects.CalendarEventId.From);
        fixture.RegisterVogenId(FamilyHub.Api.Features.Dashboard.Domain.ValueObjects.DashboardId.From);
        fixture.RegisterVogenId(FamilyHub.Api.Features.Dashboard.Domain.ValueObjects.DashboardWidgetId.From);
        fixture.RegisterVogenId(FamilyHub.Api.Features.Photos.Domain.ValueObjects.PhotoId.From);
        fixture.RegisterVogenId(FamilyHub.Api.Features.School.Domain.ValueObjects.StudentId.From);
        fixture.RegisterVogenId(FamilyHub.Api.Features.School.Domain.ValueObjects.SchoolId.From);
        fixture.RegisterVogenId(FamilyHub.Api.Features.School.Domain.ValueObjects.SchoolYearId.From);
        fixture.RegisterVogenId(FamilyHub.Api.Features.School.Domain.ValueObjects.ClassAssignmentId.From);
        fixture.RegisterVogenId(FamilyHub.Api.Features.BaseData.Domain.ValueObjects.FederalStateId.From);

        // String-based value objects
        fixture.RegisterVogenString(FamilyHub.Common.Domain.ValueObjects.ExternalUserId.From);
        fixture.RegisterVogenString(FamilyHub.Common.Domain.ValueObjects.Email.From, "test-{0}@example.com");
        fixture.RegisterVogenString(FamilyHub.Api.Features.Auth.Domain.ValueObjects.UserName.From, "Test User {0}");
        fixture.RegisterVogenString(FamilyHub.Api.Features.Family.Domain.ValueObjects.FamilyName.From, "Family {0}");
        fixture.RegisterVogenString(FamilyHub.Api.Features.Calendar.Domain.ValueObjects.EventTitle.From, "Event {0}");
        fixture.RegisterVogenString(FamilyHub.Api.Features.BaseData.Domain.ValueObjects.FederalStateName.From, "State {0}");
        fixture.RegisterVogenString(FamilyHub.Api.Features.BaseData.Domain.ValueObjects.Iso3166Code.From, "DE-X");
        fixture.RegisterVogenString(FamilyHub.Api.Features.School.Domain.ValueObjects.SchoolName.From, "School {0}");
        fixture.RegisterVogenString(FamilyHub.Api.Features.School.Domain.ValueObjects.ClassName.From, "Class {0}");
    }
}
