# Standards for Universal Search and Command Palette

The following standards apply to this work.

---

## architecture/ddd-modules

Modular monolith with 8 bounded contexts. Each module is self-contained.

### Module Layout

```
Features/{ModuleName}/
├── Domain/
│   ├── Entities/
│   ├── ValueObjects/
│   ├── Events/
│   └── Repositories/
├── Application/
│   ├── Commands/
│   ├── Queries/
│   └── Search/            # NEW: Search providers
└── Models/
```

### Feature-specific

- `SearchModule` follows the standard `IModule` pattern with self-contained DI
- Search providers live in `Features/{Module}/Application/Search/` within each contributing module
- `Common/Search/` holds cross-cutting interfaces (same pattern as `Common/Widgets/`)
- No direct module dependencies — search providers only access their own module's repositories

---

## backend/graphql-input-command

Separate Input DTOs (primitives) from Commands/Queries (Vogen).

### Feature-specific

- `UniversalSearchRequest` (Input) uses primitives: `string Query`, `string[]? Modules`, `int? Limit`, `string? Locale`
- `UniversalSearchQuery` (Query) maps to domain types: `UserId`, `FamilyId?`
- `QueryType.cs` extends `SearchQuery` namespace type, extracts user from claims via `IUserService`
- File organization: `Features/Search/Application/Queries/UniversalSearch/`

---

## backend/user-context

Accessing the current authenticated user from JWT claims.

### Feature-specific

- `QueryType.cs` uses `ClaimsPrincipal` → `IUserService.GetCurrentUser()` to resolve `UserId` and `FamilyId`
- Search results are scoped to the user's family — `SearchContext` carries both `UserId` and `FamilyId?`
- `[Authorize]` attribute on the `universal` query resolver

---

## backend/permission-system

Role-based permissions with defense-in-depth enforcement.

### Feature-specific

- Command suggestions filtered by `RequiredPermissions` against user's permission set
- Each `CommandDescriptor` declares its `RequiredPermissions` (e.g., `["family:invite"]`)
- Search results themselves are permission-scoped at the provider level (providers only return data the user's family can access)
- Frontend hides commands the user lacks permissions for (HIDE, not disable)

---

## frontend/angular-components

All components are standalone. Use signals for state.

### Feature-specific

- `CommandPaletteComponent` is standalone with `imports: [CommonModule, FormsModule]`
- `CommandPaletteService` uses `signal()` and `computed()` for state (isOpen, query, selectedIndex, mode)
- Tailwind CSS for styling: fixed overlay, backdrop blur, centered modal
- Keyboard shortcuts via `@HostListener('document:keydown.control.k')`
- Focus trap for accessibility

---

## frontend/apollo-graphql

Apollo Client with typed GraphQL operations.

### Feature-specific

- `SearchService` uses `inject(Apollo)` with `query()` method
- `fetchPolicy: 'network-only'` for fresh search results (no caching)
- 300ms debounce on input before firing GraphQL query
- GraphQL operation defined in `shared/graphql/search.operations.ts`
- Error handling with `catchError` → show error state in palette
