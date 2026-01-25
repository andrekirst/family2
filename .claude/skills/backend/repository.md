---
name: repository
description: Create repository interface and EF Core implementation
category: backend
module-aware: true
inputs:
  - entityName: Entity name (e.g., Family)
  - module: DDD module name
  - methods: Repository methods needed
---

# Repository Skill

Create repository interface in Domain layer and EF Core implementation in Persistence layer.

## Context

Load module profile: `agent-os/profiles/modules/{module}.yaml`

## Steps

### 1. Create Repository Interface

**Location:** `Modules/FamilyHub.Modules.{Module}/Domain/Repositories/I{Entity}Repository.cs`

```csharp
using FamilyHub.Modules.{Module}.Domain.Entities;
using FamilyHub.Modules.{Module}.Domain.ValueObjects;

namespace FamilyHub.Modules.{Module}.Domain.Repositories;

public interface I{Entity}Repository
{
    Task<{Entity}?> GetByIdAsync({EntityId} id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<{Entity}>> GetAllAsync(int skip, int take, CancellationToken cancellationToken = default);
    Task AddAsync({Entity} entity, CancellationToken cancellationToken = default);
    Task UpdateAsync({Entity} entity, CancellationToken cancellationToken = default);
    Task DeleteAsync({Entity} entity, CancellationToken cancellationToken = default);
}
```

### 2. Create Repository Implementation

**Location:** `Modules/FamilyHub.Modules.{Module}/Persistence/Repositories/{Entity}Repository.cs`

```csharp
using Microsoft.EntityFrameworkCore;
using FamilyHub.Modules.{Module}.Domain.Entities;
using FamilyHub.Modules.{Module}.Domain.Repositories;
using FamilyHub.Modules.{Module}.Domain.ValueObjects;

namespace FamilyHub.Modules.{Module}.Persistence.Repositories;

public sealed class {Entity}Repository : I{Entity}Repository
{
    private readonly {Module}DbContext _context;

    public {Entity}Repository({Module}DbContext context)
    {
        _context = context;
    }

    public async Task<{Entity}?> GetByIdAsync(
        {EntityId} id,
        CancellationToken cancellationToken = default)
    {
        return await _context.{Entities}
            .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<{Entity}>> GetAllAsync(
        int skip,
        int take,
        CancellationToken cancellationToken = default)
    {
        return await _context.{Entities}
            .OrderByDescending(e => e.CreatedAt)
            .Skip(skip)
            .Take(take)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(
        {Entity} entity,
        CancellationToken cancellationToken = default)
    {
        await _context.{Entities}.AddAsync(entity, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(
        {Entity} entity,
        CancellationToken cancellationToken = default)
    {
        _context.{Entities}.Update(entity);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(
        {Entity} entity,
        CancellationToken cancellationToken = default)
    {
        _context.{Entities}.Remove(entity);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
```

### 3. Register in DI

**Location:** `Modules/FamilyHub.Modules.{Module}/ModuleServiceCollectionExtensions.cs`

```csharp
services.AddScoped<I{Entity}Repository, {Entity}Repository>();
```

## Advanced Methods

**Find by criteria:**

```csharp
public async Task<{Entity}?> GetByEmailAsync(
    Email email,
    CancellationToken cancellationToken = default)
{
    return await _context.{Entities}
        .FirstOrDefaultAsync(e => e.Email == email, cancellationToken);
}
```

**Check existence:**

```csharp
public async Task<bool> ExistsAsync(
    {EntityId} id,
    CancellationToken cancellationToken = default)
{
    return await _context.{Entities}
        .AnyAsync(e => e.Id == id, cancellationToken);
}
```

**Include related entities:**

```csharp
public async Task<{Entity}?> GetByIdWithMembersAsync(
    {EntityId} id,
    CancellationToken cancellationToken = default)
{
    return await _context.{Entities}
        .Include(e => e.Members)
        .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
}
```

## Validation

- [ ] Interface in Domain/Repositories/
- [ ] Implementation in Persistence/Repositories/
- [ ] Registered in DI container
- [ ] Uses DbContext for data access
- [ ] CancellationToken on all async methods
