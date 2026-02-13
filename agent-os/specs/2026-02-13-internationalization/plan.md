# Internationalization (i18n) — Implementation Plan

## Context

Family Hub currently has **zero i18n infrastructure** — all user-facing strings are hardcoded in English across frontend templates, backend validators, domain exceptions, and GraphQL error filters. To support international families, we're adding German and English with locale-aware date/time/number formatting and a language switcher. The user's language preference persists in both `localStorage` (instant access) and the User entity in the database (cross-device sync).

## Architecture Decisions

| # | Decision | Rationale |
|---|----------|-----------|
| 1 | **`@angular/localize` with runtime `$localize`** | Official Angular i18n. Use `loadTranslations()` at bootstrap for runtime switching. Avoids separate builds per locale. |
| 2 | **JSON translation files** (not XLIFF) | Simpler than XLIFF for 2 locales, easier for devs to edit directly. |
| 3 | **Full-stack localization** | Backend error messages also localized via `.resx` resource files + `IStringLocalizer`. |
| 4 | **Error codes on `DomainException`** | Domain stays pure (no `IStringLocalizer` injection). Error filters map codes to localized messages at the infrastructure boundary. |
| 5 | **`PreferredLocale` field on User entity** | Single `string` field (`"en"` / `"de"`). Date/number formats derive from locale — no separate fields needed. |
| 6 | **Language switcher in sidebar user menu** | Always accessible, groups with existing Logout button. Language names in their own language ("Deutsch", "English"). |
| 7 | **Page reload on language switch** | `loadTranslations()` runs at bootstrap. Acceptable for infrequent switching between 2 locales. |

---

## Task 1: Save Spec Documentation

Create `agent-os/specs/2026-02-13-internationalization/` with plan.md, shape.md, standards.md, references.md.

---

## Task 2: Backend — Localization Infrastructure

Add .NET localization services and middleware to `Program.cs`.

**Modify:**

- `src/FamilyHub.Api/Program.cs` — Add `AddLocalization()` + `UseRequestLocalization()` middleware (supported cultures: `en`, `de`)

**Create:**

- `src/FamilyHub.Api/Resources/SharedResources.cs` — Marker class
- `src/FamilyHub.Api/Resources/SharedResources.en.resx` — English shared strings
- `src/FamilyHub.Api/Resources/SharedResources.de.resx` — German shared strings

---

## Task 3: Backend — Add Error Codes to DomainException

Enhance `DomainException` with an `ErrorCode` property so error filters can map codes to localized messages.

**Modify:**

- `src/FamilyHub.Common/Domain/DomainException.cs` — Add `string? ErrorCode` property + new constructor overload

**Create:**

- `src/FamilyHub.Common/Domain/DomainErrorCodes.cs` — Static class with all error code constants

**Modify (add error codes to existing `throw` statements — ~30 across):**

- `src/FamilyHub.Api/Features/Auth/Domain/Entities/User.cs`
- `src/FamilyHub.Api/Features/Family/Domain/Entities/FamilyInvitation.cs`
- All Family command handlers (CreateFamily, SendInvitation, AcceptInvitation, AcceptInvitationById, DeclineInvitation, DeclineInvitationById, RevokeInvitation)
- Calendar entities + handlers
- EventChain handlers

Pattern: `throw new DomainException("Invitation has expired", DomainErrorCodes.InvitationExpired);`

---

## Task 4: Backend — Localize GraphQL Error Filters

Update error filters to use `IStringLocalizer` for translating messages based on request culture (`Accept-Language` header).

**Create:**

- `src/FamilyHub.Api/Resources/DomainErrors.cs` — Marker class
- `src/FamilyHub.Api/Resources/DomainErrors.en.resx` — English error messages keyed by error codes
- `src/FamilyHub.Api/Resources/DomainErrors.de.resx` — German error messages

**Modify:**

- `src/FamilyHub.Api/Common/Infrastructure/GraphQL/ValidationExceptionErrorFilter.cs` — Use `IStringLocalizer`
- `src/FamilyHub.Api/Common/Infrastructure/GraphQL/BusinessLogicExceptionErrorFilter.cs` — Map `DomainException.ErrorCode` to localized message

---

## Task 5: Backend — Localize FluentValidation Messages

Switch validators from hardcoded `.WithMessage("...")` to `IStringLocalizer`-based messages.

**Create (resource files per module):**

- `src/FamilyHub.Api/Resources/Auth/ValidationMessages.{en,de}.resx`
- `src/FamilyHub.Api/Resources/Family/ValidationMessages.{en,de}.resx`
- `src/FamilyHub.Api/Resources/Calendar/ValidationMessages.{en,de}.resx`
- `src/FamilyHub.Api/Resources/EventChain/ValidationMessages.{en,de}.resx`

**Modify (all 11 validators):**

- Auth: `RegisterUserCommandValidator`, `UpdateLastLoginCommandValidator`
- Family: `CreateFamilyCommandValidator`, `SendInvitationCommandValidator`, `AcceptInvitation*`, `DeclineInvitation*`, `RevokeInvitationCommandValidator`
- Calendar: `CreateCalendarEventCommandValidator`
- EventChain: `CreateChainDefinitionCommandValidator`

Pattern: Inject `IStringLocalizer<ValidationMessages>` in constructor, use `.WithMessage(x => localizer["Key"])`.

---

## Task 6: Backend — Add PreferredLocale to User Entity

**Modify:**

- `src/FamilyHub.Api/Features/Auth/Domain/Entities/User.cs` — Add `PreferredLocale` property + `UpdateLocale()` method
- `src/FamilyHub.Api/Features/Auth/Data/UserConfiguration.cs` — Map column (varchar(10), default "en")
- `src/FamilyHub.Api/Features/Auth/Models/UserDto.cs` — Add `PreferredLocale`
- `src/FamilyHub.Api/Features/Auth/Application/Mappers/UserMapper.cs` — Map new field

**Create (new UpdateUserLocale command):**

- `src/FamilyHub.Api/Features/Auth/Application/Commands/UpdateUserLocale/UpdateUserLocaleCommand.cs`
- `src/FamilyHub.Api/Features/Auth/Application/Commands/UpdateUserLocale/UpdateUserLocaleCommandHandler.cs`
- `src/FamilyHub.Api/Features/Auth/Application/Commands/UpdateUserLocale/UpdateUserLocaleCommandValidator.cs`
- `src/FamilyHub.Api/Features/Auth/Application/Commands/UpdateUserLocale/MutationType.cs`

**Run:** `dotnet ef migrations add AddUserPreferredLocale`

---

## Task 7: Frontend — Install and Configure @angular/localize

**Run:** `ng add @angular/localize`

**Create:**

- `src/frontend/family-hub-web/src/app/core/i18n/i18n.service.ts` — Locale management service (signal-based)
- `src/frontend/family-hub-web/src/app/core/i18n/translations/en.json` — ~80 English translation keys
- `src/frontend/family-hub-web/src/app/core/i18n/translations/de.json` — ~80 German translation keys

**Modify:**

- `src/frontend/family-hub-web/src/main.ts` — Import `@angular/localize/init`, register `de` locale data, call `loadTranslations()` before `bootstrapApplication()`
- `src/frontend/family-hub-web/src/app/app.config.ts` — Provide `LOCALE_ID`

Translation key structure: `{feature}.{component}.{element}` (e.g. `family.create.title`, `nav.dashboard`, `dashboard.welcome`)

---

## Task 8: Frontend — Add Accept-Language Header to Apollo Client

**Modify:**

- `src/frontend/family-hub-web/src/app/core/graphql/apollo.config.ts` — Add `Accept-Language` header in auth link context

---

## Task 9: Frontend — Language Switcher Component

**Create:**

- `src/frontend/family-hub-web/src/app/shared/layout/sidebar/language-switcher/language-switcher.component.ts`

**Modify:**

- `src/frontend/family-hub-web/src/app/shared/layout/sidebar/sidebar.component.ts` — Add `<app-language-switcher />` in user menu popup

---

## Task 10: Frontend — Apply $localize to All Component Templates

**Modify (13 components):**

1. `features/auth/login/login.component.ts` — 4 strings
2. `features/dashboard/dashboard.component.ts` — ~20 strings
3. `shared/layout/sidebar/sidebar.component.ts` — 5 strings
4. `features/family/components/family-settings/family-settings.component.ts` — 4 strings
5. `features/family/components/members-list/members-list.component.ts` — 2 strings
6. `features/family/components/pending-invitations/pending-invitations.component.ts` — 3 strings
7. `features/family/components/invite-member/invite-member.component.ts` — 8 strings
8. `features/family/components/invitation-accept/invitation-accept.component.ts` — 12 strings
9. `features/family/components/create-family-dialog/` — 6 strings
10. `shared/components/confirmation-dialog/confirmation-dialog.component.ts` — 2 strings
11. `features/calendar/components/calendar-page/calendar-page.component.ts` — 3 strings
12. `features/calendar/components/event-dialog/event-dialog.component.ts`
13. `shared/services/top-bar.service.ts`

---

## Task 11: Frontend — Localize Date/Time Formatting

**Modify:**

- `src/frontend/family-hub-web/src/app/features/calendar/utils/week.utils.ts` — Replace `'en-US'` with stored locale
- `src/frontend/family-hub-web/src/app/features/calendar/components/calendar-page/calendar-page.component.ts`

Note: `DatePipe` instances in templates (`| date: 'mediumDate'`) automatically adapt when `LOCALE_ID` is set (handled by Task 7).

---

## Task 12: Frontend — Sync Locale to Backend

**Modify:**

- `src/frontend/family-hub-web/src/app/core/i18n/i18n.service.ts` — Add `syncToBackend()`
- `src/frontend/family-hub-web/src/app/core/user/user.service.ts` — Include `preferredLocale` in queries
- `src/frontend/family-hub-web/src/app/features/auth/graphql/auth.operations.ts` — Add `preferredLocale` field + `UPDATE_MY_LOCALE_MUTATION`
- `src/frontend/family-hub-web/src/app/features/auth/callback/callback.component.ts` — Apply backend locale after login

---

## Task 13: Unit Tests

**Backend:** `User.UpdateLocale()`, `DomainException` error codes, validator localization
**Frontend:** `i18n.service.spec.ts`, `language-switcher.component.spec.ts`

---

## Task Dependencies

```
Task 1 (Spec docs)
  ├─> Tasks 2-6 (Backend) — can run in parallel with frontend
  └─> Tasks 7-11 (Frontend) — can run in parallel with backend
       └─> Task 12 (Sync) — depends on 6 + 7
            └─> Task 13 (Tests) — depends on all above
```

## Verification

1. `dotnet build` and `ng build` succeed
2. `dotnet test` and `ng test` pass
3. App loads in English by default
4. Clicking "Deutsch" → page reloads → German UI, German dates
5. GraphQL with `Accept-Language: de` → German error messages
6. Locale persists across login/logout (synced from DB)

## Key Risks

| Risk | Mitigation |
|------|------------|
| `$localize` in inline templates requires careful backtick escaping | Test each component individually |
| Page reload on language switch | Acceptable for 2 locales |
| FluentValidation `WithMessage(string)` evaluates eagerly | Use lambda: `.WithMessage(x => localizer["Key"])` |
| Hot Chocolate error filter singleton vs per-request culture | Verify `IStringLocalizer` resolves per-request |
