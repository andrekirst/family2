---
name: unit-test
description: Create unit test with fake repositories for Wolverine handlers
category: testing
module-aware: true
inputs:
  - className: Handler class to test (e.g., CreateFamilyCommandHandler)
  - module: DDD module name
---

# Unit Test Skill

Create xUnit tests with FluentAssertions and fake repositories for static Wolverine handlers.

## Test Class

Location: `tests/FamilyHub.UnitTests/Features/{Module}/Application/{ClassName}Tests.cs`

```csharp
public class {ClassName}Tests
{
    [Fact]
    public async Task Handle_ShouldSucceed_WhenValidInput()
    {
        // Arrange
        var repo = new Fake{Entity}Repository();
        var command = new {Command}(...);

        // Act
        var result = await {ClassName}.Handle(
            command, repo, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task Handle_ShouldThrow_WhenUnauthorized()
    {
        // Arrange
        var memberRepo = new FakeFamilyMemberRepository(
            existingMember: FamilyMember.Create(familyId, userId, FamilyRole.Member));
        var authService = new FamilyAuthorizationService(memberRepo);

        // Act & Assert
        var act = () => {ClassName}.Handle(command, authService, ...);
        await act.Should().ThrowAsync<DomainException>();
    }
}
```

## Fake Repository (Inner Class)

```csharp
private class Fake{Entity}Repository : I{Entity}Repository
{
    public List<{Entity}> AddedItems { get; } = [];
    public bool SaveChangesCalled { get; private set; }

    public Task AddAsync({Entity} entity, CancellationToken ct)
    {
        AddedItems.Add(entity);
        return Task.CompletedTask;
    }

    public Task<int> SaveChangesAsync(CancellationToken ct)
    {
        SaveChangesCalled = true;
        return Task.FromResult(1);
    }
}
```

## Build Command

```bash
dotnet test tests/FamilyHub.UnitTests/FamilyHub.UnitTests.csproj --verbosity normal
```

## Validation

- [ ] Uses FluentAssertions (never xUnit Assert)
- [ ] Fake repos as inner classes
- [ ] Calls static Handle() directly
- [ ] Tests happy path + error cases
