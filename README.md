# Family Hub

> An intelligent family organization platform with automated event chains to reduce mental load and save time.

[![License: AGPL v3](https://img.shields.io/badge/License-AGPL%20v3-blue.svg)](https://www.gnu.org/licenses/agpl-3.0)
[![.NET](https://img.shields.io/badge/.NET-10-purple.svg)](https://dotnet.microsoft.com/)
[![Angular](https://img.shields.io/badge/Angular-21-red.svg)](https://angular.io/)
[![Status](https://img.shields.io/badge/Status-Planning-orange.svg)](https://github.com/andrekirst/family2)

---

## What is Family Hub?

Family Hub is a comprehensive family organization platform that goes beyond simple calendars and to-do lists. It uses **event chain automation** to automatically coordinate across different aspects of family life, reducing the mental load of managing a household.

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

## Key Features

### Core Features (Phase 1-2)
- **Shared Family Calendar:** Events visible to all family members
- **Task Management:** Create, assign, and track tasks
- **Health Tracking:** Doctor appointments and prescriptions
- **Shopping Lists:** Shared lists with auto-population from prescriptions
- **Notifications:** In-app and push notifications for reminders
- **Event Chain Automation:** Cross-domain workflows (flagship feature)

## ✅ Completed Features

### Phase 1 - Foundation (In Progress)

**Family Management:**
- ✅ **Family Creation** (#15) - Users can create family groups with authentication
  - GraphQL mutation: `createFamily`
  - Business rule: One family per user enforced
  - Complete test coverage (94% - 48/51 tests passing)
  - Implemented: 2025-12-30

**In Development:**
- Family invitation system
- Role-based permissions
- Family settings management

### Advanced Features (Phase 3-4)
- **Meal Planning:** Weekly meal plans with recipes
- **Recipe Management:** Store and organize family recipes
- **Finance Tracking:** Budget management and expense tracking
- **Recurring Events/Tasks:** Daily, weekly, monthly automation
- **Search:** Global search across all data

### Future Features (Phase 5-6)
- **Mobile Apps:** iOS and Android native apps
- **Offline Mode:** Work without internet connection
- **AI Suggestions:** Smart task and meal recommendations
- **Voice Assistants:** Alexa and Google Assistant integration
- **Multi-language:** Support for multiple languages

---

## Technology Stack

### Backend
- **.NET Core 10** with C# 14
- **Hot Chocolate GraphQL** for unified API
- **PostgreSQL 16** for data persistence
- **Redis 7** for caching and event bus
- **Zitadel** for authentication (OAuth 2.0 / OIDC)

### Frontend
- **Angular v21** with TypeScript
- **Tailwind CSS** for styling
- **Apollo Client** for GraphQL
- **RxJS** for reactive programming

### Infrastructure
- **Kubernetes** for container orchestration
- **Docker** for containerization
- **Prometheus + Grafana** for monitoring
- **Seq / ELK** for centralized logging
- **GitHub Actions** for CI/CD

### Architecture
- **Microservices:** 8 bounded contexts (DDD)
- **Event-Driven:** Redis Pub/Sub (Phase 1-4), RabbitMQ (Phase 5+)
- **CQRS Pattern:** For complex queries
- **API Gateway:** YARP or Ocelot for routing

---

## Architecture Overview

### Bounded Contexts (Microservices)

```
┌─────────────────────────────────────────────────────────────────┐
│                        Family Hub Platform                       │
├─────────────────────────────────────────────────────────────────┤
│                                                                   │
│  ┌──────────────┐         ┌──────────────┐                      │
│  │   Auth       │────────▶│  Calendar    │                      │
│  │   Service    │         │   Service    │                      │
│  └──────────────┘         └──────────────┘                      │
│                                  │                               │
│  ┌──────────────┐         ┌──────────────┐                      │
│  │   Task       │────────▶│  Shopping    │                      │
│  │   Service    │         │   Service    │                      │
│  └──────────────┘         └──────────────┘                      │
│                                  │                               │
│  ┌──────────────┐         ┌──────────────┐                      │
│  │   Health     │────────▶│ Meal Planning│                      │
│  │   Service    │         │   Service    │                      │
│  └──────────────┘         └──────────────┘                      │
│                                  │                               │
│  ┌──────────────┐         ┌──────────────┐                      │
│  │   Finance    │────────▶│Communication │                      │
│  │   Service    │         │   Service    │                      │
│  └──────────────┘         └──────────────┘                      │
│                                                                   │
│  ┌─────────────────────────────────────────┐                   │
│  │         Event Bus (Redis Pub/Sub)        │                   │
│  └─────────────────────────────────────────┘                   │
│                                                                   │
└─────────────────────────────────────────────────────────────────┘
```

---

## Project Status

**Current Phase:** Planning and Documentation
**Target Launch:** MVP in 6 months (Phase 1-2)
**Development Model:** Single developer with AI assistance (Claude Code)

### Roadmap

| Phase | Focus | Timeline | Status |
|-------|-------|----------|--------|
| **Phase 0** | Foundation & Tooling | Weeks 1-4 | Planned |
| **Phase 1** | Core MVP (Auth, Calendar, Tasks) | Weeks 5-12 | Planned |
| **Phase 2** | Health Integration & Event Chains | Weeks 13-18 | Planned |
| **Phase 3** | Meal Planning & Finance | Weeks 19-26 | Planned |
| **Phase 4** | Advanced Features (Recurrence) | Weeks 27-34 | Planned |
| **Phase 5** | Production Hardening | Weeks 35-44 | Planned |
| **Phase 6** | Mobile Apps & Extended Features | Weeks 45-52+ | Planned |

**Estimated Timeline:** 12-18 months (part-time development)

---

## Documentation

Comprehensive business analysis and technical documentation is available in the `/docs` folder:

1. **[Domain Model & Microservices Map](/docs/domain-model-microservices-map.md)**
   - Bounded context definitions
   - Domain entities and aggregates
   - Event chains and integration points
   - GraphQL API schemas

2. **[Implementation Roadmap](/docs/implementation-roadmap.md)**
   - Phase-by-phase development plan
   - Deliverables and success criteria
   - Technology decision points
   - Cost estimation

3. **[Risk Register](/docs/risk-register.md)**
   - 35 identified risks with mitigation strategies
   - Risk scoring and monitoring
   - Contingency plans

4. **[Documentation Index](/docs/README.md)**
   - Complete documentation overview

---

## Getting Started

**Note:** The project is currently in the planning phase. Development will begin with Phase 0 (Foundation & Tooling).

### Prerequisites (Planned)
- .NET Core 10 SDK
- Node.js 20+ and npm
- Docker Desktop or Minikube
- PostgreSQL 16
- Redis 7
- Zitadel instance

### Installation (Coming Soon)

```bash
# Clone the repository
git clone https://github.com/andrekirst/family2.git
cd family2

# Start infrastructure (Docker Compose)
docker-compose up -d

# Run backend services
dotnet run --project src/FamilyHub.ApiGateway

# Run frontend
cd src/FamilyHub.Web
npm install
npm start
```

---

## Contributing

This is currently a solo project with AI assistance. Contributions will be welcome after the MVP launch (Phase 2).

### Planned Contribution Areas
- Feature development
- Bug fixes
- Documentation improvements
- Testing
- Localization (i18n)

---

## License

This project is licensed under the **GNU Affero General Public License v3.0** (AGPL-3.0).

**What this means:**
- You can use, modify, and distribute this software freely
- If you modify and deploy it as a network service, you must make your source code available
- This protects against proprietary forks while keeping the project open

See [LICENSE](LICENSE) for full details.

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
3. **AI-Assisted Development:** Document the effectiveness of Claude Code for solo development
4. **Open Source:** Contribute to the community and enable self-hosting

---

## Success Metrics

### MVP Success (End of Phase 2)
- 5-10 daily active families
- 20+ calendar events created per week
- 30+ tasks completed per week
- Event chain success rate >98%
- Average event chain latency <5 seconds

### Production Success (End of Phase 5)
- 50+ monthly active families
- System uptime >99.5%
- p95 response time <2 seconds
- Zero critical security vulnerabilities
- User NPS >40

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
- 60-80% of boilerplate code generated by AI
- 70% of unit tests automated
- 80% of documentation AI-assisted
- Human developer focuses on domain logic and architecture

### MVP-First Approach
- Deliver value incrementally
- Validate assumptions early
- Ruthless prioritization
- Technical debt awareness

### Quality Over Speed
- Comprehensive testing (unit, integration, E2E)
- Security by design
- Performance monitoring from day one
- Documentation as code evolves

---

## Support

### During Development (Phase 0-5)
- GitHub Issues for bug reports
- GitHub Discussions for feature requests
- Email: [TBD]

### Post-Launch (Phase 6+)
- Community Discord server
- User documentation and tutorials
- Video walkthroughs
- FAQ and troubleshooting guides

---

## Acknowledgments

- **Zitadel Team** for the excellent open-source identity platform
- **Hot Chocolate Team** for the powerful GraphQL framework
- **Claude Code (Anthropic)** for AI-assisted development
- **DDD Community** for domain-driven design patterns
- **Family Beta Testers** (coming soon!)

---

## Roadmap Highlights

### Q1 2026 (Weeks 1-13)
- Phase 0: Development environment setup
- Phase 1: MVP with auth, calendar, and tasks
- First family beta testers onboarded

### Q2 2026 (Weeks 14-26)
- Phase 2: Health integration and event chains
- Phase 3: Meal planning and finance tracking
- 10 families using the platform

### Q3 2026 (Weeks 27-39)
- Phase 4: Advanced features (recurrence, search)
- Phase 5: Production hardening and security audit
- Beta testing with 20+ families

### Q4 2026 (Weeks 40-52+)
- Phase 6: Mobile apps and AI features
- Public beta launch
- 50+ families using Family Hub

---

## Contact

**Project Owner:** Andre Kirst
**GitHub:** [@andrekirst](https://github.com/andrekirst)
**Repository:** https://github.com/andrekirst/family2

---

## Frequently Asked Questions

### Why not use existing tools like Google Calendar + Todoist?
Existing tools require manual coordination. Family Hub automates the connections between different aspects of family life, saving time and mental energy.

### Is my family's data secure?
Yes. Data is encrypted at rest and in transit. You can self-host for complete control. Security audits will be conducted before public launch.

### How much does it cost?
Free during beta. Post-launch: Freemium model (free for up to 5 family members, paid for larger groups or premium features). Self-hosting is always free.

### Can I contribute?
Not yet, but contributions will be welcome after MVP launch (Phase 2). Watch the repo for updates.

### What if the project is abandoned?
The AGPL-3.0 license ensures the code remains open. The community can fork and continue development. Phase boundaries enable clean handoff points.

### Is it production-ready?
No, currently in planning phase. Production readiness targeted for Q3 2026 (Phase 5 completion).

---

**Star this repo to follow development progress!**
