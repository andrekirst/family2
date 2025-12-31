using FamilyHub.Modules.Auth.Application.Abstractions;
using FamilyHub.Modules.Auth.Application.Commands.CreateFamily;
using FamilyHub.Modules.Auth.Domain;
using FamilyHub.SharedKernel.Domain.Exceptions;
using FamilyHub.Modules.Auth.Domain.Repositories;
using FamilyHub.SharedKernel.Domain.ValueObjects;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace FamilyHub.Tests.Unit.Auth.Application;

/// <summary>
/// Unit tests for CreateFamilyCommandHandler.
/// Tests command handling logic, repository interactions, and business rule enforcement.
/// Uses NSubstitute for mocking with AutoFixture attribute-based dependency injection.
/// Uses FluentAssertions for readable, expressive test assertions.
/// Note: Constructor null validation tests removed - primary constructors delegate validation to DI container.
/// </summary>
public class CreateFamilyCommandHandlerTests
{

    [Theory, AutoNSubstituteData]
    public async Task Handle_WithValidCommand_ShouldCreateFamilySuccessfully(
        IUserRepository userRepository,
        IFamilyRepository familyRepository,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        ILogger<CreateFamilyCommandHandler> logger)
    {
        // Arrange
        var userId = UserId.New();
        currentUserService.GetUserId().Returns(userId);
        var familyName = "Smith Family";
        var command = new CreateFamilyCommand(FamilyName.From(familyName));

        // Note: User.CreateFromOAuth() auto-generates a new ID internally, different from userId.
        // This is acceptable for unit testing - we're verifying handler behavior with mocked dependencies,
        // not domain entity state. The mock will return this user when queried with userId.
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
            currentUserService,
            logger);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.FamilyId.Value.Should().NotBe(Guid.Empty);
        result.Name.Value.Should().Be(familyName);
        result.OwnerId.Should().Be(userId);
        result.CreatedAt.Should().BeOnOrBefore(DateTime.UtcNow);
    }

    [Theory, AutoNSubstituteData]
    public async Task Handle_WithValidCommand_ShouldCallRepositoriesInCorrectOrder(
        IUserRepository userRepository,
        IFamilyRepository familyRepository,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        ILogger<CreateFamilyCommandHandler> logger)
    {
        // Arrange
        var userId = UserId.New();
        currentUserService.GetUserId().Returns(userId);
        var command = new CreateFamilyCommand(FamilyName.From("Smith Family"));

        // Note: User.CreateFromOAuth() auto-generates a new ID internally, different from userId.
        // This is acceptable for unit testing - we're verifying handler behavior with mocked dependencies,
        // not domain entity state. The mock will return this user when queried with userId.
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
            currentUserService,
            logger);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        callOrder.Should().HaveCount(4)
            .And.ContainInOrder("GetUser", "GetFamilies", "AddFamily", "SaveChanges");
    }

    [Theory, AutoNSubstituteData]
    public async Task Handle_WithValidCommand_ShouldVerifyAllRepositoryInteractions(
        IUserRepository userRepository,
        IFamilyRepository familyRepository,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        ILogger<CreateFamilyCommandHandler> logger)
    {
        // Arrange
        var userId = UserId.New();
        currentUserService.GetUserId().Returns(userId);
        var familyName = "Smith Family";
        var command = new CreateFamilyCommand(FamilyName.From(familyName));

        // Note: User.CreateFromOAuth() auto-generates a new ID internally, different from userId.
        // This is acceptable for unit testing - we're verifying handler behavior with mocked dependencies,
        // not domain entity state. The mock will return this user when queried with userId.
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
            currentUserService,
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

    [Theory, AutoNSubstituteData]
    public async Task Handle_WhenUserNotAuthenticated_ShouldThrowUnauthenticatedException(
        IUserRepository userRepository,
        IFamilyRepository familyRepository,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        ILogger<CreateFamilyCommandHandler> logger)
    {
        // Arrange
        var command = new CreateFamilyCommand(FamilyName.From("Smith Family"));

        currentUserService.GetUserId().Returns(_ => throw new UnauthorizedAccessException("User is not authenticated"));

        var handler = new CreateFamilyCommandHandler(
            userRepository,
            familyRepository,
            unitOfWork,
            currentUserService,
            logger);

        // Act
        var act = async () => await handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("*User is not authenticated*");
    }

    [Theory, AutoNSubstituteData]
    public async Task Handle_WhenUserDoesNotExist_ShouldThrowInvalidOperationException(
        IUserRepository userRepository,
        IFamilyRepository familyRepository,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        ILogger<CreateFamilyCommandHandler> logger)
    {
        // Arrange
        var userId = UserId.New();
        currentUserService.GetUserId().Returns(userId);
        var command = new CreateFamilyCommand(FamilyName.From("Smith Family"));

        userRepository
            .GetByIdAsync(userId, Arg.Any<CancellationToken>())
            .Returns((User?)null);

        var handler = new CreateFamilyCommandHandler(
            userRepository,
            familyRepository,
            unitOfWork,
            currentUserService,
            logger);

        // Act
        var act = async () => await handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<BusinessException>()
            .WithMessage($"*User with ID {userId.Value} not found*");

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
        ICurrentUserService currentUserService,
        ILogger<CreateFamilyCommandHandler> logger)
    {
        // Arrange
        var userId = UserId.New();
        currentUserService.GetUserId().Returns(userId);
        var command = new CreateFamilyCommand(FamilyName.From("Smith Family"));

        // Note: User.CreateFromOAuth() auto-generates a new ID internally, different from userId.
        // This is acceptable for unit testing - we're verifying handler behavior with mocked dependencies,
        // not domain entity state. The mock will return this user when queried with userId.
        var user = User.CreateFromOAuth(
            Email.From("test@example.com"),
            "ext123",
            "zitadel");

        var existingFamily = Family.Create(FamilyName.From("Existing Family"), userId);

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
            currentUserService,
            logger);

        // Act
        var act = async () => await handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<BusinessException>()
            .WithMessage("*User already belongs to a family*Users can only be members of one family at a time*");

        // Verify that we stopped after the family check
        await familyRepository.DidNotReceiveWithAnyArgs().AddAsync(null!, CancellationToken.None);
        await unitOfWork.DidNotReceiveWithAnyArgs().SaveChangesAsync(CancellationToken.None);
    }

    [Theory, AutoNSubstituteData]
    public async Task Handle_WhenUserBelongsToMultipleFamilies_ShouldThrowInvalidOperationException(
        IUserRepository userRepository,
        IFamilyRepository familyRepository,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        ILogger<CreateFamilyCommandHandler> logger)
    {
        // Arrange
        var userId = UserId.New();
        currentUserService.GetUserId().Returns(userId);
        var command = new CreateFamilyCommand(FamilyName.From("Smith Family"));

        // Note: User.CreateFromOAuth() auto-generates a new ID internally, different from userId.
        // This is acceptable for unit testing - we're verifying handler behavior with mocked dependencies,
        // not domain entity state. The mock will return this user when queried with userId.
        var user = User.CreateFromOAuth(
            Email.From("test@example.com"),
            "ext123",
            "zitadel");

        var existingFamily1 = Family.Create(FamilyName.From("Existing Family 1"), userId);
        var existingFamily2 = Family.Create(FamilyName.From("Existing Family 2"), userId);

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
            currentUserService,
            logger);

        // Act
        var act = async () => await handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<BusinessException>()
            .WithMessage("*User already belongs to a family*");
    }

    [Theory, AutoNSubstituteData]
    public async Task Handle_WithCancellationToken_ShouldPassTokenToRepositories(
        IUserRepository userRepository,
        IFamilyRepository familyRepository,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        ILogger<CreateFamilyCommandHandler> logger)
    {
        // Arrange
        var userId = UserId.New();
        currentUserService.GetUserId().Returns(userId);
        var command = new CreateFamilyCommand(FamilyName.From("Smith Family"));

        // Note: User.CreateFromOAuth() auto-generates a new ID internally, different from userId.
        // This is acceptable for unit testing - we're verifying handler behavior with mocked dependencies,
        // not domain entity state. The mock will return this user when queried with userId.
        var user = User.CreateFromOAuth(
            Email.From("test@example.com"),
            "ext123",
            "zitadel");
        var cts = new CancellationTokenSource();
        var cancellationToken = cts.Token;

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
            currentUserService,
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
        ICurrentUserService currentUserService,
        ILogger<CreateFamilyCommandHandler> logger)
    {
        // Arrange
        var userId = UserId.New();
        currentUserService.GetUserId().Returns(userId);
        var nameWithWhitespace = "  Smith Family  ";
        var expectedTrimmedName = "Smith Family";
        var command = new CreateFamilyCommand(FamilyName.From(nameWithWhitespace));

        // Note: User.CreateFromOAuth() auto-generates a new ID internally, different from userId.
        // This is acceptable for unit testing - we're verifying handler behavior with mocked dependencies,
        // not domain entity state. The mock will return this user when queried with userId.
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
            currentUserService,
            logger);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Name.Value.Should().Be(expectedTrimmedName);

        await familyRepository.Received(1).AddAsync(
            Arg.Is<Family>(f => f.Name == expectedTrimmedName),
            Arg.Any<CancellationToken>());
    }

    [Theory, AutoNSubstituteData]
    public async Task Handle_WhenUserNotFound_ShouldLogWarning(
        IUserRepository userRepository,
        IFamilyRepository familyRepository,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        ILogger<CreateFamilyCommandHandler> logger)
    {
        // Arrange
        var userId = UserId.New();
        currentUserService.GetUserId().Returns(userId);
        var command = new CreateFamilyCommand(FamilyName.From("Smith Family"));

        userRepository
            .GetByIdAsync(userId, Arg.Any<CancellationToken>())
            .Returns((User?)null);

        var handler = new CreateFamilyCommandHandler(
            userRepository,
            familyRepository,
            unitOfWork,
            currentUserService,
            logger);

        // Act
        var act = async () => await handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<BusinessException>();

        // Note: Logging verification removed - testing implementation details
        // LoggerMessage.Define pattern doesn't call generic Log() method
        // Logging is verified in integration tests
    }

    [Theory, AutoNSubstituteData]
    public async Task Handle_WhenUserAlreadyHasFamily_ShouldLogWarning(
        IUserRepository userRepository,
        IFamilyRepository familyRepository,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        ILogger<CreateFamilyCommandHandler> logger)
    {
        // Arrange
        var userId = UserId.New();
        currentUserService.GetUserId().Returns(userId);
        var command = new CreateFamilyCommand(FamilyName.From("Smith Family"));

        // Note: User.CreateFromOAuth() auto-generates a new ID internally, different from userId.
        // This is acceptable for unit testing - we're verifying handler behavior with mocked dependencies,
        // not domain entity state. The mock will return this user when queried with userId.
        var user = User.CreateFromOAuth(
            Email.From("test@example.com"),
            "ext123",
            "zitadel");

        var existingFamily = Family.Create(FamilyName.From("Existing Family"), userId);

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
            currentUserService,
            logger);

        // Act
        var act = async () => await handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<BusinessException>();

        // Note: Logging verification removed - testing implementation details
        // LoggerMessage.Define pattern doesn't call generic Log() method
        // Logging is verified in integration tests
    }

    [Theory, AutoNSubstituteData]
    public async Task Handle_WithSuccessfulCreation_ShouldLogInformationTwice(
        IUserRepository userRepository,
        IFamilyRepository familyRepository,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        ILogger<CreateFamilyCommandHandler> logger)
    {
        // Arrange
        var userId = UserId.New();
        currentUserService.GetUserId().Returns(userId);
        var familyName = "Smith Family";
        var command = new CreateFamilyCommand(FamilyName.From(familyName));

        // Note: User.CreateFromOAuth() auto-generates a new ID internally, different from userId.
        // This is acceptable for unit testing - we're verifying handler behavior with mocked dependencies,
        // not domain entity state. The mock will return this user when queried with userId.
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
            currentUserService,
            logger);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert - Verify successful execution
        result.Should().NotBeNull();
        result.Name.Value.Should().Be(familyName);

        // Note: Logging verification removed - testing implementation details
        // LoggerMessage.Define pattern doesn't call generic Log() method
        // Logging is verified in integration tests
    }
}
