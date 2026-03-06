# Standards for School Module

The following standards apply to this work.

---

## architecture/ddd-modules

DDD module structure with bounded contexts (feature-folder layout).

**Key points for School module:**

- Feature-folder layout: `Features/School/` with Domain/, Application/, Data/, Infrastructure/, Models/
- Self-contained IModule with DI registrations
- One PostgreSQL schema per module (`school`)
- Cross-module references by ID only (FamilyId, FamilyMemberId, UserId)

---

## backend/graphql-input-command

ADR-003 pattern separating Input DTOs from Commands with subfolder-per-command layout.

**Key points:**

- `MarkAsStudentRequest` (primitives) in Models/
- `MarkAsStudentCommand` (Vogen types) in Commands/MarkAsStudent/
- One MutationType.cs per command extending SchoolMutation
- Dispatch via `ICommandBus.SendAsync()`

---

## backend/vogen-value-objects

Vogen value objects with EfCoreValueConverter for type safety.

**Key points:**

- `StudentId` as `[ValueObject<Guid>(conversions: Conversions.EfCoreValueConverter)]`
- Validate() rejecting Guid.Empty
- New() factory for ID generation
- EF Core config uses `.HasConversion(new StudentId.EfCoreValueConverter())`

---

## backend/permission-system

Role-based permissions with VO methods, string format, defense-in-depth enforcement.

**Key points:**

- Add `CanManageStudents()` to FamilyRole: `Value is "Owner" or "Admin"`
- Add `"school:manage-students"` to GetPermissions()
- Frontend: add `canManageStudents` computed signal to FamilyPermissionService
- Hide "Mark as Student" button when unauthorized (never disable)

---

## frontend/angular-components

Standalone components with inject() DI and computed signals.

**Key points:**

- All components standalone: true
- Use Angular Signals for state
- inject() for DI (not constructor injection)
- Dialog pattern: signal for visibility, event emitter for actions

---

## frontend/apollo-graphql

Apollo Client with typed GraphQL operations.

**Key points:**

- Separate operations file: `school.operations.ts`
- gql tagged templates for queries and mutations
- Service wraps Apollo with typed methods
- Error handling with catchError

---

## testing/unit-testing

xUnit + FluentAssertions with fake repository pattern.

**Key points:**

- FakeStudentRepository in TestCommon/Fakes/
- Arrange-Act-Assert pattern
- FluentAssertions for all assertions
- Test domain events: `DomainEvents.Should().ContainSingle()`
- Test duplicate prevention: `Should().ThrowAsync<DomainException>()`

---

## database/ef-core-migrations

DbUp SQL migrations with schema separation.

**Key points:**

- Migration file: `Database/Migrations/school/20260306000000_create-school-schema.sql`
- Schema: `school`
- Table: `school.students`
- Unique constraint on `family_member_id`
- Foreign keys to `family.families`, `family.family_members`, `auth.users`
