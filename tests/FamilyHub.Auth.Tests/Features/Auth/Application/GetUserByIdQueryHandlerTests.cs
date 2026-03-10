using FamilyHub.Common.Domain.ValueObjects;
using FamilyHub.Api.Features.Auth.Domain.ValueObjects;
using FamilyHub.Api.Features.Auth.Application.Queries.GetUserById;
using FamilyHub.Api.Features.Auth.Domain.Entities;
using FamilyHub.Api.Features.Auth.Domain.Repositories;
using FluentAssertions;
using NSubstitute;

namespace FamilyHub.Auth.Tests.Features.Auth.Application;

public class GetUserByIdQueryHandlerTests
{
    [Fact]
    public async Task Handle_ShouldReturnUser_WhenUserExistsInSameFamily()
    {
        // Arrange
        var familyId = FamilyId.New();
        var user = User.Register(
            Email.From("target@example.com"),
            UserName.From("Target User"),
            ExternalUserId.From("ext-target"),
            emailVerified: true, utcNow: DateTimeOffset.UtcNow);
        user.AssignToFamily(familyId, DateTimeOffset.UtcNow);

        var userRepo = Substitute.For<IUserRepository>();
        userRepo.GetByIdAsync(user.Id, Arg.Any<CancellationToken>())
            .Returns(user);

        var handler = new GetUserByIdQueryHandler(userRepo);
        var query = new GetUserByIdQuery(user.Id) { UserId = UserId.New(), FamilyId = familyId };

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Email.Should().Be("target@example.com");
    }

    [Fact]
    public async Task Handle_ShouldReturnNull_WhenUserExistsInDifferentFamily()
    {
        // Arrange
        var userFamilyId = FamilyId.New();
        var callerFamilyId = FamilyId.New();
        var user = User.Register(
            Email.From("other@example.com"),
            UserName.From("Other User"),
            ExternalUserId.From("ext-other"),
            emailVerified: true, utcNow: DateTimeOffset.UtcNow);
        user.AssignToFamily(userFamilyId, DateTimeOffset.UtcNow);

        var userRepo = Substitute.For<IUserRepository>();
        userRepo.GetByIdAsync(user.Id, Arg.Any<CancellationToken>())
            .Returns(user);

        var handler = new GetUserByIdQueryHandler(userRepo);
        var query = new GetUserByIdQuery(user.Id) { UserId = UserId.New(), FamilyId = callerFamilyId };

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task Handle_ShouldReturnNull_WhenUserNotFound()
    {
        // Arrange
        var userRepo = Substitute.For<IUserRepository>();
        // Use ReturnsForAnyArgs to avoid Vogen VOG009 with Arg.Any<UserId>()
        userRepo.GetByIdAsync(UserId.New(), CancellationToken.None)
            .ReturnsForAnyArgs((User?)null);

        var handler = new GetUserByIdQueryHandler(userRepo);
        var query = new GetUserByIdQuery(UserId.New()) { UserId = UserId.New(), FamilyId = FamilyId.New() };

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().BeNull();
    }
}
