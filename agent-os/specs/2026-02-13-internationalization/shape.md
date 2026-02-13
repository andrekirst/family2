# Internationalization (i18n) — Shaping Notes

**Feature**: Multi-language support (German + English) with locale-aware formatting
**Created**: 2026-02-13

---

## Scope

### What's In Scope

1. **Frontend i18n**: `@angular/localize` with runtime `$localize` and JSON translation files
2. **Backend localization**: `.resx` resource files with `IStringLocalizer` for validators, domain errors, GraphQL errors
3. **Languages**: English (default) and German
4. **Formatting**: Date, time, and number formatting via Angular locale data + `LOCALE_ID`
5. **Language switcher**: Compact toggle in sidebar user menu
6. **Preference storage**: localStorage (instant) + User entity in DB (cross-device sync)
7. **~80 translation keys** across 13 frontend components
8. **~30 domain exception messages** with error codes
9. **~36 FluentValidation messages** across 11 validators

### What's Out of Scope

1. Right-to-left (RTL) language support
2. More than 2 languages (future expansion)
3. Translation management tooling (Crowdin, Lokalise)
4. Server-side rendering / locale-based URL routing
5. Currency formatting (no financial features in current modules)
6. Vogen value object validation message localization (validation messages are internal, not user-facing)

---

## Decisions

### 1. i18n Library Choice

**Question**: Which Angular i18n approach?
**Answer**: `@angular/localize` (official Angular i18n)
**Rationale**: Best TypeScript integration, tree-shakeable, compile-time checking of `$localize` tags. Runtime mode via `loadTranslations()` supports dynamic switching without separate builds.

### 2. Translation File Format

**Question**: XLIFF vs JSON?
**Answer**: JSON
**Rationale**: Simpler for 2 locales, directly editable by developers, no tooling overhead. XLIFF would be appropriate if we used external translation services.

### 3. Backend Localization Strategy

**Question**: Frontend-only or full-stack?
**Answer**: Full-stack
**Rationale**: Backend validation and domain error messages are returned to users via GraphQL. Localizing only the frontend would leave error messages in English.

### 4. Domain Exception Pattern

**Question**: How to localize domain exceptions without injecting IStringLocalizer into entities?
**Answer**: Error codes on DomainException, localized at the GraphQL error filter boundary
**Rationale**: Keeps domain layer pure (no infrastructure dependencies). Error codes are stable identifiers that can be mapped to any language.

### 5. User Preference Storage

**Question**: Where to store locale preference?
**Answer**: Both localStorage and User entity in database
**Rationale**: localStorage for instant access at bootstrap (before authentication). Database sync for cross-device consistency.

### 6. Language Switcher Placement

**Question**: Where in the UI?
**Answer**: Sidebar user menu popup (above Logout button)
**Rationale**: Always accessible, groups with existing user preferences. Two toggle buttons showing "English" / "Deutsch" — language names in their own language for recognition.

### 7. Reload vs Hot-Swap

**Question**: Page reload or hot-swap on language change?
**Answer**: Page reload
**Rationale**: `loadTranslations()` is called at bootstrap. Hot-swapping would require a custom reactive translation pipe, adding complexity for minimal benefit (language switching is infrequent).

---

## Technical Constraints

1. **Angular 21** with standalone components (no NgModules) — `$localize` works in both inline and external templates
2. **Most components use inline templates** (TypeScript `template:` property) — must use `$localize` tagged template literals, not `i18n` HTML attributes
3. **martinothamar/Mediator** (not MediatR) — source-generated, validators resolved via DI
4. **Hot Chocolate GraphQL** — error filters are the localization boundary
5. **Single `AppDbContext`** — migration adds column to existing `Users` table
6. **Keycloak** supports a `locale` claim but we don't extract it; using our own `PreferredLocale` field instead

---

## Risks & Mitigations

### Risk: `$localize` backtick escaping in inline templates

Template literals within template literals require careful escaping.
**Mitigation**: Test each component individually after conversion.

### Risk: FluentValidation eager message evaluation

`WithMessage(string)` evaluates at validator construction time, not per-request.
**Mitigation**: Use lambda overload `.WithMessage(x => localizer["Key"])` for deferred resolution.

### Risk: Hot Chocolate error filter singleton behavior

Error filters may be registered as singletons, but `IStringLocalizer` must resolve per-request culture.
**Mitigation**: `IStringLocalizer` uses `CultureInfo.CurrentUICulture` which is set per-request by `UseRequestLocalization()` middleware.

---

## Context

- **Visuals**: None — simple language toggle buttons in sidebar user menu
- **References**: No existing i18n patterns in codebase; follows standard IModule pattern for new command
- **Product alignment**: Feature backlog lists "Multi-language" as P2, 3 weeks effort, RICE score 36.0
