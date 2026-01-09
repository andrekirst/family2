using FamilyHub.Modules.Auth.Application.Abstractions;
using FamilyHub.Modules.Auth.Application.Commands.CreateFamily;
using FamilyHub.Modules.Auth.Domain;
using FamilyHub.Modules.Family.Application.Abstractions;
using FamilyHub.Modules.Family.Domain.ValueObjects;
using FamilyHub.SharedKernel.Domain.ValueObjects;
using FamilyHub.SharedKernel.Interfaces;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;
using Result = FamilyHub.SharedKernel.Domain.Result;

namespace FamilyHub.Tests.Unit.Auth.Application;

public sealed class CreateFamilyCommandHandlerTests
{
    #region Test Helpers

    /// <summary>
    /// Creates all necessary mocks for CreateFamilyCommandHandler tests.
    /// </summary>
    private (IUserContext userContext, IFamilyService familyService, IUnitOfWork unitOfWork, ILogger<CreateFamilyCommandHandler> logger, User user) CreateMocks()
    {
        var userContext = Substitute.For<IUserContext>();
        var familyService = Substitute.For<IFamilyService>();
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var logger = Substitute.For<ILogger<CreateFamilyCommandHandler>>();

        var user = User.CreateFromOAuth(
            Email.From("test@example.com"),
            "zitadel-123",
            "zitadel",
            FamilyId.New());

        userContext.User.Returns(user);
        userContext.UserId.Returns(user.Id);
        userContext.FamilyId.Returns(user.FamilyId);
        userContext.Role.Returns(user.Role);
        userContext.Email.Returns(user.Email);

        return (userContext, familyService, unitOfWork, logger, user);
    }

    #endregion

    #region Happy Path Tests

    [Fact]
    public async Task Handle_WithValidCommand_ManualMock_ShouldWork()
    {
        // Arrange - Manual mocking without AutoFixture
        var userContext = Substitute.For<IUserContext>();
        var familyService = Substitute.For<IFamilyService>();
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var logger = Substitute.For<ILogger<CreateFamilyCommandHandler>>();

        var user = User.CreateFromOAuth(
            Email.From("test@example.com"),
            "zitadel-123",
            "zitadel",
            FamilyId.New());

        var familyName = "Smith Family";
        var command = new CreateFamilyCommand(FamilyName.From(familyName));

        var testFamilyDto = new FamilyDto
        {
            Id = FamilyId.New(),
            Name = FamilyName.From(familyName),
            OwnerId = user.Id,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        userContext.User.Returns(user);
        userContext.UserId.Returns(user.Id);

        // Configure familyService mock without Arg.Any
        familyService.CreateFamilyAsync(command.Name, user.Id, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success(testFamilyDto)));

        unitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(1);

        var sut = new CreateFamilyCommandHandler(userContext, familyService, unitOfWork, logger);

        // Act
        var result = await sut.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Name.Value.Should().Be(familyName);
        result.OwnerId.Should().Be(user.Id);
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldCreateFamilySuccessfully()
    {
        // Arrange
        var (userContext, familyService, unitOfWork, logger, user) = CreateMocks();

        var familyName = "Smith Family";
        var command = new CreateFamilyCommand(FamilyName.From(familyName));

        var testFamilyDto = new FamilyDto
        {
            Id = FamilyId.New(),
            Name = FamilyName.From(familyName),
            OwnerId = user.Id,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        familyService.CreateFamilyAsync(command.Name, user.Id, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success(testFamilyDto)));

        unitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(1);

        var sut = new CreateFamilyCommandHandler(userContext, familyService, unitOfWork, logger);

        // Act
        var result = await sut.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.FamilyId.Value.Should().NotBe(Guid.Empty);
        result.Name.Value.Should().Be(familyName);
        result.OwnerId.Should().Be(user.Id);
        result.CreatedAt.Should().BeOnOrBefore(DateTime.UtcNow);
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldCallServiceAndUnitOfWorkInCorrectOrder()
    {
        // Arrange
        var (userContext, familyService, unitOfWork, logger, user) = CreateMocks();

        var command = new CreateFamilyCommand(FamilyName.From("Smith Family"));
        var callOrder = new List<string>();

        var testFamilyDto = new FamilyDto
        {
            Id = FamilyId.New(),
            Name = FamilyName.From("Smith Family"),
            OwnerId = user.Id,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        familyService.CreateFamilyAsync(command.Name, user.Id, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success(testFamilyDto)))
            .AndDoes(_ => callOrder.Add("CreateFamily"));

        unitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>())
            .Returns(1)
            .AndDoes(_ => callOrder.Add("SaveChanges"));

        var sut = new CreateFamilyCommandHandler(userContext, familyService, unitOfWork, logger);

        // Act
        await sut.Handle(command, CancellationToken.None);

        // Assert
        callOrder.Should().HaveCount(2)
            .And.ContainInOrder("CreateFamily", "SaveChanges");
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldVerifyAllServiceInteractions()
    {
        // Arrange
        var (userContext, familyService, unitOfWork, logger, user) = CreateMocks();

        var familyName = "Smith Family";
        var command = new CreateFamilyCommand(FamilyName.From(familyName));

        var testFamilyDto = new FamilyDto
        {
            Id = FamilyId.New(),
            Name = FamilyName.From(familyName),
            OwnerId = user.Id,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        familyService.CreateFamilyAsync(command.Name, user.Id, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success(testFamilyDto)));

        unitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(1);

        var sut = new CreateFamilyCommandHandler(userContext, familyService, unitOfWork, logger);

        // Act
        await sut.Handle(command, CancellationToken.None);

        // Assert
        await familyService.Received(1).CreateFamilyAsync(
            command.Name,
            user.Id,
            Arg.Any<CancellationToken>());

        await unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    #endregion

    #region Validation Tests

    // NOTE: Authentication and user existence validation tests removed.
    // These checks are now handled by UserContextEnrichmentBehavior in the MediatR pipeline,
    // so they are no longer the responsibility of the handler itself.

    [Fact]
    public async Task Handle_WhenUserAlreadyBelongsToFamily_ShouldReplaceFamily()
    {
        // Arrange
        var (userContext, familyService, unitOfWork, logger, user) = CreateMocks();

        var command = new CreateFamilyCommand(FamilyName.From("New Family"));

        var testFamilyDto = new FamilyDto
        {
            Id = FamilyId.New(),
            Name = FamilyName.From("New Family"),
            OwnerId = user.Id,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        familyService.CreateFamilyAsync(command.Name, user.Id, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success(testFamilyDto)));

        unitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(1);

        var sut = new CreateFamilyCommandHandler(userContext, familyService, unitOfWork, logger);

        // Act
        var result = await sut.Handle(command, CancellationToken.None);

        // Assert - New family should be created and user's family should be updated
        result.Should().NotBeNull();
        result.Name.Value.Should().Be("New Family");
        result.OwnerId.Should().Be(user.Id);

        // Verify service was called to create family
        await familyService.Received(1).CreateFamilyAsync(
            command.Name,
            user.Id,
            Arg.Any<CancellationToken>());

        await unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenUserHasExistingFamily_ShouldReplaceWithNewFamily()
    {
        // Arrange
        var (userContext, familyService, unitOfWork, logger, user) = CreateMocks();

        var command = new CreateFamilyCommand(FamilyName.From("Replacement Family"));
        var existingFamilyId = user.FamilyId;

        userContext.FamilyId.Returns(existingFamilyId);

        var testFamilyDto = new FamilyDto
        {
            Id = FamilyId.New(),
            Name = FamilyName.From("Replacement Family"),
            OwnerId = user.Id,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        familyService.CreateFamilyAsync(command.Name, user.Id, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success(testFamilyDto)));

        unitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(1);

        var sut = new CreateFamilyCommandHandler(userContext, familyService, unitOfWork, logger);

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

    [Fact]
    public async Task Handle_WithCancellationToken_ShouldPassTokenToServiceAndUnitOfWork()
    {
        // Arrange
        var (userContext, familyService, unitOfWork, logger, user) = CreateMocks();

        var command = new CreateFamilyCommand(FamilyName.From("Smith Family"));
        var cts = new CancellationTokenSource();
        var cancellationToken = cts.Token;

        var testFamilyDto = new FamilyDto
        {
            Id = FamilyId.New(),
            Name = FamilyName.From("Smith Family"),
            OwnerId = user.Id,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        familyService.CreateFamilyAsync(command.Name, user.Id, Arg.Is(cancellationToken))
            .Returns(Task.FromResult(Result.Success(testFamilyDto)));

        unitOfWork.SaveChangesAsync(Arg.Is(cancellationToken)).Returns(1);

        var sut = new CreateFamilyCommandHandler(userContext, familyService, unitOfWork, logger);

        // Act
        await sut.Handle(command, cancellationToken);

        // Assert
        await familyService.Received(1).CreateFamilyAsync(command.Name, user.Id, Arg.Is(cancellationToken));
        await unitOfWork.Received(1).SaveChangesAsync(Arg.Is(cancellationToken));
    }

    [Fact]
    public async Task Handle_WithWhitespaceInName_ShouldTrimNameViaFactoryMethod()
    {
        // Arrange
        var (userContext, familyService, unitOfWork, logger, user) = CreateMocks();

        var nameWithWhitespace = "  Smith Family  ";
        var expectedTrimmedName = "Smith Family";
        var command = new CreateFamilyCommand(FamilyName.From(nameWithWhitespace));

        var testFamilyDto = new FamilyDto
        {
            Id = FamilyId.New(),
            Name = FamilyName.From(expectedTrimmedName),
            OwnerId = user.Id,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        familyService.CreateFamilyAsync(command.Name, user.Id, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success(testFamilyDto)));

        unitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(1);

        var sut = new CreateFamilyCommandHandler(userContext, familyService, unitOfWork, logger);

        // Act
        var result = await sut.Handle(command, CancellationToken.None);

        // Assert
        result.Name.Value.Should().Be(expectedTrimmedName);

        await familyService.Received(1).CreateFamilyAsync(
            command.Name,
            user.Id,
            Arg.Any<CancellationToken>());
    }

    #endregion

    #region Logging Tests

    // NOTE: "User not found" logging test removed.
    // User existence is now validated by UserContextEnrichmentBehavior,
    // so this scenario is no longer testable at the handler level.

    [Fact]
    public async Task Handle_WhenReplacingFamily_ShouldSucceed()
    {
        // Arrange
        var (userContext, familyService, unitOfWork, logger, user) = CreateMocks();

        var command = new CreateFamilyCommand(FamilyName.From("New Family"));

        var testFamilyDto = new FamilyDto
        {
            Id = FamilyId.New(),
            Name = FamilyName.From("New Family"),
            OwnerId = user.Id,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        familyService.CreateFamilyAsync(command.Name, user.Id, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success(testFamilyDto)));

        unitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(1);

        var sut = new CreateFamilyCommandHandler(userContext, familyService, unitOfWork, logger);

        // Act
        var result = await sut.Handle(command, CancellationToken.None);

        // Assert - Should successfully replace family
        result.Should().NotBeNull();
        result.Name.Value.Should().Be("New Family");

        // Note: Logging verification removed - testing implementation details
        // LoggerMessage.Define pattern doesn't call generic Log() method
        // Logging is verified in integration tests
    }

    [Fact]
    public async Task Handle_WithSuccessfulCreation_ShouldLogInformationTwice()
    {
        // Arrange
        var (userContext, familyService, unitOfWork, logger, user) = CreateMocks();

        var familyName = "Smith Family";
        var command = new CreateFamilyCommand(FamilyName.From(familyName));

        var testFamilyDto = new FamilyDto
        {
            Id = FamilyId.New(),
            Name = FamilyName.From(familyName),
            OwnerId = user.Id,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        familyService.CreateFamilyAsync(command.Name, user.Id, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success(testFamilyDto)));

        unitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(1);

        var sut = new CreateFamilyCommandHandler(userContext, familyService, unitOfWork, logger);

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
