# DDD Module Structure

Modular monolith with 8 bounded contexts. Each module is self-contained.

## Module Layout

```
Modules/FamilyHub.Modules.{ModuleName}/
├── Domain/
│   ├── Entities/          # Aggregates
│   ├── ValueObjects/      # Vogen types
│   ├── Events/            # Domain events
│   └── Repositories/      # Repository interfaces
├── Application/
│   ├── Commands/          # Write operations
│   ├── Queries/           # Read operations
│   ├── Handlers/          # MediatR handlers
│   └── Validators/        # FluentValidation
├── Persistence/
│   ├── Configurations/    # EF Core configs
│   ├── Repositories/      # Implementations
│   └── Migrations/        # EF Core migrations
└── Presentation/
    ├── GraphQL/           # Mutations, queries, types
    └── DTOs/              # Input DTOs
```

## 8 Domain Modules

| Module | Schema | Aggregates |
|--------|--------|------------|
| Auth | auth | User, Family |
| Calendar | calendar | Event, Appointment |
| Task | task | Task, Assignment |
| Shopping | shopping | ShoppingList, Item |
| Health | health | Appointment, Prescription |
| MealPlanning | meal | MealPlan, Recipe |
| Finance | finance | Budget, Expense |
| Communication | communication | Notification |

## Cross-Module Communication

- Use domain events via RabbitMQ
- Reference IDs only (no FK constraints across modules)
- IUserLookupService for cross-module queries

## Rules

- One DbContext per module
- One PostgreSQL schema per module
- No direct module dependencies
- Event-driven cross-module communication
