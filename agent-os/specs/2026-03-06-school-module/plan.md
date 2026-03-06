# School Module — Mark Family Member as Student

**Created**: 2026-03-06
**GitHub Issue**: #217
**Spec**: `agent-os/specs/2026-03-06-school-module/`

## Context

Family Hub needs a new **School** module to manage school-related information for family members. The first feature is marking family members as students ("Schüler"). This introduces a new module end-to-end (backend domain, GraphQL API, database, frontend UI, tests), following the established IModule pattern from the Family module.

**Scope decisions:**

- Only Owner & Admin can mark students
- Just a flag (no school name, grade, etc.)
- Any family member can be a student (no restrictions)
- Unmarking is NOT in scope

## Files to Modify

### New Files (~32)

**Backend Domain:**

- `src/FamilyHub.Api/Features/School/Domain/ValueObjects/StudentId.cs`
- `src/FamilyHub.Api/Features/School/Domain/Entities/Student.cs`
- `src/FamilyHub.Api/Features/School/Domain/Events/FamilyMemberMarkedAsStudentEvent.cs`
- `src/FamilyHub.Api/Features/School/Domain/Repositories/IStudentRepository.cs`

**Backend Data & Infrastructure:**

- `src/FamilyHub.Api/Features/School/Data/StudentConfiguration.cs`
- `src/FamilyHub.Api/Features/School/Infrastructure/Repositories/StudentRepository.cs`

**Backend Module & GraphQL Namespace:**

- `src/FamilyHub.Api/Features/School/SchoolModule.cs`
- `src/FamilyHub.Api/Common/Infrastructure/GraphQL/NamespaceTypes/SchoolQuery.cs`
- `src/FamilyHub.Api/Common/Infrastructure/GraphQL/NamespaceTypes/SchoolMutation.cs`

**Backend Application Layer:**

- `src/FamilyHub.Api/Features/School/Models/StudentDto.cs`
- `src/FamilyHub.Api/Features/School/Models/MarkAsStudentRequest.cs`
- `src/FamilyHub.Api/Features/School/Application/Mappers/StudentMapper.cs`
- `src/FamilyHub.Api/Features/School/Application/Commands/MarkAsStudent/MarkAsStudentCommand.cs`
- `src/FamilyHub.Api/Features/School/Application/Commands/MarkAsStudent/MarkAsStudentCommandHandler.cs`
- `src/FamilyHub.Api/Features/School/Application/Commands/MarkAsStudent/MarkAsStudentCommandValidator.cs`
- `src/FamilyHub.Api/Features/School/Application/Commands/MarkAsStudent/MutationType.cs`
- `src/FamilyHub.Api/Features/School/Application/Queries/GetStudents/GetStudentsQuery.cs`
- `src/FamilyHub.Api/Features/School/Application/Queries/GetStudents/GetStudentsQueryHandler.cs`
- `src/FamilyHub.Api/Features/School/Application/Queries/GetStudents/QueryType.cs`

**Database Migration:**

- `src/FamilyHub.Api/Database/Migrations/school/20260306000000_create-school-schema.sql`

**Frontend:**

- `src/frontend/family-hub-web/src/app/features/school/school.routes.ts`
- `src/frontend/family-hub-web/src/app/features/school/school.providers.ts`
- `src/frontend/family-hub-web/src/app/features/school/graphql/school.operations.ts`
- `src/frontend/family-hub-web/src/app/features/school/services/school.service.ts`
- `src/frontend/family-hub-web/src/app/features/school/components/school-page/school-page.component.ts`
- `src/frontend/family-hub-web/src/app/features/school/components/school-page/school-page.component.html`
- `src/frontend/family-hub-web/src/app/features/school/components/student-list/student-list.component.ts`
- `src/frontend/family-hub-web/src/app/features/school/components/student-list/student-list.component.html`
- `src/frontend/family-hub-web/src/app/features/school/components/mark-as-student-dialog/mark-as-student-dialog.component.ts`
- `src/frontend/family-hub-web/src/app/features/school/components/mark-as-student-dialog/mark-as-student-dialog.component.html`
- `src/frontend/family-hub-web/src/app/shared/icons/defs/academic-cap.ts`

**Tests:**

- `tests/FamilyHub.School.Tests/FamilyHub.School.Tests.csproj`
- `tests/FamilyHub.TestCommon/Fakes/FakeStudentRepository.cs`
- `tests/FamilyHub.School.Tests/Features/School/Domain/StudentAggregateTests.cs`
- `tests/FamilyHub.School.Tests/Features/School/Application/MarkAsStudentCommandHandlerTests.cs`
- `tests/FamilyHub.School.Tests/Features/School/Application/GetStudentsQueryHandlerTests.cs`

### Modified Files (~10)

- `src/FamilyHub.Api/Program.cs` — RegisterModule<SchoolModule>
- `src/FamilyHub.Api/Common/Database/AppDbContext.cs` — DbSet<Student>
- `src/FamilyHub.Api/Common/Infrastructure/GraphQL/NamespaceTypes/RootQuery.cs` — School() entry
- `src/FamilyHub.Api/Common/Infrastructure/GraphQL/NamespaceTypes/RootMutation.cs` — School() entry
- `src/FamilyHub.Api/Features/Family/Domain/ValueObjects/FamilyRole.cs` — CanManageStudents()
- `src/FamilyHub.Api/FamilyHub.slnx` — School test project
- `src/frontend/family-hub-web/src/app/app.routes.ts` — /school route
- `src/frontend/family-hub-web/src/app/app.config.ts` — provideSchoolFeature()
- `src/frontend/family-hub-web/src/app/shared/icons/icons.ts` — ACADEMIC_CAP
- `src/frontend/family-hub-web/src/app/shared/layout/sidebar/sidebar.component.ts` — School nav item

## Implementation Tasks

### Task 1: Save Spec, Commit, and Create GitHub Issue

1. Write spec files to `agent-os/specs/2026-03-06-school-module/`
2. Create GitHub issue with labels
3. Update spec files with issue number
4. Git commit

### Task 2: Backend Domain Layer

- StudentId (Vogen), Student (AggregateRoot), FamilyMemberMarkedAsStudentEvent, IStudentRepository
- Pattern: Family.cs, FamilyId.cs, FamilyCreatedEvent.cs, IFamilyRepository.cs

### Task 3: Backend Data & Infrastructure

- StudentConfiguration (EF Core, schema "school", unique index on FamilyMemberId)
- StudentRepository (EF Core implementation)

### Task 4: Database Migration

- DbUp SQL: CREATE SCHEMA school, CREATE TABLE school.students

### Task 5: Backend Module Registration & Shared Files

- SchoolModule, SchoolQuery/SchoolMutation markers
- Program.cs, AppDbContext, RootQuery, RootMutation modifications

### Task 6: Backend Application Layer

- StudentDto, MarkAsStudentRequest, StudentMapper
- MarkAsStudent command (Command, Handler, Validator, MutationType)
- GetStudents query (Query, Handler, QueryType)
- FamilyRole.CanManageStudents() permission

### Task 7: Frontend Feature Setup

- Routes, providers, GraphQL operations, service
- app.routes.ts and app.config.ts modifications

### Task 8: Frontend Components & Navigation

- SchoolPageComponent, StudentListComponent, MarkAsStudentDialogComponent
- Academic cap icon, sidebar nav item

### Task 9: Tests

- FakeStudentRepository, test project
- StudentAggregateTests, MarkAsStudentCommandHandlerTests, GetStudentsQueryHandlerTests
