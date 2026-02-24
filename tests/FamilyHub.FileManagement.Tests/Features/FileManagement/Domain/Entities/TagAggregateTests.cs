using FamilyHub.Api.Features.FileManagement.Domain.ValueObjects;
using FamilyHub.Common.Domain.ValueObjects;
using FluentAssertions;
using Tag = FamilyHub.Api.Features.FileManagement.Domain.Entities.Tag;

namespace FamilyHub.FileManagement.Tests.Features.FileManagement.Domain.Entities;

public class TagAggregateTests
{
    private static Tag CreateTestTag(
        FamilyId? familyId = null,
        string name = "Photos",
        string color = "#FF0000")
    {
        return Tag.Create(
            TagName.From(name),
            TagColor.From(color),
            familyId ?? FamilyId.New(),
            UserId.New());
    }

    [Fact]
    public void Create_ShouldCreateTagWithValidData()
    {
        var tag = CreateTestTag();

        tag.Should().NotBeNull();
        tag.Id.Value.Should().NotBe(Guid.Empty);
        tag.Name.Value.Should().Be("Photos");
        tag.Color.Value.Should().Be("#FF0000");
        tag.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));
    }

    [Fact]
    public void Rename_ShouldUpdateName()
    {
        var tag = CreateTestTag();
        var newName = TagName.From("Videos");

        tag.Rename(newName);

        tag.Name.Value.Should().Be("Videos");
    }

    [Fact]
    public void ChangeColor_ShouldUpdateColor()
    {
        var tag = CreateTestTag();
        var newColor = TagColor.From("#00FF00");

        tag.ChangeColor(newColor);

        tag.Color.Value.Should().Be("#00FF00");
    }
}
