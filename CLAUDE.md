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

**📍 CURRENT PHASE: Phase 0 - Foundation & Tooling (READY TO START)**
- **Phase 1 Preparation:** ✅ COMPLETED (December 2024 - Issues #4-11)
- **57 documents** (265,000+ words) of planning complete
- **Technology stack confirmed:** .NET Core 10, Angular v21, GraphQL, RabbitMQ
- **Architecture validated:** Modular monolith first (ADR-001)
- **Next:** 3 weeks to set up dev environment, CI/CD, project structure

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

1. **Check phase:** [implementation-roadmap.md](docs/implementation-roadmap.md) - Is this feature scheduled for current phase?
2. **Find feature:** [FEATURE_BACKLOG.md](docs/FEATURE_BACKLOG.md) - Priority, dependencies, RICE score
3. **Review wireframes:** [wireframes.md](docs/wireframes.md) - UI layout and user flow
4. **Check design system:** [design-system.md](docs/design-system.md) - Components and styling
5. **Identify module:** [domain-model-microservices-map.md](docs/domain-model-microservices-map.md) - Which of 8 modules?
6. **Review events:** Does this feature publish/consume domain events?
7. **Check event chains:** [event-chains-reference.md](docs/event-chains-reference.md) - Part of workflow?
8. **Verify accessibility:** [accessibility-strategy.md](docs/accessibility-strategy.md) - WCAG 2.1 AA requirements
9. **Review risks:** [risk-register.md](docs/risk-register.md) - Mitigation strategies
10. **Implement:** Follow module patterns, test thoroughly

### Architecture Questions

**Read these 3 docs:**
1. [ADR-001](docs/architecture/ADR-001-MODULAR-MONOLITH-FIRST.md) - Modular monolith decision rationale
2. [Domain Model](docs/domain-model-microservices-map.md) - 8 DDD modules, domain events, GraphQL schemas
3. [Event Chains Reference](docs/event-chains-reference.md) - Event-driven patterns and workflows

### Planning & Roadmap Questions

**Read these 3 docs:**
1. [Implementation Roadmap](docs/implementation-roadmap.md) - 6-phase plan (Phase 0-6), deliverables, timeline
2. [Feature Backlog](docs/FEATURE_BACKLOG.md) - 208 features prioritized by RICE score
3. [Risk Register](docs/risk-register.md) - 35 risks with mitigation strategies

### Product Strategy Questions

**Read these 3 docs:**
1. [Product Strategy](docs/PRODUCT_STRATEGY.md) - Vision, personas, strategic pillars, positioning
2. [UX Research Report](docs/ux-research-report.md) - 6 personas, user journeys, competitive analysis
3. [Executive Summary](docs/executive-summary.md) - 15-minute overview of vision, market, strategy

---

## 📚 DOCUMENTATION INDEX

**57 documents** (265,000+ words) organized by category. All paths relative to `/docs/`.

| Category | Key Documents | Purpose |
|----------|---------------|---------|
| **Phase 1 Completion** | [ISSUE-4-PHASE-1-COMPLETION-SUMMARY.md](docs/ISSUE-4-PHASE-1-COMPLETION-SUMMARY.md) | Phase 1 prep complete, ready for Phase 0 |
| **Architecture (Issue #11)** | [ADR-001](docs/architecture/ADR-001-MODULAR-MONOLITH-FIRST.md), [Architecture Review](docs/architecture/ARCHITECTURE-REVIEW-REPORT.md), [Deliverables](docs/architecture/ISSUE-11-DELIVERABLES-SUMMARY.md) | Modular monolith decision, review findings, validation |
| **Product Strategy (Issue #5)** | [executive-summary.md](docs/executive-summary.md), [PRODUCT_STRATEGY.md](docs/PRODUCT_STRATEGY.md), [FEATURE_BACKLOG.md](docs/FEATURE_BACKLOG.md) (208 features), [ROADMAP_VISUAL.md](docs/ROADMAP_VISUAL.md), [ISSUE_5_SUMMARY.md](docs/ISSUE_5_SUMMARY.md) | Vision, personas, RICE-scored features, visual timeline |
| **Technical Architecture** | [domain-model-microservices-map.md](docs/domain-model-microservices-map.md), [implementation-roadmap.md](docs/implementation-roadmap.md), [risk-register.md](docs/risk-register.md) | 8 DDD modules, 6-phase plan, 35 risks |
| **Cloud & Kubernetes (Issue #6)** | [cloud-architecture.md](docs/cloud-architecture.md), [kubernetes-deployment-guide.md](docs/kubernetes-deployment-guide.md), [infrastructure-cost-analysis.md](docs/infrastructure-cost-analysis.md), +5 docs | Kubernetes architecture, deployment, costs, Helm charts, CI/CD, multi-tenancy, observability |
| **Security (Issue #8)** | [ISSUE-8-SECURITY-SUMMARY.md](docs/ISSUE-8-SECURITY-SUMMARY.md), [threat-model.md](docs/threat-model.md), [security-testing-plan.md](docs/security-testing-plan.md), [vulnerability-management.md](docs/vulnerability-management.md), +1 doc | 53 threats (STRIDE), OWASP Top 10, SAST/DAST, incident response |
| **Legal Compliance (Issue #10)** | [legal/LEGAL-COMPLIANCE-SUMMARY.md](docs/legal/LEGAL-COMPLIANCE-SUMMARY.md), [legal/privacy-policy.md](docs/legal/privacy-policy.md), [legal/terms-of-service.md](docs/legal/terms-of-service.md), [legal/quick-reference-coppa-workflow.md](docs/legal/quick-reference-coppa-workflow.md), +5 docs | GDPR/COPPA/CCPA compliance, 93-item checklist, policies, DPA templates |
| **Market Strategy (Issue #9)** | [ISSUE-9-MARKET-STRATEGY-SUMMARY.md](docs/ISSUE-9-MARKET-STRATEGY-SUMMARY.md), [market-research-report.md](docs/market-research-report.md), [go-to-market-plan.md](docs/go-to-market-plan.md), +2 docs | Competitive analysis (2,700+ reviews), GTM plan, SEO/content |
| **UX/UI Design (Issue #7)** | [ux-research-report.md](docs/ux-research-report.md), [design-system.md](docs/design-system.md), [wireframes.md](docs/wireframes.md), [angular-component-specs.md](docs/angular-component-specs.md), [accessibility-strategy.md](docs/accessibility-strategy.md), +6 docs | 6 personas, design system (22+ components), complete wireframes, WCAG 2.1 AA, responsive design |
| **Event Chains** | [event-chains-reference.md](docs/event-chains-reference.md), [event-chain-ux.md](docs/event-chain-ux.md) | 10 workflow specs, UX patterns, testing strategies |
| **Supporting Docs** | [architecture-visual-summary.md](docs/architecture-visual-summary.md), [DELIVERABLES_SUMMARY.md](docs/DELIVERABLES_SUMMARY.md) | System diagrams, Issue #5 checklist |
| **Navigation Aids** | [INDEX.md](docs/INDEX.md), [README.md](docs/README.md) | Complete documentation map with FAQ, docs overview |

**Quick Access:**
- **Start here:** [Executive Summary](docs/executive-summary.md) - 15-minute overview
- **For developers:** [ADR-001](docs/architecture/ADR-001-MODULAR-MONOLITH-FIRST.md) + [Domain Model](docs/domain-model-microservices-map.md)
- **For product:** [Product Strategy](docs/PRODUCT_STRATEGY.md) + [Feature Backlog](docs/FEATURE_BACKLOG.md)
- **For design:** [UX Research](docs/ux-research-report.md) + [Wireframes](docs/wireframes.md) + [Design System](docs/design-system.md)

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

**Full spec:** See [domain-model-microservices-map.md](docs/domain-model-microservices-map.md) for domain entities, events, GraphQL schemas.

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

**Full specifications:** See [event-chains-reference.md](docs/event-chains-reference.md) for implementation patterns, monitoring, testing strategies.

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
- 57 documents (265,000+ words) of planning (45 in `/docs/`, 3 in `/docs/architecture/`, 9 in `/docs/legal/`)
- Technology stack confirmed (.NET Core 10, Angular v21, GraphQL, RabbitMQ)
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

**Full roadmap:** [implementation-roadmap.md](docs/implementation-roadmap.md)

---

## 🛠️ DEVELOPMENT WORKFLOWS

### For New Claude Code Sessions

1. **Read CLAUDE.md** (this file) - Critical context in first 80 lines
2. **Check current phase** - [implementation-roadmap.md](docs/implementation-roadmap.md)
3. **Review architecture decision** - [ADR-001](docs/architecture/ADR-001-MODULAR-MONOLITH-FIRST.md)
4. **Understand domain model** - [domain-model-microservices-map.md](docs/domain-model-microservices-map.md)

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
1. Check [implementation-roadmap.md](docs/implementation-roadmap.md) for phase scheduling
2. Review [FEATURE_BACKLOG.md](docs/FEATURE_BACKLOG.md) for priority and RICE score
3. Consult [domain-model-microservices-map.md](docs/domain-model-microservices-map.md) for module ownership

**When asked about architecture:**
1. Start with [ADR-001](docs/architecture/ADR-001-MODULAR-MONOLITH-FIRST.md) for strategic decision
2. Review [domain-model-microservices-map.md](docs/domain-model-microservices-map.md) for DDD patterns
3. Check [event-chains-reference.md](docs/event-chains-reference.md) for event-driven patterns

**When planning work:**
- Follow phases in [implementation-roadmap.md](docs/implementation-roadmap.md) - don't skip ahead
- Each phase has clear deliverables and success criteria
- Validate assumptions before implementing
- Keep Federation deferred - online service first

---

## 📋 APPENDIX: COMPLETE DOCUMENTATION MAP

**All 57 markdown documents** organized by location:

### `/docs/` Root (45 documents)

**Phase 1 Completion:**
- ISSUE-4-PHASE-1-COMPLETION-SUMMARY.md - Preparation complete, ready for Phase 0

**Product Strategy (Issue #5):**
- executive-summary.md - 15-min overview (vision, market, strategy)
- PRODUCT_STRATEGY.md - Complete strategy, personas, roadmap
- FEATURE_BACKLOG.md - 208 features (RICE scored)
- ROADMAP_VISUAL.md - Visual timeline with ASCII charts
- ISSUE_5_SUMMARY.md - Issue #5 deliverables summary

**Technical Architecture:**
- domain-model-microservices-map.md - 8 DDD modules, domain events, GraphQL
- implementation-roadmap.md - 6-phase plan (10-14 months)
- risk-register.md - 35 risks with mitigation
- event-chains-reference.md - 10 workflow specs
- architecture-visual-summary.md - System diagrams (ASCII)
- DELIVERABLES_SUMMARY.md - Issue #5 checklist
- INDEX.md - Documentation map with FAQ
- README.md - Docs directory overview

**Cloud & Kubernetes (Issue #6):**
- cloud-architecture.md - Kubernetes architecture (Phase 5+)
- kubernetes-deployment-guide.md - Deployment (local/cloud)
- helm-charts-structure.md - Helm chart templates
- observability-stack.md - Prometheus/Grafana/Loki
- cicd-pipeline.md - GitHub Actions/ArgoCD
- multi-tenancy-strategy.md - PostgreSQL RLS
- infrastructure-cost-analysis.md - Costs ($200-5K/mo)
- ISSUE-6-DELIVERABLES-SUMMARY.md - Issue #6 summary

**Security (Issue #8):**
- threat-model.md - STRIDE analysis (53 threats)
- security-testing-plan.md - OWASP Top 10, SAST/DAST
- vulnerability-management.md - Severity, remediation SLAs
- security-monitoring-incident-response.md - Monitoring, incident playbooks
- ISSUE-8-SECURITY-SUMMARY.md - Issue #8 summary

**Market Strategy (Issue #9):**
- market-research-report.md - Competitive analysis (2,700+ reviews)
- go-to-market-plan.md - GTM strategy
- seo-content-strategy.md - SEO/content plan
- brand-positioning.md - Brand guidelines
- competitive-analysis.md - Competitor analysis
- ISSUE-9-MARKET-STRATEGY-SUMMARY.md - Issue #9 summary

**UX/UI Design (Issue #7):**
- ux-research-report.md - 6 personas, user journeys
- information-architecture.md - Site map, navigation
- wireframes.md - Complete MVP wireframes
- design-system.md - Design system (22+ components)
- angular-component-specs.md - Angular v21 components
- accessibility-strategy.md - WCAG 2.1 AA + COPPA
- event-chain-ux.md - Event chain UX patterns
- responsive-design-guide.md - Mobile-first design
- interaction-design-guide.md - Micro-interactions
- ISSUE-7-UI-DESIGN-SUMMARY.md - Issue #7 UI summary
- ISSUE-7-UX-RESEARCH-SUMMARY.md - Issue #7 UX summary

### `/docs/architecture/` (3 documents)

- **ADR-001-MODULAR-MONOLITH-FIRST.md** - Architecture decision record
- **ARCHITECTURE-REVIEW-REPORT.md** - Comprehensive architecture review
- **ISSUE-11-DELIVERABLES-SUMMARY.md** - Issue #11 summary

### `/docs/legal/` (9 documents)

- **LEGAL-COMPLIANCE-SUMMARY.md** - Comprehensive compliance overview
- **terms-of-service.md** - Terms of Service
- **privacy-policy.md** - Privacy Policy (GDPR/COPPA/CCPA)
- **cookie-policy.md** - Cookie disclosure
- **compliance-checklist.md** - 93 compliance items
- **data-processing-agreement-template.md** - DPA templates
- **quick-reference-coppa-workflow.md** - COPPA implementation
- **ISSUE-10-DELIVERABLES.md** - Issue #10 deliverables
- **README.md** - Legal docs quick start

**Total:** 45 + 3 + 9 = **57 documents**

---

_This guide was created to help Claude Code navigate the Family Hub project efficiently. For full context, always refer to the `/docs/` folder._

**Last updated:** 2025-12-21
**CLAUDE.md version:** 2.0 (Optimized)
