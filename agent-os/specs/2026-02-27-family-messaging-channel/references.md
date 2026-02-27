# References for Family-Wide Messaging Channel

## Similar Implementations

### Family Module (Primary Reference)

- **Location:** `src/FamilyHub.Api/Features/Family/`
- **Relevance:** The most complete module implementation — serves as the template for the Messaging module's entire folder structure, DDD patterns, and GraphQL integration.
- **Key patterns:**
  - Module registration: `FamilyModule : IModule` with `Register()` method
  - Aggregate root: `Family.cs` with private constructor, static `Create()` factory, `RaiseDomainEvent()`
  - Value objects: Vogen VOs in `Domain/ValueObjects/` (FamilyId, FamilyName, FamilyRole)
  - Command pattern: `Commands/{Name}/Command.cs, Handler.cs, Validator.cs, MutationType.cs, Result.cs`
  - Query pattern: `Queries/{Name}/Query.cs, Handler.cs, QueryType.cs`
  - Repository: interface in `Domain/Repositories/`, EF Core impl in `Infrastructure/Repositories/`
  - EF Core config: `Data/FamilyConfiguration.cs` with schema + index definitions
  - GraphQL namespace types: `FamilyMutation`, `FamilyQuery` in `Common/Infrastructure/GraphQL/NamespaceTypes/`
  - DTOs and mappers: `Models/FamilyDto.cs`, `Application/Mappers/FamilyMapper.cs`

### Event Chain Subscriptions (Subscription Reference)

- **Location:** `src/FamilyHub.Api/Features/EventChain/GraphQL/ChainSubscriptions.cs`
- **Relevance:** Only existing subscription implementation in the codebase — shows the Hot Chocolate subscription pattern.
- **Key patterns:**
  - `[ExtendObjectType("Subscription")]` class
  - `[Subscribe]` + `[Topic("ChainExecutionUpdated_{familyId}")]` attributes
  - `[EventMessage] ChainExecutionDto execution` parameter injection
  - Topic-based with parameterized family ID

### Apollo Client Configuration (Frontend WebSocket Reference)

- **Location:** `src/frontend/family-hub-web/src/app/core/graphql/apollo.config.ts`
- **Relevance:** Must be modified to add WebSocket support. Currently HTTP-only with auth link + error link chain.
- **Key patterns:**
  - `provideApollo()` factory function
  - `ApolloLink.from([authLink, errorLink, httpLink])` — needs `split()` wrapper
  - Auth token from `localStorage.getItem('access_token')`
  - Error link with token refresh retry on `AUTH_NOT_AUTHENTICATED`

### Frontend Feature Structure (Calendar Reference)

- **Location:** `src/frontend/family-hub-web/src/app/features/calendar/`
- **Relevance:** Clean example of a lazy-loaded feature with service, GraphQL operations, signals-based components.
- **Key patterns:**
  - `calendar.routes.ts` with `CALENDAR_ROUTES` export
  - `calendar.providers.ts` with `provideCalendarFeature()`
  - `services/calendar.service.ts` — Apollo `query()`/`mutate()` with typed results
  - `graphql/calendar.operations.ts` — `gql` tagged templates
  - Signal-based page component with `signal()`, `computed()`, `inject()`

## Domain Model Reference

- **Location:** `docs/architecture/domain-model-microservices-map.md` (Section 2.8)
- **Relevance:** Defines the Communication Service as a Generic Subdomain. Currently limited to notifications — this feature extends it with messaging.

## Product Backlog Reference

- **Location:** `docs/product-strategy/FEATURE_BACKLOG.md`
- **Relevance:** Direct Messages (RICE: 42.0) and Group Messages (RICE: 38.0) are top P1 backlog items. This family-wide channel is the foundation.
