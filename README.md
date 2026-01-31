# Family Hub - Strategic Foundation & Template

> **⚠️ PROJECT RESTART (February 2026):** This project has undergone an architectural restart. All previous implementation code has been removed. What remains is a strategic foundation template with comprehensive documentation, domain models, and development guides. The previous implementation is preserved in Git tag `v0.1-phase0-archive`.

[![License: AGPL v3](https://img.shields.io/badge/License-AGPL%20v3-blue.svg)](https://www.gnu.org/licenses/agpl-3.0)
[![Status](https://img.shields.io/badge/Status-Foundation-orange.svg)](https://github.com/andrekirst/family2)

---

## What is Family Hub?

Family Hub is a comprehensive family organization platform concept that goes beyond simple calendars and to-do lists. It uses **event chain automation** to automatically coordinate across different aspects of family life, reducing the mental load of managing a household.

### The Problem

Managing a family requires juggling multiple apps and remembering countless details:

- Doctor appointments need to be in the calendar
- Prescriptions need to be picked up
- Grocery shopping for the week's meal plan
- Bills need to be paid on time
- Tasks need to be assigned and tracked

Each of these requires manual coordination across different tools, creating mental overhead and opportunities for things to fall through the cracks.

### The Solution

Family Hub automates the coordination with **event chains**. Here's how it works:

**Example: Doctor Appointment Event Chain**

```
1. You schedule a doctor appointment
   ↓ (automatic)
2. Calendar event is created
   ↓ (automatic)
3. Task is created: "Prepare questions for Dr. Smith"
   ↓ (automatic)
4. After the appointment, you record a prescription
   ↓ (automatic)
5. Medication is added to your shopping list
   ↓ (automatic)
6. Task is created: "Pick up prescription at pharmacy"
   ↓ (automatic)
7. Reminder sent before refill date
```

All of this happens automatically. You only schedule the appointment.

---

## Current Status: Strategic Foundation Template

**This repository contains:**

✓ **Strategic Documentation:** Product strategy, market analysis, UX/design concepts
✓ **Domain Model:** Comprehensive domain-driven design specifications
✓ **Event Chains:** Detailed event chain automation concepts
✓ **Architecture ADRs:** 5 foundational architecture decision records
✓ **Agent OS Infrastructure:** Spec-driven development profiles and standards
✓ **Development Guides:** Claude Code integration and patterns (10 CLAUDE.md guides)

**What is NOT included:**

✗ No runnable code (backend, frontend, database schemas)
✗ No infrastructure configurations
✗ No test suites
✗ No deployment pipelines

**Reason for Restart:** The initial architecture needed redesign. This strategic foundation preserves the valuable conceptual work while enabling a fresh architectural approach.

**Previous Implementation:** Preserved in Git tag [`v0.1-phase0-archive`](https://github.com/andrekirst/family2/tree/v0.1-phase0-archive) for reference.

---

## Planned Features

### Core Features (Future Phase 1-2)

- **Shared Family Calendar:** Events visible to all family members
- **Task Management:** Create, assign, and track tasks
- **Health Tracking:** Doctor appointments and prescriptions
- **Shopping Lists:** Shared lists with auto-population from prescriptions
- **Notifications:** In-app and push notifications for reminders
- **Event Chain Automation:** Cross-domain workflows (flagship feature)

### Advanced Features (Future Phase 3-4)

- **Meal Planning:** Weekly meal plans with recipes
- **Recipe Management:** Store and organize family recipes
- **Finance Tracking:** Budget management and expense tracking
- **Recurring Events/Tasks:** Daily, weekly, monthly automation
- **Search:** Global search across all data

### Future Features (Future Phase 5-6)

- **Mobile Apps:** iOS and Android native apps
- **Offline Mode:** Work without internet connection
- **AI Suggestions:** Smart task and meal recommendations
- **Voice Assistants:** Alexa and Google Assistant integration
- **Multi-language:** Support for multiple languages

---

## Conceptual Technology Stack

The following technologies are **planned** for future implementation:

### Backend (Conceptual)

- **.NET Core** with C#
- **Hot Chocolate GraphQL** for unified API
- **PostgreSQL** for data persistence
- **Redis** for caching and event bus
- **OAuth 2.0 / OIDC** for authentication

### Frontend (Conceptual)

- **Angular** with TypeScript
- **Tailwind CSS** for styling
- **Apollo Client** for GraphQL
- **RxJS** for reactive programming

### Infrastructure (Conceptual)

- **Kubernetes** for container orchestration
- **Docker** for containerization
- **Prometheus + Grafana** for monitoring
- **GitHub Actions** for CI/CD

### Architecture Strategy

- **Modular Monolith First:** Start with a modular monolith for faster iteration ([ADR-001](docs/architecture/ADR-001-MODULAR-MONOLITH-FIRST.md))
- **Eventual Microservices:** Extract modules using Strangler Fig pattern ([ADR-005](docs/architecture/ADR-005-FAMILY-MODULE-EXTRACTION-PATTERN.md))
- **Event-Driven:** Async communication between modules
- **CQRS Pattern:** For complex queries
- **Domain-Driven Design:** Clear bounded contexts

---

## Architecture Overview (Conceptual)

### Bounded Contexts

```
┌─────────────────────────────────────────────────────────────────┐
│                        Family Hub Platform                       │
├─────────────────────────────────────────────────────────────────┤
│                                                                   │
│  ┌──────────────┐         ┌──────────────┐                      │
│  │   Auth       │────────▶│  Calendar    │                      │
│  │   Context    │         │   Context    │                      │
│  └──────────────┘         └──────────────┘                      │
│                                  │                               │
│  ┌──────────────┐         ┌──────────────┐                      │
│  │   Task       │────────▶│  Shopping    │                      │
│  │   Context    │         │   Context    │                      │
│  └──────────────┘         └──────────────┘                      │
│                                  │                               │
│  ┌──────────────┐         ┌──────────────┐                      │
│  │   Health     │────────▶│ Meal Planning│                      │
│  │   Context    │         │   Context    │                      │
│  └──────────────┘         └──────────────┘                      │
│                                  │                               │
│  ┌──────────────┐         ┌──────────────┐                      │
│  │   Finance    │────────▶│Communication │                      │
│  │   Context    │         │   Context    │                      │
│  └──────────────┘         └──────────────┘                      │
│                                                                   │
│  ┌─────────────────────────────────────────┐                   │
│  │         Event Bus (Async Messaging)      │                   │
│  └─────────────────────────────────────────┘                   │
│                                                                   │
└─────────────────────────────────────────────────────────────────┘
```

---

## Documentation

Comprehensive strategic and technical documentation is preserved in the `/docs` folder:

### Strategic Documentation

1. **[Product Strategy](/docs/product-strategy/PRODUCT_STRATEGY.md)**
   - Vision, mission, and goals
   - Target market and personas
   - Competitive analysis

2. **[Feature Backlog](/docs/product-strategy/FEATURE_BACKLOG.md)**
   - Prioritized feature list
   - User stories and acceptance criteria

3. **[Implementation Roadmap](/docs/product-strategy/implementation-roadmap.md)**
   - Phase-by-phase development plan
   - Deliverables and success criteria

4. **[Risk Register](/docs/product-strategy/risk-register.md)**
   - Identified risks with mitigation strategies
   - Risk scoring and monitoring

### Architecture Documentation

1. **[Domain Model & Microservices Map](/docs/architecture/domain-model-microservices-map.md)**
   - Bounded context definitions
   - Domain entities and aggregates
   - Integration points

2. **[Event Chains Reference](/docs/architecture/event-chains-reference.md)**
   - Detailed event chain specifications
   - Cross-domain workflows

3. **[Architecture Decision Records (ADRs)](/docs/architecture/)**
   - ADR-001: Modular Monolith First
   - ADR-002: OAuth with Zitadel
   - ADR-003: GraphQL Input-Command Pattern
   - ADR-005: Family Module Extraction Pattern

### Development Guides

1. **[Backend Development Guide](/docs/guides/BACKEND_DEVELOPMENT.md)**
   - .NET, C#, GraphQL, DDD patterns

2. **[Frontend Development Guide](/docs/guides/FRONTEND_DEVELOPMENT.md)**
   - Angular, TypeScript, component architecture

3. **[Database Development Guide](/docs/guides/DATABASE_DEVELOPMENT.md)**
   - PostgreSQL, migrations, schema design

4. **[Infrastructure Development Guide](/docs/guides/INFRASTRUCTURE_DEVELOPMENT.md)**
   - Docker, Kubernetes, CI/CD

5. **[Claude Code Guide](/CLAUDE.md)**
   - AI-assisted development workflows
   - Integration with Claude Code

### Market & Business

- **[Market Research](/docs/market-business/)**
  - Competitor analysis
  - Market sizing
  - Business model

### UX & Design

- **[UX Design Documentation](/docs/ux-design/)**
  - User personas
  - User journeys
  - Wireframes and mockups

### Security

- **[Security Documentation](/docs/security/)**
  - Threat model
  - OWASP compliance
  - RLS (Row-Level Security) concepts

---

## Agent OS: Spec-Driven Development

This project uses **Agent OS** for spec-driven development with Claude Code:

- **[Agent OS Overview](/agent-os/)** - DDD module profiles, standards, and specs
- **[Profiles](/agent-os/profiles/)** - 8 module profiles + 5 layer profiles
- **[Standards](/agent-os/standards/)** - Extracted architectural patterns
- **[Specs](/agent-os/specs/)** - Machine-readable feature specifications
- **[Skills](/.claude/skills/)** - Claude Code implementation guides

This infrastructure enables rapid, consistent feature development following DDD and established patterns.

---

## Getting Started

### For Contributors/Developers

1. **Review Strategic Documentation:**

   ```bash
   # Read product strategy
   cat docs/product-strategy/PRODUCT_STRATEGY.md

   # Review domain model
   cat docs/architecture/domain-model-microservices-map.md

   # Understand event chains
   cat docs/architecture/event-chains-reference.md
   ```

2. **Explore Architecture Decisions:**

   ```bash
   # Read ADRs
   ls docs/architecture/ADR-*.md
   ```

3. **Review Agent OS Infrastructure:**

   ```bash
   # Explore module profiles
   ls agent-os/profiles/modules/

   # Review extracted standards
   ls agent-os/standards/
   ```

4. **Read Development Guides:**
   - [CLAUDE.md](/CLAUDE.md) - Project overview and guide navigation
   - [docs/guides/](/docs/guides/) - Domain-specific development guides

### For Future Implementation

When implementation resumes, developers will:

1. **Choose Architecture:** Use ADRs as foundation, adapt as needed
2. **Leverage Agent OS:** Use profiles and standards for consistent patterns
3. **Follow Guides:** CLAUDE.md guides provide domain-specific best practices
4. **Build Incrementally:** Use feature backlog and roadmap for prioritization

---

## Project Goals

### Primary Goals

1. **Save Time:** Reduce family organization overhead by 50% through automation
2. **Reduce Mental Load:** Fewer things to remember, fewer apps to manage
3. **Strengthen Family Connection:** Shared visibility and coordination
4. **Maintain Privacy:** Family data under their control (self-hosting option)

### Technical Goals

1. **Showcase DDD/Event-Driven Architecture:** Real-world implementation of modern patterns
2. **Demonstrate Microservices:** Properly bounded contexts with clear integration
3. **AI-Assisted Development:** Document the effectiveness of Claude Code for development
4. **Open Source:** Contribute to the community and enable self-hosting

---

## Key Differentiator

Unlike other family organization apps, Family Hub doesn't just store information - it **coordinates** your family life automatically.

**Traditional Approach:**

1. Schedule doctor appointment in calendar app
2. Set reminder in reminder app
3. Add medication to notes app
4. Add medication to shopping app
5. Create task to pick up prescription
6. Set reminder for refill date

**Family Hub Approach:**

1. Schedule doctor appointment
2. Everything else happens automatically

This is the power of event chain automation.

---

## Why Open Source?

1. **Transparency:** Families trust code they can inspect
2. **Self-Hosting:** Organizations can deploy on their infrastructure
3. **Community:** Benefit from contributions and feedback
4. **Learning:** Share knowledge about DDD and event-driven architecture
5. **Sustainability:** Project survives even if original developer moves on

---

## Development Philosophy

### Single Developer with AI Assistance

This project embraces AI-assisted development with Claude Code:

- Strategic foundation created collaboratively with AI
- Domain modeling and architecture designed iteratively
- Comprehensive documentation maintained throughout
- Human developer focuses on domain logic and strategic decisions

### Foundation-First Approach

- Comprehensive strategic planning before implementation
- Domain model clarity before code
- Architecture decisions documented as ADRs
- Agent OS infrastructure for consistent patterns

### Quality Over Speed

- Comprehensive planning and documentation
- Security by design
- Domain-driven design principles
- Event-driven architecture patterns

---

## License

This project is licensed under the **GNU Affero General Public License v3.0** (AGPL-3.0).

**What this means:**

- You can use, modify, and distribute this software freely
- If you modify and deploy it as a network service, you must make your source code available
- This protects against proprietary forks while keeping the project open

See [LICENSE](LICENSE) for full details.

---

## Contributing

This is currently a strategic foundation template. Contributions to documentation, domain modeling, and architectural planning are welcome.

### Contribution Areas

- Domain model refinement
- Architecture decision reviews
- Documentation improvements
- Event chain specifications
- UX/design concepts
- Market analysis

See [CONTRIBUTING.md](CONTRIBUTING.md) for guidelines.

---

## Support

- **GitHub Issues:** Bug reports and feature requests
- **GitHub Discussions:** Strategic discussions and questions
- **Documentation:** Comprehensive guides in `/docs`

---

## Acknowledgments

- **Claude Code (Anthropic)** for AI-assisted strategic planning and documentation
- **DDD Community** for domain-driven design patterns
- **Event Sourcing Community** for event-driven architecture patterns
- **Open Source Community** for inspiration and tools

---

## Contact

**Project Owner:** Andre Kirst
**GitHub:** [@andrekirst](https://github.com/andrekirst)
**Repository:** https://github.com/andrekirst/family2

---

## Frequently Asked Questions

### Why was the project restarted?

The initial architecture needed redesign. Rather than accumulating technical debt, we chose to restart with a clean slate while preserving all strategic documentation and conceptual work.

### What happened to the previous implementation?

All previous implementation code is preserved in Git tag [`v0.1-phase0-archive`](https://github.com/andrekirst/family2/tree/v0.1-phase0-archive) and can be referenced or restored at any time.

### Can I use this as a template for my own project?

Yes! The strategic foundation, domain models, architecture ADRs, and Agent OS infrastructure can serve as a template for similar projects. The AGPL-3.0 license allows free use with proper attribution.

### When will implementation resume?

Timeline to be determined. The current focus is on refining the strategic foundation and architecture before implementation.

### Is the strategic documentation still valuable?

Absolutely. The product strategy, domain model, event chains, and architecture decisions represent months of strategic planning and remain highly valuable for future implementation.

### Can I contribute to the strategic foundation?

Yes! Contributions to documentation, domain modeling, architecture decisions, and planning are welcome. See [CONTRIBUTING.md](CONTRIBUTING.md).

---

**Star this repo to follow the project's evolution!**
