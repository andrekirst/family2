using FamilyHub.Modules.Auth.Domain;
using FamilyHub.Modules.Auth.Domain.Repositories;
using FamilyHub.Modules.Auth.Application.Abstractions;
using FamilyHub.Modules.Auth.Application.Commands.CreateFamily;
using FamilyHub.SharedKernel.Domain.ValueObjects;
using MediatR;

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
        IFamilyRepository familyRepository,
        IUnitOfWork unitOfWork,
        string emailPrefix = "test")
    {
        var testId = Guid.NewGuid().ToString("N")[..8];
        var email = $"{emailPrefix}-{testId}@example.com";
        var familyName = $"{emailPrefix} Family {testId}";
        
        // Create family first (required by foreign key constraint)
        var family = Family.Create(FamilyName.From(familyName), UserId.New()); // Temp owner
        await familyRepository.AddAsync(family);
        await unitOfWork.SaveChangesAsync();
        
        // Create user with family ID
        var user = User.CreateFromOAuth(
            Email.From(email),
            $"zitadel-{emailPrefix}-{testId}",
            "zitadel",
            family.Id
        );
        
        // Transfer ownership to user
        family.TransferOwnership(user.Id);
        
        await userRepository.AddAsync(user);
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
    public static async Task<Family> CreateFamilyAsync(
        IMediator mediator,
        IFamilyRepository familyRepository,
        UserId ownerId,
        string? familyName = null)
    {
        familyName ??= $"Test Family {Guid.NewGuid().ToString("N")[..8]}";

        // Set the authenticated user for the command
        TestCurrentUserService.SetUserId(ownerId);

        // Use the command handler to ensure proper family creation with membership
        var command = new CreateFamilyCommand(FamilyName.From(familyName));
        var result = await mediator.Send(command);

        // Retrieve the created family from repository
        var family = await familyRepository.GetByIdAsync(result.FamilyId);
        if (family == null)
        {
            throw new InvalidOperationException($"Family {result.FamilyId.Value} was created but could not be retrieved.");
        }

        return family;
    }

    /// <summary>
    /// Creates a user and their owned family in a single operation.
    /// </summary>
    /// <example>
    /// var (user, family) = await TestDataFactory.CreateUserWithFamilyAsync(
    ///     mediator, userRepo, familyRepo, unitOfWork, emailPrefix: "owner");
    /// </example>
    public static async Task<(User user, Family family)> CreateUserWithFamilyAsync(
        IMediator mediator,
        IUserRepository userRepository,
        IFamilyRepository familyRepository,
        IUnitOfWork unitOfWork,
        string emailPrefix = "test",
        string? familyName = null)
    {
        var user = await CreateUserAsync(userRepository, familyRepository, unitOfWork, emailPrefix);
        var family = await CreateFamilyAsync(mediator, familyRepository, user.Id, familyName);
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
