# Family Hub - Claude Code Guide

## 🚨 CRITICAL CONTEXT - Read First

**Family Hub** is a privacy-first, cloud-based family organization platform that reduces mental load through intelligent **event chain automation**.

**Primary Differentiator:** Event Chain Automation - Automatic cross-domain workflows that no competitor offers:
- Doctor appointment → calendar event → preparation task → prescription → shopping list → pickup task → refill reminder
- Meal plan → shopping list → budget tracking → recipe suggestions
- Task assignment → notifications → reminders → completion tracking

**⚠️ CRITICAL ARCHITECTURE DECISION:** Starting with Modular Monolith, not microservices from day one. See [ADR-001](docs/architecture/ADR-001-MODULAR-MONOLITH-FIRST.md) for rationale.

**Technology Stack:**

| Layer | Technology | Notes |
|-------|------------|-------|
| Backend | .NET Core 10 / C# 14 | Hot Chocolate GraphQL |
| Frontend | Angular v21 + TypeScript | Tailwind CSS |
| Database | PostgreSQL 16 | Row-Level Security (RLS) |
| Event Bus | RabbitMQ | In-process Phase 1-4, network Phase 5+ |
| Auth | Zitadel | External OAuth 2.0 / OIDC |
| Infrastructure | Docker Compose → Kubernetes | Phase 1-4 → Phase 5+ |
| Monitoring | Prometheus + Grafana + Seq | |
| Value Objects | Vogen 8.0+ | Source generator for strongly-typed primitives |

**📍 CURRENT PHASE: Phase 0 - Foundation & Tooling (IN PROGRESS)**
- **Phase 1 Preparation:** ✅ COMPLETED (December 2024 - Issues #4-11)
- **OAuth Integration:** ✅ COMPLETED (December 2024 - Zitadel OAuth 2.0, 7 days)
- **51 core documents** (280,000+ words) organized in 9 thematic folders
- **Technology stack confirmed:** .NET Core 10, Angular v21, GraphQL, RabbitMQ, Zitadel
- **Architecture validated:** Modular monolith first (ADR-001), OAuth with Zitadel (ADR-002)
- **Next:** Frontend OAuth integration, then Phase 1 Core MVP

**Strategic Pivot (December 2024):**
- ✅ Launch as cloud-based online service FIRST
- ⚠️ Self-hosting and federation DEFERRED to Phase 7+ (post-MVP)
- 🎯 Focus on event chain automation as PRIMARY differentiator
- ⏱️ Faster time to market: 12 months vs 15-18 months

**Development Approach:**
- **Single developer** + **Claude Code AI** (60-80% of boilerplate)
- Domain-Driven Design (DDD) with **8 bounded contexts (modules)**
- **Modular Monolith** (Phase 1-4) → Microservices (Phase 5+)
- Event-driven architecture with RabbitMQ
- Incremental delivery: **10-14 months** to MVP (reduced from 16-22)

---

## 💡 QUICK START BY TASK

### Implementing Features

1. **Check phase:** [implementation-roadmap.md](docs/product-strategy/implementation-roadmap.md) - Is this feature scheduled for current phase?
2. **Find feature:** [FEATURE_BACKLOG.md](docs/product-strategy/FEATURE_BACKLOG.md) - Priority, dependencies, RICE score
3. **Review wireframes:** [wireframes.md](docs/ux-design/wireframes.md) - UI layout and user flow
4. **Check design system:** [design-system.md](docs/ux-design/design-system.md) - Components and styling
5. **Identify module:** [domain-model-microservices-map.md](docs/architecture/domain-model-microservices-map.md) - Which of 8 modules?
6. **Review events:** Does this feature publish/consume domain events?
7. **Check event chains:** [event-chains-reference.md](docs/architecture/event-chains-reference.md) - Part of workflow?
8. **Verify accessibility:** [accessibility-strategy.md](docs/ux-design/accessibility-strategy.md) - WCAG 2.1 AA requirements
9. **Review risks:** [risk-register.md](docs/product-strategy/risk-register.md) - Mitigation strategies
10. **Implement:** Follow module patterns, test thoroughly

### Architecture Questions

**Read these 4 docs:**
1. [ADR-001](docs/architecture/ADR-001-MODULAR-MONOLITH-FIRST.md) - Modular monolith decision rationale
2. [ADR-002](docs/architecture/ADR-002-OAUTH-WITH-ZITADEL.md) - OAuth 2.0 with Zitadel (vs Auth0, Keycloak, ASP.NET Identity)
3. [Domain Model](docs/architecture/domain-model-microservices-map.md) - 8 DDD modules, domain events, GraphQL schemas
4. [Event Chains Reference](docs/architecture/event-chains-reference.md) - Event-driven patterns and workflows

### Planning & Roadmap Questions

**Read these 3 docs:**
1. [Implementation Roadmap](docs/product-strategy/implementation-roadmap.md) - 6-phase plan (Phase 0-6), deliverables, timeline
2. [Feature Backlog](docs/product-strategy/FEATURE_BACKLOG.md) - 208 features prioritized by RICE score
3. [Risk Register](docs/product-strategy/risk-register.md) - 35 risks with mitigation strategies

### Product Strategy Questions

**Read these 3 docs:**
1. [Product Strategy](docs/product-strategy/PRODUCT_STRATEGY.md) - Vision, personas, strategic pillars, positioning
2. [UX Research Report](docs/ux-design/ux-research-report.md) - 6 personas, user journeys, competitive analysis
3. [Executive Summary](docs/executive-summary.md) - 15-minute overview of vision, market, strategy

---

## 📚 DOCUMENTATION INDEX

**51 documents** (280,000+ words) organized by thematic folders. All paths relative to `/docs/`.

| Category | Folder | Key Documents | Purpose |
|----------|--------|---------------|---------|
| **Navigation** | `/docs/` (root) | [INDEX.md](docs/INDEX.md), [README.md](docs/README.md), [executive-summary.md](docs/executive-summary.md) | Quick start guide, complete index, 15-min overview |
| **Product Strategy** | `/product-strategy/` | [PRODUCT_STRATEGY.md](docs/product-strategy/PRODUCT_STRATEGY.md), [FEATURE_BACKLOG.md](docs/product-strategy/FEATURE_BACKLOG.md) (208 features), [implementation-roadmap.md](docs/product-strategy/implementation-roadmap.md), [ROADMAP_VISUAL.md](docs/product-strategy/ROADMAP_VISUAL.md), [risk-register.md](docs/product-strategy/risk-register.md) | Vision, personas, RICE-scored features, 6-phase plan, 35 risks |
| **Architecture** | `/architecture/` | [ADR-001-MODULAR-MONOLITH-FIRST.md](docs/architecture/ADR-001-MODULAR-MONOLITH-FIRST.md), [ADR-002-OAUTH-WITH-ZITADEL.md](docs/architecture/ADR-002-OAUTH-WITH-ZITADEL.md), [domain-model-microservices-map.md](docs/architecture/domain-model-microservices-map.md), [event-chains-reference.md](docs/architecture/event-chains-reference.md), [architecture-visual-summary.md](docs/architecture/architecture-visual-summary.md), [multi-tenancy-strategy.md](docs/architecture/multi-tenancy-strategy.md) | Architecture decisions, 8 DDD modules, event chains, system diagrams, RLS |
| **Infrastructure** | `/infrastructure/` | [cloud-architecture.md](docs/infrastructure/cloud-architecture.md), [kubernetes-deployment-guide.md](docs/infrastructure/kubernetes-deployment-guide.md), [helm-charts-structure.md](docs/infrastructure/helm-charts-structure.md), [cicd-pipeline.md](docs/infrastructure/cicd-pipeline.md), [observability-stack.md](docs/infrastructure/observability-stack.md), [infrastructure-cost-analysis.md](docs/infrastructure/infrastructure-cost-analysis.md) | Kubernetes, Helm charts, CI/CD, monitoring, costs ($200-5K/mo) |
| **Security** | `/security/` | [threat-model.md](docs/security/threat-model.md), [security-testing-plan.md](docs/security/security-testing-plan.md), [vulnerability-management.md](docs/security/vulnerability-management.md), [security-monitoring-incident-response.md](docs/security/security-monitoring-incident-response.md) | STRIDE (53 threats), OWASP Top 10, SAST/DAST, incident response |
| **Legal** | `/legal/` | [LEGAL-COMPLIANCE-SUMMARY.md](docs/legal/LEGAL-COMPLIANCE-SUMMARY.md), [privacy-policy.md](docs/legal/privacy-policy.md), [terms-of-service.md](docs/legal/terms-of-service.md), [compliance-checklist.md](docs/legal/compliance-checklist.md), [quick-reference-coppa-workflow.md](docs/legal/quick-reference-coppa-workflow.md) | GDPR/COPPA/CCPA compliance, 93-item checklist, policies, DPA templates |
| **UX & Design** | `/ux-design/` | [ux-research-report.md](docs/ux-design/ux-research-report.md), [wireframes.md](docs/ux-design/wireframes.md), [design-system.md](docs/ux-design/design-system.md), [angular-component-specs.md](docs/ux-design/angular-component-specs.md), [accessibility-strategy.md](docs/ux-design/accessibility-strategy.md), [event-chain-ux.md](docs/ux-design/event-chain-ux.md), [responsive-design-guide.md](docs/ux-design/responsive-design-guide.md), [interaction-design-guide.md](docs/ux-design/interaction-design-guide.md) | 6 personas, design system (22+ components), wireframes, WCAG 2.1 AA, event chain UX |
| **Market & Business** | `/market-business/` | [market-research-report.md](docs/market-business/market-research-report.md), [competitive-analysis.md](docs/market-business/competitive-analysis.md), [go-to-market-plan.md](docs/market-business/go-to-market-plan.md), [brand-positioning.md](docs/market-business/brand-positioning.md), [seo-content-strategy.md](docs/market-business/seo-content-strategy.md) | Competitive analysis (2,700+ reviews), GTM plan, brand, SEO/content |
| **Authentication** | `/authentication/` | [OAUTH_INTEGRATION_GUIDE.md](docs/authentication/OAUTH_INTEGRATION_GUIDE.md), [ZITADEL-SETUP-GUIDE.md](docs/authentication/ZITADEL-SETUP-GUIDE.md), [ZITADEL-OAUTH-COMPLETION-SUMMARY.md](docs/authentication/ZITADEL-OAUTH-COMPLETION-SUMMARY.md) | OAuth 2.0 with Zitadel, PKCE flow, setup guide, security audit (80% OWASP compliance) |

**Quick Access:**
- **Start here:** [Executive Summary](docs/executive-summary.md) - 15-minute overview
- **For developers:** [ADR-001](docs/architecture/ADR-001-MODULAR-MONOLITH-FIRST.md) + [Domain Model](docs/architecture/domain-model-microservices-map.md)
- **For product:** [Product Strategy](docs/product-strategy/PRODUCT_STRATEGY.md) + [Feature Backlog](docs/product-strategy/FEATURE_BACKLOG.md)
- **For design:** [UX Research](docs/ux-design/ux-research-report.md) + [Wireframes](docs/ux-design/wireframes.md) + [Design System](docs/ux-design/design-system.md)
- **For authentication:** [OAuth Integration Guide](docs/authentication/OAUTH_INTEGRATION_GUIDE.md) + [Zitadel Setup](docs/authentication/ZITADEL-SETUP-GUIDE.md)

**GitHub Issues (All Completed):**
- [Issue #11: Architecture Review](https://github.com/andrekirst/family2/issues/11) - ✅ Modular monolith decision
- [Issue #10: Legal Compliance](https://github.com/andrekirst/family2/issues/10) - ✅ GDPR/COPPA/CCPA
- [Issue #9: Market Strategy](https://github.com/andrekirst/family2/issues/9) - ✅ GTM planning
- [Issue #8: Security Architecture](https://github.com/andrekirst/family2/issues/8) - ✅ Threat modeling
- [Issue #7: UX Architecture](https://github.com/andrekirst/family2/issues/7) - ✅ UI design system
- [Issue #6: Cloud Architecture](https://github.com/andrekirst/family2/issues/6) - ✅ Kubernetes strategy
- [Issue #5: Product Strategy](https://github.com/andrekirst/family2/issues/5) - ✅ Feature prioritization
- [Issue #4: Master Plan](https://github.com/andrekirst/family2/issues/4) - ✅ Phase 1 prep complete

---

## 🏗️ 8 DDD MODULES OVERVIEW

**⚠️ Architecture:** Modular Monolith (Phase 1-4) → Microservices (Phase 5+). See [ADR-001](docs/architecture/ADR-001-MODULAR-MONOLITH-FIRST.md).

**Each module owns:**
- Domain entities and aggregates
- Domain events (published/consumed via RabbitMQ)
- GraphQL schema types (merged into single `/graphql` endpoint)
- Separate PostgreSQL schema (same DB instance, Row-Level Security)

**8 Modules:**

1. **Auth Module** - Zitadel integration, family groups, role-based permissions, OAuth 2.0/OIDC
2. **Calendar Module** - Events, appointments, recurrence patterns, reminders, timezone handling
3. **Task Module** - To-dos, assignments, chores, gamification (points/badges), recurring tasks
4. **Shopping Module** - Shopping lists, items, categories, sharing, collaborative editing
5. **Health Module** - Doctor appointments, prescriptions, medications, health tracking
6. **Meal Planning Module** - Meal plans, recipes, ingredients, nutrition info, dietary restrictions
7. **Finance Module** - Budgets, expense tracking, income, financial goals, reporting
8. **Communication Module** - Notifications (email/push), in-app messaging, activity feed

**Phase 5+ Migration:** Extract modules to independent microservices using Strangler Fig pattern.

**Future (Phase 7+):** 9. **Federation Service** - Self-hosting, ActivityPub, instance federation (DEFERRED).

**Full spec:** See [domain-model-microservices-map.md](docs/architecture/domain-model-microservices-map.md) for domain entities, events, GraphQL schemas.

---

## ⚡ EVENT CHAIN AUTOMATION (Flagship Feature)

**What it is:** Intelligent cross-domain workflows that automatically trigger related actions across different services. No competitor offers this.

**Example: Doctor Appointment Event Chain**

```
User schedules doctor appointment (Health Module)
  ↓ (automatic)
Calendar event created (Calendar Module)
  ↓ (automatic)
Preparation task created (Task Module)
  ↓ (automatic)
Prescription issued after appointment (Health Module)
  ↓ (automatic)
Medication added to shopping list (Shopping Module)
  ↓ (automatic)
Pickup task created (Task Module)
  ↓ (automatic)
Refill reminder scheduled (Communication Module)
```

**Why it matters:**
- Saves 10-30 minutes per workflow
- Eliminates 3-5 things to remember
- Reduces cognitive load (40-60% stress reduction)
- Coordinates across 4-5 modules automatically

**Technical implementation:**
- Event-driven architecture using RabbitMQ
- In-process execution (Phase 1-4) for simplicity
- Network-based messaging (Phase 5+) for microservices
- Saga pattern for complex multi-step workflows

**10 event chains specified:**
1. Doctor Appointment → Calendar → Tasks → Shopping
2. Prescription → Shopping List → Pickup Task → Refill Reminder
3. Meal Plan → Shopping List → Budget Update → Recipe Suggestions
4. Budget Threshold → Alert → Spending Review Task
5. Recurring Task → Auto-creation → Assignment → Reminder
6. Shopping List Complete → Archive → Budget Update → Analytics
7. Health Metric Alert → Doctor Appointment Suggestion → Calendar
8. Family Member Birthday → Gift Ideas → Shopping List → Budget
9. School Event → Calendar → Permission Slip Task → Reminder
10. Bill Due → Payment Reminder → Budget Check → Confirmation

**Full specifications:** See [event-chains-reference.md](docs/architecture/event-chains-reference.md) for implementation patterns, monitoring, testing strategies.

---

## 📍 CURRENT PHASE DETAILS

### Strategic Pivot (December 2024)

**Decision:** Launch as cloud-based **online service first**, defer self-hosting and federation to Phase 7+ (post-MVP).

**Rationale:**
- ✅ Faster time to market: 12 months vs 15-18 months
- ✅ Simpler infrastructure and operations (Docker Compose vs Kubernetes from day one)
- ✅ Focus on event chain automation (PRIMARY differentiator)
- ✅ Validate product-market fit before adding complexity
- ✅ Federation still planned (just later)

### Phase 0: Foundation & Tooling - READY TO START

**Duration:** 3 weeks (reduced from 4 weeks)
**Status:** ✅ Phase 1 Preparation COMPLETED (December 2024)

**Phase 1 Preparation Achievements:**
- ✅ **Issue #4:** Master implementation plan & agent coordination
- ✅ **Issue #5:** Product strategy (208 features, 6 personas)
- ✅ **Issue #6:** Cloud architecture & Kubernetes deployment
- ✅ **Issue #7:** UX research (2,700+ reviews) & UI design system (22+ components)
- ✅ **Issue #8:** Security architecture (53 threats analyzed, STRIDE)
- ✅ **Issue #9:** Market strategy & go-to-market planning
- ✅ **Issue #10:** Legal compliance (GDPR, COPPA, CCPA)
- ✅ **Issue #11:** Architecture review → **MODULAR MONOLITH DECISION**

**Key Metrics:**
- 51 core documentation files (280,000+ words) organized in 9 thematic folders
- Technology stack confirmed (.NET Core 10, Angular v21, GraphQL, RabbitMQ, Zitadel)
- Risk reduction: Developer Burnout CRITICAL → MEDIUM
- Timeline optimization: -6 to -12 months to MVP
- Cost optimization: -$1,500 to -$2,000 Year 1

**Phase 0 Next Steps (3 weeks):**
1. Set up dev environment (.NET Core 10 SDK, Node.js, Docker Desktop)
2. Configure CI/CD pipeline (GitHub Actions)
3. Create modular monolith project structure (.NET Core 10 solution)
4. Initialize Git repository structure
5. Set up Zitadel instance (OAuth 2.0 provider)
6. Configure RabbitMQ (in-process execution framework)
7. Set up Hot Chocolate GraphQL (schema merging across modules)
8. Implement PostgreSQL RLS testing framework
9. Create Docker Compose for local dev environment

**Phase 1: Core MVP (6 weeks, reduced from 8 weeks):**
- Auth Module with Zitadel integration + GraphQL schema
- Calendar Module with events + GraphQL schema
- Task Module with assignments + GraphQL schema
- Basic event chains (in-process via RabbitMQ)

**Success Criteria (by end of Phase 6):**
- **User Metrics:** 100 active families, 80%+ Day 30 retention, 50%+ using event chains weekly, NPS > 40
- **Business Metrics:** 25+ premium subscribers, $2,500+ MRR, positive unit economics
- **Technical Metrics:** 99.5%+ uptime, <2s event chain latency, <3s p95 API response time

**Full roadmap:** [implementation-roadmap.md](docs/product-strategy/implementation-roadmap.md)

---

## 🛠️ DEVELOPMENT WORKFLOWS

### For New Claude Code Sessions

1. **Read CLAUDE.md** (this file) - Critical context in first 80 lines
2. **Check current phase** - [implementation-roadmap.md](docs/product-strategy/implementation-roadmap.md)
3. **Review architecture decision** - [ADR-001](docs/architecture/ADR-001-MODULAR-MONOLITH-FIRST.md)
4. **Understand domain model** - [domain-model-microservices-map.md](docs/architecture/domain-model-microservices-map.md)

### Database Migrations with EF Core

**CRITICAL: Use EF Core Code-First migrations for ALL schema changes** - Never write custom SQL migration scripts.

**What are EF Core Migrations?**
Entity Framework Core Code-First migrations provide type-safe, version-controlled database schema management. Changes to entity classes automatically generate migration files in C#, which are compiled, type-checked, and tracked in git.

**Why EF Core Migrations (not SQL scripts)?**
1. **Type Safety** - C# migrations are compiled and type-checked at build time
2. **Consistent Naming** - Follows .NET conventions (snake_case for PostgreSQL)
3. **Database Agnostic** - Same code works with PostgreSQL, SQL Server, SQLite
4. **Automatic Tracking** - EF Core manages `__EFMigrationsHistory` table
5. **Vogen Integration** - Value converters work seamlessly
6. **CI/CD Ready** - Programmatic migration execution
7. **Rollback Support** - Generated `Down()` methods for reverting

**Architecture Pattern:**
- **One DbContext per module** (AuthDbContext, CalendarDbContext, etc.)
- **Each DbContext targets its own PostgreSQL schema** (auth, calendar, etc.)
- **Fluent API configurations** in `IEntityTypeConfiguration<T>` classes
- **PostgreSQL-specific features** (RLS, triggers) via `migrationBuilder.Sql()`

**Creating a Migration:**

```bash
# Navigate to module
cd src/api/Modules/FamilyHub.Modules.Auth

# Create migration
dotnet ef migrations add InitialCreate \
    --context AuthDbContext \
    --project . \
    --startup-project ../../FamilyHub.Api \
    --output-dir Persistence/Migrations
```

**Applying Migrations:**

```bash
# Development
dotnet ef database update --context AuthDbContext \
    --project Modules/FamilyHub.Modules.Auth \
    --startup-project FamilyHub.Api

# Production (programmatic in Program.cs)
using var scope = app.Services.CreateScope();
var authDbContext = scope.ServiceProvider.GetRequiredService<AuthDbContext>();
await authDbContext.Database.MigrateAsync();
```

**Adding PostgreSQL Features (RLS, Triggers):**

```csharp
public partial class InitialCreate : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // Auto-generated table creation from Fluent API
        migrationBuilder.CreateTable(...);

        // Manual SQL for PostgreSQL-specific features
        migrationBuilder.Sql(@"
            ALTER TABLE auth.users ENABLE ROW LEVEL SECURITY;
            CREATE POLICY users_isolation_policy ON auth.users
                USING (id = auth.current_user_id());
        ");
    }
}
```

**Entity Configuration with Vogen:**

```csharp
public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("users", "auth");

        // Vogen value object with EF Core converter
        builder.Property(u => u.Id)
            .HasConversion(new UserId.EfCoreValueConverter())
            .HasColumnName("id");

        builder.Property(u => u.Email)
            .HasConversion(new Email.EfCoreValueConverter())
            .HasColumnName("email")
            .HasMaxLength(320);
    }
}
```

**Complete guide:** See `/database/docs/MIGRATION_STRATEGY.md`

**SQL scripts reference:** Original SQL design scripts are preserved in `/database/docs/reference/sql-design/` as reference documentation for RLS policies, triggers, and constraints. These are NOT executed - they inform EF Core migration implementation.

### Value Objects with Vogen

**CRITICAL: Use Vogen for ALL value objects** - Never create manual value object base classes.

**What is Vogen?**
[Vogen](https://github.com/SteveDunn/Vogen) is a .NET source generator that transforms primitives into strongly-typed value objects, enforcing domain concepts and preventing invalid states through compile-time errors. It eliminates boilerplate by auto-generating:
- Constructors and factory methods (`From`, `TryFrom`)
- Equality and comparison operators
- Validation logic
- EF Core value converters
- JSON serialization converters

**Creating Value Objects:**

```csharp
using Vogen;

// Simple strongly-typed ID
[ValueObject<Guid>(conversions: Conversions.Default | Conversions.EfCoreValueConverter)]
public readonly partial struct UserId
{
    private static Validation Validate(Guid value)
    {
        if (value == Guid.Empty)
            return Validation.Invalid("UserId cannot be empty.");
        return Validation.Ok;
    }

    public static UserId New() => From(Guid.NewGuid());
}

// Value object with validation and normalization
[ValueObject<string>(conversions: Conversions.Default | Conversions.EfCoreValueConverter)]
public readonly partial struct Email
{
    private static Validation Validate(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return Validation.Invalid("Email cannot be empty.");
        if (!Regex.IsMatch(value, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
            return Validation.Invalid("Email format is invalid.");
        return Validation.Ok;
    }

    private static string NormalizeInput(string input) =>
        input.Trim().ToLowerInvariant();
}
```

**Using Value Objects:**

```csharp
// Creating instances
var userId = UserId.New();  // Generate new ID
var email = Email.From("user@example.com");  // Direct creation

// Safe creation
if (Email.TryFrom("user@example.com", out var validEmail))
{
    // Use validEmail
}

// Access underlying value
string emailString = email.Value;
```

**Entity Framework Core Integration:**

Vogen automatically generates EF Core value converters. Configure in DbContext:

```csharp
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    modelBuilder.Entity<User>()
        .Property(u => u.Id)
        .HasConversion(new UserId.EfCoreValueConverter());

    modelBuilder.Entity<User>()
        .Property(u => u.Email)
        .HasConversion(new Email.EfCoreValueConverter())
        .HasMaxLength(320);
}
```

**Example Value Objects:**
- `/src/api/FamilyHub.SharedKernel/Domain/ValueObjects/Email.cs` - Email with validation
- `/src/api/FamilyHub.SharedKernel/Domain/ValueObjects/UserId.cs` - Strongly-typed user ID
- `/src/api/FamilyHub.SharedKernel/Domain/ValueObjects/FamilyId.cs` - Strongly-typed family ID
- See `/src/api/FamilyHub.SharedKernel/Domain/ValueObjects/README.md` for comprehensive guide

**Benefits:**
1. **Type Safety**: Prevents accidental assignment (`UserId` ≠ `FamilyId`)
2. **Domain Validation**: Ensures only valid values exist
3. **Self-Documenting**: Makes intent clear (`UserId` vs `Guid`)
4. **Zero Boilerplate**: No manual equality, validation, or converter code
5. **EF Core Seamless**: Automatic database mapping

**Best Practices:**
- Always use `readonly partial struct` (better performance than classes)
- Include `Conversions.EfCoreValueConverter` when using with EF Core
- Implement `Validate` method for domain-critical value objects
- Use `NormalizeInput` for consistent data format (e.g., email lowercasing)
- Provide factory methods (like `New()`) for ID generation

### Contributing & Creating Issues

**Issue Templates** (`.github/ISSUE_TEMPLATE/`):
- Feature Request (RICE scoring, user stories)
- Bug Report (severity, reproduction steps)
- Phase Deliverable (epic coordination)
- Research & Documentation
- Technical Debt
- Blank (custom)

**Pull Requests:** Use `.github/PULL_REQUEST_TEMPLATE.md` with architecture impact, testing, quality checklists.

**Labels System (60+ labels):**

| Category | Labels |
|----------|--------|
| Type | feature, bug, epic, research, docs, tech-debt, infrastructure, security, performance |
| Phase | phase-0 through phase-6, phase-7-future |
| Service | auth, calendar, task, shopping, health, meal, finance, communication, frontend, infrastructure, multiple |
| Status | triage, planning, ready, in-progress, blocked, review, testing, done, wontfix |
| Priority | p0 (critical), p1 (high), p2 (medium), p3 (low) |
| Domain | auth, calendar, tasks, shopping, health, meals, finance, notifications, event-chains, mobile, ux |
| Effort | xs (<1 day), s (1-3 days), m (1 week), l (2 weeks), xl (>2 weeks) |
| Special | good-first-issue, help-wanted, breaking-change, needs-documentation, needs-design, ai-assisted |

**Create labels:** `./scripts/create-labels.sh`

**Full guide:** [CONTRIBUTING.md](CONTRIBUTING.md)

**Why this matters:**
- Structured issues aligned with DDD architecture
- RICE scoring maintains prioritization methodology
- Phase alignment ensures roadmap adherence
- Event chain awareness prompts cross-module impact consideration
- AI-friendly templates (Claude Code generates 60-80% of code)

---

## ⚠️ STRATEGIC CONTEXT & CONSTRAINTS

### Strategic Decisions

1. **Online Service First** - Launch as cloud-based SaaS, NOT self-hosted initially. Self-hosting/federation deferred to Phase 7+ (post-MVP). Focus on event chain automation as PRIMARY differentiator.

2. **Event Chains are #1 Priority** - This is what makes Family Hub unique. No competitor offers automated cross-domain workflows. Must work flawlessly. Saves users 10-30 minutes per workflow.

3. **Single Developer + AI** - Project designed for AI-assisted solo development. Claude Code generates 60-80% of boilerplate, tests, schemas. Quality over speed. Incremental delivery.

4. **Privacy-First but Pragmatic** - GDPR compliant, no data selling, transparent security. Cloud-hosted for ease of use initially. Self-hosting for privacy advocates comes later (Phase 7+).

### What NOT to Do

❌ **Don't implement Federation Service** (Phase 7+, deferred - not in scope until post-MVP)
❌ **Don't skip phases** in implementation roadmap (follow sequential delivery)
❌ **Don't ignore event chains** when designing features (core innovation)
❌ **Don't assume features** - check `FEATURE_BACKLOG.md` for priorities (208 features RICE-scored)
❌ **Don't break DDD boundaries** - respect module ownership (8 modules, clear responsibilities)
❌ **Don't duplicate documentation** - reference `/docs/` instead of repeating content

### Tips for Claude Code

**When implementing features:**
1. Check [implementation-roadmap.md](docs/product-strategy/implementation-roadmap.md) for phase scheduling
2. Review [FEATURE_BACKLOG.md](docs/product-strategy/FEATURE_BACKLOG.md) for priority and RICE score
3. Consult [domain-model-microservices-map.md](docs/architecture/domain-model-microservices-map.md) for module ownership

**When asked about architecture:**
1. Start with [ADR-001](docs/architecture/ADR-001-MODULAR-MONOLITH-FIRST.md) for strategic decision
2. Review [domain-model-microservices-map.md](docs/architecture/domain-model-microservices-map.md) for DDD patterns
3. Check [event-chains-reference.md](docs/architecture/event-chains-reference.md) for event-driven patterns

**When planning work:**
- Follow phases in [implementation-roadmap.md](docs/product-strategy/implementation-roadmap.md) - don't skip ahead
- Each phase has clear deliverables and success criteria
- Validate assumptions before implementing
- Keep Federation deferred - online service first

---

## 📋 APPENDIX: COMPLETE DOCUMENTATION MAP

**All 51 markdown documents** organized by thematic folders:

### `/docs/` Root (3 documents)

Navigation and quick access:
- **executive-summary.md** - 15-minute overview of vision, market, strategy
- **INDEX.md** - Complete documentation map with FAQ
- **README.md** - Docs directory overview

### `/docs/product-strategy/` (5 documents)

Product vision, features, and roadmap:
- **PRODUCT_STRATEGY.md** - Complete strategy, personas, strategic pillars
- **FEATURE_BACKLOG.md** - 208 features (RICE scored with priorities)
- **implementation-roadmap.md** - 6-phase plan (10-14 months timeline)
- **ROADMAP_VISUAL.md** - Visual timeline with ASCII Gantt charts
- **risk-register.md** - 35 risks with mitigation strategies

### `/docs/architecture/` (6 documents)

Technical architecture and design decisions:
- **ADR-001-MODULAR-MONOLITH-FIRST.md** - Modular monolith decision rationale
- **ADR-002-OAUTH-WITH-ZITADEL.md** - OAuth 2.0 with Zitadel decision
- **ARCHITECTURE-REVIEW-REPORT.md** - Comprehensive architecture review
- **domain-model-microservices-map.md** - 8 DDD modules, domain events, GraphQL
- **event-chains-reference.md** - 10 workflow specifications
- **architecture-visual-summary.md** - System diagrams (ASCII)
- **multi-tenancy-strategy.md** - PostgreSQL Row-Level Security

### `/docs/infrastructure/` (6 documents)

Cloud, Kubernetes, CI/CD, and observability:
- **cloud-architecture.md** - Kubernetes architecture (Phase 5+)
- **kubernetes-deployment-guide.md** - Deployment guide (local/cloud)
- **helm-charts-structure.md** - Helm chart templates
- **cicd-pipeline.md** - GitHub Actions + ArgoCD
- **observability-stack.md** - Prometheus + Grafana + Loki
- **infrastructure-cost-analysis.md** - Cost projections ($200-5K/mo)

### `/docs/security/` (4 documents)

Security architecture and testing:
- **threat-model.md** - STRIDE analysis (53 threats)
- **security-testing-plan.md** - OWASP Top 10, SAST/DAST
- **vulnerability-management.md** - Severity levels, remediation SLAs
- **security-monitoring-incident-response.md** - Monitoring, incident playbooks

### `/docs/legal/` (8 documents)

Legal compliance and policies:
- **LEGAL-COMPLIANCE-SUMMARY.md** - Comprehensive compliance overview
- **privacy-policy.md** - Privacy Policy (GDPR/COPPA/CCPA)
- **terms-of-service.md** - Terms of Service
- **cookie-policy.md** - Cookie disclosure
- **compliance-checklist.md** - 93-item compliance checklist
- **data-processing-agreement-template.md** - DPA templates (B2B)
- **quick-reference-coppa-workflow.md** - COPPA implementation workflow
- **README.md** - Legal docs quick start guide

### `/docs/ux-design/` (9 documents)

UX research, design system, and UI specifications:
- **ux-research-report.md** - 6 personas, user journeys (2,700+ reviews)
- **wireframes.md** - Complete MVP wireframes (all screens)
- **design-system.md** - Design system (22+ components)
- **angular-component-specs.md** - Angular v21 component specs
- **information-architecture.md** - Site map, navigation structure
- **accessibility-strategy.md** - WCAG 2.1 AA + COPPA compliance
- **event-chain-ux.md** - Event chain UX patterns
- **responsive-design-guide.md** - Mobile-first responsive design
- **interaction-design-guide.md** - Micro-interactions, animations

### `/docs/market-business/` (5 documents)

Market research, competitive analysis, and GTM:
- **market-research-report.md** - Competitive analysis (2,700+ app reviews)
- **competitive-analysis.md** - Competitor SWOT analysis
- **go-to-market-plan.md** - GTM strategy (channels, pricing, launch)
- **brand-positioning.md** - Brand guidelines, messaging
- **seo-content-strategy.md** - SEO strategy, content calendar

### `/docs/authentication/` (4 documents)

OAuth 2.0 and authentication guides:
- **OAUTH_INTEGRATION_GUIDE.md** - Complete OAuth integration guide (331 lines)
- **ZITADEL-SETUP-GUIDE.md** - Zitadel setup instructions
- **ZITADEL-OAUTH-COMPLETION-SUMMARY.md** - OAuth completion summary
- **OAUTH-FINAL-REVIEW-CHECKLIST.md** - Security audit checklist

**Total:** 3 + 5 + 6 + 6 + 4 + 8 + 9 + 5 + 4 = **50 documents** (51 counting ARCHITECTURE-REVIEW-REPORT.md)

---

_This guide was created to help Claude Code navigate the Family Hub project efficiently. For full context, always refer to the `/docs/` folder._

**Last updated:** 2025-12-23
**CLAUDE.md version:** 2.3 (Reorganized docs into thematic folders)
