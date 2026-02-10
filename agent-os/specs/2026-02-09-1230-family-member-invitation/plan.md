# Family Member Invitation via Email - Implementation Plan

**Created**: 2026-02-09
**Issue**: #115
**Branch**: `feature/115-invitation-email`
**Spec Folder**: `agent-os/specs/2026-02-09-1230-family-member-invitation/`

---

## Overview

**User Story**: As a family owner/admin, I can invite new members to my family via email. The invitee receives an email with a secure link, clicks it, logs in (or registers), and accepts/declines the invitation. This introduces a `FamilyMember` join entity with roles (Owner/Admin/Member) and a `FamilyInvitation` aggregate with full lifecycle management.

**Current State**:

- Family creation exists with Owner assignment via `User.FamilyId`
- No way to add other members to a family
- No email infrastructure
- No invitation workflow

**What We're Building**:

1. `FamilyMember` entity with roles (Owner, Admin, Member)
2. `FamilyInvitation` aggregate with lifecycle (Pending → Accepted/Declined/Revoked/Expired)
3. Email service infrastructure (MailKit + MailHog for dev)
4. GraphQL mutations/queries for invitation CRUD
5. Frontend: Family settings page, invitation management, acceptance page
6. Unit tests for domain logic and command handlers

---

## 13 Implementation Tasks

1. Save spec documentation
2. FamilyRole VO + FamilyMember entity
3. FamilyInvitation aggregate + domain events
4. Database migration + EF configurations
5. Repositories for FamilyMember + FamilyInvitation
6. Authorization service + update CreateFamilyCommandHandler
7. Email service infrastructure (MailKit, templates, docker)
8. Send/Accept/Decline/Revoke command handlers
9. GraphQL mutations + queries
10. Frontend invitation service + GraphQL operations
11. Frontend family settings page
12. Frontend invitation acceptance page
13. Unit tests

---

## Key Design Decisions

- **Token strategy**: 64-char crypto-random token, SHA256 hashed in DB, plaintext in email
- **FamilyMember**: Entity owned by Family aggregate (not a separate aggregate root)
- **FamilyInvitation**: Aggregate root with state machine (Pending → terminal states)
- **Invitation expiry**: 30 days
- **Authorization**: Only Owner/Admin can send invitations
- **Email**: MailKit for SMTP, MailHog for local development
