# Standards for Photo Gallery with Grid and Image Views

The following standards apply to this work.

---

## architecture/ddd-modules

Modular monolith with 8 bounded contexts. Each module is self-contained.

### Module Layout

```
Features/Photos/
├── Domain/
│   ├── Entities/          # Aggregates
│   ├── ValueObjects/      # Vogen types
│   ├── Events/            # Domain events
│   └── Repositories/      # Repository interfaces
├── Application/
│   ├── Commands/          # Write operations
│   ├── Queries/           # Read operations
│   ├── Handlers/          # Mediator handlers
│   └── Validators/        # FluentValidation
├── Data/
│   └── PhotoConfiguration.cs  # EF Core config
├── Infrastructure/
│   └── Repositories/      # Implementations
├── GraphQL/               # Mutations, queries
├── Models/                # DTOs, request types
└── PhotosModule.cs        # IModule registration
```

### Rules

- One PostgreSQL schema per module (`photos`)
- No direct module dependencies
- Event-driven cross-module communication
- Reference IDs only (no FK constraints across modules)

---

## backend/graphql-input-command

Separate Input DTOs (primitives) from Commands (Vogen). See ADR-003.

### Pattern

- Input DTOs in `Models/` with primitives (string, Guid, int)
- Commands in `Application/Commands/` with Vogen types (PhotoId, PhotoCaption)
- GraphQL mutations map primitives → Vogen, dispatch via `ICommandBus.SendAsync()`
- Each mutation handler: extract user from ClaimsPrincipal, validate, create command, send

### For Photos

- `UploadPhotoRequest` (primitives) → `UploadPhotoCommand` (Vogen types)
- `UpdatePhotoCaptionRequest` → `UpdatePhotoCaptionCommand`

---

## backend/vogen-value-objects

Always use Vogen 8.0+ for domain value objects. Never use primitives in commands/domain.

### For Photos

```csharp
[ValueObject<Guid>(conversions: Conversions.EfCoreValueConverter)]
public readonly partial struct PhotoId
{
    public static PhotoId New() => From(Guid.NewGuid());
    private static Validation Validate(Guid value)
        => value == Guid.Empty ? Validation.Invalid("PhotoId cannot be empty.") : Validation.Ok;
}

[ValueObject<string>(conversions: Conversions.EfCoreValueConverter)]
public readonly partial struct PhotoCaption
{
    private static Validation Validate(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return Validation.Invalid("Caption cannot be empty.");
        if (value.Length > 500)
            return Validation.Invalid("Caption cannot exceed 500 characters.");
        return Validation.Ok;
    }
}
```

---

## frontend/angular-components

All components are standalone (no NgModules). Use Angular Signals for state.

### For Photos

- `PhotoGridComponent` — standalone, signal inputs (`input()`), output emitters
- `PhotoViewerComponent` — standalone, keyboard `@HostListener`
- `PhotosPageComponent` — standalone, orchestrates view switching with signals

### Rules

- Always use `standalone: true`
- Import dependencies in `imports` array
- Use Angular Signals for state (`signal()`, `computed()`)

---

## frontend/apollo-graphql

Use Apollo Client for GraphQL with typed operations.

### For Photos

- Operations in `graphql/photos.operations.ts` using `gql` tagged templates
- Service wraps Apollo with `inject(Apollo)`
- Error handling with `catchError` returning fallback values
- Use `fetchPolicy: 'network-only'` for mutations that change state

---

## database/ef-core-migrations

EF Core with schema separation per module.

### For Photos

- Schema: `photos`
- Table: `photos.photos`
- Indexes: `(FamilyId, CreatedAt)` for grid pagination, `(FamilyId, IsDeleted)` for filtered queries
- Vogen converters: `PhotoId.EfCoreValueConverter`, `PhotoCaption.EfCoreValueConverter`
- Ignore `DomainEvents` collection

---

## testing/unit-testing

xUnit + FluentAssertions with fake repository pattern.

### For Photos

- `FakePhotoRepository` in TestCommon with in-memory `List<Photo>`
- Domain tests: aggregate creation, domain events, validation
- Handler tests: call handler directly with fakes, verify state changes
- FluentAssertions for all assertions (never xUnit Assert)
- Arrange-Act-Assert pattern

---

## backend/permission-system

Role-based permissions with defense-in-depth enforcement.

### For Photos

- Family membership check: users must belong to the photo's family
- Permission format: `photos:upload`, `photos:delete`
- Owner/Admin can delete any family photo
- All family members can view and upload
- Frontend: HIDE unauthorized actions (delete button only for owners/admins)
