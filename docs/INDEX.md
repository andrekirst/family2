# Family Hub - Documentation Index

**Last Updated:** 2026-01-12
**Total Documents:** 65 markdown files organized in 9 thematic folders (6 new ADRs added)
**Total Content:** 320,000+ words

---

## 🚀 Quick Start

**New to Family Hub?**
Start here: [Executive Summary](executive-summary.md) (15-minute overview)

**Developers:**
→ [Coding Standards](development/CODING_STANDARDS.md)
→ [Architecture Overview](architecture/ADR-001-MODULAR-MONOLITH-FIRST.md)
→ [Domain Model](architecture/domain-model-microservices-map.md)
→ [Implementation Roadmap](product-strategy/implementation-roadmap.md)

**Product/Business:**
→ [Product Strategy](product-strategy/PRODUCT_STRATEGY.md)
→ [Feature Backlog](product-strategy/FEATURE_BACKLOG.md) (208 features, RICE-scored)
→ [Market Research](market-business/market-research-report.md)

**Designers:**
→ [UX Research](ux-design/ux-research-report.md) (6 personas)
→ [Design System](ux-design/design-system.md) (22+ components)
→ [Wireframes](ux-design/wireframes.md) (all MVP screens)

---

## 📁 Folder Structure

```
/docs/
├── /architecture/          # Technical architecture & ADRs (13 docs)
├── /authentication/        # OAuth 2.0 & Zitadel guides (4 docs)
├── /development/           # Coding standards & workflows (5 docs)
├── /infrastructure/        # Cloud, K8s, CI/CD (6 docs)
├── /legal/                 # GDPR, COPPA, compliance (8 docs)
├── /market-business/       # Market research, GTM (5 docs)
├── /product-strategy/      # Vision, roadmap, features (5 docs)
├── /security/              # Threat model, testing (4 docs)
├── /ux-design/             # UX research, design system (9 docs)
├── executive-summary.md    # Start here!
├── INDEX.md                # This file
└── README.md               # Folder overview
```

---

## 📚 Complete Documentation Map

### 1. Architecture (`/architecture/` - 13 documents)

**Architecture Decisions (ADRs):**

- [ADR-001: Modular Monolith First](architecture/ADR-001-MODULAR-MONOLITH-FIRST.md) - Why not microservices from day one
- [ADR-002: OAuth with Zitadel](architecture/ADR-002-OAUTH-WITH-ZITADEL.md) - Why Zitadel vs Auth0/Keycloak
- [ADR-003: GraphQL Input/Command Pattern](architecture/ADR-003-GRAPHQL-INPUT-COMMAND-PATTERN.md) - Separation of presentation and domain concerns
- [ADR-004: Playwright Migration](architecture/ADR-004-PLAYWRIGHT-MIGRATION.md) - E2E testing framework choice
- [ADR-005: Family Module Extraction Pattern](architecture/ADR-005-FAMILY-MODULE-EXTRACTION-PATTERN.md) - Reusable bounded context extraction pattern
- [ADR-006: Email-Only Authentication](architecture/ADR-006-EMAIL-ONLY-AUTHENTICATION.md) - Authentication strategy
- [ADR-007: Family DbContext Separation Strategy](architecture/ADR-007-FAMILY-DBCONTEXT-SEPARATION-STRATEGY.md) - One DbContext per module with schema separation
- [ADR-008: RabbitMQ Integration Strategy](architecture/ADR-008-RABBITMQ-INTEGRATION-STRATEGY.md) - Message broker with Polly resilience
- [ADR-009: Modular Middleware Composition Pattern](architecture/ADR-009-MODULAR-MIDDLEWARE-COMPOSITION-PATTERN.md) - UseAuthModule/UseFamilyModule pattern
- [ADR-010: Performance Testing Strategy](architecture/ADR-010-PERFORMANCE-TESTING-STRATEGY.md) - k6-based performance testing
- [ADR-011: DataLoader Pattern](architecture/ADR-011-DATALOADER-PATTERN.md) - Hot Chocolate DataLoader for N+1 prevention
- [ADR-012: Architecture Testing Strategy](architecture/ADR-012-ARCHITECTURE-TESTING-STRATEGY.md) - NetArchTest for architecture validation
- [Architecture Review Report](architecture/ARCHITECTURE-REVIEW-REPORT.md) - Comprehensive architecture review

**Domain-Driven Design:**

- [Domain Model & Microservices Map](architecture/domain-model-microservices-map.md) - 8 DDD modules, domain events, GraphQL schemas
- [Event Chains Reference](architecture/event-chains-reference.md) - 10 automated workflows
- [Architecture Visual Summary](architecture/architecture-visual-summary.md) - ASCII system diagrams
- [Multi-Tenancy Strategy](architecture/multi-tenancy-strategy.md) - PostgreSQL Row-Level Security

---

### 2. Authentication (`/authentication/` - 4 documents)

**OAuth 2.0 Integration:**

- [OAuth Integration Guide](authentication/OAUTH_INTEGRATION_GUIDE.md) - Complete guide (331 lines): PKCE flow, JWT validation, troubleshooting
- [Zitadel Setup Guide](authentication/ZITADEL-SETUP-GUIDE.md) - Local dev setup instructions
- [OAuth Completion Summary](authentication/ZITADEL-OAUTH-COMPLETION-SUMMARY.md) - Implementation summary
- [OAuth Security Checklist](authentication/OAUTH-FINAL-REVIEW-CHECKLIST.md) - OWASP compliance (80%)

---

### 3. Development (`/development/` - 10 documents)

**Coding Standards & Patterns:**

- [Coding Standards](development/CODING_STANDARDS.md) - Comprehensive coding standards (C#, TypeScript, DDD, GraphQL, Testing)
- [DDD Patterns](development/PATTERNS.md) - Domain-Driven Design patterns and examples
- [Development Workflows](development/WORKFLOWS.md) - Database migrations, value objects, testing, GraphQL

**Implementation Process:**

- [Implementation Workflow](development/IMPLEMENTATION_WORKFLOW.md) - Standard feature implementation process
- [Post-Tool-Use Hooks](development/HOOKS.md) - Automatic code formatting and quality checks

**Setup & Testing:**

- [Local Development Setup](development/LOCAL_DEVELOPMENT_SETUP.md) - Complete local dev environment setup guide
- [Testing with Playwright](development/TESTING_WITH_PLAYWRIGHT.md) - E2E testing guide with API-first approach

**Guides:**

- [Debugging Guide](development/DEBUGGING_GUIDE.md) - Comprehensive troubleshooting reference
- [Module Extraction Quickstart](development/MODULE_EXTRACTION_QUICKSTART.md) - Bounded context extraction guide
- [Claude Code Guide](development/CLAUDE_CODE_GUIDE.md) - AI-assisted development workflow

---

### 4. Infrastructure (`/infrastructure/` - 6 documents)

**Cloud & Kubernetes (Phase 5+):**

- [Cloud Architecture](infrastructure/cloud-architecture.md) - Kubernetes architecture overview
- [Kubernetes Deployment Guide](infrastructure/kubernetes-deployment-guide.md) - Local & cloud deployment
- [Helm Charts Structure](infrastructure/helm-charts-structure.md) - Helm chart templates

**DevOps & Observability:**

- [CI/CD Pipeline](infrastructure/cicd-pipeline.md) - GitHub Actions + ArgoCD
- [Observability Stack](infrastructure/observability-stack.md) - Prometheus + Grafana + Loki
- [Infrastructure Cost Analysis](infrastructure/infrastructure-cost-analysis.md) - Cost projections ($200-5K/month)

---

### 5. Legal (`/legal/` - 8 documents)

**Compliance:**

- [Legal Compliance Summary](legal/LEGAL-COMPLIANCE-SUMMARY.md) - GDPR, COPPA, CCPA overview
- [Compliance Checklist](legal/compliance-checklist.md) - 93-item compliance checklist
- [COPPA Workflow](legal/quick-reference-coppa-workflow.md) - Child protection implementation

**Policies:**

- [Privacy Policy](legal/privacy-policy.md) - GDPR/COPPA/CCPA compliant
- [Terms of Service](legal/terms-of-service.md) - User agreement
- [Cookie Policy](legal/cookie-policy.md) - Cookie disclosure
- [Data Processing Agreement Template](legal/data-processing-agreement-template.md) - B2B DPA

**Reference:**

- [README](legal/README.md) - Legal docs quick start

---

### 6. Market & Business (`/market-business/` - 5 documents)

**Market Research:**

- [Market Research Report](market-business/market-research-report.md) - Competitive analysis (2,700+ app reviews)
- [Competitive Analysis](market-business/competitive-analysis.md) - Competitor SWOT analysis

**Go-to-Market:**

- [Go-to-Market Plan](market-business/go-to-market-plan.md) - Channels, pricing, launch strategy
- [Brand Positioning](market-business/brand-positioning.md) - Brand guidelines, messaging
- [SEO & Content Strategy](market-business/seo-content-strategy.md) - SEO plan, content calendar

---

### 7. Product Strategy (`/product-strategy/` - 5 documents)

**Vision & Strategy:**

- [Product Strategy](product-strategy/PRODUCT_STRATEGY.md) - Vision, personas, strategic pillars, positioning

**Features & Roadmap:**

- [Feature Backlog](product-strategy/FEATURE_BACKLOG.md) - 208 features (RICE scored)
- [Implementation Roadmap](product-strategy/implementation-roadmap.md) - 6-phase plan (Phase 0-6, 10-14 months)
- [Roadmap Visual](product-strategy/ROADMAP_VISUAL.md) - ASCII Gantt charts, visual timeline

**Risk Management:**

- [Risk Register](product-strategy/risk-register.md) - 35 risks with mitigation strategies

---

### 8. Security (`/security/` - 4 documents)

**Threat Modeling:**

- [Threat Model](security/threat-model.md) - STRIDE analysis (53 threats)

**Testing & Vulnerability Management:**

- [Security Testing Plan](security/security-testing-plan.md) - OWASP Top 10, SAST/DAST
- [Vulnerability Management](security/vulnerability-management.md) - Severity levels, remediation SLAs

**Monitoring & Incident Response:**

- [Security Monitoring & Incident Response](security/security-monitoring-incident-response.md) - Monitoring, incident playbooks

---

### 9. UX & Design (`/ux-design/` - 9 documents)

**Research:**

- [UX Research Report](ux-design/ux-research-report.md) - 6 personas, user journeys (2,700+ app reviews analyzed)

**Design System:**

- [Design System](ux-design/design-system.md) - 22+ components (buttons, inputs, cards, etc.)
- [Wireframes](ux-design/wireframes.md) - Complete MVP wireframes (all screens)
- [Angular Component Specs](ux-design/angular-component-specs.md) - Angular v21 component specifications

**Information Architecture:**

- [Information Architecture](ux-design/information-architecture.md) - Site map, navigation structure

**Accessibility & Responsive:**

- [Accessibility Strategy](ux-design/accessibility-strategy.md) - WCAG 2.1 AA + COPPA compliance
- [Responsive Design Guide](ux-design/responsive-design-guide.md) - Mobile-first responsive design

**Interactions:**

- [Event Chain UX](ux-design/event-chain-ux.md) - Event chain UX patterns
- [Interaction Design Guide](ux-design/interaction-design-guide.md) - Micro-interactions, animations

---

### 10. Root Navigation (`/` - 3 documents)

- [Executive Summary](executive-summary.md) - 15-minute overview (START HERE!)
- [INDEX.md](INDEX.md) - This file
- [README.md](README.md) - Docs folder overview

---

## 🔍 Find Documentation By Topic

### Authentication & Security

- OAuth 2.0: [OAuth Integration Guide](authentication/OAUTH_INTEGRATION_GUIDE.md)
- Zitadel Setup: [Zitadel Setup Guide](authentication/ZITADEL-SETUP-GUIDE.md)
- Security: [Threat Model](security/threat-model.md), [Security Testing](security/security-testing-plan.md)

### Architecture & Design

- Modular Monolith: [ADR-001](architecture/ADR-001-MODULAR-MONOLITH-FIRST.md)
- Domain Model: [Domain Model Map](architecture/domain-model-microservices-map.md)
- Event Chains: [Event Chains Reference](architecture/event-chains-reference.md)

### Development & Deployment

- Coding Standards: [Coding Standards](development/CODING_STANDARDS.md)
- Workflows: [Development Workflows](development/WORKFLOWS.md), [DDD Patterns](development/PATTERNS.md)
- Roadmap: [Implementation Roadmap](product-strategy/implementation-roadmap.md)
- Features: [Feature Backlog](product-strategy/FEATURE_BACKLOG.md)
- Infrastructure: [Cloud Architecture](infrastructure/cloud-architecture.md), [K8s Deployment](infrastructure/kubernetes-deployment-guide.md)

### User Experience

- Personas: [UX Research Report](ux-design/ux-research-report.md)
- Design: [Design System](ux-design/design-system.md), [Wireframes](ux-design/wireframes.md)
- Accessibility: [Accessibility Strategy](ux-design/accessibility-strategy.md)

### Business & Legal

- Product: [Product Strategy](product-strategy/PRODUCT_STRATEGY.md)
- Market: [Market Research](market-business/market-research-report.md), [GTM Plan](market-business/go-to-market-plan.md)
- Compliance: [Legal Compliance](legal/LEGAL-COMPLIANCE-SUMMARY.md), [Privacy Policy](legal/privacy-policy.md)

---

## ❓ Frequently Asked Questions

### Where do I start?

→ [Executive Summary](executive-summary.md) for a 15-minute overview

### What's the current development phase?

→ Phase 0: Foundation & Tooling (3 weeks) - See [Implementation Roadmap](product-strategy/implementation-roadmap.md)

### What features are planned?

→ 208 features in [Feature Backlog](product-strategy/FEATURE_BACKLOG.md) (RICE-scored)

### How does authentication work?

→ OAuth 2.0 with Zitadel - See [OAuth Integration Guide](authentication/OAUTH_INTEGRATION_GUIDE.md)

### What's the architecture?

→ Modular Monolith (Phase 1-4) → Microservices (Phase 5+) - See [ADR-001](architecture/ADR-001-MODULAR-MONOLITH-FIRST.md)

### Is this GDPR compliant?

→ Yes - See [Legal Compliance Summary](legal/LEGAL-COMPLIANCE-SUMMARY.md) and [Privacy Policy](legal/privacy-policy.md)

### What's an event chain?

→ Automated cross-domain workflows - See [Event Chains Reference](architecture/event-chains-reference.md)

### How do I deploy to Kubernetes?

→ See [Kubernetes Deployment Guide](infrastructure/kubernetes-deployment-guide.md)

---

## 📊 Documentation Statistics

- **Total Documents:** 65 markdown files
- **Total Words:** 280,000+
- **Total Lines:** ~15,000+
- **Folders:** 9 thematic categories
- **Diagrams:** 20+ ASCII diagrams
- **Code Examples:** 150+ snippets

**Breakdown by Category:**

- UX & Design: 9 docs
- Legal: 8 docs
- Architecture: 13 docs
- Infrastructure: 6 docs
- Development: 5 docs
- Product Strategy: 5 docs
- Market & Business: 5 docs
- Authentication: 4 docs
- Security: 4 docs
- Navigation: 3 docs

---

## 🔗 External Resources

**GitHub:**

- [Repository](https://github.com/andrekirst/family2)
- [Issues](https://github.com/andrekirst/family2/issues)
- [Pull Requests](https://github.com/andrekirst/family2/pulls)

**Technology Documentation:**

- [.NET Core 10](https://learn.microsoft.com/en-us/dotnet/core/)
- [Angular v21](https://angular.dev/)
- [Hot Chocolate GraphQL](https://chillicream.com/docs/hotchocolate)
- [PostgreSQL 16](https://www.postgresql.org/docs/16/)
- [Zitadel](https://zitadel.com/docs)
- [RabbitMQ](https://www.rabbitmq.com/documentation.html)

---

## 📝 Recent Updates

### 2026-01-12

- **New ADRs:** Created 6 new Architecture Decision Records (Issue #76):
  - [ADR-007: Family DbContext Separation Strategy](architecture/ADR-007-FAMILY-DBCONTEXT-SEPARATION-STRATEGY.md) - One DbContext per module with PostgreSQL schema separation
  - [ADR-008: RabbitMQ Integration Strategy](architecture/ADR-008-RABBITMQ-INTEGRATION-STRATEGY.md) - IMessageBrokerPublisher with Polly v8 resilience
  - [ADR-009: Modular Middleware Composition Pattern](architecture/ADR-009-MODULAR-MIDDLEWARE-COMPOSITION-PATTERN.md) - UseAuthModule/UseFamilyModule extension methods
  - [ADR-010: Performance Testing Strategy](architecture/ADR-010-PERFORMANCE-TESTING-STRATEGY.md) - k6-based performance testing with DataLoader benchmarks
  - [ADR-011: DataLoader Pattern](architecture/ADR-011-DATALOADER-PATTERN.md) - Hot Chocolate GreenDonut DataLoaders for N+1 prevention
  - [ADR-012: Architecture Testing Strategy](architecture/ADR-012-ARCHITECTURE-TESTING-STRATEGY.md) - NetArchTest with ExceptionRegistry pattern

### 2026-01-09

- **New ADR:** Created [ADR-005: Family Module Extraction Pattern](architecture/ADR-005-FAMILY-MODULE-EXTRACTION-PATTERN.md)
  - Comprehensive 4-phase extraction process (Domain → Application → Persistence → Presentation)
  - Logical vs physical separation strategy for pragmatic modular monolith
  - Reusable template for extracting 7 remaining modules (Calendar, Task, Shopping, Health, Meal Planning, Finance, Communication)
  - Detailed coupling point documentation with Phase 5+ resolution plans
  - 5 educational insight boxes explaining DDD principles
  - Code examples and validation criteria
  - Architecture score improvement: 65 → 90 (+25 points)
  - DDD compliance: 70 → 95 (+25 points)

### 2026-01-04

- **Feature Split:** "Family Member Invites" split into two separate backlog items:
  - **Family Member Invites (Wizard):** Optional Step 2 in family creation wizard for member invitations
  - **Family Member Invites (Management):** Ongoing member invitation from family management UI
- **New Document:** Created [CHILD-ACCOUNT-SETUP.md](authentication/CHILD-ACCOUNT-SETUP.md) documenting child account creation flow
  - Username/password authentication for children without email addresses
  - Zitadel Management API integration
  - Synthetic email pattern and security considerations
- **Updated Documentation:**
  - [FEATURE_BACKLOG.md](product-strategy/FEATURE_BACKLOG.md) - Split invitation features, updated effort totals
  - [domain-model-microservices-map.md](architecture/domain-model-microservices-map.md) - Added FamilyMemberInvitation aggregate and domain events
  - [event-chains-reference.md](architecture/event-chains-reference.md) - Added Event Chain #11: Family Member Invitation Event Chain
  - [wireframes.md](ux-design/wireframes.md) - Enhanced wizard Step 2, added password display modal and family management UI
  - [implementation-roadmap.md](product-strategy/implementation-roadmap.md) - Updated Phase 1 deliverables (Week 5-7)

### 2025-12-23

- Reorganized documentation into 9 thematic folders (51 documents)
- Version 2.0 structure for improved navigation and discoverability

---

_Last updated: 2026-01-12_
_Version: 2.3 (Added 6 new ADRs: ADR-007 through ADR-012)_
