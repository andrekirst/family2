using FamilyHub.Api.Features.FileManagement.Application.Commands.DeleteSavedSearch;
using FamilyHub.Api.Features.FileManagement.Domain.Entities;
using FamilyHub.Api.Features.FileManagement.Domain.Repositories;
using FamilyHub.Common.Domain;
using FamilyHub.Common.Domain.ValueObjects;
using FluentAssertions;
using NSubstitute;

namespace FamilyHub.FileManagement.Tests.Features.FileManagement.Application;

public class DeleteSavedSearchCommandHandlerTests
{
    private readonly ISavedSearchRepository _savedRepo = Substitute.For<ISavedSearchRepository>();
    private readonly DeleteSavedSearchCommandHandler _handler;

    public DeleteSavedSearchCommandHandlerTests()
    {
        _handler = new DeleteSavedSearchCommandHandler(_savedRepo);
    }

    [Fact]
    public async Task Handle_ShouldDeleteSavedSearch()
    {
        var userId = UserId.New();
        var search = SavedSearch.Create(userId, "Test", "query", null, DateTimeOffset.UtcNow);
        _savedRepo.GetByIdAsync(search.Id, Arg.Any<CancellationToken>()).Returns(search);

        var command = new DeleteSavedSearchCommand(search.Id)
        {
            UserId = userId,
            FamilyId = FamilyId.New()
        };
        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        await _savedRepo.Received(1).RemoveAsync(search, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldThrowWhenNotFound()
    {
        _savedRepo.GetByIdAsync(SavedSearchId.New(), Arg.Any<CancellationToken>())
            .ReturnsForAnyArgs((SavedSearch?)null);

        var command = new DeleteSavedSearchCommand(SavedSearchId.New())
        {
            UserId = UserId.New(),
            FamilyId = FamilyId.New()
        };
        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Be(DomainErrorCodes.NotFound);
    }

    [Fact]
    public async Task Handle_ShouldThrowWhenDifferentUser()
    {
        var search = SavedSearch.Create(UserId.New(), "Test", "query", null, DateTimeOffset.UtcNow);
        _savedRepo.GetByIdAsync(search.Id, Arg.Any<CancellationToken>()).Returns(search);

        var command = new DeleteSavedSearchCommand(search.Id)
        {
            UserId = UserId.New(),
            FamilyId = FamilyId.New()
        }; // Different user
        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Be(DomainErrorCodes.Forbidden);
    }
}
