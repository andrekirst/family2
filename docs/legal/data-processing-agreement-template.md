# Data Processing Agreement (DPA) Template

**For Third-Party Service Providers Processing Personal Data on Behalf of Family Hub**

**Version:** 1.0
**Date:** December 20, 2025
**Template Type:** Standard Contractual Clauses (GDPR Compliant)

---

## Purpose of This Template

This Data Processing Agreement (DPA) template is used when Family Hub engages third-party service providers (data processors) who will process personal data on our behalf.

**Use Cases:**
- Cloud infrastructure providers (DigitalOcean, AWS, etc.)
- Authentication services (Zitadel)
- Payment processors (Stripe)
- Email delivery services
- Analytics providers
- Backup and disaster recovery services

**GDPR Requirement:** Article 28 requires a written contract between data controllers (Family Hub) and data processors (service providers).

---

## DATA PROCESSING AGREEMENT

**BETWEEN:**

**Data Controller:**
- **Name:** Family Hub, Inc.
- **Address:** 123 Main Street, Suite 100, Wilmington, DE 19801, USA
- **Contact:** privacy@familyhub.app
- **Role:** Data Controller (determines purposes and means of processing)

**AND**

**Data Processor:**
- **Name:** [SERVICE PROVIDER NAME]
- **Address:** [SERVICE PROVIDER ADDRESS]
- **Contact:** [SERVICE PROVIDER EMAIL]
- **Role:** Data Processor (processes personal data on behalf of Controller)

**Effective Date:** [DATE]

---

## 1. Definitions

**1.1** Terms used in this DPA have the meanings set forth in the GDPR (EU Regulation 2016/679) and the UK GDPR.

**1.2** Key Definitions:

- **"Personal Data"**: Information relating to an identified or identifiable natural person (data subject)
- **"Processing"**: Any operation performed on personal data (collection, storage, use, disclosure, deletion)
- **"Data Subject"**: An identified or identifiable natural person whose personal data is processed
- **"Sub-processor"**: Any third party engaged by Processor to process personal data
- **"GDPR"**: General Data Protection Regulation (EU) 2016/679
- **"UK GDPR"**: UK GDPR as defined by the Data Protection Act 2018
- **"SCCs"**: Standard Contractual Clauses approved by the European Commission

---

## 2. Scope and Purpose of Processing

**2.1 Purpose**

Processor shall process Personal Data only for the following purposes:

- [X] **Cloud Infrastructure Hosting**: Storing and transmitting data for the Family Hub service
- [X] **Authentication Services**: Validating user identities and managing sessions
- [X] **Payment Processing**: Processing subscription payments and managing billing
- [ ] **Email Delivery**: Sending transactional and marketing emails
- [ ] **Analytics**: Measuring service performance and user engagement
- [ ] **Backup and Recovery**: Creating and storing data backups
- [ ] **Other**: [SPECIFY]

**2.2 Nature of Processing**

Processing activities include:

- [X] Collection
- [X] Recording
- [X] Organization
- [X] Structuring
- [X] Storage
- [X] Adaptation or alteration
- [X] Retrieval
- [X] Consultation
- [X] Use
- [X] Disclosure by transmission
- [X] Restriction
- [X] Erasure or destruction

**2.3 Type of Personal Data**

Categories of personal data processed:

- [X] **Identification Data**: Name, email address, phone number, date of birth
- [X] **Account Data**: Username, password (hashed), account settings
- [X] **Contact Data**: Email, phone number, physical address
- [X] **Payment Data**: Credit card details, billing address, transaction history
- [X] **Usage Data**: IP address, browser type, device information, log files
- [X] **Content Data**: Calendar events, tasks, shopping lists, meal plans, budget data, documents
- [X] **Children's Data**: First name, birthdate, task completion (for users under 13)
- [ ] **Sensitive Data**: [SPECIFY IF ANY - e.g., health data, biometric data]

**2.4 Categories of Data Subjects**

- Family Hub users (parents, guardians)
- Children and teens (ages 5-17)
- Extended family members
- Guest users (babysitters, caregivers)

**2.5 Duration of Processing**

Processing shall continue for the duration of the Master Service Agreement between Controller and Processor, unless earlier termination occurs.

---

## 3. Processor's Obligations

**3.1 Compliance with Instructions**

Processor shall:
- Process Personal Data only on documented instructions from Controller
- Not process Personal Data for any other purpose
- Immediately inform Controller if instructions violate GDPR or UK GDPR

**Controller's Instructions:**
- Process data as necessary to provide the agreed services (see Section 2.1)
- Respond to data subject requests forwarded by Controller
- Delete or return data upon termination (see Section 10)
- Comply with Controller's written instructions provided via email to [processor contact]

**3.2 Confidentiality**

Processor shall:
- Ensure all personnel with access to Personal Data are bound by confidentiality obligations
- Implement confidentiality agreements with employees and contractors
- Limit access to Personal Data to those who need it to perform services

**3.3 Security Measures**

Processor shall implement appropriate technical and organizational measures to ensure a level of security appropriate to the risk, including:

**Technical Measures:**
- [X] Encryption of data in transit (TLS 1.3 or higher)
- [X] Encryption of data at rest (AES-256 or equivalent)
- [X] Pseudonymization where applicable
- [X] Secure key management
- [X] Regular security testing and vulnerability scanning
- [X] Intrusion detection and prevention systems

**Organizational Measures:**
- [X] Access controls (role-based access, least privilege principle)
- [X] Multi-factor authentication for administrative access
- [X] Security awareness training for personnel
- [X] Incident response plan
- [X] Business continuity and disaster recovery plan
- [X] Regular security audits (at least annually)

**Certifications (if applicable):**
- [ ] ISO 27001
- [X] SOC 2 Type II
- [ ] PCI DSS (for payment processors)
- [X] GDPR-compliant

**3.4 Sub-processors**

**3.4.1** Processor may engage sub-processors only with Controller's prior written consent.

**3.4.2** Current sub-processors approved by Controller:

| Sub-processor Name | Service | Location | Safeguards |
|--------------------|---------|----------|------------|
| [Name] | [Service] | [Country] | [SCCs, Adequacy Decision, etc.] |
| [Name] | [Service] | [Country] | [SCCs, Adequacy Decision, etc.] |

**3.4.3** Processor shall:
- Notify Controller of any intended changes (additions or replacements) to sub-processors at least **30 days** in advance
- Allow Controller to object to the new sub-processor
- Impose the same data protection obligations on sub-processors as in this DPA
- Remain fully liable to Controller for sub-processor's performance

**3.5 Data Subject Rights**

**3.5.1** Processor shall assist Controller in responding to data subject requests (DSRs):

- **Right of access** (Article 15): Provide access to personal data
- **Right to rectification** (Article 16): Correct inaccurate data
- **Right to erasure** ("right to be forgotten") (Article 17): Delete data
- **Right to restriction** (Article 18): Limit processing
- **Right to data portability** (Article 20): Export data in machine-readable format
- **Right to object** (Article 21): Stop processing for legitimate interests or direct marketing
- **Automated decision-making** (Article 22): Human review of automated decisions

**3.5.2** Processor shall:
- Respond to Controller's DSR requests within **10 business days**
- Provide necessary information and assistance to enable Controller to respond to DSRs within legal timeframes (30 days for GDPR, 45 days for CCPA)
- Not respond directly to data subjects without Controller's prior authorization

**3.6 Data Protection Impact Assessments (DPIAs)**

Processor shall assist Controller in conducting Data Protection Impact Assessments (DPIAs) when required by Article 35 GDPR by:
- Providing information about processing activities
- Describing security measures in place
- Assessing risks to data subjects
- Identifying mitigation measures

**3.7 Data Breach Notification**

**3.7.1** Processor shall notify Controller **without undue delay** and **within 24 hours** of becoming aware of a personal data breach.

**3.7.2** Breach notification shall include:
- Nature of the breach (type of data affected)
- Categories and approximate number of data subjects affected
- Categories and approximate number of personal data records affected
- Contact point for more information
- Likely consequences of the breach
- Measures taken or proposed to address the breach and mitigate harm

**3.7.3** Processor shall:
- Investigate the breach and provide regular updates to Controller
- Preserve evidence (logs, forensic data) for investigation
- Cooperate with Controller's breach response and remediation efforts

**3.8 Audits and Inspections**

**3.8.1** Controller (or an independent auditor) may audit Processor's compliance with this DPA:
- Upon reasonable notice (at least **30 days**)
- During normal business hours
- No more than **once per year** (unless triggered by a data breach or regulatory investigation)

**3.8.2** Processor shall:
- Make available all information necessary to demonstrate compliance
- Allow and contribute to audits and inspections
- Provide access to relevant facilities, systems, and personnel
- Remediate any non-compliance identified within **60 days**

**3.8.3** Audit costs:
- Controller bears audit costs
- If audit reveals material non-compliance, Processor bears costs

---

## 4. Controller's Obligations

**4.1** Controller shall:
- Ensure it has a lawful basis for processing (GDPR Article 6)
- Obtain necessary consents from data subjects (e.g., COPPA parental consent)
- Provide clear and complete instructions to Processor
- Respond to data subject requests within legal timeframes
- Notify Processor of any changes to processing instructions

**4.2** Controller warrants that:
- It has authority to enter into this DPA
- Processing instructions comply with applicable laws
- It has obtained all necessary consents and provided required notices

---

## 5. International Data Transfers

**5.1 Transfers Outside the EEA**

If Processor transfers personal data outside the European Economic Area (EEA) or UK:

**5.1.1** Processor shall ensure one of the following safeguards:

- [X] **Standard Contractual Clauses (SCCs)**: Use EU-approved SCCs (2021 version)
- [ ] **Adequacy Decision**: Transfer to a country with an adequacy decision
- [ ] **Binding Corporate Rules (BCRs)**: If Processor has approved BCRs
- [ ] **Derogations**: Specific derogations (Article 49) with Controller consent

**5.1.2** If using SCCs:
- The SCCs in **Annex A** (attached) are incorporated into this DPA
- Processor shall complete and sign the SCCs
- Processor shall ensure sub-processors also implement SCCs

**5.2 Transfers to the United States**

**5.2.1** If Processor is in the US or uses US sub-processors:

- **EU-U.S. Data Privacy Framework**: If Processor is certified, indicate certification number: [CERTIFICATION NUMBER]
- **Standard Contractual Clauses**: Attached in Annex A
- **Supplementary Measures**: Processor shall implement additional safeguards (encryption, access controls) as required by Schrems II

**5.3 Notification of Government Requests**

Processor shall:
- Notify Controller immediately if it receives a government request for access to personal data (unless legally prohibited)
- Challenge overly broad or unlawful requests
- Provide only the minimum data necessary to comply with valid legal requests

---

## 6. Data Security and Incident Response

**6.1 Security Standards**

Processor shall maintain security certifications:
- [ ] ISO 27001 (Information Security Management)
- [X] SOC 2 Type II (Security, Availability, Confidentiality)
- [ ] PCI DSS Level 1 (for payment data)

**6.2 Security Audits**

Processor shall:
- Conduct annual penetration testing by independent third parties
- Perform quarterly vulnerability scans
- Remediate critical vulnerabilities within **7 days**, high within **30 days**

**6.3 Incident Response**

Processor shall maintain a documented incident response plan including:
- Detection and containment procedures
- Notification protocols (24-hour notification to Controller)
- Forensic investigation capabilities
- Recovery and remediation processes

**6.4 Business Continuity**

Processor shall:
- Maintain disaster recovery plan with **RTO ≤ 4 hours**, **RPO ≤ 1 hour**
- Test disaster recovery plan at least annually
- Maintain geographically distributed backups

---

## 7. Data Retention and Deletion

**7.1 Retention Period**

Processor shall retain Personal Data only as long as necessary to provide the services or as required by law.

**7.2 Deletion Upon Termination**

Upon termination or expiration of the Master Service Agreement, Processor shall (at Controller's choice):

**Option 1: Return Data**
- Export all Personal Data in a structured, commonly used format (JSON, CSV)
- Deliver to Controller within **30 days** of termination

**Option 2: Delete Data**
- Securely delete all Personal Data within **30 days** of termination
- Provide written certification of deletion
- Use secure deletion methods (NIST 800-88 compliant)

**7.3 Retention for Legal Obligations**

Processor may retain Personal Data to the extent required by law (e.g., tax records, audit logs) but shall:
- Inform Controller of retention requirements
- Continue to protect retained data under this DPA
- Delete data when legal obligation ends

**7.4 Backup Retention**

Backups containing Personal Data shall be:
- Deleted within **90 days** of termination
- Protected with same security measures as production data

---

## 8. Liability and Indemnification

**8.1 Processor Liability**

Processor shall be liable for damages caused by:
- Non-compliance with GDPR obligations specific to data processors (Article 28)
- Acting outside or contrary to Controller's lawful instructions

**8.2 Limitation of Liability**

**8.2.1** Processor's total liability under this DPA is limited to:
- **[AMOUNT]** per incident, or
- **[AMOUNT]** per year in aggregate, or
- The fees paid by Controller to Processor in the 12 months preceding the claim

**Exception:** No limitation for:
- Gross negligence or willful misconduct
- Violation of data subject rights
- Regulatory fines or penalties

**8.3 Indemnification**

Processor shall indemnify Controller for:
- Fines or penalties imposed due to Processor's non-compliance with GDPR
- Third-party claims arising from Processor's breach of this DPA
- Data breaches caused by Processor's negligence

---

## 9. Regulatory Cooperation

**9.1 Supervisory Authority Requests**

If a supervisory authority (e.g., ICO, CNIL, DPC) contacts Processor regarding Personal Data:
- Processor shall notify Controller within **24 hours**
- Processor shall cooperate with Controller's response
- Processor shall not respond without Controller's authorization (unless legally required)

**9.2 Regulatory Investigations**

In the event of a regulatory investigation:
- Processor shall provide all requested information to Controller
- Processor shall make employees available for interviews
- Processor shall preserve all relevant evidence

---

## 10. Termination

**10.1 Termination for Cause**

Controller may terminate this DPA immediately if:
- Processor materially breaches this DPA and fails to remedy within **30 days**
- Processor suffers a data breach due to negligence
- Processor refuses to cooperate with an audit

**10.2 Termination of Master Agreement**

If the Master Service Agreement terminates, this DPA terminates simultaneously.

**10.3 Effect of Termination**

Upon termination:
- Processor shall cease all processing of Personal Data (except as required by law)
- Processor shall return or delete all Personal Data (per Section 7.2)
- Processor shall provide certification of deletion
- Sections 8 (Liability), 9 (Regulatory Cooperation), and 11 (General) survive termination

---

## 11. General Provisions

**11.1 Governing Law**

This DPA is governed by the laws of:
- **[JURISDICTION]** (e.g., State of Delaware, USA)
- GDPR and UK GDPR (where applicable)

**11.2 Dispute Resolution**

Disputes shall be resolved by:
- **Negotiation**: Good faith discussions for 30 days
- **Mediation**: Binding mediation if negotiation fails
- **Arbitration or Litigation**: Per the Master Service Agreement

**11.3 Order of Precedence**

In case of conflict:
1. This DPA
2. Standard Contractual Clauses (if applicable)
3. Master Service Agreement

**11.4 Amendments**

Amendments to this DPA must be:
- In writing
- Signed by authorized representatives of both parties
- Compliant with GDPR requirements

**11.5 Severability**

If any provision is found invalid, the remaining provisions remain in full force.

**11.6 Entire Agreement**

This DPA, together with the Master Service Agreement and SCCs (if applicable), constitutes the entire agreement regarding data processing.

---

## SIGNATURES

**CONTROLLER (Family Hub, Inc.):**

Signature: _______________________________
Name: [PRINTED NAME]
Title: [TITLE]
Date: _______________

**PROCESSOR ([SERVICE PROVIDER NAME]):**

Signature: _______________________________
Name: [PRINTED NAME]
Title: [TITLE]
Date: _______________

---

## ANNEX A: Standard Contractual Clauses (SCCs)

**[Attached separately: EU Commission Standard Contractual Clauses (2021 version)]**

**Module Selected:**
- [ ] Module One: Controller to Controller
- [X] Module Two: Controller to Processor
- [ ] Module Three: Processor to Processor
- [ ] Module Four: Processor to Controller

**Optional Clauses:**
- [X] Clause 7 (Docking clause): Allowed
- [X] Clause 9 (Use of sub-processors): General authorization with notification
- [X] Clause 11 (Redress): Data subjects can invoke SCCs
- [X] Clause 17 (Governing law): [SPECIFY EU MEMBER STATE LAW]
- [X] Clause 18 (Choice of forum): Courts of [SPECIFY EU MEMBER STATE]

**Annexes to SCCs:**
- **Annex I.A**: List of parties (Controller and Processor details)
- **Annex I.B**: Description of transfer (see Section 2 of this DPA)
- **Annex I.C**: Competent supervisory authority (e.g., Irish DPC)
- **Annex II**: Technical and organizational measures (see Section 3.3 of this DPA)
- **Annex III**: List of sub-processors (see Section 3.4.2 of this DPA)

---

## ANNEX B: Security Measures

**Detailed list of technical and organizational measures implemented by Processor:**

### 1. Access Control

**Physical Access:**
- [X] Data center access restricted to authorized personnel
- [X] Biometric access controls (fingerprint, facial recognition)
- [X] 24/7 security cameras and monitoring
- [X] Visitor logs and escort requirements

**Logical Access:**
- [X] Role-based access control (RBAC)
- [X] Principle of least privilege
- [X] Multi-factor authentication (MFA) for administrative access
- [X] Regular access reviews (quarterly)

### 2. Transmission Control

- [X] TLS 1.3 encryption for all data in transit
- [X] Certificate pinning for API communications
- [X] VPN for remote access
- [X] Secure file transfer protocols (SFTP, HTTPS)

### 3. Input Control

- [X] Audit logging of all data access and modifications
- [X] Tamper-evident logs (cryptographic hashing)
- [X] Log retention for at least 1 year
- [X] Automated anomaly detection

### 4. Availability Control

- [X] 99.9% uptime SLA
- [X] Redundant infrastructure (multi-zone, multi-region)
- [X] Automated failover
- [X] Regular backups (daily incremental, weekly full)
- [X] Disaster recovery plan (RTO 4 hours, RPO 1 hour)

### 5. Separation Control

- [X] Multi-tenant architecture with logical separation
- [X] Database-level isolation (separate schemas or databases per tenant)
- [X] Network segmentation
- [X] Container isolation (Kubernetes namespaces)

### 6. Encryption

- [X] AES-256 encryption at rest (database, file storage)
- [X] Encrypted backups
- [X] Secure key management (AWS KMS, HashiCorp Vault, or equivalent)
- [X] Key rotation (annually or upon breach)

### 7. Personnel Security

- [X] Background checks for employees with data access
- [X] Confidentiality agreements (NDAs)
- [X] Security awareness training (annually)
- [X] Incident response training

### 8. Incident Response

- [X] 24/7 security operations center (SOC)
- [X] Incident response plan tested annually
- [X] Breach notification within 24 hours
- [X] Forensic capabilities (log analysis, memory dumps)

---

## ANNEX C: Sub-processor List

**Current sub-processors approved by Controller:**

| Sub-processor | Service | Data Processed | Location | Safeguards |
|---------------|---------|----------------|----------|------------|
| [Name] | [Service description] | [Data categories] | [Country] | [SCCs / Adequacy Decision] |
| [Name] | [Service description] | [Data categories] | [Country] | [SCCs / Adequacy Decision] |

**Notification Process:**
1. Processor emails Controller at privacy@familyhub.app
2. Email includes: Sub-processor name, service, data categories, location, safeguards
3. Controller has **30 days** to object
4. If Controller objects, parties negotiate alternative or terminate services

---

## Document Control

**Version History:**
- v1.0 (December 20, 2025): Initial template

**Next Review Date:** December 20, 2026 (annual review)

**Owner:** Family Hub Legal Team (legal@familyhub.app)

**Approved By:**
- Chief Privacy Officer
- General Counsel

---

**END OF DATA PROCESSING AGREEMENT**
