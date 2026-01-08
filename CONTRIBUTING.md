# Contributing to Family Hub

Thank you for your interest in contributing to Family Hub! This guide will help you create well-structured issues and pull requests.

## Table of Contents

1. [Creating Issues](#creating-issues)
2. [Issue Templates](#issue-templates)
3. [Pull Request Process](#pull-request-process)
4. [Understanding Project Structure](#understanding-project-structure)
5. [Code Standards](#code-standards)
6. [Labels Reference](#labels-reference)

---

## Creating Issues

### When to Create an Issue

Create an issue when you want to:

- **Propose a new feature** that aligns with the product roadmap
- **Report a bug** or defect
- **Track a phase deliverable** or epic
- **Document research** or architectural decisions
- **Address technical debt** or refactoring needs

### Before Creating an Issue

1. **Search existing issues** to avoid duplicates
2. **Check the Feature Backlog** (`/docs/FEATURE_BACKLOG.md`) - your feature may already be planned
3. **Review the Implementation Roadmap** (`/docs/implementation-roadmap.md`) - understand which phase it fits
4. **Read relevant documentation** in `/docs/` folder

### Issue Dependencies & Blocking Relationships

When creating issues that depend on other issues, use dependency syntax in the issue description:

**Syntax:**

- `**Depends on:** #X` - GitHub automatically creates "blocked by" relationships
- `**Blocks:** #Y` - Explicitly mark what this issue blocks
- Multiple dependencies: `**Depends on:** #32, #33, #34`

**Benefits:**

- Visual dependency graph in GitHub UI sidebar (⛔ blocked by, 🔒 blocking)
- Prevents premature work on blocked issues
- Clear implementation order for issue chains
- Automatic relationship creation via GitHub API

**Example:**

```markdown
## Description
Extract the Family domain from Auth module to a dedicated module.

**Depends on:** #32

## Tasks
- [ ] Create Family module structure
- [ ] Move domain entities
- [ ] Update tests
```

**Result:** GitHub automatically creates the blocking relationship:

- Issue #32: 🔒 **Blocking** Issue #33
- Issue #33: ⛔ **Blocked by** Issue #32

**Use cases:**

- Sequential refactoring tasks (Issue #32-#37 Family domain extraction)
- Feature prerequisites (auth before user features)
- Infrastructure before application code
- Test setup before test implementation

### Issue Lifecycle

All issues follow this lifecycle:

1. **Triage** (`status-triage`) - Newly created, needs review
   - Team reviews for validity, priority, and scope
   - Labels assigned (type, phase, service, priority)

2. **Planning** (`status-planning`) - Approved, needs detailed planning
   - Technical design and approach defined
   - Dependencies identified
   - Subtasks created for epics

3. **Ready** (`status-ready`) - Ready for development
   - All blockers resolved
   - Clear acceptance criteria
   - Assigned to developer

4. **In Progress** (`status-in-progress`) - Active development
   - Work has started
   - PR linked when created

5. **Review** - PR submitted, awaiting approval
   - CI checks passing
   - Code review in progress

6. **Done** (`status-done`) - Merged and closed
   - PR merged to main
   - Acceptance criteria met

### Best Practices for Issue Descriptions

**DO:**

- ✅ Use the appropriate issue template
- ✅ Provide clear, specific titles (e.g., "Add medication reminder notifications" not "Fix stuff")
- ✅ Include acceptance criteria with checkboxes
- ✅ Link to related issues, PRs, or documentation
- ✅ Add relevant labels (type, phase, service, priority)
- ✅ Specify dependencies with `**Depends on:**` syntax
- ✅ Keep scope focused (break large issues into smaller ones)

**DON'T:**

- ❌ Create vague titles ("Improve calendar" - improve how?)
- ❌ Mix multiple unrelated changes in one issue
- ❌ Skip the issue template without good reason
- ❌ Forget to tag the appropriate service/domain
- ❌ Leave dependencies implicit (use `**Depends on:**`)

### Using Labels Effectively

**Required labels for every issue:**

1. **Type** - `type-feature`, `type-bug`, `type-tech-debt`, etc.
2. **Phase** - `phase-0` through `phase-6` (align with roadmap)
3. **Service** - `service-auth`, `service-calendar`, etc. (bounded context)

**Optional but recommended:**

4. **Priority** - `priority-p0` (critical) through `priority-p3` (low)
5. **Effort** - `effort-s`, `effort-m`, `effort-l`, `effort-xl`
6. **Domain** - `domain-event-chain`, `domain-security`, etc.

**See full label reference below.**

### Issue Assignment & Tracking

**Single developer project:**

- Assign issues to yourself when starting work
- Update status labels as you progress
- Link PRs to issues (use "Closes #X" in PR description)

**Multi-developer teams:**

- Coordinate on who takes what
- Use GitHub Projects board for tracking
- Comment on blockers promptly

### Issue Updates & Communication

**When to comment:**

- Progress updates on long-running issues
- Discovered blockers or changed scope
- Questions for maintainers or stakeholders
- Linking related work

**When to close:**

- ✅ Acceptance criteria met
- ✅ PR merged
- ✅ Tests passing in production
- ✅ Documentation updated

---

## Issue Templates

We provide 6 issue templates to help structure your contributions:

### 1. Feature Request

Use when proposing new features or enhancements.

**When to use:**

- Adding new functionality to the platform
- Enhancing existing features
- Implementing features from the backlog

**Required information:**

- Implementation phase (Phase 0-6)
- Microservice/bounded context
- User story (As a... I want... So that...)
- Acceptance criteria
- RICE scoring (optional but recommended)

**Example:**

```
Title: [Feature] Add medication reminder notifications
Phase: Phase 2
Service: Communication Service
User Story: As a parent, I want to receive notifications for child medications,
so that I never miss giving them their medicine.
```

### 2. Bug Report

Use when reporting bugs or defects.

**When to use:**

- Something is broken or not working as expected
- Error messages or crashes
- Performance issues

**Required information:**

- Severity (Critical, High, Medium, Low)
- Affected service/component
- Steps to reproduce
- Expected vs actual behavior
- Environment details

**Example:**

```
Title: [Bug] Calendar events not syncing in real-time
Severity: High
Service: Calendar Service
Steps: 1. User A creates event, 2. User B doesn't see it for 5+ minutes
```

### 3. Phase Deliverable (Epic)

Use for major phase deliverables that coordinate multiple sub-issues.

**When to use:**

- Tracking a full phase (e.g., Phase 1: Core MVP)
- Major feature area requiring multiple sub-issues
- Cross-service initiatives

**Required information:**

- Phase number
- Context and objectives
- Deliverables checklist
- Success criteria
- Sub-issues list

**Example:**

```
Title: [Phase 2] Health Integration & Event Chains
Deliverables:
- [ ] Health Service implementation
- [ ] Doctor appointment event chain
- [ ] Prescription event chain
```

### 4. Research & Documentation

Use for research tasks, spikes, or documentation work.

**When to use:**

- Technical investigations or proof-of-concepts
- Architectural decision records (ADRs)
- Documentation updates
- Competitive analysis or market research

**Required information:**

- Research question or goal
- Scope of investigation
- Expected deliverables
- Timeline

**Example:**

```
Title: [Research] Evaluate event bus alternatives (Redis vs RabbitMQ)
Goal: Determine best event bus for production workloads
Deliverables:
- [ ] Performance comparison
- [ ] Cost analysis
- [ ] Recommendation document
```

### 5. Technical Debt / Refactoring

Use for technical debt or refactoring work.

**When to use:**

- Code quality issues (duplication, complexity)
- Architecture improvements
- Missing tests or flaky tests
- Performance optimizations needed
- Security vulnerabilities

**Required information:**

- Severity (Critical, High, Medium, Low)
- Affected service/component
- Type of technical debt
- Impact of NOT addressing
- Proposed solution

**Example:**

```
Title: [Tech Debt] Refactor Calendar Service event handling
Severity: Medium
Type: Code Quality
Impact: Difficult to add new event types
Solution: Extract event handler pattern
```

### 6. Blank Issue

Use when none of the templates fit.

---

## Understanding Project Structure

### Feature Backlog

All planned features are documented in `/docs/FEATURE_BACKLOG.md` with:

- **208 total features** across 16 domains
- **RICE scoring** for prioritization
- **Phase assignments** (MVP, Phase 2, Phase 3+)

**Before proposing a feature:**

1. Check if it's already in the backlog
2. Review its RICE score and priority
3. Understand its phase assignment

### Implementation Roadmap

The roadmap (`/docs/implementation-roadmap.md`) defines **6 phases**:

- **Phase 0**: Foundation & Tooling (4 weeks)
- **Phase 1**: Core MVP - Auth + Calendar + Tasks (8 weeks)
- **Phase 2**: Health Integration & Event Chains (6 weeks)
- **Phase 3**: Meal Planning & Finance (8 weeks)
- **Phase 4**: Recurrence & Advanced Features (8 weeks)
- **Phase 5**: Microservices Extraction & Production Hardening (10 weeks)
- **Phase 6**: Mobile App & Extended Features (8+ weeks)

**When creating issues:**

- Align with current phase (check project status)
- Respect phase dependencies
- Consider cross-phase impacts

### Microservices Architecture

Family Hub uses **8 bounded contexts** (microservices):

1. **Auth Service** - Zitadel integration, family groups
2. **Calendar Service** - Events, appointments, recurrence
3. **Task Service** - To-dos, assignments, chores
4. **Shopping Service** - Lists, items, sharing
5. **Health Service** - Appointments, prescriptions
6. **Meal Planning Service** - Meal plans, recipes
7. **Finance Service** - Budgets, expenses
8. **Communication Service** - Notifications, messages

**Tag issues with the appropriate service label.**

### Event Chain Automation

**Event chains** are Family Hub's flagship differentiator - automated cross-domain workflows.

**Example:** Doctor Appointment Chain

```
User schedules appointment (Health Service)
  ↓
Calendar event created (Calendar Service)
  ↓
Preparation task created (Task Service)
  ↓
Prescription issued (Health Service)
  ↓
Medication → shopping list (Shopping Service)
```

**When creating features:**

- Consider event chain integration
- Document events published/consumed
- Test end-to-end workflows

---

## Pull Request Process

### Before Submitting a PR

1. **Create or link an issue** - All PRs should reference an issue
2. **Branch naming**: `feature/123-brief-description` or `fix/123-brief-description`
3. **Run tests locally** - Ensure all tests pass
4. **Follow code standards** - Linting, formatting, DDD principles
5. **Update documentation** - Code comments, API docs, `/docs/` if needed

### PR Checklist

Use the PR template (`.github/PULL_REQUEST_TEMPLATE.md`):

- [ ] PR title follows convention: `[Type] Brief description`
- [ ] Related issue(s) linked
- [ ] Type of change marked (bug fix, feature, breaking change, etc.)
- [ ] Services affected listed
- [ ] Domain events documented (if applicable)
- [ ] Event chain impact assessed
- [ ] Tests added/updated (unit, integration, E2E)
- [ ] Documentation updated
- [ ] CI checks passing
- [ ] Code reviewed

### Code Review Guidelines

**As a reviewer:**

- Verify DDD boundaries respected (services don't cross bounded contexts)
- Check event chain integration
- Ensure tests cover happy path, errors, and edge cases
- Validate security and performance
- Confirm documentation is complete

**As an author:**

- Respond to feedback promptly
- Keep PRs focused and small
- Explain architectural decisions
- Update based on review comments

---

## Code Standards

### .NET / C# (Backend)

- **Style**: Follow .NET coding conventions
- **Architecture**: DDD with aggregates, entities, value objects
- **Testing**: >80% code coverage for domain logic
- **Documentation**: XML comments for public APIs

### Angular / TypeScript (Frontend)

- **Style**: Follow Angular style guide
- **Components**: Standalone components (Angular v21)
- **Styling**: Tailwind CSS utility classes
- **Testing**: Jest unit tests, Cypress E2E tests
- **Accessibility**: WCAG 2.1 AA compliance

### GraphQL

- **Schema**: Document all queries, mutations, types
- **Naming**: Use PascalCase for types, camelCase for fields
- **Validation**: Input validation on all mutations

### Database

- **Migrations**: Always use migrations (Entity Framework Core)
- **Naming**: snake_case for tables and columns
- **Indexes**: Index foreign keys and frequently queried fields

---

## Labels Reference

### Type Labels

- `type-feature` - New feature
- `type-bug` - Bug or defect
- `type-epic` - Phase deliverable
- `type-research` - Research/documentation
- `type-tech-debt` - Technical debt

### Phase Labels

- `phase-0` through `phase-6` - Implementation phases
- `phase-7-future` - Future work

### Service Labels

- `service-auth`, `service-calendar`, `service-task`, etc.

### Priority Labels

- `priority-p0` - Critical (must have)
- `priority-p1` - High (should have)
- `priority-p2` - Medium (nice to have)
- `priority-p3` - Low (future)

### Status Labels

- `status-triage` - Needs review
- `status-planning` - Planning phase
- `status-ready` - Ready for development
- `status-in-progress` - In progress
- `status-blocked` - Blocked
- `status-done` - Completed

---

## Getting Help

- **Documentation**: Check `/docs/` folder (250,000+ words of planning docs)
- **Issues**: Search existing issues or create a question issue
- **GitHub Discussions**: Use GitHub Discussions for open-ended questions

---

## Project Philosophy

Family Hub is built with:

- **Privacy first** - GDPR compliance, no data selling
- **Quality over speed** - Build it right from the start
- **AI-assisted development** - Claude Code generates 60-80% of boilerplate
- **Incremental delivery** - Each phase delivers real value
- **DDD & event-driven** - Clean architecture, loose coupling

Thank you for contributing to Family Hub!
