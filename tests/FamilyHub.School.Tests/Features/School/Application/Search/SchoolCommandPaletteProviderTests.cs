using FamilyHub.Api.Features.School.Application.Search;
using FluentAssertions;

namespace FamilyHub.School.Tests.Features.School.Application.Search;

public class SchoolCommandPaletteProviderTests
{
    private readonly SchoolCommandPaletteProvider _provider = new();

    [Fact]
    public void ModuleName_ShouldBeSchool()
    {
        _provider.ModuleName.Should().Be("school");
    }

    [Fact]
    public void GetCommands_ShouldReturnThreeCommands()
    {
        var commands = _provider.GetCommands();

        commands.Should().HaveCount(3);
        commands.Select(c => c.Label).Should().BeEquivalentTo(
            "View Students", "Go to School", "Mark as Student");
    }

    [Fact]
    public void ViewStudents_ShouldRequireNoPermissions()
    {
        var commands = _provider.GetCommands();
        var viewStudents = commands.Single(c => c.Label == "View Students");

        viewStudents.RequiredPermissions.Should().BeEmpty();
        viewStudents.Route.Should().Be("/school");
        viewStudents.Icon.Should().Be("graduation-cap");
        viewStudents.Group.Should().Be("school");
        viewStudents.LabelDe.Should().Be("Schüler anzeigen");
    }

    [Fact]
    public void MarkAsStudent_ShouldRequireManageStudentsPermission()
    {
        var commands = _provider.GetCommands();
        var markAsStudent = commands.Single(c => c.Label == "Mark as Student");

        markAsStudent.RequiredPermissions.Should().Contain("school:manage-students");
        markAsStudent.Route.Should().Be("/school?action=mark-student");
    }
}
