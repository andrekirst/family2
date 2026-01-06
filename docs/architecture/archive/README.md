# Architecture Decision Records - Archive

This directory contains ADRs that have been superseded or reversed due to changed requirements or strategic decisions.

## Archived ADRs

### ADR-005: Dual Authentication with Zitadel (REVERSED)

**File:** `ADR-005-DUAL-AUTHENTICATION-ZITADEL.md`

**Status:** Reversed and replaced by ADR-006

**Original Decision:** Implement dual authentication supporting both username-based login (for managed accounts) and email-based OAuth 2.0 login via Zitadel.

**Reversal Rationale:**

This ADR was reversed in January 2026 after implementation revealed that:

1. **No Real Benefit Over Email-Only:** Zitadel User Management API limitations made managed accounts more complex than email-only OAuth with no clear advantages for MVP
2. **Reduced Scope:** Strategic decision to simplify authentication to email-only for faster time to market
3. **Clean Slate:** No managed accounts existed in production, making reversal risk-free
4. **Deferred Feature:** Managed accounts for children/elderly deferred to Phase 7+ (post-MVP)

**Implementation Timeline:**
- **Proposed:** January 2026 (as part of Epic #24)
- **Implemented:** Partially (Phases 1-2 of 6-phase plan)
- **Reversed:** January 2026 (Phases 1-6 cleanup completed)
- **Replaced by:** ADR-006 Email-Only Authentication

**Impact:**
- 100+ files modified across backend and frontend
- 6 database columns removed from `auth.users` table
- GraphQL schema simplified (removed managed account mutations)
- Frontend simplified (removed dual-tab invitation UI)
- Tests simplified (removed username/managed account test cases)

**Lessons Learned:**
1. **Validate External Dependencies Early:** Zitadel API limitations should have been discovered during spike/proof-of-concept
2. **Simplify for MVP:** Starting with minimal authentication (email-only OAuth) is faster and simpler
3. **Incremental Complexity:** Add managed accounts later if user research validates the need

**Reference:**
- Superseded by: [ADR-006: Email-Only Authentication](../ADR-006-EMAIL-ONLY-AUTHENTICATION.md)
- Related Epic: Epic #24 - Family Member Invitation System
- Implementation Plan: `/home/andrekirst/.claude/plans/harmonic-bouncing-donut.md` (6-phase reversal)

---

## Archive Policy

ADRs are archived when:
1. The decision is reversed due to new information or changed requirements
2. The ADR is superseded by a newer ADR
3. The feature/capability is removed from the product

Archived ADRs are kept for historical context and to document lessons learned.
