# Phase 0 Implementation Plan - Complete Foundation & Tooling

**Created:** 2026-01-09
**Issue:** #13 - Phase 0: Foundation & Tooling
**Timeline:** 4 weeks
**Status:** Planning Complete

---

## Executive Summary

Phase 0 is **75% complete** but has critical gaps preventing success criteria fulfillment:

**Completed (75%):**

- ✅ Docker Compose infrastructure (PostgreSQL, RabbitMQ, Zitadel, Seq, MailHog)
- ✅ CI/CD pipeline (GitHub Actions with backend/frontend builds)
- ✅ Backend OAuth integration (100% - 4/4 tests passing)
- ✅ Frontend Angular v21 scaffolding with Playwright
- ✅ Auth module (fully implemented with DDD structure)
- ✅ Family module (30% - placeholder structure)
- ✅ All 8 PostgreSQL schemas created

**Critical Gaps (25%):**

- ❌ Kubernetes manifests (0% complete)
- ❌ RLS policies for multi-tenant isolation (0% complete)
- ❌ 6 DDD modules not scaffolded (Calendar, Task, Shopping, Health, MealPlanning, Finance, Communication)
- ❌ Frontend OAuth UI components (callback page, login page)
- ❌ Integration tests disabled in CI
- ❌ Comprehensive setup documentation incomplete
- ❌ Hot reload verification not documented

**Strategy:** Focus on pragmatic completion of gaps rather than perfection. Phase 0 establishes foundation for Phase 1 development.

---

## Success Criteria Analysis

| # | Criterion | Current Status | Gap | Priority |
|---|-----------|----------------|-----|----------|
| 1 | Developer can spin up full environment in <30 min | 🟡 Partial (Docker works, K8s missing) | K8s manifests + docs | P1 |
| 2 | CI/CD runs on every PR (build, test, lint) | ✅ Complete | None | Done |
| 3 | All 8 DDD modules have basic structure | 🔴 25% (2/8 modules) | 6 module scaffolds | P1 |
| 4 | PostgreSQL with RLS enabled | 🟡 50% (schemas exist, RLS missing) | RLS policies | P0 |
| 5 | Zitadel OAuth testable locally | 🟡 70% (backend works, UI missing) | Frontend UI | P2 |
| 6 | Hot reload works for backend and frontend | 🟢 Works (needs verification) | Documentation | P3 |
| 7 | Documentation complete and tested | 🟡 60% (technical docs exist, setup guide incomplete) | Setup guide | P1 |

**Priority Legend:**

- P0: Critical security/data integrity
- P1: Blocks Phase 1 development
- P2: Important but workarounds exist
- P3: Nice-to-have, can defer

---

## Implementation Plan

### Week 1: Critical Infrastructure & Security

**Focus:** RLS policies, module scaffolding, K8s basics

#### Sub-Issue 1.1: Implement RLS Policies for Multi-Tenant Isolation

**Estimated Effort:** 6-8 hours
**Priority:** P0 (Critical Security)

**Acceptance Criteria:**

- RLS enabled on all tables in auth schema
- Family group isolation policy applied to all relevant tables
- Helper functions created for user context setting
- RLS policies tested with integration tests
- Migration script created for RLS policies

**Deliverables:**

- SQL migration: `002-enable-rls-policies.sql`
- RLS policy for `auth.families` table
- RLS policy for `auth.family_members` table
- RLS policy for `auth.family_member_invitations` table
- C# middleware to set `app.current_user_id` context
- Integration tests verifying isolation
- Documentation: RLS architecture decision record

**Dependencies:** None (can start immediately)

**Technical Approach:**

```sql
-- Enable RLS on family tables
ALTER TABLE auth.families ENABLE ROW LEVEL SECURITY;

-- Policy: Users can only see families they belong to
CREATE POLICY family_member_access ON auth.families
  USING (id IN (
    SELECT family_id FROM auth.family_members 
    WHERE user_id = current_user_id()
  ));
```

---

#### Sub-Issue 1.2: Scaffold 6 Missing DDD Modules

**Estimated Effort:** 12-14 hours
**Priority:** P1 (Blocks Phase 1)

**Acceptance Criteria:**

- Calendar module structure created
- Task module structure created
- Shopping module structure created
- Health module structure created
- MealPlanning module structure created
- Finance module structure created
- Communication module structure created (simplified - Phase 1 only)
- Each module has: Domain/, Application/, Infrastructure/, GraphQL/ folders
- Each module has placeholder DbContext
- Each module registered in API Composition Root
- Module structure documented

**Deliverables:**

- 6 module directories with DDD structure
- Placeholder aggregates for each module
- Empty DbContext classes
- GraphQL schema type definitions
- Module registration in `Program.cs`
- Architecture documentation update

**Dependencies:** None (parallel with 1.1)

**Module Structure (per module):**

```
FamilyHub.Modules.[ModuleName]/
├── Domain/
│   ├── Aggregates/
│   ├── Events/
│   ├── ValueObjects/
│   └── Interfaces/
├── Application/
│   ├── Commands/
│   ├── Queries/
│   └── Handlers/
├── Infrastructure/
│   ├── Persistence/
│   │   └── [ModuleName]DbContext.cs
│   └── EventHandlers/
└── GraphQL/
    ├── Types/
    ├── Queries/
    └── Mutations/
```

---

#### Sub-Issue 1.3: Create Basic Kubernetes Manifests

**Estimated Effort:** 8-10 hours
**Priority:** P1 (Success Criterion)

**Acceptance Criteria:**

- K8s namespace manifest for `familyhub`
- PostgreSQL StatefulSet with persistent volume
- ConfigMap for environment variables
- Secrets for sensitive data
- Service manifests for all components
- Ingress manifest for API Gateway
- All manifests validated with `kubectl apply --dry-run`
- README with K8s deployment instructions
- Works on Docker Desktop Kubernetes (local)

**Deliverables:**

- `k8s/namespace.yaml`
- `k8s/postgres-statefulset.yaml`
- `k8s/postgres-service.yaml`
- `k8s/postgres-pvc.yaml`
- `k8s/configmap.yaml`
- `k8s/secrets.yaml.template`
- `k8s/api-deployment.yaml`
- `k8s/frontend-deployment.yaml`
- `k8s/ingress.yaml`
- `k8s/README.md`

**Dependencies:** None (parallel work)

**Note:** Keep simple for Phase 0. Advanced features (Helm, HPA, Istio) deferred to Phase 5.

---

### Week 2: Developer Experience & Testing

**Focus:** Setup documentation, integration tests, frontend OAuth

#### Sub-Issue 2.1: Fix Integration Tests and Re-enable in CI

**Estimated Effort:** 6-8 hours
**Priority:** P1 (CI/CD gap)

**Acceptance Criteria:**

- Integration tests run successfully locally
- Testcontainers work in CI environment
- Integration tests re-enabled in `ci.yml`
- All integration tests pass
- Coverage reports generated
- Flaky tests identified and fixed

**Deliverables:**

- Fixed integration test suite
- Updated `ci.yml` with integration tests enabled
- Testcontainers configuration for CI
- Integration test troubleshooting guide

**Dependencies:** None

**Investigation Areas:**

- Docker API version compatibility in CI
- Testcontainers resource limits
- Network configuration in GitHub Actions

---

#### Sub-Issue 2.2: Complete Frontend OAuth UI Components

**Estimated Effort:** 8-10 hours
**Priority:** P2 (Important for E2E testing)

**Acceptance Criteria:**

- OAuth callback page component created
- Login page component created
- Login flow integrated with auth service
- Token storage implemented (secure)
- Protected route guards working
- E2E test for login flow passing
- Error handling for failed auth

**Deliverables:**

- `src/frontend/family-hub-web/src/app/auth/callback.component.ts`
- `src/frontend/family-hub-web/src/app/auth/login.component.ts`
- `src/frontend/family-hub-web/src/app/auth/auth.guard.ts`
- E2E test: `auth-flow.spec.ts`
- User guide: OAuth login flow

**Dependencies:** Backend OAuth (complete)

**Design Pattern:** Follow existing Angular component patterns in codebase.

---

#### Sub-Issue 2.3: Create Comprehensive Setup Documentation

**Estimated Effort:** 6-8 hours
**Priority:** P1 (Success Criterion #7)

**Acceptance Criteria:**

- Fresh developer can set up environment in <30 minutes
- Prerequisites clearly documented
- Step-by-step setup instructions
- Troubleshooting section for common issues
- Screenshots/diagrams where helpful
- Tested by at least one person unfamiliar with project
- Hot reload verification documented

**Deliverables:**

- `docs/setup/QUICK_START.md` (10-minute guide)
- `docs/setup/DETAILED_SETUP.md` (comprehensive)
- `docs/setup/TROUBLESHOOTING.md`
- `docs/setup/VERIFICATION.md` (checklist)
- Update root `README.md` with setup link

**Dependencies:** All infrastructure must be working

**Sections:**

1. Prerequisites (Node, .NET, Docker, etc.)
2. Clone and configure
3. Start infrastructure (Docker Compose)
4. Run migrations
5. Configure Zitadel
6. Start backend API
7. Start frontend
8. Verify hot reload
9. Run tests
10. Troubleshooting

---

### Week 3: Database Migrations & Module Integration

**Focus:** Complete module migrations, event bus setup

#### Sub-Issue 3.1: Create Database Migrations for All Modules

**Estimated Effort:** 10-12 hours
**Priority:** P1 (Required for Phase 1)

**Acceptance Criteria:**

- EF Core migrations created for each module
- Migration names follow convention: `[Timestamp]_InitialCreate_[ModuleName]`
- All migrations apply cleanly
- Down migrations tested
- Migration order documented
- Seed data for development (optional)

**Deliverables:**

- 6 migration files (one per new module)
- Migration execution script
- Rollback documentation
- Database schema diagram (updated)

**Dependencies:** Sub-Issue 1.2 (module scaffolding)

**Migration Strategy:**

- One DbContext per module (multi-tenant with shared connection string)
- Separate migration history tables per module
- Idempotent migrations

---

#### Sub-Issue 3.2: Set Up Event Bus Abstraction

**Estimated Effort:** 8-10 hours
**Priority:** P2 (Phase 1 needs this)

**Acceptance Criteria:**

- Event bus abstraction interface defined
- In-memory event bus implementation (Phase 1)
- RabbitMQ implementation skeleton (Phase 5+)
- Domain event base class created
- Event publishing pattern documented
- Sample event handler implemented
- Unit tests for event bus

**Deliverables:**

- `IEventPublisher` interface
- `InMemoryEventBus` implementation
- `DomainEvent` base class
- `EventHandler<T>` pattern
- Documentation: Event-driven architecture
- Sample: `FamilyCreatedEvent` → `SendWelcomeEmailHandler`

**Dependencies:** Module scaffolding complete

**Note:** Start simple (in-memory), add RabbitMQ in Phase 5 when microservices extracted.

---

### Week 4: Polish, Validation, and Handoff

**Focus:** End-to-end testing, documentation polish, Phase 0 closeout

#### Sub-Issue 4.1: Create End-to-End Setup Test

**Estimated Effort:** 6-8 hours
**Priority:** P1 (Success Criterion #1 validation)

**Acceptance Criteria:**

- Automated script tests full environment setup
- Script times setup duration (<30 min target)
- Script validates all services healthy
- Script runs sample GraphQL queries
- Script creates test family and member
- Script verifies hot reload
- CI job runs setup test

**Deliverables:**

- `scripts/test-setup.sh` (Bash)
- `scripts/test-setup.ps1` (PowerShell for Windows)
- CI job: `setup-validation`
- Setup test report template

**Dependencies:** All other sub-issues complete

---

#### Sub-Issue 4.2: Phase 0 Documentation Polish

**Estimated Effort:** 4-6 hours
**Priority:** P2 (Professional handoff)

**Acceptance Criteria:**

- All ADRs reviewed and updated
- README badges accurate
- Architecture diagrams updated
- CONTRIBUTING.md reviewed
- Phase 0 completion checklist marked
- Known issues documented
- Phase 1 handoff notes created

**Deliverables:**

- Updated ADRs (if changes made)
- `docs/phase-0/COMPLETION_REPORT.md`
- `docs/phase-0/KNOWN_ISSUES.md`
- `docs/phase-1/HANDOFF.md`
- Phase 0 retrospective notes

**Dependencies:** All technical work complete

---

#### Sub-Issue 4.3: Phase 0 Acceptance Testing

**Estimated Effort:** 4-6 hours
**Priority:** P0 (Gate to Phase 1)

**Acceptance Criteria:**

- All success criteria verified
- Fresh developer setup tested (<30 min)
- All CI/CD jobs passing
- All documentation reviewed
- No critical bugs
- Phase 0 demo prepared
- Stakeholder sign-off obtained

**Deliverables:**

- Phase 0 acceptance test report
- Success criteria verification checklist
- Phase 0 demo script
- Stakeholder approval

**Dependencies:** Everything else complete

---

## Dependency Map

```
Week 1:
  [1.1 RLS Policies] ──────────────┐
  [1.2 Module Scaffolding] ────┐   │
  [1.3 K8s Manifests] ─────────┼───┼──→ Week 2
                               │   │
Week 2:                        │   │
  [2.1 Fix Integration Tests]  │   │
  [2.2 Frontend OAuth UI] ─────┤   │
  [2.3 Setup Documentation] ←──┴───┴──┐
                                      │
Week 3:                               │
  [3.1 DB Migrations] ←───────────────┤
  [3.2 Event Bus] ←───────────────────┤
                                      │
Week 4:                               │
  [4.1 E2E Setup Test] ←──────────────┤
  [4.2 Documentation Polish] ←────────┤
  [4.3 Acceptance Testing] ←──────────┘
```

**Parallelizable Work:**

- Week 1: All three sub-issues can run in parallel
- Week 2: Sub-issues 2.1 and 2.2 parallel, 2.3 depends on 1.3
- Week 3: Sub-issues depend on Week 1-2 completion
- Week 4: Sequential gate checks

---

## Effort Estimation

| Week | Sub-Issues | Total Hours | Confidence |
|------|------------|-------------|------------|
| Week 1 | 1.1, 1.2, 1.3 | 26-32 hours | High |
| Week 2 | 2.1, 2.2, 2.3 | 20-26 hours | Medium |
| Week 3 | 3.1, 3.2 | 18-22 hours | Medium |
| Week 4 | 4.1, 4.2, 4.3 | 14-20 hours | High |
| **Total** | **12 sub-issues** | **78-100 hours** | **High** |

**Assumptions:**

- Single developer with Claude Code assistance
- No major blockers or scope changes
- Existing infrastructure works as documented

**Contingency:** 20% buffer = 94-120 hours (fits within 4-week part-time timeline)

---

## Risk Mitigation

### Risk 1: RLS Policies Complexity

**Impact:** High (security critical)
**Probability:** Medium
**Mitigation:** Start with simple policies, iterate based on testing. Consult PostgreSQL RLS best practices.
**Contingency:** If RLS proves too complex, defer to Phase 1 Week 1 and use application-level checks temporarily.

### Risk 2: Integration Tests Flakiness

**Impact:** Medium (CI reliability)
**Probability:** High (Testcontainers in CI known issue)
**Mitigation:** Thorough investigation of Docker API compatibility. Add retries for flaky tests.
**Contingency:** Keep integration tests local-only if CI issues persist; run before each PR manually.

### Risk 3: K8s Complexity

**Impact:** Medium (success criterion)
**Probability:** Low (basic manifests only)
**Mitigation:** Keep K8s simple for Phase 0. Focus on Docker Compose as primary local dev.
**Contingency:** K8s can be "demo-able" rather than production-ready for Phase 0.

### Risk 4: Module Scaffolding Tedium

**Impact:** Low (just boilerplate)
**Probability:** Low (clear pattern to follow)
**Mitigation:** Use Claude Code generation for boilerplate. Follow Auth module pattern exactly.
**Contingency:** Scaffold only 3 modules if time runs out (Calendar, Task, Shopping - highest Phase 1 priority).

---

## Out of Scope (Deferred)

**Explicitly NOT included in Phase 0:**

- ❌ Helm charts (Phase 5)
- ❌ RabbitMQ production setup (Phase 5)
- ❌ Horizontal Pod Autoscaling (Phase 5)
- ❌ Istio/Service Mesh (Phase 5)
- ❌ Production-grade monitoring (Phase 5)
- ❌ Advanced GraphQL features (federation, etc.)
- ❌ Mobile app scaffolding (Phase 6)
- ❌ AI/ML features (Phase 6+)
- ❌ Federation Service (Phase 7+)

**Minimal Viable Phase 0:**
Focus on "developer can start Phase 1 work immediately" rather than "production-ready infrastructure."

---

## Acceptance Criteria (Final Checklist)

### Infrastructure

- [ ] Docker Compose starts all services successfully
- [ ] PostgreSQL accessible with all 8 schemas created
- [ ] RabbitMQ accessible (UI at localhost:15672)
- [ ] Zitadel accessible and configured (localhost:8080)
- [ ] Kubernetes manifests exist and validate
- [ ] K8s deployment works on Docker Desktop

### Backend

- [ ] All 8 modules have basic structure
- [ ] Auth module complete with tests passing
- [ ] Family module complete with tests passing
- [ ] RLS policies implemented and tested
- [ ] Event bus abstraction created
- [ ] All migrations apply cleanly
- [ ] GraphQL playground accessible
- [ ] Hot reload works for C# changes

### Frontend

- [ ] Angular app builds successfully
- [ ] OAuth callback page functional
- [ ] Login page functional
- [ ] Protected routes working
- [ ] Hot reload works for TypeScript changes
- [ ] Playwright tests configured

### CI/CD

- [ ] All CI jobs passing
- [ ] Backend build succeeds
- [ ] Frontend build succeeds
- [ ] Unit tests pass
- [ ] Integration tests pass (re-enabled)
- [ ] E2E tests pass (Playwright)
- [ ] Code quality checks pass
- [ ] Docker builds succeed

### Documentation

- [ ] Quick start guide complete (<30 min setup)
- [ ] Detailed setup guide complete
- [ ] Troubleshooting guide created
- [ ] Verification checklist provided
- [ ] Hot reload verification documented
- [ ] All ADRs up to date
- [ ] Phase 0 completion report written

### Testing

- [ ] Fresh developer can set up in <30 minutes
- [ ] All success criteria verified
- [ ] Automated setup test passes
- [ ] No critical bugs
- [ ] Phase 0 demo prepared

---

## Phase 1 Handoff Notes

**Ready for Phase 1 when:**

1. All Phase 0 acceptance criteria met
2. Developer environment proven with fresh setup
3. All 8 modules scaffolded and ready for feature work
4. CI/CD pipeline reliable
5. Documentation comprehensive

**Phase 1 Priorities (Week 5-12):**

1. Family invitation system (Auth module)
2. Calendar event CRUD (Calendar module)
3. Task management basics (Task module)
4. First event chain (Calendar → Task)
5. Frontend pages for Calendar and Tasks

**Technical Debt to Address in Phase 1:**

- None critical (Phase 0 should be clean)
- Consider: Upgrade to RabbitMQ if event volume high
- Consider: Add Redis caching if needed

---

## Success Metrics

**Phase 0 Complete When:**

- ✅ All 7 success criteria verified
- ✅ 12 sub-issues closed
- ✅ Fresh developer setup <30 min
- ✅ CI/CD pipeline 100% green
- ✅ Stakeholder sign-off obtained

**Exit Criteria for Phase 1 Entry:**

- Developer can create new module features without infrastructure friction
- Testing is fast and reliable
- Documentation is self-serve
- No critical blockers or unknowns

---

**Document Version:** 1.0
**Last Updated:** 2026-01-09
**Status:** Ready for Implementation
