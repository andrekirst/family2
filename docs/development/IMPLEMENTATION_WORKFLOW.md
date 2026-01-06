# Implementation Workflow for Claude Code

**Audience:** Claude Code AI assistant
**Purpose:** Standard workflow for implementing features to maximize code quality and minimize rework

---

## Problem Statement

**Current situation:** Generated code requires 40-60% modification before commit

**Root causes:**

1. Not following existing project patterns
2. Over-engineering solutions (YAGNI violations)
3. Implementing without understanding codebase context

**Goal:** Reduce code modification to 10-20% through mandatory pattern exploration

---

## Standard Implementation Workflow

### Step 1: Clarify Requirements

**ALWAYS use AskUserQuestion when:**

- Requirements are ambiguous
- Multiple implementation approaches exist
- Architectural decisions are needed
- Constraints or edge cases are unclear

**Questions to ask:**

- "What existing features are similar to this?"
- "Should I follow the [similar feature] pattern?"
- "Which module owns this feature?"
- "Any special constraints or considerations?"

**DON'T assume - ASK**

---

### Step 2: Explore Existing Patterns

**MANDATORY for:**

- Multi-file features (3+ files)
- New domain concepts
- Complex workflows
- Cross-module features

**Spawn feature-dev:code-explorer subagent:**

```
Task: "Explore codebase for [similar feature/pattern] implementations"

Instructions:
- Use Serena semantic tools to find similar features
- Identify value objects, commands, handlers used
- Find test patterns for similar features
- Return concrete code examples to follow

Output: Code examples with file paths
```

**Example:**

```
User: "Add family member invitation feature"

Claude: Spawning code-explorer to find similar patterns...

Code-explorer finds:
- CreateFamilyCommand pattern (Commands/CreateFamilyCommand.cs)
- EmailVerificationToken pattern (Domain/EmailVerificationToken.cs)
- FamilyCreated domain event (Events/FamilyCreatedDomainEvent.cs)
- CreateFamily E2E tests (e2e/family-creation.spec.ts)
```

---

### Step 3: Plan Implementation

**MANDATORY for:**

- Complex features (5+ files)
- Architectural changes
- Features touching multiple modules

**Spawn feature-dev:code-architect subagent:**

```
Task: "Design [feature] following patterns from code-explorer"

Input: Patterns and examples from code-explorer

Instructions:
- Design approach matching existing patterns
- Identify files to create/modify
- Plan domain model (aggregates, value objects, events)
- Consider DDD module boundaries
- Plan test structure

Output: Implementation plan with file structure
```

**Example:**

```
Code-architect plans:
1. Create InviteFamilyMemberCommand (following CreateFamilyCommand structure)
2. Create FamilyInvitation value object (following EmailVerificationToken pattern)
3. Add InviteMember method to Family aggregate
4. Create FamilyMemberInvitedDomainEvent
5. Communication module handles notification
6. Tests following CreateFamily test patterns
```

---

### Step 4: Implement Following Exact Patterns

**CRITICAL: Copy structure from similar features**

**Do:**

- Use same value object patterns
- Follow same command/handler structure
- Match test patterns exactly
- Reuse existing validation patterns
- Follow same error handling approach

**DON'T:**

- Create new patterns unless necessary
- Over-engineer (violate YAGNI)
- Add unnecessary abstractions
- Implement features not requested

**Example:**

```csharp
// GOOD - Follows CreateFamilyCommand pattern
public record InviteFamilyMemberCommand(
    FamilyId FamilyId,
    Email Email,
    FamilyRole Role
) : IRequest<InviteFamilyMemberPayload>;

// BAD - Over-engineered
public record InviteFamilyMemberCommand
{
    public FamilyId FamilyId { get; init; }
    public Email Email { get; init; }
    public FamilyRole Role { get; init; }
    public InvitationExpirationStrategy ExpirationStrategy { get; init; }  // NOT REQUESTED
    public NotificationPreferences NotificationPrefs { get; init; }       // OVER-ENGINEERING
}
```

---

### Step 5: Follow TDD Pattern

User prefers: **Plan → TDD (tests first) → Implement**

**Test structure:**

1. Write test outline with critical assertions
2. User writes detailed tests based on patterns
3. Implement feature to pass tests
4. Refine based on feedback

**Example test outline:**

```csharp
[Theory, AutoNSubstituteData]
public async Task InviteFamilyMember_Success(
    IFamilyRepository repository,
    IEmailService emailService,
    InviteFamilyMemberCommand command)
{
    // Arrange
    // TODO: Setup family exists
    // TODO: Configure email service

    // Act
    var handler = new InviteFamilyMemberCommandHandler(repository, emailService);
    var result = await handler.Handle(command, CancellationToken.None);

    // Assert
    result.Should().NotBeNull();
    result.Invitation.Should().NotBeNull();
    await emailService.Received(1).SendInvitationAsync(Arg.Any<Email>());
}

// TODO: Test validation errors
// TODO: Test duplicate invitation
// TODO: Test expired invitation
```

---

## Subagent Decision Tree

### Simple Features (1-2 files)

**Direct implementation** - No subagents needed

- Adding a property to existing entity
- Simple validation rule
- Minor UI tweak

### Moderate Features (3-5 files)

**Explore → Implement**

- Spawn code-explorer to find patterns
- Implement directly following patterns
- No planning subagent needed

### Complex Features (5+ files)

**Explore → Plan → Implement**

- Spawn code-explorer for patterns
- Spawn code-architect for design
- Implement following plan

### Architectural Changes

**Explore → Plan → Review → Implement**

- Spawn code-explorer for impact analysis
- Spawn code-architect for design
- User reviews plan before implementation
- Implement with careful validation

---

## Tool Usage Guidelines

### Use MORE

**Serena (semantic code operations):**

- find_symbol instead of file reads
- find_referencing_symbols for impact analysis
- replace_symbol_body for targeted edits
- search_for_pattern for cross-file patterns

**Context7 (third-party docs):**

- Query for up-to-date library documentation
- Use instead of relying on knowledge cutoff

**Sequential-thinking:**

- Complex planning and decision making
- Multi-step reasoning
- Trade-off analysis

**Task tool with subagents:**

- feature-dev:code-explorer - pattern discovery
- feature-dev:code-architect - design planning
- Explore - codebase navigation
- Plan - architecture design

### Use LESS

**Extensive code comments:**

- Generate minimal comments
- Code should be self-documenting
- Use clear names instead of comments

**Reading full documentation files:**

- Reference doc paths instead
- Only read when specific information needed

---

## Commit Message Format

```
<type>(<scope>): <summary> (#<issue>)

Co-Authored-By: Claude Sonnet 4.5 <noreply@anthropic.com>
```

**Types:** feat, fix, docs, style, refactor, test, chore

**Examples:**

```
feat(auth): add family invitation flow (#42)
fix(calendar): resolve timezone offset bug (#58)
test(family): add invitation validation tests (#61)
```

**Keep concise** - User is solo developer, no code reviews needed

---

## Educational Insights

**ALWAYS provide "Insight" boxes when:**

- Explaining architectural patterns discovered
- Clarifying DDD concepts
- Describing trade-offs in design decisions
- Teaching project-specific patterns

**Format:**

```
★ Insight ─────────────────────────────────────
[2-3 key educational points about patterns, architecture, or decisions]
─────────────────────────────────────────────────
```

**Example:**

```
★ Insight ─────────────────────────────────────
Family aggregate uses the Factory Method pattern for creation (Family.Create()) rather than exposing constructors. This ensures domain events are always raised and business rules are validated at creation time. The private parameterless constructor is only for EF Core materialization.
─────────────────────────────────────────────────
```

---

## Quality Checklist

Before completing implementation, verify:

- [ ] Followed existing patterns from code-explorer
- [ ] Used Vogen for all value objects
- [ ] Commands use value objects, Inputs use primitives
- [ ] Tests use FluentAssertions (not xUnit Assert)
- [ ] Tests use [Theory, AutoNSubstituteData] for dependencies
- [ ] Vogen value objects created manually in tests
- [ ] Domain events raised for significant changes
- [ ] Repository methods are async
- [ ] Error handling follows existing patterns
- [ ] No over-engineering (YAGNI principle)
- [ ] Minimal comments (self-documenting code)
- [ ] Commit message follows format

---

## Success Metrics

**Target:** 80-90% code correctness (vs current 40-60%)

**How to measure:**

- Reduced modification needed before commit
- Fewer pattern violations
- Less over-engineering
- Faster implementation cycles

**User feedback:**

- "This follows the pattern perfectly"
- "Just needed minor tweaks"
- "Ready to commit as-is"

---

**Last updated:** 2026-01-06
**Version:** 1.0.0
