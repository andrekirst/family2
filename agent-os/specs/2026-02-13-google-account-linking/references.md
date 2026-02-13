# References for Google Account Linking

## Similar Implementations

### Auth Module (Primary Reference)

- **Location:** `src/FamilyHub.Api/Features/Auth/`
- **Relevance:** Establishes the OIDC authentication pattern, User aggregate structure, and ExternalUserId storage
- **Key patterns:**
  - `AuthModule : IModule` with DI registrations
  - `User` aggregate with `ExternalUserId` / `ExternalProvider` fields
  - `RegisterUserCommand` handler: upsert pattern (create new or update existing)
  - `ClaimsPrincipal` → `IUserService.GetCurrentUser()` pattern
  - `UserConfiguration` EF Core config with Vogen converters

### Calendar Module (HostedService Reference)

- **Location:** `src/FamilyHub.Api/Features/Calendar/`
- **Relevance:** Shows IModule pattern with HostedService registration (for background token refresh)
- **Key patterns:**
  - `CalendarModule.Register()` with `services.AddHostedService<CancelledEventCleanupService>()`
  - `Configure<CalendarCleanupOptions>(configuration.GetSection(...))`
  - Schema separation: `calendar` schema

### Family Module (Command/Query Reference)

- **Location:** `src/FamilyHub.Api/Features/Family/`
- **Relevance:** Most complete example of the GraphQL Input->Command pattern with authorization
- **Key patterns:**
  - `Commands/{Name}/Command.cs, Handler.cs, MutationType.cs, Validator.cs` subfolder layout
  - `FamilyAuthorizationService` for domain-level authorization
  - `FamilyMutation` / `FamilyQuery` namespace types
  - Secure token pattern in `SendInvitation` (SHA256 hash)

### Shared Family Calendar Spec

- **Location:** `agent-os/specs/2026-02-09-shared-family-calendar/`
- **Relevance:** Explicitly lists "External calendar sync (Google, Apple)" as future/out-of-scope work
- **Key insight:** This Google Account Linking feature is the prerequisite for that future calendar sync

## Architecture Documents

- **ADR-001:** `docs/architecture/ADR-001-MODULAR-MONOLITH-FIRST.md` — Module isolation strategy
- **ADR-003:** `docs/architecture/ADR-003-GRAPHQL-INPUT-COMMAND-PATTERN.md` — Input->Command separation
- **CLAUDE.md:** Root CLAUDE.md — IModule pattern, conflict surface, module registration

## Key Files

| Purpose | File Path |
|---------|-----------|
| IModule interface | `src/FamilyHub.Api/Common/Modules/IModule.cs` |
| Module registration | `src/FamilyHub.Api/Common/Modules/ModuleExtensions.cs` |
| User aggregate | `src/FamilyHub.Api/Features/Auth/Domain/Entities/User.cs` |
| User EF config | `src/FamilyHub.Api/Features/Auth/Data/UserConfiguration.cs` |
| AppDbContext | `src/FamilyHub.Api/Common/Database/AppDbContext.cs` |
| RootQuery | `src/FamilyHub.Api/Common/Infrastructure/GraphQL/NamespaceTypes/RootQuery.cs` |
| RootMutation | `src/FamilyHub.Api/Common/Infrastructure/GraphQL/NamespaceTypes/RootMutation.cs` |
| Program.cs | `src/FamilyHub.Api/Program.cs` |
| SecureTokenHelper | `src/FamilyHub.Api/Common/Infrastructure/Security/SecureTokenHelper.cs` |
| CalendarModule | `src/FamilyHub.Api/Features/Calendar/CalendarModule.cs` |
| Keycloak realm | `infrastructure/keycloak/familyhub-realm-template.json` |
| Frontend routes | `src/frontend/family-hub-web/src/app/app.routes.ts` |
| Frontend config | `src/frontend/family-hub-web/src/app/app.config.ts` |
| Auth service (FE) | `src/frontend/family-hub-web/src/app/core/auth/auth.service.ts` |
