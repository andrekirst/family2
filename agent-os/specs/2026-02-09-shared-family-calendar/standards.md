# Standards — Shared Family Calendar

## Backend

- **DDD:** CalendarEvent as aggregate root, Vogen value objects for IDs and titles
- **CQRS:** Wolverine static handlers, ICommand/IQuery marker interfaces
- **GraphQL:** Input → Command pattern (ADR-003), Hot Chocolate type extensions
- **Persistence:** EF Core with manual Vogen HasConversion, PostgreSQL schema separation
- **RLS:** Row-Level Security via session variables (app.current_family_id)
- **Events:** Domain events as sealed records extending DomainEvent base

## Frontend

- **Angular signals** for reactive state management
- **Standalone components** with inline templates where appropriate
- **Apollo Angular** for GraphQL with Observable pipe(map, catchError)
- **Tailwind CSS** for styling, matching dashboard design
- **data-testid** attributes for E2E test selectors

## Testing

- **E2E:** Playwright with zero-retry policy, data-testid selectors
