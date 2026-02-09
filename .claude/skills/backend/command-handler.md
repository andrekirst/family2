---
name: command-handler
description: Create Wolverine command handler with validation
category: backend
module-aware: true
inputs:
  - commandName: PascalCase command name (e.g., SendInvitationCommand)
  - module: Feature module name (e.g., Family, Auth)
  - resultType: Return type (e.g., SendInvitationResult)
---

# Command Handler Skill

Create a Wolverine command with a static handler class and FluentValidation validator.

## Folder Layout

Each command lives in its own subfolder:

```
src/FamilyHub.Api/Features/{Module}/Application/Commands/{CommandName}/
  {CommandName}Command.cs
  {CommandName}CommandHandler.cs
  {CommandName}CommandValidator.cs
  {CommandName}Result.cs
  MutationType.cs
```

## Command

```csharp
public sealed record {CommandName}Command(
    {ValueObject1} Field1,
    {ValueObject2} Field2
) : ICommand<{ResultType}>;
```

## Handler (Static Class)

```csharp
public static class {CommandName}CommandHandler
{
    public static async Task<{ResultType}> Handle(
        {CommandName}Command command,
        I{Entity}Repository repository,
        CancellationToken ct)
    {
        var entity = {Entity}.Create(command.Field1, command.Field2);
        await repository.AddAsync(entity, ct);
        await repository.SaveChangesAsync(ct);
        return new {ResultType}(entity.Id);
    }
}
```

Key: Static class, static Handle(), dependencies as parameters, no DI registration.

## Validator

```csharp
public class {CommandName}CommandValidator : AbstractValidator<{CommandName}Command>
{
    public {CommandName}CommandValidator()
    {
        RuleFor(x => x.Field1.Value).NotEmpty();
    }
}
```

## Validation

- [ ] Command implements `ICommand<TResult>`
- [ ] Handler is `public static class` with static `Handle()`
- [ ] Dependencies are method parameters (not constructor)
- [ ] All files in subfolder `Commands/{CommandName}/`
