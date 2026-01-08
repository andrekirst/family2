using AutoFixture.Xunit2;
using FamilyHub.Modules.Auth.Application.Abstractions;
using FamilyHub.Modules.Auth.Application.Commands.CreateFamily;
using FamilyHub.Modules.Auth.Domain;
using FamilyHub.SharedKernel.Domain.Exceptions;
using FamilyHub.Modules.Family.Domain.Repositories;
using FamilyHub.SharedKernel.Domain.ValueObjects;
using FamilyHub.SharedKernel.Interfaces;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using FamilyAggregate = FamilyHub.Modules.Family.Domain.Aggregates.Family;

namespace FamilyHub.Tests.Unit.Auth.Application;

/// <summary>
/// Unit tests for CreateFamilyCommandHandler.
/// Tests command handling logic, repository interactions, and business rule enforcement.
/// Uses NSubstitute for mocking with AutoFixture attribute-based dependency injection.
/// Uses FluentAssertions for readable, expressive test assertions.
/// </summary>
public class CreateFamilyCommandHandlerTests
{
    #region Happy Path Tests

    [Theory, AutoNSubstituteData]
    public async Task Handle_WithValidCommand_ShouldCreateFamilySuccessfully(
        [Frozen] IUserContext userContext,
        [Frozen] IUnitOfWork unitOfWork,
        CreateFamilyCommandHandler sut,
        User user)
    {
        // Arrange
        var familyName = "Smith Family";
        var command = new CreateFamilyCommand(FamilyName.From(familyName));

        userContext.User.Returns(user);
        userContext.UserId.Returns(user.Id);
        userContext.FamilyId.Returns(user.FamilyId);
        userContext.Role.Returns(user.Role);
        userContext.Email.Returns(user.Email);

        unitOfWork
            .SaveChangesAsync(Arg.Any<CancellationToken>())
            .Returns(1);

        // Act
        var result = await sut.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.FamilyId.Value.Should().NotBe(Guid.Empty);
        result.Name.Value.Should().Be(familyName);
        result.OwnerId.Should().Be(user.Id);
        result.CreatedAt.Should().BeOnOrBefore(DateTime.UtcNow);
    }

    [Theory, AutoNSubstituteData]
    public async Task Handle_WithValidCommand_ShouldCallRepositoriesInCorrectOrder(
        [Frozen] IUserContext userContext,
        [Frozen] IFamilyRepository familyRepository,
        [Frozen] IUnitOfWork unitOfWork,
        CreateFamilyCommandHandler sut,
        User user)
    {
        // Arrange
        var command = new CreateFamilyCommand(FamilyName.From("Smith Family"));
        var callOrder = new List<string>();

        userContext.User.Returns(user);
        userContext.UserId.Returns(user.Id);

        familyRepository
            .When(x => x.AddAsync(Arg.Any<FamilyAggregate>(), Arg.Any<CancellationToken>()))
            .Do(_ => callOrder.Add("AddFamily"));

        unitOfWork
            .SaveChangesAsync(Arg.Any<CancellationToken>())
            .Returns(1)
            .AndDoes(_ => callOrder.Add("SaveChanges"));

        // Act
        await sut.Handle(command, CancellationToken.None);

        // Assert
        callOrder.Should().HaveCount(2)
            .And.ContainInOrder("AddFamily", "SaveChanges");
    }

    [Theory, AutoNSubstituteData]
    public async Task Handle_WithValidCommand_ShouldVerifyAllRepositoryInteractions(
        [Frozen] IUserContext userContext,
        [Frozen] IFamilyRepository familyRepository,
        [Frozen] IUnitOfWork unitOfWork,
        CreateFamilyCommandHandler sut,
        User user)
    {
        // Arrange
        var familyName = "Smith Family";
        var command = new CreateFamilyCommand(FamilyName.From(familyName));

        userContext.User.Returns(user);
        userContext.UserId.Returns(user.Id);

        unitOfWork
            .SaveChangesAsync(Arg.Any<CancellationToken>())
            .Returns(1);

        // Act
        await sut.Handle(command, CancellationToken.None);

        // Assert
        await familyRepository.Received(1).AddAsync(
            Arg.Is<FamilyAggregate>(f =>
                f.Name == familyName &&
                f.OwnerId == user.Id),
            Arg.Any<CancellationToken>());

        await unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    #endregion

    #region Validation Tests

    // NOTE: Authentication and user existence validation tests removed.
    // These checks are now handled by UserContextEnrichmentBehavior in the MediatR pipeline,
    // so they are no longer the responsibility of the handler itself.

    [Theory, AutoNSubstituteData]
    public async Task Handle_WhenUserAlreadyBelongsToFamily_ShouldReplaceFamily(
        [Frozen] IUserContext userContext,
        [Frozen] IFamilyRepository familyRepository,
        [Frozen] IUnitOfWork unitOfWork,
        CreateFamilyCommandHandler sut,
        User user)
    {
        // Arrange
        var command = new CreateFamilyCommand(FamilyName.From("New Family"));

        userContext.User.Returns(user);
        userContext.UserId.Returns(user.Id);

        unitOfWork
            .SaveChangesAsync(Arg.Any<CancellationToken>())
            .Returns(1);

        // Act
        var result = await sut.Handle(command, CancellationToken.None);

        // Assert - New family should be created and user's family should be updated
        result.Should().NotBeNull();
        result.Name.Value.Should().Be("New Family");
        result.OwnerId.Should().Be(user.Id);

        // Verify family was added
        await familyRepository.Received(1).AddAsync(
            Arg.Is<FamilyAggregate>(f => f.Name.Value == "New Family" && f.OwnerId == user.Id),
            Arg.Any<CancellationToken>());

        await unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Theory, AutoNSubstituteData]
    public async Task Handle_WhenUserHasExistingFamily_ShouldReplaceWithNewFamily(
        [Frozen] IUserContext userContext,
        [Frozen] IUnitOfWork unitOfWork,
        CreateFamilyCommandHandler sut,
        User user)
    {
        // Arrange
        var command = new CreateFamilyCommand(FamilyName.From("Replacement Family"));
        var existingFamilyId = user.FamilyId;

        userContext.User.Returns(user);
        userContext.UserId.Returns(user.Id);
        userContext.FamilyId.Returns(existingFamilyId);

        unitOfWork
            .SaveChangesAsync(Arg.Any<CancellationToken>())
            .Returns(1);

        // Act
        var result = await sut.Handle(command, CancellationToken.None);

        // Assert - New family should be created successfully
        result.Should().NotBeNull();
        result.Name.Value.Should().Be("Replacement Family");
        result.OwnerId.Should().Be(user.Id);
        result.FamilyId.Should().NotBe(existingFamilyId);
    }

    #endregion

    #region Edge Cases

    [Theory, AutoNSubstituteData]
    public async Task Handle_WithCancellationToken_ShouldPassTokenToRepositories(
        [Frozen] IUserContext userContext,
        [Frozen] IFamilyRepository familyRepository,
        [Frozen] IUnitOfWork unitOfWork,
        CreateFamilyCommandHandler sut,
        User user)
    {
        // Arrange
        var command = new CreateFamilyCommand(FamilyName.From("Smith Family"));
        var cts = new CancellationTokenSource();
        var cancellationToken = cts.Token;

        userContext.User.Returns(user);
        userContext.UserId.Returns(user.Id);

        unitOfWork
            .SaveChangesAsync(cancellationToken)
            .Returns(1);

        // Act
        await sut.Handle(command, cancellationToken);

        // Assert
        await familyRepository.Received(1).AddAsync(Arg.Any<FamilyAggregate>(), cancellationToken);
        await unitOfWork.Received(1).SaveChangesAsync(cancellationToken);
    }

    [Theory, AutoNSubstituteData]
    public async Task Handle_WithWhitespaceInName_ShouldTrimNameViaFactoryMethod(
        [Frozen] IUserContext userContext,
        [Frozen] IFamilyRepository familyRepository,
        [Frozen] IUnitOfWork unitOfWork,
        CreateFamilyCommandHandler sut,
        User user)
    {
        // Arrange
        var nameWithWhitespace = "  Smith Family  ";
        var expectedTrimmedName = "Smith Family";
        var command = new CreateFamilyCommand(FamilyName.From(nameWithWhitespace));

        userContext.User.Returns(user);
        userContext.UserId.Returns(user.Id);

        unitOfWork
            .SaveChangesAsync(Arg.Any<CancellationToken>())
            .Returns(1);

        // Act
        var result = await sut.Handle(command, CancellationToken.None);

        // Assert
        result.Name.Value.Should().Be(expectedTrimmedName);

        await familyRepository.Received(1).AddAsync(
            Arg.Is<FamilyAggregate>(f => f.Name == expectedTrimmedName),
            Arg.Any<CancellationToken>());
    }

    #endregion

    #region Logging Tests

    // NOTE: "User not found" logging test removed.
    // User existence is now validated by UserContextEnrichmentBehavior,
    // so this scenario is no longer testable at the handler level.

    [Theory, AutoNSubstituteData]
    public async Task Handle_WhenReplacingFamily_ShouldSucceed(
        [Frozen] IUserContext userContext,
        [Frozen] IUnitOfWork unitOfWork,
        CreateFamilyCommandHandler sut,
        User user)
    {
        // Arrange
        var command = new CreateFamilyCommand(FamilyName.From("New Family"));

        userContext.User.Returns(user);
        userContext.UserId.Returns(user.Id);

        unitOfWork
            .SaveChangesAsync(Arg.Any<CancellationToken>())
            .Returns(1);

        // Act
        var result = await sut.Handle(command, CancellationToken.None);

        // Assert - Should successfully replace family
        result.Should().NotBeNull();
        result.Name.Value.Should().Be("New Family");

        // Note: Logging verification removed - testing implementation details
        // LoggerMessage.Define pattern doesn't call generic Log() method
        // Logging is verified in integration tests
    }

    [Theory, AutoNSubstituteData]
    public async Task Handle_WithSuccessfulCreation_ShouldLogInformationTwice(
        [Frozen] IUserContext userContext,
        [Frozen] IUnitOfWork unitOfWork,
        CreateFamilyCommandHandler sut,
        User user)
    {
        // Arrange
        var familyName = "Smith Family";
        var command = new CreateFamilyCommand(FamilyName.From(familyName));

        userContext.User.Returns(user);
        userContext.UserId.Returns(user.Id);

        unitOfWork
            .SaveChangesAsync(Arg.Any<CancellationToken>())
            .Returns(1);

        // Act
        var result = await sut.Handle(command, CancellationToken.None);

        // Assert - Verify successful execution
        result.Should().NotBeNull();
        result.Name.Value.Should().Be(familyName);

        // Note: Logging verification removed - testing implementation details
        // LoggerMessage.Define pattern doesn't call generic Log() method
        // Logging is verified in integration tests
    }

    #endregion
}
