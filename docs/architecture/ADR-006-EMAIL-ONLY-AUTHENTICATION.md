# ADR-006: Email-Only Authentication via Zitadel OAuth 2.0

## Status

**ACCEPTED** - January 2026

Supersedes: [ADR-005: Dual Authentication with Zitadel](archive/ADR-005-DUAL-AUTHENTICATION-ZITADEL.md) (REVERSED)

## Context

Family Hub authentication system was initially designed with dual authentication to support:

1. **Email-based OAuth 2.0** for regular users (adults with email addresses)
2. **Username-based authentication** for managed accounts (children, elderly family members without email)

After partially implementing this approach (ADR-005), we discovered:

### Technical Challenges

1. **Zitadel API Limitations:**
   - Complex service account authentication with JWT assertions
   - Frequent 401/403 errors requiring extensive retry logic
   - No clear benefit over standard email-only OAuth flow

2. **Implementation Complexity:**
   - Synthetic email mapping (`username@noemail.family-hub.internal`)
   - Dual authentication flows in frontend and backend
   - Complex migration path when managed accounts get real emails

3. **Development Velocity:**
   - 100+ files affected for managed account support
   - Significant testing surface area
   - Slower time to market for MVP

### Strategic Considerations

1. **MVP Focus:** Need to launch cloud-based service quickly (12 months vs 15-18 months)
2. **User Research Gap:** No validation that managed accounts are critical for MVP
3. **Clean Slate:** No managed accounts exist in production - reversal is risk-free
4. **Incremental Approach:** Can add managed accounts in Phase 7+ if user research validates need

## Decision

**Simplify authentication to email-only OAuth 2.0 via Zitadel.**

### Authentication Flow

```
1. User clicks "Sign in with Email"
2. Frontend calls GetZitadelAuthUrl GraphQL query
3. Backend generates PKCE parameters (code_verifier, state)
4. User redirected to Zitadel OAuth 2.0 authorization page
5. User authenticates with email/password (Zitadel handles this)
6. Zitadel redirects back with authorization code
7. Frontend calls CompleteLogin GraphQL mutation
8. Backend exchanges code for access/refresh tokens via Zitadel token endpoint
9. User session established with JWT stored in httpOnly cookie
```

### User Management

- **User Creation:** `User.CreateFromOAuth()` - ONLY creation method
- **Identity Source:** Zitadel (external OAuth 2.0 / OIDC provider)
- **User Properties:**
  - `Email` (required, from Zitadel)
  - `EmailVerified` (boolean, from Zitadel)
  - `ExternalUserId` (Zitadel user ID)
  - `ExternalProvider` ("zitadel")
  - `FamilyId`, `Role`, `CreatedAt`, `UpdatedAt`

### Invitation System

- **Email Invitations Only:** Invite family members by email address
- **Invitation Flow:**
  1. Owner/Admin sends email invitation via GraphQL `inviteFamilyMemberByEmail` mutation
  2. Invitee receives email with secure token link
  3. Invitee clicks link, redirects to Zitadel for OAuth registration
  4. After OAuth complete, invitation auto-accepted via `acceptInvitation` mutation
  5. User becomes family member with assigned role (ADMIN or MEMBER)

### GraphQL Schema (Simplified)

```graphql
# Mutations
type Mutation {
  # Email-only invitation
  inviteFamilyMemberByEmail(input: InviteFamilyMemberByEmailInput!): InviteFamilyMemberByEmailPayload!

  # Invitation management
  cancelInvitation(input: CancelInvitationInput!): CancelInvitationPayload!
  updateInvitationRole(input: UpdateInvitationRoleInput!): UpdateInvitationRolePayload!
  acceptInvitation(input: AcceptInvitationInput!): AcceptInvitationPayload!
}

# Queries
type Query {
  # Get pending invitations (Owner/Admin only)
  pendingInvitations(familyId: UUID!): [PendingInvitationType!]!

  # Get family members
  familyMembers(familyId: UUID!): [FamilyMemberType!]!
}

# Types
type PendingInvitationType {
  id: UUID!
  email: String!              # Required - email-only invitations
  role: UserRoleType!
  status: InvitationStatusType!
  invitedAt: DateTime!
  expiresAt: DateTime!
  displayCode: String         # For debugging/support
}

enum UserRoleType {
  OWNER
  ADMIN
  MEMBER
  # MANAGED_ACCOUNT removed
}
```

## Consequences

### Positive

1. **Faster Time to Market:**
   - 100+ fewer files to implement/test
   - Simpler authentication flow
   - Focus on core family management features

2. **Reduced Complexity:**
   - Single OAuth flow (no username → synthetic email mapping)
   - No dual authentication logic in frontend/backend
   - Simpler GraphQL schema

3. **Better Developer Experience:**
   - Standard OAuth 2.0 / OIDC patterns
   - No custom Zitadel Actions required
   - Easier to test and debug

4. **Clean Architecture:**
   - `User.CreateFromOAuth()` is the ONLY user creation method
   - Email is always present and required
   - No special cases for managed accounts

### Negative

1. **No Managed Accounts (Yet):**
   - Children without email addresses cannot be added as family members
   - Elderly family members without email cannot participate
   - **Mitigation:** Deferred to Phase 7+ based on user research

2. **Requires Email for All Users:**
   - Every family member must have an email address
   - Some users may need to create an email just for Family Hub
   - **Mitigation:** Most adults already have email addresses

3. **Future Migration Path:**
   - If we add managed accounts later, need migration strategy
   - May require new value objects (Username, PersonName)
   - **Mitigation:** Design decision can be revisited based on user feedback

### Technical Debt

**None.** This is a simplification, not a compromise. Email-only OAuth is the industry standard for modern web applications.

## Alternatives Considered

### A. Keep Dual Authentication (ADR-005)

**Rejected because:**

- Implementation complexity outweighs benefits for MVP
- No user validation that managed accounts are critical
- Zitadel API limitations make it error-prone

### B. Use ASP.NET Identity with Local Password Storage

**Rejected because:**

- Contradicts "External OAuth Only" decision (ADR-002)
- Increases security surface area (password storage, reset flows)
- Loses benefits of external identity provider (Zitadel features, MFA, audit logs)

### C. Wait for User Research Before Deciding

**Rejected because:**

- Would delay MVP launch significantly
- Can add managed accounts later if validated
- Email-only is simpler default choice

## Implementation

### Phases Completed

1. ✅ **Phase 1:** Backend domain & application cleanup (removed User.CreateManagedAccount(), removed Username/PersonName value objects)
2. ✅ **Phase 2:** Database migration (dropped 6 columns: username, name, zitadel_user_id, real_email, real_email_verified, username_login_enabled)
3. ✅ **Phase 3:** GraphQL API simplification (removed createManagedMember mutation, removed MANAGED_ACCOUNT role)
4. ✅ **Phase 4:** Frontend simplification (removed managed account tab, simplified invite modal to email-only)
5. ✅ **Phase 5:** Testing & validation (rewrote auth.service.spec.ts, validated build succeeds)
6. ✅ **Phase 6:** Documentation cleanup (this ADR, archive ADR-005, update CLAUDE.md)

### Database Schema Changes

```sql
-- Phase 2 Migration: DropManagedAccountColumns

-- Dropped columns from auth.users:
ALTER TABLE auth.users DROP COLUMN username;
ALTER TABLE auth.users DROP COLUMN name;
ALTER TABLE auth.users DROP COLUMN zitadel_user_id;
ALTER TABLE auth.users DROP COLUMN real_email;
ALTER TABLE auth.users DROP COLUMN real_email_verified;
ALTER TABLE auth.users DROP COLUMN username_login_enabled;

-- Dropped indexes:
DROP INDEX ix_users_username;
DROP INDEX ix_users_zitadel_user_id;
DROP INDEX ix_users_real_email;
```

### GraphQL Schema Changes

**Removed:**

- `createManagedMember` mutation
- `batchInviteFamilyMembers` mutation
- `MANAGED_ACCOUNT` from UserRoleType enum
- `username` and `personName` fields from PendingInvitationType
- `ManagedAccountCredentials` type
- `ManagedAccountResult` type

**Kept:**

- `inviteFamilyMemberByEmail` mutation (email-only)
- `pendingInvitations` query (email-only)
- `acceptInvitation` mutation
- Email-based invitation flow

## Monitoring & Success Criteria

### Metrics

1. **User Registration Rate:** Track how many users successfully complete OAuth registration
2. **Invitation Acceptance Rate:** Track how many email invitations are accepted
3. **Authentication Error Rate:** Monitor OAuth failures, token refresh errors

### Success Criteria

- ✅ Application builds successfully (TypeScript, C#)
- ✅ All tests pass (unit, integration, E2E)
- ✅ OAuth flow works end-to-end (registration, login, invitation acceptance)
- ✅ No managed account references in codebase
- ✅ Documentation updated (CLAUDE.md, this ADR)

## Future Considerations

### If Managed Accounts Are Needed (Phase 7+)

Based on user research, we may add managed accounts with:

1. **Username Value Object:** Validate username format (alphanumeric, underscores, hyphens)
2. **PersonName Value Object:** Full name for display purposes
3. **Zitadel User Management API:** Create users programmatically
4. **Migration Path:** Add real email to existing managed account, disable username login
5. **Dual Authentication:** Support both email OAuth and username login

**Decision Point:** Revisit after 3 months of user feedback and analytics (April 2026)

## References

- **Supersedes:** [ADR-005: Dual Authentication with Zitadel](archive/ADR-005-DUAL-AUTHENTICATION-ZITADEL.md)
- **Builds on:** [ADR-002: OAuth with Zitadel](ADR-002-OAUTH-WITH-ZITADEL.md)
- **Related:** [ADR-003: GraphQL Input/Command Pattern](ADR-003-GRAPHQL-INPUT-COMMAND-PATTERN.md)
- **Implementation Plan:** `/home/andrekirst/.claude/plans/harmonic-bouncing-donut.md` (6-phase reversal)
- **Epic:** #24 - Family Member Invitation System
- **Zitadel Docs:** https://zitadel.com/docs/apis/introduction
- **OAuth 2.0 RFC:** https://datatracker.ietf.org/doc/html/rfc6749

---

**Last Updated:** January 6, 2026
**Decision Makers:** Technical Lead (AI-assisted)
**Status:** Accepted and Implemented
