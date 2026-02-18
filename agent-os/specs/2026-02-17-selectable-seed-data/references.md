# References for Selectable Seed Data

## Similar Implementations

### CreateFamily Mutation (Backend Pattern)

- **Location:** `src/FamilyHub.Api/Features/Family/Application/Commands/CreateFamily/`
- **Relevance:** Template for GraphQL mutation structure — `[ExtendObjectType]`, Input DTO, Command, Handler
- **Key patterns:**
  - `MutationType.cs` uses `[ExtendObjectType(typeof(FamilyMutation))]`
  - Maps primitive input to Vogen value objects
  - Dispatches via `ICommandBus.SendAsync()`
  - Returns mapped DTO
- **Deviation for Dev:** Dev resolvers call service directly instead of dispatching through Mediator (separate assembly limitation)

### LoginComponent (Frontend Pattern)

- **Location:** `src/frontend/family-hub-web/src/app/features/auth/login/login.component.ts`
- **Relevance:** This is the exact component we're modifying — the dev seed panel is added below the sign-in button
- **Key patterns:**
  - Standalone component with inline template
  - `inject(AuthService)` for DI
  - Tailwind CSS centered card layout
  - i18n attributes on user-facing text
- **Modification:** Add conditional `@if (!environment.production) { <app-dev-seed-panel /> }`

### RootQuery / RootMutation (GraphQL Namespace Pattern)

- **Location:** `src/FamilyHub.Api/Common/Infrastructure/GraphQL/NamespaceTypes/`
- **Relevance:** The dev extensions (`RootQueryDevExtension`, `RootMutationDevExtension`) extend these types
- **Key patterns:**
  - Namespace type returns (`DevQuery Dev() => new()`)
  - `[Authorize]` on protected entries (Dev entries have NO authorize — pre-auth)
  - `[ExtendObjectType(typeof(RootQuery))]` from separate files

### AggregateRoot Base Class

- **Location:** `src/FamilyHub.Common/Domain/AggregateRoot.cs`
- **Relevance:** Seed datasets call `ClearDomainEvents()` after creating entities via factory methods
- **Key patterns:**
  - `RaiseDomainEvent()` called in factory methods (`User.Register()`, `Family.Create()`)
  - `ClearDomainEvents()` public method clears the event list
  - `DomainEventInterceptor` normally collects and publishes events on SaveChanges
  - During seeding, we clear events BEFORE SaveChanges to prevent handler side effects

### Environment Files (Frontend Dev Detection)

- **Location:** `src/frontend/family-hub-web/src/environments/`
- **Relevance:** `environment.production` is the build-time flag used to conditionally show the dev panel
- **Key patterns:**
  - `environment.ts`: `production: true` (production build)
  - `environment.development.ts`: `production: false` (development build)
  - Angular tree-shaking removes dead code paths in production builds

### IModule Pattern

- **Location:** `src/FamilyHub.Api/Common/Modules/IModule.cs`
- **Relevance:** `DevSetup` follows a similar registration pattern but as a static class (since it's loaded via reflection)
- **Key patterns:**
  - `IModule.Register(IServiceCollection, IConfiguration)` for DI registration
  - `RegisterModule<T>()` extension method in Program.cs
  - Conditional registration: `if (builder.Environment.IsDevelopment())`
