---
name: graphql-mutation
description: Create a complete GraphQL mutation following ADR-003 Input→Command pattern
category: backend
module-aware: true
inputs:
  - mutationName: PascalCase mutation name (e.g., CreateFamily)
  - module: DDD module name (e.g., auth, calendar)
  - fields: Input fields with types
---

# GraphQL Mutation Skill

Creates a complete GraphQL mutation following the ADR-003 Input→Command separation pattern.

## Context Loading

Load module profile from: `agent-os/profiles/modules/{module}.yaml`

## Files Created

1. `Presentation/DTOs/{MutationName}Input.cs` - GraphQL input (primitives)
2. `Application/Commands/{MutationName}Command.cs` - MediatR command (Vogen)
3. `Application/Handlers/{MutationName}CommandHandler.cs` - Command handler
4. `Presentation/GraphQL/{Module}Mutations.cs` - GraphQL mutation method (append)
5. `tests/.../Handlers/{MutationName}CommandHandlerTests.cs` - Unit tests

## Step 1: Create Input DTO

Location: `Modules/FamilyHub.Modules.{Module}/Presentation/DTOs/{MutationName}Input.cs`

```csharp
public sealed record {MutationName}Input
{
    [Required]
    public required string {Field} { get; init; }
}
```

**Rules:**

- Only primitive types (string, int, Guid, DateTime, bool)
- Use `[Required]` for required fields
- Use `required` keyword for non-nullable properties

## Step 2: Create Vogen Value Objects (if needed)

Check if value objects exist in `Domain/ValueObjects/`. If not, create them:

```csharp
[ValueObject<string>(conversions: Conversions.EfCoreValueConverter)]
public readonly partial struct {FieldName}
{
    private static Validation Validate(string value) { ... }
}
```

## Step 3: Create MediatR Command

Location: `Modules/FamilyHub.Modules.{Module}/Application/Commands/{MutationName}Command.cs`

```csharp
public sealed record {MutationName}Command(
    {VogenType} {Field}
) : IRequest<{MutationName}Result>;

public sealed record {MutationName}Result(
    {IdType} Id
);
```

**Rules:**

- Use Vogen types, not primitives
- Return a Result record, not the entity

## Step 4: Create Command Handler

Location: `Modules/FamilyHub.Modules.{Module}/Application/Handlers/{MutationName}CommandHandler.cs`

```csharp
public sealed class {MutationName}CommandHandler
    : IRequestHandler<{MutationName}Command, {MutationName}Result>
{
    private readonly I{Entity}Repository _repository;

    public {MutationName}CommandHandler(I{Entity}Repository repository)
    {
        _repository = repository;
    }

    public async Task<{MutationName}Result> Handle(
        {MutationName}Command command,
        CancellationToken cancellationToken)
    {
        var entity = {Entity}.Create(command.{Field});
        await _repository.AddAsync(entity, cancellationToken);
        return new {MutationName}Result(entity.Id);
    }
}
```

## Step 5: Add GraphQL Mutation Method

Location: `Modules/FamilyHub.Modules.{Module}/Presentation/GraphQL/{Module}Mutations.cs`

```csharp
public async Task<{MutationName}Payload> {MutationName}(
    {MutationName}Input input,
    [Service] IMediator mediator,
    CancellationToken cancellationToken)
{
    var command = new {MutationName}Command(
        {VogenType}.From(input.{Field})
    );

    var result = await mediator.Send(command, cancellationToken);
    return new {MutationName}Payload(result.Id);
}
```

## Step 6: Create Unit Tests

Location: `tests/FamilyHub.Tests.Unit/{Module}/Handlers/{MutationName}CommandHandlerTests.cs`

```csharp
public class {MutationName}CommandHandlerTests
{
    [Fact]
    public async Task Handle_ValidCommand_Creates{Entity}()
    {
        // Arrange
        var repository = Substitute.For<I{Entity}Repository>();
        var handler = new {MutationName}CommandHandler(repository);
        var command = new {MutationName}Command({VogenType}.From("value"));

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Id.Should().NotBeEmpty();
        await repository.Received(1).AddAsync(
            Arg.Any<{Entity}>(),
            Arg.Any<CancellationToken>());
    }
}
```

## Verification

- [ ] Input DTO uses only primitives
- [ ] Command uses Vogen types
- [ ] Handler follows repository pattern
- [ ] Mutation maps Input→Command correctly
- [ ] Unit test covers happy path
