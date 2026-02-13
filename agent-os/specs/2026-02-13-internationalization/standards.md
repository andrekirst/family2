# Standards for Internationalization

The following standards apply to this work and guide implementation decisions.

---

## 1. frontend/angular-components

**Source**: `agent-os/standards/frontend/angular-components.md`

All components are standalone (no NgModules). Use atomic design hierarchy.

**i18n Relevance**: All components use `standalone: true` with `inject()` DI. Translation keys accessed via `$localize` tagged template literals in inline templates. The `I18nService` is injectable via `inject(I18nService)`.

Key rules:

- Always use `standalone: true`
- Import dependencies in `imports` array
- Use Angular Signals for state
- Follow atomic design for organization

---

## 2. frontend/apollo-graphql

**Source**: `agent-os/standards/frontend/apollo-graphql.md`

Use Apollo Client for GraphQL with typed operations.

**i18n Relevance**: The Apollo `setContext` link (auth link) is where we add the `Accept-Language` header. The `UPDATE_MY_LOCALE_MUTATION` follows the existing mutation pattern. Error handling in `catchError` will receive localized error messages from the backend.

Key rules:

- Use `inject(Apollo)` for dependency injection
- Handle errors with catchError
- Use typed operations (gql tagged templates)

---

## 3. backend/graphql-input-command

**Source**: `agent-os/standards/backend/graphql-input-command.md`

Separate Input DTOs (primitives) from Commands (Vogen). See ADR-003.

**i18n Relevance**: The `UpdateUserLocale` command follows this pattern exactly:

- `UpdateUserLocaleRequest` (Input DTO with `string Locale`)
- `UpdateUserLocaleCommand` (Command with `ExternalUserId` + `string Locale`)
- `MutationType.cs` maps input to command

Key rules:

- Input DTOs in `Models/` with primitives
- Commands in `Commands/{Name}/` with Vogen types
- One MutationType per command
- Dispatch via `ICommandBus.SendAsync()`

---

## 4. backend/vogen-value-objects

**Source**: `agent-os/standards/backend/vogen-value-objects.md`

Always use Vogen 8.0+ for domain value objects.

**i18n Relevance**: The `PreferredLocale` field on User is a simple `string` (not a Vogen VO) because locale codes are infrastructure concerns, not domain value objects. Vogen VOs with validation messages (e.g., `Email.Validate()`) keep their English messages — these are internal validation, not user-facing.

Key rules:

- Always include `conversions: Conversions.EfCoreValueConverter`
- Implement `Validate()` for business rules
- Location: `Domain/ValueObjects/{Name}.cs`

---

## 5. database/ef-core-migrations

**Source**: `agent-os/standards/database/ef-core-migrations.md`

**i18n Relevance**: Adding `PreferredLocale` column to the `Users` table requires a migration. Uses the single `AppDbContext` (not per-module context). Column has `HasDefaultValue("en")` so existing rows get English as default.

Key rules:

- Migration name format: `{Timestamp}_{Description}`
- Always test down migrations
- Schema name = module name (lowercase)

---

## 6. testing/unit-testing

**Source**: `agent-os/standards/testing/unit-testing.md`

xUnit + FluentAssertions with fake repository pattern.

**i18n Relevance**: New tests for `User.UpdateLocale()` follow the domain event testing pattern. Validator localization tests set `CultureInfo.CurrentUICulture` to `de` before validation, then assert German messages.

Key rules:

- FluentAssertions for all assertions
- Fake repositories as inner classes
- Arrange-Act-Assert pattern
- Call static `Handler.Handle()` directly with fakes

---

## 7. testing/playwright-e2e

**Source**: `agent-os/standards/testing/playwright-e2e.md`

API-first testing approach. Zero retry policy.

**i18n Relevance**: E2E tests should verify language switching works. Use `data-testid="lang-de"` and `data-testid="lang-en"` selectors for language switcher buttons. Test that switching to German shows German text in key locations.

Key rules:

- Zero retries
- Use data-testid for selectors
- API-first: setup data via GraphQL
- Multi-browser testing
