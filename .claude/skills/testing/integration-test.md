---
name: integration-test
description: Create integration test with real DbContext
category: testing
module-aware: true
inputs:
  - testName: Test class name
  - repositoryName: Repository being tested
  - module: DDD module name
---

# Integration Test Skill

Create integration tests with real DbContext using InMemory database.

## Context

Load module profile: `agent-os/profiles/modules/{module}.yaml`

## Steps

### 1. Create Test Class

**Location:** `Modules/FamilyHub.Modules.{Module}.Tests/Persistence/Repositories/{Entity}RepositoryTests.cs`

```csharp
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;
using FamilyHub.Modules.{Module}.Domain.Entities;
using FamilyHub.Modules.{Module}.Domain.ValueObjects;
using FamilyHub.Modules.{Module}.Persistence;
using FamilyHub.Modules.{Module}.Persistence.Repositories;

namespace FamilyHub.Modules.{Module}.Tests.Persistence.Repositories;

public class {Entity}RepositoryTests : IAsyncLifetime
{
    private {Module}DbContext _context = null!;
    private {Entity}Repository _repository = null!;

    public async Task InitializeAsync()
    {
        var options = new DbContextOptionsBuilder<{Module}DbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new {Module}DbContext(options);
        await _context.Database.EnsureCreatedAsync();

        _repository = new {Entity}Repository(_context);
    }

    public async Task DisposeAsync()
    {
        await _context.DisposeAsync();
    }

    [Fact]
    public async Task AddAsync_ValidEntity_PersistsToDatabase()
    {
        // Arrange
        var entity = {Entity}.Create({EntityName}.From("Test Name"));

        // Act
        await _repository.AddAsync(entity);

        // Assert
        var persisted = await _context.{Entities}
            .FirstOrDefaultAsync(e => e.Id == entity.Id);

        persisted.Should().NotBeNull();
        persisted!.Name.Should().Be(entity.Name);
    }

    [Fact]
    public async Task GetByIdAsync_ExistingEntity_ReturnsEntity()
    {
        // Arrange
        var entity = {Entity}.Create({EntityName}.From("Test Name"));
        await _context.{Entities}.AddAsync(entity);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByIdAsync(entity.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(entity.Id);
        result.Name.Should().Be(entity.Name);
    }

    [Fact]
    public async Task GetByIdAsync_NonExistingEntity_ReturnsNull()
    {
        // Arrange
        var nonExistingId = {EntityId}.New();

        // Act
        var result = await _repository.GetByIdAsync(nonExistingId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetAllAsync_MultipleEntities_ReturnsPaginatedList()
    {
        // Arrange
        var entities = Enumerable.Range(1, 5)
            .Select(i => {Entity}.Create({EntityName}.From($"Test {i}")))
            .ToList();

        await _context.{Entities}.AddRangeAsync(entities);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetAllAsync(skip: 1, take: 2);

        // Assert
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task UpdateAsync_ExistingEntity_UpdatesInDatabase()
    {
        // Arrange
        var entity = {Entity}.Create({EntityName}.From("Original Name"));
        await _context.{Entities}.AddAsync(entity);
        await _context.SaveChangesAsync();

        // Act
        entity.UpdateName({EntityName}.From("Updated Name"));
        await _repository.UpdateAsync(entity);

        // Assert
        var updated = await _context.{Entities}
            .FirstOrDefaultAsync(e => e.Id == entity.Id);

        updated!.Name.Value.Should().Be("Updated Name");
    }

    [Fact]
    public async Task DeleteAsync_ExistingEntity_RemovesFromDatabase()
    {
        // Arrange
        var entity = {Entity}.Create({EntityName}.From("To Delete"));
        await _context.{Entities}.AddAsync(entity);
        await _context.SaveChangesAsync();

        // Act
        await _repository.DeleteAsync(entity);

        // Assert
        var deleted = await _context.{Entities}
            .FirstOrDefaultAsync(e => e.Id == entity.Id);

        deleted.Should().BeNull();
    }
}
```

### 2. Test with Related Entities

```csharp
[Fact]
public async Task GetByIdWithMembersAsync_ReturnsWithRelatedEntities()
{
    // Arrange
    var family = Family.Create(FamilyName.From("Test Family"));
    var member = FamilyMember.Create(
        family.Id,
        Email.From("member@test.com"),
        FamilyRole.Member
    );

    await _context.Families.AddAsync(family);
    await _context.FamilyMembers.AddAsync(member);
    await _context.SaveChangesAsync();

    // Clear tracking to ensure fresh load
    _context.ChangeTracker.Clear();

    // Act
    var result = await _repository.GetByIdWithMembersAsync(family.Id);

    // Assert
    result.Should().NotBeNull();
    result!.Members.Should().HaveCount(1);
    result.Members.First().Email.Value.Should().Be("member@test.com");
}
```

### 3. Test Unique Constraint

```csharp
[Fact]
public async Task AddAsync_DuplicateEmail_ThrowsException()
{
    // Arrange
    var email = Email.From("duplicate@test.com");
    var entity1 = User.Create(email);
    var entity2 = User.Create(email);

    await _repository.AddAsync(entity1);

    // Act
    var act = () => _repository.AddAsync(entity2);

    // Assert
    await act.Should().ThrowAsync<DbUpdateException>();
}
```

## Validation

- [ ] Uses InMemoryDatabase for isolation
- [ ] Implements IAsyncLifetime for setup/teardown
- [ ] Each test uses fresh database (Guid in name)
- [ ] Tests CRUD operations
- [ ] Tests edge cases (not found, duplicates)
