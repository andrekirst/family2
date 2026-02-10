# Handler Patterns (Wolverine)

**Purpose:** Guide for implementing command and query handlers using Wolverine in Family Hub.

**Key distinction:** Family Hub uses **Wolverine** (not MediatR). Handlers are **static classes** with a `Handle()` method. Wolverine auto-discovers handlers by convention and injects dependencies as method parameters.

---

## Handler Architecture Overview

```
src/FamilyHub.Api/Features/{Module}/Application/
  Commands/
    {CommandName}/
      {CommandName}Command.cs        # Command record (Vogen value objects)
      {CommandName}CommandHandler.cs  # Static handler class
      {CommandName}Result.cs          # Result record
      {CommandName}CommandValidator.cs # FluentValidation (optional)
      MutationType.cs                # GraphQL mutation endpoint
  Queries/
    {QueryName}/
      {QueryName}Query.cs           # Query record
      {QueryName}QueryHandler.cs    # Static handler class
      QueryType.cs                  # GraphQL query endpoint
  Commands/Shared/
      SharedResult.cs               # Results shared across commands
```

Each command or query lives in its own subfolder with all related files co-located.

---

## Command Pattern

### Command Record

Commands implement `ICommand<TResult>` and use Vogen value objects for all domain-typed parameters.

```csharp
// src/FamilyHub.Api/Features/Family/Application/Commands/SendInvitation/SendInvitationCommand.cs

using FamilyHub.Api.Common.Application;
using FamilyHub.Api.Common.Domain.ValueObjects;
using FamilyHub.Api.Features.Auth.Domain.ValueObjects;
using FamilyHub.Api.Features.Family.Domain.ValueObjects;

namespace FamilyHub.Api.Features.Family.Application.Commands.SendInvitation;

public sealed record SendInvitationCommand(
    FamilyId FamilyId,
    UserId InvitedBy,
    Email InviteeEmail,
    FamilyRole Role
) : ICommand<SendInvitationResult>;
```

The `ICommand<TResult>` interface is a marker interface defined in `Common/Application/`:

```csharp
namespace FamilyHub.Api.Common.Application;

public interface ICommand<out TResult>
{
}
```

### Command Result Record

```csharp
// src/FamilyHub.Api/Features/Family/Application/Commands/SendInvitation/SendInvitationResult.cs

public sealed record SendInvitationResult(InvitationId InvitationId);
```

### Command Handler (Static Class)

Handlers are **static classes** with a **static `Handle()` method**. Wolverine discovers them by convention and injects dependencies as method parameters. There is no constructor injection, no interface implementation.

```csharp
// src/FamilyHub.Api/Features/Family/Application/Commands/SendInvitation/SendInvitationCommandHandler.cs

public static class SendInvitationCommandHandler
{
    public static async Task<SendInvitationResult> Handle(
        SendInvitationCommand command,           // The command (always first)
        FamilyAuthorizationService authService,  // Injected by Wolverine
        IFamilyInvitationRepository invitationRepository,
        IFamilyMemberRepository memberRepository,
        CancellationToken ct)                    // CancellationToken (always last)
    {
        // Authorization check
        if (!await authService.CanInviteAsync(command.InvitedBy, command.FamilyId, ct))
        {
            throw new DomainException("You do not have permission to send invitations");
        }

        // Business logic...
        var invitation = FamilyInvitation.Create(
            command.FamilyId,
            command.InvitedBy,
            command.InviteeEmail,
            command.Role,
            InvitationToken.From(tokenHash),
            plaintextToken);

        await invitationRepository.AddAsync(invitation, ct);
        await invitationRepository.SaveChangesAsync(ct);

        return new SendInvitationResult(invitation.Id);
    }
}
```

**Parameter injection rules:**

1. The first parameter is the command/query message.
2. Remaining parameters are resolved from the DI container by Wolverine.
3. `CancellationToken` is injected automatically when declared.
4. No `[Service]` attributes needed -- that is a Hot Chocolate GraphQL concept, not a Wolverine one.

---

## Query Pattern

### Query Record

Queries implement `IQuery<TResult>`:

```csharp
// src/FamilyHub.Api/Features/Family/Application/Queries/GetMyFamily/GetMyFamilyQuery.cs

public sealed record GetMyFamilyQuery(
    ExternalUserId ExternalUserId
) : IQuery<FamilyDto?>;
```

The `IQuery<TResult>` interface:

```csharp
namespace FamilyHub.Api.Common.Application;

public interface IQuery<out TResult>
{
}
```

### Query Handler (Static Class)

```csharp
// src/FamilyHub.Api/Features/Family/Application/Queries/GetMyFamily/GetMyFamilyQueryHandler.cs

public static class GetMyFamilyQueryHandler
{
    public static async Task<FamilyDto?> Handle(
        GetMyFamilyQuery query,
        IUserRepository userRepository,
        IFamilyRepository familyRepository,
        CancellationToken ct)
    {
        var user = await userRepository.GetByExternalIdAsync(query.ExternalUserId, ct);
        if (user?.FamilyId == null)
        {
            return null;
        }

        var family = await familyRepository.GetByIdWithMembersAsync(user.FamilyId.Value, ct);
        return family is not null ? FamilyMapper.ToDto(family) : null;
    }
}
```

---

## Command Bus and Query Bus

GraphQL types dispatch commands and queries through bus abstractions that wrap Wolverine:

```csharp
// Common/Application/ICommandBus.cs
public interface ICommandBus
{
    Task<TResult> SendAsync<TResult>(ICommand<TResult> command, CancellationToken ct = default);
}

// Common/Application/IQueryBus.cs
public interface IQueryBus
{
    Task<TResult> QueryAsync<TResult>(IQuery<TResult> query, CancellationToken ct = default);
}
```

These are implemented by `WolverineCommandBus` and `WolverineQueryBus`, which delegate to Wolverine's `IMessageBus.InvokeAsync<T>()`:

```csharp
// Common/Infrastructure/Messaging/WolverineCommandBus.cs

public sealed class WolverineCommandBus(IMessageBus messageBus) : ICommandBus
{
    public async Task<TResult> SendAsync<TResult>(ICommand<TResult> command, CancellationToken ct = default)
    {
        return await messageBus.InvokeAsync<TResult>(command, ct);
    }
}
```

---

## Handler Discovery

Wolverine auto-discovers handlers by scanning the assembly. The convention is:

- Handler class name: `{MessageName}Handler` (e.g., `SendInvitationCommandHandler`)
- Handler method name: `Handle`
- First parameter type must match the message type (command, query, or event)
- Handler class must be `static`

No manual registration is needed.

---

## FluentValidation

Validators use FluentValidation and follow the same subfolder pattern:

```csharp
// src/FamilyHub.Api/Features/Family/Application/Commands/SendInvitation/SendInvitationCommandValidator.cs

public class SendInvitationCommandValidator : AbstractValidator<SendInvitationCommand>
{
    public SendInvitationCommandValidator()
    {
        RuleFor(x => x.InviteeEmail.Value)
            .NotEmpty().WithMessage("Invitee email is required")
            .EmailAddress().WithMessage("Invalid email format");

        RuleFor(x => x.Role.Value)
            .Must(role => role is "Admin" or "Member")
            .WithMessage("Invitation role must be 'Admin' or 'Member'");
    }
}
```

Note: Validators access `.Value` on Vogen structs to apply FluentValidation rules on the underlying primitive.

---

## Checklist: Adding a New Command

1. Create subfolder: `Application/Commands/{Name}/`
2. Create `{Name}Command.cs` as a `sealed record` implementing `ICommand<{Name}Result>`
3. Create `{Name}Result.cs` as a `sealed record`
4. Create `{Name}CommandHandler.cs` as a `static class` with `static async Task<{Name}Result> Handle(...)`
5. (Optional) Create `{Name}CommandValidator.cs` extending `AbstractValidator<{Name}Command>`
6. Create `MutationType.cs` for the GraphQL endpoint (see [graphql-patterns.md](graphql-patterns.md))

## Checklist: Adding a New Query

1. Create subfolder: `Application/Queries/{Name}/`
2. Create `{Name}Query.cs` as a `sealed record` implementing `IQuery<TResult>`
3. Create `{Name}QueryHandler.cs` as a `static class` with `static async Task<TResult> Handle(...)`
4. Create `QueryType.cs` for the GraphQL endpoint (see [graphql-patterns.md](graphql-patterns.md))

---

## Related Guides

- [GraphQL Patterns](graphql-patterns.md) -- how MutationType/QueryType dispatch to handlers
- [Vogen Value Objects](vogen-value-objects.md) -- value objects used in commands/queries
- [Testing Patterns](testing-patterns.md) -- how to test static handlers
- [Domain Events](domain-events.md) -- events raised inside handlers
- [Authorization Patterns](authorization-patterns.md) -- permission checks in handlers

---

**Last Updated:** 2026-02-09
