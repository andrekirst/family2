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

The `/docs/` folder contains **47 documents** totaling **230,000+ words** of detailed planning:

- Product strategy and feature prioritization
- Technical architecture and domain models
- Cloud architecture and Kubernetes deployment
- Security architecture and compliance (Issue #8)
- Legal compliance and data protection (NEW - Issue #10)
- Market strategy and go-to-market planning (Issue #9)
- UX research and UI design system
- Implementation roadmap and risk analysis
- Event chain specifications
- Visual roadmaps and summaries

**Always check `/docs/` first** when you need to understand:

- Product vision and strategy
- Feature priorities and backlog
- Technical architecture decisions
- Security architecture and threat modeling
- Legal compliance and data protection (NEW - GDPR, COPPA, CCPA)
- Market strategy and go-to-market planning
- UX research and UI design patterns
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
2. [`/docs/cloud-architecture.md`](docs/cloud-architecture.md) - **Kubernetes architecture** with multi-tenancy, deployment guides
3. [`/docs/kubernetes-deployment-guide.md`](docs/kubernetes-deployment-guide.md) - Step-by-step deployment for local and cloud
4. [`/docs/implementation-roadmap.md`](docs/implementation-roadmap.md) - **6-phase plan** (12-18 months) with deliverables
5. [`/docs/event-chains-reference.md`](docs/event-chains-reference.md) - Event chain specifications and patterns
6. [`/docs/risk-register.md`](docs/risk-register.md) - 35 risks with mitigation strategies

### For Security Engineers

Start here to understand security architecture and compliance:

1. [`/docs/threat-model.md`](docs/threat-model.md) - **STRIDE analysis**, 53 threats identified, attack surface mapping
2. [`/docs/security-testing-plan.md`](docs/security-testing-plan.md) - **OWASP Top 10**, SAST/DAST, penetration testing schedule
3. [`/docs/vulnerability-management.md`](docs/vulnerability-management.md) - Severity classification, remediation SLAs, disclosure policy
4. [`/docs/security-monitoring-incident-response.md`](docs/security-monitoring-incident-response.md) - Monitoring strategy, incident response playbooks
5. [`/docs/ISSUE-8-SECURITY-SUMMARY.md`](docs/ISSUE-8-SECURITY-SUMMARY.md) - Security architecture executive summary

### For Legal Advisors

Start here to understand legal compliance and data protection:

1. [`/docs/legal/LEGAL-COMPLIANCE-SUMMARY.md`](docs/legal/LEGAL-COMPLIANCE-SUMMARY.md) - **Comprehensive compliance overview**, legal risk assessment
2. [`/docs/legal/terms-of-service.md`](docs/legal/terms-of-service.md) - **Terms of Service** with family-specific provisions
3. [`/docs/legal/privacy-policy.md`](docs/legal/privacy-policy.md) - **Privacy Policy** (GDPR, COPPA, CCPA compliant)
4. [`/docs/legal/cookie-policy.md`](docs/legal/cookie-policy.md) - Cookie disclosure and consent mechanisms
5. [`/docs/legal/compliance-checklist.md`](docs/legal/compliance-checklist.md) - **93 compliance items** across GDPR, COPPA, CCPA
6. [`/docs/legal/data-processing-agreement-template.md`](docs/legal/data-processing-agreement-template.md) - DPA templates for third-party processors
7. [`/docs/legal/quick-reference-coppa-workflow.md`](docs/legal/quick-reference-coppa-workflow.md) - **COPPA implementation** with code examples
8. [`/docs/legal/README.md`](docs/legal/README.md) - Legal documentation quick start guide

### For UX/UI Designers

Start here to understand user experience and design system:

1. [`/docs/ux-research-report.md`](docs/ux-research-report.md) - **6 personas**, user journeys, competitive analysis (2,700+ reviews)
2. [`/docs/information-architecture.md`](docs/information-architecture.md) - Site map, role-based navigation, permission matrix
3. [`/docs/wireframes.md`](docs/wireframes.md) - Complete wireframes for all MVP screens
4. [`/docs/design-system.md`](docs/design-system.md) - Visual design system, color palette, typography, **22+ components**
5. [`/docs/angular-component-specs.md`](docs/angular-component-specs.md) - Angular v21 component architecture
6. [`/docs/accessibility-strategy.md`](docs/accessibility-strategy.md) - **WCAG 2.1 AA** + **COPPA** compliance
7. [`/docs/event-chain-ux.md`](docs/event-chain-ux.md) - Event chain UX design (flagship feature)
8. [`/docs/responsive-design-guide.md`](docs/responsive-design-guide.md) - Mobile-first responsive strategy
9. [`/docs/interaction-design-guide.md`](docs/interaction-design-guide.md) - Micro-interactions and animations

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

### Cloud Architecture & Kubernetes (Issue #6 Deliverables)

1. **`cloud-architecture.md`** (~76KB)

   - Complete Kubernetes architecture with ASCII diagrams
   - Multi-tenancy strategy with PostgreSQL Row-Level Security
   - 8 microservices deployment architecture
   - NGINX Ingress Controller (no service mesh)
   - Security architecture with zero-trust principles
   - Namespace organization and resource management
   - Database and cache configuration
   - Network policies and service mesh considerations

2. **`kubernetes-deployment-guide.md`** (~42KB)

   - Step-by-step deployment for local (Minikube/kind) and cloud
   - Environment setup (development, staging, production)
   - PostgreSQL and Redis deployment
   - Zitadel OAuth 2.0 setup
   - NGINX Ingress configuration
   - Disaster recovery procedures
   - Backup and restore strategies
   - Troubleshooting guide

3. **`helm-charts-structure.md`** (~22KB)

   - Complete Helm chart templates for all 8 microservices
   - Parent `family-hub` chart with subcharts
   - Environment-specific values (dev, staging, prod)
   - GitOps integration with ArgoCD
   - Security best practices (non-root, read-only filesystem)
   - Resource requests and limits
   - ConfigMap and Secret management

4. **`observability-stack.md`** (~27KB)

   - Prometheus + Grafana monitoring setup
   - Loki logging architecture (lightweight, 300m CPU)
   - OpenTelemetry distributed tracing
   - 25 predefined alert rules for critical issues
   - Custom dashboards for each microservice
   - Log aggregation and querying
   - Performance metrics and SLOs

5. **`cicd-pipeline.md`** (~15KB)

   - GitHub Actions CI/CD workflows
   - Build, test, and security scanning
   - ArgoCD GitOps deployment
   - Multi-environment promotion (dev → staging → prod)
   - Automated rollback on failure
   - Container image building and scanning
   - Deployment automation

6. **`multi-tenancy-strategy.md`** (~26KB)

   - PostgreSQL Row-Level Security implementation
   - Shared database with RLS policies per family (tenant)
   - Cost savings: $9,900/month vs dedicated databases
   - Automated tenant onboarding with CLI tools
   - Resource quotas and limits per tenant
   - Cost allocation and billing integration
   - Tenant isolation and security

7. **`infrastructure-cost-analysis.md`** (~24KB)

   - Detailed cost breakdowns by scale:
     - 100 families: $200-400/month
     - 1,000 families: $800-1,200/month
     - 10,000 families: $3,500-5,000/month
   - Cloud provider comparisons (DigitalOcean, AWS, Azure, GCP)
   - Break-even analysis: 45 premium subscribers @ $9.99/month
   - ROI projections and optimization strategies
   - Cost allocation per microservice
   - Recommended provider: DigitalOcean ($195/month for 100 families)

8. **`ISSUE-6-DELIVERABLES-SUMMARY.md`** (~8KB)
   - Cloud Architecture & Kubernetes completion summary
   - All 8 success criteria fulfilled
   - Critical architectural decisions documented
   - Next steps for implementation

### UX Research & UI Design (Issue #7 Deliverables)

1. **`ux-research-report.md`** (~50KB)

   - **6 Detailed Personas**: Sarah (Primary Parent), Mike (Co-Parent), Emma (Teen), Noah (Child), Margaret (Extended Family), Jessica (Guest)
   - **5 User Journey Maps**: Complete emotional arcs for key workflows
   - **Competitive Analysis**: 2,700+ user reviews analyzed (Cozi, FamilyWall, TimeTree, Picniic)
   - **10 Key Findings**: Privacy concerns (487 mentions), automation gaps (312 mentions), fragmentation pain
   - **10 Design Recommendations**: Privacy-first design, event chain discoverability, mobile-first

2. **`information-architecture.md`** (~35KB)

   - **Complete Site Map**: 6 top-level sections with 40+ screens
   - **Role-Based Navigation**: Custom navigation for 6 user personas
   - **Permission Matrix**: Detailed access control (Parent, Co-Parent, Teen, Child, Extended Family, Guest)
   - **Deep-Linking Strategy**: URL patterns for all major screens
   - **Multi-Role Experience**: Parent vs Teen vs Child dashboard designs

3. **`wireframes.md`** (~162KB)

   - Complete wireframes for all MVP screens (onboarding, dashboards, calendar, lists, tasks, event chains)
   - Multi-persona dashboards (Parent, Teen, Child with gamification)
   - Calendar views (Month, Week, Day) for desktop + mobile
   - Shopping lists with swipe gestures
   - Task & chore management with points/badges
   - Event chain visual builder
   - Mobile layouts with bottom navigation

4. **`design-system.md`** (~42KB)

   - **Brand Identity**: Visual guidelines, logo usage, brand voice
   - **Color System**: 60+ WCAG AA compliant tokens (4.5:1 contrast ratio)
   - **Typography**: Inter font family, 9 sizes, responsive scale
   - **Component Library**: 22+ production-ready components (Button, Input, Modal, Card, etc.)
   - **Design Tokens**: Tailwind CSS configuration
   - **Iconography**: Heroicons library (200+ icons)
   - **Dark Mode**: Full support with semantic color tokens
   - **Spacing & Layout**: 8px grid system

5. **`angular-component-specs.md`** (~28KB)

   - **25+ Angular v21 Components**: TypeScript implementations with standalone components
   - Button, Input, Checkbox, Toggle, Modal, Dropdown, Tabs, Card, Avatar, Badge, Toast, etc.
   - ARIA labels and accessibility patterns
   - Reactive forms integration
   - Input/Output decorators
   - Component composition patterns

6. **`accessibility-strategy.md`** (~25KB)

   - **WCAG 2.1 Level AA Compliance**: All 50 success criteria documented
   - **COPPA Compliance**: Children's Online Privacy Protection Act requirements for kids under 13
   - **Age-Appropriate Design**: Simplified UI for children, privacy controls
   - **Assistive Technology Support**: Screen readers (NVDA, JAWS, VoiceOver), magnifiers, voice control
   - **Testing Checklist**: 50+ items for accessibility validation
   - **ARIA Patterns**: Proper roles, labels, live regions

7. **`event-chain-ux.md`** (~18KB)

   - **Discovery Strategy**: Onboarding tour, contextual suggestions, navigation prominence
   - **Visualization**: Flow diagrams, status indicators, trigger/action cards
   - **Configuration UX**: Template gallery (10 pre-built chains), visual builder
   - **User Education**: Tooltips, help center, success stories
   - **Success Metrics**: Activation rate (60%), creation rate (40%), retention (2.5× lift)

8. **`responsive-design-guide.md`** (~17KB)

   - **Mobile-First Approach**: Design for smallest screen first
   - **Breakpoints**: Mobile (<640px), Tablet (640-1024px), Desktop (>1024px), Large Desktop (>1920px)
   - **Component Adaptations**: How each component responds across breakpoints
   - **Touch-Friendly Design**: 44×44px minimum touch targets
   - **Progressive Disclosure**: Show more features as screen size increases

9. **`interaction-design-guide.md`** (~23KB)

   - **Micro-Interactions**: Button press, checkbox, toggle, drag-and-drop
   - **Page Transitions**: Fade, slide, scale animations
   - **List Animations**: Stagger, reorder, delete with confirmation
   - **Gamification UI**: Points animation, confetti, achievement unlock
   - **Real-Time Updates**: Loading states, optimistic UI, error recovery
   - **Gesture Patterns**: Swipe to delete, pull to refresh, pinch to zoom

10. **`ISSUE-7-UI-DESIGN-SUMMARY.md`** (~15KB)

    - UI Design deliverables summary
    - Component library overview
    - Implementation priorities
    - Timeline and next steps

11. **`ISSUE-7-UX-RESEARCH-SUMMARY.md`** (~18KB)
    - UX Research deliverables summary
    - Key findings and recommendations
    - Persona highlights
    - Accessibility commitments
    - Event chain UX strategy
    - Competitive positioning

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
- **Cloud architecture**: Read `/docs/cloud-architecture.md` or `/docs/kubernetes-deployment-guide.md`
- **Infrastructure costs**: Read `/docs/infrastructure-cost-analysis.md`
- **Security architecture**: Read `/docs/threat-model.md` or `/docs/ISSUE-8-SECURITY-SUMMARY.md`
- **Security testing**: Read `/docs/security-testing-plan.md` (OWASP Top 10, SAST/DAST)
- **Vulnerability management**: Read `/docs/vulnerability-management.md`
- **Incident response**: Read `/docs/security-monitoring-incident-response.md`
- **Legal compliance**: Read `/docs/legal/LEGAL-COMPLIANCE-SUMMARY.md` (GDPR, COPPA, CCPA) (NEW)
- **Terms of Service**: Read `/docs/legal/terms-of-service.md` (NEW)
- **Privacy Policy**: Read `/docs/legal/privacy-policy.md` (NEW)
- **COPPA implementation**: Read `/docs/legal/quick-reference-coppa-workflow.md` (NEW)
- **Market strategy**: Read `/docs/market-research-report.md` or `/docs/ISSUE-9-MARKET-STRATEGY-SUMMARY.md`
- **Go-to-market plan**: Read `/docs/go-to-market-plan.md`
- **SEO strategy**: Read `/docs/seo-content-strategy.md`
- **UX research & personas**: Read `/docs/ux-research-report.md`
- **UI design system**: Read `/docs/design-system.md` or `/docs/angular-component-specs.md`
- **Wireframes**: Read `/docs/wireframes.md`
- **Accessibility**: Read `/docs/accessibility-strategy.md` (WCAG 2.1 AA + COPPA)
- **Event chain UX**: Read `/docs/event-chain-ux.md`
- **Implementation plan**: Read `/docs/implementation-roadmap.md`
- **Deployment**: Read `/docs/kubernetes-deployment-guide.md` and `/docs/cicd-pipeline.md`
- **Event chains**: Read `/docs/event-chains-reference.md`
- **Risks**: Read `/docs/risk-register.md`

**When implementing features**:

1. Check which **phase** the feature belongs to in `/docs/implementation-roadmap.md`
2. Find the feature in `/docs/FEATURE_BACKLOG.md` to understand priority and dependencies
3. Review **wireframes** in `/docs/wireframes.md` to understand UI layout
4. Check **design system** in `/docs/design-system.md` for components and styling
5. Review **personas** in `/docs/ux-research-report.md` to understand user needs
6. Verify **accessibility requirements** in `/docs/accessibility-strategy.md` (WCAG 2.1 AA)
7. Identify the **bounded context** (microservice) in `/docs/domain-model-microservices-map.md`
8. Review relevant **domain events** and **GraphQL schema** in the domain model doc
9. Check for any **event chain** integration in `/docs/event-chains-reference.md`
10. Review associated **risks** in `/docs/risk-register.md`

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
- ✅ Cloud architecture and Kubernetes deployment strategy (Issue #6)
- ✅ UX research and UI design system (Issue #7)
- ✅ Implementation roadmap (6 phases)
- ✅ Risk analysis (35 risks identified)
- ✅ Event chain specifications (10 workflows)
- ✅ 208 features prioritized
- ✅ 6 detailed personas
- ✅ Complete wireframes and design system
- ✅ WCAG 2.1 AA + COPPA compliance strategy

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
- [Cloud Architecture](docs/cloud-architecture.md) - Kubernetes deployment strategy
- [Implementation Roadmap](docs/implementation-roadmap.md) - Phase-by-phase plan
- [Feature Backlog](docs/FEATURE_BACKLOG.md) - All 208 features prioritized

### By Topic

**Product Strategy**:
- [PRODUCT_STRATEGY.md](docs/PRODUCT_STRATEGY.md)
- [FEATURE_BACKLOG.md](docs/FEATURE_BACKLOG.md)

**UX Research & UI Design**:
- [ux-research-report.md](docs/ux-research-report.md) - Personas & competitive analysis
- [information-architecture.md](docs/information-architecture.md) - Site map & navigation
- [wireframes.md](docs/wireframes.md) - Complete wireframes
- [design-system.md](docs/design-system.md) - Design system & components
- [angular-component-specs.md](docs/angular-component-specs.md) - Angular components
- [accessibility-strategy.md](docs/accessibility-strategy.md) - WCAG 2.1 AA + COPPA
- [event-chain-ux.md](docs/event-chain-ux.md) - Event chain UX patterns
- [responsive-design-guide.md](docs/responsive-design-guide.md) - Mobile-first design
- [interaction-design-guide.md](docs/interaction-design-guide.md) - Micro-interactions

**Cloud Architecture**:
- [cloud-architecture.md](docs/cloud-architecture.md)
- [kubernetes-deployment-guide.md](docs/kubernetes-deployment-guide.md)
- [infrastructure-cost-analysis.md](docs/infrastructure-cost-analysis.md)
- [cicd-pipeline.md](docs/cicd-pipeline.md)

**Event Chains**:
- [event-chains-reference.md](docs/event-chains-reference.md)
- [event-chain-ux.md](docs/event-chain-ux.md)

**Other**:
- [risk-register.md](docs/risk-register.md)
- [ROADMAP_VISUAL.md](docs/ROADMAP_VISUAL.md)
- [architecture-visual-summary.md](docs/architecture-visual-summary.md)
- [INDEX.md](docs/INDEX.md)

### GitHub Issues

- [Issue #7: UX Architecture & Design System](https://github.com/andrekirst/family2/issues/7)
- [Issue #6: Cloud Architecture & Kubernetes Deployment Strategy](https://github.com/andrekirst/family2/issues/6)
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

**Total**: 47 documents, 230,000+ words
**Location**: `/home/andrekirst/git/github/andrekirst/family2/docs/`
**Categories**:
- Product Strategy (5 docs)
- Technical Architecture (3 docs)
- Cloud & Kubernetes (8 docs)
- Security Architecture (5 docs - Issue #8)
- Legal Compliance (9 docs - Issue #10)
- Market Strategy (5 docs - Issue #9)
- UX Research & UI Design (11 docs)
- Supporting Documents (1 doc)

**Purpose**: Comprehensive planning and architecture for Family Hub
**Audience**: Product managers, developers, UX/UI designers, stakeholders, Claude Code

**Remember**: Always check `/docs/` folder first when you need context about Family Hub!

---

_This guide was created to help Claude Code navigate the Family Hub project efficiently. For the full context, always refer to the `/docs/` folder._
