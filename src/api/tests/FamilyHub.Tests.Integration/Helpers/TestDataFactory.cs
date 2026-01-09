using FamilyHub.Modules.Auth.Application.Commands.CreateFamily;
using FamilyHub.Modules.Auth.Domain;
using FamilyHub.Modules.Auth.Domain.Repositories;
using FamilyHub.Modules.Family.Application.Abstractions;
using FamilyHub.Modules.Family.Domain.Aggregates;
using FamilyHub.Modules.Family.Domain.ValueObjects;
using FamilyHub.SharedKernel.Domain.ValueObjects;
using FamilyHub.SharedKernel.Interfaces;
using MediatR;
using FamilyAggregate = FamilyHub.Modules.Family.Domain.Aggregates.Family;

namespace FamilyHub.Tests.Integration.Helpers;

/// <summary>
/// Factory for creating test data entities (users, families) with proper persistence.
/// Eliminates code duplication across test classes.
/// </summary>
public static class TestDataFactory
{
    /// <summary>
    /// Creates a test user with unique email and external ID.
    /// Automatically persists to database via unit of work.
    /// </summary>
    /// <param name="userRepository">The user repository to add the user to.</param>
    /// <param name="unitOfWork">The unit of work to save changes.</param>
    /// <param name="emailPrefix">The prefix for the test user's email (default: "test").</param>
    /// <returns>The created and persisted user entity.</returns>
    /// <example>
    /// var user = await TestDataFactory.CreateUserAsync(userRepo, unitOfWork, "oauth");
    /// // user.Email => "oauth-a1b2c3d4@example.com"
    /// </example>
    public static async Task<User> CreateUserAsync(
        IUserRepository userRepository,
        IFamilyService familyService,
        IUnitOfWork unitOfWork,
        string emailPrefix = "test")
    {
        var testId = Guid.NewGuid().ToString("N")[..8];
        var email = $"{emailPrefix}-{testId}@example.com";
        var familyName = $"{emailPrefix} Family {testId}";

        // WORKAROUND for foreign key constraint:
        // Create temporary dummy family first, then user, then actual family
        // This satisfies the FK constraint users.family_id -> families.id
        var tempFamilyResult = await familyService.CreateFamilyAsync(
            FamilyName.From($"Temp Family {testId}"),
            UserId.New(), // Temp owner that doesn't exist
            CancellationToken.None);

        if (tempFamilyResult.IsFailure)
        {
            throw new InvalidOperationException($"Failed to create temp family: {tempFamilyResult.Error}");
        }

        var tempFamilyDto = tempFamilyResult.Value;

        // Create user with temp family ID
        var user = User.CreateFromOAuth(
            Email.From(email),
            $"zitadel-{emailPrefix}-{testId}",
            "zitadel",
            tempFamilyDto.Id
        );

        // Persist user
        await userRepository.AddAsync(user);
        await unitOfWork.SaveChangesAsync();

        // Now create actual family with user as owner
        var createResult = await familyService.CreateFamilyAsync(
            FamilyName.From(familyName),
            user.Id,
            CancellationToken.None);

        if (createResult.IsFailure)
        {
            throw new InvalidOperationException($"Failed to create family: {createResult.Error}");
        }

        var familyDto = createResult.Value;

        // Update user's family ID to actual family
        user.UpdateFamily(familyDto.Id);
        await unitOfWork.SaveChangesAsync();

        return user;
    }

    /// <summary>
    /// Creates a test family owned by the given user using the CreateFamilyCommand handler.
    /// Automatically persists to database via the command handler's unit of work.
    /// </summary>
    /// <param name="mediator">The MediatR mediator to send commands.</param>
    /// <param name="familyRepository">The family repository to retrieve the created family.</param>
    /// <param name="ownerId">The ID of the user who will own the family.</param>
    /// <param name="familyName">The name of the family (default: auto-generated).</param>
    /// <returns>The created and persisted family entity.</returns>
    /// <example>
    /// var family = await TestDataFactory.CreateFamilyAsync(mediator, familyRepo, user.Id);
    /// // family.Name => "Test Family a1b2c3d4"
    /// </example>
    public static async Task<FamilyAggregate> CreateFamilyAsync(
        IMediator mediator,
        IFamilyService familyService,
        UserId ownerId,
        string? familyName = null)
    {
        familyName ??= $"Test Family {Guid.NewGuid().ToString("N")[..8]}";

        // Set the authenticated user for the command
        TestCurrentUserService.SetUserId(ownerId);

        // Use the command handler to ensure proper family creation with membership
        var command = new CreateFamilyCommand(FamilyName.From(familyName));
        var result = await mediator.Send(command);

        // Retrieve the created family from service
        var familyDto = await familyService.GetFamilyByIdAsync(result.FamilyId, CancellationToken.None) ?? throw new InvalidOperationException($"Family {result.FamilyId.Value} was created but could not be retrieved.");

        // Reconstitute aggregate for test usage
        return FamilyAggregate.Reconstitute(
            familyDto.Id,
            familyDto.Name,
            familyDto.OwnerId,
            familyDto.CreatedAt,
            familyDto.UpdatedAt);
    }

    /// <summary>
    /// Creates a user and their owned family in a single operation.
    /// </summary>
    /// <example>
    /// var (user, family) = await TestDataFactory.CreateUserWithFamilyAsync(
    ///     mediator, userRepo, familyRepo, unitOfWork, emailPrefix: "owner");
    /// </example>
    public static async Task<(User user, FamilyAggregate family)> CreateUserWithFamilyAsync(
        IMediator mediator,
        IUserRepository userRepository,
        IFamilyService familyService,
        IUnitOfWork unitOfWork,
        string emailPrefix = "test",
        string? familyName = null)
    {
        var user = await CreateUserAsync(userRepository, familyService, unitOfWork, emailPrefix);
        var family = await CreateFamilyAsync(mediator, familyService, user.Id, familyName);
        return (user, family);
    }

    /// <summary>
    /// Generates a unique test ID (8-character hex string).
    /// Useful for creating unique names in tests.
    /// </summary>
    /// <example>
    /// var testId = TestDataFactory.GenerateTestId();
    /// var familyName = $"Family {testId}";
    /// </example>
    public static string GenerateTestId() => Guid.NewGuid().ToString("N")[..8];
}
