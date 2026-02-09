---
name: integration-test
description: Create integration test with AppDbContext
category: testing
module-aware: true
inputs:
  - entityName: Entity to test (e.g., FamilyInvitation)
  - module: DDD module name
---

# Integration Test Skill

Create integration tests using EF Core InMemoryDatabase for repository testing.

## Test Class

Location: `tests/FamilyHub.IntegrationTests/Features/{Module}/{EntityName}RepositoryTests.cs`

```csharp
public class {EntityName}RepositoryTests : IAsyncLifetime
{
    private AppDbContext _context = null!;
    private {EntityName}Repository _repository = null!;

    public Task InitializeAsync()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: $"Test_{Guid.NewGuid()}")
            .Options;
        _context = new AppDbContext(options);
        _repository = new {EntityName}Repository(_context);
        return Task.CompletedTask;
    }

    public Task DisposeAsync()
    {
        _context.Dispose();
        return Task.CompletedTask;
    }

    [Fact]
    public async Task AddAndRetrieve_ShouldPersist()
    {
        var entity = {EntityName}.Create(...);
        await _repository.AddAsync(entity);
        await _repository.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        var retrieved = await _repository.GetByIdAsync(entity.Id);
        retrieved.Should().NotBeNull();
    }
}
```

## Validation

- [ ] Uses InMemoryDatabase with unique name per test
- [ ] Implements IAsyncLifetime
- [ ] Calls ChangeTracker.Clear() before re-querying
- [ ] Tests CRUD + edge cases
