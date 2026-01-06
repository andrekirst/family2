# Remaining Phase 2 Tasks - Implementation Guide

**Status:** 4/7 Command Handlers Complete, 4/7 Mutations Complete
**Remaining:** 3 Handlers, 3 Mutations, 4 Queries, DI Registration

---

## Task 1: UpdateInvitationRole Command (30 minutes)

### Files to Create

**1.1 Command:**

```csharp
// Location: /src/api/Modules/FamilyHub.Modules.Auth/Application/Commands/UpdateInvitationRole/UpdateInvitationRoleCommand.cs

using FamilyHub.Modules.Auth.Domain.ValueObjects;
using FamilyHub.SharedKernel.Domain;
using MediatR;

namespace FamilyHub.Modules.Auth.Application.Commands.UpdateInvitationRole;

public record UpdateInvitationRoleCommand(
    InvitationId InvitationId,
    UserRole NewRole
) : IRequest<Result<UpdateInvitationRoleResult>>;
```

**1.2 Result:**

```csharp
// UpdateInvitationRoleResult.cs

public record UpdateInvitationRoleResult
{
    public required InvitationId InvitationId { get; init; }
    public required UserRole Role { get; init; }
}
```

**1.3 Handler:**

```csharp
// UpdateInvitationRoleCommandHandler.cs

public sealed partial class UpdateInvitationRoleCommandHandler(
    IFamilyMemberInvitationRepository invitationRepository,
    IUserRepository userRepository,
    IUnitOfWork unitOfWork,
    ICurrentUserService currentUserService,
    ILogger<UpdateInvitationRoleCommandHandler> logger)
    : IRequestHandler<UpdateInvitationRoleCommand, Result<UpdateInvitationRoleResult>>
{
    public async Task<Result<UpdateInvitationRoleResult>> Handle(
        UpdateInvitationRoleCommand request,
        CancellationToken cancellationToken)
    {
        // 1. Get authenticated user
        var currentUserId = await _currentUserService.GetUserIdAsync(cancellationToken);

        // 2. Get invitation
        var invitation = await _invitationRepository.GetByIdAsync(request.InvitationId, cancellationToken);
        if (invitation == null)
            return Result.Failure<UpdateInvitationRoleResult>("Invitation not found.");

        // 3. Validate user is owner or admin
        var currentUser = await _userRepository.GetByIdAsync(currentUserId, cancellationToken);
        if (currentUser?.Role != UserRole.Owner && currentUser?.Role != UserRole.Admin)
            return Result.Failure<UpdateInvitationRoleResult>("Only OWNER or ADMIN can update invitation roles.");

        // 4. Validate new role (cannot update to OWNER)
        if (request.NewRole == UserRole.Owner)
            return Result.Failure<UpdateInvitationRoleResult>("Cannot update invitation role to OWNER.");

        // 5. Validate invitation is pending
        if (invitation.Status != InvitationStatus.Pending)
            return Result.Failure<UpdateInvitationRoleResult>("Can only update pending invitations.");

        // 6. Update role (add UpdateRole method to FamilyMemberInvitation domain)
        invitation.UpdateRole(request.NewRole); // TODO: Add this method to domain

        // 7. Persist
        await _invitationRepository.UpdateAsync(invitation, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // 8. Return result
        return Result.Success(new UpdateInvitationRoleResult
        {
            InvitationId = invitation.Id,
            Role = invitation.Role
        });
    }
}
```

**1.4 Add UpdateRole method to FamilyMemberInvitation:**

```csharp
// Location: /src/api/Modules/FamilyHub.Modules.Auth/Domain/FamilyMemberInvitation.cs

public void UpdateRole(UserRole newRole)
{
    if (Status != InvitationStatus.Pending)
        throw new InvalidOperationException("Can only update role of pending invitations.");

    if (newRole == UserRole.Owner)
        throw new InvalidOperationException("Cannot update role to OWNER.");

    Role = newRole;

    // Optional: Publish InvitationUpdatedEvent
}
```

**1.5 Payload Factory:**

```csharp
// Location: /src/api/Modules/FamilyHub.Modules.Auth/Presentation/GraphQL/Factories/UpdateInvitationRolePayloadFactory.cs

public class UpdateInvitationRolePayloadFactory
    : IPayloadFactory<UpdateInvitationRoleResult, UpdateInvitationRolePayload>
{
    public UpdateInvitationRolePayload Success(UpdateInvitationRoleResult result)
    {
        var invitation = new PendingInvitationType
        {
            Id = result.InvitationId.Value,
            Role = result.Role.Value,
            // Other properties can be null in partial update
        };
        return new UpdateInvitationRolePayload(invitation);
    }

    public UpdateInvitationRolePayload Error(IReadOnlyList<UserError> errors)
    {
        return new UpdateInvitationRolePayload(errors);
    }
}
```

**1.6 Add to InvitationMutations:**

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

---

## Task 2: AcceptInvitation Command (45 minutes)

### Files to Create

**2.1 Command:**

```csharp
// AcceptInvitationCommand.cs

public record AcceptInvitationCommand(
    InvitationToken Token
) : IRequest<Result<AcceptInvitationResult>>;
```

**2.2 Handler Logic:**

```csharp
public async Task<Result<AcceptInvitationResult>> Handle(...)
{
    // 1. Get invitation by token
    var invitation = await _invitationRepository.GetByTokenAsync(request.Token, cancellationToken);
    if (invitation == null)
        return Result.Failure<AcceptInvitationResult>("Invalid invitation token.");

    // 2. Get authenticated user
    var currentUserId = await _currentUserService.GetUserIdAsync(cancellationToken);
    var currentUser = await _userRepository.GetByIdAsync(currentUserId, cancellationToken);

    // 3. Validate user email matches invitation email
    if (invitation.Email != currentUser.Email)
        return Result.Failure<AcceptInvitationResult>("Invitation email does not match authenticated user.");

    // 4. Accept invitation (domain method handles expiration check)
    try
    {
        invitation.Accept(currentUserId);
    }
    catch (InvalidOperationException ex)
    {
        return Result.Failure<AcceptInvitationResult>(ex.Message);
    }

    // 5. Update user's role to invitation role
    currentUser.UpdateRole(invitation.Role); // TODO: Add UpdateRole to User domain

    // 6. Persist
    await _invitationRepository.UpdateAsync(invitation, cancellationToken);
    await _unitOfWork.SaveChangesAsync(cancellationToken);

    // 7. Return family info
    var family = await _familyRepository.GetByIdAsync(invitation.FamilyId, cancellationToken);
    return Result.Success(new AcceptInvitationResult
    {
        FamilyId = family.Id,
        FamilyName = family.Name,
        Role = invitation.Role
    });
}
```

**2.3 Add UpdateRole to User:**

```csharp
// /src/api/Modules/FamilyHub.Modules.Auth/Domain/User.cs

public void UpdateRole(UserRole newRole)
{
    Role = newRole;
    // Optional: Publish UserRoleChangedEvent
}
```

---

## Task 3: BatchInviteFamilyMembers Command (90 minutes)

**COMPLEX:** Implements two-phase validation pattern.

### Handler Structure

```csharp
public async Task<Result<BatchInvitationResult>> Handle(...)
{
    // PHASE 1: VALIDATE ALL
    var validationErrors = new List<UserError>();

    // Validate all email invitations
    foreach (var emailInput in request.EmailInvitations)
    {
        // Check duplicates, existing members, etc.
        // Add to validationErrors if invalid
    }

    // Validate all managed accounts
    foreach (var managedInput in request.ManagedAccounts)
    {
        // Check username duplicates, etc.
        // Add to validationErrors if invalid
    }

    // If ANY validation errors, return ALL errors
    if (validationErrors.Any())
        return Result.Failure<BatchInvitationResult>("Validation failed");

    // PHASE 2: COMMIT ALL (atomic transaction)
    using var transaction = await _unitOfWork.BeginTransactionAsync(cancellationToken);

    try
    {
        var emailInvitations = new List<FamilyMemberInvitation>();
        var managedAccounts = new List<(User, FamilyMemberInvitation, ManagedAccountCredentials)>();

        // Create all email invitations
        foreach (var emailInput in request.EmailInvitations)
        {
            var invitation = FamilyMemberInvitation.CreateEmailInvitation(...);
            await _invitationRepository.AddAsync(invitation, cancellationToken);
            emailInvitations.Add(invitation);
        }

        // Create all managed accounts
        foreach (var managedInput in request.ManagedAccounts)
        {
            var password = _passwordService.GeneratePassword(...);
            var zitadelUser = await _zitadelClient.CreateHumanUserAsync(...);
            var user = User.CreateManagedAccount(...);
            var invitation = FamilyMemberInvitation.CreateManagedAccountInvitation(...);

            await _userRepository.AddAsync(user, cancellationToken);
            await _invitationRepository.AddAsync(invitation, cancellationToken);

            managedAccounts.Add((user, invitation, credentials));
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return Result.Success(new BatchInvitationResult
        {
            EmailInvitations = emailInvitations,
            ManagedAccounts = managedAccounts
        });
    }
    catch (Exception ex)
    {
        await transaction.RollbackAsync(cancellationToken);
        return Result.Failure<BatchInvitationResult>($"Batch operation failed: {ex.Message}");
    }
}
```

---

## Task 4: GraphQL Queries (60 minutes)

### Create InvitationQueries.cs

```csharp
// Location: /src/api/Modules/FamilyHub.Modules.Auth/Presentation/GraphQL/Queries/InvitationQueries.cs

[ExtendObjectType("Query")]
public sealed class InvitationQueries
{
    /// <summary>
    /// Get all members of a family.
    /// Requires family membership (any role).
    /// </summary>
    [Authorize]
    public async Task<List<FamilyMemberType>> FamilyMembers(
        Guid familyId,
        [Service] IUserRepository userRepository,
        CancellationToken cancellationToken)
    {
        var users = await userRepository.GetByFamilyIdAsync(FamilyId.From(familyId), cancellationToken);

        return users.Select(u => new FamilyMemberType
        {
            Id = u.Id.Value,
            Email = u.Email.Value,
            Username = u.Username?.Value,
            Role = u.Role.Value,
            JoinedAt = u.CreatedAt,
            IsOwner = u.Role == UserRole.Owner
        }).ToList();
    }

    /// <summary>
    /// Get all pending invitations for a family.
    /// Requires OWNER or ADMIN role.
    /// </summary>
    [Authorize(Policy = "RequireOwnerOrAdmin")]
    public async Task<List<PendingInvitationType>> PendingInvitations(
        Guid familyId,
        [Service] IFamilyMemberInvitationRepository invitationRepository,
        CancellationToken cancellationToken)
    {
        var invitations = await invitationRepository.GetPendingByFamilyIdAsync(
            FamilyId.From(familyId),
            cancellationToken);

        return invitations.Select(i => new PendingInvitationType
        {
            Id = i.Id.Value,
            Email = i.Email?.Value,
            Username = i.Username?.Value,
            Role = i.Role.Value,
            Status = i.Status.Value,
            InvitedAt = i.CreatedAt,
            ExpiresAt = i.ExpiresAt,
            DisplayCode = i.DisplayCode.Value
        }).ToList();
    }

    /// <summary>
    /// Get a single invitation by ID.
    /// Requires OWNER or ADMIN role.
    /// </summary>
    [Authorize(Policy = "RequireOwnerOrAdmin")]
    public async Task<PendingInvitationType?> Invitation(
        Guid invitationId,
        [Service] IFamilyMemberInvitationRepository invitationRepository,
        CancellationToken cancellationToken)
    {
        var invitation = await invitationRepository.GetByIdAsync(
            InvitationId.From(invitationId),
            cancellationToken);

        if (invitation == null)
            return null;

        return new PendingInvitationType
        {
            Id = invitation.Id.Value,
            Email = invitation.Email?.Value,
            Username = invitation.Username?.Value,
            Role = invitation.Role.Value,
            Status = invitation.Status.Value,
            InvitedAt = invitation.CreatedAt,
            ExpiresAt = invitation.ExpiresAt,
            DisplayCode = invitation.DisplayCode.Value
        };
    }

    /// <summary>
    /// Get an invitation by token (for acceptance flow).
    /// No authentication required (public).
    /// </summary>
    public async Task<PendingInvitationType?> InvitationByToken(
        string token,
        [Service] IFamilyMemberInvitationRepository invitationRepository,
        CancellationToken cancellationToken)
    {
        var invitation = await invitationRepository.GetByTokenAsync(
            InvitationToken.From(token),
            cancellationToken);

        if (invitation == null)
            return null;

        // Return limited info for public query (no token, no display code)
        return new PendingInvitationType
        {
            Id = invitation.Id.Value,
            Email = invitation.Email?.Value,
            Role = invitation.Role.Value,
            Status = invitation.Status.Value,
            ExpiresAt = invitation.ExpiresAt,
            InvitedAt = invitation.CreatedAt
        };
    }
}
```

**Note:** Need to add `GetByFamilyIdAsync` to IUserRepository.

---

## Task 5: Dependency Injection Registration (15 minutes)

### Update AuthModuleServiceRegistration.cs

```csharp
// Location: /src/api/Modules/FamilyHub.Modules.Auth/AuthModuleServiceRegistration.cs

public static class AuthModuleServiceRegistration
{
    public static IServiceCollection AddAuthModule(this IServiceCollection services, IConfiguration configuration)
    {
        // ... existing registrations

        // Register repositories
        services.AddScoped<IFamilyMemberInvitationRepository, FamilyMemberInvitationRepository>();

        // Register payload factories
        services.AddScoped<IPayloadFactory<InviteFamilyMemberByEmailResult, InviteFamilyMemberByEmailPayload>, InviteFamilyMemberByEmailPayloadFactory>();
        services.AddScoped<IPayloadFactory<CreateManagedMemberResult, CreateManagedMemberPayload>, CreateManagedMemberPayloadFactory>();
        services.AddScoped<IPayloadFactory<Result, CancelInvitationPayload>, CancelInvitationPayloadFactory>();
        services.AddScoped<IPayloadFactory<ResendInvitationResult, ResendInvitationPayload>, ResendInvitationPayloadFactory>();
        services.AddScoped<IPayloadFactory<UpdateInvitationRoleResult, UpdateInvitationRolePayload>, UpdateInvitationRolePayloadFactory>();
        services.AddScoped<IPayloadFactory<AcceptInvitationResult, AcceptInvitationPayload>, AcceptInvitationPayloadFactory>();
        services.AddScoped<IPayloadFactory<BatchInvitationResult, BatchInviteFamilyMembersPayload>, BatchInviteFamilyMembersPayloadFactory>();

        // Register authorization policy
        services.AddAuthorization(options =>
        {
            options.AddPolicy("RequireOwnerOrAdmin", policy =>
                policy.RequireAssertion(context =>
                    context.User.HasClaim("role", "Owner") ||
                    context.User.HasClaim("role", "Admin")));
        });

        return services;
    }
}
```

---

## Task 6: Add Missing Domain Methods

### User.cs

```csharp
public void UpdateRole(UserRole newRole)
{
    Role = newRole;
}
```

### FamilyMemberInvitation.cs

```csharp
public void UpdateRole(UserRole newRole)
{
    if (Status != InvitationStatus.Pending)
        throw new InvalidOperationException("Can only update role of pending invitations.");

    if (newRole == UserRole.Owner)
        throw new InvalidOperationException("Cannot update role to OWNER.");

    Role = newRole;
}
```

### IUserRepository.cs

```csharp
Task<List<User>> GetByFamilyIdAsync(FamilyId familyId, CancellationToken cancellationToken = default);
```

### UserRepository.cs

```csharp
public async Task<List<User>> GetByFamilyIdAsync(FamilyId familyId, CancellationToken cancellationToken = default)
{
    return await _context.Users
        .Where(u => u.FamilyId == familyId)
        .ToListAsync(cancellationToken);
}
```

---

## Estimated Time Breakdown

- Task 1 (UpdateInvitationRole): 30 minutes
- Task 2 (AcceptInvitation): 45 minutes
- Task 3 (BatchInvite): 90 minutes
- Task 4 (Queries): 60 minutes
- Task 5 (DI Registration): 15 minutes
- Task 6 (Domain Methods): 15 minutes

**Total:** 4 hours 15 minutes

---

## Testing Checklist

After implementation:

- [ ] All 7 command handlers have unit tests
- [ ] All 7 mutations return correct payloads
- [ ] All 4 queries return correct data
- [ ] Authorization policy enforced (OWNER/ADMIN)
- [ ] Batch validation enforces two-phase pattern
- [ ] Credentials returned only once for managed accounts
- [ ] Domain events published correctly
- [ ] EF Core change tracking works for updates

---

**Next:** After completing these tasks, proceed to Phase 3 (Frontend Wizard)
