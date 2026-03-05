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

## Shared Test Fakes (FamilyHub.TestCommon)

For cross-cutting concerns, shared fakes live in `tests/FamilyHub.TestCommon/Fakes/`:

```csharp
public sealed class FakeSearchProvider(
    string moduleName, List<SearchResultItem>? results = null) : ISearchProvider
{
    public string ModuleName => moduleName;
    public int SearchCallCount { get; private set; }

    public Task<IReadOnlyList<SearchResultItem>> SearchAsync(
        SearchContext context, CancellationToken ct)
    {
        SearchCallCount++;
        return Task.FromResult<IReadOnlyList<SearchResultItem>>(
            results?.AsReadOnly() ?? new List<SearchResultItem>().AsReadOnly());
    }
}
```

## NullLogger for ILogger Dependencies

When handlers take `ILogger<T>`, use `NullLogger<T>.Instance`:

```csharp
using Microsoft.Extensions.Logging.Abstractions;

var handler = new UniversalSearchQueryHandler(
    [provider], registry, NullLogger<UniversalSearchQueryHandler>.Instance);
```

## Rules

- FluentAssertions for all assertions (never xUnit Assert)
- Fake repositories as inner classes (NSubstitute migration planned)
- Arrange-Act-Assert pattern
- Call static `Handler.Handle()` directly with fakes
- `NullLogger<T>.Instance` for handlers with ILogger (from `Microsoft.Extensions.Logging.Abstractions`)
- Shared cross-cutting fakes go in `FamilyHub.TestCommon/Fakes/`
- Location: `tests/FamilyHub.{Module}.Tests/` (per-module test projects)
