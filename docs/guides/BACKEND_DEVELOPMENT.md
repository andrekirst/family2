# Backend Development Guide

**Purpose:** Index for Family Hub's .NET backend development documentation.

**Tech Stack:** .NET Core 10, C# 14, **martinothamar/Mediator** (source-generated, compile-time discovery), Hot Chocolate GraphQL 14.1, EF Core 10, PostgreSQL 16, Vogen 8.0+, FluentValidation

---

## Module Structure

```
src/FamilyHub.Api/
  Common/
    Application/           # ICommand, IQuery, ICommandBus, IQueryBus
    Database/              # AppDbContext, migrations
    Domain/                # AggregateRoot, DomainEvent, shared value objects
    Infrastructure/        # MediatorCommandBus, Behaviors, ClaimNames
    Middleware/             # PostgresRlsMiddleware
    Services/              # IUserService
    Email/                 # IEmailService, SmtpEmailService, templates
  Features/
    Auth/
      Application/         # Handlers, mappers, validators
      Data/                # EF Core configurations
      Domain/              # Entities, value objects, events, repositories
      GraphQL/             # AuthMutations, AuthQueries (root types)
      Infrastructure/      # Repository implementations
      Models/              # Request/Response DTOs
    Family/
      Application/
        Commands/
          CreateFamily/    # Command, Handler, Result, Validator, MutationType
          SendInvitation/  # Command, Handler, Result, Validator, MutationType
          AcceptInvitation/
          AcceptInvitationById/
          DeclineInvitation/
          DeclineInvitationById/
          RevokeInvitation/
          Shared/          # Shared result records
        Queries/
          GetMyFamily/     # Query, Handler, QueryType
          GetFamilyMembers/
        EventHandlers/     # InvitationSentEventHandler
        Mappers/           # FamilyMapper, InvitationMapper, FamilyMemberMapper
        Services/          # FamilyAuthorizationService
      Data/                # Entity configurations
      Domain/
        Entities/          # Family, FamilyMember, FamilyInvitation
        ValueObjects/      # FamilyId, FamilyRole, InvitationToken, etc.
        Events/            # FamilyCreatedEvent, InvitationSentEvent, etc.
        Repositories/      # Repository interfaces
      GraphQL/             # FamilyMutations, FamilyQueries
      Infrastructure/      # Repository implementations
      Models/              # Request/Response DTOs
```

---

## Sub-Guides

| Guide | When to Use |
|-------|------------|
| [Handler Patterns](backend/handler-patterns.md) | Adding commands, queries, or handlers. Interface-based handlers with Mediator. |
| [Authorization Patterns](backend/authorization-patterns.md) | Implementing permission checks, understanding FamilyRole, defense-in-depth strategy. |
| [Testing Patterns](backend/testing-patterns.md) | Writing unit tests, creating fake repositories, testing domain entities and handlers. |
| [GraphQL Patterns](backend/graphql-patterns.md) | Adding GraphQL mutations/queries, Input-to-Command mapping, Hot Chocolate conventions. |
| [EF Core Patterns](backend/ef-core-patterns.md) | Database configuration, migrations, Vogen value converters, repository pattern. |
| [Vogen Value Objects](backend/vogen-value-objects.md) | Creating value objects, ID types, validation, NormalizeInput, enumeration-style VOs. |
| [Domain Events](backend/domain-events.md) | Raising and handling domain events, event publishing lifecycle, cross-module events. |

---

## Quick Reference

### Build

```bash
dotnet build src/FamilyHub.Api/FamilyHub.Api.csproj
```

### Test

```bash
dotnet test tests/FamilyHub.UnitTests/FamilyHub.UnitTests.csproj --verbosity normal
```

### Key Conventions

- **Handlers:** Sealed classes implementing `ICommandHandler<T,R>` or `IQueryHandler<T,R>` with constructor DI and `ValueTask<T>` returns.
- **Commands:** `ICommand<TResult>` records with Vogen value objects.
- **Queries:** `IQuery<TResult>` records with Vogen value objects.
- **Pipeline:** `DomainEventPublishing → Logging → Validation → Transaction → Handler` (behaviors registered in `Program.cs`).
- **GraphQL:** `MutationType` / `QueryType` per command/query, extends root types with `[ExtendObjectType]`.
- **Value Objects:** `[ValueObject<T>(conversions: Conversions.EfCoreValueConverter)]` on all domain types.
- **Tests:** xUnit + FluentAssertions, fake repository inner classes (no mocking framework).
- **Events:** `sealed record` extending `DomainEvent`, raised via `RaiseDomainEvent()`, published post-commit via `DomainEventPublishingBehavior`.
- **Transactions:** Handlers do NOT call `SaveChangesAsync()` — centralized in `TransactionBehavior`.

### Common Tasks

| Task | Start Here |
|------|-----------|
| Add a new mutation | [GraphQL Patterns](backend/graphql-patterns.md) and [Handler Patterns](backend/handler-patterns.md) |
| Add a new query | [GraphQL Patterns](backend/graphql-patterns.md) and [Handler Patterns](backend/handler-patterns.md) |
| Add a new value object | [Vogen Value Objects](backend/vogen-value-objects.md) |
| Add a new domain event | [Domain Events](backend/domain-events.md) |
| Add permission checks | [Authorization Patterns](backend/authorization-patterns.md) |
| Write tests for a handler | [Testing Patterns](backend/testing-patterns.md) |
| Add a new entity to EF Core | [EF Core Patterns](backend/ef-core-patterns.md) |
| Run migrations | [EF Core Patterns](backend/ef-core-patterns.md) |

---

## Related Documentation

- **Coding Standards:** [docs/development/CODING_STANDARDS.md](../development/CODING_STANDARDS.md)
- **Workflows:** [docs/development/WORKFLOWS.md](../development/WORKFLOWS.md)
- **Patterns:** [docs/development/PATTERNS.md](../development/PATTERNS.md)
- **ADR-003:** [GraphQL Input-Command Pattern](../architecture/ADR-003-GRAPHQL-INPUT-COMMAND-PATTERN.md)
- **ADR-005:** [Module Extraction](../architecture/ADR-005-FAMILY-MODULE-EXTRACTION-PATTERN.md)
- **Frontend Guide:** [FRONTEND_DEVELOPMENT.md](FRONTEND_DEVELOPMENT.md)
- **Database Guide:** [DATABASE_DEVELOPMENT.md](DATABASE_DEVELOPMENT.md)

---

**Last Updated:** 2026-02-10
**Version:** 8.0.0 (Migrated from Wolverine to martinothamar/Mediator)
