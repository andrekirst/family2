using FamilyHub.Api.Features.FileManagement.Application.Commands.SaveSearch;
using FamilyHub.Api.Features.FileManagement.Domain.Entities;
using FamilyHub.Api.Features.FileManagement.Domain.Repositories;
using FamilyHub.Common.Domain.ValueObjects;
using FluentAssertions;
using NSubstitute;

namespace FamilyHub.FileManagement.Tests.Features.FileManagement.Application;

public class SaveSearchCommandHandlerTests
{
    private readonly ISavedSearchRepository _savedRepo = Substitute.For<ISavedSearchRepository>();
    private readonly SaveSearchCommandHandler _handler;

    public SaveSearchCommandHandlerTests()
    {
        _handler = new SaveSearchCommandHandler(_savedRepo, TimeProvider.System);
    }

    [Fact]
    public async Task Handle_ShouldSaveSearch()
    {
        var command = new SaveSearchCommand(
            "My Photos",
            "vacation",
            """{"mimeTypes":["image/jpeg"]}""")
        {
            UserId = UserId.New(),
            FamilyId = FamilyId.New()
        };

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        await _savedRepo.Received(1).AddAsync(
            Arg.Is<SavedSearch>(s => s.Name == "My Photos" && s.Query == "vacation"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldSaveWithNullFilters()
    {
        var command = new SaveSearchCommand("Simple", "query", null)
        {
            UserId = UserId.New(),
            FamilyId = FamilyId.New()
        };
        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        await _savedRepo.Received(1).AddAsync(
            Arg.Is<SavedSearch>(s => s.FiltersJson == null),
            Arg.Any<CancellationToken>());
    }
}
