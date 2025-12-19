# Family Hub - Claude Code Guide

## Project Overview

**Family Hub** is a privacy-first, cloud-based family organization platform that reduces mental load through intelligent **event chain automation**. Built for busy families (2-6 members) seeking better coordination without the overwhelm.

### Primary Differentiator

**Event Chain Automation** - Automatic cross-domain workflows that no competitor offers:

- Doctor appointment → calendar event → preparation task → prescription → shopping list → pickup task → refill reminder
- Meal plan → shopping list → budget tracking → recipe suggestions
- Task assignment → notifications → reminders → completion tracking

### Technology Stack

- **Backend**: .NET Core 10 / C# 14 with Hot Chocolate GraphQL
- **Frontend**: Angular v21 + TypeScript + Tailwind CSS
- **Database**: PostgreSQL 16 (per service)
- **Cache/Events**: Redis 7 (Pub/Sub for event bus)
- **Auth**: Zitadel (external OAuth 2.0 / OIDC provider)
- **Infrastructure**: Kubernetes (cloud-agnostic)
- **Monitoring**: Prometheus + Grafana + Seq

### Development Approach

- **Single developer** with **Claude Code AI assistance** (60-80% of boilerplate)
- Domain-Driven Design (DDD) with 8 bounded contexts
- Event-driven microservices architecture
- Incremental implementation over 12-18 months

---

## 📁 Documentation Hub: `/docs/` Folder

**IMPORTANT**: All comprehensive planning and architecture documentation is stored in the `/docs/` folder.

The `/docs/` folder contains **14 documents** totaling **68,500+ words** of detailed planning:

- Product strategy and feature prioritization
- Technical architecture and domain models
- Implementation roadmap and risk analysis
- Event chain specifications
- Visual roadmaps and summaries

**Always check `/docs/` first** when you need to understand:

- Product vision and strategy
- Feature priorities and backlog
- Technical architecture decisions
- Implementation phases and timeline
- Domain models and microservices structure
- Event chain workflows
- Risk mitigation strategies

---

## Quick Navigation by Role

### For Product Managers

Start here to understand product strategy and priorities:

1. [`/docs/EXECUTIVE_SUMMARY.md`](docs/EXECUTIVE_SUMMARY.md) - **15-minute overview** of vision, market, competitive analysis
2. [`/docs/PRODUCT_STRATEGY.md`](docs/PRODUCT_STRATEGY.md) - Complete strategy with personas, roadmap, GTM
3. [`/docs/FEATURE_BACKLOG.md`](docs/FEATURE_BACKLOG.md) - 208 prioritized features (MVP, Phase 2, Phase 3+)
4. [`/docs/ROADMAP_VISUAL.md`](docs/ROADMAP_VISUAL.md) - Visual timeline with ASCII charts

### For Developers

Start here to understand technical architecture:

1. [`/docs/domain-model-microservices-map.md`](docs/domain-model-microservices-map.md) - **8 microservices** with DDD models, GraphQL schemas
2. [`/docs/implementation-roadmap.md`](docs/implementation-roadmap.md) - **6-phase plan** (12-18 months) with deliverables
3. [`/docs/event-chains-reference.md`](docs/event-chains-reference.md) - Event chain specifications and patterns
4. [`/docs/risk-register.md`](docs/risk-register.md) - 35 risks with mitigation strategies

### For Stakeholders

Start here for high-level overview:

1. [`/docs/EXECUTIVE_SUMMARY.md`](docs/EXECUTIVE_SUMMARY.md) - Vision, value proposition, revenue projections
2. [`/docs/ISSUE_5_SUMMARY.md`](docs/ISSUE_5_SUMMARY.md) - Phase 1 deliverables and success criteria
3. [`/docs/ROADMAP_VISUAL.md`](docs/ROADMAP_VISUAL.md) - Visual roadmap with success metrics

---

## Core Planning Documents

### Product Strategy (Issue #5 Deliverables)

1. **`EXECUTIVE_SUMMARY.md`** (~17KB)

   - Project vision: "Intelligent family organization platform with event chain automation"
   - Target market: 80M families in US, 60% feeling overwhelmed
   - Competitive analysis vs Cozi, FamilyWall, TimeTree, Picniic
   - Revenue projections: Year 1 ($3,500), Year 2 ($30K), Year 3 ($150K)
   - Business model: Freemium SaaS (Free, Premium $9.99/mo, Family $14.99/mo, Enterprise)

2. **`PRODUCT_STRATEGY.md`** (~24KB)

   - Complete product vision and mission
   - 3 detailed user personas (Sarah - Organized Parent, Mike - Practical Dad, Emma - Busy Teen)
   - Strategic pillars: Intelligent Automation, Privacy & Control, Modern Experience, Extensibility
   - Competitive positioning matrix
   - Success metrics and KPI framework
   - 3-phase product roadmap

3. **`FEATURE_BACKLOG.md`** (~42KB)

   - **208 total features** prioritized using RICE scoring
   - **MVP (49 features)**: Calendar, Tasks, Lists, Event Chain Automation, Mobile PWA, Infrastructure
   - **Phase 2 (65 features)**: Meals, Budget, Documents, Advanced Event Chains, Native Mobile
   - **Phase 3+ (94 features)**: AI/ML, Analytics, Integrations, Platform, Enterprise
   - 16 functional domains
   - Competitive feature comparison matrix

4. **`ROADMAP_VISUAL.md`** (~34KB)

   - Visual timeline with ASCII charts
   - Feature stack diagrams showing evolution
   - Success trajectory graphs
   - Competitive positioning evolution
   - Domain coverage timeline

5. **`ISSUE_5_SUMMARY.md`** (~15KB)
   - Product Strategy & Feature Prioritization completion summary
   - Key strategic decisions documented
   - Quick reference for MVP, Phase 2, Phase 3+
   - Links to GitHub Issue #5

### Technical Architecture

1. **`domain-model-microservices-map.md`** (~61KB)

   - **8 Bounded Contexts** (DDD microservices):
     1. Auth Service (Zitadel integration)
     2. Calendar Service (events, appointments, recurrence)
     3. Task Service (to-dos, assignments, chores)
     4. Shopping Service (lists, items, sharing)
     5. Health Service (appointments, prescriptions, tracking)
     6. Meal Planning Service (meal plans, recipes, nutrition)
     7. Finance Service (budgets, expenses, tracking)
     8. Communication Service (notifications, messaging)
     9. **Federation Service** (deferred to Phase 7+ - self-hosting and fediverse)
   - Domain entities and aggregates with C# code examples
   - Domain events (published/consumed) for each service
   - GraphQL API schema outlines
   - **Event chain specifications** (flagship feature workflows)
   - Cross-service integration patterns
   - Database storage strategies per service
   - Kubernetes deployment architecture

2. **`implementation-roadmap.md`** (~35KB)

   - **6-phase development plan** (52 weeks / 12-18 months):
     - Phase 0: Foundation & Tooling (4 weeks)
     - Phase 1: Core MVP - Auth + Calendar + Tasks (8 weeks)
     - Phase 2: Health Integration & Event Chains (6 weeks)
     - Phase 3: Meal Planning & Finance (8 weeks)
     - Phase 4: Recurrence & Advanced Features (8 weeks)
     - Phase 5: Microservices Extraction & Production Hardening (10 weeks)
     - Phase 6: Mobile App & Extended Features (8+ weeks)
     - **Phase 7: Federation (DEFERRED)** - Self-hosting and fediverse integration
   - Deliverables and success criteria per phase
   - Single developer optimization strategies
   - AI-assisted development approach (Claude Code 60-80% automation)
   - Technology decision points and POCs
   - Cost estimation: $2,745-5,685 first year
   - Risk mitigation timeline

3. **`risk-register.md`** (~33KB)
   - **35 risks identified** across 5 categories:
     - Market & Product Risks (9 risks)
     - Technical Risks (10 risks)
     - Business & Financial Risks (7 risks)
     - Operational Risks (5 risks)
     - Legal & Compliance Risks (4 risks)
   - Risk scoring: Probability (1-5) × Impact (1-5)
   - **Critical risks** (score 20):
     - Low User Adoption
     - Developer Burnout
   - **High risks** (score 12-16):
     - Event Bus Bottleneck
     - Database Scalability
     - Zitadel Integration Complexity
   - Comprehensive mitigation strategies for each risk
   - Monitoring metrics and contingency plans

### Supporting Documents

1. **`architecture-visual-summary.md`** (~69KB)

   - System architecture diagrams (ASCII)
   - Bounded context relationships map
   - Event flow visualizations
   - Data flow patterns
   - Database schema overview
   - Kubernetes deployment map
   - Security architecture
   - Monitoring dashboard layout

2. **`event-chains-reference.md`** (~23KB)

   - 10 fully specified event chains:
     1. Doctor Appointment → Calendar → Shopping → Task
     2. Prescription → Shopping List → Task
     3. Meal Plan → Shopping List → Finance
     4. Budget Threshold Alert
     5. Recurring Task Automation
     6. And 5 more workflows
   - Expected outcomes and time savings per chain
   - Implementation patterns (Direct, Saga, Enrichment)
   - Monitoring metrics and dashboards
   - Testing strategies (unit, integration, load)
   - Troubleshooting guide

3. **`DELIVERABLES_SUMMARY.md`** (~16KB)

   - Issue #5 completion checklist
   - Traceability matrix (requirements → design → implementation)
   - Stakeholder sign-off section
   - Quick start guides

4. **`INDEX.md`** (~23KB)

   - Complete documentation navigation map
   - Quick start by role (PM, Developer, Stakeholder)
   - FAQ section
   - Next steps guidance

5. **`README.md`** (~18KB)
   - Documentation directory overview
   - Quick start guides
   - Technology summary
   - Success metrics

---

## 🚀 Getting Started with Family Hub

### For New Claude Code Sessions

1. **Read this file first** (CLAUDE.md) to understand project structure
2. **Check `/docs/EXECUTIVE_SUMMARY.md`** for vision and value proposition
3. **Review `/docs/domain-model-microservices-map.md`** for technical architecture
4. **Consult `/docs/implementation-roadmap.md`** to understand current phase and next steps

### Understanding the Codebase

**When you need to understand**:

- **Product vision**: Read `/docs/EXECUTIVE_SUMMARY.md` or `/docs/PRODUCT_STRATEGY.md`
- **Feature priorities**: Read `/docs/FEATURE_BACKLOG.md`
- **Technical architecture**: Read `/docs/domain-model-microservices-map.md`
- **Implementation plan**: Read `/docs/implementation-roadmap.md`
- **Event chains**: Read `/docs/event-chains-reference.md`
- **Risks**: Read `/docs/risk-register.md`

**When implementing features**:

1. Check which **phase** the feature belongs to in `/docs/implementation-roadmap.md`
2. Find the feature in `/docs/FEATURE_BACKLOG.md` to understand priority and dependencies
3. Identify the **bounded context** (microservice) in `/docs/domain-model-microservices-map.md`
4. Review relevant **domain events** and **GraphQL schema** in the domain model doc
5. Check for any **event chain** integration in `/docs/event-chains-reference.md`
6. Review associated **risks** in `/docs/risk-register.md`

---

## 🎯 Key Concepts

### Event Chain Automation (Flagship Feature)

**What it is**: Intelligent cross-domain workflows that automatically trigger related actions across different services.

**Example**: Doctor Appointment Event Chain

```
User schedules doctor appointment (Health Service)
  ↓ (automatic)
Calendar event created (Calendar Service)
  ↓ (automatic)
Preparation task created (Task Service)
  ↓ (automatic)
Prescription issued after appointment (Health Service)
  ↓ (automatic)
Medication added to shopping list (Shopping Service)
  ↓ (automatic)
Pickup task created (Task Service)
  ↓ (automatic)
Refill reminder scheduled (Communication Service)
```

**Why it matters**: Saves 10-30 minutes per workflow, eliminates 3-5 things to remember, reduces cognitive load.

**Technical implementation**: Event-driven architecture using Redis Pub/Sub (Phase 1-4) → RabbitMQ (Phase 5+)

### Microservices Architecture (8 Bounded Contexts)

**DDD Approach**: Each microservice owns a bounded context with:

- Domain entities and aggregates
- Domain events (published/consumed)
- GraphQL API schema
- Independent database (PostgreSQL)
- Event bus integration (Redis)

**Services**:

1. **Auth Service**: Zitadel integration, family groups, permissions
2. **Calendar Service**: Events, appointments, recurrence, reminders
3. **Task Service**: To-dos, assignments, chores, gamification
4. **Shopping Service**: Lists, items, categories, sharing
5. **Health Service**: Appointments, prescriptions, medications, tracking
6. **Meal Planning Service**: Meal plans, recipes, ingredients, nutrition
7. **Finance Service**: Budgets, expenses, income, goals
8. **Communication Service**: Notifications, messages, activity feed

**Future (Phase 7+)**: 9. **Federation Service**: Self-hosting, instance federation (deferred)

### Development Philosophy

**Single Developer + AI**: Project designed for one developer with Claude Code generating 60-80% of:

- Boilerplate code
- Unit and integration tests
- API documentation
- Database schemas
- GraphQL resolvers

**Quality over Speed**: 12-18 months for MVP, building it right from the start:

- Comprehensive testing
- Clean architecture (DDD, CQRS, Event Sourcing where appropriate)
- Production-ready from Phase 5
- Security and privacy built-in

**Incremental Delivery**: Each phase delivers working software with real user value.

---

## 📋 Current Phase & Status

### Strategic Pivot (December 2024)

**Decision**: Launch as pure **online service first**, defer self-hosting and federation to Phase 7+ (post-MVP)

**Rationale**:

- ✅ Faster time to market (12 months vs 15-18 months)
- ✅ Simpler infrastructure and operations
- ✅ Focus on core differentiator (event chains) first
- ✅ Validate product-market fit before adding complexity
- ✅ Federation still planned for future (just later)

### Current Status

**Phase**: Pre-implementation (Phase 0 not started)

**Completed**:

- ✅ Product strategy and feature prioritization (Issue #5)
- ✅ Technical architecture design (8 microservices)
- ✅ Implementation roadmap (6 phases)
- ✅ Risk analysis (35 risks identified)
- ✅ Event chain specifications (10 workflows)
- ✅ 208 features prioritized

**Next Steps**:

1. Start Phase 0: Foundation & Tooling (4 weeks)

   - Set up development environment
   - Configure CI/CD pipeline
   - Create project structure
   - Initialize Git repository structure
   - Set up Zitadel instance
   - Create Docker Compose for local dev

2. Proceed to Phase 1: Core MVP (8 weeks)
   - Auth Service with Zitadel integration
   - Calendar Service with events
   - Task Service with assignments
   - Basic event chains

### Success Criteria for MVP

**User Metrics** (by end of Phase 6):

- 100 active families
- 80%+ Day 30 retention
- 50%+ using event chains weekly
- NPS > 40

**Business Metrics**:

- 25+ premium subscribers
- $2,500+ MRR
- Positive unit economics

**Technical Metrics**:

- 99.5%+ uptime
- <2s event chain latency
- <3s p95 API response time

---

## 🔗 Quick Links

### Essential Reading

- [Executive Summary](docs/EXECUTIVE_SUMMARY.md) - Start here (15 min read)
- [Domain Model & Microservices](docs/domain-model-microservices-map.md) - Technical architecture
- [Implementation Roadmap](docs/implementation-roadmap.md) - Phase-by-phase plan
- [Feature Backlog](docs/FEATURE_BACKLOG.md) - All 208 features prioritized

### By Topic

- **Product Strategy**: [PRODUCT_STRATEGY.md](docs/PRODUCT_STRATEGY.md)
- **Event Chains**: [event-chains-reference.md](docs/event-chains-reference.md)
- **Risks**: [risk-register.md](docs/risk-register.md)
- **Visual Roadmap**: [ROADMAP_VISUAL.md](docs/ROADMAP_VISUAL.md)
- **Architecture Diagrams**: [architecture-visual-summary.md](docs/architecture-visual-summary.md)
- **Documentation Index**: [INDEX.md](docs/INDEX.md)

### GitHub Issues

- [Issue #5: Product Strategy & Feature Prioritization](https://github.com/andrekirst/family2/issues/5)
- [Issue #4: Master Implementation Plan](https://github.com/andrekirst/family2/issues/4)
- [Issue #1: Family Hub Feature Ideas](https://github.com/andrekirst/family2/issues/1)

---

## 💡 Tips for Claude Code

### When Asked to Implement Features

1. **Check the phase** in `/docs/implementation-roadmap.md` - is this feature scheduled for current phase?
2. **Find the feature** in `/docs/FEATURE_BACKLOG.md` - understand priority, dependencies, RICE score
3. **Identify the service** in `/docs/domain-model-microservices-map.md` - which bounded context?
4. **Review domain events** - does this feature trigger or consume events?
5. **Check event chains** in `/docs/event-chains-reference.md` - is this part of a workflow?
6. **Consider risks** in `/docs/risk-register.md` - any mitigation strategies to follow?

### When Asked About Product Direction

- Refer to `/docs/PRODUCT_STRATEGY.md` for vision, personas, and strategic pillars
- Check `/docs/FEATURE_BACKLOG.md` for feature priorities and competitive analysis
- Review `/docs/EXECUTIVE_SUMMARY.md` for high-level positioning and market gaps

### When Asked About Architecture Decisions

- Consult `/docs/domain-model-microservices-map.md` for DDD patterns and service boundaries
- Review `/docs/event-chains-reference.md` for event-driven patterns
- Check `/docs/architecture-visual-summary.md` for system diagrams

### When Planning Implementation

- Follow phases in `/docs/implementation-roadmap.md` - don't skip ahead
- Each phase has clear deliverables and success criteria
- Validate assumptions with user before implementing
- Keep Phase 7 (Federation) deferred - online service first!

---

## 🚨 Important Notes

### Strategic Decisions

1. **Online Service First**: Launch as cloud-based SaaS, NOT self-hosted initially

   - Self-hosting and federation deferred to Phase 7+ (post-MVP)
   - Focus on event chain automation as PRIMARY differentiator
   - Simpler operations, faster to market

2. **Event Chains are #1 Priority**: This is what makes Family Hub unique

   - No competitor offers automated cross-domain workflows
   - Saves users 10-30 minutes per workflow
   - Must work flawlessly

3. **Single Developer Approach**: Project designed for AI-assisted solo development

   - Claude Code generates 60-80% of code
   - Quality over speed
   - Incremental delivery

4. **Privacy-First but Pragmatic**: GDPR compliant, no data selling, but cloud-hosted for ease of use
   - Self-hosting comes later when we have traction
   - Privacy advocates are future audience, not initial target

### What NOT to Do

❌ **Don't implement Federation Service** (Phase 7+, deferred)
❌ **Don't skip phases** in implementation roadmap
❌ **Don't ignore event chains** - they're the core innovation
❌ **Don't assume features** - check `/docs/FEATURE_BACKLOG.md` for priorities
❌ **Don't break DDD boundaries** - respect microservice ownership
❌ **Don't duplicate documentation** - reference `/docs/` instead

---

## 📚 Documentation Summary

**Total**: 14 documents, 68,500+ words
**Location**: `/home/andrekirst/git/github/andrekirst/family2/docs/`
**Purpose**: Comprehensive planning and architecture for Family Hub
**Audience**: Product managers, developers, stakeholders, Claude Code

**Remember**: Always check `/docs/` folder first when you need context about Family Hub!

---

_This guide was created to help Claude Code navigate the Family Hub project efficiently. For the full context, always refer to the `/docs/` folder._
