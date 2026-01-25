---
name: command-handler
description: Create MediatR command handler with validation
category: backend
module-aware: true
inputs:
  - commandName: PascalCase command name (e.g., CreateFamilyCommand)
  - module: DDD module name
  - resultType: Return type (e.g., CreateFamilyResult)
---

# Command Handler Skill

Create a MediatR command with handler and FluentValidation validator.

## Context

Load module profile: `agent-os/profiles/modules/{module}.yaml`

## Steps

### 1. Create Command

**Location:** `Modules/FamilyHub.Modules.{Module}/Application/Commands/{CommandName}/{CommandName}.cs`

```csharp
using MediatR;
using FamilyHub.Modules.{Module}.Domain.ValueObjects;

namespace FamilyHub.Modules.{Module}.Application.Commands.{CommandName};

public sealed record {CommandName}(
    // Use Vogen value objects, not primitives
    {ValueObject1} Field1,
    {ValueObject2} Field2
) : IRequest<{ResultType}>;
```

### 2. Create Result Type

**Location:** `Modules/FamilyHub.Modules.{Module}/Application/Commands/{CommandName}/{ResultType}.cs`

```csharp
namespace FamilyHub.Modules.{Module}.Application.Commands.{CommandName};

public sealed record {ResultType}(
    {EntityId} Id,
    // Other result fields
    DateTime CreatedAt
);
```

### 3. Create Command Handler

**Location:** `Modules/FamilyHub.Modules.{Module}/Application/Commands/{CommandName}/{CommandName}Handler.cs`

```csharp
using MediatR;
using FamilyHub.Modules.{Module}.Domain.Repositories;

namespace FamilyHub.Modules.{Module}.Application.Commands.{CommandName};

public sealed class {CommandName}Handler
    : IRequestHandler<{CommandName}, {ResultType}>
{
    private readonly I{Entity}Repository _repository;
    private readonly ILogger<{CommandName}Handler> _logger;

    public {CommandName}Handler(
        I{Entity}Repository repository,
        ILogger<{CommandName}Handler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<{ResultType}> Handle(
        {CommandName} command,
        CancellationToken cancellationToken)
    {
        // Create entity using factory method
        var entity = {Entity}.Create(command.Field1, command.Field2);

        // Persist
        await _repository.AddAsync(entity, cancellationToken);

        _logger.LogInformation(
            "{Entity} created with Id {Id}",
            nameof({Entity}),
            entity.Id);

        return new {ResultType}(
            entity.Id,
            entity.CreatedAt
        );
    }
}
```

### 4. Create Validator

**Location:** `Modules/FamilyHub.Modules.{Module}/Application/Commands/{CommandName}/{CommandName}Validator.cs`

```csharp
using FluentValidation;

namespace FamilyHub.Modules.{Module}.Application.Commands.{CommandName};

public sealed class {CommandName}Validator
    : AbstractValidator<{CommandName}>
{
    public {CommandName}Validator()
    {
        RuleFor(x => x.Field1)
            .NotEmpty()
            .WithMessage("Field1 is required.");

        RuleFor(x => x.Field2)
            .NotEmpty()
            .WithMessage("Field2 is required.");
    }
}
```

## Example: CreateFamily

**Command:**

```csharp
public sealed record CreateFamilyCommand(
    FamilyName Name
) : IRequest<CreateFamilyResult>;
```

**Handler:**

```csharp
public async Task<CreateFamilyResult> Handle(
    CreateFamilyCommand command,
    CancellationToken cancellationToken)
{
    var family = Family.Create(command.Name);
    await _repository.AddAsync(family, cancellationToken);
    return new CreateFamilyResult(family.Id, family.Name);
}
```

## Validation

- [ ] Command uses Vogen value objects (not primitives)
- [ ] Handler uses repository interface
- [ ] Entity created via factory method
- [ ] FluentValidation validator created
- [ ] All files in same folder under Commands/{CommandName}/
