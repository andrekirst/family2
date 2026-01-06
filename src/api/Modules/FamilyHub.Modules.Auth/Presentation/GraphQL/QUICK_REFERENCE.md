# GraphQL Invitation System - Quick Reference

**For:** Developers implementing mutations, queries, and subscriptions
**Version:** 1.0
**Date:** 2026-01-04

---

## File Locations

```
src/api/Modules/FamilyHub.Modules.Auth/Presentation/GraphQL/
├── Inputs/
│   ├── InviteFamilyMemberByEmailInput.cs
│   ├── CreateManagedMemberInput.cs
│   ├── BatchInviteFamilyMembersInput.cs
│   ├── EmailInvitationInput.cs
│   ├── ManagedAccountInput.cs
│   ├── PasswordGenerationConfigInput.cs
│   ├── CancelInvitationInput.cs
│   ├── ResendInvitationInput.cs
│   ├── UpdateInvitationRoleInput.cs
│   └── AcceptInvitationInput.cs
│
├── Payloads/
│   ├── InviteFamilyMemberByEmailPayload.cs
│   ├── CreateManagedMemberPayload.cs
│   ├── BatchInviteFamilyMembersPayload.cs
│   ├── CancelInvitationPayload.cs
│   ├── ResendInvitationPayload.cs
│   ├── UpdateInvitationRolePayload.cs
│   └── AcceptInvitationPayload.cs
│
├── Types/
│   ├── InvitationErrorCode.cs
│   ├── ManagedAccountCredentials.cs
│   ├── ManagedAccountResult.cs
│   ├── ChangeType.cs
│   ├── FamilyMembersChangedPayload.cs
│   └── PendingInvitationsChangedPayload.cs
│
└── Mutations/
    └── InvitationMutations.cs (TO BE IMPLEMENTED)
```

---

## Input → Command Mapping Pattern

**Per ADR-003:** Separate GraphQL Inputs (primitives) from MediatR Commands (Vogen).

```csharp
// GraphQL Input (primitives)
public sealed record InviteFamilyMemberByEmailInput
{
    public required Guid FamilyId { get; init; }
    public required string Email { get; init; }
    public required string Role { get; init; }
    public string? Message { get; init; }
}

// MediatR Command (Vogen value objects)
public sealed record InviteFamilyMemberByEmailCommand(
    FamilyId FamilyId,
    Email Email,
    UserRole Role,
    Message? Message
) : IRequest<InviteFamilyMemberByEmailResult>;

// Mutation method (maps Input → Command)
public async Task<InviteFamilyMemberByEmailPayload> InviteFamilyMemberByEmail(
    InviteFamilyMemberByEmailInput input,
    [Service] IMediator mediator,
    CancellationToken cancellationToken)
{
    try
    {
        var command = new InviteFamilyMemberByEmailCommand(
            FamilyId: FamilyId.From(input.FamilyId),
            Email: Email.From(input.Email),
            Role: Enum.Parse<UserRole>(input.Role),
            Message: input.Message != null ? Message.From(input.Message) : null
        );

        var result = await mediator.Send(command, cancellationToken);

        return new InviteFamilyMemberByEmailPayload(result.Invitation);
    }
    catch (Exception ex)
    {
        return new InviteFamilyMemberByEmailPayload(new[]
        {
            new UserError
            {
                Code = DetermineErrorCode(ex),
                Message = ex.Message,
                Field = DetermineField(ex)
            }
        });
    }
}
```

---

## Error Handling Template

```csharp
private static string DetermineErrorCode(Exception ex)
{
    return ex switch
    {
        VogenValidationException when ex.Message.Contains("email") =>
            nameof(InvitationErrorCode.INVALID_EMAIL_FORMAT),
        VogenValidationException when ex.Message.Contains("username") =>
            nameof(InvitationErrorCode.INVALID_USERNAME_FORMAT),
        DuplicateEmailException =>
            nameof(InvitationErrorCode.DUPLICATE_EMAIL),
        DuplicateUsernameException =>
            nameof(InvitationErrorCode.DUPLICATE_USERNAME),
        FamilyNotFoundException =>
            nameof(InvitationErrorCode.FAMILY_NOT_FOUND),
        UnauthorizedException =>
            nameof(InvitationErrorCode.UNAUTHORIZED),
        ZitadelApiException =>
            nameof(InvitationErrorCode.ZITADEL_API_ERROR),
        _ =>
            nameof(InvitationErrorCode.VALIDATION_FAILED)
    };
}

private static string? DetermineField(Exception ex)
{
    return ex switch
    {
        VogenValidationException when ex.Message.Contains("email") => "email",
        VogenValidationException when ex.Message.Contains("username") => "username",
        VogenValidationException when ex.Message.Contains("familyId") => "familyId",
        _ => null
    };
}
```

---

## Mutation Implementation Checklist

For each mutation:

- [ ] Create Input class in `Inputs/` (primitives only)
- [ ] Create Command class in `Application/Commands/` (Vogen value objects)
- [ ] Create Payload class in `Payloads/` (inherits `PayloadBase`)
- [ ] Create mutation method in `Mutations/InvitationMutations.cs`
- [ ] Map Input → Command using Vogen factory methods
- [ ] Handle exceptions and return payload
- [ ] Add authorization attribute (e.g., `[Authorize(Roles = "OWNER,ADMIN")]`)
- [ ] Write unit tests for command handler
- [ ] Write integration tests for mutation
- [ ] Write E2E tests with Playwright

---

## Query Implementation Checklist

For each query:

- [ ] Create query method in `Queries/InvitationQueries.cs`
- [ ] Query repository/database
- [ ] Map domain entities → GraphQL types
- [ ] Add authorization attribute
- [ ] Handle not found cases
- [ ] Write integration tests

---

## Subscription Implementation Checklist

For each subscription:

- [ ] Create subscription method in `Subscriptions/InvitationSubscriptions.cs`
- [ ] Subscribe to Redis PubSub channel
- [ ] Filter messages by `familyId`
- [ ] Map domain events → GraphQL subscription payloads
- [ ] Add authorization attribute
- [ ] Write E2E tests with WebSocket

---

## Mutation Examples

### 1. Email Invitation

```csharp
[Authorize(Roles = "OWNER,ADMIN")]
public async Task<InviteFamilyMemberByEmailPayload> InviteFamilyMemberByEmail(
    InviteFamilyMemberByEmailInput input,
    [Service] IMediator mediator,
    CancellationToken cancellationToken)
{
    var command = new InviteFamilyMemberByEmailCommand(
        FamilyId.From(input.FamilyId),
        Email.From(input.Email),
        Enum.Parse<UserRole>(input.Role),
        input.Message != null ? Message.From(input.Message) : null
    );

    var result = await mediator.Send(command, cancellationToken);

    return new InviteFamilyMemberByEmailPayload(result.Invitation);
}
```

### 2. Managed Account Creation

```csharp
[Authorize(Roles = "OWNER,ADMIN")]
public async Task<CreateManagedMemberPayload> CreateManagedMember(
    CreateManagedMemberInput input,
    [Service] IMediator mediator,
    CancellationToken cancellationToken)
{
    var command = new CreateManagedMemberCommand(
        FamilyId.From(input.FamilyId),
        Username.From(input.Username),
        FullName.From(input.FullName),
        Enum.Parse<UserRole>(input.Role),
        new PasswordConfig(
            Length: input.PasswordConfig.Length,
            IncludeUppercase: input.PasswordConfig.IncludeUppercase,
            IncludeLowercase: input.PasswordConfig.IncludeLowercase,
            IncludeDigits: input.PasswordConfig.IncludeDigits,
            IncludeSymbols: input.PasswordConfig.IncludeSymbols
        )
    );

    var result = await mediator.Send(command, cancellationToken);

    return new CreateManagedMemberPayload(
        result.Invitation,
        result.User,
        new ManagedAccountCredentials
        {
            Username = result.Username,
            Password = result.Password, // ONLY RETURNED ONCE!
            SyntheticEmail = result.SyntheticEmail,
            LoginUrl = result.LoginUrl
        }
    );
}
```

### 3. Batch Invitation

```csharp
[Authorize(Roles = "OWNER,ADMIN")]
public async Task<BatchInviteFamilyMembersPayload> BatchInviteFamilyMembers(
    BatchInviteFamilyMembersInput input,
    [Service] IMediator mediator,
    CancellationToken cancellationToken)
{
    var command = new BatchInviteFamilyMembersCommand(
        FamilyId.From(input.FamilyId),
        EmailInvitations: input.EmailInvitations.Select(e => new EmailInvitationItem(
            Email.From(e.Email),
            Enum.Parse<UserRole>(e.Role),
            e.Message != null ? Message.From(e.Message) : null
        )).ToList(),
        ManagedAccounts: input.ManagedAccounts.Select(m => new ManagedAccountItem(
            Username.From(m.Username),
            FullName.From(m.FullName),
            Enum.Parse<UserRole>(m.Role),
            new PasswordConfig(
                Length: m.PasswordConfig.Length,
                IncludeUppercase: m.PasswordConfig.IncludeUppercase,
                IncludeLowercase: m.PasswordConfig.IncludeLowercase,
                IncludeDigits: m.PasswordConfig.IncludeDigits,
                IncludeSymbols: m.PasswordConfig.IncludeSymbols
            )
        )).ToList()
    );

    var result = await mediator.Send(command, cancellationToken);

    return new BatchInviteFamilyMembersPayload(
        result.EmailInvitations,
        result.ManagedAccounts.Select(m => new ManagedAccountResult
        {
            User = m.User,
            Credentials = new ManagedAccountCredentials
            {
                Username = m.Username,
                Password = m.Password, // ONLY RETURNED ONCE!
                SyntheticEmail = m.SyntheticEmail,
                LoginUrl = m.LoginUrl
            }
        }).ToList()
    );
}
```

---

## Query Examples

### 1. Family Members

```csharp
[Authorize]
public async Task<List<FamilyMemberType>> FamilyMembers(
    Guid familyId,
    [Service] IFamilyRepository familyRepository,
    ClaimsPrincipal claimsPrincipal)
{
    // 1. Authorize: User must be a member of this family
    var userId = UserId.From(Guid.Parse(claimsPrincipal.FindFirst("sub")!.Value));
    var family = await familyRepository.GetByIdAsync(FamilyId.From(familyId));

    if (family == null || !family.HasMember(userId))
        throw new UnauthorizedException("You are not a member of this family");

    // 2. Query members
    var members = await familyRepository.GetMembersAsync(FamilyId.From(familyId));

    // 3. Map to GraphQL types
    return members.Select(m => new FamilyMemberType
    {
        Id = m.Id.Value,
        Email = m.Email.Value,
        EmailVerified = m.EmailVerified,
        Role = (UserRoleType)m.Role,
        JoinedAt = m.JoinedAt,
        IsOwner = m.Role == UserRole.Owner,
        AuditInfo = new AuditInfoType
        {
            CreatedAt = m.CreatedAt,
            UpdatedAt = m.UpdatedAt
        }
    }).ToList();
}
```

### 2. Pending Invitations

```csharp
[Authorize(Roles = "OWNER,ADMIN")]
public async Task<List<PendingInvitationType>> PendingInvitations(
    Guid familyId,
    [Service] IInvitationRepository invitationRepository)
{
    var invitations = await invitationRepository.GetPendingByFamilyIdAsync(
        FamilyId.From(familyId)
    );

    return invitations.Select(i => new PendingInvitationType
    {
        Id = i.Id.Value,
        Email = i.Email?.Value ?? string.Empty,
        Role = (UserRoleType)i.Role,
        Status = (InvitationStatusType)i.Status,
        InvitedById = i.InvitedByUserId.Value,
        InvitedAt = i.CreatedAt,
        ExpiresAt = i.ExpiresAt,
        IsExpired = DateTime.UtcNow > i.ExpiresAt,
        Message = i.Message?.Value
    }).ToList();
}
```

---

## Subscription Examples

### 1. Family Members Changed

```csharp
[Authorize]
[Subscribe]
public async IAsyncEnumerable<FamilyMembersChangedPayload> FamilyMembersChanged(
    Guid familyId,
    [Service] ITopicEventReceiver receiver,
    ClaimsPrincipal claimsPrincipal,
    [EnumeratorCancellation] CancellationToken cancellationToken)
{
    // 1. Authorize: User must be a member of this family
    var userId = UserId.From(Guid.Parse(claimsPrincipal.FindFirst("sub")!.Value));
    // TODO: Check family membership

    // 2. Subscribe to Redis PubSub channel
    var stream = await receiver.SubscribeAsync<FamilyMembersChangedMessage>(
        $"family-members-changed:{familyId}",
        cancellationToken
    );

    // 3. Yield messages
    await foreach (var message in stream.WithCancellation(cancellationToken))
    {
        yield return new FamilyMembersChangedPayload
        {
            FamilyId = message.FamilyId,
            ChangeType = (ChangeType)message.ChangeType,
            Member = message.Member != null ? new FamilyMemberType
            {
                Id = message.Member.Id,
                Email = message.Member.Email,
                Role = (UserRoleType)message.Member.Role,
                JoinedAt = message.Member.JoinedAt,
                IsOwner = message.Member.Role == UserRole.Owner,
                AuditInfo = new AuditInfoType
                {
                    CreatedAt = message.Member.CreatedAt,
                    UpdatedAt = message.Member.UpdatedAt
                }
            } : null
        };
    }
}
```

---

## Testing Templates

### Unit Test (Command Handler)

```csharp
[Fact]
public async Task InviteFamilyMemberByEmail_Should_Create_Invitation()
{
    // Arrange
    var command = new InviteFamilyMemberByEmailCommand(
        FamilyId.From(Guid.NewGuid()),
        Email.From("jane@example.com"),
        UserRole.Member,
        null
    );

    var handler = new InviteFamilyMemberByEmailCommandHandler(
        _invitationRepository,
        _emailService,
        _timeProvider
    );

    // Act
    var result = await handler.Handle(command, CancellationToken.None);

    // Assert
    result.Invitation.Should().NotBeNull();
    result.Invitation.Email.Value.Should().Be("jane@example.com");
    result.Invitation.Status.Should().Be(InvitationStatus.Pending);
}
```

### Integration Test (Mutation)

```csharp
[Fact]
public async Task InviteFamilyMemberByEmail_Should_Return_Success()
{
    // Arrange
    var client = _factory.CreateClient();
    var query = @"
        mutation($input: InviteFamilyMemberByEmailInput!) {
            inviteFamilyMemberByEmail(input: $input) {
                invitation {
                    id
                    email
                    role
                }
                errors {
                    code
                    message
                }
            }
        }
    ";

    // Act
    var response = await client.PostGraphQLAsync(query, new
    {
        input = new
        {
            familyId = _testFamilyId,
            email = "jane@example.com",
            role = "MEMBER"
        }
    });

    // Assert
    response.Should().NotBeNull();
    response.Data["inviteFamilyMemberByEmail"]["invitation"]["email"]
        .Should().Be("jane@example.com");
    response.Data["inviteFamilyMemberByEmail"]["errors"]
        .Should().BeNull();
}
```

### E2E Test (Playwright)

```typescript
test('should create email invitation', async ({ authenticatedPage, client }) => {
  // Arrange
  const familyId = await createTestFamily(client);

  // Act
  const result = await client.mutate(INVITE_BY_EMAIL_MUTATION, {
    input: {
      familyId: familyId,
      email: 'jane@example.com',
      role: 'MEMBER'
    }
  });

  // Assert
  expect(result.data.inviteFamilyMemberByEmail.invitation).toBeDefined();
  expect(result.data.inviteFamilyMemberByEmail.invitation.email).toBe('jane@example.com');
  expect(result.data.inviteFamilyMemberByEmail.errors).toBeNull();

  // Verify UI update
  await authenticatedPage.goto('/family/members');
  await expect(authenticatedPage.getByText('jane@example.com')).toBeVisible();
});
```

---

## Common Patterns

### Pattern 1: Nullable vs Required

```csharp
// Input (from client)
public string? Message { get; init; }  // Optional in GraphQL

// Command (domain)
public Message? Message { get; init; }  // Optional Vogen value object

// Mapping
Message = input.Message != null ? Message.From(input.Message) : null
```

### Pattern 2: Enum Parsing

```csharp
// Input (string from GraphQL)
public required string Role { get; init; }

// Command (enum)
public required UserRole Role { get; init; }

// Mapping
Role = Enum.Parse<UserRole>(input.Role)
```

### Pattern 3: Vogen Factory Methods

```csharp
// Simple types
FamilyId.From(input.FamilyId)
Email.From(input.Email)
Username.From(input.Username)

// Generate new IDs
InvitationId.New()
UserId.New()

// Generate random values
InvitationToken.Generate()
InvitationDisplayCode.Generate()
```

### Pattern 4: Error Handling in Payloads

```csharp
try
{
    var command = new SomeCommand(...);
    var result = await mediator.Send(command, cancellationToken);
    return new SomePayload(result.Data);
}
catch (Exception ex)
{
    return new SomePayload(new[]
    {
        new UserError
        {
            Code = DetermineErrorCode(ex),
            Message = ex.Message,
            Field = DetermineField(ex)
        }
    });
}
```

---

## Authorization Helpers

```csharp
public static class ClaimsPrincipalExtensions
{
    public static UserId GetUserId(this ClaimsPrincipal principal)
    {
        var userIdClaim = principal.FindFirst("sub")
            ?? throw new UnauthorizedException("User ID not found in claims");

        return UserId.From(Guid.Parse(userIdClaim.Value));
    }

    public static UserRole GetUserRole(this ClaimsPrincipal principal)
    {
        var roleClaim = principal.FindFirst(ClaimTypes.Role)
            ?? throw new UnauthorizedException("Role not found in claims");

        return Enum.Parse<UserRole>(roleClaim.Value);
    }

    public static bool IsOwnerOrAdmin(this ClaimsPrincipal principal)
    {
        var role = principal.GetUserRole();
        return role == UserRole.Owner || role == UserRole.Admin;
    }
}
```

---

## Redis PubSub Integration

### Domain Event → Redis Message

```csharp
// Domain event handler
public class FamilyMemberInvitedEventHandler : INotificationHandler<FamilyMemberInvitedEvent>
{
    private readonly ITopicEventSender _eventSender;

    public async Task Handle(FamilyMemberInvitedEvent notification, CancellationToken cancellationToken)
    {
        // Publish to Redis PubSub
        await _eventSender.SendAsync(
            $"pending-invitations-changed:{notification.FamilyId.Value}",
            new PendingInvitationsChangedMessage
            {
                FamilyId = notification.FamilyId.Value,
                ChangeType = ChangeType.ADDED,
                Invitation = new PendingInvitationDto
                {
                    Id = notification.InvitationId.Value,
                    Email = notification.Email.Value,
                    Role = notification.Role,
                    InvitedAt = DateTime.UtcNow,
                    ExpiresAt = notification.ExpiresAt
                }
            },
            cancellationToken
        );
    }
}
```

---

## Next Steps

1. **backend-developer**: Implement MediatR commands and handlers
2. **backend-developer**: Implement mutation resolvers in `InvitationMutations.cs`
3. **backend-developer**: Implement query resolvers in `InvitationQueries.cs`
4. **backend-developer**: Implement subscription resolvers in `InvitationSubscriptions.cs`
5. **microservices-architect**: Implement Redis PubSub integration
6. **frontend-developer**: Generate TypeScript types and Apollo hooks
7. **test-automator**: Write E2E tests with Playwright

---

**Quick Reference Version:** 1.0
**Last Updated:** 2026-01-04
**Agent:** api-designer
