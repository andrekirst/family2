using FamilyHub.Modules.Auth.Application.Commands.CreateFamily;
using FamilyHub.Modules.Auth.Domain.Repositories;
using FamilyHub.Modules.Auth.Domain.ValueObjects;
using FamilyHub.SharedKernel.Domain.Exceptions;
using FamilyHub.SharedKernel.Domain.ValueObjects;
using FamilyHub.Tests.Integration.Helpers;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace FamilyHub.Tests.Integration.Auth;

/// <summary>
/// Integration tests for Create Family feature.
/// Tests end-to-end database operations, transaction behavior, and concurrent operations.
/// </summary>
public sealed class CreateFamilyIntegrationTests(TestApplicationFactory factory) : IClassFixture<TestApplicationFactory>
{
    #region Helper Methods

    /// <summary>
    /// Asserts that a family was created correctly with all expected properties.
    /// Verifies family entity properties and owner membership relationship.
    /// </summary>
    private static async Task AssertFamilyCreatedCorrectlyAsync(
        IFamilyRepository familyRepository,
        FamilyId familyId,
        string expectedName,
        UserId expectedOwnerId)
    {
        var createdFamily = await familyRepository.GetByIdAsync(familyId);

        // Family assertions
        createdFamily.Should().NotBeNull();
        createdFamily.Name.Value.Should().Be(expectedName);
        createdFamily.OwnerId.Should().Be(expectedOwnerId);
        createdFamily.DeletedAt.Should().BeNull();

        // Membership assertions
        createdFamily.UserFamilies.Should().HaveCount(1);
        var membership = createdFamily.UserFamilies.First();
        membership.UserId.Should().Be(expectedOwnerId);
        membership.FamilyId.Should().Be(familyId);
        membership.Role.Should().Be(UserRole.Owner);
        membership.IsActive.Should().BeTrue();
        membership.InvitedBy.Should().BeNull();
    }

    #endregion

    [Fact]
    public async Task CreateFamily_WithValidInput_CreatesRecordsInDatabase()
    {
        // Arrange
        using var scope = factory.Services.CreateScope();
        var (mediator, userRepo, familyRepo, unitOfWork) = TestServices.ResolveCommandServices(scope);

        var user = await TestDataFactory.CreateUserAsync(userRepo, unitOfWork);
        TestCurrentUserService.SetUserId(user.Id);

        var testId = TestDataFactory.GenerateTestId();
        var familyName = $"Test Family {testId}";
        var command = new CreateFamilyCommand(FamilyName.From(familyName));

        // Act
        var result = await mediator.Send(command);

        // Assert
        result.Should().NotBeNull();
        result.FamilyId.Value.Should().NotBe(Guid.Empty);
        result.Name.Value.Should().Be(familyName);
        result.OwnerId.Should().Be(user.Id);
        result.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));

        await AssertFamilyCreatedCorrectlyAsync(familyRepo, result.FamilyId, familyName, user.Id);
    }

    [Fact]
    public async Task CreateFamily_WithSoftDeletedFamily_AllowsNewCreation()
    {
        // Arrange
        using var scope = factory.Services.CreateScope();
        var (mediator, userRepo, familyRepo, unitOfWork) = TestServices.ResolveCommandServices(scope);

        var user = await TestDataFactory.CreateUserAsync(userRepo, unitOfWork, "softdelete");
        TestCurrentUserService.SetUserId(user.Id);
        var testId = TestDataFactory.GenerateTestId();

        // Create first family
        var firstFamilyName = $"First Family {testId}";
        var firstCommand = new CreateFamilyCommand(FamilyName.From(firstFamilyName));
        var firstResult = await mediator.Send(firstCommand);

        // Soft delete the first family
        var firstFamily = await familyRepo.GetByIdAsync(firstResult.FamilyId);
        firstFamily.Should().NotBeNull();
        firstFamily.Delete();
        await unitOfWork.SaveChangesAsync();

        // Act - Create a new family after soft delete
        var newFamilyName = $"New Family After Delete {testId}";
        var newCommand = new CreateFamilyCommand(FamilyName.From(newFamilyName));
        var newResult = await mediator.Send(newCommand);

        // Assert
        newResult.Should().NotBeNull();
        newResult.FamilyId.Should().NotBe(firstResult.FamilyId);
        newResult.Name.Value.Should().Be(newFamilyName);

        // Verify new family exists
        var newFamily = await familyRepo.GetByIdAsync(newResult.FamilyId);
        newFamily.Should().NotBeNull();
        newFamily.Name.Value.Should().Be(newFamilyName);
        newFamily.DeletedAt.Should().BeNull();
    }

    [Fact(Skip = "Concurrent requests test requires real PostgreSQL database. In-memory DB has EF Core navigation property issues with Vogen value objects.")]
    public async Task CreateFamily_ConcurrentRequests_OnlyOneSucceeds()
    {
        // Note: This test verifies business rule enforcement (one family per user) under concurrent load.
        // With in-memory database, EF Core has issues materializing navigation properties with Vogen value objects.
        // In production with PostgreSQL, unique constraints and proper transaction isolation would enforce this.
        //
        // To enable this test:
        // 1. Configure integration tests to use a real PostgreSQL test database
        // 2. Add database cleanup/reset between test runs
        // 3. Configure proper transaction isolation levels
        //
        // The business logic is already tested in CreateFamily_WhenUserAlreadyHasFamily_ThrowsException

        await Task.CompletedTask;
    }

    [Fact]
    public async Task CreateFamily_WithNonExistentUser_ThrowsException()
    {
        // Arrange
        using var scope = factory.Services.CreateScope();
        var (mediator, _, _, _) = TestServices.ResolveCommandServices(scope);

        var nonExistentUserId = UserId.New();
        TestCurrentUserService.SetUserId(nonExistentUserId);

        var command = new CreateFamilyCommand(FamilyName.From("Test Family"));

        // Act
        var act = async () => await mediator.Send(command);

        // Assert
        await act.Should().ThrowAsync<BusinessException>()
            .WithMessage($"*User with ID {nonExistentUserId.Value} not found*");
    }

    [Fact]
    public async Task CreateFamily_WithEmptyName_ThrowsValidationException()
    {
        // Arrange
        using var scope = factory.Services.CreateScope();
        var (_, userRepo, _, unitOfWork) = TestServices.ResolveCommandServices(scope);

        await TestDataFactory.CreateUserAsync(userRepo, unitOfWork, "emptyname");

        // Act - FamilyName.From() validates at value object level (before reaching MediatR)
        var act = () => FamilyName.From(string.Empty);

        // Assert
        act.Should().Throw<Vogen.ValueObjectValidationException>()
            .WithMessage("*Family name cannot be empty*");
    }

    [Fact]
    public async Task CreateFamily_WithNameTooLong_ThrowsValidationException()
    {
        // Arrange
        using var scope = factory.Services.CreateScope();
        var (_, userRepo, _, unitOfWork) = TestServices.ResolveCommandServices(scope);

        await TestDataFactory.CreateUserAsync(userRepo, unitOfWork, "longname");
        var longName = new string('A', 101); // Exceeds 100 character limit

        // Act - FamilyName.From() validates at value object level (before reaching MediatR)
        var act = () => FamilyName.From(longName);

        // Assert
        act.Should().Throw<Vogen.ValueObjectValidationException>()
            .WithMessage("*100 characters*");
    }

    [Fact]
    public async Task CreateFamily_WhenUserAlreadyHasFamily_ThrowsException()
    {
        // Arrange
        using var scope = factory.Services.CreateScope();
        var (mediator, userRepo, _, unitOfWork) = TestServices.ResolveCommandServices(scope);

        var user = await TestDataFactory.CreateUserAsync(userRepo, unitOfWork, "hasfamily");
        TestCurrentUserService.SetUserId(user.Id);
        var testId = TestDataFactory.GenerateTestId();

        // Create first family
        var firstCommand = new CreateFamilyCommand(FamilyName.From($"First Family {testId}"));
        await mediator.Send(firstCommand);

        // Act - Try to create a second family
        var secondCommand = new CreateFamilyCommand(FamilyName.From($"Second Family {testId}"));
        var act = async () => await mediator.Send(secondCommand);

        // Assert
        await act.Should().ThrowAsync<BusinessException>()
            .WithMessage("*User already belongs to a family*");
    }
}
