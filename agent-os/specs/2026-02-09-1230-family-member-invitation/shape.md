# Family Member Invitation - Shaping Decisions

## Scope

- Email-based invitation with secure token link
- Full invitation lifecycle: Send → Accept/Decline/Revoke
- FamilyMember join entity with roles replacing direct User.FamilyId for authorization
- Family settings page for managing members and invitations

## User Decisions

- **Invitation expiry**: 30 days (configurable later)
- **Roles**: Owner (creator), Admin (can invite), Member (basic access)
- **Authorization**: Only Owner and Admin can send/revoke invitations
- **Token**: 64-char crypto-random, SHA256 hashed in DB for security
- **Email**: Simple HTML+text templates with string interpolation (no templating engine)
- **Dev email**: MailHog for local development (captures all outgoing email)
- **Acceptance flow**: Public page shows invitation details, requires login to accept/decline
- **Post-login redirect**: Reuses existing `consumePostLoginRedirect()` pattern

## Out of Scope

- Member removal/banning
- Role changes after joining
- Invitation resend functionality
- Rate limiting on invitation sends
- Custom invitation messages
