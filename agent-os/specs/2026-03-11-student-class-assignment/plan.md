# Student Class Assignment with School Management

**Created**: 2026-03-11
**GitHub Issue**: #230
**Builds on**: School Module Spec #217 (mark-as-student implemented)
**Spec**: `agent-os/specs/2026-03-11-student-class-assignment/`

## Context

Family Hub's School module currently only supports marking family members as students (#217). This feature expands it with full school management: creating schools (with federal state + location), school years (with federal-state-specific start/end dates), and class assignments that track a student's enrollment history. Additionally, a shared Address value object is added to `FamilyHub.Common` for family member profiles.

**Scope decisions:**

- Builds on top of #217 (mark-as-student assumed implemented)
- Schools are per-family (future: global school registry)
- School years have explicit start/end dates per federal state
- Class assignment history (current determined by SchoolYear date range)
- Address is a shared value object in FamilyHub.Common
- Delete protection for Schools and SchoolYears in use

## Domain Model

### School (Entity in School module)

- `SchoolId` (Vogen), `FamilyId`, `SchoolName` (Vogen string), `FederalStateId` (from BaseData), `City` (string), `PostalCode` (string)
- CreatedAt, UpdatedAt

### SchoolYear (Entity in School module)

- `SchoolYearId` (Vogen), `FamilyId`, `FederalStateId` (from BaseData)
- `StartYear` (int), `EndYear` (int), `StartDate` (DateOnly), `EndDate` (DateOnly)
- Each federal state has different school year start/end dates
- CreatedAt, UpdatedAt

### ClassAssignment (AggregateRoot in School module)

- `ClassAssignmentId` (Vogen), `StudentId` (FK), `SchoolId` (FK), `SchoolYearId` (FK)
- `ClassName` (Vogen string, e.g. "1a"), `FamilyId`, `AssignedAt`, `AssignedByUserId`
- "Current" = SchoolYear.StartDate <= today <= SchoolYear.EndDate

### Address (Shared VO in FamilyHub.Common)

- `Street`, `HouseNumber`, `PostalCode`, `City`, `Country`, `FederalStateId` (nullable)
- EF Core owned type on FamilyMember (address_* columns in family.family_members table)

## GraphQL API

### Queries (extend SchoolQuery)

- `schools: [SchoolDto!]!` — all schools for the current family
- `schoolYears: [SchoolYearDto!]!` — all school years for the current family
- `studentClassAssignments(studentId: ID!): [ClassAssignmentDto!]!` — assignments with `isCurrent` flag

### Mutations (extend SchoolMutation)

- `createSchool(input: CreateSchoolInput!): SchoolDto!`
- `updateSchool(input: UpdateSchoolInput!): SchoolDto!`
- `deleteSchool(input: DeleteSchoolInput!): Boolean!` — fails if school is used by ClassAssignment
- `createSchoolYear(input: CreateSchoolYearInput!): SchoolYearDto!`
- `updateSchoolYear(input: UpdateSchoolYearInput!): SchoolYearDto!`
- `deleteSchoolYear(input: DeleteSchoolYearInput!): Boolean!` — fails if school year is used
- `assignStudentToClass(input: AssignStudentToClassInput!): ClassAssignmentDto!`
- `updateClassAssignment(input: UpdateClassAssignmentInput!): ClassAssignmentDto!`
- `removeClassAssignment(input: RemoveClassAssignmentInput!): Boolean!`

## Files to Modify

### New Files (~100+)

**Backend — Value Objects** (`Features/School/Domain/ValueObjects/`):

- `SchoolId.cs`, `SchoolYearId.cs`, `ClassAssignmentId.cs`, `SchoolName.cs`, `ClassName.cs`

**Backend — Entities** (`Features/School/Domain/Entities/`):

- `School.cs`, `SchoolYear.cs`, `ClassAssignment.cs`

**Backend — Domain Events** (`Features/School/Domain/Events/`):

- `SchoolCreatedEvent.cs`, `SchoolYearCreatedEvent.cs`
- `StudentAssignedToClassEvent.cs`, `ClassAssignmentUpdatedEvent.cs`, `ClassAssignmentRemovedEvent.cs`

**Backend — Repositories** (`Features/School/Domain/Repositories/`):

- `ISchoolRepository.cs`, `ISchoolYearRepository.cs`, `IClassAssignmentRepository.cs`

**Backend — EF Configurations** (`Features/School/Data/`):

- `SchoolConfiguration.cs`, `SchoolYearConfiguration.cs`, `ClassAssignmentConfiguration.cs`

**Backend — Repository Implementations** (`Features/School/Infrastructure/Repositories/`):

- `SchoolRepository.cs`, `SchoolYearRepository.cs`, `ClassAssignmentRepository.cs`

**Backend — Commands** (9 command folders, ~54 files):

- `CreateSchool/`, `UpdateSchool/`, `DeleteSchool/`
- `CreateSchoolYear/`, `UpdateSchoolYear/`, `DeleteSchoolYear/`
- `AssignStudentToClass/`, `UpdateClassAssignment/`, `RemoveClassAssignment/`

**Backend — Queries** (3 query folders, ~9 files):

- `GetSchools/`, `GetSchoolYears/`, `GetStudentClassAssignments/`

**Backend — Models/DTOs** (~12 files):

- `SchoolDto.cs`, `SchoolYearDto.cs`, `ClassAssignmentDto.cs`
- Request models for each mutation
- `SchoolMapper.cs`, `SchoolYearMapper.cs`, `ClassAssignmentMapper.cs`

**Shared — Address VO:**

- `src/FamilyHub.Common/Domain/ValueObjects/Address.cs`

**Database Migrations:**

- `20260311000001_add-address-to-family-members.sql` (family schema)
- `20260311000002_create-schools-table.sql` (school schema)
- `20260311000003_create-school-years-table.sql` (school schema)
- `20260311000004_create-class-assignments-table.sql` (school schema)

**Frontend — New Components** (7 folders):

- `student-card/` — card with avatar, name, current school, current class
- `student-detail-page/` — inline assignment (GitHub milestone picker style)
- `schools-page/` — CRUD management
- `school-form-dialog/` — create/edit school
- `school-years-page/` — CRUD management
- `school-year-form-dialog/` — create/edit school year
- `assign-class-dialog/` — assign student to class

**Tests** (~20 files):

- Fake repositories: `FakeSchoolRepository.cs`, `FakeSchoolYearRepository.cs`, `FakeClassAssignmentRepository.cs`
- Domain tests: `SchoolEntityTests.cs`, `SchoolYearEntityTests.cs`, `ClassAssignmentAggregateTests.cs`
- Handler tests for all commands and queries

### Modified Files (~18)

- `AppDbContext.cs` — add 3 new DbSets
- `SchoolModule.cs` — register new repositories
- `DomainErrorCodes.cs` — add School error codes
- `FamilyMember.cs` — add Address property
- `FamilyMemberConfiguration.cs` — add OwnsOne mapping
- `FamilyRole.cs` — add CanManageSchools() permission
- `StudentDto.cs` — add CurrentSchoolName, CurrentClassName
- `GetStudentsQueryHandler.cs` — enrich with current class info
- `StudentMapper.cs` — map new fields
- `school.operations.ts` — add new GraphQL operations
- `school.service.ts` — add service methods
- `school-page.component.*` — add sub-navigation tabs
- `student-list.component.*` — refactor to use student-card
- `school.routes.ts` — add child routes
- `family-permission.service.ts` — add canManageSchools signal
- `VogenCustomization.cs` — register new VOs

## Implementation Tasks

### Task 1: Save Spec, Commit, and Create GitHub Issue

1. Write spec files to `agent-os/specs/2026-03-11-student-class-assignment/`
2. Create GitHub issue with labels: type-feature, status-planning, phase-1, priority-p2, effort-xl
3. Update spec files with issue number
4. Git commit

### Task 2: Shared Address Value Object

- `Address.cs` in FamilyHub.Common
- FamilyMember.Address property + EF owned type configuration
- DB migration for address columns

### Task 3: Backend Domain Layer — Value Objects & Entities

- 5 VOs, 3 entities, 5 domain events

### Task 4: Backend Repositories & Data Layer

- 3 repo interfaces, 3 EF configs, 3 repo implementations, 3 DB migrations
- AppDbContext, SchoolModule, DomainErrorCodes modifications

### Task 5: Backend Application Layer — School CRUD

- CreateSchool, UpdateSchool, DeleteSchool (with delete protection), GetSchools
- SchoolDto, request models, mapper

### Task 6: Backend Application Layer — SchoolYear CRUD

- Same pattern for SchoolYear entities

### Task 7: Backend Application Layer — ClassAssignment

- AssignStudentToClass, UpdateClassAssignment, RemoveClassAssignment
- GetStudentClassAssignments with isCurrent computation
- Enrich existing GetStudents query

### Task 8: Frontend — GraphQL Operations & Service

- All new operations + TypeScript interfaces

### Task 9: Frontend — Student Cards & Detail Page

- Student card component, detail page with inline assignment

### Task 10: Frontend — School & SchoolYear Management Pages

- CRUD pages with sub-navigation, delete protection

### Task 11: Backend Tests

- 3 fake repos, ~14 test files, VogenCustomization updates
