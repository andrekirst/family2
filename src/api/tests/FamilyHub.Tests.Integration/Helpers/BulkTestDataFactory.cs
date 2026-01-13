using FamilyHub.Modules.Auth.Domain;
using FamilyHub.Modules.Auth.Persistence;
using FamilyHub.Modules.Family.Domain.Aggregates;
using FamilyHub.Modules.Family.Persistence;
using FamilyHub.SharedKernel.Domain.ValueObjects;
using FamilyAggregate = FamilyHub.Modules.Family.Domain.Aggregates.Family;

namespace FamilyHub.Tests.Integration.Helpers;

/// <summary>
/// Factory for bulk-creating test data for DataLoader integration tests.
/// Optimized for efficient batch inserts without going through application layer.
/// </summary>
/// <remarks>
/// This factory creates data directly via DbContext to:
/// 1. Avoid overhead of service/mediator layers
/// 2. Enable precise control over test data structure
/// 3. Support bulk operations for performance testing
/// </remarks>
public static class BulkTestDataFactory
{
    /// <summary>
    /// Creates multiple families with specified number of users each.
    /// Returns mapping of FamilyId to list of Users.
    /// </summary>
    /// <param name="authContext">Auth DbContext for user persistence.</param>
    /// <param name="familyContext">Family DbContext for family persistence.</param>
    /// <param name="familyCount">Number of families to create.</param>
    /// <param name="usersPerFamily">Number of users per family.</param>
    /// <returns>Dictionary mapping FamilyId to list of created users.</returns>
    public static async Task<Dictionary<FamilyId, List<User>>> CreateFamiliesWithUsersAsync(
        AuthDbContext authContext,
        FamilyDbContext familyContext,
        int familyCount,
        int usersPerFamily)
    {
        var result = new Dictionary<FamilyId, List<User>>();
        var allUsers = new List<User>();
        var allFamilies = new List<FamilyAggregate>();

        for (var i = 0; i < familyCount; i++)
        {
            var ownerId = UserId.New();
            var family = FamilyAggregate.Create(
                FamilyName.From($"Test Family {i + 1}"),
                ownerId);

            allFamilies.Add(family);

            var familyUsers = new List<User>();
            for (var j = 0; j < usersPerFamily; j++)
            {
                var user = User.CreateFromOAuth(
                    Email.From($"user{i}-{j}-{Guid.NewGuid():N}@test.example.com"),
                    $"zitadel-{Guid.NewGuid():N}",
                    "zitadel",
                    family.Id);

                familyUsers.Add(user);
                allUsers.Add(user);
            }

            result[family.Id] = familyUsers;
        }

        // Batch insert families first (FK target)
        await familyContext.Families.AddRangeAsync(allFamilies);
        await familyContext.SaveChangesAsync();
        familyContext.ChangeTracker.Clear();

        // Then batch insert users (FK source)
        await authContext.Users.AddRangeAsync(allUsers);
        await authContext.SaveChangesAsync();
        authContext.ChangeTracker.Clear();

        return result;
    }

    /// <summary>
    /// Creates a single family with specified number of members.
    /// </summary>
    /// <param name="authContext">Auth DbContext for user persistence.</param>
    /// <param name="familyContext">Family DbContext for family persistence.</param>
    /// <param name="memberCount">Number of members in the family.</param>
    /// <returns>Tuple of created family and list of member users.</returns>
    public static async Task<(FamilyAggregate family, List<User> members)> CreateFamilyWithMembersAsync(
        AuthDbContext authContext,
        FamilyDbContext familyContext,
        int memberCount)
    {
        var ownerId = UserId.New();
        var family = FamilyAggregate.Create(
            FamilyName.From($"Large Family {Guid.NewGuid():N}"),
            ownerId);

        var members = new List<User>();
        for (var i = 0; i < memberCount; i++)
        {
            var user = User.CreateFromOAuth(
                Email.From($"member{i}-{Guid.NewGuid():N}@test.example.com"),
                $"zitadel-{Guid.NewGuid():N}",
                "zitadel",
                family.Id);
            members.Add(user);
        }

        // Insert family first
        await familyContext.Families.AddAsync(family);
        await familyContext.SaveChangesAsync();
        familyContext.ChangeTracker.Clear();

        // Then batch insert users
        await authContext.Users.AddRangeAsync(members);
        await authContext.SaveChangesAsync();
        authContext.ChangeTracker.Clear();

        return (family, members);
    }

    /// <summary>
    /// Creates invitations for a family.
    /// </summary>
    /// <param name="familyContext">Family DbContext for invitation persistence.</param>
    /// <param name="familyId">ID of the family to invite to.</param>
    /// <param name="invitedByUserId">ID of the user sending invitations.</param>
    /// <param name="invitationCount">Number of invitations to create.</param>
    /// <returns>List of created invitations.</returns>
    public static async Task<List<FamilyMemberInvitation>> CreateInvitationsAsync(
        FamilyDbContext familyContext,
        FamilyId familyId,
        UserId invitedByUserId,
        int invitationCount)
    {
        var invitations = new List<FamilyMemberInvitation>();

        for (var i = 0; i < invitationCount; i++)
        {
            var invitation = FamilyMemberInvitation.CreateEmailInvitation(
                familyId,
                Email.From($"invitee{i}-{Guid.NewGuid():N}@test.example.com"),
                FamilyRole.Member,
                invitedByUserId,
                $"Welcome message {i}");

            // Clear domain events to avoid side effects in tests
            invitation.ClearDomainEvents();
            invitations.Add(invitation);
        }

        await familyContext.FamilyMemberInvitations.AddRangeAsync(invitations);
        await familyContext.SaveChangesAsync();
        familyContext.ChangeTracker.Clear();

        return invitations;
    }

    /// <summary>
    /// Creates specified number of users with unique families (1 user per family).
    /// Useful for testing FamilyBatchDataLoader with many distinct families.
    /// </summary>
    /// <param name="authContext">Auth DbContext for user persistence.</param>
    /// <param name="familyContext">Family DbContext for family persistence.</param>
    /// <param name="count">Number of user/family pairs to create.</param>
    /// <returns>List of tuples containing user and their family.</returns>
    public static async Task<List<(User user, FamilyAggregate family)>> CreateUsersWithFamiliesAsync(
        AuthDbContext authContext,
        FamilyDbContext familyContext,
        int count)
    {
        var result = new List<(User, FamilyAggregate)>();
        var allUsers = new List<User>();
        var allFamilies = new List<FamilyAggregate>();

        for (var i = 0; i < count; i++)
        {
            var userId = UserId.New();
            var family = FamilyAggregate.Create(
                FamilyName.From($"User {i}'s Family"),
                userId);

            var user = User.CreateFromOAuth(
                Email.From($"user{i}-{Guid.NewGuid():N}@test.example.com"),
                $"zitadel-{Guid.NewGuid():N}",
                "zitadel",
                family.Id);

            allUsers.Add(user);
            allFamilies.Add(family);
            result.Add((user, family));
        }

        // Batch insert families first (FK target)
        await familyContext.Families.AddRangeAsync(allFamilies);
        await familyContext.SaveChangesAsync();
        familyContext.ChangeTracker.Clear();

        // Then batch insert users (FK source)
        await authContext.Users.AddRangeAsync(allUsers);
        await authContext.SaveChangesAsync();
        authContext.ChangeTracker.Clear();

        return result;
    }
}
