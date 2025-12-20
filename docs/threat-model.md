# Threat Model - Family Hub Security Architecture

**Version:** 1.0
**Date:** 2025-12-20
**Status:** Final
**Owner:** Security Engineer
**Compliance:** WCAG 2.1 AA, COPPA, GDPR

---

## Executive Summary

This threat model identifies potential security threats to Family Hub, a privacy-first family organization platform handling sensitive data including children's information (COPPA), health records, financial data, and location information. Using the STRIDE methodology, we analyze threats across all system components and define comprehensive mitigation strategies.

### Key Findings

**Critical Threats Identified:** 12
**High Priority Threats:** 18
**Medium Priority Threats:** 23

**Priority Security Controls:**

1. End-to-end encryption for health records and financial data
2. Multi-factor authentication (MFA) for privileged accounts
3. Row-Level Security (RLS) for multi-tenant data isolation
4. Comprehensive audit logging for all sensitive operations
5. Container security with non-root users and read-only filesystems
6. Network policies for pod-to-pod communication restrictions

---

## Table of Contents

1. [System Overview](#1-system-overview)
2. [Assets & Data Classification](#2-assets--data-classification)
3. [Threat Modeling Methodology](#3-threat-modeling-methodology)
4. [STRIDE Analysis by Component](#4-stride-analysis-by-component)
5. [Attack Surface Analysis](#5-attack-surface-analysis)
6. [Threat Scenarios](#6-threat-scenarios)
7. [Risk Assessment Matrix](#7-risk-assessment-matrix)
8. [Mitigation Summary](#8-mitigation-summary)

---

## 1. System Overview

### 1.1 Architecture Components

```
┌──────────────────────────────────────────────────────────────┐
│                     Internet (Attackers)                      │
└────────────────────────┬─────────────────────────────────────┘
                         │
          ┌──────────────▼──────────────┐
          │     Edge Protection         │
          │  - WAF (Future)             │
          │  - DDoS Protection          │
          │  - Rate Limiting            │
          └──────────────┬──────────────┘
                         │
          ┌──────────────▼──────────────┐
          │  NGINX Ingress Controller   │
          │  - TLS Termination          │
          │  - SSL/TLS 1.3 Only         │
          │  - Certificate Validation   │
          └──────────────┬──────────────┘
                         │
┌────────────────────────┼────────────────────────────┐
│  Kubernetes Cluster    │                            │
│            ┌───────────▼──────────┐                 │
│            │   API Gateway        │                 │
│            │  - Auth Validation   │                 │
│            │  - GraphQL Schema    │                 │
│            │  - Rate Limiting     │                 │
│            └───────────┬──────────┘                 │
│                        │                            │
│        ┌───────────────┼──────────────┐             │
│        │               │              │             │
│  ┌─────▼────┐  ┌──────▼─────┐  ┌────▼─────┐        │
│  │  Auth    │  │  Calendar  │  │   Task   │        │
│  │  Service │  │  Service   │  │  Service │        │
│  └─────┬────┘  └──────┬─────┘  └────┬─────┘        │
│        │               │             │              │
│  ┌─────▼────┐  ┌──────▼─────┐  ┌────▼─────┐        │
│  │  Health  │  │  Finance   │  │ Shopping │        │
│  │  Service │  │  Service   │  │  Service │        │
│  └─────┬────┘  └──────┬─────┘  └────┬─────┘        │
│        │               │             │              │
│        └───────────────┼─────────────┘              │
│                        │                            │
│        ┌───────────────▼──────────────┐             │
│        │  PostgreSQL (Multi-tenant)   │             │
│        │  - Row-Level Security (RLS)  │             │
│        │  - Encryption at Rest        │             │
│        └──────────────────────────────┘             │
│                                                     │
│        ┌────────────────────────────┐               │
│        │  Redis (Event Bus)         │               │
│        │  - Pub/Sub Events          │               │
│        │  - Session Storage         │               │
│        └────────────────────────────┘               │
└─────────────────────────────────────────────────────┘

External Dependencies:
┌──────────────┐     ┌──────────────┐
│   Zitadel    │     │  Let's       │
│   (OAuth)    │     │  Encrypt     │
└──────────────┘     └──────────────┘
```

### 1.2 Trust Boundaries

**Trust Boundary 1: Internet → Ingress**
- Untrusted user traffic
- Potential DDoS, injection attacks
- Protection: TLS, WAF, rate limiting

**Trust Boundary 2: Ingress → API Gateway**
- Partially trusted (authenticated)
- Token validation required
- Protection: JWT validation, RBAC

**Trust Boundary 3: API Gateway → Microservices**
- Trusted internal network
- Service-to-service communication
- Protection: Network policies, mutual TLS (future)

**Trust Boundary 4: Microservices → Database**
- Highly trusted
- Data access layer
- Protection: RLS, encrypted connections, least privilege

**Trust Boundary 5: External Services**
- Zitadel (Auth Provider): Partially trusted
- Let's Encrypt: Trusted
- Protection: TLS pinning, API key rotation

---

## 2. Assets & Data Classification

### 2.1 Data Classification

| Classification | Data Types | Encryption | Access Control | Retention |
|----------------|-----------|------------|----------------|-----------|
| **Critical** | - Children's PII (COPPA)<br>- Health records<br>- Prescription data<br>- Financial account numbers | End-to-end encryption<br>+ Encryption at rest<br>+ TLS in transit | MFA required<br>Audit logging<br>RLS enforcement | 7 years (health)<br>Deleted on request |
| **Sensitive** | - Family member names<br>- Email addresses<br>- Calendar events<br>- Tasks<br>- Budget data | Encryption at rest<br>+ TLS in transit | Standard auth<br>RLS enforcement | Active + 1 year<br>Deleted on request |
| **Internal** | - Session tokens<br>- API keys<br>- Database credentials | Encrypted secrets<br>Sealed Secrets | Service accounts<br>K8s RBAC | Rotated quarterly |
| **Public** | - App metadata<br>- Public documentation | TLS in transit | None | Indefinite |

### 2.2 Critical Assets

**User Data Assets:**
1. **Children's Information (COPPA Protected)**
   - Names, birthdates, task history
   - Threat: Unauthorized access, data leak
   - Impact: Legal liability, regulatory fines, reputational damage

2. **Health Records**
   - Appointments, prescriptions, medications
   - Threat: Data breach, unauthorized modification
   - Impact: Privacy violation, medical harm, legal liability

3. **Financial Data**
   - Budget information, expense records, payment methods
   - Threat: Financial fraud, identity theft
   - Impact: Direct financial loss, fraud liability

4. **Location Data**
   - Calendar event locations, home addresses
   - Threat: Stalking, physical harm
   - Impact: Personal safety, privacy violation

**System Assets:**
5. **Authentication Credentials**
   - OAuth tokens, session IDs, API keys
   - Threat: Account takeover, privilege escalation
   - Impact: Full system compromise

6. **Database (PostgreSQL)**
   - All family data, multi-tenant storage
   - Threat: SQL injection, data exfiltration
   - Impact: Complete data breach

7. **Event Bus (Redis)**
   - Real-time events, session data
   - Threat: Event manipulation, session hijacking
   - Impact: Data integrity compromise

8. **Kubernetes Cluster**
   - Infrastructure control plane
   - Threat: Container escape, privilege escalation
   - Impact: Full infrastructure compromise

---

## 3. Threat Modeling Methodology

### 3.1 STRIDE Framework

We use Microsoft's STRIDE methodology to systematically identify threats:

| Threat Type | Security Property | Description |
|------------|------------------|-------------|
| **S**poofing | Authentication | Attacker impersonates another user or system |
| **T**ampering | Integrity | Unauthorized modification of data or code |
| **R**epudiation | Non-Repudiation | User denies performing an action |
| **I**nformation Disclosure | Confidentiality | Exposure of sensitive information |
| **D**enial of Service | Availability | System becomes unavailable |
| **E**levation of Privilege | Authorization | Attacker gains unauthorized permissions |

### 3.2 Risk Scoring

**Likelihood:**
- 1 = Very Low (theoretical)
- 2 = Low (requires sophisticated attacker)
- 3 = Medium (moderate skill/resources)
- 4 = High (script kiddies can exploit)
- 5 = Very High (actively exploited in wild)

**Impact:**
- 1 = Negligible (minor inconvenience)
- 2 = Low (limited data exposure)
- 3 = Medium (service disruption, some data exposure)
- 4 = High (significant data breach, extended outage)
- 5 = Critical (complete compromise, legal liability)

**Risk Score = Likelihood × Impact**

**Priority Levels:**
- **Critical (20-25):** Immediate action required
- **High (15-19):** Fix within 1 sprint
- **Medium (8-14):** Fix within 2-3 sprints
- **Low (1-7):** Backlog, address when possible

---

## 4. STRIDE Analysis by Component

### 4.1 NGINX Ingress Controller

**Component:** Edge traffic handler, TLS termination

#### Spoofing Threats

| Threat | Likelihood | Impact | Risk | Mitigation |
|--------|-----------|--------|------|------------|
| **T4.1.1:** Attacker spoofs legitimate domain via DNS poisoning | 2 | 4 | 8 | - DNSSEC<br>- Certificate Transparency logs<br>- HSTS preloading |
| **T4.1.2:** Man-in-the-middle attack on TLS connection | 1 | 5 | 5 | - TLS 1.3 only<br>- Strong cipher suites<br>- Certificate pinning (mobile apps) |

#### Tampering Threats

| Threat | Likelihood | Impact | Risk | Mitigation |
|--------|-----------|--------|------|------------|
| **T4.1.3:** HTTP header injection | 3 | 3 | 9 | - Input validation<br>- Header sanitization<br>- NGINX security config |

#### Denial of Service Threats

| Threat | Likelihood | Impact | Risk | Mitigation |
|--------|-----------|--------|------|------------|
| **T4.1.4:** DDoS attack overwhelms ingress | 4 | 4 | **16 HIGH** | - Cloud provider DDoS protection<br>- Rate limiting (100 req/min per IP)<br>- Connection limits<br>- Monitoring/alerting |
| **T4.1.5:** Slowloris attack (slow HTTP) | 3 | 3 | 9 | - Connection timeouts<br>- Request body size limits<br>- nginx timeout config |

#### Information Disclosure Threats

| Threat | Likelihood | Impact | Risk | Mitigation |
|--------|-----------|--------|------|------------|
| **T4.1.6:** TLS downgrade attack exposes data | 1 | 5 | 5 | - TLS 1.3 only<br>- Disable TLS 1.0/1.1<br>- HSTS headers |
| **T4.1.7:** Server information leakage in headers | 2 | 1 | 2 | - Remove server version headers<br>- Custom error pages |

---

### 4.2 API Gateway (YARP/GraphQL)

**Component:** GraphQL federation, authentication validation

#### Spoofing Threats

| Threat | Likelihood | Impact | Risk | Mitigation |
|--------|-----------|--------|------|------------|
| **T4.2.1:** Stolen JWT token used for impersonation | 3 | 5 | **15 HIGH** | - Short token expiry (1 hour)<br>- Refresh token rotation<br>- Token revocation list<br>- IP binding (optional) |
| **T4.2.2:** Session fixation attack | 2 | 4 | 8 | - Regenerate session on login<br>- Secure cookie flags<br>- SameSite=Strict |

#### Tampering Threats

| Threat | Likelihood | Impact | Risk | Mitigation |
|--------|-----------|--------|------|------------|
| **T4.2.3:** GraphQL injection via malicious query | 3 | 4 | 12 | - Query depth limiting (max 5 levels)<br>- Query cost analysis<br>- Parameterized queries<br>- Input validation |
| **T4.2.4:** JWT tampering to elevate privileges | 2 | 5 | 10 | - JWT signature validation<br>- Use RS256 (not HS256)<br>- Validate all claims |

#### Repudiation Threats

| Threat | Likelihood | Impact | Risk | Mitigation |
|--------|-----------|--------|------|------------|
| **T4.2.5:** User denies deleting family member | 2 | 3 | 6 | - Audit logging for sensitive operations<br>- Immutable log storage<br>- Log retention 1 year |

#### Information Disclosure Threats

| Threat | Likelihood | Impact | Risk | Mitigation |
|--------|-----------|--------|------|------------|
| **T4.2.6:** GraphQL introspection leaks schema to attacker | 3 | 2 | 6 | - Disable introspection in production<br>- Schema analysis tooling disabled |
| **T4.2.7:** Verbose error messages expose system internals | 3 | 2 | 6 | - Generic error messages to users<br>- Detailed errors only in logs<br>- Error code mapping |

#### Denial of Service Threats

| Threat | Likelihood | Impact | Risk | Mitigation |
|--------|-----------|--------|------|------------|
| **T4.2.8:** Complex GraphQL query causes CPU exhaustion | 4 | 4 | **16 HIGH** | - Query complexity limiting (max 1000 points)<br>- Query timeout (5 seconds)<br>- Rate limiting per user (1000 points/min) |
| **T4.2.9:** Batch query abuse (large array of queries) | 3 | 3 | 9 | - Batch size limits (max 10 queries)<br>- Cost accumulation across batch |

#### Elevation of Privilege Threats

| Threat | Likelihood | Impact | Risk | Mitigation |
|--------|-----------|--------|------|------------|
| **T4.2.10:** Authorization bypass via direct service call | 2 | 5 | 10 | - Enforce auth at gateway AND service level<br>- Network policies restrict service-to-service<br>- Service mesh (future) |

---

### 4.3 Auth Service (Zitadel Integration)

**Component:** User authentication, family group management

#### Spoofing Threats

| Threat | Likelihood | Impact | Risk | Mitigation |
|--------|-----------|--------|------|------------|
| **T4.3.1:** Phishing attack steals user credentials | 4 | 4 | **16 HIGH** | - Passwordless authentication (WebAuthn)<br>- MFA enforcement for admins<br>- Email verification<br>- Security training |
| **T4.3.2:** Credential stuffing from leaked databases | 3 | 4 | 12 | - Rate limiting on login (5 attempts/min)<br>- Account lockout after 10 failed attempts<br>- Breach notification monitoring |
| **T4.3.3:** Brute force attack on passwords | 3 | 3 | 9 | - Password complexity requirements<br>- bcrypt hashing (cost 12)<br>- Progressive delays on failed login |

#### Tampering Threats

| Threat | Likelihood | Impact | Risk | Mitigation |
|--------|-----------|--------|------|------------|
| **T4.3.4:** OAuth state parameter tampering | 2 | 4 | 8 | - Cryptographically secure state generation<br>- State validation on callback<br>- PKCE for OAuth flow |

#### Information Disclosure Threats

| Threat | Likelihood | Impact | Risk | Mitigation |
|--------|-----------|--------|------|------------|
| **T4.3.5:** Username enumeration via registration | 3 | 2 | 6 | - Generic error message ("Email sent if account exists")<br>- Rate limiting on registration |
| **T4.3.6:** Timing attack reveals valid usernames | 2 | 2 | 4 | - Constant-time comparison for passwords<br>- Consistent response times |

#### Elevation of Privilege Threats

| Threat | Likelihood | Impact | Risk | Mitigation |
|--------|-----------|--------|------|------------|
| **T4.3.7:** Child account claims admin role | 2 | 5 | 10 | - Role validation at API and database level<br>- Immutable role assignment for children<br>- Audit logging |
| **T4.3.8:** Invited member claims owner role | 2 | 5 | 10 | - Owner role cannot be transferred via invitation<br>- Explicit ownership transfer process |

---

### 4.4 Health Service

**Component:** Medical appointments, prescriptions (HIPAA-adjacent)

#### Spoofing Threats

| Threat | Likelihood | Impact | Risk | Mitigation |
|--------|-----------|--------|------|------------|
| **T4.4.1:** Family member views another's health records | 3 | 5 | **15 HIGH** | - Row-Level Security per patient<br>- Explicit access grants only<br>- Audit logging |

#### Tampering Threats

| Threat | Likelihood | Impact | Risk | Mitigation |
|--------|-----------|--------|------|------------|
| **T4.4.2:** Prescription data modified to increase dosage | 2 | 5 | 10 | - Immutable prescription records (append-only)<br>- Audit trail<br>- Admin approval for changes |

#### Information Disclosure Threats

| Threat | Likelihood | Impact | Risk | Mitigation |
|--------|-----------|--------|------|------------|
| **T4.4.3:** Health data exposed in logs | 3 | 5 | **15 HIGH** | - **End-to-end encryption** for health records<br>- Field-level encryption<br>- Sanitize logs (mask sensitive fields)<br>- PII detection in logs |
| **T4.4.4:** Health data leaked via insecure backup | 2 | 5 | 10 | - Encrypted backups (AES-256)<br>- Secure backup storage (S3 with encryption)<br>- Access logging |

---

### 4.5 Finance Service

**Component:** Budget tracking, expense management

#### Information Disclosure Threats

| Threat | Likelihood | Impact | Risk | Mitigation |
|--------|-----------|--------|------|------------|
| **T4.5.1:** Financial data exposed to unauthorized family member | 3 | 4 | 12 | - RLS enforcement<br>- Explicit sharing permissions<br>- View-only vs edit permissions |
| **T4.5.2:** Bank account numbers exposed in logs | 2 | 5 | 10 | - Field-level encryption for payment methods<br>- Token last 4 digits only<br>- PII redaction in logs |

#### Tampering Threats

| Threat | Likelihood | Impact | Risk | Mitigation |
|--------|-----------|--------|------|------------|
| **T4.5.3:** Budget amounts manipulated to hide spending | 2 | 3 | 6 | - Audit trail for all budget changes<br>- Version history<br>- Integrity checks |

---

### 4.6 PostgreSQL Database

**Component:** Primary data store, multi-tenant

#### Spoofing Threats

| Threat | Likelihood | Impact | Risk | Mitigation |
|--------|-----------|--------|------|------------|
| **T4.6.1:** Service spoofs another service to access data | 2 | 5 | 10 | - Separate database users per service<br>- Least privilege grants<br>- mTLS service auth (future) |

#### Tampering Threats

| Threat | Likelihood | Impact | Risk | Mitigation |
|--------|-----------|--------|------|------------|
| **T4.6.2:** SQL injection attack modifies data | 3 | 5 | **15 HIGH** | - Parameterized queries only<br>- ORM with SQL injection protection<br>- Input validation<br>- Database WAF (pgbouncer) |
| **T4.6.3:** Direct database access bypasses RLS | 2 | 5 | 10 | - No direct database access from outside cluster<br>- Network policies<br>- Database firewall rules |

#### Information Disclosure Threats

| Threat | Likelihood | Impact | Risk | Mitigation |
|--------|-----------|--------|------|------------|
| **T4.6.4:** RLS misconfiguration exposes cross-tenant data | 3 | 5 | **15 HIGH** | - Automated RLS testing<br>- Code review for all RLS policies<br>- Tenant isolation validation tests<br>- Separate test tenant for security testing |
| **T4.6.5:** Database backup leaked with unencrypted data | 2 | 5 | 10 | - Encrypted backups (AES-256)<br>- Secure backup storage<br>- Access control on backups<br>- Regular backup restoration tests |

#### Denial of Service Threats

| Threat | Likelihood | Impact | Risk | Mitigation |
|--------|-----------|--------|------|------------|
| **T4.6.6:** Expensive query locks database | 3 | 4 | 12 | - Query timeout (30 seconds)<br>- Connection pooling (PgBouncer)<br>- Query plan analysis<br>- Index optimization |

---

### 4.7 Redis (Event Bus & Cache)

**Component:** Event pub/sub, session storage

#### Tampering Threats

| Threat | Likelihood | Impact | Risk | Mitigation |
|--------|-----------|--------|------|------------|
| **T4.7.1:** Malicious event injected into bus | 2 | 4 | 8 | - Service authentication for pub/sub<br>- Event schema validation<br>- Event signing (HMAC) |

#### Information Disclosure Threats

| Threat | Likelihood | Impact | Risk | Mitigation |
|--------|-----------|--------|------|------------|
| **T4.7.2:** Session data exposed via Redis access | 2 | 4 | 8 | - Redis password authentication<br>- TLS for Redis connections<br>- Network policies |

#### Denial of Service Threats

| Threat | Likelihood | Impact | Risk | Mitigation |
|--------|-----------|--------|------|------------|
| **T4.7.3:** Event flood overwhelms Redis | 3 | 4 | 12 | - Event rate limiting per service<br>- Event TTL<br>- Redis memory limits<br>- Circuit breaker on event publishing |

---

### 4.8 Kubernetes Infrastructure

**Component:** Container orchestration, cluster control plane

#### Elevation of Privilege Threats

| Threat | Likelihood | Impact | Risk | Mitigation |
|--------|-----------|--------|------|------------|
| **T4.8.1:** Container escape to host system | 2 | 5 | 10 | - Pod Security Standards (restricted)<br>- Non-root containers<br>- Read-only root filesystem<br>- seccomp profiles<br>- AppArmor/SELinux |
| **T4.8.2:** Privilege escalation via misconfigured RBAC | 2 | 5 | 10 | - Least privilege RBAC<br>- No cluster-admin for apps<br>- Regular RBAC audits<br>- Separate service accounts per deployment |

#### Information Disclosure Threats

| Threat | Likelihood | Impact | Risk | Mitigation |
|--------|-----------|--------|------|------------|
| **T4.8.3:** Secrets exposed in container environment | 3 | 5 | **15 HIGH** | - Sealed Secrets for GitOps<br>- Secrets mounted as volumes (not env vars)<br>- External Secrets Operator (future)<br>- Secret rotation |
| **T4.8.4:** Cluster metadata exposed via API server | 2 | 3 | 6 | - API server authentication required<br>- RBAC for API access<br>- Network policies |

#### Denial of Service Threats

| Threat | Likelihood | Impact | Risk | Mitigation |
|--------|-----------|--------|------|------------|
| **T4.8.5:** Resource exhaustion by single pod | 3 | 3 | 9 | - Resource limits and requests<br>- LimitRanges<br>- ResourceQuotas<br>- Horizontal Pod Autoscaling |

---

## 5. Attack Surface Analysis

### 5.1 External Attack Surface

**Entry Points:**

1. **HTTPS/443 - Web Application**
   - GraphQL API endpoint
   - Angular SPA
   - Threat: Injection attacks, XSS, CSRF
   - Protection: Input validation, CSP, CSRF tokens

2. **OAuth Callback Endpoint**
   - Zitadel authentication flow
   - Threat: OAuth token theft, session hijacking
   - Protection: State validation, PKCE, secure cookies

3. **Let's Encrypt ACME Challenge**
   - Certificate renewal (HTTP-01 challenge)
   - Threat: Domain takeover
   - Protection: DNS validation, short-lived certificates

**Exposed Metadata:**

- TLS certificate (domain name, issuer)
- HTTP headers (server info - minimize)
- Error messages (sanitize)
- GraphQL schema (disable introspection in prod)

### 5.2 Internal Attack Surface

**Inter-Service Communication:**

1. **Service-to-Service API Calls**
   - HTTP/gRPC between microservices
   - Threat: Service impersonation, data exfiltration
   - Protection: Network policies, service accounts, future mTLS

2. **Event Bus (Redis Pub/Sub)**
   - Event publishing and subscription
   - Threat: Event injection, eavesdropping
   - Protection: Event schema validation, Redis auth, TLS

3. **Database Connections**
   - PostgreSQL connections from services
   - Threat: SQL injection, unauthorized access
   - Protection: Parameterized queries, least privilege, connection pooling

**Kubernetes API Surface:**

- kubectl access (developers)
- CI/CD service account (ArgoCD)
- Threat: Cluster takeover, secret theft
- Protection: RBAC, audit logging, MFA for admins

### 5.3 Supply Chain Attack Surface

**Dependencies:**

1. **NuGet Packages (.NET)**
   - Threat: Malicious package, vulnerable dependency
   - Protection: Dependency scanning (Dependabot), package signing verification

2. **npm Packages (Angular)**
   - Threat: Malicious package, prototype pollution
   - Protection: npm audit, package-lock.json, SRI for CDN resources

3. **Container Base Images**
   - Threat: Vulnerable base image, malware
   - Protection: Official images only, image scanning (Trivy), distroless images

4. **Helm Charts / Kubernetes Manifests**
   - Threat: Misconfigured security settings, backdoors
   - Protection: Code review, policy enforcement (OPA), kubesec

---

## 6. Threat Scenarios

### Scenario 1: Account Takeover via Phishing

**Threat Actor:** External attacker (opportunistic)
**Motivation:** Financial gain, identity theft
**Skill Level:** Medium

**Attack Chain:**

```
1. Attacker sends phishing email mimicking Family Hub
   ↓
2. User clicks malicious link, enters credentials on fake site
   ↓
3. Attacker obtains credentials, logs into real Family Hub
   ↓
4. Attacker accesses children's data, health records, financial info
   ↓
5. Attacker exfiltrates data, potentially sells on dark web
```

**Impact:**
- **Confidentiality:** HIGH - Full access to family data
- **Integrity:** MEDIUM - Attacker could modify data
- **Availability:** LOW - Unlikely to disrupt service
- **Overall Risk:** **HIGH (Likelihood 4 × Impact 4 = 16)**

**Mitigations:**

1. **Prevention:**
   - Passwordless authentication (WebAuthn/passkeys)
   - MFA enforcement for all users (optional for free tier, required for premium)
   - Email verification for new devices
   - Anti-phishing training links in app

2. **Detection:**
   - Anomalous login location detection
   - Multiple failed login attempts alerting
   - New device login notification

3. **Response:**
   - Automatic account lockout after 10 failed attempts
   - User notification of suspicious activity
   - One-click session termination

---

### Scenario 2: SQL Injection Attack on Health Service

**Threat Actor:** External attacker (sophisticated)
**Motivation:** Data exfiltration, ransomware
**Skill Level:** High

**Attack Chain:**

```
1. Attacker identifies GraphQL query for health appointments
   ↓
2. Attacker crafts malicious input: `doctorName: "Dr. Smith'; DROP TABLE prescriptions;--"`
   ↓
3. If query is not parameterized, SQL injection succeeds
   ↓
4. Attacker gains read/write access to database
   ↓
5. Attacker exfiltrates health records for all families
```

**Impact:**
- **Confidentiality:** CRITICAL - All health data exposed
- **Integrity:** CRITICAL - Data could be modified or deleted
- **Availability:** CRITICAL - Database could be dropped
- **Overall Risk:** **CRITICAL (Likelihood 3 × Impact 5 = 15)**

**Mitigations:**

1. **Prevention:**
   - **Always use parameterized queries** (ORM with Entity Framework)
   - Input validation and sanitization
   - Principle of least privilege (service DB user cannot DROP tables)
   - Database Web Application Firewall (pgbouncer rules)

2. **Detection:**
   - SQL injection pattern detection in WAF
   - Anomalous query detection (pg_stat_statements)
   - Database activity monitoring

3. **Response:**
   - Automatic query blocking
   - Incident response playbook activation
   - Forensic analysis of query logs

---

### Scenario 3: Child Account Privacy Violation (COPPA)

**Threat Actor:** Malicious family member OR external attacker
**Motivation:** Identity theft, exploitation
**Skill Level:** Low to Medium

**Attack Chain:**

```
1. Attacker gains access to parent account (via phishing or insider threat)
   ↓
2. Attacker accesses child's profile (Noah, age 7)
   ↓
3. Attacker views task history, achievements, personal info
   ↓
4. Attacker exfiltrates child's data
   ↓
5. COPPA violation - improper access to child data
```

**Impact:**
- **Legal:** CRITICAL - COPPA fines up to $43,280 per violation
- **Reputational:** CRITICAL - Trust lost with families
- **Confidentiality:** HIGH - Child's data exposed
- **Overall Risk:** **CRITICAL (Likelihood 3 × Impact 5 = 15)**

**Mitigations:**

1. **Prevention:**
   - Separate access controls for child accounts
   - Parental consent logging for all child data access
   - MFA required to view child data
   - No child data in logs or analytics

2. **Detection:**
   - Audit logging for all child data access
   - Anomalous access pattern detection
   - Regular COPPA compliance audits

3. **Response:**
   - Immediate access revocation
   - Parent notification of child data access
   - Legal team involvement for COPPA violations

---

### Scenario 4: Kubernetes Cluster Compromise

**Threat Actor:** Advanced Persistent Threat (APT)
**Motivation:** Espionage, data exfiltration
**Skill Level:** Very High

**Attack Chain:**

```
1. Attacker exploits vulnerability in container image
   ↓
2. Attacker achieves container escape to Kubernetes node
   ↓
3. Attacker accesses Kubernetes API via node service account
   ↓
4. Attacker escalates privileges to cluster-admin
   ↓
5. Attacker accesses all Secrets (DB credentials, API keys)
   ↓
6. Attacker exfiltrates entire database
```

**Impact:**
- **Confidentiality:** CRITICAL - All data exposed
- **Integrity:** CRITICAL - Full control of infrastructure
- **Availability:** CRITICAL - Could shut down entire platform
- **Overall Risk:** **CRITICAL (Likelihood 2 × Impact 5 = 10, but increasing)**

**Mitigations:**

1. **Prevention:**
   - **Pod Security Standards:** Restricted mode
   - **Non-root containers:** All images run as UID 1000+
   - **Read-only root filesystem**
   - **seccomp and AppArmor profiles**
   - **Network policies:** Deny all by default
   - **Image scanning:** Trivy in CI/CD, block critical CVEs
   - **Least privilege RBAC:** No cluster-admin for services

2. **Detection:**
   - Runtime security (Falco or similar)
   - Audit logging for all API server access
   - Anomalous container behavior detection
   - Network traffic analysis

3. **Response:**
   - Automated pod termination on policy violation
   - Incident response playbook
   - Cluster forensics and rebuild

---

### Scenario 5: DDoS Attack on Production

**Threat Actor:** External attacker (hacktivist or competitor)
**Motivation:** Service disruption
**Skill Level:** Medium

**Attack Chain:**

```
1. Attacker launches volumetric DDoS attack (100k req/sec)
   ↓
2. NGINX Ingress overwhelmed, response times increase to 30+ seconds
   ↓
3. Users unable to access Family Hub
   ↓
4. Reputation damage, user churn
```

**Impact:**
- **Availability:** CRITICAL - Service unavailable
- **Financial:** MEDIUM - Cloud costs increase
- **Reputational:** HIGH - Users lose trust
- **Overall Risk:** **HIGH (Likelihood 4 × Impact 4 = 16)**

**Mitigations:**

1. **Prevention:**
   - Cloud provider DDoS protection (Layer 3/4)
   - Rate limiting (100 requests/min per IP)
   - NGINX connection limits
   - CDN for static assets (Cloudflare, Fastly)
   - Geographic IP blocking if necessary

2. **Detection:**
   - Traffic anomaly detection
   - Latency and error rate monitoring
   - Alerting on traffic spikes

3. **Response:**
   - Automatic rate limiting escalation
   - Traffic shaping and filtering
   - Communication with users during incident
   - Post-incident review and hardening

---

## 7. Risk Assessment Matrix

### 7.1 Critical Risks (Immediate Action Required)

| Risk ID | Threat | Likelihood | Impact | Risk Score | Mitigation Status |
|---------|--------|-----------|--------|------------|-------------------|
| R-001 | SQL injection on Health Service | 3 | 5 | 15 | ✅ Parameterized queries enforced |
| R-002 | RLS misconfiguration (cross-tenant leak) | 3 | 5 | 15 | ⚠️ Needs automated testing |
| R-003 | Secrets exposed in Kubernetes | 3 | 5 | 15 | ⚠️ Sealed Secrets planned |
| R-004 | Health data in logs (HIPAA-adjacent) | 3 | 5 | 15 | ⚠️ Field-level encryption needed |
| R-005 | Child data access (COPPA violation) | 3 | 5 | 15 | ⚠️ Access controls + audit logging |

### 7.2 High Risks (Fix Within 1 Sprint)

| Risk ID | Threat | Likelihood | Impact | Risk Score | Mitigation Status |
|---------|--------|-----------|--------|------------|-------------------|
| R-006 | Account takeover via phishing | 4 | 4 | 16 | ⚠️ MFA optional, needs enforcement |
| R-007 | DDoS attack overwhelms ingress | 4 | 4 | 16 | ⚠️ Cloud DDoS + rate limiting |
| R-008 | Complex GraphQL query DoS | 4 | 4 | 16 | ⚠️ Query complexity limits needed |
| R-009 | JWT token theft/replay | 3 | 5 | 15 | ⚠️ Short expiry + rotation |
| R-010 | Container escape to host | 2 | 5 | 10 | ⚠️ Pod Security Standards planned |

### 7.3 Medium Risks (Fix Within 2-3 Sprints)

| Risk ID | Threat | Likelihood | Impact | Risk Score | Mitigation Status |
|---------|--------|-----------|--------|------------|-------------------|
| R-011 | GraphQL injection | 3 | 4 | 12 | ⚠️ Input validation + depth limiting |
| R-012 | Credential stuffing attack | 3 | 4 | 12 | ⚠️ Rate limiting + breach monitoring |
| R-013 | Database query performance DoS | 3 | 4 | 12 | ⚠️ Query timeout + optimization |
| R-014 | Financial data exposure | 3 | 4 | 12 | ⚠️ Field encryption + RLS |
| R-015 | Event bus flooding | 3 | 4 | 12 | ⚠️ Event rate limiting needed |

### 7.4 Risk Heatmap

```
Impact
  5 │     R-001  R-002  R-009
    │     R-003  R-004  R-010
    │     R-005
  4 │     R-006  R-007  R-011
    │     R-008  R-012  R-013
    │            R-014  R-015
  3 │
    │
  2 │
    │
  1 │
    └─────────────────────────────
      1    2    3    4    5
                 Likelihood

Legend:
R-001 to R-005: Critical (red)
R-006 to R-010: High (orange)
R-011 to R-015: Medium (yellow)
```

---

## 8. Mitigation Summary

### 8.1 By Security Domain

#### Authentication & Authorization

**Implemented:**
- OAuth 2.0 / OIDC via Zitadel
- JWT token validation
- Role-based access control (RBAC)

**Planned (Phase 1):**
- ✅ MFA enforcement for admins
- ✅ Passwordless authentication (WebAuthn)
- ✅ Session management with short token expiry (1 hour)
- ✅ Refresh token rotation

**Planned (Phase 2+):**
- Magic link authentication
- Biometric authentication (mobile)
- Hardware security key support (YubiKey)

#### Data Protection

**Implemented:**
- TLS 1.3 for all external connections
- PostgreSQL encryption at rest

**Planned (Phase 1):**
- ✅ End-to-end encryption for health records
- ✅ Field-level encryption for sensitive data (SSN, payment methods)
- ✅ PII redaction in logs
- ✅ Encrypted database backups (AES-256)

**Planned (Phase 2+):**
- Client-side encryption before upload (documents)
- Zero-knowledge architecture for ultra-sensitive data

#### Application Security

**Implemented:**
- Input validation (basic)
- HTTPS-only connections

**Planned (Phase 1):**
- ✅ GraphQL query depth limiting (max 5 levels)
- ✅ GraphQL query cost analysis (max 1000 points)
- ✅ Rate limiting (100 req/min per IP)
- ✅ CSRF protection (SameSite cookies, CSRF tokens)
- ✅ XSS protection (Content Security Policy)
- ✅ Parameterized queries (SQL injection prevention)

**Planned (Phase 2+):**
- Web Application Firewall (WAF)
- Runtime application self-protection (RASP)
- Security headers (HSTS, X-Frame-Options, etc.)

#### Infrastructure Security

**Implemented:**
- Kubernetes cluster (basic security)
- Network policies (planned)

**Planned (Phase 1):**
- ✅ Pod Security Standards (restricted mode)
- ✅ Non-root containers
- ✅ Read-only root filesystems
- ✅ seccomp profiles
- ✅ Network policies (deny all by default)
- ✅ RBAC least privilege
- ✅ Sealed Secrets for GitOps

**Planned (Phase 2+):**
- Service mesh with mTLS (Linkerd)
- Runtime security monitoring (Falco)
- Image signing and verification (Cosign)
- Admission controller for policy enforcement (OPA Gatekeeper)

#### Monitoring & Incident Response

**Implemented:**
- Basic logging (Loki)
- Metrics (Prometheus)

**Planned (Phase 1):**
- ✅ Comprehensive audit logging (all sensitive operations)
- ✅ Security event correlation
- ✅ Alerting for critical events
- ✅ Incident response playbook

**Planned (Phase 2+):**
- Security Information and Event Management (SIEM)
- Automated incident response (SOAR)
- Forensics data collection
- Threat intelligence integration

### 8.2 Implementation Timeline

#### Phase 0 (Week 1-4): Foundation
- ✅ TLS 1.3 configuration
- ✅ Zitadel OAuth integration
- ✅ Basic input validation
- ✅ HTTPS-only enforcement

#### Phase 1 (Week 5-12): Core Security
- ✅ MFA for admins
- ✅ RLS implementation and testing
- ✅ Audit logging framework
- ✅ GraphQL security (depth, cost limits)
- ✅ Rate limiting
- ✅ Sealed Secrets

#### Phase 2 (Week 13-18): Data Protection
- ✅ End-to-end encryption for health data
- ✅ Field-level encryption
- ✅ PII redaction in logs
- ✅ Encrypted backups

#### Phase 3 (Week 19-26): Container Security
- ✅ Pod Security Standards
- ✅ Container image scanning (Trivy)
- ✅ Non-root containers
- ✅ Read-only filesystems

#### Phase 4-5 (Week 27-40): Advanced Security
- ⏳ WAF deployment
- ⏳ Runtime security (Falco)
- ⏳ Service mesh with mTLS
- ⏳ Penetration testing

#### Phase 6+ (Week 41+): Continuous Improvement
- ⏳ Security automation
- ⏳ Threat intelligence
- ⏳ Bug bounty program
- ⏳ SOC 2 Type II certification

---

## 9. Compliance Mapping

### 9.1 COPPA Requirements

| Requirement | Threat Mitigation | Implementation |
|-------------|------------------|----------------|
| Parental consent for child data | T4.3.7, R-005 | Consent flow + audit logging |
| Limited data collection | R-005 | Minimal data schema for children |
| Parental access to child data | R-005 | Parent dashboard with full access |
| Data deletion on request | R-005 | GDPR-compliant deletion workflow |
| No direct marketing to children | N/A | Policy enforcement |

### 9.2 GDPR Requirements

| Requirement | Threat Mitigation | Implementation |
|-------------|------------------|----------------|
| Right to access | Audit logging | Data export API |
| Right to be forgotten | Data retention | Deletion workflow with audit |
| Data portability | N/A | JSON export format |
| Breach notification (72 hours) | Incident response | Automated breach detection + playbook |
| Data minimization | R-005 | Collect only necessary data |

### 9.3 WCAG 2.1 AA (Security Perspective)

| Requirement | Threat Mitigation | Implementation |
|-------------|------------------|----------------|
| Accessible security features | Phishing resistance | Screen reader support for MFA |
| Timeout warnings | Session hijacking | 30-minute warning before logout |
| Error identification | Username enumeration | Generic error messages |

---

## 10. Threat Model Maintenance

### 10.1 Review Schedule

**Quarterly Reviews (Every 3 months):**
- Update threat model with new features
- Re-assess risk scores based on incidents
- Update mitigations based on emerging threats

**Annual Reviews:**
- Comprehensive threat modeling workshop
- External security audit integration
- Compliance requirement updates

**Trigger-Based Reviews:**
- New major feature deployment
- Significant architecture changes
- Security incident or breach
- New compliance requirements

### 10.2 Ownership

**Threat Model Owner:** Security Engineer
**Contributors:**
- DevOps Engineer (infrastructure threats)
- Backend Developers (application threats)
- Product Manager (business impact assessment)

### 10.3 Change Management

All threat model changes must:
1. Be reviewed by security team
2. Update risk register accordingly
3. Trigger mitigation plan updates
4. Be communicated to engineering team

---

## Appendix A: Threat Taxonomy

### A.1 STRIDE Definitions

**Spoofing Identity:**
- Definition: Pretending to be someone or something else
- Examples: Phishing, token theft, session hijacking
- Countermeasures: Strong authentication, MFA, passwordless

**Tampering with Data:**
- Definition: Malicious modification of data
- Examples: SQL injection, event manipulation, JWT tampering
- Countermeasures: Input validation, integrity checks, signatures

**Repudiation:**
- Definition: Denying having performed an action
- Examples: User claims they didn't delete data
- Countermeasures: Audit logging, immutable logs, digital signatures

**Information Disclosure:**
- Definition: Exposing information to unauthorized parties
- Examples: Data breach, log exposure, backup leak
- Countermeasures: Encryption, access controls, data minimization

**Denial of Service:**
- Definition: Making system unavailable
- Examples: DDoS, resource exhaustion, query performance
- Countermeasures: Rate limiting, resource limits, monitoring

**Elevation of Privilege:**
- Definition: Gaining unauthorized permissions
- Examples: Authorization bypass, RBAC misconfiguration
- Countermeasures: Least privilege, RBAC audits, defense in depth

---

## Appendix B: Security Patterns

### B.1 Zero-Trust Architecture

**Principles:**
1. Never trust, always verify
2. Assume breach
3. Verify explicitly
4. Use least privilege access
5. Segment access

**Implementation:**
- Mutual TLS between services (Phase 5+)
- Token validation at every layer
- Network microsegmentation
- Continuous authentication

### B.2 Defense in Depth

**Layer 1: Edge Protection**
- DDoS mitigation
- WAF (future)
- Rate limiting

**Layer 2: Network Security**
- Network policies
- TLS encryption
- Service mesh (future)

**Layer 3: Application Security**
- Input validation
- Authentication/authorization
- CSRF/XSS protection

**Layer 4: Data Security**
- Encryption at rest
- Encryption in transit
- Field-level encryption

**Layer 5: Monitoring & Response**
- Audit logging
- Anomaly detection
- Incident response

---

**Document Status:** Final
**Last Updated:** 2025-12-20
**Next Review:** 2026-03-20 (Quarterly)
**Approved By:** [Pending Security Team Review]
