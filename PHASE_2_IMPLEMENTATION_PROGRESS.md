# Phase 2.A+2.B Implementation Progress Report

**Date:** 2026-01-04
**Status:** IN PROGRESS (70% Complete)

---

## Completed Tasks ✅

### Task 5: Domain Method Updates (100% DONE)

- ✅ Added `User.UpdateRole(UserRole newRole)` method
- ✅ Added `FamilyMemberInvitation.UpdateRole(UserRole newRole)` method with validation
- **Files Modified:**
  - `/src/api/Modules/FamilyHub.Modules.Auth/Domain/User.cs`
  - `/src/api/Modules/FamilyHub.Modules.Auth/Domain/FamilyMemberInvitation.cs`

### Task 6: Repository Extension (100% DONE)

- ✅ Added `GetByFamilyIdAsync()` to `IUserRepository`
- ✅ Implemented `GetByFamilyIdAsync()` in `UserRepository`
- **Files Modified:**
  - `/src/api/Modules/FamilyHub.Modules.Auth/Domain/Repositories/IUserRepository.cs`
  - `/src/api/Modules/FamilyHub.Modules.Auth/Persistence/Repositories/UserRepository.cs`

### Task 1: UpdateInvitationRole Command (90% DONE)

- ✅ Created `UpdateInvitationRoleCommand.cs`
- ✅ Created `UpdateInvitationRoleResult.cs`
- ✅ Created `UpdateInvitationRoleCommandHandler.cs`
- ✅ Created `UpdateInvitationRolePayloadFactory.cs`
- ⚠️ **Remaining:** Add mutation to `InvitationMutations.cs`
- **Files Created:**
  - `/src/api/Modules/FamilyHub.Modules.Auth/Application/Commands/UpdateInvitationRole/UpdateInvitationRoleCommand.cs`
  - `/src/api/Modules/FamilyHub.Modules.Auth/Application/Commands/UpdateInvitationRole/UpdateInvitationRoleResult.cs`
  - `/src/api/Modules/FamilyHub.Modules.Auth/Application/Commands/UpdateInvitationRole/UpdateInvitationRoleCommandHandler.cs`
  - `/src/api/Modules/FamilyHub.Modules.Auth/Presentation/GraphQL/Factories/UpdateInvitationRolePayloadFactory.cs`
- **Note:** Input and Payload files already exist from previous work

---

## Remaining Tasks 📋

### CRITICAL BUILD FIXES NEEDED FIRST

**Problem:** Project currently has build errors due to missing command implementations that are referenced by existing mutations/factories.

**Required Fix:**

1. ResendInvitation command files don't exist but are referenced in:
   - `ResendInvitationPayloadFactory.cs`
   - `InvitationMutations.cs` (line 5, 113-120)

2. CancelInvitation command files exist but missing handler implementation

3. AcceptInvitation and BatchInviteFamilyMembers directories are empty

**Immediate Action Required:**

- Create stub/placeholder implementations for ResendInvitation OR
- Comment out references in mutations until command is implemented OR
- Implement full ResendInvitation command (30 min estimated)

---

### Task 1: UpdateInvitationRole (10% remaining)

**What's left:**

- Add mutation method to `InvitationMutations.cs`

**Implementation:**

```csharp
[Authorize(Policy = "RequireOwnerOrAdmin")]
public async Task<UpdateInvitationRolePayload> UpdateInvitationRole(
    UpdateInvitationRoleInput input,
    [Service] IMutationHandler mutationHandler,
    [Service] IMediator mediator,
    CancellationToken cancellationToken)
{
    return await mutationHandler.Handle<UpdateInvitationRoleResult, UpdateInvitationRolePayload>(async () =>
    {
        var command = new UpdateInvitationRoleCommand(
            InvitationId: InvitationId.From(input.InvitationId),
            NewRole: UserRole.From(input.NewRole)
        );
        return await mediator.Send(command, cancellationToken);
    });
}
```

**Estimated Time:** 10 minutes

---

### Task 2: AcceptInvitation Command (NOT STARTED)

**Files to Create:**

1. `AcceptInvitationCommand.cs`
2. `AcceptInvitationResult.cs`
3. `AcceptInvitationCommandHandler.cs`
4. `AcceptInvitationInput.cs`
5. `AcceptInvitationPayload.cs`
6. `AcceptInvitationPayloadFactory.cs`
7. Mutation in `InvitationMutations.cs`

**Business Logic:**

- No `[Authorize]` attribute (public endpoint)
- Validates token is valid
- Validates authenticated user email matches invitation email
- Calls `invitation.Accept(userId)` domain method
- Updates user role via `user.UpdateRole(invitation.Role)`
- Returns `FamilyId`, `FamilyName`, `Role`

**Estimated Time:** 45 minutes

---

### Task 3: BatchInviteFamilyMembers Command (NOT STARTED)

**Complexity:** HIGH - implements two-phase validation pattern

**Files to Create:**

1. `BatchInviteFamilyMembersCommand.cs`
2. `BatchInviteFamilyMembersResult.cs`
3. `BatchInviteFamilyMembersCommandHandler.cs`
4. `BatchInviteFamilyMembersInput.cs`
5. `BatchInviteFamilyMembersPayload.cs`
6. `BatchInviteFamilyMembersPayloadFactory.cs`
7. Mutation in `InvitationMutations.cs`

**Implementation Pattern:**

```csharp
// PHASE 1: VALIDATE ALL
var validationErrors = new List<UserError>();

foreach (var emailInput in request.EmailInvitations)
{
    // Check duplicates, existing members
    if (/* validation fails */)
        validationErrors.Add(new UserError("code", "message"));
}

foreach (var managedInput in request.ManagedAccounts)
{
    // Check username duplicates
    if (/* validation fails */)
        validationErrors.Add(new UserError("code", "message"));
}

if (validationErrors.Any())
    return Result.Failure<BatchInvitationResult>("Validation failed");

// PHASE 2: COMMIT ALL (atomic transaction)
using var transaction = await _unitOfWork.BeginTransactionAsync(cancellationToken);

try
{
    // Create all invitations
    // Create all managed accounts
    await _unitOfWork.SaveChangesAsync(cancellationToken);
    await transaction.CommitAsync(cancellationToken);
}
catch (Exception ex)
{
    await transaction.RollbackAsync(cancellationToken);
    return Result.Failure<BatchInvitationResult>(ex.Message);
}
```

**Dependencies:**

- `IZitadelManagementClient`
- `IPasswordGenerationService`
- `IFamilyMemberInvitationRepository`
- `IUserRepository`
- `IFamilyRepository`

**Estimated Time:** 90 minutes

---

### Task 4: GraphQL Queries (NOT STARTED)

**File to Create:**

- `/src/api/Modules/FamilyHub.Modules.Auth/Presentation/GraphQL/Queries/InvitationQueries.cs`

**4 Queries Required:**

1. **FamilyMembers(familyId)** - Returns `List<FamilyMemberType>`
   - Auth: `[Authorize]` (any family member)
   - Uses `userRepository.GetByFamilyIdAsync()`
   - Maps `User` → `FamilyMemberType`

2. **PendingInvitations(familyId)** - Returns `List<PendingInvitationType>`
   - Auth: `[Authorize(Policy = "RequireOwnerOrAdmin")]`
   - Uses `invitationRepository.GetPendingByFamilyIdAsync()`

3. **Invitation(invitationId)** - Returns `PendingInvitationType?`
   - Auth: `[Authorize(Policy = "RequireOwnerOrAdmin")]`
   - Uses `invitationRepository.GetByIdAsync()`

4. **InvitationByToken(token)** - Returns `PendingInvitationType?`
   - Auth: NONE (public for acceptance flow)
   - Uses `invitationRepository.GetByTokenAsync()`
   - Returns limited info (no token, no display code)

**Estimated Time:** 60 minutes

---

### Task 7: DI Registration (NOT STARTED)

**File to Modify:**

- `/src/api/Modules/FamilyHub.Modules.Auth/AuthModuleServiceRegistration.cs`

**Required Registrations:**

```csharp
// Repository (if not already registered)
services.AddScoped<IFamilyMemberInvitationRepository, FamilyMemberInvitationRepository>();

// Payload Factories (add missing ones)
services.AddScoped<IPayloadFactory<UpdateInvitationRoleResult, UpdateInvitationRolePayload>, UpdateInvitationRolePayloadFactory>();
services.AddScoped<IPayloadFactory<AcceptInvitationResult, AcceptInvitationPayload>, AcceptInvitationPayloadFactory>();
services.AddScoped<IPayloadFactory<BatchInvitationResult, BatchInviteFamilyMembersPayload>, BatchInviteFamilyMembersPayloadFactory>();

// Authorization Policy (if not exists)
services.AddAuthorization(options =>
{
    options.AddPolicy("RequireOwnerOrAdmin", policy =>
        policy.RequireAssertion(context =>
            context.User.HasClaim("role", "Owner") ||
            context.User.HasClaim("role", "Admin")));
});
```

**Estimated Time:** 15 minutes

---

## Build Errors to Fix

### Current Errors (10 errors)

1. **ResendInvitation namespace not found** (2 occurrences)
   - `ResendInvitationPayloadFactory.cs:1`
   - `InvitationMutations.cs:5`

2. **InvitationId type not found** (2 occurrences)
   - `CancelInvitationCommand.cs:12`
   - `UpdateInvitationRoleCommand.cs:11`
   - `UpdateInvitationRoleResult.cs:10`

**Fix:** These are missing `using FamilyHub.Modules.Auth.Domain.ValueObjects;` statements

1. **ResendInvitationResult not found** (2 occurrences)
   - `ResendInvitationPayloadFactory.cs:12,14`

**Fix:** Create the ResendInvitation command files OR comment out factory/mutation

---

## Testing Status

**Unit Tests:** NOT CREATED YET

**Required Test Files:**

- `UpdateInvitationRoleCommandHandlerTests.cs`
- `AcceptInvitationCommandHandlerTests.cs`
- `BatchInviteFamilyMembersCommandHandlerTests.cs`
- `InvitationQueriesTests.cs`

**Test Patterns:**

- Use `[Theory, AutoNSubstituteData]` for dependency injection
- Use FluentAssertions (`.Should()` syntax)
- Create Vogen value objects manually (`UserId.New()`, `Email.From()`)

**Estimated Time for All Tests:** 2-3 hours

---

## Time Estimates Summary

| Task | Status | Time Remaining |
|------|--------|----------------|
| Task 1 (UpdateInvitationRole) | 90% | 10 min |
| Task 2 (AcceptInvitation) | 0% | 45 min |
| Task 3 (BatchInvite) | 0% | 90 min |
| Task 4 (Queries) | 0% | 60 min |
| Task 5 (Domain Methods) | 100% | 0 min |
| Task 6 (Repository) | 100% | 0 min |
| Task 7 (DI Registration) | 0% | 15 min |
| **Build Fixes** | **CRITICAL** | **30 min** |
| **Unit Tests** | 0% | 2-3 hours |

**Total Remaining:** ~5-6 hours

---

## Recommended Next Steps

### Priority 1: Fix Build (30 min)

1. Add missing `using` statements to fix InvitationId errors
2. Create ResendInvitation command files (or temporarily comment out references)
3. Verify project builds successfully

### Priority 2: Complete Task 1 (10 min)

1. Add UpdateInvitationRole mutation to InvitationMutations.cs
2. Test mutation with GraphQL playground

### Priority 3: Implement Task 2 (45 min)

1. Create AcceptInvitation command, handler, result
2. Create AcceptInvitation GraphQL files
3. Add mutation to InvitationMutations.cs

### Priority 4: Implement Task 4 (60 min)

1. Create InvitationQueries.cs with all 4 queries
2. Register queries in GraphQL schema

### Priority 5: Implement Task 3 (90 min)

1. Create BatchInviteFamilyMembers command with two-phase validation
2. Implement atomic transaction handling
3. Add mutation

### Priority 6: DI Registration (15 min)

1. Register all payload factories
2. Add authorization policy if missing

### Priority 7: Unit Tests (2-3 hours)

1. Test all command handlers
2. Test all queries
3. Test authorization policies

---

## Known Issues & Notes

1. **PendingInvitationType Discrepancy:** Factory files reference `Username` and `DisplayCode` fields that don't exist in current `PendingInvitationType` definition. May need to update type definition or fix factories.

2. **Result<T> Ambiguity:** When using `Result<T>`, always use fully qualified name `FamilyHub.SharedKernel.Domain.Result<T>` to avoid conflicts with `GreenDonut.Result<TValue>` from HotChocolate.

3. **IFamilyMemberInvitationRepository:** Already has all required methods:
   - `GetByIdAsync()`
   - `GetByTokenAsync()`
   - `GetPendingByFamilyIdAsync()`
   - `GetPendingByEmailAsync()`
   - `GetPendingByUsernameAsync()`
   - `GetByFamilyIdAsync()`
   - `AddAsync()`
   - `UpdateAsync()`
   - `IsUserMemberOfFamilyAsync()`

4. **Authorization Policy:** May already exist in `AuthModuleServiceRegistration.cs`. Verify before adding duplicate.

---

## Files Modified in This Session

1. ✅ `/src/api/Modules/FamilyHub.Modules.Auth/Domain/User.cs`
2. ✅ `/src/api/Modules/FamilyHub.Modules.Auth/Domain/FamilyMemberInvitation.cs`
3. ✅ `/src/api/Modules/FamilyHub.Modules.Auth/Domain/Repositories/IUserRepository.cs`
4. ✅ `/src/api/Modules/FamilyHub.Modules.Auth/Persistence/Repositories/UserRepository.cs`
5. ✅ `/src/api/Modules/FamilyHub.Modules.Auth/Application/Commands/UpdateInvitationRole/UpdateInvitationRoleCommand.cs` (CREATED)
6. ✅ `/src/api/Modules/FamilyHub.Modules.Auth/Application/Commands/UpdateInvitationRole/UpdateInvitationRoleResult.cs` (CREATED)
7. ✅ `/src/api/Modules/FamilyHub.Modules.Auth/Application/Commands/UpdateInvitationRole/UpdateInvitationRoleCommandHandler.cs` (CREATED)
8. ✅ `/src/api/Modules/FamilyHub.Modules.Auth/Presentation/GraphQL/Factories/UpdateInvitationRolePayloadFactory.cs` (CREATED)

---

**End of Progress Report**
