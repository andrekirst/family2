using FamilyHub.Modules.Auth.Application.Abstractions;
using FamilyHub.Modules.Auth.Application.Commands.CreateFamily;
using FamilyHub.Modules.Auth.Domain;
using FamilyHub.Modules.Auth.Domain.Repositories;
using FamilyHub.SharedKernel.Domain.ValueObjects;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace FamilyHub.Tests.Unit.Auth.Application;

/// <summary>
/// Unit tests for CreateFamilyCommandHandler.
/// Tests command handling logic, repository interactions, and business rule enforcement.
/// Uses NSubstitute for mocking with AutoFixture attribute-based dependency injection.
/// </summary>
public class CreateFamilyCommandHandlerTests
{
    #region Constructor Tests

    [Theory, AutoNSubstituteData]
    public void Constructor_WithNullUserRepository_ShouldThrowArgumentNullException(
        IFamilyRepository familyRepository,
        IUnitOfWork unitOfWork,
        ILogger<CreateFamilyCommandHandler> logger)
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() =>
            new CreateFamilyCommandHandler(
                null!,
                familyRepository,
                unitOfWork,
                logger));

        Assert.Equal("userRepository", exception.ParamName);
    }

    [Theory, AutoNSubstituteData]
    public void Constructor_WithNullFamilyRepository_ShouldThrowArgumentNullException(
        IUserRepository userRepository,
        IUnitOfWork unitOfWork,
        ILogger<CreateFamilyCommandHandler> logger)
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() =>
            new CreateFamilyCommandHandler(
                userRepository,
                null!,
                unitOfWork,
                logger));

        Assert.Equal("familyRepository", exception.ParamName);
    }

    [Theory, AutoNSubstituteData]
    public void Constructor_WithNullUnitOfWork_ShouldThrowArgumentNullException(
        IUserRepository userRepository,
        IFamilyRepository familyRepository,
        ILogger<CreateFamilyCommandHandler> logger)
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() =>
            new CreateFamilyCommandHandler(
                userRepository,
                familyRepository,
                null!,
                logger));

        Assert.Equal("unitOfWork", exception.ParamName);
    }

    [Theory, AutoNSubstituteData]
    public void Constructor_WithNullLogger_ShouldThrowArgumentNullException(
        IUserRepository userRepository,
        IFamilyRepository familyRepository,
        IUnitOfWork unitOfWork)
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() =>
            new CreateFamilyCommandHandler(
                userRepository,
                familyRepository,
                unitOfWork,
                null!));

        Assert.Equal("logger", exception.ParamName);
    }

    #endregion

    #region Happy Path Tests

    [Theory, AutoNSubstituteData]
    public async Task Handle_WithValidCommand_ShouldCreateFamilySuccessfully(
        IUserRepository userRepository,
        IFamilyRepository familyRepository,
        IUnitOfWork unitOfWork,
        ILogger<CreateFamilyCommandHandler> logger)
    {
        // Arrange
        var userId = UserId.New();
        var familyName = "Smith Family";
        var command = new CreateFamilyCommand(familyName, userId);
        var user = User.CreateFromOAuth(
            Email.From("test@example.com"),
            "ext123",
            "zitadel");

        userRepository
            .GetByIdAsync(userId, Arg.Any<CancellationToken>())
            .Returns(user);

        familyRepository
            .GetFamiliesByUserIdAsync(userId, Arg.Any<CancellationToken>())
            .Returns(new List<Family>());

        unitOfWork
            .SaveChangesAsync(Arg.Any<CancellationToken>())
            .Returns(1);

        var handler = new CreateFamilyCommandHandler(
            userRepository,
            familyRepository,
            unitOfWork,
            logger);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.NotEqual(Guid.Empty, result.FamilyId.Value);
        Assert.Equal(familyName, result.Name);
        Assert.Equal(userId, result.OwnerId);
        Assert.True(result.CreatedAt <= DateTime.UtcNow);
    }

    [Theory, AutoNSubstituteData]
    public async Task Handle_WithValidCommand_ShouldCallRepositoriesInCorrectOrder(
        IUserRepository userRepository,
        IFamilyRepository familyRepository,
        IUnitOfWork unitOfWork,
        ILogger<CreateFamilyCommandHandler> logger)
    {
        // Arrange
        var userId = UserId.New();
        var command = new CreateFamilyCommand("Smith Family", userId);
        var user = User.CreateFromOAuth(
            Email.From("test@example.com"),
            "ext123",
            "zitadel");

        var callOrder = new List<string>();

        userRepository
            .GetByIdAsync(userId, Arg.Any<CancellationToken>())
            .Returns(user)
            .AndDoes(_ => callOrder.Add("GetUser"));

        familyRepository
            .GetFamiliesByUserIdAsync(userId, Arg.Any<CancellationToken>())
            .Returns(new List<Family>())
            .AndDoes(_ => callOrder.Add("GetFamilies"));

        familyRepository
            .When(x => x.AddAsync(Arg.Any<Family>(), Arg.Any<CancellationToken>()))
            .Do(_ => callOrder.Add("AddFamily"));

        unitOfWork
            .SaveChangesAsync(Arg.Any<CancellationToken>())
            .Returns(1)
            .AndDoes(_ => callOrder.Add("SaveChanges"));

        var handler = new CreateFamilyCommandHandler(
            userRepository,
            familyRepository,
            unitOfWork,
            logger);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Equal(4, callOrder.Count);
        Assert.Equal("GetUser", callOrder[0]);
        Assert.Equal("GetFamilies", callOrder[1]);
        Assert.Equal("AddFamily", callOrder[2]);
        Assert.Equal("SaveChanges", callOrder[3]);
    }

    [Theory, AutoNSubstituteData]
    public async Task Handle_WithValidCommand_ShouldVerifyAllRepositoryInteractions(
        IUserRepository userRepository,
        IFamilyRepository familyRepository,
        IUnitOfWork unitOfWork,
        ILogger<CreateFamilyCommandHandler> logger)
    {
        // Arrange
        var userId = UserId.New();
        var familyName = "Smith Family";
        var command = new CreateFamilyCommand(familyName, userId);
        var user = User.CreateFromOAuth(
            Email.From("test@example.com"),
            "ext123",
            "zitadel");

        userRepository
            .GetByIdAsync(userId, Arg.Any<CancellationToken>())
            .Returns(user);

        familyRepository
            .GetFamiliesByUserIdAsync(userId, Arg.Any<CancellationToken>())
            .Returns(new List<Family>());

        unitOfWork
            .SaveChangesAsync(Arg.Any<CancellationToken>())
            .Returns(1);

        var handler = new CreateFamilyCommandHandler(
            userRepository,
            familyRepository,
            unitOfWork,
            logger);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        await userRepository.Received(1).GetByIdAsync(userId, Arg.Any<CancellationToken>());
        await familyRepository.Received(1).GetFamiliesByUserIdAsync(userId, Arg.Any<CancellationToken>());

        await familyRepository.Received(1).AddAsync(
            Arg.Is<Family>(f =>
                f.Name == familyName &&
                f.OwnerId == userId &&
                f.UserFamilies.Count == 1),
            Arg.Any<CancellationToken>());

        await unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    #endregion

    #region Validation Tests

    [Theory, AutoNSubstituteData]
    public async Task Handle_WhenUserDoesNotExist_ShouldThrowInvalidOperationException(
        IUserRepository userRepository,
        IFamilyRepository familyRepository,
        IUnitOfWork unitOfWork,
        ILogger<CreateFamilyCommandHandler> logger)
    {
        // Arrange
        var userId = UserId.New();
        var command = new CreateFamilyCommand("Smith Family", userId);

        userRepository
            .GetByIdAsync(userId, Arg.Any<CancellationToken>())
            .Returns((User?)null);

        var handler = new CreateFamilyCommandHandler(
            userRepository,
            familyRepository,
            unitOfWork,
            logger);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => handler.Handle(command, CancellationToken.None));

        Assert.Contains($"User with ID {userId.Value} not found", exception.Message);

        // Verify that we stopped at user validation and didn't proceed
        await familyRepository.DidNotReceiveWithAnyArgs().GetFamiliesByUserIdAsync(UserId.New(), CancellationToken.None);
        await familyRepository.DidNotReceiveWithAnyArgs().AddAsync(null!, CancellationToken.None);
        await unitOfWork.DidNotReceiveWithAnyArgs().SaveChangesAsync(CancellationToken.None);
    }

    [Theory, AutoNSubstituteData]
    public async Task Handle_WhenUserAlreadyBelongsToFamily_ShouldThrowInvalidOperationException(
        IUserRepository userRepository,
        IFamilyRepository familyRepository,
        IUnitOfWork unitOfWork,
        ILogger<CreateFamilyCommandHandler> logger)
    {
        // Arrange
        var userId = UserId.New();
        var command = new CreateFamilyCommand("Smith Family", userId);
        var user = User.CreateFromOAuth(
            Email.From("test@example.com"),
            "ext123",
            "zitadel");

        var existingFamily = Family.Create("Existing Family", userId);

        userRepository
            .GetByIdAsync(userId, Arg.Any<CancellationToken>())
            .Returns(user);

        familyRepository
            .GetFamiliesByUserIdAsync(userId, Arg.Any<CancellationToken>())
            .Returns(new List<Family> { existingFamily });

        var handler = new CreateFamilyCommandHandler(
            userRepository,
            familyRepository,
            unitOfWork,
            logger);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => handler.Handle(command, CancellationToken.None));

        Assert.Contains("User already belongs to a family", exception.Message);
        Assert.Contains("Users can only be members of one family at a time", exception.Message);

        // Verify that we stopped after the family check
        await familyRepository.DidNotReceiveWithAnyArgs().AddAsync(null!, CancellationToken.None);
        await unitOfWork.DidNotReceiveWithAnyArgs().SaveChangesAsync(CancellationToken.None);
    }

    [Theory, AutoNSubstituteData]
    public async Task Handle_WhenUserBelongsToMultipleFamilies_ShouldThrowInvalidOperationException(
        IUserRepository userRepository,
        IFamilyRepository familyRepository,
        IUnitOfWork unitOfWork,
        ILogger<CreateFamilyCommandHandler> logger)
    {
        // Arrange
        var userId = UserId.New();
        var command = new CreateFamilyCommand("Smith Family", userId);
        var user = User.CreateFromOAuth(
            Email.From("test@example.com"),
            "ext123",
            "zitadel");

        var existingFamily1 = Family.Create("Existing Family 1", userId);
        var existingFamily2 = Family.Create("Existing Family 2", userId);

        userRepository
            .GetByIdAsync(userId, Arg.Any<CancellationToken>())
            .Returns(user);

        familyRepository
            .GetFamiliesByUserIdAsync(userId, Arg.Any<CancellationToken>())
            .Returns(new List<Family> { existingFamily1, existingFamily2 });

        var handler = new CreateFamilyCommandHandler(
            userRepository,
            familyRepository,
            unitOfWork,
            logger);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => handler.Handle(command, CancellationToken.None));

        Assert.Contains("User already belongs to a family", exception.Message);
    }

    #endregion

    #region Edge Cases

    [Theory, AutoNSubstituteData]
    public async Task Handle_WithCancellationToken_ShouldPassTokenToRepositories(
        IUserRepository userRepository,
        IFamilyRepository familyRepository,
        IUnitOfWork unitOfWork,
        ILogger<CreateFamilyCommandHandler> logger)
    {
        // Arrange
        var userId = UserId.New();
        var command = new CreateFamilyCommand("Smith Family", userId);
        var user = User.CreateFromOAuth(
            Email.From("test@example.com"),
            "ext123",
            "zitadel");
        var cancellationToken = new CancellationToken();

        userRepository
            .GetByIdAsync(userId, cancellationToken)
            .Returns(user);

        familyRepository
            .GetFamiliesByUserIdAsync(userId, cancellationToken)
            .Returns(new List<Family>());

        unitOfWork
            .SaveChangesAsync(cancellationToken)
            .Returns(1);

        var handler = new CreateFamilyCommandHandler(
            userRepository,
            familyRepository,
            unitOfWork,
            logger);

        // Act
        await handler.Handle(command, cancellationToken);

        // Assert
        await userRepository.Received(1).GetByIdAsync(userId, cancellationToken);
        await familyRepository.Received(1).GetFamiliesByUserIdAsync(userId, cancellationToken);
        await familyRepository.Received(1).AddAsync(Arg.Any<Family>(), cancellationToken);
        await unitOfWork.Received(1).SaveChangesAsync(cancellationToken);
    }

    [Theory, AutoNSubstituteData]
    public async Task Handle_WithWhitespaceInName_ShouldTrimNameViaFactoryMethod(
        IUserRepository userRepository,
        IFamilyRepository familyRepository,
        IUnitOfWork unitOfWork,
        ILogger<CreateFamilyCommandHandler> logger)
    {
        // Arrange
        var userId = UserId.New();
        var nameWithWhitespace = "  Smith Family  ";
        var expectedTrimmedName = "Smith Family";
        var command = new CreateFamilyCommand(nameWithWhitespace, userId);
        var user = User.CreateFromOAuth(
            Email.From("test@example.com"),
            "ext123",
            "zitadel");

        userRepository
            .GetByIdAsync(userId, Arg.Any<CancellationToken>())
            .Returns(user);

        familyRepository
            .GetFamiliesByUserIdAsync(userId, Arg.Any<CancellationToken>())
            .Returns(new List<Family>());

        unitOfWork
            .SaveChangesAsync(Arg.Any<CancellationToken>())
            .Returns(1);

        var handler = new CreateFamilyCommandHandler(
            userRepository,
            familyRepository,
            unitOfWork,
            logger);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Equal(expectedTrimmedName, result.Name);

        await familyRepository.Received(1).AddAsync(
            Arg.Is<Family>(f => f.Name == expectedTrimmedName),
            Arg.Any<CancellationToken>());
    }

    #endregion

    #region Logging Tests

    [Theory, AutoNSubstituteData]
    public async Task Handle_WhenUserNotFound_ShouldLogWarning(
        IUserRepository userRepository,
        IFamilyRepository familyRepository,
        IUnitOfWork unitOfWork,
        ILogger<CreateFamilyCommandHandler> logger)
    {
        // Arrange
        var userId = UserId.New();
        var command = new CreateFamilyCommand("Smith Family", userId);

        userRepository
            .GetByIdAsync(userId, Arg.Any<CancellationToken>())
            .Returns((User?)null);

        var handler = new CreateFamilyCommandHandler(
            userRepository,
            familyRepository,
            unitOfWork,
            logger);

        // Act
        try
        {
            await handler.Handle(command, CancellationToken.None);
        }
        catch (InvalidOperationException)
        {
            // Expected exception
        }

        // Assert
        logger.Received(1).Log(
            LogLevel.Warning,
            Arg.Any<EventId>(),
            Arg.Is<object>(v => v.ToString()!.Contains($"User {userId.Value} not found")),
            Arg.Any<Exception>(),
            Arg.Any<Func<object, Exception?, string>>());
    }

    [Theory, AutoNSubstituteData]
    public async Task Handle_WhenUserAlreadyHasFamily_ShouldLogWarning(
        IUserRepository userRepository,
        IFamilyRepository familyRepository,
        IUnitOfWork unitOfWork,
        ILogger<CreateFamilyCommandHandler> logger)
    {
        // Arrange
        var userId = UserId.New();
        var command = new CreateFamilyCommand("Smith Family", userId);
        var user = User.CreateFromOAuth(
            Email.From("test@example.com"),
            "ext123",
            "zitadel");

        var existingFamily = Family.Create("Existing Family", userId);

        userRepository
            .GetByIdAsync(userId, Arg.Any<CancellationToken>())
            .Returns(user);

        familyRepository
            .GetFamiliesByUserIdAsync(userId, Arg.Any<CancellationToken>())
            .Returns(new List<Family> { existingFamily });

        var handler = new CreateFamilyCommandHandler(
            userRepository,
            familyRepository,
            unitOfWork,
            logger);

        // Act
        try
        {
            await handler.Handle(command, CancellationToken.None);
        }
        catch (InvalidOperationException)
        {
            // Expected exception
        }

        // Assert
        logger.Received(1).Log(
            LogLevel.Warning,
            Arg.Any<EventId>(),
            Arg.Is<object>(v => v.ToString()!.Contains("already belongs to")),
            Arg.Any<Exception>(),
            Arg.Any<Func<object, Exception?, string>>());
    }

    [Theory, AutoNSubstituteData]
    public async Task Handle_WithSuccessfulCreation_ShouldLogInformationTwice(
        IUserRepository userRepository,
        IFamilyRepository familyRepository,
        IUnitOfWork unitOfWork,
        ILogger<CreateFamilyCommandHandler> logger)
    {
        // Arrange
        var userId = UserId.New();
        var familyName = "Smith Family";
        var command = new CreateFamilyCommand(familyName, userId);
        var user = User.CreateFromOAuth(
            Email.From("test@example.com"),
            "ext123",
            "zitadel");

        userRepository
            .GetByIdAsync(userId, Arg.Any<CancellationToken>())
            .Returns(user);

        familyRepository
            .GetFamiliesByUserIdAsync(userId, Arg.Any<CancellationToken>())
            .Returns(new List<Family>());

        unitOfWork
            .SaveChangesAsync(Arg.Any<CancellationToken>())
            .Returns(1);

        var handler = new CreateFamilyCommandHandler(
            userRepository,
            familyRepository,
            unitOfWork,
            logger);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert - Should log at start and at success
        logger.Received(2).Log(
            LogLevel.Information,
            Arg.Any<EventId>(),
            Arg.Any<object>(),
            Arg.Any<Exception>(),
            Arg.Any<Func<object, Exception?, string>>());
    }

    #endregion
}
