using FluentAssertions;
using FamilyHub.Api.Features.GoogleIntegration.Domain.ValueObjects;

namespace FamilyHub.GoogleIntegration.Tests.Domain;

public class ValueObjectTests
{
    [Fact]
    public void GoogleAccountLinkId_New_ShouldCreateUniqueIds()
    {
        var id1 = GoogleAccountLinkId.New();
        var id2 = GoogleAccountLinkId.New();
        id1.Should().NotBe(id2);
    }

    [Fact]
    public void GoogleAccountId_ShouldTrimInput()
    {
        var id = GoogleAccountId.From("  sub-123  ");
        id.Value.Should().Be("sub-123");
    }

    [Fact]
    public void GoogleScopes_HasCalendarScope_ShouldDetectCalendarReadonly()
    {
        var scopes = GoogleScopes.From("openid email https://www.googleapis.com/auth/calendar.readonly");
        scopes.HasCalendarScope().Should().BeTrue();
    }

    [Fact]
    public void GoogleScopes_HasCalendarScope_ShouldDetectCalendarEvents()
    {
        var scopes = GoogleScopes.From("openid https://www.googleapis.com/auth/calendar.events");
        scopes.HasCalendarScope().Should().BeTrue();
    }

    [Fact]
    public void GoogleScopes_HasCalendarScope_ShouldReturnFalseWithoutCalendar()
    {
        var scopes = GoogleScopes.From("openid email profile");
        scopes.HasCalendarScope().Should().BeFalse();
    }

    [Fact]
    public void GoogleLinkStatus_Active_ShouldBeActive()
    {
        var status = GoogleLinkStatus.Active;
        status.IsActive.Should().BeTrue();
        status.Value.Should().Be("Active");
    }

    [Fact]
    public void GoogleLinkStatus_Error_ShouldNotBeActive()
    {
        var status = GoogleLinkStatus.Error;
        status.IsActive.Should().BeFalse();
    }
}
