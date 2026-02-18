using FamilyHub.Api.Features.FileManagement.Application.Commands.DeleteSavedSearch;
using FamilyHub.Api.Features.FileManagement.Domain.Entities;
using FamilyHub.Common.Domain;
using FamilyHub.Common.Domain.ValueObjects;
using FamilyHub.TestCommon.Fakes;
using FluentAssertions;

namespace FamilyHub.FileManagement.Tests.Features.FileManagement.Application;

public class DeleteSavedSearchCommandHandlerTests
{
    [Fact]
    public async Task Handle_ShouldDeleteSavedSearch()
    {
        var savedRepo = new FakeSavedSearchRepository();
        var handler = new DeleteSavedSearchCommandHandler(savedRepo);

        var userId = UserId.New();
        var search = SavedSearch.Create(userId, "Test", "query", null);
        savedRepo.Searches.Add(search);

        var command = new DeleteSavedSearchCommand(search.Id, userId);
        var result = await handler.Handle(command, CancellationToken.None);

        result.Success.Should().BeTrue();
        savedRepo.Searches.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_ShouldThrowWhenNotFound()
    {
        var savedRepo = new FakeSavedSearchRepository();
        var handler = new DeleteSavedSearchCommandHandler(savedRepo);

        var command = new DeleteSavedSearchCommand(SavedSearchId.New(), UserId.New());
        var act = () => handler.Handle(command, CancellationToken.None).AsTask();

        await act.Should().ThrowAsync<DomainException>()
            .Where(e => e.ErrorCode == DomainErrorCodes.NotFound);
    }

    [Fact]
    public async Task Handle_ShouldThrowWhenDifferentUser()
    {
        var savedRepo = new FakeSavedSearchRepository();
        var handler = new DeleteSavedSearchCommandHandler(savedRepo);

        var search = SavedSearch.Create(UserId.New(), "Test", "query", null);
        savedRepo.Searches.Add(search);

        var command = new DeleteSavedSearchCommand(search.Id, UserId.New()); // Different user
        var act = () => handler.Handle(command, CancellationToken.None).AsTask();

        await act.Should().ThrowAsync<DomainException>()
            .Where(e => e.ErrorCode == DomainErrorCodes.Forbidden);
    }
}
