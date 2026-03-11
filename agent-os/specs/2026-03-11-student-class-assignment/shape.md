# Student Class Assignment — Shaping Notes

**Feature**: Student class assignment with school and school year management
**Created**: 2026-03-11
**GitHub Issue**: #230
**Builds on**: School Module #217

---

## Scope

Expand the existing School module (which currently only supports marking family members as students) with full school management capabilities:

**What's included:**

- **School entity** — name, federal state (from BaseData), location (city, postal code), per-family scoped
- **SchoolYear entity** — structured start/end years, explicit start/end dates, assigned to a federal state (because each German federal state has different school year calendars)
- **ClassAssignment** — links a student to a school + school year + class name (e.g., "1a"), supports assignment history
- **Student card list** — cards showing avatar, name, current school, current class, link to detail page
- **Student detail page** — inline school/class/school year assignment (GitHub milestone picker style)
- **School management sub-nav** — CRUD with delete protection (can't delete if in use)
- **School year management sub-nav** — CRUD with delete protection (can't delete if in use)
- **Address on FamilyMember** — shared VO in FamilyHub.Common (street, house number, postal code, city, country, federal state)

**What's NOT included:**

- Global school registry (future enhancement)
- Automatic school year date population per federal state
- Student grade/performance tracking
- Teacher/classroom management
- School schedule/timetable

## Decisions

- **Builds on #217:** Mark-as-student is assumed implemented. This spec adds schools, school years, classes, and addresses on top.
- **Address in FamilyHub.Common:** The user chose "Shared/Core" placement. Address is a class (not Vogen VO, since it's multi-field) that lives in `FamilyHub.Common/Domain/ValueObjects/Address.cs`. EF Core maps it as an owned type on FamilyMember.
- **SchoolYear with federal state + explicit dates:** Each German federal state (Bundesland) has different school year start/end dates (e.g., Saxony starts earlier than Bavaria). SchoolYear has StartYear, EndYear, FederalStateId, StartDate (DateOnly), EndDate (DateOnly).
- **Per-family schools:** Each family creates and manages their own schools. Fits the existing multi-tenancy model and RLS pattern. Future: could add a global school registry.
- **Class assignment history:** A student can have multiple ClassAssignments over time. The "current" assignment is determined by which SchoolYear's date range contains today's date. This allows tracking class progression (1a → 2a → 3b).
- **ClassAssignment as AggregateRoot:** It's the core business action and raises domain events (StudentAssignedToClassEvent). School and SchoolYear are plain entities.
- **Delete protection via BusinessValidator + FK:** BusinessValidator checks if any ClassAssignment references the School/SchoolYear before allowing delete. FK constraints provide ultimate safety net.
- **New permission: CanManageSchools():** Added to FamilyRole (Owner & Admin), permission string `"school:manage-schools"`. Reuses existing FamilyPermissionService pattern.
- **Frontend: school page as landing:** Student card list is the main view. Schools and School Years are accessible via sub-navigation tabs.
- **Inline assignment (GitHub-style):** Student detail page uses inline pickers for school and school year (similar to GitHub's milestone/label pickers on issues), with a text input for class name.

## Context

- **Visuals:** None provided
- **References:** Family module (all layers), existing School module (#217), BaseData module (FederalState entity)
- **Product alignment:** Phase 1 - Core MVP. School management is a natural extension of the family domain, building on the already-implemented student marking feature.

## Standards Applied

- **ddd-modules** — Feature-folder layout with bounded contexts, School module self-contained
- **graphql-input-command** — Input DTO → Command separation for all 9 new mutations
- **vogen-value-objects** — SchoolId, SchoolYearId, ClassAssignmentId, SchoolName, ClassName as Vogen VOs
- **permission-system** — FamilyRole.CanManageSchools(), `"school:manage-schools"` permission string, hide unauthorized UI
- **angular-components** — Standalone components with signals for student-card, detail page, management pages
- **apollo-graphql** — Typed GraphQL operations for all new queries and mutations
- **unit-testing** — xUnit + FluentAssertions, fake repositories for 3 new repo interfaces
- **ef-core-migrations** — DbUp SQL migrations in school schema, plus family schema for address columns
