using FamilyHub.Api.Features.Dashboard.Domain.ValueObjects;
using FluentAssertions;
using Vogen;

namespace FamilyHub.Dashboard.Tests.Features.Dashboard.Domain;

public class ValueObjectTests
{
    [Fact]
    public void DashboardId_New_ShouldCreateUniqueId()
    {
        var id1 = DashboardId.New();
        var id2 = DashboardId.New();

        id1.Value.Should().NotBe(Guid.Empty);
        id1.Should().NotBe(id2);
    }

    [Fact]
    public void DashboardId_Empty_ShouldThrow()
    {
        var act = () => DashboardId.From(Guid.Empty);
        act.Should().Throw<ValueObjectValidationException>();
    }

    [Fact]
    public void DashboardWidgetId_New_ShouldCreateUniqueId()
    {
        var id1 = DashboardWidgetId.New();
        var id2 = DashboardWidgetId.New();

        id1.Value.Should().NotBe(Guid.Empty);
        id1.Should().NotBe(id2);
    }

    [Fact]
    public void DashboardWidgetId_Empty_ShouldThrow()
    {
        var act = () => DashboardWidgetId.From(Guid.Empty);
        act.Should().Throw<ValueObjectValidationException>();
    }

    [Fact]
    public void DashboardLayoutName_Valid_ShouldCreate()
    {
        var name = DashboardLayoutName.From("My Dashboard");
        name.Value.Should().Be("My Dashboard");
    }

    [Fact]
    public void DashboardLayoutName_Empty_ShouldThrow()
    {
        var act = () => DashboardLayoutName.From("");
        act.Should().Throw<ValueObjectValidationException>();
    }

    [Fact]
    public void DashboardLayoutName_TooLong_ShouldThrow()
    {
        var act = () => DashboardLayoutName.From(new string('x', 101));
        act.Should().Throw<ValueObjectValidationException>();
    }

    [Fact]
    public void WidgetTypeId_Valid_ShouldCreate()
    {
        var id = WidgetTypeId.From("family:overview");
        id.Value.Should().Be("family:overview");
    }

    [Fact]
    public void WidgetTypeId_Empty_ShouldThrow()
    {
        var act = () => WidgetTypeId.From("");
        act.Should().Throw<ValueObjectValidationException>();
    }

    [Fact]
    public void WidgetTypeId_TooLong_ShouldThrow()
    {
        var act = () => WidgetTypeId.From(new string('x', 101));
        act.Should().Throw<ValueObjectValidationException>();
    }
}
