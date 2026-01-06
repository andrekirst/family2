# Family Hub - Legal Documentation

**Last Updated:** December 20, 2025
**Version:** 1.0
**Status:** Draft - Requires Legal Review

---

## Overview

This folder contains all legal compliance documentation for Family Hub, including policies, agreements, procedures, and compliance frameworks.

**IMPORTANT:** All documents are drafts and require review by qualified legal counsel before use in production.

---

## Quick Start

### For Legal Team

1. Start with [LEGAL-COMPLIANCE-SUMMARY.md](LEGAL-COMPLIANCE-SUMMARY.md) - Overview of all deliverables
2. Review [compliance-checklist.md](compliance-checklist.md) - 93 compliance items
3. Engage attorneys for [legal review](#legal-review-required)

### For Engineering Team

1. Review [Privacy Policy](privacy-policy.md) - Understand data handling requirements
2. Review [compliance-checklist.md](compliance-checklist.md) - Technical implementation items
3. Focus on COPPA consent flow (critical path)

### For Product Team

1. Review [Terms of Service](terms-of-service.md) - Feature restrictions and user obligations
2. Review [Privacy Policy Section 10](privacy-policy.md#10-childrens-privacy-coppa-compliance) - Children's features

---

## Documents in This Folder

### 1. User-Facing Policies (Published)

| Document | Purpose | Length | Status | Priority |
|----------|---------|--------|--------|----------|
| **[terms-of-service.md](terms-of-service.md)** | Legal agreement with users | 10,000 words | Draft | 🔴 Critical |
| **[privacy-policy.md](privacy-policy.md)** | GDPR/COPPA/CCPA privacy notice | 13,000 words | Draft | 🔴 Critical |
| **[cookie-policy.md](cookie-policy.md)** | Cookie usage and consent | 3,500 words | Draft | 🔴 Critical |

**Publication Location:**

- Website footer: Links to all three policies
- Registration flow: Require acceptance of Terms and Privacy Policy
- Cookie banner: Link to Cookie Policy

**Review Frequency:** Annually or upon regulatory changes

---

### 2. Internal Compliance Documents

| Document | Purpose | Audience | Status |
|----------|---------|----------|--------|
| **[compliance-checklist.md](compliance-checklist.md)** | 93 compliance items across 6 regulations | Legal, Engineering, Product | Draft |
| **[data-processing-agreement-template.md](data-processing-agreement-template.md)** | DPA template for third-party processors | Legal, Procurement | Draft |
| **[LEGAL-COMPLIANCE-SUMMARY.md](LEGAL-COMPLIANCE-SUMMARY.md)** | Executive summary of all legal deliverables | CEO, Legal, Product | Complete |

**Access:** Internal only (not published to users)

---

## Key Compliance Frameworks

### GDPR (General Data Protection Regulation)

**Applies to:** EU and UK users

**Key Requirements:**

- Legal basis for processing (consent, contract, legitimate interest)
- Data subject rights (access, deletion, portability, objection)
- Data breach notification (72 hours to supervisory authority)
- Data Protection Impact Assessment (DPIA) for high-risk processing
- International data transfers (Standard Contractual Clauses)

**Status:** 33 compliance items in checklist
**Priority:** 🔴 Critical (before EU launch)

**Documents:**

- [Privacy Policy Section 4](privacy-policy.md#4-your-privacy-rights) - GDPR rights
- [Privacy Policy Section 9](privacy-policy.md#9-international-data-transfers) - Data transfers
- [DPA Template](data-processing-agreement-template.md) - Article 28 compliance
- [Compliance Checklist Section 1](compliance-checklist.md#1-gdpr-compliance-eu-regulation-2016679) - GDPR items

---

### COPPA (Children's Online Privacy Protection Act)

**Applies to:** US users under 13 years old

**Key Requirements:**

- Verifiable parental consent before collecting data from children
- Limited data collection (no email, phone, location, photos)
- No marketing to children
- Parental rights to view, modify, delete child data
- Transition to teen account at age 13

**Status:** 23 compliance items in checklist
**Priority:** 🔴 CRITICAL (highest legal risk)

**Documents:**

- [Privacy Policy Section 10](privacy-policy.md#10-childrens-privacy-coppa-compliance) - COPPA compliance
- [Terms of Service Section 2.3](terms-of-service.md#23-children-under-13-coppa-compliance) - Parental consent
- [Compliance Checklist Section 2](compliance-checklist.md#2-coppa-compliance-15-usc-§§-65016506) - COPPA items
- [Legal Summary Section 6.1](LEGAL-COMPLIANCE-SUMMARY.md#61-identified-legal-risks-specific-to-family-hub) - COPPA risk assessment

**Legal Review Required:** Engage COPPA specialist attorney (budget: $5,000-$10,000)

---

### CCPA / CPRA (California Consumer Privacy Act)

**Applies to:** California residents

**Key Requirements:**

- Right to know what data is collected
- Right to delete personal information
- Right to opt-out of sale (N/A - we don't sell data)
- Right to correct inaccurate data
- No discrimination for exercising rights

**Status:** 16 compliance items in checklist
**Priority:** 🟠 High

**Documents:**

- [Privacy Policy Section 11](privacy-policy.md#11-state-specific-rights) - CCPA rights
- [Compliance Checklist Section 3](compliance-checklist.md#3-ccpa--cpra-compliance-california) - CCPA items

---

## Legal Review Required

**IMPORTANT:** All documents in this folder are DRAFTS and require review by qualified legal counsel before production use.

### Recommended Legal Review

**Phase 0 (Pre-Launch) - CRITICAL:**

| Review Area | Attorney Type | Estimated Cost | Timeline | Status |
|-------------|---------------|----------------|----------|--------|
| **COPPA Compliance** | COPPA specialist (tech/privacy law firm) | $5,000-$10,000 | Week 0-2 | ⬜ Not Started |
| **Privacy Policy** | Privacy attorney (GDPR/CCPA expertise) | $2,000-$4,000 | Week 1-2 | ⬜ Not Started |
| **Terms of Service** | General counsel (tech contracts) | $2,000-$4,000 | Week 1-2 | ⬜ Not Started |
| **DPA Template** | Privacy attorney (GDPR expertise) | $1,000-$2,000 | Week 2-3 | ⬜ Not Started |
| **DPIA Assistance** | Privacy attorney | $1,000-$2,000 | Week 3-4 | ⬜ Not Started |
| **TOTAL** | | **$11,000-$22,000** | Week 0-4 | |

**Phase 1 (Post-Launch):**

| Review Area | Attorney Type | Estimated Cost | Timeline | Status |
|-------------|---------------|----------------|----------|--------|
| **GDPR / EU Compliance** | EU privacy attorney (Ireland-based) | $5,000-$10,000 | Week 5-8 | ⬜ Not Started |
| **State Law Compliance** | US privacy attorney (multi-state) | $2,000-$4,000 | Week 8-10 | ⬜ Not Started |
| **TOTAL** | | **$7,000-$14,000** | Week 5-10 | |

**Total Legal Budget (Pre-Launch + Phase 1):** $18,000-$36,000

**Contact Legal Team:**

- Email: legal@familyhub.app
- Privacy: privacy@familyhub.app

---

## Recommended Law Firms

**COPPA Specialists:**

1. **Kelley Drye & Warren LLP** (New York) - FTC enforcement defense
2. **Hunton Andrews Kurth** (Multiple US offices) - Privacy, COPPA

**GDPR / EU Privacy:**

1. **Mason Hayes & Curran** (Dublin, Ireland) - GDPR, Irish DPC jurisdiction
2. **Bird & Bird** (Multiple EU offices) - GDPR, data transfers

**General Tech/Privacy:**

1. **Foley & Lardner** - Tech startups, privacy
2. **Cooley LLP** - Startup legal, privacy

**Contact Information:** See [Legal Summary Section 9.3](LEGAL-COMPLIANCE-SUMMARY.md#93-recommended-law-firms--attorneys)

---

## Implementation Timeline

### Week 0-4 (Phase 0: Foundation)

**Legal Tasks:**

- [ ] Week 0: Engage COPPA attorney
- [ ] Week 1-2: Legal review of Privacy Policy, Terms, Cookie Policy
- [ ] Week 2-3: COPPA consent flow review
- [ ] Week 3-4: Execute DPAs with Zitadel, Stripe, DigitalOcean
- [ ] Week 3-4: Complete DPIAs for children's data and event chains

**Technical Tasks:**

- [ ] Week 2-3: Implement consent management UI
- [ ] Week 3-4: Implement COPPA parental consent flow
- [ ] Week 3-4: Implement age gate
- [ ] Week 4: Implement cookie consent banner

**Completion Criteria:**

- ✅ All policies legally reviewed and approved
- ✅ COPPA consent flow tested and documented
- ✅ DPAs executed
- ✅ DPIAs completed

---

### Week 5-12 (Phase 1: MVP)

**Legal Tasks:**

- [ ] Week 5-8: Execute Standard Contractual Clauses (SCCs) with US processors
- [ ] Week 5-8: GDPR legal review (EU attorney)
- [ ] Week 8-10: State law compliance review (CCPA, Virginia, Colorado)

**Technical Tasks:**

- [ ] Week 5-8: Implement GDPR data subject rights (access, delete, export)
- [ ] Week 5-8: Implement COPPA parental controls
- [ ] Week 8-10: Implement CCPA consumer rights
- [ ] Week 10-12: Implement data breach notification procedures

**Completion Criteria:**

- ✅ 90% compliance checklist completion
- ✅ All GDPR/COPPA/CCPA rights functional
- ✅ Clean legal sign-off for public launch

---

## Critical Compliance Items

**Must-Have Before Public Launch:**

1. **COPPA Parental Consent** (🔴 CRITICAL)
   - Email + confirmation method implemented
   - Consent logged with timestamps
   - Legal review by COPPA attorney

2. **GDPR Data Subject Rights** (🔴 CRITICAL)
   - Access: Download data (JSON, CSV)
   - Deletion: Delete account + 30-day purge
   - Portability: Export in machine-readable format

3. **Privacy Policy Published** (🔴 CRITICAL)
   - Legally reviewed
   - GDPR/COPPA/CCPA compliant
   - Linked from registration flow and footer

4. **DPAs Executed** (🔴 CRITICAL)
   - Zitadel (authentication)
   - DigitalOcean (cloud hosting)
   - Stripe (payment processing)

5. **Cookie Consent** (🟠 HIGH)
   - Cookie banner on first visit
   - Granular consent (essential, performance, preference)
   - Respect Do Not Track (DNT)

---

## Compliance Monitoring

### Quarterly Review (Q1, Q2, Q3, Q4)

**Legal Team Reviews:**

- Compliance checklist status
- Regulatory changes (GDPR amendments, new state laws)
- Data breach log
- DSR (Data Subject Request) log
- DPA renewals

**Action Items:**

- Update policies if regulations change
- Re-engage legal counsel if major changes

---

### Annual Audit (Q4)

**External Audit (Phase 5+):**

- Third-party privacy audit ($5,000-$10,000)
- Security audit (penetration testing)
- COPPA compliance audit
- SOC 2 Type II (optional, for enterprise sales)

**Budget:** $15,000-$28,000 annually (Phase 5+)

---

## Key Risks & Mitigation

### Risk 1: COPPA Non-Compliance

**Risk Level:** 🔴 CRITICAL

**Consequences:**

- FTC fines up to $46,517 per violation
- Reputational damage
- Loss of user trust

**Mitigation:**

- Engage COPPA specialist attorney (Week 0)
- Implement technical controls (no email, phone, location for under-13)
- Annual COPPA audit (Phase 5+)

**Contingency:** Remove children under 13 feature if compliance too complex

---

### Risk 2: GDPR Data Transfer Violations

**Risk Level:** 🟠 HIGH

**Consequences:**

- EU DPA fines up to €20M or 4% annual revenue
- Loss of EU market access

**Mitigation:**

- Execute Standard Contractual Clauses (SCCs) with US processors
- Conduct Transfer Impact Assessment (TIA)
- Implement supplementary measures (encryption, access controls)
- Consider EU-only data residency (Phase 2+)

**Contingency:** Move EU data processing entirely to EU (DigitalOcean Frankfurt)

---

### Risk 3: Health Data Sensitivity

**Risk Level:** 🟡 MEDIUM

**Consequences:**

- Reputational harm if health data leaked
- User churn

**Mitigation:**

- Separate health data storage with field-level encryption
- Explicit consent for health data collection (GDPR Article 9)
- Disclaim medical advice in Privacy Policy and Terms

**Note:** Family Hub is NOT a HIPAA-covered entity (we're not a healthcare provider).

---

## Frequently Asked Questions (FAQ)

### Q1: Do we need a Data Protection Officer (DPO)?

**A:** Not required initially (< 250 employees, not core activity of large-scale sensitive data processing). Reassess if:
>
- >250 employees
- Large-scale processing of special category data (health data)
- Significant expansion into EU market

### Q2: Can we use Google Analytics?

**A:** Potentially problematic under GDPR (Schrems II concerns with data transfers to US). Alternatives:

- **Plausible Analytics** (privacy-friendly, GDPR compliant, cookieless)
- **Self-hosted analytics** (e.g., Matomo)
- **First-party analytics** (build our own)

**Recommendation:** Use Plausible (no cookies, no personal data) for Phase 1.

### Q3: What's the difference between GDPR and CCPA?

| Aspect | GDPR | CCPA |
|--------|------|------|
| **Jurisdiction** | EU + UK | California, USA |
| **Applies to** | All EU/UK residents' data | California residents |
| **Rights** | Access, delete, restrict, portability, object | Know, delete, opt-out, correct, limit |
| **Response Time** | 30 days (extendable to 60) | 45 days (extendable to 90) |
| **Penalties** | Up to €20M or 4% revenue | Up to $7,500 per violation |
| **Children** | Age 16 (or lower if member state allows) | Age 13 (COPPA applies) |

**Implementation:** Our compliance framework addresses both (mostly overlapping requirements).

### Q4: How do we handle divorce/custody disputes over family data?

**A:**

- Family Administrator has full access (clearly stated in Terms)
- Any member can leave the family and take their personal data
- Respond to court orders (divorce decrees, custody orders)
- Customer support mediates disputes

**Recommendation:** Create separate family accounts for divorced/separated couples.

### Q5: What happens when a child turns 13?

**A:**

- 30 days before 13th birthday: Email parent notification
- On 13th birthday: Automatically convert account from Child → Teen
- Teen account unlocks restricted features (can create events, add to lists)
- Parent retains Family Administrator access

**No action required from user** (automated process).

---

## Contact Information

**Legal & Compliance Team:**

- **Legal Advisor:** legal@familyhub.app
- **Privacy Officer:** privacy@familyhub.app
- **Data Protection Officer (if appointed):** dpo@familyhub.app

**External Counsel:**

**Regulatory Authorities:**

- **FTC (COPPA):** https://www.ftc.gov/COPPA
- **Irish DPC (GDPR):** https://www.dataprotection.ie
- **California AG (CCPA):** https://oag.ca.gov/privacy

---

## Version History

**v1.0 (December 20, 2025):**

- Initial legal documentation suite
- Terms of Service drafted
- Privacy Policy drafted (GDPR/COPPA/CCPA compliant)
- Cookie Policy drafted
- DPA template created
- Compliance checklist created (93 items)
- Legal risk assessment completed

**Next Review:** March 31, 2026 (post-launch)

---

## Acknowledgments

**Prepared By:** Legal Advisor (AI-assisted)

**References:**

- GDPR (EU Regulation 2016/679)
- COPPA (15 U.S.C. §§ 6501–6506, 16 CFR Part 312)
- CCPA (Cal. Civ. Code §§ 1798.100–1798.199)
- UK GDPR (Data Protection Act 2018)
- Standard Contractual Clauses (EU Commission 2021 version)

**Disclaimer:** All documents are drafts and do not constitute legal advice. Consult qualified legal counsel before relying on any information in this folder.

---

**🔴 IMPORTANT: All documents in this folder require legal review by qualified attorneys before use in production.**

**Budget Approved:** [ ] Yes [ ] No
**Legal Counsel Engaged:** [ ] Yes [ ] No
**Compliance Team Assigned:** [ ] Yes [ ] No

---

**END OF README**
