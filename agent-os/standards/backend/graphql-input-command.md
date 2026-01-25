# GraphQL Input→Command Pattern

Separate Input DTOs (primitives) from MediatR Commands (Vogen). See ADR-003.

## Why

Hot Chocolate cannot deserialize Vogen value objects. This creates clean separation between presentation and domain layers.

## GraphQL Input (primitives only)

```csharp
public sealed record CreateFamilyInput
{
    [Required]
    public required string Name { get; init; }
}
```

## MediatR Command (Vogen types)

```csharp
public sealed record CreateFamilyCommand(
    FamilyName Name
) : IRequest<CreateFamilyResult>;
```

## Mutation (mapping layer)

```csharp
public async Task<CreateFamilyPayload> CreateFamily(
    CreateFamilyInput input,
    [Service] IMediator mediator)
{
    var command = new CreateFamilyCommand(
        FamilyName.From(input.Name)  // Primitive → Vogen
    );
    var result = await mediator.Send(command);
    return new CreateFamilyPayload(result);
}
```

## Rules

- Input DTOs: `Presentation/DTOs/{Name}Input.cs`
- Commands: `Application/Commands/{Name}Command.cs`
- Handlers: `Application/Handlers/{Name}CommandHandler.cs`
- Never use Vogen types in GraphQL input types
- Always validate at Vogen boundary (`.From()` throws if invalid)
