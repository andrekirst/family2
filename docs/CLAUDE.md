# Documentation Navigation Guide

**Purpose:** Central navigation hub for Family Hub's 54 documentation files organized across 9 thematic folders.

**Total Content:** 310,000+ words of planning, architecture, and implementation documentation.

---

## Quick Start

**New to Family Hub?**
Start here: [Executive Summary](executive-summary.md) (15-minute overview)

**Developers:**

- [Coding Standards](development/CODING_STANDARDS.md)
- [Implementation Workflow](development/IMPLEMENTATION_WORKFLOW.md)
- [Architecture Overview](architecture/ADR-001-MODULAR-MONOLITH-FIRST.md)

**Product/Business:**

- [Product Strategy](product-strategy/PRODUCT_STRATEGY.md)
- [Feature Backlog](product-strategy/FEATURE_BACKLOG.md) (208 features, RICE-scored)
- [Market Research](market-business/market-research-report.md)

---

## Documentation Structure (9 Folders)

### Development Documentation

→ **[docs/development/CLAUDE.md](development/CLAUDE.md)** - Development patterns, coding standards, workflows

**Key Documents:**

- [CODING_STANDARDS.md](development/CODING_STANDARDS.md) - Comprehensive coding standards
- [PATTERNS.md](development/PATTERNS.md) - DDD patterns and examples
- [WORKFLOWS.md](development/WORKFLOWS.md) - Database, testing, GraphQL workflows
- [LOCAL_DEVELOPMENT_SETUP.md](development/LOCAL_DEVELOPMENT_SETUP.md) - Setup guide
- [TESTING_WITH_PLAYWRIGHT.md](development/TESTING_WITH_PLAYWRIGHT.md) - E2E testing
- [DEBUGGING_GUIDE.md](development/DEBUGGING_GUIDE.md) - Troubleshooting
- [MODULE_EXTRACTION_QUICKSTART.md](development/MODULE_EXTRACTION_QUICKSTART.md) - Bounded context extraction

### Architecture Documentation

→ **[docs/architecture/CLAUDE.md](architecture/CLAUDE.md)** - ADRs, domain model, event chains

**Key Documents:**

- [ADR-001: Modular Monolith First](architecture/ADR-001-MODULAR-MONOLITH-FIRST.md)
- [ADR-002: OAuth with Zitadel](architecture/ADR-002-OAUTH-WITH-ZITADEL.md)
- [ADR-003: GraphQL Input/Command Pattern](architecture/ADR-003-GRAPHQL-INPUT-COMMAND-PATTERN.md)
- [ADR-004: Playwright Migration](architecture/ADR-004-PLAYWRIGHT-MIGRATION.md)
- [ADR-005: Family Module Extraction](architecture/ADR-005-FAMILY-MODULE-EXTRACTION-PATTERN.md)
- [ADR-006: Email-Only Authentication](architecture/ADR-006-EMAIL-ONLY-AUTHENTICATION.md)
- [Domain Model & Microservices Map](architecture/domain-model-microservices-map.md)
- [Event Chains Reference](architecture/event-chains-reference.md)

### Security Documentation

→ **[docs/security/CLAUDE.md](security/CLAUDE.md)** - Threat model, OWASP testing, RLS patterns

**Key Documents:**

- [Threat Model](security/threat-model.md) - STRIDE analysis
- [Security Testing Plan](security/security-testing-plan.md) - OWASP Top 10
- [Vulnerability Management](security/vulnerability-management.md)
- [Security Monitoring & Incident Response](security/security-monitoring-incident-response.md)

### Product Strategy

**Key Documents:**

- [Product Strategy](product-strategy/PRODUCT_STRATEGY.md) - Vision, mission, competitive advantage
- [Feature Backlog](product-strategy/FEATURE_BACKLOG.md) - 208 features with RICE scores
- [Implementation Roadmap](product-strategy/implementation-roadmap.md) - 6 phases, 10-14 months
- [Risk Register](product-strategy/risk-register.md) - Technical and business risks
- [Assumptions Log](product-strategy/assumptions-log.md) - Critical assumptions

### UX Design

**Key Documents:**

- [UX Research Report](ux-design/ux-research-report.md) - 6 user personas
- [Design System](ux-design/design-system.md) - 22+ components
- [Wireframes](ux-design/wireframes.md) - All MVP screens
- [Component Specifications](ux-design/component-specifications.md)
- [Information Architecture](ux-design/information-architecture.md)

### Authentication

**Key Documents:**

- [OAuth Integration Guide](authentication/OAUTH_INTEGRATION_GUIDE.md) - Complete PKCE implementation
- [Zitadel Setup Guide](authentication/ZITADEL-SETUP-GUIDE.md)
- [OAuth Security Checklist](authentication/OAUTH-FINAL-REVIEW-CHECKLIST.md)

### Infrastructure

**Key Documents:**

- [Cloud Architecture](infrastructure/cloud-architecture.md) - Kubernetes architecture
- [Kubernetes Deployment Guide](infrastructure/kubernetes-deployment-guide.md)
- [CI/CD Pipeline](infrastructure/cicd-pipeline.md) - GitHub Actions + ArgoCD
- [Observability Stack](infrastructure/observability-stack.md) - Prometheus + Grafana
- [Infrastructure Cost Analysis](infrastructure/infrastructure-cost-analysis.md)

### Legal & Compliance

**Key Documents:**

- [Legal Compliance Summary](legal/LEGAL-COMPLIANCE-SUMMARY.md) - GDPR, COPPA, CCPA
- [Privacy Policy](legal/privacy-policy.md)
- [Terms of Service](legal/terms-of-service.md)
- [Compliance Checklist](legal/compliance-checklist.md) - 93 items

### Market & Business

**Key Documents:**

- [Market Research Report](market-business/market-research-report.md) - 2,700+ app reviews
- [Competitive Analysis](market-business/competitive-analysis.md)
- [Go-to-Market Plan](market-business/go-to-market-plan.md)
- [SEO & Content Strategy](market-business/seo-content-strategy.md)

---

## Documentation by Role

### For Developers

**Must-Read:**

1. [CODING_STANDARDS.md](development/CODING_STANDARDS.md) - Code quality requirements
2. [IMPLEMENTATION_WORKFLOW.md](development/IMPLEMENTATION_WORKFLOW.md) - Feature development process
3. [Domain Model](architecture/domain-model-microservices-map.md) - 8 DDD modules
4. [Event Chains](architecture/event-chains-reference.md) - Automated workflows

**Reference:**

- [PATTERNS.md](development/PATTERNS.md) - DDD patterns
- [WORKFLOWS.md](development/WORKFLOWS.md) - Database, testing, GraphQL
- [ADRs](architecture/) - All architectural decisions

### For Product Managers

**Strategic:**

1. [Product Strategy](product-strategy/PRODUCT_STRATEGY.md)
2. [Feature Backlog](product-strategy/FEATURE_BACKLOG.md)
3. [Implementation Roadmap](product-strategy/implementation-roadmap.md)
4. [Risk Register](product-strategy/risk-register.md)

**Market:**

- [Market Research](market-business/market-research-report.md)
- [Competitive Analysis](market-business/competitive-analysis.md)
- [Go-to-Market Plan](market-business/go-to-market-plan.md)

### For Designers

**UX:**

1. [UX Research Report](ux-design/ux-research-report.md) - User personas
2. [Design System](ux-design/design-system.md) - Component library
3. [Wireframes](ux-design/wireframes.md) - Screen layouts
4. [Information Architecture](ux-design/information-architecture.md)

### For DevOps Engineers

**Infrastructure:**

1. [Cloud Architecture](infrastructure/cloud-architecture.md)
2. [Kubernetes Deployment](infrastructure/kubernetes-deployment-guide.md)
3. [CI/CD Pipeline](infrastructure/cicd-pipeline.md)
4. [Observability Stack](infrastructure/observability-stack.md)

**Security:**

- [Threat Model](security/threat-model.md)
- [Security Testing Plan](security/security-testing-plan.md)

---

## Finding Documentation

### By Topic

**Architecture Decisions:** `/architecture/ADR-*.md` (6 ADRs)
**DDD Patterns:** `/development/PATTERNS.md`
**Testing:** `/development/TESTING_WITH_PLAYWRIGHT.md`
**Security:** `/security/` (4 documents)
**OAuth:** `/authentication/` (4 documents)
**Features:** `/product-strategy/FEATURE_BACKLOG.md`

### By Phase

**Phase 0 (Foundation):** ADR-001, ADR-002, LOCAL_DEVELOPMENT_SETUP.md
**Phase 1 (MVP):** FEATURE_BACKLOG.md (MVP features), implementation-roadmap.md
**Phase 2-4:** Feature Backlog (by phase assignment)
**Phase 5+ (Microservices):** cloud-architecture.md, kubernetes-deployment-guide.md

---

## Documentation Contribution

### When to Update Documentation

**Always update:**

- ADRs when making architectural decisions
- Feature Backlog when adding/changing features
- PATTERNS.md when discovering new patterns
- Wireframes when changing UI

**Consider updating:**

- CODING_STANDARDS.md for new standards
- WORKFLOWS.md for new workflows
- Domain Model when adding modules

### How to Update

1. Read existing docs first
2. Check for consistency with related docs
3. Update canonical sources (not derived docs)
4. Run link checker
5. Update INDEX.md if adding new docs

---

## Related Documentation

- **Root:** [/CLAUDE.md](../CLAUDE.md) - Root navigation hub
- **Backend:** [/src/api/CLAUDE.md](../src/api/CLAUDE.md) - Backend guide
- **Frontend:** [/src/frontend/CLAUDE.md](../src/frontend/CLAUDE.md) - Frontend guide
- **Complete Index:** [INDEX.md](INDEX.md) - All 54 documents mapped

---

**Last Updated:** 2026-01-09
**Derived from:** Root CLAUDE.md v5.0.0
**Canonical Sources:**

- docs/INDEX.md (Complete documentation map)
- docs/executive-summary.md (Project overview)

**Sync Checklist:**

- [ ] Folder structure matches INDEX.md
- [ ] Key documents list accurate
- [ ] Links tested (no 404s)
- [ ] Role-based navigation helpful
