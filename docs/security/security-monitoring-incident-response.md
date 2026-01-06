# Security Monitoring & Incident Response - Family Hub

**Version:** 1.0  
**Date:** 2025-12-20  
**Status:** Implementation Ready  
**Author:** Security Team (Incident Responder)

---

## Executive Summary

This document defines the security monitoring strategy and incident response procedures for Family Hub. Given the sensitive nature of family data (health records, financial information, children's data), rapid detection and response to security incidents is critical.

**Key Components:**

- **Security Monitoring**: Real-time detection of suspicious activity
- **Incident Response**: Structured process for handling security incidents
- **Communication Plan**: Internal and external notification procedures
- **Post-Incident Analysis**: Learn and improve from incidents

---

## Table of Contents

1. [Security Monitoring Strategy](#1-security-monitoring-strategy)
2. [Anomaly Detection](#2-anomaly-detection)
3. [Incident Classification](#3-incident-classification)
4. [Incident Response Plan](#4-incident-response-plan)
5. [Communication Protocol](#5-communication-protocol)
6. [Post-Incident Analysis](#6-post-incident-analysis)

---

## 1. Security Monitoring Strategy

### 1.1 Monitoring Layers

```
┌─────────────────────────────────────────────────────────────────┐
│                   Security Monitoring Stack                      │
├─────────────────────────────────────────────────────────────────┤
│                                                                  │
│  Layer 1: Application-Level Monitoring                          │
│  ┌────────────────────────────────────────────────────────┐    │
│  │ - Authentication events (success, failure, lockout)    │    │
│  │ - Authorization failures (403 errors)                  │    │
│  │ - GraphQL query anomalies (depth, cost, frequency)     │    │
│  │ - Data access patterns (unusual queries)               │    │
│  │ - Event bus message integrity                          │    │
│  │ Tool: Custom instrumentation + Loki logs               │    │
│  └────────────────────────────────────────────────────────┘    │
│                          ↓                                       │
│  Layer 2: Infrastructure Monitoring                              │
│  ┌────────────────────────────────────────────────────────┐    │
│  │ - Pod restarts and crashes                             │    │
│  │ - Resource exhaustion (CPU, memory, disk)              │    │
│  │ - Network traffic anomalies                            │    │
│  │ - Failed deployments                                   │    │
│  │ Tool: Prometheus + Grafana                             │    │
│  └────────────────────────────────────────────────────────┘    │
│                          ↓                                       │
│  Layer 3: Network Monitoring                                     │
│  ┌────────────────────────────────────────────────────────┐    │
│  │ - Unusual traffic patterns (DDoS, port scanning)       │    │
│  │ - Geo-location anomalies                               │    │
│  │ - TLS/SSL handshake failures                           │    │
│  │ Tool: NGINX Ingress logs + CloudFlare (future)         │    │
│  └────────────────────────────────────────────────────────┘    │
│                          ↓                                       │
│  Layer 4: Database Monitoring                                    │
│  ┌────────────────────────────────────────────────────────┐    │
│  │ - Failed login attempts                                │    │
│  │ - Unusual queries (full table scans, cross-schema)     │    │
│  │ - Row-Level Security (RLS) violations                  │    │
│  │ - Backup integrity                                     │    │
│  │ Tool: PostgreSQL logs + pg_stat_statements             │    │
│  └────────────────────────────────────────────────────────┘    │
│                                                                  │
└─────────────────────────────────────────────────────────────────┘
```

### 1.2 Audit Logging Requirements

**Events to Log:**

| Event Type | Severity | Retention | Example |
|------------|----------|-----------|---------|
| **Authentication Events** | INFO/WARN | 90 days | User login, logout, failed login |
| **Authorization Failures** | WARN | 90 days | User accessing unauthorized resource |
| **Data Access** | INFO | 30 days | Calendar event viewed, task updated |
| **Data Mutations** | INFO | 365 days | Event created, prescription deleted |
| **Admin Actions** | WARN | 365 days | User role change, family group deleted |
| **Security Events** | ERROR | 365 days | SQL injection attempt, XSS detected |
| **System Errors** | ERROR | 30 days | Service crash, database connection failure |

**Log Format (Structured JSON):**

```json
{
  "timestamp": "2025-12-20T10:30:00.123Z",
  "level": "WARN",
  "event_type": "authentication_failure",
  "service": "auth-service",
  "user_id": null,
  "email": "attacker@example.com",
  "source_ip": "203.0.113.45",
  "user_agent": "Mozilla/5.0...",
  "reason": "invalid_password",
  "attempt_count": 3,
  "family_group_id": null,
  "trace_id": "abc123def456",
  "span_id": "xyz789"
}
```

**Log Storage:**

- **Short-term (30 days)**: Loki (fast queries)
- **Long-term (1 year)**: S3-compatible storage (compliance)
- **Real-time**: Prometheus alerts

### 1.3 Security Metrics

**Prometheus Metrics:**

```prometheus
# Authentication metrics
auth_login_attempts_total{status="success|failure"}
auth_login_failures_total by (email, source_ip)
auth_account_lockouts_total

# Authorization metrics
http_requests_total{status="403"}
graphql_authorization_failures_total

# Attack detection metrics
waf_blocks_total{rule="sql_injection|xss|csrf"}
rate_limit_exceeded_total by (source_ip, endpoint)

# Data access metrics
database_queries_total{type="read|write|delete"}
sensitive_data_access_total{table="health.prescriptions|finance.expenses"}

# Security events
security_incidents_total{severity="critical|high|medium|low"}
vulnerability_scans_total{status="pass|fail"}
```

### 1.4 Real-Time Alerting

**Prometheus Alert Rules:**

```yaml
# security-alerts.yaml
groups:
  - name: security_alerts
    interval: 30s
    rules:
      # Brute force attack detection
      - alert: BruteForceAttack
        expr: |
          rate(auth_login_failures_total[5m]) > 10
        for: 2m
        labels:
          severity: critical
          team: security
        annotations:
          summary: "Brute force attack detected from {{ $labels.source_ip }}"
          description: "More than 10 failed login attempts per minute from {{ $labels.source_ip }}"
          runbook: "https://docs.familyhub.com/runbooks/brute-force-response"

      # Unusual authorization failures
      - alert: UnusualAuthorizationFailures
        expr: |
          rate(http_requests_total{status="403"}[10m]) > 50
        for: 5m
        labels:
          severity: high
          team: security
        annotations:
          summary: "High rate of authorization failures detected"
          description: "More than 50 authorization failures per 10 minutes"

      # SQL injection attempt
      - alert: SQLInjectionAttempt
        expr: |
          rate(waf_blocks_total{rule="sql_injection"}[5m]) > 0
        labels:
          severity: critical
          team: security
        annotations:
          summary: "SQL injection attack detected"
          description: "WAF blocked SQL injection attempt from {{ $labels.source_ip }}"

      # Mass data access
      - alert: MassDataAccess
        expr: |
          rate(database_queries_total[5m]) > 1000
        for: 5m
        labels:
          severity: high
          team: sre
        annotations:
          summary: "Unusual database query volume"
          description: "More than 1000 database queries per 5 minutes"

      # Sensitive data access outside business hours
      - alert: SensitiveDataAccessAfterHours
        expr: |
          sensitive_data_access_total{table=~"health.*|finance.*"} > 0
          and hour() < 6 or hour() > 22
        labels:
          severity: medium
          team: security
        annotations:
          summary: "Sensitive data accessed outside business hours"
          description: "Health or finance data accessed between 10 PM and 6 AM"
```

---

## 2. Anomaly Detection

### 2.1 Behavioral Anomaly Detection

**User Behavior Analytics (UBA):**

| Anomaly | Detection Method | Threshold | Alert |
|---------|------------------|-----------|-------|
| **Login from new location** | GeoIP comparison | >1000km from usual | Medium |
| **Login from new device** | User-agent fingerprint | First time device | Low |
| **Unusual data access volume** | Query count deviation | >3 std dev from avg | High |
| **Access to unusual data** | Cross-family access attempt | Any unauthorized | Critical |
| **Rapid sequential queries** | Request rate | >100 req/min per user | Medium |

**Example: Detect Login from New Country**

```sql
-- PostgreSQL query to detect anomalous logins
SELECT 
  user_id,
  email,
  source_ip,
  country,
  previous_country,
  login_time
FROM (
  SELECT 
    user_id,
    email,
    source_ip,
    country,
    LAG(country) OVER (PARTITION BY user_id ORDER BY login_time) as previous_country,
    login_time
  FROM auth.login_events
) t
WHERE country != previous_country
  AND previous_country IS NOT NULL
  AND login_time > NOW() - INTERVAL '1 hour';
```

### 2.2 Statistical Anomaly Detection

**Baseline Metrics (Normal Behavior):**

```
Average login attempts per hour: 150 ± 30
Average database queries per minute: 500 ± 100
Average API requests per user per hour: 50 ± 20
Average event bus messages per minute: 100 ± 25
```

**Anomaly Thresholds:**

- **+2 std dev**: Warning alert (investigate)
- **+3 std dev**: Critical alert (immediate action)
- **-2 std dev**: Potential service degradation

**Prometheus Query (Anomaly Detection):**

```promql
# Detect API request rate anomaly (>3 std dev)
(
  rate(http_requests_total[5m]) 
  - 
  avg_over_time(rate(http_requests_total[5m])[1h:5m])
) 
/ 
stddev_over_time(rate(http_requests_total[5m])[1h:5m])
> 3
```

---

## 3. Incident Classification

### 3.1 Incident Severity Levels

| Severity | Definition | Response Time | Escalation | Examples |
|----------|------------|---------------|------------|----------|
| **SEV-1 (Critical)** | Active security breach, data loss imminent | **15 minutes** | CTO, All Hands | Active data breach, ransomware, RCE exploit |
| **SEV-2 (High)** | Significant security risk, no active breach | **1 hour** | Security Lead, Engineering Manager | SQL injection discovered, authentication bypass found |
| **SEV-3 (Medium)** | Security vulnerability with limited impact | **4 hours** | Security Team | XSS vulnerability, information disclosure |
| **SEV-4 (Low)** | Minor security concern, no immediate risk | **24 hours** | Security Engineer | Missing security header, verbose error message |

### 3.2 Incident Types

**Attack Incidents:**

- **Brute Force Attack**: Repeated failed login attempts
- **SQL Injection**: Malicious SQL code injection
- **XSS Attack**: Cross-site scripting attempt
- **CSRF Attack**: Cross-site request forgery
- **DDoS Attack**: Distributed denial of service
- **Data Exfiltration**: Unauthorized data access and export

**Compromise Incidents:**

- **Account Takeover**: Unauthorized access to user account
- **Credential Theft**: Stolen passwords or API keys
- **Malware Infection**: Compromised server or container
- **Insider Threat**: Malicious employee activity

**Availability Incidents:**

- **Service Outage**: Security-related service downtime
- **Data Loss**: Accidental or malicious data deletion
- **Ransomware**: Data encrypted by attacker

---

## 4. Incident Response Plan

### 4.1 Incident Response Phases

```
┌─────────────────────────────────────────────────────────────────┐
│              Incident Response Lifecycle (NIST)                  │
├─────────────────────────────────────────────────────────────────┤
│                                                                  │
│  1. PREPARATION                                                  │
│     - Incident response plan documented                          │
│     - Team roles assigned                                        │
│     - Tools and access ready                                     │
│     - Runbooks prepared                                          │
│     ↓                                                            │
│  2. DETECTION & ANALYSIS                                         │
│     - Monitor alerts (Prometheus, Loki)                          │
│     - Triage incident (severity, scope)                          │
│     - Collect evidence (logs, network traces)                    │
│     - Determine root cause                                       │
│     ↓                                                            │
│  3. CONTAINMENT                                                  │
│     - Short-term: Isolate affected systems                       │
│     - Long-term: Apply patches, rotate credentials               │
│     - Prevent further damage                                     │
│     ↓                                                            │
│  4. ERADICATION                                                  │
│     - Remove attacker access                                     │
│     - Delete malware/backdoors                                   │
│     - Fix vulnerabilities                                        │
│     ↓                                                            │
│  5. RECOVERY                                                     │
│     - Restore services from clean backups                        │
│     - Monitor for re-infection                                   │
│     - Return to normal operations                                │
│     ↓                                                            │
│  6. POST-INCIDENT ACTIVITY                                       │
│     - Document lessons learned                                   │
│     - Update security controls                                   │
│     - Improve detection/response                                 │
│                                                                  │
└─────────────────────────────────────────────────────────────────┘
```

### 4.2 Incident Response Team Roles

| Role | Responsibilities | Contact |
|------|------------------|---------|
| **Incident Commander** | Overall coordination, decision-making | CTO |
| **Security Lead** | Technical investigation, containment | Security Team Lead |
| **Communications Lead** | Internal and external communications | Marketing/PR |
| **Legal Counsel** | Legal implications, compliance | Legal Team |
| **Technical Responders** | Forensics, remediation, recovery | DevOps/SRE |

### 4.3 Incident Response Playbook: Data Breach

**Scenario:** Unauthorized access to health records detected

**Step 1: Detection (0-15 min)**

```bash
# Alert received: Unusual database query pattern
# Prometheus alert: MassDataAccess

# Action: Security engineer investigates logs
kubectl logs -n family-hub health-service-xxx --since=1h | grep "SELECT.*prescriptions"

# Confirm: Unauthorized query detected
# Query: SELECT * FROM health.prescriptions WHERE 1=1
# Source IP: 203.0.113.45
# User: attacker@example.com (compromised account)
```

**Step 2: Containment (15-30 min)**

```bash
# Immediate actions:
# 1. Disable compromised account
psql -U postgres -d familyhub -c "UPDATE auth.users SET is_active = false WHERE email = 'attacker@example.com';"

# 2. Block source IP at NGINX Ingress
kubectl annotate ingress familyhub-ingress \
  nginx.ingress.kubernetes.io/whitelist-source-range="0.0.0.0/0,!203.0.113.45/32"

# 3. Force logout all sessions for compromised account
redis-cli DEL session:attacker@example.com

# 4. Enable enhanced monitoring
kubectl set env deployment/health-service ENHANCED_LOGGING=true
```

**Step 3: Investigation (30-120 min)**

```bash
# Collect forensic evidence
# 1. Export all logs for compromised account
kubectl logs -n family-hub --all-containers=true --timestamps \
  --selector=app.kubernetes.io/part-of=family-hub \
  --since=24h \
  | grep "attacker@example.com" > /tmp/incident-logs.txt

# 2. Database query audit
psql -U postgres -d familyhub -c "
SELECT 
  timestamp,
  user_name,
  database_name,
  query
FROM pg_stat_statements
WHERE user_name = 'attacker@example.com'
  AND timestamp > NOW() - INTERVAL '24 hours'
ORDER BY timestamp DESC;
" > /tmp/db-audit.txt

# 3. Network traffic analysis (if available)
tcpdump -r /var/log/network.pcap 'host 203.0.113.45' > /tmp/network-trace.txt

# 4. Determine scope of breach
# Query: How many records accessed?
psql -U postgres -d familyhub -c "
SELECT COUNT(*) FROM health.prescriptions 
WHERE family_group_id IN (
  SELECT DISTINCT family_group_id 
  FROM audit_logs 
  WHERE user_id = 'attacker-user-id' 
    AND action = 'read' 
    AND table_name = 'health.prescriptions'
);
"
# Result: 150 prescription records accessed (30 families affected)
```

**Step 4: Notification (2-4 hours)**

```bash
# Notify affected users
# Generate list of affected families
psql -U postgres -d familyhub -c "
SELECT DISTINCT 
  fg.id,
  fg.name,
  string_agg(fm.email, ', ') as member_emails
FROM auth.family_groups fg
JOIN auth.family_members fm ON fg.id = fm.family_group_id
WHERE fg.id IN (
  SELECT DISTINCT family_group_id 
  FROM audit_logs 
  WHERE user_id = 'attacker-user-id' 
    AND table_name = 'health.prescriptions'
)
GROUP BY fg.id, fg.name;
" > /tmp/affected-families.csv

# Send notification email via Communication Service
# Subject: Important Security Notice - Unauthorized Access to Your Health Data
# Content: We detected unauthorized access to your health records on [date]. 
#          We have taken immediate action to secure your account...
```

**Step 5: Eradication (4-24 hours)**

```bash
# Fix vulnerability that allowed breach
# Example: Implement stricter Row-Level Security policy

# 1. Deploy fix to staging
kubectl set image deployment/health-service \
  health-service=familyhub/health-service:security-fix-12345 \
  -n family-hub-staging

# 2. Test fix
./security-tests/verify-rls-fix.sh

# 3. Deploy to production
kubectl set image deployment/health-service \
  health-service=familyhub/health-service:security-fix-12345 \
  -n family-hub

# 4. Rotate all credentials
./scripts/rotate-database-credentials.sh
```

**Step 6: Recovery (24-48 hours)**

```bash
# Restore normal operations
# 1. Re-enable affected accounts (after password reset)
# 2. Remove IP blocks
# 3. Monitor for 72 hours

# Monitor for suspicious activity
watch -n 60 'kubectl logs -n family-hub health-service-xxx | grep -c "403\|401"'
```

**Step 7: Post-Incident (48 hours - 1 week)**

- Document incident timeline
- Conduct post-mortem meeting
- Update security controls
- File breach notification (if required by GDPR)

---

## 5. Communication Protocol

### 5.1 Internal Communication

**War Room (Critical Incidents):**

```yaml
Platform: Slack
Channel: #incident-response-YYYY-MM-DD
Participants:
  - Incident Commander (CTO)
  - Security Lead
  - Engineering Manager
  - DevOps/SRE Team
  - Legal (if data breach)

Update Frequency:
  - SEV-1: Every 30 minutes
  - SEV-2: Every 2 hours
  - SEV-3: Daily
  - SEV-4: As needed

Status Update Template:
  Current Status: [Investigating|Contained|Eradicated|Recovered]
  Impact: [Number of users affected, services impacted]
  Actions Taken: [List of actions completed]
  Next Steps: [Planned actions with ETA]
  ETA to Resolution: [Best estimate]
```

**Incident Status Page:**

```markdown
# Incident #2025-12-001: Unauthorized Data Access

**Status:** Investigating  
**Severity:** SEV-1 (Critical)  
**Start Time:** 2025-12-20 10:30 UTC  
**Duration:** 2 hours 15 minutes  

**Impact:**
- Health Service: Degraded performance
- Affected Users: ~30 families
- Data Exposure: 150 prescription records

**Timeline:**
- 10:30: Alert triggered (unusual database query)
- 10:35: Incident confirmed, war room created
- 10:45: Compromised account disabled, IP blocked
- 11:00: Root cause identified (RLS bypass)
- 11:30: Fix deployed to staging
- 12:00: Fix deployed to production
- 12:30: Affected users notified
- 12:45: Monitoring for re-occurrence

**Next Update:** 13:30 UTC
```

### 5.2 External Communication

**User Notification Template (Data Breach):**

```
Subject: Important Security Notice - Action Required

Dear [Family Name],

We are writing to inform you of a security incident that may have affected your Family Hub account.

WHAT HAPPENED:
On December 20, 2025, we detected unauthorized access to our system. An attacker gained access to a limited number of health records, including prescription information for approximately 30 families.

WHAT INFORMATION WAS INVOLVED:
The following information may have been accessed:
- Prescription medication names
- Dosage information
- Prescribing doctor names
- Prescription dates

WHAT WE ARE DOING:
- We immediately disabled the compromised account and blocked the attacker's access
- We have deployed a security fix to prevent similar incidents
- We are working with security experts to investigate the full extent of the breach
- We have notified relevant authorities as required by law

WHAT YOU SHOULD DO:
1. Reset your Family Hub password immediately using the link below
2. Enable two-factor authentication in your account settings
3. Review your account activity for any suspicious entries
4. Contact us if you have any concerns: security@familyhub.com

We sincerely apologize for this incident and are committed to protecting your data. We are taking additional security measures to prevent future incidents.

For more information, please visit: https://familyhub.com/security/incident-2025-12-001

Sincerely,
The Family Hub Security Team
```

**Regulatory Notification (GDPR):**

- **Timeline**: Within 72 hours of breach discovery
- **Recipient**: Data Protection Authority (DPA)
- **Content**: Nature of breach, affected data, remediation steps

### 5.3 Public Disclosure

**Security Advisory (Public):**

- **Platform**: Blog post, security page
- **Timing**: After incident resolution and user notification
- **Content**: Technical details, lessons learned, improvements made

---

## 6. Post-Incident Analysis

### 6.1 Post-Mortem Template

```markdown
# Post-Incident Review: Unauthorized Health Data Access (INC-2025-12-001)

**Date:** 2025-12-22  
**Incident ID:** INC-2025-12-001  
**Severity:** SEV-1 (Critical)  
**Participants:** Security Lead, Engineering Manager, DevOps Team

## Executive Summary

On December 20, 2025, an attacker exploited a vulnerability in the Health Service's Row-Level Security (RLS) policy to access prescription records belonging to 30 families. The attack was detected within 30 minutes, contained within 1 hour, and fully remediated within 24 hours. No financial data was accessed.

## Timeline

| Time (UTC) | Event |
|------------|-------|
| 10:30 | Prometheus alert: MassDataAccess triggered |
| 10:35 | Security engineer confirms unauthorized access |
| 10:40 | War room created, CTO notified |
| 10:45 | Compromised account disabled, IP blocked |
| 11:00 | Root cause identified: RLS bypass via NULL check |
| 11:30 | Security fix deployed to staging |
| 12:00 | Security fix deployed to production |
| 12:30 | Affected users notified via email |
| 14:00 | DPA notified (GDPR requirement) |

## Root Cause Analysis

**Vulnerability:** Row-Level Security (RLS) policy in `health.prescriptions` table had a logic flaw allowing access when `family_group_id IS NULL`.

**Vulnerable Code:**
```sql
CREATE POLICY family_isolation_policy ON health.prescriptions
  USING (
    family_group_id IN (
      SELECT fm.family_group_id
      FROM auth.family_members fm
      WHERE fm.user_id = current_setting('app.current_user_id')::UUID
    )
    OR family_group_id IS NULL  -- FLAW: Allows access to orphaned records
  );
```

**Attack Vector:** Attacker created prescriptions with NULL `family_group_id`, then queried all such records.

## Impact Assessment

- **Users Affected:** 30 families (150 prescription records)
- **Data Exposed:** Medication names, dosages, doctor names, dates
- **Financial Impact:** $0 (no financial data exposed)
- **Reputational Impact:** Medium (proactive user notification)
- **Compliance:** GDPR breach notification filed

## What Went Well

- ✅ Alert triggered within 30 minutes of attack
- ✅ Incident response team mobilized quickly
- ✅ Containment achieved within 1 hour
- ✅ Fix deployed within 24 hours
- ✅ Transparent communication with affected users

## What Went Wrong

- ❌ RLS policy not thoroughly tested before deployment
- ❌ No security code review for database schema changes
- ❌ Insufficient unit tests for authorization logic
- ❌ No alerting for NULL `family_group_id` records

## Lessons Learned

1. **All database schema changes require security review**
2. **RLS policies must be tested with edge cases (NULL values)**
3. **Implement pre-deployment security testing (SAST for SQL)**
4. **Add monitoring for data integrity anomalies**

## Action Items

| Action | Owner | Deadline | Status |
|--------|-------|----------|--------|
| Update RLS policy to reject NULL family_group_id | Security Lead | 2025-12-20 | ✅ Done |
| Add unit tests for RLS policies | DevOps | 2025-12-21 | ✅ Done |
| Implement security code review for DB changes | Engineering Manager | 2025-12-23 | 🔄 In Progress |
| Add SAST rule for SQL authorization checks | Security Lead | 2025-12-30 | ⏳ Pending |
| Create runbook for data breach response | Security Team | 2026-01-10 | ⏳ Pending |
| Quarterly security training for all developers | HR + Security | 2026-01-15 | ⏳ Pending |

```

### 6.2 Continuous Improvement

**Metrics to Track:**

| Metric | Target | Q1 2026 |
|--------|--------|---------|
| **Mean Time to Detect (MTTD)** | < 1 hour | - |
| **Mean Time to Contain (MTTC)** | < 2 hours | - |
| **Mean Time to Resolve (MTTR)** | < 24 hours (SEV-1) | - |
| **Incident Recurrence Rate** | < 5% | - |
| **Post-Mortem Completion Rate** | 100% | - |

**Quarterly Security Review:**

- Review all incidents from past quarter
- Identify trends and patterns
- Update security controls and monitoring
- Conduct tabletop exercises (simulate incidents)

---

## Appendix A: Emergency Contact List

| Role | Name | Phone | Email | Availability |
|------|------|-------|-------|--------------|
| **Incident Commander** | CTO | +1-XXX-XXX-XXXX | cto@familyhub.com | 24/7 |
| **Security Lead** | Security Team Lead | +1-XXX-XXX-XXXX | security@familyhub.com | 24/7 on-call |
| **Engineering Manager** | Engineering Manager | +1-XXX-XXX-XXXX | eng-mgr@familyhub.com | Business hours |
| **Legal Counsel** | Legal Team | +1-XXX-XXX-XXXX | legal@familyhub.com | Business hours |
| **Communications Lead** | Marketing/PR | +1-XXX-XXX-XXXX | pr@familyhub.com | Business hours |

---

## Appendix B: Incident Response Runbooks

**Quick Reference Links:**

- [Brute Force Attack Response](/runbooks/brute-force-response.md)
- [Data Breach Response](/runbooks/data-breach-response.md)
- [DDoS Mitigation](/runbooks/ddos-mitigation.md)
- [Account Takeover Response](/runbooks/account-takeover-response.md)
- [Ransomware Response](/runbooks/ransomware-response.md)

---

**Document Status:** Implementation Ready  
**Last Updated:** 2025-12-20  
**Next Review:** Quarterly (post-incident)  
**Maintained By:** Security Team
