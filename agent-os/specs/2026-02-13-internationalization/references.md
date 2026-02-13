# References for Internationalization

## Key Files Explored During Shaping

### 1. DomainException

**Location**: `src/FamilyHub.Common/Domain/DomainException.cs`
**Current state**: Simple exception with `string message` constructor only. No error code.
**Change needed**: Add `string? ErrorCode` property + new constructor overload.

### 2. Program.cs

**Location**: `src/FamilyHub.Api/Program.cs`
**Relevance**: Insert point for `AddLocalization()` services and `UseRequestLocalization()` middleware.

### 3. main.ts (Frontend Bootstrap)

**Location**: `src/frontend/family-hub-web/src/main.ts`
**Current state**: Simple `bootstrapApplication(App, appConfig)` call (6 lines).
**Change needed**: Import `@angular/localize/init`, register `de` locale data, call `loadTranslations()` before bootstrap.

### 4. app.config.ts

**Location**: `src/frontend/family-hub-web/src/app/app.config.ts`
**Current state**: Provides router, HTTP client, Apollo, calendar, family features.
**Change needed**: Add `{ provide: LOCALE_ID, useValue: locale }` provider.

### 5. Sidebar Component

**Location**: `src/frontend/family-hub-web/src/app/shared/layout/sidebar/sidebar.component.ts`
**Current state**: User menu popup with only a "Logout" button. 137 lines, inline template.
**Change needed**: Import `LanguageSwitcherComponent`, add `<app-language-switcher />` above logout in user menu.
**Hardcoded strings**: "Family Hub", "Expand sidebar", "Collapse sidebar", nav labels ("Dashboard", "Family", "Calendar"), "Logout".

### 6. User Entity

**Location**: `src/FamilyHub.Api/Features/Auth/Domain/Entities/User.cs`
**Current state**: ~200 lines. Fields: Email, Name, Username, ExternalUserId, ExternalProvider, FamilyId, EmailVerified, IsActive, CreatedAt, LastLoginAt, UpdatedAt.
**Change needed**: Add `PreferredLocale` property (string, default "en") + `UpdateLocale()` method.

### 7. GraphQL Error Filters

**Location**: `src/FamilyHub.Api/Common/Infrastructure/GraphQL/`

- `ValidationExceptionErrorFilter.cs` — hardcoded "Validation failed"
- `BusinessLogicExceptionErrorFilter.cs` — passes through exception message
**Change needed**: Inject `IStringLocalizer`, map error codes to localized messages.

### 8. Apollo Config

**Location**: `src/frontend/family-hub-web/src/app/core/graphql/apollo.config.ts`
**Relevance**: The `setContext` auth link where `Accept-Language` header will be added alongside the existing `authorization` header.

---

## Components with Hardcoded Strings

| Component | Location | String Count |
|-----------|----------|-------------|
| login.component.ts | features/auth/login/ | 4 |
| dashboard.component.ts | features/dashboard/ | ~20 |
| sidebar.component.ts | shared/layout/sidebar/ | 5 |
| family-settings.component.ts | features/family/components/family-settings/ | 4 |
| members-list.component.ts | features/family/components/members-list/ | 2 |
| pending-invitations.component.ts | features/family/components/pending-invitations/ | 3 |
| invite-member.component.ts | features/family/components/invite-member/ | 8 |
| invitation-accept.component.ts | features/family/components/invitation-accept/ | 12 |
| create-family-dialog | features/family/components/create-family-dialog/ | 6 |
| confirmation-dialog.component.ts | shared/components/confirmation-dialog/ | 2 |
| calendar-page.component.ts | features/calendar/components/calendar-page/ | 3 |
| event-dialog.component.ts | features/calendar/components/event-dialog/ | TBD |
| **Total** | | **~70-80** |

---

## Validators with Hardcoded Messages

| Validator | Module | Message Count |
|-----------|--------|--------------|
| RegisterUserCommandValidator | Auth | ~4 |
| UpdateLastLoginCommandValidator | Auth | ~3 |
| CreateFamilyCommandValidator | Family | ~2 |
| SendInvitationCommandValidator | Family | ~3 |
| AcceptInvitationCommandValidator | Family | ~2 |
| AcceptInvitationByIdCommandValidator | Family | ~2 |
| DeclineInvitationCommandValidator | Family | ~1 |
| DeclineInvitationByIdCommandValidator | Family | ~2 |
| RevokeInvitationCommandValidator | Family | ~1 |
| CreateCalendarEventCommandValidator | Calendar | ~4 |
| CreateChainDefinitionCommandValidator | EventChain | ~10 |
| **Total** | | **~36** |

---

## Domain Exceptions with Hardcoded Messages

| File | Module | Throw Count |
|------|--------|------------|
| User.cs | Auth | 2 |
| FamilyInvitation.cs | Family | 4 |
| CreateFamilyCommandHandler.cs | Family | 2 |
| SendInvitationCommandHandler.cs | Family | 2 |
| AcceptInvitationCommandHandler.cs | Family | 3 |
| AcceptInvitationByIdCommandHandler.cs | Family | 4 |
| DeclineInvitationCommandHandler.cs | Family | 1 |
| DeclineInvitationByIdCommandHandler.cs | Family | 3 |
| RevokeInvitationCommandHandler.cs | Family | 2 |
| CalendarEvent.cs | Calendar | 1 |
| CancelCalendarEventCommandHandler.cs | Calendar | 1 |
| UpdateCalendarEventCommandHandler.cs | Calendar | 2 |
| EventChain handlers | EventChain | ~5 |
| **Total** | | **~30** |
