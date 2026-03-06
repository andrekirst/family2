# School Module — Shaping Notes

**Feature**: New School module with mark-as-student functionality
**Created**: 2026-03-06
**GitHub Issue**: #217

---

## Scope

Introduce a new **School** module to Family Hub. The first feature allows family Owners and Admins to mark family members as students ("Schüler"). The module follows the same IModule pattern as the Family module.

**What's included:**

- Backend: Student aggregate root, MarkAsStudent command, GetStudents query, GraphQL API
- Frontend: School navigation item, school page with student list, mark-as-student dialog
- Database: `school.students` table with unique constraint on family_member_id
- Tests: Domain tests, command handler tests, query handler tests

**What's NOT included:**

- Unmarking/removing student status
- Additional student data (school name, grade, enrollment date)
- Student-specific permissions beyond "can manage students"

## Decisions

- **Student as Aggregate Root:** Even though it's essentially a link entity, Student is its own aggregate in the School bounded context. This preserves module separation — School doesn't modify Family entities.
- **Permission model:** Reuse FamilyRole with a new `CanManageStudents()` method (Owner & Admin). Add `"school:manage-students"` to GetPermissions(). Frontend uses FamilyPermissionService.
- **Uniqueness:** One student record per family member, enforced at DB level (UNIQUE constraint on family_member_id) and application level (ExistsByFamilyMemberIdAsync check).
- **No age/role restrictions:** Any family member can be marked as a student.
- **Icon:** New Heroicons academic-cap SVG for sidebar navigation.
- **Navigation placement:** After "Family", before "Calendar" in sidebar.

## Context

- **Visuals:** None
- **References:** Family module (primary pattern source for all layers)
- **Product alignment:** Phase 1 - Core MVP. School is a natural extension of the family management domain.

## Standards Applied

- **ddd-modules** — Feature-folder layout with IModule, bounded context separation
- **graphql-input-command** — Input DTO (MarkAsStudentRequest) separate from Command (MarkAsStudentCommand), subfolder-per-command
- **vogen-value-objects** — StudentId as Vogen ValueObject<Guid> with EfCoreValueConverter
- **permission-system** — FamilyRole.CanManageStudents(), "school:manage-students" permission string, hide unauthorized UI
- **angular-components** — Standalone components with signals and inject() DI
- **apollo-graphql** — Typed GraphQL operations with gql tagged templates
- **unit-testing** — xUnit + FluentAssertions, fake repositories, Arrange-Act-Assert
- **ef-core-migrations** — DbUp SQL migration, schema "school", IEntityTypeConfiguration
