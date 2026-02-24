using FamilyHub.Api.Features.FileManagement.Application.Commands.SaveSearch;
using FamilyHub.Common.Domain.ValueObjects;
using FamilyHub.TestCommon.Fakes;
using FluentAssertions;

namespace FamilyHub.FileManagement.Tests.Features.FileManagement.Application;

public class SaveSearchCommandHandlerTests
{
    [Fact]
    public async Task Handle_ShouldSaveSearch()
    {
        var savedRepo = new FakeSavedSearchRepository();
        var handler = new SaveSearchCommandHandler(savedRepo);

        var command = new SaveSearchCommand(
            "My Photos",
            "vacation",
            """{"mimeTypes":["image/jpeg"]}""",
            UserId.New());

        var result = await handler.Handle(command, CancellationToken.None);

        result.Success.Should().BeTrue();
        savedRepo.Searches.Should().HaveCount(1);
        savedRepo.Searches.First().Name.Should().Be("My Photos");
        savedRepo.Searches.First().Query.Should().Be("vacation");
    }

    [Fact]
    public async Task Handle_ShouldSaveWithNullFilters()
    {
        var savedRepo = new FakeSavedSearchRepository();
        var handler = new SaveSearchCommandHandler(savedRepo);

        var command = new SaveSearchCommand("Simple", "query", null, UserId.New());
        var result = await handler.Handle(command, CancellationToken.None);

        result.Success.Should().BeTrue();
        savedRepo.Searches.First().FiltersJson.Should().BeNull();
    }
}
