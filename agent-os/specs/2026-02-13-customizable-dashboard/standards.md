# Standards for Customizable Dashboard

The following standards apply to this work.

---

## architecture/ddd-modules

DDD module structure with bounded contexts. Each module is self-contained.

### Module Layout

```
Features/Dashboard/
├── Domain/
│   ├── Entities/          # DashboardLayout (aggregate), DashboardWidget
│   ├── ValueObjects/      # DashboardId, DashboardLayoutName, DashboardWidgetId, WidgetTypeId
│   ├── Events/            # DashboardCreatedEvent
│   └── Repositories/      # IDashboardLayoutRepository
├── Application/
│   ├── Commands/          # SaveDashboardLayout, AddWidget, RemoveWidget, UpdateWidgetConfig, ResetDashboard
│   ├── Queries/           # GetAvailableWidgets, GetMyDashboard, GetFamilyDashboard
│   └── Mappers/           # DashboardMapper
├── Infrastructure/
│   └── Repositories/      # DashboardLayoutRepository
├── Data/                  # EF Core configurations
├── Models/                # DTOs (DashboardLayoutDto, DashboardWidgetDto, WidgetDescriptorDto)
└── DashboardModule.cs     # IModule implementation
```

### Cross-Module Communication

- Widget infrastructure (`Common/Widgets/`) is cross-cutting — any module can register widgets
- Reference IDs only (UserId, FamilyId from other modules)
- No direct dependencies on other module internals

---

## backend/graphql-input-command

Separate Input DTOs (primitives) from Mediator Commands (Vogen types). See ADR-003.

### Applied to Dashboard

```csharp
// Input (primitives) — GraphQL layer
public sealed record SaveDashboardLayoutRequest
{
    public required string Name { get; init; }
    public required bool IsShared { get; init; }
    public required List<WidgetPositionInput> Widgets { get; init; }
}

// Command (Vogen types) — Domain layer
public sealed record SaveDashboardLayoutCommand(
    DashboardLayoutName Name,
    UserId? UserId,
    FamilyId? FamilyId,
    bool IsShared,
    IReadOnlyList<WidgetPositionData> Widgets
) : ICommand<SaveDashboardLayoutResult>;
```

### File Organization

```
Commands/SaveDashboardLayout/
  SaveDashboardLayoutCommand.cs
  SaveDashboardLayoutCommandHandler.cs
  SaveDashboardLayoutCommandValidator.cs
  SaveDashboardLayoutResult.cs
  MutationType.cs
```

---

## backend/permission-system

Role-based permissions with defense-in-depth enforcement.

### Applied to Dashboard

- Widget descriptors declare `RequiredPermissions` (e.g., `["family:view"]`)
- `GetAvailableWidgets` query filters widgets by user's permissions
- Shared dashboard mutations require admin/owner role
- Personal dashboard mutations require authenticated user
- Frontend: hide widgets user lacks permissions for (never disable)

### New Permissions (Future)

```
dashboard:edit-shared    — Edit the shared family dashboard
dashboard:view-shared    — View the shared family dashboard
```

For V1, shared dashboard editing uses existing `family:edit` permission.

---

## backend/domain-events

Domain events as sealed records extending `DomainEvent`.

### Applied to Dashboard

```csharp
public sealed record DashboardCreatedEvent(
    DashboardId DashboardId,
    UserId CreatedByUserId,
    bool IsShared) : DomainEvent;
```

- Raised in `DashboardLayout.CreatePersonal()` and `DashboardLayout.CreateShared()` factory methods
- Location: `Features/Dashboard/Domain/Events/DashboardCreatedEvent.cs`
- Future: Can trigger EventChain automations (e.g., auto-add widgets on family creation)

---

## database/ef-core-migrations

EF Core migrations with Data/ folder for configurations.

### Applied to Dashboard

- Schema: `dashboard` (one PostgreSQL schema per module)
- Tables: `dashboard_layouts`, `dashboard_widgets`
- Configurations in `Features/Dashboard/Data/`
- Two migrations: schema creation + RLS policies
- Auto-applied in development via `db.Database.MigrateAsync()`
- CASCADE delete on widgets when layout is deleted

---

## database/rls-policies

PostgreSQL Row-Level Security for multi-tenancy.

### Applied to Dashboard

```sql
-- dashboard_layouts: OR logic for dual ownership
CREATE POLICY dashboard_layout_policy ON dashboard.dashboard_layouts
    FOR ALL
    USING (
        ("user_id"::text = current_setting('app.current_user_id', true))
        OR
        ("family_id"::text = current_setting('app.current_family_id', true))
    );

-- dashboard_widgets: inherited via subquery join
CREATE POLICY dashboard_widget_policy ON dashboard.dashboard_widgets
    FOR ALL
    USING ("dashboard_id" IN (
        SELECT "id" FROM dashboard.dashboard_layouts
        WHERE ("user_id"::text = current_setting('app.current_user_id', true))
           OR ("family_id"::text = current_setting('app.current_family_id', true))
    ));
```

Session variables set by `PostgresRlsMiddleware` (existing infrastructure).

---

## frontend/angular-components

Standalone components with `inject()` DI and computed signals.

### Applied to Dashboard

- All widget components are `standalone: true`
- Use `inject()` for DI (never constructor injection)
- Use `signal()` for mutable state, `computed()` for derived state
- Use `input()` and `output()` for component communication
- `NgComponentOutlet` for dynamic widget rendering
- No NgModules — everything is standalone

### Widget Component Pattern

```typescript
@Component({
  selector: 'app-family-overview-widget',
  standalone: true,
  imports: [CommonModule],
  template: `...`
})
export class FamilyOverviewWidgetComponent implements DashboardWidgetComponent {
  widgetConfig = signal<Record<string, unknown> | null>(null);

  private familyService = inject(FamilyService);
  members = signal<FamilyMemberDto[]>([]);
  isLoading = signal(true);
}
```

---

## frontend/apollo-graphql

Apollo Client with typed GraphQL operations.

### Applied to Dashboard

```typescript
// Typed operations in graphql/dashboard.operations.ts
const GET_MY_DASHBOARD = gql`
  query GetMyDashboard {
    dashboard {
      myDashboard { id name isShared widgets { ... } }
    }
  }
`;

// Service pattern
@Injectable({ providedIn: 'root' })
export class DashboardService {
  private apollo = inject(Apollo);

  getMyDashboard() {
    return this.apollo.query<GetMyDashboardResult>({
      query: GET_MY_DASHBOARD
    }).pipe(map(r => r.data.dashboard.myDashboard));
  }
}
```

---

## testing/unit-testing

xUnit + FluentAssertions with fake repository pattern.

### Applied to Dashboard

- `FakeDashboardLayoutRepository` in `tests/FamilyHub.TestCommon/Fakes/`
- Aggregate tests: factory methods, AddWidget, RemoveWidget, domain events
- Handler tests: create handler with fakes, call `Handle()` directly
- Value object tests: validation rules
- Registry tests: RegisterProvider, query methods

### Test Pattern

```csharp
[Fact]
public async Task Handle_ShouldCreateNewDashboard_WhenNoneExists()
{
    // Arrange
    var repo = new FakeDashboardLayoutRepository();
    var registry = new WidgetRegistry();
    registry.RegisterProvider(new TestWidgetProvider());
    var handler = new SaveDashboardLayoutCommandHandler(repo, registry);
    var command = new SaveDashboardLayoutCommand(...);

    // Act
    var result = await handler.Handle(command, CancellationToken.None);

    // Assert
    result.Should().NotBeNull();
    repo.AddedLayouts.Should().ContainSingle();
}
```
