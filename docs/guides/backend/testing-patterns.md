# Testing Patterns

**Purpose:** Guide for writing unit and integration tests in Family Hub.

**Stack:** xUnit + FluentAssertions. Fake repository pattern (no Moq). NSubstitute migration planned.

**Test location:** `tests/FamilyHub.UnitTests/Features/{Module}/`

---

## Test Project Structure

```
tests/FamilyHub.UnitTests/
  Features/
    Auth/
      Application/
        GetCurrentUserQueryHandlerTests.cs
      Domain/
        UserAggregateTests.cs
    Family/
      Application/
        CreateFamilyCommandHandlerTests.cs
        SendInvitationCommandHandlerTests.cs
        AcceptInvitationCommandHandlerTests.cs
        AcceptInvitationByIdCommandHandlerTests.cs
      Domain/
        FamilyAggregateTests.cs
        FamilyInvitationTests.cs
        FamilyMemberTests.cs
        FamilyRoleTests.cs
```

---

## Fake Repository Pattern

Instead of mocking frameworks, Family Hub uses **inner classes** that implement repository interfaces with in-memory state. This provides type-safe fakes that are easy to reason about.

```csharp
// Inside a test class
private class FakeFamilyMemberRepository : IFamilyMemberRepository
{
    private readonly FamilyMember? _existingMember;

    public FakeFamilyMemberRepository(FamilyMember? existingMember = null)
    {
        _existingMember = existingMember;
    }

    public Task<FamilyMember?> GetByUserAndFamilyAsync(
        UserId userId, FamilyId familyId, CancellationToken ct = default) =>
        Task.FromResult(_existingMember);

    public Task<List<FamilyMember>> GetByFamilyIdAsync(
        FamilyId familyId, CancellationToken ct = default) =>
        Task.FromResult(_existingMember is not null
            ? [_existingMember]
            : new List<FamilyMember>());

    public Task AddAsync(FamilyMember member, CancellationToken ct = default) =>
        Task.CompletedTask;

    public Task<int> SaveChangesAsync(CancellationToken ct = default) =>
        Task.FromResult(1);
}
```

For repositories that need to track calls (assert that `Add` was called, inspect saved entities), add public state:

```csharp
private class FakeFamilyInvitationRepository : IFamilyInvitationRepository
{
    private readonly FamilyInvitation? _existingByEmail;
    private readonly FamilyInvitation? _existingByTokenHash;

    // State for assertions
    public List<FamilyInvitation> AddedInvitations { get; } = [];
    public bool SaveChangesCalled { get; private set; }

    public FakeFamilyInvitationRepository(
        FamilyInvitation? existingByEmail = null,
        FamilyInvitation? existingByTokenHash = null)
    {
        _existingByEmail = existingByEmail;
        _existingByTokenHash = existingByTokenHash;
    }

    public Task AddAsync(FamilyInvitation invitation, CancellationToken ct = default)
    {
        AddedInvitations.Add(invitation);
        return Task.CompletedTask;
    }

    public Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        SaveChangesCalled = true;
        return Task.FromResult(1);
    }

    // ... other interface methods return defaults
}
```

---

## Testing Static Handlers

Because handlers are static classes, you pass fakes directly as method parameters. No DI container needed.

```csharp
public class SendInvitationCommandHandlerTests
{
    [Fact]
    public async Task Handle_ShouldCreateInvitationAndReturnResult()
    {
        // Arrange
        var familyId = FamilyId.New();
        var inviterId = UserId.New();
        var inviterMember = FamilyMember.Create(familyId, inviterId, FamilyRole.Owner);
        var memberRepo = new FakeFamilyMemberRepository(existingMember: inviterMember);
        var invitationRepo = new FakeFamilyInvitationRepository();
        var authService = new FamilyAuthorizationService(memberRepo);
        var command = new SendInvitationCommand(
            familyId, inviterId,
            Email.From("newmember@example.com"),
            FamilyRole.Member);

        // Act
        var result = await SendInvitationCommandHandler.Handle(
            command, authService, invitationRepo, memberRepo, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.InvitationId.Value.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public async Task Handle_ShouldThrow_WhenUserLacksPermission()
    {
        // Arrange -- user is a Member (cannot invite)
        var familyId = FamilyId.New();
        var inviterId = UserId.New();
        var regularMember = FamilyMember.Create(familyId, inviterId, FamilyRole.Member);
        var memberRepo = new FakeFamilyMemberRepository(existingMember: regularMember);
        var invitationRepo = new FakeFamilyInvitationRepository();
        var authService = new FamilyAuthorizationService(memberRepo);
        var command = new SendInvitationCommand(
            familyId, inviterId,
            Email.From("newmember@example.com"),
            FamilyRole.Member);

        // Act & Assert
        var act = () => SendInvitationCommandHandler.Handle(
            command, authService, invitationRepo, memberRepo, CancellationToken.None);
        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("You do not have permission to send invitations for this family");
    }
}
```

---

## Domain Entity Tests

Test domain aggregates through their factory methods, state transitions, invariant enforcement, and domain events.

```csharp
public class FamilyInvitationTests
{
    private static readonly FamilyId TestFamilyId = FamilyId.New();
    private static readonly UserId TestInviterId = UserId.New();
    private static readonly Email TestEmail = Email.From("invitee@example.com");
    private static readonly FamilyRole TestRole = FamilyRole.Member;
    private const string TestTokenHash =
        "a1b2c3d4e5f6a1b2c3d4e5f6a1b2c3d4e5f6a1b2c3d4e5f6a1b2c3d4e5f6a1b2";

    [Fact]
    public void Create_ShouldCreateInvitationWithValidData()
    {
        var invitation = FamilyInvitation.Create(
            TestFamilyId, TestInviterId, TestEmail, TestRole,
            InvitationToken.From(TestTokenHash), "plaintext-token");

        invitation.Should().NotBeNull();
        invitation.Id.Value.Should().NotBe(Guid.Empty);
        invitation.FamilyId.Should().Be(TestFamilyId);
        invitation.Status.Should().Be(InvitationStatus.Pending);
        invitation.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));
    }

    [Fact]
    public void Create_ShouldRaiseInvitationSentEvent()
    {
        var invitation = FamilyInvitation.Create(
            TestFamilyId, TestInviterId, TestEmail, TestRole,
            InvitationToken.From(TestTokenHash), "plaintext-token");

        invitation.DomainEvents.Should().HaveCount(1);
        invitation.DomainEvents.First().Should().BeOfType<InvitationSentEvent>();
    }

    [Fact]
    public void Accept_ShouldThrow_WhenAlreadyAccepted()
    {
        var invitation = CreateTestInvitation();
        invitation.Accept(UserId.New());

        var act = () => invitation.Accept(UserId.New());
        act.Should().Throw<DomainException>()
            .WithMessage("Cannot accept invitation in status 'Accepted'");
    }
}
```

### Testing State Transitions

Test each valid and invalid transition. Verify:

- The status changes correctly.
- A domain event is raised.
- Invalid transitions throw `DomainException`.

### Testing Expired Entities

For time-dependent logic, use reflection to set private properties:

```csharp
private static FamilyInvitation CreateExpiredInvitation()
{
    var invitation = CreateTestInvitation();

    // Use reflection to set ExpiresAt to the past
    var expiresAtProperty = typeof(FamilyInvitation)
        .GetProperty(nameof(FamilyInvitation.ExpiresAt));
    expiresAtProperty!.SetValue(invitation, DateTime.UtcNow.AddDays(-1));

    return invitation;
}
```

This is necessary because factory methods always create entities with future expiration dates.

---

## Value Object Tests

Test Vogen value objects for valid creation, validation rejection, and edge cases:

```csharp
public class FamilyRoleTests
{
    [Theory]
    [InlineData("Owner")]
    [InlineData("Admin")]
    [InlineData("Member")]
    public void From_ShouldCreateValidRole(string role)
    {
        var familyRole = FamilyRole.From(role);
        familyRole.Value.Should().Be(role);
    }

    [Fact]
    public void From_ShouldThrow_ForInvalidRole()
    {
        var act = () => FamilyRole.From("InvalidRole");
        act.Should().Throw<ValueObjectValidationException>();
    }

    [Fact]
    public void CanInvite_ShouldReturnTrue_ForOwner()
    {
        FamilyRole.Owner.CanInvite().Should().BeTrue();
    }

    [Fact]
    public void CanInvite_ShouldReturnFalse_ForMember()
    {
        FamilyRole.Member.CanInvite().Should().BeFalse();
    }
}
```

---

## Assertion Conventions

**Always use FluentAssertions.** Never use xUnit's built-in `Assert`.

```csharp
// Correct
result.Should().NotBeNull();
result.FamilyId.Value.Should().NotBe(Guid.Empty);
invitationRepo.AddedInvitations.Should().HaveCount(1);

// Wrong -- do not use
Assert.NotNull(result);
Assert.NotEqual(Guid.Empty, result.FamilyId.Value);
```

### Common Assertion Patterns

```csharp
// Null checks
result.Should().NotBeNull();
result.Should().BeNull();

// Value equality (Vogen)
invitation.FamilyId.Should().Be(expectedFamilyId);

// Collection assertions
invitationRepo.AddedInvitations.Should().HaveCount(1);
invitation.DomainEvents.Should().ContainSingle();

// Exception assertions (async)
var act = () => handler.Handle(command, ...);
await act.Should().ThrowAsync<DomainException>()
    .WithMessage("Expected error message");

// Exception assertions (sync)
var act = () => invitation.Accept(userId);
act.Should().Throw<DomainException>();

// Approximate time assertions
invitation.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));

// Boolean flags
invitationRepo.SaveChangesCalled.Should().BeTrue();
```

---

## Test Naming Convention

```
{MethodUnderTest}_Should{ExpectedBehavior}[_When{Condition}]
```

Examples:

- `Handle_ShouldCreateInvitationAndReturnResult`
- `Handle_ShouldThrow_WhenUserLacksPermission`
- `Handle_ShouldThrow_WhenDuplicateInvitationExists`
- `Accept_ShouldTransitionToPendingToAccepted`
- `Accept_ShouldThrow_WhenAlreadyAccepted`
- `IsExpired_ShouldReturnTrue_WhenExpired`

---

## Build Command

```bash
dotnet test tests/FamilyHub.UnitTests/FamilyHub.UnitTests.csproj \
  --filter "FullyQualifiedName~Family" --verbosity normal
```

Run all tests:

```bash
dotnet test tests/FamilyHub.UnitTests/FamilyHub.UnitTests.csproj --verbosity normal
```

---

## Important Notes

### No InternalsVisibleTo

The test project does not have `InternalsVisibleTo` access. Internal methods in the API project are not visible to tests. If you need to test internal logic, either:

- Make the method `public` if it is part of the contract.
- Replicate the logic in the test (e.g., SHA256 hashing).

### No Mocking Framework (Yet)

The project currently does not use Moq or NSubstitute. All test doubles are hand-written fake classes defined as inner classes within test files. A migration to NSubstitute is planned but not yet started.

---

## Related Guides

- [Handler Patterns](handler-patterns.md) -- the handler code being tested
- [Domain Events](domain-events.md) -- testing event handlers
- [Vogen Value Objects](vogen-value-objects.md) -- creating test data with Vogen
- [Authorization Patterns](authorization-patterns.md) -- testing permission checks

---

**Last Updated:** 2026-02-09
