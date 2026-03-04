# Relay Cursor Pagination and Scroll UX for Messaging -- Shaping Notes

**Feature**: Full-stack pagination hardening with Relay connections and Slack/Discord-like scroll UX
**Created**: 2026-03-04
**GitHub Issue**: #206

---

## Scope

Harden the existing message pagination from basic `before`/`limit` to production-grade Relay cursor connections with composite cursors, proper `hasNextPage` responses, and enhanced frontend scroll UX. Design to be multi-channel ready for issue #210.

### In Scope

- **Backend:** Hot Chocolate Relay connection types (`edges`/`nodes`/`pageInfo`), composite cursor (`SentAt` + `MessageId`), N+1 fetch pattern for `hasNextPage`, updated DB index
- **Frontend:** Apollo connection query integration, real `pageInfo`-driven pagination, scroll position restoration, jump-to-bottom FAB with unread badge, unread message separator
- **Tests:** Cursor encode/decode tests, paged handler tests, fake repository update
- **Migration:** Composite index `(FamilyId, SentAt DESC, Id DESC)`

### Out of Scope

- Multi-channel implementation (issue #210 -- but we design for it)
- Message search/jump-to-message
- Read receipts / typing indicators
- Message reactions

## Decisions

- **Relay connections over simple cursor:** Industry standard, gives us `pageInfo` for free, supports bidirectional paging when needed
- **Composite cursor (`SentAt` + `MessageId`):** Handles duplicate timestamps without data loss -- critical for high-volume families
- **N+1 fetch pattern:** Avoids expensive COUNT(*) queries on PostgreSQL; fetch `limit + 1`, trim if overflow
- **Base64 cursor encoding:** Opaque to client, encodes `"{SentAt.Ticks}|{MessageId}"` -- deterministic and debuggable
- **No virtual scrolling yet:** Angular CDK virtual scroll with variable-height messages is complex; scroll restoration + FAB covers 95% of UX needs for MVP volumes
- **Breaking schema change acceptable:** No external API consumers in MVP phase; frontend + backend deploy atomically

## Context

- **Visuals:** Slack/Discord-like UX -- jump-to-bottom FAB, unread separator, smooth scroll restoration
- **References:** Existing messaging module at `src/FamilyHub.Api/Features/Messaging/` and `src/frontend/family-hub-web/src/app/features/messaging/`
- **Product alignment:** N/A (no product folder)

## Standards Applied

- **backend/graphql-input-command** -- GraphQL resolver patterns with `[ExtendObjectType]` and service injection
- **backend/vogen-value-objects** -- `MessageId` Vogen VO used in composite cursor
- **database/ef-core-migrations** -- EF Core migration for updated index
- **frontend/angular-components** -- Standalone components with signals for scroll state
- **frontend/apollo-graphql** -- Apollo client with typed connection queries
- **testing/unit-testing** -- xUnit + FluentAssertions with fake repository pattern
