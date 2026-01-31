# Family Hub - Documentation Index

> **⚠️ PROJECT RESTART (February 2026):** All implementation code has been removed. This documentation index now reflects a strategic foundation template. See README.md for full context. Previous implementation preserved in Git tag `v0.1-phase0-archive`.

**Last Updated:** 2026-02-01
**Status:** Strategic Foundation Template
**Total Documents:** ~45-50 markdown files organized in 6 thematic folders
**Total Content:** 280,000+ words of strategic planning, architecture, and domain modeling

---

## 🚀 Quick Start

**New to Family Hub?**
Start here: [Executive Summary](executive-summary.md) (15-minute overview)

**Developers (Planning):**
→ [Coding Standards](development/CODING_STANDARDS.md)
→ [Architecture Overview](architecture/ADR-001-MODULAR-MONOLITH-FIRST.md)
→ [Domain Model](architecture/domain-model-microservices-map.md)
→ [Backend Development Guide](guides/BACKEND_DEVELOPMENT.md)

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
├── /architecture/          # Technical architecture & ADRs (7 docs)
├── /development/           # Coding standards & patterns (6 docs)
├── /guides/                # Domain-specific development guides (4 docs)
├── /market-business/       # Market research, GTM (5 docs)
├── /product-strategy/      # Vision, roadmap, features (5 docs)
├── /security/              # Threat model, OWASP (4 docs)
├── /ux-design/             # UX research, design system (9 docs)
├── /agent-os/              # Spec-driven development profiles
├── executive-summary.md    # Start here!
├── INDEX.md                # This file
├── README.md               # Folder overview
└── CLAUDE.md               # Documentation navigation guide
```

---

## 📚 Complete Documentation Map

### 1. Architecture (`/architecture/` - 7 documents)

**Core Architecture Decisions (ADRs):**

- [ADR-001: Modular Monolith First](architecture/ADR-001-MODULAR-MONOLITH-FIRST.md) - Why not microservices from day one
- [ADR-002: OAuth with Zitadel](architecture/ADR-002-OAUTH-WITH-ZITADEL.md) - Why Zitadel vs Auth0/Keycloak
- [ADR-003: GraphQL Input/Command Pattern](architecture/ADR-003-GRAPHQL-INPUT-COMMAND-PATTERN.md) - Separation of presentation and domain concerns
- [ADR-005: Family Module Extraction Pattern](architecture/ADR-005-FAMILY-MODULE-EXTRACTION-PATTERN.md) - Reusable bounded context extraction pattern

**Domain-Driven Design:**

- [Domain Model & Microservices Map](architecture/domain-model-microservices-map.md) - 8 DDD modules, domain events, GraphQL schemas
- [Event Chains Reference](architecture/event-chains-reference.md) - 10 automated workflows (flagship feature)
- [Architecture Visual Summary](architecture/architecture-visual-summary.md) - ASCII system diagrams

---

### 2. Development (`/development/` - 6 documents)

**Coding Standards & Patterns:**

- [Coding Standards](development/CODING_STANDARDS.md) - Comprehensive coding standards (C#, TypeScript, DDD, GraphQL, Testing)
- [DDD Patterns](development/PATTERNS.md) - Domain-Driven Design patterns and examples
- [Development Workflows](development/WORKFLOWS.md) - Database migrations, value objects, testing, GraphQL

**Guides:**

- [Module Extraction Quickstart](development/MODULE_EXTRACTION_QUICKSTART.md) - Bounded context extraction guide
- [Claude Code Guide](development/CLAUDE_CODE_GUIDE.md) - AI-assisted development workflow
- [API Standards](development/API_STANDARDS.md) - GraphQL and REST API standards

---

### 3. Development Guides (`/guides/` - 4 documents)

**Domain-Specific Guides:**

- [Backend Development](guides/BACKEND_DEVELOPMENT.md) - .NET, C#, GraphQL, DDD patterns
- [Frontend Development](guides/FRONTEND_DEVELOPMENT.md) - Angular, TypeScript, component architecture
- [Database Development](guides/DATABASE_DEVELOPMENT.md) - PostgreSQL, migrations, schema design
- [Infrastructure Development](guides/INFRASTRUCTURE_DEVELOPMENT.md) - Docker, Kubernetes, CI/CD

---

### 4. Market & Business (`/market-business/` - 5 documents)

**Market Research:**

- [Market Research Report](market-business/market-research-report.md) - Competitive analysis (2,700+ app reviews)
- [Competitive Analysis](market-business/competitive-analysis.md) - Competitor SWOT analysis

**Go-to-Market:**

- [Go-to-Market Plan](market-business/go-to-market-plan.md) - Channels, pricing, launch strategy
- [Brand Positioning](market-business/brand-positioning.md) - Brand guidelines, messaging
- [SEO & Content Strategy](market-business/seo-content-strategy.md) - SEO plan, content calendar

---

### 5. Product Strategy (`/product-strategy/` - 5 documents)

**Vision & Strategy:**

- [Product Strategy](product-strategy/PRODUCT_STRATEGY.md) - Vision, personas, strategic pillars, positioning

**Features & Roadmap:**

- [Feature Backlog](product-strategy/FEATURE_BACKLOG.md) - 208 features (RICE scored)
- [Implementation Roadmap](product-strategy/implementation-roadmap.md) - 6-phase plan (10-14 months)
- [Roadmap Visual](product-strategy/ROADMAP_VISUAL.md) - ASCII Gantt charts, visual timeline

**Risk Management:**

- [Risk Register](product-strategy/risk-register.md) - 35 risks with mitigation strategies

---

### 6. Security (`/security/` - 4 documents)

**Threat Modeling:**

- [Threat Model](security/threat-model.md) - STRIDE analysis (53 threats)

**Testing & Vulnerability Management:**

- [Security Testing Plan](security/security-testing-plan.md) - OWASP Top 10, SAST/DAST
- [Vulnerability Management](security/vulnerability-management.md) - Severity levels, remediation SLAs

**Monitoring & Incident Response:**

- [Security Monitoring & Incident Response](security/security-monitoring-incident-response.md) - Monitoring, incident playbooks

---

### 7. UX & Design (`/ux-design/` - 9 documents)

**Research:**

- [UX Research Report](ux-design/ux-research-report.md) - 6 personas, user journeys (2,700+ app reviews analyzed)

**Design System:**

- [Design System](ux-design/design-system.md) - 22+ components (buttons, inputs, cards, etc.)
- [Wireframes](ux-design/wireframes.md) - Complete MVP wireframes (all screens)
- [Angular Component Specs](ux-design/angular-component-specs.md) - Angular component specifications

**Information Architecture:**

- [Information Architecture](ux-design/information-architecture.md) - Site map, navigation structure

**Accessibility & Responsive:**

- [Accessibility Strategy](ux-design/accessibility-strategy.md) - WCAG 2.1 AA + COPPA compliance
- [Responsive Design Guide](ux-design/responsive-design-guide.md) - Mobile-first responsive design

**Interactions:**

- [Event Chain UX](ux-design/event-chain-ux.md) - Event chain UX patterns
- [Interaction Design Guide](ux-design/interaction-design-guide.md) - Micro-interactions, animations

---

### 8. Agent OS (`/agent-os/`)

**Spec-Driven Development Infrastructure:**

- **Profiles:** `agent-os/profiles/` - 8 module profiles + 5 layer profiles
- **Standards:** `agent-os/standards/` - Extracted architectural patterns
- **Specs:** `agent-os/specs/` - Machine-readable feature specifications

**See:** [Agent OS Overview](../agent-os/) for complete documentation

---

### 9. Root Navigation (`/` - 4 documents)

- [Executive Summary](executive-summary.md) - 15-minute overview (START HERE!)
- [INDEX.md](INDEX.md) - This file
- [README.md](README.md) - Docs folder overview
- [CLAUDE.md](CLAUDE.md) - Documentation navigation guide

---

## 🔍 Find Documentation By Topic

### Architecture & Design

- Modular Monolith: [ADR-001](architecture/ADR-001-MODULAR-MONOLITH-FIRST.md)
- Domain Model: [Domain Model Map](architecture/domain-model-microservices-map.md)
- Event Chains: [Event Chains Reference](architecture/event-chains-reference.md)
- Module Extraction: [ADR-005](architecture/ADR-005-FAMILY-MODULE-EXTRACTION-PATTERN.md)

### Development Planning

- Coding Standards: [Coding Standards](development/CODING_STANDARDS.md)
- DDD Patterns: [DDD Patterns](development/PATTERNS.md)
- Workflows: [Development Workflows](development/WORKFLOWS.md)
- Backend Guide: [Backend Development](guides/BACKEND_DEVELOPMENT.md)
- Frontend Guide: [Frontend Development](guides/FRONTEND_DEVELOPMENT.md)
- Database Guide: [Database Development](guides/DATABASE_DEVELOPMENT.md)

### Product & Strategy

- Vision: [Product Strategy](product-strategy/PRODUCT_STRATEGY.md)
- Features: [Feature Backlog](product-strategy/FEATURE_BACKLOG.md)
- Roadmap: [Implementation Roadmap](product-strategy/implementation-roadmap.md)
- Risks: [Risk Register](product-strategy/risk-register.md)

### User Experience

- Personas: [UX Research Report](ux-design/ux-research-report.md)
- Design: [Design System](ux-design/design-system.md), [Wireframes](ux-design/wireframes.md)
- Accessibility: [Accessibility Strategy](ux-design/accessibility-strategy.md)

### Business & Market

- Product: [Product Strategy](product-strategy/PRODUCT_STRATEGY.md)
- Market: [Market Research](market-business/market-research-report.md)
- GTM: [Go-to-Market Plan](market-business/go-to-market-plan.md)

### Security

- Threats: [Threat Model](security/threat-model.md)
- Testing: [Security Testing Plan](security/security-testing-plan.md)
- Monitoring: [Security Monitoring](security/security-monitoring-incident-response.md)

---

## ❓ Frequently Asked Questions

### Where do I start?

→ [Executive Summary](executive-summary.md) for a 15-minute overview

### What's the current status?

→ Strategic foundation template (post-restart). See [README.md](../README.md) for details.

### What features are planned?

→ 208 features in [Feature Backlog](product-strategy/FEATURE_BACKLOG.md) (RICE-scored)

### What's the architecture strategy?

→ Modular Monolith First → Eventual Microservices - See [ADR-001](architecture/ADR-001-MODULAR-MONOLITH-FIRST.md)

### What's an event chain?

→ Automated cross-domain workflows (flagship feature) - See [Event Chains Reference](architecture/event-chains-reference.md)

### What happened to the previous implementation?

→ Preserved in Git tag [`v0.1-phase0-archive`](https://github.com/andrekirst/family2/tree/v0.1-phase0-archive)

---

## 📊 Documentation Statistics

**Strategic Foundation:**

- **Total Documents:** ~45-50 markdown files
- **Total Words:** 280,000+
- **Folders:** 6 thematic categories + Agent OS
- **Diagrams:** 20+ ASCII diagrams
- **Code Examples:** Conceptual patterns and examples

**Breakdown by Category:**

- UX & Design: 9 docs
- Architecture: 7 docs
- Development: 6 docs
- Product Strategy: 5 docs
- Market & Business: 5 docs
- Development Guides: 4 docs
- Security: 4 docs
- Navigation: 4 docs

**What's NOT Included (Removed During Restart):**

- Implementation-specific guides (local setup, debugging, testing)
- Authentication/OAuth implementation docs
- Legal/compliance docs (privacy policy, terms of service)
- Infrastructure deployment guides
- Database migration guides

---

## 🔗 External Resources

**GitHub:**

- [Repository](https://github.com/andrekirst/family2)
- [Issues](https://github.com/andrekirst/family2/issues)
- [Archive Tag (v0.1-phase0-archive)](https://github.com/andrekirst/family2/tree/v0.1-phase0-archive)

**Technology Documentation (Planned):**

- [.NET Core](https://learn.microsoft.com/en-us/dotnet/core/)
- [Angular](https://angular.dev/)
- [Hot Chocolate GraphQL](https://chillicream.com/docs/hotchocolate)
- [PostgreSQL](https://www.postgresql.org/docs/)

---

## 📝 Project History

### 2026-02-01 - Project Restart

**Major Change:** Architectural restart - all implementation code removed

**What Was Removed:**

- All backend code (src/api/)
- All frontend code (src/frontend/)
- All database schemas and migrations
- All infrastructure configurations
- Implementation-specific documentation (38 files)
- Build artifacts and test suites

**What Was Preserved:**

- Strategic documentation (product strategy, market research, UX design)
- Core architecture ADRs (5 foundational decisions)
- Domain models and event chains
- Development patterns and standards
- Agent OS infrastructure (profiles, standards, specs)
- All CLAUDE.md development guides (moved to docs/guides/)

**Reason:** Architecture needed redesign. Strategic foundation preserved for future implementation.

**Previous Implementation:** Preserved in Git tag `v0.1-phase0-archive`

### 2026-01-12 - Pre-Restart Peak

- 65 documentation files across 11 folders
- 6 new ADRs created (ADR-007 through ADR-012)
- Complete implementation with .NET backend, Angular frontend
- OAuth integration completed
- Family member invites completed (#97)

---

_Last updated: 2026-02-01_
_Version: 3.0.0 (Strategic Foundation - Post Restart)_
