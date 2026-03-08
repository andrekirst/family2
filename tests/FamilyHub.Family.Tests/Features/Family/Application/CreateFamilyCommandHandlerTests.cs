using FamilyHub.Common.Domain.ValueObjects;
using FamilyHub.Api.Features.Auth.Domain.ValueObjects;
using FamilyHub.Api.Features.Auth.Domain.Entities;
using FamilyHub.Api.Features.Family.Application.Commands.CreateFamily;
using FamilyHub.Api.Features.Family.Domain.ValueObjects;
using FamilyHub.TestCommon.Fakes;
using FluentAssertions;
using FamilyEntity = FamilyHub.Api.Features.Family.Domain.Entities.Family;

namespace FamilyHub.Family.Tests.Features.Family.Application;

public class CreateFamilyCommandHandlerTests
{
    [Fact]
    public async Task Handle_ShouldCreateFamilyAndReturnResult()
    {
        // Arrange
        var user = CreateTestUser();
        var (handler, _, _, _) = CreateHandler(user);
        var command = new CreateFamilyCommand(FamilyName.From("Smith Family")) { UserId = user.Id };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.FamilyId.Value.Should().NotBe(Guid.Empty);
        result.CreatedFamily.Should().NotBeNull();
        result.CreatedFamily.Name.Should().Be(command.Name);
    }

    [Fact]
    public async Task Handle_ShouldAssignUserToFamily()
    {
        // Arrange
        var user = CreateTestUser();
        var (handler, _, _, _) = CreateHandler(user);
        var command = new CreateFamilyCommand(FamilyName.From("Smith Family")) { UserId = user.Id };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        user.FamilyId.Should().NotBeNull();
        user.FamilyId.Should().Be(result.FamilyId);
    }

    [Fact]
    public async Task Handle_ShouldAddFamilyToRepository()
    {
        // Arrange
        var user = CreateTestUser();
        var (handler, familyRepo, _, _) = CreateHandler(user);
        var command = new CreateFamilyCommand(FamilyName.From("Smith Family")) { UserId = user.Id };

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        familyRepo.AddedFamilies.Should().HaveCount(1);
        familyRepo.AddedFamilies[0].Name.Should().Be(command.Name);
        familyRepo.AddedFamilies[0].OwnerId.Should().Be(user.Id);
    }

    // --- Helpers ---

    private static User CreateTestUser()
    {
        var email = Email.From("test@example.com");
        var name = UserName.From("Test User");
        var externalId = ExternalUserId.From("test-external-id");

        var user = User.Register(email, name, externalId, emailVerified: true);
        user.ClearDomainEvents();

        return user;
    }

    private static (CreateFamilyCommandHandler Handler, FakeFamilyRepository FamilyRepo, FakeUserRepository UserRepo, FakeFamilyMemberRepository MemberRepo) CreateHandler(
        User? user,
        FamilyEntity? existingFamilyForOwner = null)
    {
        var userRepo = new FakeUserRepository(user);
        var familyRepo = new FakeFamilyRepository(existingFamilyForOwner: existingFamilyForOwner);
        var memberRepo = new FakeFamilyMemberRepository();
        var handler = new CreateFamilyCommandHandler(familyRepo, userRepo, memberRepo);
        return (handler, familyRepo, userRepo, memberRepo);
    }
}
