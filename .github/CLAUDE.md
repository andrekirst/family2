# GitHub Workflow Guide

**Purpose:** Guide for creating issues, pull requests, managing labels, and following contribution workflows in Family Hub.

**Key Resources:** Issue templates, PR template, CONTRIBUTING.md, 60+ labels

---

## Quick Reference

### Issue Types

Family Hub uses **6 issue templates**:

1. **Feature Request** - New features or enhancements
2. **Bug Report** - Bugs, defects, or errors
3. **Phase Deliverable (Epic)** - Major phase tracking
4. **Research & Documentation** - Research, spikes, ADRs
5. **Technical Debt** - Refactoring, code quality
6. **Blank** - Custom issues

**Location:** `.github/ISSUE_TEMPLATE/`

---

## Critical Patterns (3)

### 1. Issue Creation & Dependencies

**When to Create an Issue:**

- Proposing new features (check [FEATURE_BACKLOG.md](../docs/product-strategy/FEATURE_BACKLOG.md) first)
- Reporting bugs or defects
- Tracking phase deliverables or epics
- Documenting research or ADRs
- Addressing technical debt

**Before Creating:**

1. Search existing issues (avoid duplicates)
2. Check Feature Backlog - feature may be planned
3. Review Implementation Roadmap - understand phase
4. Read relevant docs in `/docs/`

**Issue Dependencies:**

Use dependency syntax to create automatic blocking relationships:

```markdown
## Description
Extract the Family domain from Auth module.

**Depends on:** #32

**Blocks:** #34, #35

## Tasks
- [ ] Create Family module structure
- [ ] Move domain entities
- [ ] Update tests
```

**Result:** GitHub automatically creates:

- Issue #32: 🔒 **Blocking** Issue #33
- Issue #33: ⛔ **Blocked by** Issue #32
- Issue #34: ⛔ **Blocked by** Issue #33

**Use Cases:**

- Sequential refactoring tasks
- Feature prerequisites (auth before user features)
- Infrastructure before application code
- Test setup before test implementation

---

### 2. Issue Lifecycle

All issues follow this lifecycle:

1. **Triage** (`status-triage`)
   - Newly created, needs review
   - Team reviews validity, priority, scope
   - Labels assigned

2. **Planning** (`status-planning`)
   - Approved, needs detailed planning
   - Technical design defined
   - Dependencies identified

3. **Ready** (`status-ready`)
   - Ready for development
   - All blockers resolved
   - Clear acceptance criteria

4. **In Progress** (`status-in-progress`)
   - Active development
   - PR linked when created

5. **Review**
   - PR submitted, awaiting approval
   - CI checks passing

6. **Done** (`status-done`)
   - PR merged to main
   - Acceptance criteria met

**Status Labels:** Update as issue progresses through lifecycle.

---

### 3. Pull Request Process

**Before Submitting PR:**

1. **Create or link issue** - All PRs reference an issue
2. **Branch naming:** `feature/123-brief-description` or `fix/123-brief-description`
3. **Run tests locally** - Ensure all tests pass
4. **Follow code standards** - Linting, formatting, DDD
5. **Update documentation** - Code comments, API docs, `/docs/`

**PR Template Checklist:**

Required sections:

- [ ] Summary (what does this PR do?)
- [ ] Related issues (Closes #X, Related to #Y)
- [ ] Type of change (bug fix, feature, breaking change)
- [ ] Services affected (Auth, Calendar, Task, etc.)
- [ ] Domain events (if applicable)
- [ ] Event chain impact
- [ ] Database changes (migrations)
- [ ] API changes (GraphQL schema)
- [ ] Tests (unit, integration, E2E)
- [ ] Documentation updates

**Architecture Impact:**

Every PR must document:

- **Services Affected** - Which modules/services changed
- **Domain Events** - Published or consumed events
- **Event Chain Impact** - Does this affect event chains?
- **Database Changes** - Schema changes, migrations
- **API Changes** - GraphQL schema updates

**Example PR Description:**

```markdown
## Summary
Implement Family module extraction from Auth module (Phase 1, Issue #33).

## Related Issues
Closes #33
Depends on #32

## Type of Change
- [x] Refactoring (no functional changes)
- [ ] Breaking change

## Architecture Impact

### Services Affected
- [x] Auth Service
- [x] Family Service (NEW)

### Domain Events
**Published:**
- `FamilyCreatedEvent` - Published when family created

**Consumed:**
- None

### Database Changes
- [x] Database schema changes (migration included)
- [x] New tables (families, family_invitations)

## Testing
- [x] Unit tests added (Family aggregate)
- [x] Integration tests added (FamilyRepository)
- [x] E2E tests passing

## Documentation
- [x] Code comments added
- [x] GraphQL schema documentation updated
- [x] /docs/ updated (MODULE_EXTRACTION_QUICKSTART.md)
```

**Code Review Guidelines:**

**As Reviewer:**

- Verify DDD boundaries respected
- Check event chain integration
- Ensure tests cover happy path, errors, edges
- Validate security and performance
- Confirm documentation complete

**As Author:**

- Respond to feedback promptly
- Keep PRs focused and small
- Explain architectural decisions
- Update based on review comments

---

## Label System (60+ Labels)

### Required Labels (Every Issue)

1. **Type** - `type-feature`, `type-bug`, `type-tech-debt`, etc.
2. **Phase** - `phase-0` through `phase-6`
3. **Service** - `service-auth`, `service-calendar`, etc.

### Optional Labels

**Priority:**

- `priority-p0` - Critical (must have)
- `priority-p1` - High (should have)
- `priority-p2` - Medium (nice to have)
- `priority-p3` - Low (future)

**Effort:**

- `effort-s` - Small (< 4 hours)
- `effort-m` - Medium (4-16 hours)
- `effort-l` - Large (2-5 days)
- `effort-xl` - Extra large (1+ weeks)

**Domain:**

- `domain-event-chain` - Event chain related
- `domain-security` - Security related
- `domain-performance` - Performance related
- `domain-accessibility` - A11y related

**Status:**

- `status-triage`, `status-planning`, `status-ready`, `status-in-progress`, `status-blocked`, `status-done`

**Special:**

- `good-first-issue` - Good for newcomers
- `help-wanted` - Help needed
- `breaking-change` - Breaking API change
- `needs-discussion` - Needs team discussion

**See:** [CONTRIBUTING.md](../CONTRIBUTING.md#labels-reference) for complete label list.

---

## Common GitHub Tasks

### Create Feature Request

1. Click "New Issue"
2. Select "Feature Request" template
3. Fill required fields:
   - Phase (Phase 0-6)
   - Service (bounded context)
   - User story
   - Acceptance criteria
   - RICE score (optional)
4. Add labels (type, phase, service)
5. Submit

### Report Bug

1. Click "New Issue"
2. Select "Bug Report" template
3. Fill required fields:
   - Severity (Critical, High, Medium, Low)
   - Service
   - Steps to reproduce
   - Expected vs actual behavior
   - Environment
4. Add labels
5. Submit

### Link PR to Issue

In PR description:

```markdown
Closes #123
Fixes #124
Resolves #125
```

GitHub automatically:

- Links PR to issue
- Closes issue when PR merges
- Shows PR status in issue

### Update Issue Status

As issue progresses:

1. Remove old status label
2. Add new status label
3. Comment with progress update

### Close Issue

**When to Close:**

- ✅ Acceptance criteria met
- ✅ PR merged
- ✅ Tests passing in production
- ✅ Documentation updated

**How:**

- Automatically (via PR "Closes #X")
- Manually (click "Close issue")

---

## Issue Best Practices

**DO:**

- ✅ Use appropriate template
- ✅ Clear, specific titles
- ✅ Include acceptance criteria
- ✅ Link related issues/PRs/docs
- ✅ Add relevant labels
- ✅ Specify dependencies (`**Depends on:**`)
- ✅ Keep scope focused

**DON'T:**

- ❌ Vague titles ("Improve calendar")
- ❌ Mix unrelated changes
- ❌ Skip template without reason
- ❌ Forget service/domain tags
- ❌ Leave dependencies implicit

---

## Project Board (GitHub Projects)

**Kanban Board Columns:**

1. **Backlog** - Triaged issues, not yet planned
2. **Planned** - Planned for current phase
3. **Ready** - Ready for development
4. **In Progress** - Active work
5. **Review** - PR in review
6. **Done** - Merged and closed

**Views:**

- By Phase (Phase 0-6)
- By Service (Auth, Calendar, Task, etc.)
- By Priority (P0-P3)
- By Assignee

---

## Related Documentation

- **Contributing Guide:** [CONTRIBUTING.md](../CONTRIBUTING.md) - Comprehensive contribution guide
- **Feature Backlog:** [docs/product-strategy/FEATURE_BACKLOG.md](../docs/product-strategy/FEATURE_BACKLOG.md) - All planned features
- **Roadmap:** [docs/product-strategy/implementation-roadmap.md](../docs/product-strategy/implementation-roadmap.md) - Phase timeline
- **Coding Standards:** [docs/development/CODING_STANDARDS.md](../docs/development/CODING_STANDARDS.md) - Code quality

---

**Last Updated:** 2026-01-09
**Derived from:** Root CLAUDE.md v5.0.0
**Canonical Sources:**

- CONTRIBUTING.md (Issue lifecycle, label system, best practices)
- .github/PULL_REQUEST_TEMPLATE.md (PR checklist)
- .github/ISSUE_TEMPLATE/ (Issue templates)

**Sync Checklist:**

- [ ] Issue lifecycle matches CONTRIBUTING.md
- [ ] Label categories match label list
- [ ] PR checklist matches template
- [ ] Dependency syntax accurate
