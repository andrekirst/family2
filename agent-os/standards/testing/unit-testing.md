# Unit Testing

xUnit + FluentAssertions with fake repository pattern.

## Static Handler Testing

```csharp
var result = await CreateFamilyCommandHandler.Handle(
    command, familyRepo, userRepo, memberRepo, CancellationToken.None);

result.Should().NotBeNull();
result.FamilyId.Value.Should().NotBe(Guid.Empty);
```

## Fake Repository Pattern

Inner classes implementing repository interfaces with in-memory state:

```csharp
private class FakeFamilyRepository : IFamilyRepository
{
    public List<Family> AddedFamilies { get; } = [];
    public bool SaveChangesCalled { get; private set; }

    public Task AddAsync(Family family, CancellationToken ct = default)
    {
        AddedFamilies.Add(family);
        return Task.CompletedTask;
    }

    public Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        SaveChangesCalled = true;
        return Task.FromResult(1);
    }
}
```

## Testing Authorization

Pass real authorization service backed by fakes:

```csharp
var memberRepo = new FakeFamilyMemberRepository(existingMember: regularMember);
var authService = new FamilyAuthorizationService(memberRepo);

var act = () => SendInvitationCommandHandler.Handle(
    command, authService, invitationRepo, memberRepo, CancellationToken.None);
await act.Should().ThrowAsync<DomainException>();
```

## Domain Event Testing

```csharp
var family = Family.Create(FamilyName.From("Smith"), UserId.New());
family.DomainEvents.Should().ContainSingle()
    .Which.Should().BeOfType<FamilyCreatedEvent>();
```

## Testing Expired Entities

Use reflection for time-dependent logic:

```csharp
typeof(FamilyInvitation).GetProperty("ExpiresAt")!
    .SetValue(invitation, DateTime.UtcNow.AddDays(-1));
```

## Rules

- FluentAssertions for all assertions (never xUnit Assert)
- Fake repositories as inner classes (NSubstitute migration planned)
- Arrange-Act-Assert pattern
- Call static `Handler.Handle()` directly with fakes
- Location: `tests/FamilyHub.UnitTests/Features/{Module}/`
