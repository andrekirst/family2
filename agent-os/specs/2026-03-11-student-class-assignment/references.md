# References for Student Class Assignment

## Similar Implementations

### School Module — Existing (#217)

- **Location:** `src/FamilyHub.Api/Features/School/`
- **Relevance:** This feature builds directly on top of the existing School module
- **Key patterns:**
  - `SchoolModule.cs` — IModule registration, will add new repository registrations
  - `Domain/Entities/Student.cs` — AggregateRoot pattern to replicate for ClassAssignment
  - `Domain/ValueObjects/StudentId.cs` — Vogen VO pattern for SchoolId, SchoolYearId, etc.
  - `Application/Commands/MarkAsStudent/` — Full command pattern to replicate
  - `Application/Queries/GetStudents/` — Query pattern to replicate and enrich
  - `Data/StudentConfiguration.cs` — EF Core configuration pattern
  - `Models/StudentDto.cs` — DTO pattern to extend with CurrentSchoolName, CurrentClassName

### Family Module — Backend

- **Location:** `src/FamilyHub.Api/Features/Family/`
- **Relevance:** Complete module with all layers, primary pattern reference
- **Key patterns:**
  - `Domain/Entities/FamilyMember.cs` — Entity to extend with Address property
  - `Data/FamilyMemberConfiguration.cs` — EF Core config to extend with OwnsOne(Address)
  - `Domain/ValueObjects/FamilyRole.cs` — Permission VO to extend with CanManageSchools()
  - `Application/Commands/CreateFamily/` — CRUD command pattern
  - `Application/Services/FamilyAuthorizationService.cs` — Authorization pattern

### BaseData Module — FederalState

- **Location:** `src/FamilyHub.Api/Features/BaseData/`
- **Relevance:** FederalState entity is referenced by School and SchoolYear
- **Key patterns:**
  - `Domain/Entities/FederalState.cs` — Entity referenced via FederalStateId
  - `Domain/ValueObjects/FederalStateId.cs` — Existing Vogen VO used as FK

### Family Module — Frontend

- **Location:** `src/frontend/family-hub-web/src/app/features/family/`
- **Relevance:** Frontend feature module pattern, card list UI
- **Key patterns:**
  - `components/members-list/` — Card list component to reference for student cards
  - `services/invitation.service.ts` — Apollo service pattern
  - `graphql/family.operations.ts` — GraphQL operations pattern

### School Module — Frontend (Existing)

- **Location:** `src/frontend/family-hub-web/src/app/features/school/`
- **Relevance:** Existing frontend to extend with new pages and components
- **Key patterns:**
  - `components/school-page/` — Main page to add sub-navigation tabs
  - `components/student-list/` — List to refactor with student-card component
  - `components/mark-as-student-dialog/` — Dialog pattern for new dialogs
  - `school.routes.ts` — Routes to add new child routes
  - `school.service.ts` — Service to extend with new methods

### Shared Components

- **Location:** `src/frontend/family-hub-web/src/app/shared/components/`
- **Relevance:** Reusable components for inline editing and dialogs
- **Key patterns:**
  - `inline-edit-text/` — Click-to-edit pattern, reference for inline assignment
  - `confirmation-dialog/` — Modal dialog with variants, reference for delete confirmation

### FamilyHub.Common — Shared Domain

- **Location:** `src/FamilyHub.Common/Domain/`
- **Relevance:** Where the shared Address VO will live
- **Key patterns:**
  - `AggregateRoot.cs` — Base class for ClassAssignment
  - `DomainErrorCodes.cs` — Error code constants to extend
  - `ValueObjects/` — Existing shared VOs (UserId, FamilyId, etc.)

### Test Common

- **Location:** `tests/FamilyHub.TestCommon/`
- **Relevance:** Shared test infrastructure
- **Key patterns:**
  - `Fakes/FakeStudentRepository.cs` — Existing School module fake to reference
  - `Fixtures/VogenCustomization.cs` — AutoFixture customization to extend with new VOs
