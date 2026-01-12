# Implementation Plan: Family Member Invitation System

**Epic:** #24 - Family Member Invitation System
**Sub-Issues:** #25 (Multi-Step Wizard), #26 (Management UI)
**Branch:** `feature/family-member-invitation-system`
**Phase:** Phase 1 - Core MVP
**Priority:** P0 - Critical
**Timeline:** 22-24 days (3-5 days prerequisite + 19-21 days implementation)
**Deployment:** Single atomic release (#25 + #26 together)

---

## Executive Summary

### Scope

Implement a complete family member invitation system with dual workflows:

1. **Email Invitations**: Token-based invitations with 14-day expiration
2. **Managed Account Creation**: Zitadel-managed accounts for users without email (e.g., children)

**Components:**

- Multi-step family creation wizard (2 steps: Family Info → Invite Members)
- Family management UI (current members, pending invitations, invite modal)
- Backend domain model (FamilyMemberInvitation aggregate, User extensions)
- GraphQL API (mutations, queries, real-time subscriptions)
- Event-driven architecture (outbox pattern, RabbitMQ, domain events)
- Background job processing (Quartz.NET retry workers)

### Team & Agents

**11 Specialized Agents**:

- `database-administrator`: Enum migration, schema design, indexes
- `backend-developer`: Domain model, command handlers, GraphQL mutations
- `api-designer`: GraphQL schema design, input/command patterns
- `security-engineer`: Zitadel integration, rate limiting, password policies
- `microservices-architect`: Outbox pattern, event publishing, Redis PubSub
- `devops-engineer`: Quartz.NET setup, background jobs, infrastructure
- `frontend-developer`: Angular wizard, reactive forms, Apollo integration
- `ui-designer`: Password strength UI, modal components, UX patterns
- `test-automator`: E2E tests (Playwright + TestContainers)
- `qa-expert`: Test strategy, edge cases, integration tests
- `accessibility-tester`: WCAG 2.1 AA compliance validation
- `architect-reviewer`: Architecture review, technical decisions validation
- `code-reviewer`: Code quality, patterns, documentation review

### Timeline

- **Phase 0** (3-5 days): Terminology update (CHILD → MANAGED_ACCOUNT) - **BLOCKING**
- **Phase 1** (4 days): Backend foundation (domain model, GraphQL schema, security)
- **Phase 2** (4 days): Backend services (commands, mutations, event publishing)
- **Phase 3** (4 days): Frontend wizard & real-time features
- **Phase 4** (4 days): Management UI (#26)
- **Phase 5** (3 days): Testing & quality assurance
- **Phase 6** (1 day): Architecture review & polish

**Total:** 22-24 days

---

## Technical Decisions Summary

### Infrastructure

| Decision | Choice | Rationale |
|----------|--------|-----------|
| Background Jobs | Quartz.NET (enterprise scheduler) | Cron expressions, clustering, production-grade |
| Real-time Subscriptions | Hot Chocolate + Redis PubSub | Multi-instance scaling, production-ready |
| Event Publishing | Outbox pattern with background worker | Reliable, eventual consistency, no event loss |
| Database Indexes | Unique on token, index on expires_at | Optimize lookups and cleanup queries |
| Synthetic Email Domain | Configurable in appsettings.json | Environment-specific (@noemail.{env}.family-hub.internal) |
| Event Versioning | Yes - 'eventVersion: 1' field | Future-proof for schema evolution |
| Outbox Retention | Forever (archive after 90 days) | Complete audit trail, cold storage for old events |

### Security

| Decision | Choice | Rationale |
|----------|--------|-----------|
| Zitadel Authentication | JWT-based service auth with short-lived tokens | Secure, no static credentials |
| Token Caching | Cache with automatic refresh before expiry | Reduces API calls, respects rate limits |
| Invitation Token Format | Cryptographically random (64 chars URL-safe base64) | Standard approach, requires DB lookup |
| Rate Limiting | IP-based (10 attempts per hour) | Prevent brute-force attacks |
| Password Policy | Configurable per family | Owner sets expiration requirements |
| Audit Trail | Timestamp + inviter user ID (minimal) | Privacy-focused, essential only |

### UX & Frontend

| Decision | Choice | Rationale |
|----------|--------|-----------|
| Wizard Framework | Generic wizard framework (reusable) | Future-proof for other wizards |
| Wizard Step 3 | 2-step now, API supports future steps | YAGNI but extensible |
| Password Strength UI | User-defined slider (12-32 chars) + checkboxes | Maximum flexibility |
| Password Preview | Real-time preview as user adjusts slider | Immediate feedback, better UX |
| QR Code | Defer to Phase 2 (no mobile app yet) | Avoid premature work |
| State Persistence | SessionStorage (survives refresh) | Balances simplicity with safety |
| Batch Invitation Mode | Mixed mode (email + managed accounts) | Flexible, supports all scenarios |
| Table Reordering | Auto-sort alphabetically | Simplest, order doesn't matter |
| Auto-save Timing | Save on blur (field completion) | Balanced data safety |
| Modal Content | Username + password + synthetic email | Complete credentials for reference |
| Skip Wizard Prompt | Just skip - no reminders | Clean UX, trust user |
| Real-time Updates | GraphQL subscriptions (Hot Chocolate + Redis) | True real-time, best UX |

### Validation & Error Handling

| Decision | Choice | Rationale |
|----------|--------|-----------|
| Validation Strategy | Dual: client-side + backend | Best UX + defense in depth |
| Batch Processing | Two-phase: validate all, then commit all | Atomic consistency |
| GraphQL Errors | Unified error list with error codes | Simple, consistent API |
| Duplicate Detection | Block with error (strict) | Prevent duplicates completely |
| Zitadel API Failures | Queue to background job with notification | Best UX, reliable delivery |
| Circuit Breaker | Exponential backoff with max delay (15min ceiling) | Persistent retry without hammering |
| Expired Invitations | Hard delete after 30 days grace period | Balances audit trail with DB size |

### Testing

| Decision | Choice | Rationale |
|----------|--------|-----------|
| E2E Framework | Playwright with TestContainers RabbitMQ | Full integration, production-like |
| Event Testing | All: presence + content + routing | Comprehensive coverage |
| Component Testing | TestBed with real Angular modules | Integration-style confidence |
| Quartz.NET Retry Schedule | Exponential backoff: 1min, 5min, 15min, 1hr, 4hr | Balances recovery speed with API respect |

### Data & Domain Model

| Decision | Choice | Rationale |
|----------|--------|-----------|
| Enum Migration | 3-step: create new, migrate, drop old | Safe, zero downtime |
| InvitationId Format | Hybrid: GUID + short code display | Internal efficiency + user-friendly debugging |
| Batch Limit | Configurable in appsettings.json | Flexible per environment |
| Default Managed Account Role | 'Member' (all equal) | No special treatment, role is orthogonal |
| Invitation Actions | Resend, Edit role, Add message, Cancel | Rich feature set for Phase 1 |
| Table Editing | Edit modal (safer, confirmation step) | Prevents accidental changes |

### Implementation

| Decision | Choice | Rationale |
|----------|--------|-----------|
| Implementation Order | Terminology → Backend → Frontend → Tests | Foundation first, de-risk early |
| Form Architecture | Reactive Forms with FormArray | Angular standard, built-in validation |
| Component Reusability | Template inputs (@Input() TemplateRef) | Flexible, programmatic control |
| Deployment | Single release (#25 + #26 together) | Atomic feature delivery |
| Complexity Level | Proceed with full complexity | Production-grade from day 1 |
| Analytics | Privacy-first: no telemetry | Strongest privacy stance |
| Accessibility Focus | Focus on first interactive element | Direct to action, faster workflow |

---

## Agent Coordination Matrix

| Agent | Responsibilities | Dependencies | Duration |
|-------|-----------------|--------------|----------|
| **database-administrator** | Enum migration (Phase 0), table schema, indexes | None | 3-5 days |
| **api-designer** | GraphQL schema design, input/command patterns | Phase 0 complete | 2 days |
| **backend-developer** | Domain model, commands, handlers, queries | Phase 0, api-designer | 8 days |
| **security-engineer** | Zitadel JWT integration, rate limiting, password gen | Phase 0 | 3 days |
| **microservices-architect** | Outbox pattern, event publishing, Redis PubSub | backend-developer (events) | 3 days |
| **devops-engineer** | Quartz.NET setup, background jobs, Docker Compose | microservices-architect (outbox) | 2 days |
| **frontend-developer** | Angular wizard, forms, Apollo, subscriptions | api-designer (schema), backend-developer (API) | 6 days |
| **ui-designer** | Password strength UI, modals, component API | frontend-developer (framework) | 2 days |
| **test-automator** | E2E tests (Playwright + TestContainers) | frontend-developer, backend-developer | 3 days |
| **qa-expert** | Test strategy, integration tests, edge cases | backend-developer, frontend-developer | 2 days |
| **accessibility-tester** | WCAG 2.1 AA compliance, focus management | frontend-developer | 1 day |
| **architect-reviewer** | Architecture review, technical debt assessment | All agents complete | 1 day |
| **code-reviewer** | Code quality, patterns, documentation | All agents complete | 1 day |

### Handoff Points

1. **Phase 0 → Phase 1**: database-administrator completes enum migration, all agents can start
2. **api-designer → backend-developer**: GraphQL schema approved, implementation begins
3. **api-designer → frontend-developer**: GraphQL schema published, frontend can mock API
4. **backend-developer → microservices-architect**: Domain events defined, outbox can be implemented
5. **backend-developer → frontend-developer**: Mutations deployed to dev, frontend integration
6. **frontend-developer → test-automator**: UI components stable, E2E tests can be written
7. **All → architect-reviewer**: Implementation complete, review begins

---

## Phase 0: Terminology Update (BLOCKING)

**Duration:** 3-5 days
**Agent:** database-administrator
**Blocks:** All other phases (MUST complete first)

### Context

Rename `UserRole.CHILD` → `UserRole.MANAGED_ACCOUNT` across the entire codebase with a safe, zero-downtime migration strategy.

**Current State:** UserRole enum has values: OWNER, ADMIN, MEMBER, CHILD
**Target State:** UserRole enum has values: OWNER, ADMIN, MEMBER, MANAGED_ACCOUNT

### Tasks

#### 0.1: Create Migration Adding MANAGED_ACCOUNT Enum Value

**Agent:** database-administrator
**Duration:** 1 day

**Subtasks:**

- [ ] Create EF Core migration: `AddManagedAccountRole`

  ```sql
  ALTER TYPE auth.user_role ADD VALUE IF NOT EXISTS 'MANAGED_ACCOUNT';
  ```

- [ ] Update C# enum in `FamilyHub.Modules.Auth/Domain/UserRole.cs`:

  ```csharp
  public enum UserRole
  {
      Owner = 1,
      Admin = 2,
      Member = 3,
      Child = 4,           // Keep temporarily
      ManagedAccount = 5   // Add new value
  }
  ```

- [ ] Add Vogen value object for UserRole if not already using
- [ ] Test migration on local dev environment
- [ ] Document rollback procedure

**Acceptance Criteria:**

- Migration runs successfully without errors
- Both CHILD and MANAGED_ACCOUNT enum values exist
- Existing users with CHILD role are unaffected
- No application downtime

#### 0.2: Create Data Migration Script

**Agent:** database-administrator
**Duration:** 1 day

**Subtasks:**

- [ ] Create migration: `MigrateChildToManagedAccount`

  ```sql
  UPDATE auth.users
  SET role = 'MANAGED_ACCOUNT'
  WHERE role = 'CHILD';
  ```

- [ ] Add verification query to check migration success:

  ```sql
  SELECT COUNT(*) FROM auth.users WHERE role = 'CHILD'; -- Should return 0
  SELECT COUNT(*) FROM auth.users WHERE role = 'MANAGED_ACCOUNT';
  ```

- [ ] Test migration with sample data (create 10 test users with CHILD role, verify migration)
- [ ] Create rollback script (reverse migration if needed within 48-hour window)

**Acceptance Criteria:**

- All users with CHILD role are migrated to MANAGED_ACCOUNT
- Zero data loss during migration
- Rollback script tested and documented
- Migration idempotent (can run multiple times safely)

#### 0.3: Remove CHILD Enum Value

**Agent:** database-administrator
**Duration:** 1 day

**Subtasks:**

- [ ] Wait 48 hours after 0.2 deployment (safety buffer for rollback)
- [ ] Verify no users have CHILD role in production
- [ ] Remove CHILD from C# enum:

  ```csharp
  public enum UserRole
  {
      Owner = 1,
      Admin = 2,
      Member = 3,
      ManagedAccount = 5  // Renumbered from 4
  }
  ```

- [ ] Create migration: `RemoveChildRole` (optional - PostgreSQL allows unused enum values)

  ```sql
  -- NOTE: PostgreSQL doesn't support removing enum values directly
  -- Document that CHILD exists but is unused
  -- Or recreate enum entirely (complex, risky)
  ```

- [ ] Search codebase for string literals: "CHILD", "Child" - replace all references
- [ ] Update GraphQL schema: rename ChildRole → ManagedAccountRole
- [ ] Update documentation: MANAGED-ACCOUNT-SETUP.md, domain-model-microservices-map.md

**Acceptance Criteria:**

- No code references to CHILD role remain
- GraphQL schema updated and deployed
- All tests pass with new enum values
- Documentation updated

#### 0.4: Update GraphQL Schema

**Agent:** database-administrator + api-designer
**Duration:** 1 day

**Subtasks:**

- [ ] Update GraphQL `UserRole` enum:

  ```graphql
  enum UserRole {
    OWNER
    ADMIN
    MEMBER
    MANAGED_ACCOUNT  # Renamed from CHILD
  }
  ```

- [ ] Create `FamilyMemberType` type (new for #25/#26):

  ```graphql
  type FamilyMemberType {
    userId: ID!
    name: String!
    email: String
    username: String
    role: UserRole!
    joinedAt: DateTime!
  }
  ```

- [ ] Create `PendingInvitation` type:

  ```graphql
  type PendingInvitation {
    invitationId: ID!
    displayCode: String!
    email: String
    username: String
    role: UserRole!
    invitedBy: String!
    sentAt: DateTime!
    expiresAt: DateTime!
    status: InvitationStatus!
  }

  enum InvitationStatus {
    PENDING
    ACCEPTED
    EXPIRED
    CANCELED
  }
  ```

- [ ] Update existing `family` query to return new types:

  ```graphql
  type Family {
    id: ID!
    name: String!
    members: [FamilyMemberType!]!  # Changed from [User!]!
    pendingInvitations: [PendingInvitation!]!  # New field
  }
  ```

- [ ] Publish schema to dev environment
- [ ] Notify frontend-developer of schema changes

**Acceptance Criteria:**

- GraphQL schema compiles without errors
- Breaking changes documented in CHANGELOG
- Frontend receives schema notification before implementation starts
- Schema published to dev GraphQL playground

#### 0.5: Integration Testing

**Agent:** database-administrator + qa-expert
**Duration:** 1 day

**Subtasks:**

- [ ] Create integration test: User with MANAGED_ACCOUNT role can be queried
- [ ] Create integration test: GraphQL query returns correct UserRole enum
- [ ] Create integration test: Family.members query returns FamilyMemberType
- [ ] Verify no CHILD references in test fixtures
- [ ] Run full test suite (unit + integration)
- [ ] Fix any broken tests referencing CHILD

**Acceptance Criteria:**

- All tests pass (0 failures)
- Code coverage maintained (>80% backend)
- No CHILD enum references in tests
- CI/CD pipeline green

### Phase 0 Deliverables

- [ ] 3-step enum migration complete and tested
- [ ] GraphQL schema restructured (FamilyMemberType, PendingInvitation)
- [ ] Zero data loss, zero downtime
- [ ] Documentation updated
- [ ] All tests passing
- [ ] Merged to main branch

### Phase 0 Risks & Mitigation

**Risk:** Enum migration causes downtime
**Mitigation:** 3-step approach allows rollback at each step, tested in dev first

**Risk:** Breaking changes affect existing frontend
**Mitigation:** Coordinate schema deployment with frontend-developer, feature flag if needed

**Risk:** Data migration fails for edge cases
**Mitigation:** Extensive testing with diverse data, rollback script ready

---

## Phase 1: Backend Foundation

**Duration:** 4 days
**Agents:** backend-developer, api-designer, security-engineer, database-administrator
**Dependencies:** Phase 0 complete
**Parallel Work:** Yes (3 workstreams)

### Workstream A: Domain Model (backend-developer)

**Duration:** 3 days

#### 1.A.1: FamilyMemberInvitation Aggregate

**Subtasks:**

- [ ] Create `InvitationId` value object (Vogen GUID):

  ```csharp
  [ValueObject<Guid>(conversions: Conversions.EfCoreValueConverter)]
  public readonly partial struct InvitationId
  {
      public static InvitationId New() => From(Guid.NewGuid());
  }
  ```

- [ ] Create `InvitationDisplayCode` value object (short code):

  ```csharp
  [ValueObject<string>]
  public readonly partial struct InvitationDisplayCode
  {
      private static Validation Validate(string value) =>
          value.Length == 8 && IsAlphanumeric(value)
              ? Validation.Ok
              : Validation.Invalid("Display code must be 8 alphanumeric characters");

      public static InvitationDisplayCode Generate() =>
          From(GenerateRandomAlphanumeric(8)); // INV-KX7M2P format
  }
  ```

- [ ] Create `InvitationToken` value object (64-char random):

  ```csharp
  [ValueObject<string>]
  public readonly partial struct InvitationToken
  {
      private static Validation Validate(string value) =>
          value.Length == 64 && IsUrlSafeBase64(value)
              ? Validation.Ok
              : Validation.Invalid("Token must be 64 URL-safe base64 characters");

      public static InvitationToken Generate()
      {
          var bytes = new byte[48]; // 48 bytes = 64 base64 chars
          using var rng = RandomNumberGenerator.Create();
          rng.GetBytes(bytes);
          return From(Convert.ToBase64String(bytes)
              .Replace("+", "-").Replace("/", "_").TrimEnd('='));
      }
  }
  ```

- [ ] Create `FamilyMemberInvitation` aggregate root:

  ```csharp
  public class FamilyMemberInvitation : AggregateRoot<InvitationId>
  {
      public FamilyId FamilyId { get; private set; }
      public Email? Email { get; private set; }
      public Username? Username { get; private set; }
      public UserRole Role { get; private set; }
      public InvitationToken Token { get; private set; }
      public InvitationDisplayCode DisplayCode { get; private set; }
      public DateTime ExpiresAt { get; private set; }
      public UserId InvitedByUserId { get; private set; }
      public InvitationStatus Status { get; private set; }
      public DateTime CreatedAt { get; private set; }
      public DateTime? AcceptedAt { get; private set; }

      // Factory methods
      public static FamilyMemberInvitation CreateEmailInvitation(
          FamilyId familyId, Email email, UserRole role, UserId invitedBy)
      {
          var invitation = new FamilyMemberInvitation
          {
              Id = InvitationId.New(),
              FamilyId = familyId,
              Email = email,
              Role = role,
              Token = InvitationToken.Generate(),
              DisplayCode = InvitationDisplayCode.Generate(),
              ExpiresAt = DateTime.UtcNow.AddDays(14),
              InvitedByUserId = invitedBy,
              Status = InvitationStatus.Pending,
              CreatedAt = DateTime.UtcNow
          };

          invitation.AddDomainEvent(new FamilyMemberInvitedEvent(
              eventVersion: 1,
              invitationId: invitation.Id,
              familyId: familyId,
              email: email,
              role: role,
              token: invitation.Token,
              expiresAt: invitation.ExpiresAt,
              invitedByUserId: invitedBy
          ));

          return invitation;
      }

      public static FamilyMemberInvitation CreateManagedAccountInvitation(
          FamilyId familyId, Username username, PersonName personName, UserRole role, UserId invitedBy)
      {
          var invitation = new FamilyMemberInvitation
          {
              Id = InvitationId.New(),
              FamilyId = familyId,
              Username = username,
              Role = role,
              Token = InvitationToken.Generate(), // Still generate for tracking
              DisplayCode = InvitationDisplayCode.Generate(),
              ExpiresAt = DateTime.UtcNow.AddDays(1), // Shorter expiration for managed accounts
              InvitedByUserId = invitedBy,
              Status = InvitationStatus.Pending,
              CreatedAt = DateTime.UtcNow
          };

          // Event published after Zitadel account creation (see command handler)

          return invitation;
      }

      // Business logic methods
      public void Accept(UserId userId)
      {
          if (Status != InvitationStatus.Pending)
              throw new InvalidOperationException($"Cannot accept invitation in {Status} status");

          if (DateTime.UtcNow > ExpiresAt)
          {
              Status = InvitationStatus.Expired;
              throw new InvalidOperationException("Invitation has expired");
          }

          Status = InvitationStatus.Accepted;
          AcceptedAt = DateTime.UtcNow;

          AddDomainEvent(new InvitationAcceptedEvent(
              eventVersion: 1,
              invitationId: Id,
              familyId: FamilyId,
              userId: userId,
              acceptedAt: AcceptedAt.Value
          ));
      }

      public void Cancel(UserId canceledBy)
      {
          if (Status != InvitationStatus.Pending)
              throw new InvalidOperationException($"Cannot cancel invitation in {Status} status");

          Status = InvitationStatus.Canceled;

          AddDomainEvent(new InvitationCanceledEvent(
              eventVersion: 1,
              invitationId: Id,
              familyId: FamilyId,
              canceledByUserId: canceledBy,
              canceledAt: DateTime.UtcNow
          ));
      }

      public void Resend(UserId resentBy)
      {
          if (Status != InvitationStatus.Expired && Status != InvitationStatus.Pending)
              throw new InvalidOperationException($"Cannot resend invitation in {Status} status");

          // Generate new token and extend expiration
          Token = InvitationToken.Generate();
          ExpiresAt = DateTime.UtcNow.AddDays(14);
          Status = InvitationStatus.Pending;

          AddDomainEvent(new FamilyMemberInvitedEvent(
              eventVersion: 1,
              invitationId: Id,
              familyId: FamilyId,
              email: Email!,
              role: Role,
              token: Token,
              expiresAt: ExpiresAt,
              invitedByUserId: resentBy,
              isResend: true
          ));
      }
  }

  public enum InvitationStatus
  {
      Pending = 1,
      Accepted = 2,
      Expired = 3,
      Canceled = 4
  }
  ```

**Acceptance Criteria:**

- Aggregate compiles without errors
- Factory methods enforce business rules (email XOR username required)
- Domain events are published correctly
- Unit tests cover all business logic (Accept, Cancel, Resend)
- Vogen value objects generate EF Core converters

#### 1.A.2: User Entity Extensions

**Subtasks:**

- [ ] Add managed account properties to `User` entity:

  ```csharp
  public class User : AggregateRoot<UserId>
  {
      // Existing properties
      public Email Email { get; private set; }
      public UserRole Role { get; private set; }

      // New properties for managed accounts
      public Username? Username { get; private set; }
      public PersonName? FullName { get; private set; }
      public string? ZitadelUserId { get; private set; }
      public bool IsSyntheticEmail => Email.Value.EndsWith("@noemail.family-hub.internal");

      // Factory method for managed accounts
      public static User CreateManagedAccount(
          Username username,
          PersonName personName,
          UserRole role,
          string zitadelUserId,
          string syntheticEmailDomain)
      {
          var syntheticEmail = Email.From($"{username.Value}@{syntheticEmailDomain}");

          var user = new User
          {
              Id = UserId.New(),
              Email = syntheticEmail,
              Username = username,
              FullName = fullName,
              Role = role,
              ZitadelUserId = zitadelUserId,
              CreatedAt = DateTime.UtcNow
          };

          return user;
      }
  }
  ```

- [ ] Create `Username` value object:

  ```csharp
  [ValueObject<string>(conversions: Conversions.EfCoreValueConverter)]
  public readonly partial struct Username
  {
      private static Validation Validate(string value) =>
          value.Length >= 3 && value.Length <= 20 && IsAlphanumericWithUnderscores(value)
              ? Validation.Ok
              : Validation.Invalid("Username must be 3-20 alphanumeric characters or underscores");

      private static string NormalizeInput(string input) => input.Trim().ToLowerInvariant();
  }
  ```

- [ ] Create `FullName` value object:

  ```csharp
  [ValueObject<string>(conversions: Conversions.EfCoreValueConverter)]
  public readonly partial struct PersonName
  {
      private static Validation Validate(string value) =>
          value.Length >= 1 && value.Length <= 100
              ? Validation.Ok
              : Validation.Invalid("Full name must be 1-100 characters");

      private static string NormalizeInput(string input) => input.Trim();
  }
  ```

- [ ] Update User repository interface: `IUserRepository.GetByUsernameAsync(Username username)`
- [ ] Create unit tests for managed account creation

**Acceptance Criteria:**

- User entity supports both email and managed account types
- Username and FullName value objects have proper validation
- Factory method generates synthetic email correctly
- Repository can query by username
- Unit tests pass (>80% coverage)

#### 1.A.3: Domain Events

**Subtasks:**

- [ ] Create `FamilyMemberInvitedEvent`:

  ```csharp
  public record FamilyMemberInvitedEvent(
      int EventVersion,
      InvitationId InvitationId,
      FamilyId FamilyId,
      Email Email,
      UserRole Role,
      InvitationToken Token,
      DateTime ExpiresAt,
      UserId InvitedByUserId,
      bool IsResend = false
  ) : DomainEvent(EventVersion);
  ```

- [ ] Create `ManagedAccountCreatedEvent`:

  ```csharp
  public record ManagedAccountCreatedEvent(
      int EventVersion,
      InvitationId InvitationId,
      FamilyId FamilyId,
      UserId UserId,
      Username Username,
      PersonName PersonName,
      UserRole Role,
      string ZitadelUserId,
      UserId CreatedByUserId
  ) : DomainEvent(EventVersion);
  ```

- [ ] Create `InvitationAcceptedEvent`:

  ```csharp
  public record InvitationAcceptedEvent(
      int EventVersion,
      InvitationId InvitationId,
      FamilyId FamilyId,
      UserId UserId,
      DateTime AcceptedAt
  ) : DomainEvent(EventVersion);
  ```

- [ ] Create `InvitationCanceledEvent`:

  ```csharp
  public record InvitationCanceledEvent(
      int EventVersion,
      InvitationId InvitationId,
      FamilyId FamilyId,
      UserId CanceledByUserId,
      DateTime CanceledAt
  ) : DomainEvent(EventVersion);
  ```

- [ ] Ensure all events include `EventVersion` field for future schema evolution
- [ ] Create unit tests verifying event publishing from aggregate methods

**Acceptance Criteria:**

- All domain events are immutable records
- EventVersion = 1 for all Phase 1 events
- Events contain all necessary data for consumers
- Unit tests verify events are published when business methods are called

### Workstream B: GraphQL Schema (api-designer)

**Duration:** 2 days (can start after Phase 0 complete)

#### 1.B.1: Design Invitation Mutations Schema

**Subtasks:**

- [ ] Create `InviteFamilyMemberByEmailInput`:

  ```graphql
  input InviteFamilyMemberByEmailInput {
    familyId: ID!
    email: String!
    role: UserRole!
    message: String  # Optional personal message for Phase 1 (resend/edit role features)
  }
  ```

- [ ] Create `CreateManagedMemberInput`:

  ```graphql
  input CreateManagedMemberInput {
    familyId: ID!
    username: String!
    name: String!
    role: UserRole!
    passwordStrength: PasswordStrength!
  }

  enum PasswordStrength {
    SIMPLE      # Passphrase style
    MEDIUM      # Mixed characters
    STRONG      # Maximum complexity
  }

  input PasswordGenerationConfig {
    length: Int!  # 12-32
    includeUppercase: Boolean!
    includeLowercase: Boolean!
    includeDigits: Boolean!
    includeSymbols: Boolean!
  }
  ```

- [ ] Create `BatchInviteFamilyMembersInput` (mixed mode):

  ```graphql
  input BatchInviteFamilyMembersInput {
    familyId: ID!
    emailInvitations: [EmailInvitationInput!]!
    managedAccounts: [ManagedAccountInput!]!
  }

  input EmailInvitationInput {
    email: String!
    role: UserRole!
    message: String
  }

  input ManagedAccountInput {
    username: String!
    name: String!
    role: UserRole!
    passwordConfig: PasswordGenerationConfig!
  }
  ```

- [ ] Create mutation payloads:

  ```graphql
  type InviteFamilyMemberByEmailPayload {
    invitation: PendingInvitation
    errors: [MutationError!]
  }

  type CreateManagedMemberPayload {
    invitation: PendingInvitation
    user: User
    credentials: ManagedAccountCredentials  # Returned only once!
    errors: [MutationError!]
  }

  type ManagedAccountCredentials {
    username: String!
    password: String!  # Only returned on creation, never again
    syntheticEmail: String!
    loginUrl: String!
  }

  type BatchInviteFamilyMembersPayload {
    emailInvitations: [PendingInvitation!]
    managedAccounts: [ManagedAccountResult!]
    errors: [MutationError!]
  }

  type ManagedAccountResult {
    user: User
    credentials: ManagedAccountCredentials
  }

  type MutationError {
    code: ErrorCode!
    message: String!
    field: String  # Which input field caused the error
    attemptedValue: String  # What value was attempted
  }

  enum ErrorCode {
    VALIDATION_FAILED
    DUPLICATE_EMAIL
    DUPLICATE_USERNAME
    INVALID_EMAIL_FORMAT
    INVALID_USERNAME_FORMAT
    ZITADEL_API_ERROR
    FAMILY_NOT_FOUND
    UNAUTHORIZED
    RATE_LIMIT_EXCEEDED
    BATCH_SIZE_EXCEEDED
  }
  ```

#### 1.B.2: Design Query Schema

**Subtasks:**

- [ ] Create `FamilyMemberInvitation` queries:

  ```graphql
  extend type Query {
    familyMembers(familyId: ID!): [FamilyMemberType!]!
    pendingInvitations(familyId: ID!): [PendingInvitation!]!
    invitation(invitationId: ID!): PendingInvitation
    invitationByToken(token: String!): PendingInvitation
  }
  ```

- [ ] Create `CancelInvitationInput` and payload:

  ```graphql
  input CancelInvitationInput {
    invitationId: ID!
  }

  type CancelInvitationPayload {
    success: Boolean!
    errors: [MutationError!]
  }
  ```

- [ ] Create `ResendInvitationInput`:

  ```graphql
  input ResendInvitationInput {
    invitationId: ID!
    message: String  # Updated message for resend
  }

  type ResendInvitationPayload {
    invitation: PendingInvitation
    errors: [MutationError!]
  }
  ```

- [ ] Create `UpdateInvitationRoleInput`:

  ```graphql
  input UpdateInvitationRoleInput {
    invitationId: ID!
    newRole: UserRole!
  }

  type UpdateInvitationRolePayload {
    invitation: PendingInvitation
    errors: [MutationError!]
  }
  ```

#### 1.B.3: Design Subscription Schema (Real-time Updates)

**Subtasks:**

- [ ] Create GraphQL subscriptions:

  ```graphql
  extend type Subscription {
    familyMembersChanged(familyId: ID!): FamilyMembersChangedPayload!
    pendingInvitationsChanged(familyId: ID!): PendingInvitationsChangedPayload!
  }

  type FamilyMembersChangedPayload {
    familyId: ID!
    changeType: ChangeType!
    member: FamilyMemberType
  }

  type PendingInvitationsChangedPayload {
    familyId: ID!
    changeType: ChangeType!
    invitation: PendingInvitation
  }

  enum ChangeType {
    ADDED
    UPDATED
    REMOVED
  }
  ```

- [ ] Document subscription lifecycle (connect, authenticate, filter by familyId)
- [ ] Plan Redis PubSub integration (messages published on domain events)

**Acceptance Criteria:**

- GraphQL schema compiles and validates
- Input/Command pattern maintained (separate inputs from commands)
- Error codes comprehensive and descriptive
- Schema documented in `/docs/architecture/domain-model-microservices-map.md`
- Published to GraphQL playground for frontend team

### Workstream C: Security & Zitadel Integration (security-engineer)

**Duration:** 3 days (can start after Phase 0)

#### 1.C.1: Zitadel JWT Service Authentication

**Subtasks:**

- [ ] Create `IZitadelManagementClient` interface:

  ```csharp
  public interface IZitadelManagementClient
  {
      Task<ZitadelUser> CreateHumanUserAsync(
          string username,
          string email,
          string firstName,
          string lastName,
          string password);

      Task<string> GetAccessTokenAsync(); // Internal: JWT token acquisition

      Task<bool> ValidateTokenAsync(string token); // For health checks
  }
  ```

- [ ] Implement `ZitadelManagementClient`:

  ```csharp
  public class ZitadelManagementClient : IZitadelManagementClient
  {
      private readonly HttpClient _httpClient;
      private readonly IMemoryCache _tokenCache;
      private readonly ZitadelOptions _options;

      public async Task<string> GetAccessTokenAsync()
      {
          // Check cache first
          if (_tokenCache.TryGetValue("zitadel_management_token", out string cachedToken))
          {
              var jwt = new JwtSecurityTokenHandler().ReadJwtToken(cachedToken);
              if (jwt.ValidTo > DateTime.UtcNow.AddMinutes(5)) // Refresh 5min before expiry
                  return cachedToken;
          }

          // Request new token with JWT bearer assertion
          var assertion = CreateJwtAssertion(); // Create JWT signed with private key
          var request = new HttpRequestMessage(HttpMethod.Post, $"{_options.Authority}/oauth/v2/token")
          {
              Content = new FormUrlEncodedContent(new Dictionary<string, string>
              {
                  ["grant_type"] = "urn:ietf:params:oauth:grant-type:jwt-bearer",
                  ["assertion"] = assertion,
                  ["scope"] = "openid urn:zitadel:iam:org:project:id:zitadel:aud"
              })
          };

          var response = await _httpClient.SendAsync(request);
          response.EnsureSuccessStatusCode();

          var tokenResponse = await response.Content.ReadFromJsonAsync<TokenResponse>();

          // Cache with sliding expiration
          _tokenCache.Set("zitadel_management_token", tokenResponse.AccessToken, new MemoryCacheEntryOptions
          {
              AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(tokenResponse.ExpiresIn - 300)
          });

          return tokenResponse.AccessToken;
      }

      private string CreateJwtAssertion()
      {
          var key = new RsaSecurityKey(LoadPrivateKeyFromConfig());
          var credentials = new SigningCredentials(key, SecurityAlgorithms.RsaSha256);

          var claims = new[]
          {
              new Claim(JwtRegisteredClaimNames.Sub, _options.ServiceAccountId),
              new Claim(JwtRegisteredClaimNames.Iss, _options.ServiceAccountId),
              new Claim(JwtRegisteredClaimNames.Aud, $"{_options.Authority}/oauth/v2/token")
          };

          var token = new JwtSecurityToken(
              claims: claims,
              expires: DateTime.UtcNow.AddMinutes(5),
              signingCredentials: credentials
          );

          return new JwtSecurityTokenHandler().WriteToken(token);
      }

      public async Task<ZitadelUser> CreateHumanUserAsync(
          string username, string email, string firstName, string lastName, string password)
      {
          var token = await GetAccessTokenAsync();

          var request = new HttpRequestMessage(HttpMethod.Post, $"{_options.Authority}/management/v1/users/human")
          {
              Headers = { Authorization = new AuthenticationHeaderValue("Bearer", token) },
              Content = JsonContent.Create(new
              {
                  userName = username,
                  profile = new { firstName, lastName },
                  email = new { email, isEmailVerified = false },
                  password = password
              })
          };

          var response = await _httpClient.SendAsync(request);

          if (!response.IsSuccessStatusCode)
          {
              var error = await response.Content.ReadAsStringAsync();
              throw new ZitadelApiException($"Failed to create user: {error}", response.StatusCode);
          }

          var result = await response.Content.ReadFromJsonAsync<CreateUserResponse>();

          return new ZitadelUser
          {
              UserId = result.UserId,
              Username = username,
              Email = email
          };
      }
  }

  public class ZitadelOptions
  {
      public string Authority { get; set; } = default!;
      public string ServiceAccountId { get; set; } = default!;
      public string PrivateKeyPath { get; set; } = default!;
  }

  public class ZitadelApiException : Exception
  {
      public HttpStatusCode StatusCode { get; }
      public ZitadelApiException(string message, HttpStatusCode statusCode) : base(message)
      {
          StatusCode = statusCode;
      }
  }
  ```

- [ ] Configure in `appsettings.json`:

  ```json
  {
    "Zitadel": {
      "Authority": "https://auth.family-hub.com",
      "ServiceAccountId": "123456789@family-hub",
      "PrivateKeyPath": "/secrets/zitadel-service-account-key.pem"
    }
  }
  ```

- [ ] Add dependency injection in `AuthModule.cs`:

  ```csharp
  services.AddMemoryCache();
  services.AddHttpClient<IZitadelManagementClient, ZitadelManagementClient>();
  services.Configure<ZitadelOptions>(configuration.GetSection("Zitadel"));
  ```

**Acceptance Criteria:**

- JWT assertion created correctly with private key signing
- Access token cached with automatic refresh 5min before expiry
- CreateHumanUserAsync successfully creates users in Zitadel dev environment
- Error handling for Zitadel API failures (409 Conflict for duplicate username)
- Unit tests with mocked HttpClient (NSubstitute)

#### 1.C.2: Password Generation Service

**Subtasks:**

- [ ] Create `IPasswordGenerationService` interface:

  ```csharp
  public interface IPasswordGenerationService
  {
      string GeneratePassword(PasswordGenerationConfig config);
      string GeneratePassphrase(int wordCount = 4);
      bool ValidatePasswordComplexity(string password);
  }

  public record PasswordGenerationConfig(
      int Length,
      bool IncludeUppercase,
      bool IncludeLowercase,
      bool IncludeDigits,
      bool IncludeSymbols
  );
  ```

- [ ] Implement `PasswordGenerationService`:

  ```csharp
  public class PasswordGenerationService : IPasswordGenerationService
  {
      private static readonly string Lowercase = "abcdefghijklmnopqrstuvwxyz";
      private static readonly string Uppercase = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
      private static readonly string Digits = "0123456789";
      private static readonly string Symbols = "!@#$%^&*()_+-=[]{}|;:,.<>?";
      private static readonly string[] WordList = LoadWordList(); // 2000 common words

      public string GeneratePassword(PasswordGenerationConfig config)
      {
          var characterSet = "";
          if (config.IncludeLowercase) characterSet += Lowercase;
          if (config.IncludeUppercase) characterSet += Uppercase;
          if (config.IncludeDigits) characterSet += Digits;
          if (config.IncludeSymbols) characterSet += Symbols;

          if (string.IsNullOrEmpty(characterSet))
              throw new ArgumentException("At least one character type must be included");

          var password = new char[config.Length];
          using var rng = RandomNumberGenerator.Create();

          for (int i = 0; i < config.Length; i++)
          {
              var randomByte = new byte[1];
              rng.GetBytes(randomByte);
              password[i] = characterSet[randomByte[0] % characterSet.Length];
          }

          // Ensure at least one of each required type
          EnsureComplexity(password, config);

          return new string(password);
      }

      public string GeneratePassphrase(int wordCount = 4)
      {
          using var rng = RandomNumberGenerator.Create();
          var words = new string[wordCount];

          for (int i = 0; i < wordCount; i++)
          {
              var randomBytes = new byte[2];
              rng.GetBytes(randomBytes);
              var index = BitConverter.ToUInt16(randomBytes) % WordList.Length;
              words[i] = CapitalizeFirst(WordList[index]);
          }

          // Add digits and symbols
          var digitBytes = new byte[1];
          rng.GetBytes(digitBytes);
          var digit = digitBytes[0] % 100; // 0-99

          var symbolBytes = new byte[1];
          rng.GetBytes(symbolBytes);
          var symbol = Symbols[symbolBytes[0] % Symbols.Length];

          return $"{string.Join("-", words)}{digit}{symbol}"; // Horse-Battery-Staple-Tree42!
      }

      public bool ValidatePasswordComplexity(string password)
      {
          return password.Length >= 12 &&
                 password.Any(char.IsLower) &&
                 password.Any(char.IsUpper) &&
                 password.Any(char.IsDigit) &&
                 password.Any(c => Symbols.Contains(c));
      }

      private void EnsureComplexity(char[] password, PasswordGenerationConfig config)
      {
          // If config requires uppercase but none exist, replace first char
          if (config.IncludeUppercase && !password.Any(char.IsUpper))
              password[0] = Uppercase[GetRandomIndex(Uppercase.Length)];

          // Similar for other types...
      }
  }
  ```

- [ ] Create unit tests:
  - Generated passwords meet length requirements
  - Complexity requirements enforced
  - Passphrase format is correct (Word-Word-Word12!)
  - Validation logic catches weak passwords

**Acceptance Criteria:**

- Password generation service creates cryptographically random passwords
- Passphrase generation uses wordlist (2000+ common English words)
- Complexity validation enforces 12+ chars, upper, lower, digit, symbol
- Unit tests verify all generation modes (simple, medium, strong)
- No predictable patterns in generated passwords

#### 1.C.3: Rate Limiting Middleware

**Subtasks:**

- [ ] Install AspNetCoreRateLimit package:

  ```bash
  dotnet add package AspNetCoreRateLimit
  ```

- [ ] Configure IP rate limiting in `Program.cs`:

  ```csharp
  services.AddMemoryCache();
  services.Configure<IpRateLimitOptions>(configuration.GetSection("IpRateLimiting"));
  services.AddInMemoryRateLimiting();
  services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();

  app.UseIpRateLimiting();
  ```

- [ ] Configure in `appsettings.json`:

  ```json
  {
    "IpRateLimiting": {
      "EnableEndpointRateLimiting": true,
      "StackBlockedRequests": false,
      "RealIpHeader": "X-Real-IP",
      "ClientIdHeader": "X-ClientId",
      "HttpStatusCode": 429,
      "GeneralRules": [
        {
          "Endpoint": "POST:/graphql",
          "Period": "1h",
          "Limit": 100
        },
        {
          "Endpoint": "GET:/graphql/invitation/accept",
          "Period": "1h",
          "Limit": 10
        }
      ]
    }
  }
  ```

- [ ] Add GraphQL-specific rate limiting for invitation mutations:

  ```csharp
  [RateLimit(Period = "1h", Limit = 20)] // Custom attribute
  public async Task<InviteFamilyMemberByEmailPayload> InviteFamilyMemberByEmailAsync(
      InviteFamilyMemberByEmailInput input,
      [Service] IMediator mediator)
  {
      // ...
  }
  ```

- [ ] Create integration tests verifying rate limiting:
  - 11th request in 1 hour returns 429 Too Many Requests
  - Rate limit resets after 1 hour
  - Different IPs have independent limits

**Acceptance Criteria:**

- Rate limiting active on invitation acceptance endpoint
- 10 attempts per IP per hour enforced
- 429 status code returned with Retry-After header
- Integration tests verify rate limit behavior
- Redis option available for multi-instance deployment (future)

### Workstream D: Database Schema (database-administrator)

**Duration:** 2 days (can start after Phase 0)

#### 1.D.1: FamilyMemberInvitation Table Migration

**Subtasks:**

- [ ] Create EF Core migration: `CreateFamilyMemberInvitationsTable`

  ```csharp
  protected override void Up(MigrationBuilder migrationBuilder)
  {
      migrationBuilder.CreateTable(
          name: "family_member_invitations",
          schema: "auth",
          columns: table => new
          {
              invitation_id = table.Column<Guid>(type: "uuid", nullable: false),
              display_code = table.Column<string>(type: "varchar(8)", maxLength: 8, nullable: false),
              family_id = table.Column<Guid>(type: "uuid", nullable: false),
              email = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true),
              username = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: true),
              role = table.Column<string>(type: "varchar(20)", nullable: false), // user_role enum
              token = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: false),
              expires_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
              invited_by_user_id = table.Column<Guid>(type: "uuid", nullable: false),
              status = table.Column<int>(type: "integer", nullable: false), // invitation_status enum
              message = table.Column<string>(type: "text", nullable: true),
              created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
              accepted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
              updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
          },
          constraints: table =>
          {
              table.PrimaryKey("pk_family_member_invitations", x => x.invitation_id);
              table.ForeignKey(
                  name: "fk_family_member_invitations_families_family_id",
                  column: x => x.family_id,
                  principalSchema: "auth",
                  principalTable: "families",
                  principalColumn: "family_id",
                  onDelete: ReferentialAction.Cascade);
              table.ForeignKey(
                  name: "fk_family_member_invitations_users_invited_by",
                  column: x => x.invited_by_user_id,
                  principalSchema: "auth",
                  principalTable: "users",
                  principalColumn: "user_id",
                  onDelete: ReferentialAction.Restrict);
          });

      // Unique index on token
      migrationBuilder.CreateIndex(
          name: "ix_family_member_invitations_token",
          schema: "auth",
          table: "family_member_invitations",
          column: "token",
          unique: true);

      // Index on expires_at for cleanup queries
      migrationBuilder.CreateIndex(
          name: "ix_family_member_invitations_expires_at",
          schema: "auth",
          table: "family_member_invitations",
          column: "expires_at");

      // Composite index for dashboard queries
      migrationBuilder.CreateIndex(
          name: "ix_family_member_invitations_family_status",
          schema: "auth",
          table: "family_member_invitations",
          columns: new[] { "family_id", "status" });

      // Check constraint: email XOR username must be set
      migrationBuilder.Sql(@"
          ALTER TABLE auth.family_member_invitations
          ADD CONSTRAINT chk_email_or_username
          CHECK (
              (email IS NOT NULL AND username IS NULL) OR
              (email IS NULL AND username IS NOT NULL)
          )
      ");
  }
  ```

- [ ] Create EF Core entity configuration:

  ```csharp
  public class FamilyMemberInvitationConfiguration : IEntityTypeConfiguration<FamilyMemberInvitation>
  {
      public void Configure(EntityTypeBuilder<FamilyMemberInvitation> builder)
      {
          builder.ToTable("family_member_invitations", "auth");

          builder.HasKey(i => i.Id);
          builder.Property(i => i.Id)
              .HasColumnName("invitation_id")
              .HasConversion(new InvitationId.EfCoreValueConverter());

          builder.Property(i => i.DisplayCode)
              .HasColumnName("display_code")
              .HasMaxLength(8)
              .HasConversion(new InvitationDisplayCode.EfCoreValueConverter());

          builder.Property(i => i.FamilyId)
              .HasColumnName("family_id")
              .HasConversion(new FamilyId.EfCoreValueConverter());

          builder.Property(i => i.Email)
              .HasColumnName("email")
              .HasMaxLength(255)
              .HasConversion(new Email.EfCoreValueConverter())
              .IsRequired(false);

          builder.Property(i => i.Username)
              .HasColumnName("username")
              .HasMaxLength(20)
              .HasConversion(new Username.EfCoreValueConverter())
              .IsRequired(false);

          builder.Property(i => i.Role)
              .HasColumnName("role")
              .HasConversion<string>(); // Store as string for PostgreSQL enum

          builder.Property(i => i.Token)
              .HasColumnName("token")
              .HasMaxLength(64)
              .HasConversion(new InvitationToken.EfCoreValueConverter());

          builder.Property(i => i.ExpiresAt)
              .HasColumnName("expires_at");

          builder.Property(i => i.InvitedByUserId)
              .HasColumnName("invited_by_user_id")
              .HasConversion(new UserId.EfCoreValueConverter());

          builder.Property(i => i.Status)
              .HasColumnName("status")
              .HasConversion<int>(); // Store as integer enum

          builder.Property(i => i.Message)
              .HasColumnName("message")
              .IsRequired(false);

          builder.Property(i => i.CreatedAt)
              .HasColumnName("created_at")
              .HasDefaultValueSql("NOW()");

          builder.Property(i => i.AcceptedAt)
              .HasColumnName("accepted_at")
              .IsRequired(false);

          builder.Property(i => i.UpdatedAt)
              .HasColumnName("updated_at")
              .HasDefaultValueSql("NOW()");

          // Relationships
          builder.HasOne<Family>()
              .WithMany()
              .HasForeignKey(i => i.FamilyId)
              .OnDelete(DeleteBehavior.Cascade);

          builder.HasOne<User>()
              .WithMany()
              .HasForeignKey(i => i.InvitedByUserId)
              .OnDelete(DeleteBehavior.Restrict);
      }
  }
  ```

- [ ] Test migration on local dev database
- [ ] Verify indexes are created correctly (`\d auth.family_member_invitations` in psql)

**Acceptance Criteria:**

- Migration runs successfully without errors
- Unique index on token prevents duplicate tokens
- Composite index on (family_id, status) optimizes dashboard queries
- Check constraint enforces email XOR username
- Foreign keys enforce referential integrity
- EF Core can query and insert invitations

#### 1.D.2: User Table Extensions

**Subtasks:**

- [ ] Create migration: `AddManagedAccountFields`

  ```csharp
  protected override void Up(MigrationBuilder migrationBuilder)
  {
      migrationBuilder.AddColumn<string>(
          name: "username",
          schema: "auth",
          table: "users",
          type: "varchar(20)",
          maxLength: 20,
          nullable: true);

      migrationBuilder.AddColumn<string>(
          name: "name",
          schema: "auth",
          table: "users",
          type: "varchar(100)",
          maxLength: 100,
          nullable: true);

      migrationBuilder.AddColumn<string>(
          name: "zitadel_user_id",
          schema: "auth",
          table: "users",
          type: "varchar(255)",
          maxLength: 255,
          nullable: true);

      // Unique index on username
      migrationBuilder.CreateIndex(
          name: "ix_users_username",
          schema: "auth",
          table: "users",
          column: "username",
          unique: true,
          filter: "username IS NOT NULL");

      // Index on zitadel_user_id for lookups
      migrationBuilder.CreateIndex(
          name: "ix_users_zitadel_user_id",
          schema: "auth",
          table: "users",
          column: "zitadel_user_id",
          filter: "zitadel_user_id IS NOT NULL");
  }
  ```

- [ ] Update User entity configuration:

  ```csharp
  builder.Property(u => u.Username)
      .HasColumnName("username")
      .HasMaxLength(20)
      .HasConversion(new Username.EfCoreValueConverter())
      .IsRequired(false);

  builder.Property(u => u.PersonName)
      .HasColumnName("name")
      .HasMaxLength(100)
      .HasConversion(new PersonName.EfCoreValueConverter())
      .IsRequired(false);

  builder.Property(u => u.ZitadelUserId)
      .HasColumnName("zitadel_user_id")
      .HasMaxLength(255)
      .IsRequired(false);
  ```

- [ ] Test querying users by username

**Acceptance Criteria:**

- Migration adds columns without errors
- Unique index on username prevents duplicates
- Nullable fields allow existing users to remain unchanged
- EF Core can query by username

---

## Phase 1 Deliverables

- [ ] Domain model complete: FamilyMemberInvitation aggregate, User extensions, domain events
- [ ] GraphQL schema designed and published: mutations, queries, subscriptions
- [ ] Zitadel integration: JWT service authentication, user creation API
- [ ] Password generation service: simple/medium/strong modes with real-time preview
- [ ] Rate limiting: IP-based, 10 attempts/hour on acceptance endpoint
- [ ] Database migrations: FamilyMemberInvitation table, User extensions, indexes
- [ ] All unit tests passing (>80% backend coverage)
- [ ] Documentation updated: GraphQL schema in domain-model-microservices-map.md

---

## Phase 2: Backend Services (Days 6-10)

**Duration:** 4 days
**Agents:** backend-developer, microservices-architect, devops-engineer
**Dependencies:** Phase 1 complete

### Overview

Implement command handlers, GraphQL mutations/queries, outbox pattern, and background job processing.

### Workstream A: Command Handlers (backend-developer)

**Duration:** 3 days

#### 2.A.1: InviteFamilyMemberByEmailCommand

**Subtasks:**

- [ ] Create command:

  ```csharp
  public record InviteFamilyMemberByEmailCommand(
      FamilyId FamilyId,
      Email Email,
      UserRole Role,
      UserId InvitedByUserId,
      string? Message = null
  ) : IRequest<Result<FamilyMemberInvitation>>;
  ```

- [ ] Create command handler:

  ```csharp
  public class InviteFamilyMemberByEmailCommandHandler
      : IRequestHandler<InviteFamilyMemberByEmailCommand, Result<FamilyMemberInvitation>>
  {
      private readonly IFamilyMemberInvitationRepository _invitationRepository;
      private readonly IUserRepository _userRepository;
      private readonly IUnitOfWork _unitOfWork;

      public async Task<Result<FamilyMemberInvitation>> Handle(
          InviteFamilyMemberByEmailCommand command,
          CancellationToken cancellationToken)
      {
          // 1. Check if user with email already exists in family
          var existingMember = await _userRepository.GetByEmailAsync(command.Email);
          if (existingMember != null)
          {
              var familyMember = await _userRepository.IsInFamilyAsync(existingMember.Id, command.FamilyId);
              if (familyMember)
                  return Result.Failure<FamilyMemberInvitation>("User is already a family member");
          }

          // 2. Check for pending invitation with same email
          var pendingInvitation = await _invitationRepository.GetPendingByEmailAsync(
              command.FamilyId, command.Email);
          if (pendingInvitation != null)
              return Result.Failure<FamilyMemberInvitation>("User already has a pending invitation");

          // 3. Create invitation
          var invitation = FamilyMemberInvitation.CreateEmailInvitation(
              command.FamilyId,
              command.Email,
              command.Role,
              command.InvitedByUserId
          );

          if (!string.IsNullOrEmpty(command.Message))
              invitation.SetMessage(command.Message);

          await _invitationRepository.AddAsync(invitation);
          await _unitOfWork.CommitAsync(cancellationToken);

          return Result.Success(invitation);
      }
  }
  ```

- [ ] Create unit tests (AutoFixture + NSubstitute):
  - Happy path: invitation created successfully
  - Duplicate email: returns error
  - Pending invitation exists: returns error
  - Domain event published

**Acceptance Criteria:**

- Command handler enforces business rules (no duplicates)
- Unit tests cover all paths (>90% coverage)
- Domain events published via aggregate
- Result type used for error handling (no exceptions for business logic errors)

#### 2.A.2: CreateManagedMemberCommand

**Subtasks:**

- [ ] Create command:

  ```csharp
  public record CreateManagedMemberCommand(
      FamilyId FamilyId,
      Username Username,
      PersonName PersonName,
      UserRole Role,
      PasswordGenerationConfig PasswordConfig,
      UserId CreatedByUserId
  ) : IRequest<Result<CreateManagedMemberResult>>;

  public record CreateManagedMemberResult(
      FamilyMemberInvitation Invitation,
      User User,
      ManagedAccountCredentials Credentials
  );

  public record ManagedAccountCredentials(
      string Username,
      string Password,
      string SyntheticEmail,
      string LoginUrl
  );
  ```

- [ ] Create command handler:

  ```csharp
  public class CreateManagedMemberCommandHandler
      : IRequestHandler<CreateManagedMemberCommand, Result<CreateManagedMemberResult>>
  {
      private readonly IZitadelManagementClient _zitadelClient;
      private readonly IPasswordGenerationService _passwordService;
      private readonly IFamilyMemberInvitationRepository _invitationRepository;
      private readonly IUserRepository _userRepository;
      private readonly IUnitOfWork _unitOfWork;
      private readonly IOptions<ManagedAccountOptions> _options;

      public async Task<Result<CreateManagedMemberResult>> Handle(
          CreateManagedMemberCommand command,
          CancellationToken cancellationToken)
      {
          // 1. Check username uniqueness
          var existingUser = await _userRepository.GetByUsernameAsync(command.Username);
          if (existingUser != null)
              return Result.Failure<CreateManagedMemberResult>("Username is already taken");

          // 2. Generate password
          var password = _passwordService.GeneratePassword(command.PasswordConfig);

          // 3. Create synthetic email
          var syntheticEmail = $"{command.Username.Value}@{_options.Value.SyntheticEmailDomain}";

          // 4. Create user in Zitadel
          ZitadelUser zitadelUser;
          try
          {
              zitadelUser = await _zitadelClient.CreateHumanUserAsync(
                  command.Username.Value,
                  syntheticEmail,
                  command.PersonName.Value.Split(' ')[0], // First name
                  command.PersonName.Value.Split(' ').Skip(1).FirstOrDefault() ?? "", // Last name
                  password
              );
          }
          catch (ZitadelApiException ex) when (ex.StatusCode == HttpStatusCode.Conflict)
          {
              return Result.Failure<CreateManagedMemberResult>("Username is already taken in Zitadel");
          }
          catch (ZitadelApiException ex)
          {
              // Queue for retry via background job
              await QueueManagedAccountCreation(command, password);
              return Result.Failure<CreateManagedMemberResult>(
                  "Account creation is processing. You will be notified when complete.");
          }

          // 5. Create invitation record (for tracking)
          var invitation = FamilyMemberInvitation.CreateManagedAccountInvitation(
              command.FamilyId,
              command.Username,
              command.PersonName,
              command.Role,
              command.CreatedByUserId
          );

          // 6. Create user entity
          var user = User.CreateManagedAccount(
              command.Username,
              command.PersonName,
              command.Role,
              zitadelUser.UserId,
              _options.Value.SyntheticEmailDomain
          );

          // 7. Publish domain event
          invitation.AddDomainEvent(new ManagedAccountCreatedEvent(
              eventVersion: 1,
              invitationId: invitation.Id,
              familyId: command.FamilyId,
              userId: user.Id,
              username: command.Username,
              name: command.PersonName,
              role: command.Role,
              zitadelUserId: zitadelUser.UserId,
              createdByUserId: command.CreatedByUserId
          ));

          // 8. Mark invitation as accepted immediately
          invitation.Accept(user.Id);

          await _invitationRepository.AddAsync(invitation);
          await _userRepository.AddAsync(user);
          await _unitOfWork.CommitAsync(cancellationToken);

          // 9. Return credentials (ONLY TIME PASSWORD IS RETURNED!)
          var credentials = new ManagedAccountCredentials(
              command.Username.Value,
              password,
              syntheticEmail,
              _options.Value.LoginUrl
          );

          return Result.Success(new CreateManagedMemberResult(invitation, user, credentials));
      }

      private async Task QueueManagedAccountCreation(
          CreateManagedMemberCommand command,
          string password)
      {
          // Store in database for background job pickup
          var queuedJob = new QueuedManagedAccountCreation
          {
              FamilyId = command.FamilyId,
              Username = command.Username,
              FullName = command.PersonName,
              Role = command.Role,
              Password = password, // Encrypted!
              CreatedByUserId = command.CreatedByUserId,
              RetryCount = 0,
              Status = QueuedJobStatus.Pending,
              CreatedAt = DateTime.UtcNow
          };

          await _queuedJobRepository.AddAsync(queuedJob);
          await _unitOfWork.CommitAsync();
      }
  }

  public class ManagedAccountOptions
  {
      public string SyntheticEmailDomain { get; set; } = "noemail.family-hub.internal";
      public string LoginUrl { get; set; } = "https://app.family-hub.com/login";
  }
  ```

- [ ] Configure in `appsettings.json`:

  ```json
  {
    "ManagedAccount": {
      "SyntheticEmailDomain": "noemail.dev.family-hub.internal",
      "LoginUrl": "https://dev.app.family-hub.com/login"
    }
  }
  ```

- [ ] Create unit tests:
  - Happy path: managed account created in Zitadel and local DB
  - Duplicate username: returns error
  - Zitadel API failure: queues for retry
  - Password returned only once
  - Domain events published

**Acceptance Criteria:**

- Managed account created in Zitadel via API
- User entity created with synthetic email
- Password returned in result (never stored!)
- Zitadel failures queued for background job retry
- Unit tests with mocked Zitadel client (>90% coverage)

#### 2.A.3: BatchInviteFamilyMembersCommand

**Subtasks:**

- [ ] Create command:

  ```csharp
  public record BatchInviteFamilyMembersCommand(
      FamilyId FamilyId,
      List<EmailInvitationRequest> EmailInvitations,
      List<ManagedAccountRequest> ManagedAccounts,
      UserId InvitedByUserId
  ) : IRequest<Result<BatchInvitationResult>>;

  public record EmailInvitationRequest(Email Email, UserRole Role, string? Message);
  public record ManagedAccountRequest(Username Username, PersonName PersonName, UserRole Role, PasswordGenerationConfig PasswordConfig);

  public record BatchInvitationResult(
      List<FamilyMemberInvitation> EmailInvitations,
      List<CreateManagedMemberResult> ManagedAccounts,
      List<BatchError> Errors
  );

  public record BatchError(int Index, string Type, string Message); // Type = "email" or "managed"
  ```

- [ ] Create command handler with two-phase validation:

  ```csharp
  public class BatchInviteFamilyMembersCommandHandler
      : IRequestHandler<BatchInviteFamilyMembersCommand, Result<BatchInvitationResult>>
  {
      public async Task<Result<BatchInvitationResult>> Handle(
          BatchInviteFamilyMembersCommand command,
          CancellationToken cancellationToken)
      {
          var errors = new List<BatchError>();

          // PHASE 1: Validate all invitations
          var emailValidationErrors = await ValidateEmailInvitations(command.EmailInvitations, command.FamilyId);
          errors.AddRange(emailValidationErrors);

          var managedValidationErrors = await ValidateManagedAccounts(command.ManagedAccounts);
          errors.AddRange(managedValidationErrors);

          // If any validation errors, return immediately without processing
          if (errors.Any())
          {
              return Result.Failure<BatchInvitationResult>(
                  $"Validation failed: {errors.Count} errors found");
          }

          // PHASE 2: Process all invitations in transaction
          using var transaction = await _unitOfWork.BeginTransactionAsync();

          try
          {
              var emailInvitations = new List<FamilyMemberInvitation>();
              foreach (var request in command.EmailInvitations)
              {
                  var result = await _mediator.Send(new InviteFamilyMemberByEmailCommand(
                      command.FamilyId, request.Email, request.Role, command.InvitedByUserId, request.Message));

                  if (result.IsSuccess)
                      emailInvitations.Add(result.Value);
                  else
                      throw new Exception($"Unexpected failure: {result.Error}");
              }

              var managedAccounts = new List<CreateManagedMemberResult>();
              foreach (var request in command.ManagedAccounts)
              {
                  var result = await _mediator.Send(new CreateManagedMemberCommand(
                      command.FamilyId, request.Username, request.PersonName, request.Role,
                      request.PasswordConfig, command.InvitedByUserId));

                  if (result.IsSuccess)
                      managedAccounts.Add(result.Value);
                  else
                      throw new Exception($"Unexpected failure: {result.Error}");
              }

              await transaction.CommitAsync(cancellationToken);

              return Result.Success(new BatchInvitationResult(
                  emailInvitations, managedAccounts, new List<BatchError>()));
          }
          catch (Exception ex)
          {
              await transaction.RollbackAsync(cancellationToken);
              return Result.Failure<BatchInvitationResult>($"Batch processing failed: {ex.Message}");
          }
      }

      private async Task<List<BatchError>> ValidateEmailInvitations(
          List<EmailInvitationRequest> requests, FamilyId familyId)
      {
          var errors = new List<BatchError>();

          for (int i = 0; i < requests.Count; i++)
          {
              var request = requests[i];

              // Check duplicate within batch
              if (requests.Skip(i + 1).Any(r => r.Email == request.Email))
                  errors.Add(new BatchError(i, "email", $"Duplicate email in batch: {request.Email}"));

              // Check existing member
              var existingMember = await _userRepository.GetByEmailAsync(request.Email);
              if (existingMember != null && await _userRepository.IsInFamilyAsync(existingMember.Id, familyId))
                  errors.Add(new BatchError(i, "email", $"User is already a family member: {request.Email}"));

              // Check pending invitation
              var pendingInvitation = await _invitationRepository.GetPendingByEmailAsync(familyId, request.Email);
              if (pendingInvitation != null)
                  errors.Add(new BatchError(i, "email", $"User already has a pending invitation: {request.Email}"));
          }

          return errors;
      }

      private async Task<List<BatchError>> ValidateManagedAccounts(List<ManagedAccountRequest> requests)
      {
          var errors = new List<BatchError>();

          for (int i = 0; i < requests.Count; i++)
          {
              var request = requests[i];

              // Check duplicate within batch
              if (requests.Skip(i + 1).Any(r => r.Username == request.Username))
                  errors.Add(new BatchError(i, "managed", $"Duplicate username in batch: {request.Username}"));

              // Check existing user
              var existingUser = await _userRepository.GetByUsernameAsync(request.Username);
              if (existingUser != null)
                  errors.Add(new BatchError(i, "managed", $"Username is already taken: {request.Username}"));
          }

          return errors;
      }
  }
  ```

- [ ] Create unit tests:
  - Happy path: 10 invitations (5 email, 5 managed) all succeed
  - Validation failure: duplicate email in batch, all rolled back
  - Mid-batch failure: exception during processing, all rolled back
  - Performance: 10 invitations complete in <2 seconds

**Acceptance Criteria:**

- Two-phase validation: all validated before any processed
- Transaction ensures atomic success/failure
- Errors include index for frontend mapping
- Performance target: <2s for 10 invitations
- Unit tests verify rollback on failure

#### 2.A.4: CancelInvitationCommand, ResendInvitationCommand, UpdateInvitationRoleCommand

**Subtasks:**

- [ ] Create `CancelInvitationCommand` and handler
- [ ] Create `ResendInvitationCommand` and handler
- [ ] Create `UpdateInvitationRoleCommand` and handler
- [ ] Unit tests for all three commands

**Acceptance Criteria:**

- Cancel: marks invitation as canceled, publishes event
- Resend: generates new token, extends expiration, publishes event
- UpdateRole: changes role while pending, publishes event
- Authorization: only Owner/Admin can perform these actions

### Workstream B: GraphQL Mutations & Queries (backend-developer)

**Duration:** 2 days (parallel with 2.A)

#### 2.B.1: Implement GraphQL Mutations

**Subtasks:**

- [ ] Create `FamilyMemberInvitationMutations` class:

  ```csharp
  [ExtendObjectType(typeof(Mutation))]
  public class FamilyMemberInvitationMutations
  {
      [Authorize(Policy = "RequireOwnerOrAdmin")]
      public async Task<InviteFamilyMemberByEmailPayload> InviteFamilyMemberByEmailAsync(
          InviteFamilyMemberByEmailInput input,
          [Service] IMediator mediator,
          [Service] IHttpContextAccessor httpContext)
      {
          var userId = httpContext.HttpContext.User.GetUserId();

          var command = new InviteFamilyMemberByEmailCommand(
              FamilyId.From(input.FamilyId),
              Email.From(input.Email),
              input.Role,
              userId,
              input.Message
          );

          var result = await mediator.Send(command);

          if (result.IsFailure)
          {
              return new InviteFamilyMemberByEmailPayload
              {
                  Errors = new[]
                  {
                      new MutationError
                      {
                          Code = ErrorCode.ValidationFailed,
                          Message = result.Error,
                          Field = "email"
                      }
                  }
              };
          }

          return new InviteFamilyMemberByEmailPayload
          {
              Invitation = MapToGraphQL(result.Value)
          };
      }

      [Authorize(Policy = "RequireOwnerOrAdmin")]
      public async Task<CreateManagedMemberPayload> CreateManagedMemberAsync(
          CreateManagedMemberInput input,
          [Service] IMediator mediator,
          [Service] IHttpContextAccessor httpContext)
      {
          var userId = httpContext.HttpContext.User.GetUserId();

          var passwordConfig = MapPasswordConfig(input.PasswordConfig);

          var command = new CreateManagedMemberCommand(
              FamilyId.From(input.FamilyId),
              Username.From(input.Username),
              PersonName.From(input.PersonName),
              input.Role,
              passwordConfig,
              userId
          );

          var result = await mediator.Send(command);

          if (result.IsFailure)
          {
              return new CreateManagedMemberPayload
              {
                  Errors = new[]
                  {
                      new MutationError
                      {
                          Code = ErrorCode.ZitadelApiError,
                          Message = result.Error
                      }
                  }
              };
          }

          var value = result.Value;

          return new CreateManagedMemberPayload
          {
              Invitation = MapToGraphQL(value.Invitation),
              User = MapToGraphQL(value.User),
              Credentials = new ManagedAccountCredentialsType
              {
                  Username = value.Credentials.Username,
                  Password = value.Credentials.Password,
                  SyntheticEmail = value.Credentials.SyntheticEmail,
                  LoginUrl = value.Credentials.LoginUrl
              }
          };
      }

      [Authorize(Policy = "RequireOwnerOrAdmin")]
      public async Task<BatchInviteFamilyMembersPayload> BatchInviteFamilyMembersAsync(
          BatchInviteFamilyMembersInput input,
          [Service] IMediator mediator,
          [Service] IHttpContextAccessor httpContext)
      {
          // Similar to above, calls BatchInviteFamilyMembersCommand
          // ...
      }

      [Authorize(Policy = "RequireOwnerOrAdmin")]
      public async Task<CancelInvitationPayload> CancelInvitationAsync(
          CancelInvitationInput input,
          [Service] IMediator mediator)
      {
          // Calls CancelInvitationCommand
          // ...
      }

      // Similarly for ResendInvitation and UpdateInvitationRole
  }
  ```

- [ ] Create authorization policy:

  ```csharp
  services.AddAuthorization(options =>
  {
      options.AddPolicy("RequireOwnerOrAdmin", policy =>
          policy.RequireAssertion(context =>
              context.User.HasClaim("role", "Owner") ||
              context.User.HasClaim("role", "Admin")
          ));
  });
  ```

- [ ] Integration tests for mutations:
  - InviteFamilyMemberByEmail: success path
  - CreateManagedMember: success path with credentials returned
  - BatchInviteFamilyMembers: mixed mode (3 email, 2 managed)
  - Authorization: non-admin returns 403 Forbidden

**Acceptance Criteria:**

- All mutations functional and tested
- Authorization enforced via policy
- GraphQL input → Command mapping correct
- Error responses use MutationError type
- Integration tests with TestServer + PostgreSQL (TestContainers)

#### 2.B.2: Implement GraphQL Queries

**Subtasks:**

- [ ] Create `FamilyMemberInvitationQueries` class:

  ```csharp
  [ExtendObjectType(typeof(Query))]
  public class FamilyMemberInvitationQueries
  {
      [Authorize]
      public async Task<List<FamilyMemberType>> FamilyMembersAsync(
          Guid familyId,
          [Service] IUserRepository userRepository,
          [Service] IHttpContextAccessor httpContext)
      {
          var currentUserId = httpContext.HttpContext.User.GetUserId();

          // Verify user is member of family (authorization)
          if (!await userRepository.IsInFamilyAsync(currentUserId, FamilyId.From(familyId)))
              throw new UnauthorizedAccessException("You are not a member of this family");

          var members = await userRepository.GetFamilyMembersAsync(FamilyId.From(familyId));

          return members.Select(MapToFamilyMemberType).ToList();
      }

      [Authorize(Policy = "RequireOwnerOrAdmin")]
      public async Task<List<PendingInvitationType>> PendingInvitationsAsync(
          Guid familyId,
          [Service] IFamilyMemberInvitationRepository invitationRepository)
      {
          var invitations = await invitationRepository.GetPendingByFamilyIdAsync(FamilyId.From(familyId));

          return invitations.Select(MapToPendingInvitationType).ToList();
      }

      public async Task<PendingInvitationType?> InvitationByTokenAsync(
          string token,
          [Service] IFamilyMemberInvitationRepository invitationRepository)
      {
          var invitation = await invitationRepository.GetByTokenAsync(InvitationToken.From(token));

          if (invitation == null || invitation.Status != InvitationStatus.Pending)
              return null;

          return MapToPendingInvitationType(invitation);
      }
  }
  ```

- [ ] Integration tests:
  - FamilyMembers: returns all members for authorized user
  - PendingInvitations: returns pending invitations for Owner/Admin
  - InvitationByToken: returns invitation for valid token
  - Authorization: non-member cannot query family members

**Acceptance Criteria:**

- Queries enforce authorization
- Performance: <100ms for 100-member family
- Null safety: returns null for not found, not exception
- Integration tests verify authorization logic

### Workstream C: Outbox Pattern & Event Publishing (microservices-architect)

**Duration:** 3 days

#### 2.C.1: Outbox Table & Repository

**Subtasks:**

- [ ] Create migration: `CreateOutboxEventsTable`

  ```csharp
  protected override void Up(MigrationBuilder migrationBuilder)
  {
      migrationBuilder.CreateTable(
          name: "outbox_events",
          schema: "auth",
          columns: table => new
          {
              event_id = table.Column<Guid>(nullable: false),
              event_type = table.Column<string>(maxLength: 255, nullable: false),
              event_version = table.Column<int>(nullable: false),
              aggregate_type = table.Column<string>(maxLength: 255, nullable: false),
              aggregate_id = table.Column<Guid>(nullable: false),
              payload = table.Column<string>(type: "jsonb", nullable: false),
              created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
              processed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
              status = table.Column<int>(nullable: false), // Pending = 0, Processed = 1, Failed = 2
              retry_count = table.Column<int>(nullable: false, defaultValue: 0),
              error_message = table.Column<string>(nullable: true)
          },
          constraints: table =>
          {
              table.PrimaryKey("pk_outbox_events", x => x.event_id);
          });

      migrationBuilder.CreateIndex(
          name: "ix_outbox_events_status_created_at",
          schema: "auth",
          table: "outbox_events",
          columns: new[] { "status", "created_at" });

      migrationBuilder.CreateIndex(
          name: "ix_outbox_events_created_at",
          schema: "auth",
          table: "outbox_events",
          column: "created_at");
  }
  ```

- [ ] Create `OutboxEvent` entity:

  ```csharp
  public class OutboxEvent
  {
      public Guid EventId { get; set; }
      public string EventType { get; set; } = default!;
      public int EventVersion { get; set; }
      public string AggregateType { get; set; } = default!;
      public Guid AggregateId { get; set; }
      public string Payload { get; set; } = default!; // JSON
      public DateTime CreatedAt { get; set; }
      public DateTime? ProcessedAt { get; set; }
      public OutboxEventStatus Status { get; set; }
      public int RetryCount { get; set; }
      public string? ErrorMessage { get; set; }
  }

  public enum OutboxEventStatus
  {
      Pending = 0,
      Processed = 1,
      Failed = 2
  }
  ```

- [ ] Create `IOutboxEventRepository` interface and implementation
- [ ] Unit tests for outbox repository

**Acceptance Criteria:**

- Migration creates table with indexes
- Outbox events stored as JSON in PostgreSQL
- Repository can query pending events efficiently
- Unit tests verify CRUD operations

#### 2.C.2: Domain Event Interceptor (Save to Outbox)

**Subtasks:**

- [ ] Create `DomainEventOutboxInterceptor`:

  ```csharp
  public class DomainEventOutboxInterceptor : SaveChangesInterceptor
  {
      public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(
          DbContextEventData eventData,
          InterceptionResult<int> result,
          CancellationToken cancellationToken = default)
      {
          var dbContext = eventData.Context;
          if (dbContext == null) return result;

          // Get all aggregates with domain events
          var aggregates = dbContext.ChangeTracker
              .Entries<AggregateRoot>()
              .Where(e => e.Entity.DomainEvents.Any())
              .Select(e => e.Entity)
              .ToList();

          // Convert domain events to outbox events
          var outboxEvents = aggregates
              .SelectMany(aggregate => aggregate.DomainEvents.Select(@event => new OutboxEvent
              {
                  EventId = Guid.NewGuid(),
                  EventType = @event.GetType().Name,
                  EventVersion = @event.EventVersion,
                  AggregateType = aggregate.GetType().Name,
                  AggregateId = aggregate.Id.Value, // Assume AggregateRoot<TId> has Id property
                  Payload = JsonSerializer.Serialize(@event, new JsonSerializerOptions
                  {
                      PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                  }),
                  CreatedAt = DateTime.UtcNow,
                  Status = OutboxEventStatus.Pending,
                  RetryCount = 0
              }))
              .ToList();

          // Add outbox events to dbContext
          await dbContext.Set<OutboxEvent>().AddRangeAsync(outboxEvents, cancellationToken);

          // Clear domain events from aggregates (important!)
          foreach (var aggregate in aggregates)
              aggregate.ClearDomainEvents();

          return result;
      }
  }
  ```

- [ ] Register interceptor in `AuthDbContext`:

  ```csharp
  protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
  {
      optionsBuilder.AddInterceptors(new DomainEventOutboxInterceptor());
  }
  ```

- [ ] Integration test: Domain event saved to outbox on SaveChangesAsync

**Acceptance Criteria:**

- Domain events automatically saved to outbox table
- Events serialized as JSON with correct format
- Aggregate events cleared after save (no duplicate publishing)
- Integration test verifies outbox contains event after SaveChanges

#### 2.C.3: Outbox Event Publisher (Background Worker)

**Subtasks:**

- [ ] Create `OutboxEventPublisher` background service:

  ```csharp
  public class OutboxEventPublisher : BackgroundService
  {
      private readonly IServiceProvider _serviceProvider;
      private readonly ILogger<OutboxEventPublisher> _logger;
      private readonly OutboxOptions _options;

      public OutboxEventPublisher(
          IServiceProvider serviceProvider,
          ILogger<OutboxEventPublisher> logger,
          IOptions<OutboxOptions> options)
      {
          _serviceProvider = serviceProvider;
          _logger = logger;
          _options = options.Value;
      }

      protected override async Task ExecuteAsync(CancellationToken stoppingToken)
      {
          while (!stoppingToken.IsCancellationRequested)
          {
              try
              {
                  await ProcessPendingEventsAsync(stoppingToken);
              }
              catch (Exception ex)
              {
                  _logger.LogError(ex, "Error processing outbox events");
              }

              await Task.Delay(_options.PollingIntervalMs, stoppingToken);
          }
      }

      private async Task ProcessPendingEventsAsync(CancellationToken cancellationToken)
      {
          using var scope = _serviceProvider.CreateScope();
          var repository = scope.ServiceProvider.GetRequiredService<IOutboxEventRepository>();
          var rabbitMqPublisher = scope.ServiceProvider.GetRequiredService<IMessageBrokerPublisher>();

          // Fetch pending events (batch of 100)
          var pendingEvents = await repository.GetPendingEventsAsync(100, cancellationToken);

          foreach (var outboxEvent in pendingEvents)
          {
              try
              {
                  // Publish to RabbitMQ with exponential backoff retry
                  await PublishWithRetryAsync(rabbitMqPublisher, outboxEvent, cancellationToken);

                  // Mark as processed
                  outboxEvent.Status = OutboxEventStatus.Processed;
                  outboxEvent.ProcessedAt = DateTime.UtcNow;

                  await repository.UpdateAsync(outboxEvent, cancellationToken);
              }
              catch (Exception ex)
              {
                  _logger.LogError(ex, "Failed to publish event {EventId} after max retries", outboxEvent.EventId);

                  outboxEvent.Status = OutboxEventStatus.Failed;
                  outboxEvent.ErrorMessage = ex.Message;
                  outboxEvent.RetryCount++;

                  await repository.UpdateAsync(outboxEvent, cancellationToken);
              }
          }
      }

      private async Task PublishWithRetryAsync(
          IMessageBrokerPublisher publisher,
          OutboxEvent outboxEvent,
          CancellationToken cancellationToken)
      {
          int maxRetries = 5;
          int[] delays = { 1000, 2000, 5000, 15000, 60000 }; // 1s, 2s, 5s, 15s, 1min

          for (int attempt = 0; attempt < maxRetries; attempt++)
          {
              try
              {
                  await publisher.PublishAsync(
                      exchange: "family_hub_events",
                      routingKey: outboxEvent.EventType,
                      message: outboxEvent.Payload,
                      cancellationToken: cancellationToken
                  );

                  return; // Success
              }
              catch (Exception ex) when (attempt < maxRetries - 1)
              {
                  _logger.LogWarning(ex, "Retry {Attempt}/{MaxRetries} for event {EventId}",
                      attempt + 1, maxRetries, outboxEvent.EventId);

                  await Task.Delay(delays[attempt], cancellationToken);
              }
          }

          throw new Exception($"Failed to publish event after {maxRetries} retries");
      }
  }

  public class OutboxOptions
  {
      public int PollingIntervalMs { get; set; } = 5000; // 5 seconds
      public int MaxRetryAttempts { get; set; } = 5;
  }
  ```

- [ ] Configure in `appsettings.json`:

  ```json
  {
    "Outbox": {
      "PollingIntervalMs": 5000,
      "MaxRetryAttempts": 5
    }
  }
  ```

- [ ] Register in `Program.cs`:

  ```csharp
  services.Configure<OutboxOptions>(configuration.GetSection("Outbox"));
  services.AddHostedService<OutboxEventPublisher>();
  ```

- [ ] Integration test: Outbox event published to RabbitMQ and marked as processed

**Acceptance Criteria:**

- Background worker polls outbox every 5 seconds
- Events published to RabbitMQ with exponential backoff retry
- Failed events marked with error message and retry count
- Circuit breaker: stop retrying after 5 attempts (status = Failed)
- Integration test with TestContainers RabbitMQ verifies publishing

#### 2.C.4: Outbox Cleanup Job (Archive Old Events)

**Subtasks:**

- [ ] Create Quartz.NET job: `OutboxCleanupJob`

  ```csharp
  public class OutboxCleanupJob : IJob
  {
      private readonly IServiceProvider _serviceProvider;

      public async Task Execute(IJobExecutionContext context)
      {
          using var scope = _serviceProvider.CreateScope();
          var repository = scope.ServiceProvider.GetRequiredService<IOutboxEventRepository>();

          // Archive events older than 90 days
          var cutoffDate = DateTime.UtcNow.AddDays(-90);
          var oldEvents = await repository.GetProcessedEventsOlderThanAsync(cutoffDate);

          // Move to archive table (or delete if no archive needed)
          foreach (var @event in oldEvents)
          {
              await repository.ArchiveAsync(@event);
          }
      }
  }
  ```

- [ ] Schedule job to run daily at 2 AM:

  ```csharp
  var job = JobBuilder.Create<OutboxCleanupJob>()
      .WithIdentity("OutboxCleanup", "Maintenance")
      .Build();

  var trigger = TriggerBuilder.Create()
      .WithIdentity("OutboxCleanupTrigger", "Maintenance")
      .WithCronSchedule("0 0 2 * * ?") // Daily at 2 AM
      .Build();

  await scheduler.ScheduleJob(job, trigger);
  ```

**Acceptance Criteria:**

- Job runs daily at 2 AM
- Events older than 90 days are archived
- Performance: archiving 10,000 events takes <30 seconds

### Workstream D: Quartz.NET Setup & Background Jobs (devops-engineer)

**Duration:** 2 days

#### 2.D.1: Quartz.NET Installation & Configuration

**Subtasks:**

- [ ] Install Quartz.NET packages:

  ```bash
  dotnet add package Quartz
  dotnet add package Quartz.AspNetCore
  dotnet add package Quartz.Serialization.Json
  ```

- [ ] Configure Quartz.NET in `Program.cs`:

  ```csharp
  services.AddQuartz(q =>
  {
      q.UseMicrosoftDependencyInjectionJobFactory();
      q.UseSimpleTypeLoader();
      q.UseInMemoryStore();
      q.UseDefaultThreadPool(tp => { tp.MaxConcurrency = 10; });

      // Add jobs here (see next tasks)
  });

  services.AddQuartzHostedService(options =>
  {
      options.WaitForJobsToComplete = true;
  });
  ```

- [ ] Verify Quartz.NET starts correctly in dev environment

**Acceptance Criteria:**

- Quartz.NET configured and starts without errors
- In-memory job store used (sufficient for single instance)
- Logs show scheduler started

#### 2.D.2: Managed Account Retry Job

**Subtasks:**

- [ ] Create `ManagedAccountRetryJob`:

  ```csharp
  public class ManagedAccountRetryJob : IJob
  {
      private readonly IServiceProvider _serviceProvider;
      private readonly ILogger<ManagedAccountRetryJob> _logger;

      public async Task Execute(IJobExecutionContext context)
      {
          using var scope = _serviceProvider.CreateScope();
          var repository = scope.ServiceProvider.GetRequiredService<IQueuedManagedAccountRepository>();
          var zitadelClient = scope.ServiceProvider.GetRequiredService<IZitadelManagementClient>();
          var userRepository = scope.ServiceProvider.GetRequiredService<IUserRepository>();
          var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

          // Fetch pending/failed jobs
          var queuedJobs = await repository.GetPendingOrFailedAsync();

          foreach (var job in queuedJobs)
          {
              try
              {
                  // Attempt to create user in Zitadel
                  var zitadelUser = await zitadelClient.CreateHumanUserAsync(
                      job.Username.Value,
                      $"{job.Username.Value}@{context.JobDetail.JobDataMap.GetString("SyntheticEmailDomain")}",
                      job.PersonName.Value.Split(' ')[0],
                      job.PersonName.Value.Split(' ').Skip(1).FirstOrDefault() ?? "",
                      DecryptPassword(job.EncryptedPassword) // Decrypt password
                  );

                  // Create user entity
                  var user = User.CreateManagedAccount(
                      job.Username,
                      job.PersonName,
                      job.Role,
                      zitadelUser.UserId,
                      context.JobDetail.JobDataMap.GetString("SyntheticEmailDomain")!
                  );

                  await userRepository.AddAsync(user);

                  // Mark job as completed
                  job.Status = QueuedJobStatus.Completed;
                  job.CompletedAt = DateTime.UtcNow;

                  await repository.UpdateAsync(job);
                  await unitOfWork.CommitAsync();

                  _logger.LogInformation("Successfully created managed account for {Username} after retry",
                      job.Username.Value);

                  // TODO: Send notification to creator that account is ready
              }
              catch (Exception ex)
              {
                  _logger.LogError(ex, "Failed to create managed account for {Username}, attempt {RetryCount}",
                      job.Username.Value, job.RetryCount);

                  job.RetryCount++;
                  job.LastErrorMessage = ex.Message;
                  job.LastAttemptAt = DateTime.UtcNow;

                  // Exponential backoff: 1min, 5min, 15min, 1hr, 4hr
                  var delays = new[] { 1, 5, 15, 60, 240 }; // Minutes
                  if (job.RetryCount < delays.Length)
                  {
                      job.NextRetryAt = DateTime.UtcNow.AddMinutes(delays[job.RetryCount]);
                      job.Status = QueuedJobStatus.Pending;
                  }
                  else
                  {
                      job.Status = QueuedJobStatus.Failed;
                      job.NextRetryAt = null;

                      // TODO: Send notification to creator that account creation failed
                  }

                  await repository.UpdateAsync(job);
                  await unitOfWork.CommitAsync();
              }
          }
      }
  }
  ```

- [ ] Create `QueuedManagedAccountCreation` entity and repository
- [ ] Schedule job to run every 1 minute:

  ```csharp
  services.AddQuartz(q =>
  {
      q.AddJob<ManagedAccountRetryJob>(opts => opts.WithIdentity("ManagedAccountRetry"));

      q.AddTrigger(opts => opts
          .ForJob("ManagedAccountRetry")
          .WithIdentity("ManagedAccountRetryTrigger")
          .WithSimpleSchedule(x => x
              .WithIntervalInMinutes(1)
              .RepeatForever())
      );
  });
  ```

**Acceptance Criteria:**

- Job runs every 1 minute
- Retries failed Zitadel account creations with exponential backoff
- Gives up after 5 attempts (status = Failed)
- Integration test verifies retry logic

#### 2.D.3: Expired Invitation Cleanup Job

**Subtasks:**

- [ ] Create `ExpiredInvitationCleanupJob`:

  ```csharp
  public class ExpiredInvitationCleanupJob : IJob
  {
      public async Task Execute(IJobExecutionContext context)
      {
          using var scope = _serviceProvider.CreateScope();
          var repository = scope.ServiceProvider.GetRequiredService<IFamilyMemberInvitationRepository>();

          // Hard delete invitations expired more than 30 days ago
          var cutoffDate = DateTime.UtcNow.AddDays(-30);
          var expiredInvitations = await repository.GetExpiredOlderThanAsync(cutoffDate);

          foreach (var invitation in expiredInvitations)
          {
              await repository.DeleteAsync(invitation);
          }

          await unitOfWork.CommitAsync();

          _logger.LogInformation("Deleted {Count} expired invitations older than 30 days",
              expiredInvitations.Count);
      }
  }
  ```

- [ ] Schedule job to run daily at 3 AM:

  ```csharp
  services.AddQuartz(q =>
  {
      q.AddJob<ExpiredInvitationCleanupJob>(opts => opts.WithIdentity("ExpiredInvitationCleanup"));

      q.AddTrigger(opts => opts
          .ForJob("ExpiredInvitationCleanup")
          .WithIdentity("ExpiredInvitationCleanupTrigger")
          .WithCronSchedule("0 0 3 * * ?") // Daily at 3 AM
      );
  });
  ```

**Acceptance Criteria:**

- Job runs daily at 3 AM
- Deletes invitations expired >30 days ago (14-day expiration + 30-day grace = 44 days total)
- Performance: deleting 1,000 records takes <5 seconds

---

## Phase 2 Deliverables

- [ ] All command handlers implemented and tested (>90% coverage)
- [ ] GraphQL mutations and queries functional
- [ ] Outbox pattern: domain events saved to outbox, published by background worker
- [ ] Quartz.NET: managed account retry job, expired invitation cleanup job
- [ ] Integration tests: mutations + queries with TestServer + PostgreSQL
- [ ] E2E RabbitMQ tests: events published and consumable

---

(... Continue with Phases 3-6 in similar detail ...)

Due to message length constraints, I'll summarize the remaining phases:

## Phase 3: Frontend Wizard & Real-time Features (Days 11-15)

- Generic wizard framework (frontend-developer)
- Reactive forms with FormArray (frontend-developer)
- Password strength UI with real-time preview (ui-designer)
- GraphQL Apollo integration (frontend-developer)
- Hot Chocolate subscriptions + Redis (backend-developer + frontend-developer)
- SessionStorage state persistence (frontend-developer)

## Phase 4: Management UI #26 (Days 16-18)

- Family settings page layout (frontend-developer)
- Current members table (frontend-developer)
- Pending invitations dashboard (frontend-developer)
- Invite member modal (reuses wizard components) (frontend-developer)
- Real-time subscription integration (frontend-developer)

## Phase 5: Testing & Quality (Days 19-22)

- E2E tests with Playwright + TestContainers (test-automator)
- Integration test suite completion (qa-expert)
- WCAG 2.1 AA compliance validation (accessibility-tester)
- Performance testing: batch processing, password generation (qa-expert)

## Phase 6: Review & Polish (Days 23-24)

- Architecture review (architect-reviewer)
- Code review (code-reviewer)
- Documentation updates (technical-writer or backend-developer)
- Final integration testing and bug fixes

---

## Success Criteria

From issues #24, #25, #26, the feature is considered complete when:

### Backend

- [x] FamilyMemberInvitation aggregate functional with all business logic
- [x] User entity supports managed accounts with synthetic emails
- [x] GraphQL mutations: inviteFamilyMemberByEmail, createManagedMember, batchInviteFamilyMembers, cancelInvitation, resendInvitation, updateInvitationRole
- [x] GraphQL queries: familyMembers, pendingInvitations, invitationByToken
- [x] GraphQL subscriptions: familyMembersChanged, pendingInvitationsChanged
- [x] Zitadel integration: JWT service auth, managed account creation
- [x] Outbox pattern: reliable event publishing
- [x] Quartz.NET jobs: managed account retries, expired invitation cleanup, outbox archival
- [x] Rate limiting: 10 attempts/hour on invitation acceptance
- [x] All unit tests passing (>80% backend coverage)
- [x] All integration tests passing (GraphQL + database)

### Frontend

- [x] Generic wizard framework supports N steps
- [x] Family creation wizard: 2 steps (Family Info, Invite Members)
- [x] Wizard Step 2: mixed-mode batch invitations (email + managed accounts)
- [x] Password strength UI: slider (12-32 chars) + checkboxes (character types)
- [x] Real-time password preview updates as user adjusts settings
- [x] Password modal: displays username, password, synthetic email, copy button
- [x] SessionStorage: wizard state persists across page refresh
- [x] Auto-save on blur: prevents data loss
- [x] Family management UI: current members table, pending invitations dashboard
- [x] Invite member modal: reuses wizard Step 2 components
- [x] Real-time updates: GraphQL subscriptions with Hot Chocolate + Redis
- [x] Dual validation: client-side (Angular) + backend (GraphQL)
- [x] Error handling: displays GraphQL errors inline
- [x] WCAG 2.1 AA compliance: keyboard navigation, focus management, ARIA labels

### Testing

- [x] Unit tests: >80% backend coverage, >70% frontend coverage
- [x] Integration tests: all GraphQL mutations/queries with PostgreSQL
- [x] E2E tests: 10+ scenarios covering happy paths and edge cases
  - Email invitation flow
  - Managed account creation flow
  - Mixed batch invitations
  - Wizard skip flow
  - Management UI invitation flow
  - Real-time subscription updates
  - Password modal copy functionality
  - Validation error scenarios
  - Authorization failures
  - Zitadel API failures (queued for retry)
- [x] Performance benchmarks:
  - Batch processing: 10 invitations in <2 seconds
  - Password generation: <50ms for all strength levels
  - GraphQL queries: <100ms for 100-member family
  - Real-time subscription latency: <500ms

### Infrastructure

- [x] Docker Compose: PostgreSQL, RabbitMQ, Redis, Zitadel (dev environment)
- [x] Quartz.NET configured and jobs scheduled
- [x] Rate limiting middleware active
- [x] Outbox background worker running
- [x] Logging: structured logs for all critical paths
- [x] Configuration: environment-specific settings (dev/staging/prod)

### Documentation

- [x] GraphQL schema documented in domain-model-microservices-map.md
- [x] Domain events documented in event-chains-reference.md
- [x] ADR created: ADR-005-FAMILY-INVITATION-SYSTEM.md
- [x] MANAGED-ACCOUNT-SETUP.md updated with new flows
- [x] README updated with setup instructions (Zitadel service account, Redis, Quartz.NET)

---

## Risks & Mitigation

| Risk | Impact | Probability | Mitigation |
|------|--------|-------------|------------|
| Zitadel API instability | High | Medium | Background job retry, queue for processing, extensive error handling |
| Redis infrastructure adds deployment complexity | Medium | High | Document setup clearly, Docker Compose for dev, optional for single-instance |
| TestContainers slow in CI/CD | Medium | High | Parallel test execution, 30s timeout budget, selective test running |
| Outbox pattern increases code complexity | Low | High | Comprehensive unit tests, clear documentation, code review by microservices-architect |
| Generic wizard framework over-engineered | Medium | Low | Start minimal, iterate based on future wizard needs, validate design early |
| Password strength UI confusing to users | Medium | Medium | User testing in Phase 5, clear tooltips and examples, preview shows output |
| GraphQL subscriptions don't scale | High | Low | Redis PubSub from day 1, tested with multiple instances, fallback to polling |
| 22-24 day timeline too optimistic | High | Medium | Parallel workstreams, daily standup with agents, cut scope if needed (defer QR code, simplify real-time) |

---

## Agent Assignments Summary

1. **database-administrator**: Phase 0 (enum migration), Phase 1.D (table schemas), Phase 2 (support)
2. **backend-developer**: Phase 1.A (domain model), Phase 2.A (commands), Phase 2.B (GraphQL), Phase 3 (subscriptions)
3. **api-designer**: Phase 1.B (GraphQL schema), Phase 2 (review mutations)
4. **security-engineer**: Phase 1.C (Zitadel, passwords, rate limiting)
5. **microservices-architect**: Phase 2.C (outbox pattern, event publishing)
6. **devops-engineer**: Phase 2.D (Quartz.NET, background jobs, infrastructure)
7. **frontend-developer**: Phase 3 (wizard framework, forms, Apollo), Phase 4 (management UI)
8. **ui-designer**: Phase 3 (password strength UI, modal design)
9. **test-automator**: Phase 5 (E2E tests with Playwright + TestContainers)
10. **qa-expert**: Phase 5 (test strategy, integration tests)
11. **accessibility-tester**: Phase 5 (WCAG 2.1 AA compliance)
12. **architect-reviewer**: Phase 6 (architecture review)
13. **code-reviewer**: Phase 6 (code quality, patterns)

---

**Next Steps:**

1. User approves this implementation plan
2. Kick off Phase 0 (Terminology Update) with database-administrator agent
3. Daily progress tracking via GitHub issues and project board
4. Weekly architecture review meetings
5. Continuous integration: merge to main after each phase completion

**Total Estimated Duration:** 22-24 days
**Target Completion:** End of Month 2, Phase 1 MVP

---

_Generated: 2026-01-04 by Claude Code (Sonnet 4.5)_
_Epic: #24, Sub-Issues: #25, #26_
_Branch: feature/family-member-invitation-system_
