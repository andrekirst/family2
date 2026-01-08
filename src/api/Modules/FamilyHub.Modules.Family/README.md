# Family Module

## Current Status (Issue #35)

This module currently serves as a **placeholder** for the future Family bounded context.

### Architecture Note

The Family module structure was created in issue #33, but the actual Family-related code (Domain, Application, Presentation) remains in the **Auth module** due to architectural constraints:

1. **Domain Entities**: `Family` and `FamilyMemberInvitation` are in `FamilyHub.Modules.Auth.Domain`
2. **Application Layer**: Commands, queries, and handlers are in `FamilyHub.Modules.Auth.Application`
3. **Presentation Layer**: GraphQL mutations and queries are in `FamilyHub.Modules.Auth.Presentation`

### Why Not Moved Yet?

Moving the Application layer without the Domain layer would create:
- **Circular dependencies**: Auth → Family → Auth
- **Architectural inconsistency**: Application layer separated from its Domain entities
- **Increased complexity**: Commands operating on entities in a different module

### Future Refactoring (Recommended)

A future issue should move the entire Family bounded context together:

1. **Phase 1**: Extract Domain layer
   - Move `Family` aggregate root to `FamilyHub.Modules.Family.Domain`
   - Move `FamilyMemberInvitation` aggregate root
   - Handle User ↔ Family relationship carefully (consider using FamilyId value object instead of navigation property)

2. **Phase 2**: Extract Application layer
   - Move commands, queries, handlers
   - Move validators
   - Update MediatR registration

3. **Phase 3**: Extract Presentation layer
   - Move GraphQL mutations and queries
   - Update GraphQL registration

4. **Phase 4**: Extract Persistence layer
   - Move repositories
   - Create FamilyDbContext
   - Move EF Core configurations
   - Handle shared database vs separate schemas decision

### Current Module Contents

- `FamilyModuleServiceRegistration.cs`: DI configuration (mostly placeholder)
- `GlobalUsings.cs`: Global using directives for Vogen

### IFamilyService Interface

*Placeholder for future cross-module communication interface*

---

**Related Issues:**
- Issue #33: Created module structure
- Issue #35: Attempted application layer extraction (deferred due to architectural constraints)
- Future issue: Complete Family bounded context extraction
