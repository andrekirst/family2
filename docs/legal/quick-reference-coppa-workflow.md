# COPPA Compliance Workflow - Quick Reference

**Purpose:** Step-by-step guide for implementing COPPA-compliant parental consent for children under 13.

**Priority:** 🔴 CRITICAL (highest legal risk)

**Owner:** Engineering + Legal Teams

---

## Overview

The Children's Online Privacy Protection Act (COPPA) requires **verifiable parental consent** before collecting personal information from children under 13.

**Key Requirements:**
1. Obtain verifiable parental consent before collecting data
2. Collect only necessary data (minimal data collection)
3. Provide parental controls (view, modify, delete)
4. No marketing to children

**Consequences of Non-Compliance:**
- FTC fines up to $46,517 per violation
- Reputational damage
- Loss of user trust

---

## Parental Consent Workflow

### Step 1: Age Detection

**When:** User attempts to add a child to the family

**UI Flow:**

```
Parent (Sarah) logs in → Family Settings → Add Family Member

Form:
  First name: [Noah]
  Birthdate: [MM/DD/YYYY]
  [Continue]

Backend checks age:
  IF age >= 13:
    → Create Teen account (no COPPA consent needed)
    → Skip to Step 5 (account activated)

  IF age < 13:
    → Trigger COPPA consent flow
    → Continue to Step 2
```

**Technical Implementation:**

```csharp
// Backend: Age verification
public bool RequiresCoppaConsent(DateTime birthdate)
{
    var age = DateTime.UtcNow.Year - birthdate.Year;
    if (birthdate > DateTime.UtcNow.AddYears(-age)) age--;

    return age < 13; // True if COPPA consent required
}
```

---

### Step 2: Show COPPA Notice to Parent

**When:** Age < 13 detected

**UI Flow:**

```
🛡️ Parental Consent Required

Noah is under 13, so we need your consent under the Children's Online Privacy Protection Act (COPPA).

What we'll collect from Noah:
✅ First name (to assign chores)
✅ Birthdate (to verify age)
✅ Task completion (to award points and badges)

What we WON'T collect:
❌ Email, phone number, or address
❌ Photos or videos
❌ Location data

You can view, modify, or delete Noah's data anytime.

[I Understand - Continue] [Cancel]
```

**Backend Action:**
- Create child account in "Pending Consent" state
- Generate unique consent token (expires in 7 days)
- Send consent email to parent's verified email address

---

### Step 3: Send Consent Email to Parent

**Email Template:**

```
From: Family Hub <noreply@familyhub.app>
To: sarah@example.com
Subject: Confirm consent for Noah (age 7) to use Family Hub

Hi Sarah,

You're adding Noah (birthdate: [redacted]) to your Family Hub account.

Because Noah is under 13, we need your consent under the Children's Online Privacy Protection Act (COPPA).

━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

What We'll Collect from Noah:
✅ First name
✅ Birthdate (for age verification)
✅ Task completion history (for points and badges)

What We WON'T Collect:
❌ Email, phone number, or physical address
❌ Photos or videos of Noah
❌ Geolocation data
❌ Social Security number

━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

By clicking below, you confirm:
- You are Noah's parent or legal guardian
- You consent to our Children's Privacy Policy
- You understand you can view, modify, or delete Noah's data anytime

[I Give Consent] ← (unique link with consent token)

This consent link expires in 7 days.

Questions? Email privacy@familyhub.app or call 1-800-XXX-XXXX.

Learn more: [Link to Children's Privacy Policy]

—Family Hub Team
```

**Technical Implementation:**

```csharp
// Generate unique consent token
public string GenerateConsentToken(Guid parentUserId, Guid childUserId)
{
    var token = Guid.NewGuid().ToString("N");
    var expiration = DateTime.UtcNow.AddDays(7);

    // Store in database
    _db.ConsentTokens.Add(new ConsentToken
    {
        Token = token,
        ParentUserId = parentUserId,
        ChildUserId = childUserId,
        ExpiresAt = expiration,
        CreatedAt = DateTime.UtcNow
    });

    return token;
}

// Generate consent URL
public string GetConsentUrl(string token)
{
    return $"https://familyhub.app/consent/{token}";
}
```

---

### Step 4: Parent Clicks Consent Link

**When:** Parent clicks "I Give Consent" in email

**UI Flow:**

```
URL: https://familyhub.app/consent/{token}

Page:
  🛡️ Parental Consent Confirmation

  You're giving consent for Noah (age 7) to use Family Hub.

  By clicking "Confirm," you acknowledge:
  - You are Noah's parent or legal guardian
  - You've read our Children's Privacy Policy
  - You can revoke consent anytime by deleting Noah's account

  [Confirm Consent] [Cancel]
```

**Backend Action:**
1. Validate consent token (not expired, matches parent + child)
2. Mark child account as "Consent Given"
3. Log consent with timestamp (audit trail)
4. Send confirmation email to parent
5. Activate child account

**Technical Implementation:**

```csharp
// Validate and process consent
public async Task<bool> ProcessConsentAsync(string token)
{
    var consentToken = await _db.ConsentTokens
        .FirstOrDefaultAsync(ct => ct.Token == token && ct.ExpiresAt > DateTime.UtcNow);

    if (consentToken == null)
        return false; // Invalid or expired token

    // Update child account
    var childAccount = await _db.Users.FindAsync(consentToken.ChildUserId);
    childAccount.ConsentStatus = "Consented";
    childAccount.ConsentedAt = DateTime.UtcNow;

    // Log consent (audit trail)
    _db.ConsentLogs.Add(new ConsentLog
    {
        UserId = consentToken.ChildUserId,
        ParentUserId = consentToken.ParentUserId,
        ConsentType = "COPPA",
        Consented = true,
        ConsentedAt = DateTime.UtcNow,
        IpAddress = GetAnonymizedIp(), // Anonymized after 90 days
        UserAgent = GetUserAgent(),
        Method = "email_link",
        Version = "v1.0" // Privacy Policy version
    });

    await _db.SaveChangesAsync();

    // Send confirmation email
    await SendConsentConfirmationEmail(consentToken.ParentUserId, consentToken.ChildUserId);

    return true;
}
```

---

### Step 5: Child Account Activated

**When:** Consent processed successfully

**UI Flow (Parent sees):**

```
✅ Consent Confirmed

Noah's account is now active!

Noah can now:
- See assigned chores
- Earn points and badges
- View family events

You can manage Noah's account in Family Settings.

[Go to Family Settings]
```

**Confirmation Email:**

```
Subject: Noah's account is active

Hi Sarah,

You've successfully given consent for Noah (age 7) to use Family Hub.

Noah can now:
✅ See assigned chores
✅ Earn points and badges for completing tasks
✅ View family calendar events

Your Parental Controls:
- View Noah's activity: Family Settings → Noah's Profile
- Modify Noah's permissions: Family Settings → Permissions
- Delete Noah's data: Family Settings → Delete Account

When Noah turns 13:
- We'll notify you 30 days before his 13th birthday
- His account will automatically convert to a Teen account
- You'll retain full parental access

Questions? Email privacy@familyhub.app

—Family Hub Team
```

---

## Data Collection Restrictions

### What We CAN Collect (with consent)

```
✅ First name
✅ Birthdate (for age verification)
✅ Task completion data (for gamification)
✅ Points and badges earned
✅ Parent-assigned chores
```

### What We CANNOT Collect

```
❌ Email address
❌ Phone number
❌ Physical address (street, city, state, ZIP)
❌ Geolocation data (GPS, IP-based location)
❌ Photos or videos of the child
❌ Social Security number
❌ Biometric data (fingerprints, facial recognition)
❌ Persistent identifiers for advertising (cookies, device IDs)
```

**Technical Implementation:**

```csharp
// Input validation: Block prohibited fields for children
public class ChildUserValidator : AbstractValidator<User>
{
    public ChildUserValidator()
    {
        When(u => u.Age < 13, () =>
        {
            // Allowed fields
            RuleFor(u => u.FirstName).NotEmpty().MaximumLength(50);
            RuleFor(u => u.Birthdate).NotNull();

            // Prohibited fields (must be null)
            RuleFor(u => u.Email).Null().WithMessage("Email cannot be collected from children under 13");
            RuleFor(u => u.PhoneNumber).Null().WithMessage("Phone number cannot be collected from children under 13");
            RuleFor(u => u.Address).Null().WithMessage("Address cannot be collected from children under 13");
            RuleFor(u => u.ProfilePhotoUrl).Null().WithMessage("Photos cannot be collected from children under 13");
        });
    }
}
```

**Database Schema Enforcement:**

```sql
-- Trigger: Prevent prohibited data for children
CREATE OR REPLACE FUNCTION prevent_child_prohibited_data()
RETURNS TRIGGER AS $$
BEGIN
    IF (SELECT age FROM users WHERE id = NEW.id) < 13 THEN
        IF NEW.email IS NOT NULL OR
           NEW.phone_number IS NOT NULL OR
           NEW.address IS NOT NULL OR
           NEW.profile_photo_url IS NOT NULL THEN
            RAISE EXCEPTION 'Cannot collect prohibited data from children under 13 (COPPA violation)';
        END IF;
    END IF;
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

CREATE TRIGGER enforce_coppa_restrictions
BEFORE INSERT OR UPDATE ON users
FOR EACH ROW
EXECUTE FUNCTION prevent_child_prohibited_data();
```

---

## Parental Controls

### 1. View Child's Activity

**Location:** Family Settings → Noah's Profile → Activity

**UI:**

```
Noah's Activity

Tasks Completed (Last 30 Days): 28 tasks
Points Earned: 280 points
Badges Unlocked: 3 badges (On Fire, Early Bird, Champion)

Recent Tasks:
- Feed the dog (Dec 19, 2025) - 10 points
- Brush teeth (Dec 19, 2025) - 5 points
- Take out trash (Dec 18, 2025) - 10 points

[View All Activity]
```

### 2. Modify Child's Data

**Location:** Family Settings → Noah's Profile → Edit

**UI:**

```
Edit Noah's Profile

First Name: [Noah]
Birthdate: [MM/DD/YYYY] (age 7)
Role: Child

Permissions:
☐ Can create calendar events (disabled for children)
☐ Can add shopping list items (disabled for children)
☐ Can view family budget (disabled for children)

[Save Changes]
```

### 3. Delete Child's Data

**Location:** Family Settings → Noah's Profile → Delete Account

**UI:**

```
⚠️ Delete Noah's Account?

This will permanently delete:
- Noah's account
- All task completion history
- All points and badges
- All activity logs

This action cannot be undone.

Why are you deleting Noah's account?
( ) Noah is no longer using Family Hub
( ) I'm revoking parental consent
( ) Privacy concerns
( ) Other: [________]

[Delete Account] [Cancel]
```

**Backend Action:**
- Soft delete: Mark account as deleted (retain for 30 days)
- Hard delete after 30 days: Permanently erase all data
- Send confirmation email to parent

**Technical Implementation:**

```csharp
// Delete child account
public async Task DeleteChildAccountAsync(Guid childUserId, string reason)
{
    var childAccount = await _db.Users.FindAsync(childUserId);

    // Soft delete
    childAccount.DeletedAt = DateTime.UtcNow;
    childAccount.DeletionReason = reason;
    childAccount.ConsentStatus = "Revoked";

    // Log consent withdrawal
    _db.ConsentLogs.Add(new ConsentLog
    {
        UserId = childUserId,
        ConsentType = "COPPA",
        Consented = false,
        WithdrawnAt = DateTime.UtcNow,
        Method = "parent_deletion"
    });

    await _db.SaveChangesAsync();

    // Schedule hard delete after 30 days
    _backgroundJobs.Schedule(() => HardDeleteAccount(childUserId), TimeSpan.FromDays(30));

    // Send confirmation email to parent
    await SendAccountDeletionConfirmationEmail(childAccount.ParentUserId, childUserId);
}
```

---

## No Marketing to Children

### Prohibited Notifications

```
❌ Promotional emails ("Try our new meal planning feature!")
❌ Push notifications about new features
❌ In-app ads or promotions
❌ Targeted recommendations based on behavior
❌ Gamification designed to increase engagement (beyond chore completion)
```

### Permitted Notifications

```
✅ Task reminders ("Time to feed the dog!")
✅ Points earned ("You earned 10 points!")
✅ Messages from parents within the family
✅ Event reminders (calendar events parents created)
✅ Chore completion confirmations ("Great job!")
```

**Technical Implementation:**

```csharp
// Notification filter: Block marketing for children
public async Task SendNotificationAsync(Guid userId, Notification notification)
{
    var user = await _db.Users.FindAsync(userId);

    if (user.Age < 13 && notification.Type == "Marketing")
    {
        // Block marketing notifications for children
        _logger.LogWarning($"Blocked marketing notification for child user {userId} (COPPA compliance)");
        return;
    }

    // Send notification
    await _notificationService.SendAsync(userId, notification);
}
```

---

## Age Transition: Child → Teen (Age 13)

### 30 Days Before 13th Birthday

**Automated Email to Parent:**

```
Subject: Noah will turn 13 soon - Account will convert to Teen

Hi Sarah,

Noah will turn 13 on [January 15, 2026].

When he turns 13:
✅ His account will automatically convert to a Teen account
✅ He'll be able to create calendar events
✅ He'll be able to add items to shopping lists
✅ He can provide an email address (optional)

What stays the same:
- You'll retain full access as Family Administrator
- You can still view, modify, or delete Noah's data
- All his points, badges, and task history are preserved

No action required - this happens automatically.

Questions? Email privacy@familyhub.app

—Family Hub Team
```

### On 13th Birthday

**Backend Action:**
1. Update user role: Child → Teen
2. Unlock restricted features (create events, add to lists)
3. Send notification to parent

**Technical Implementation:**

```csharp
// Background job: Check for age transitions daily
public async Task CheckAgeTransitionsAsync()
{
    var today = DateTime.UtcNow.Date;

    // Find children turning 13 today
    var childrenTurning13 = await _db.Users
        .Where(u => u.Role == "Child" && u.Birthdate.AddYears(13).Date == today)
        .ToListAsync();

    foreach (var child in childrenTurning13)
    {
        // Convert to teen account
        child.Role = "Teen";

        // Log transition
        _db.AgeTransitionLogs.Add(new AgeTransitionLog
        {
            UserId = child.Id,
            FromRole = "Child",
            ToRole = "Teen",
            TransitionedAt = DateTime.UtcNow
        });

        // Notify parent
        await SendAgeTransitionNotificationEmail(child.ParentUserId, child.Id);
    }

    await _db.SaveChangesAsync();
}
```

---

## COPPA Audit Trail

### Consent Log Database

```sql
CREATE TABLE consent_log (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    user_id UUID NOT NULL, -- Child user
    parent_user_id UUID NOT NULL, -- Parent who consented
    consent_type VARCHAR(50) NOT NULL, -- 'COPPA', 'GDPR', etc.
    consented BOOLEAN NOT NULL, -- TRUE if consented, FALSE if revoked
    consented_at TIMESTAMP, -- When consent was given
    withdrawn_at TIMESTAMP, -- When consent was revoked
    ip_address VARCHAR(45), -- Anonymized after 90 days
    user_agent TEXT,
    method VARCHAR(50), -- 'email_link', 'credit_card', etc.
    version VARCHAR(20), -- Privacy Policy version
    created_at TIMESTAMP DEFAULT NOW()
);

CREATE INDEX idx_consent_log_user ON consent_log(user_id);
CREATE INDEX idx_consent_log_parent ON consent_log(parent_user_id);
```

### Audit Report (for FTC Inspection)

**Query:**

```sql
-- Generate COPPA compliance audit report
SELECT
    cl.user_id AS child_user_id,
    u.first_name AS child_first_name,
    DATE_PART('year', AGE(u.birthdate)) AS child_age,
    cl.parent_user_id,
    p.email AS parent_email,
    cl.consented,
    cl.consented_at,
    cl.withdrawn_at,
    cl.method AS consent_method,
    cl.version AS privacy_policy_version
FROM consent_log cl
JOIN users u ON cl.user_id = u.id
JOIN users p ON cl.parent_user_id = p.id
WHERE cl.consent_type = 'COPPA'
ORDER BY cl.consented_at DESC;
```

**Retention:** Keep consent logs for 5 years (demonstrate COPPA compliance to FTC).

---

## Testing Checklist

**Before Public Launch:**

- [ ] Age gate blocks children under 13 from self-registration
- [ ] Parental consent email is sent when adding child < 13
- [ ] Consent link validates token and expires after 7 days
- [ ] Child account is in "Pending Consent" until parent confirms
- [ ] Prohibited data (email, phone, location, photos) cannot be added to child accounts
- [ ] Marketing notifications are blocked for children < 13
- [ ] Parents can view child's activity in Family Settings
- [ ] Parents can delete child's data
- [ ] Age transition (Child → Teen) happens automatically on 13th birthday
- [ ] Consent log is created with timestamp for audit trail
- [ ] Privacy Policy Section 10 (Children's Privacy) is published

---

## Legal Review Checklist

**Engage COPPA Attorney:**
- [ ] Review parental consent flow (email + confirmation)
- [ ] Validate data collection restrictions (no email, phone, location)
- [ ] Audit privacy notice (clear, plain language)
- [ ] Review parental control features (view, modify, delete)
- [ ] Approve consent email template
- [ ] Sign off on COPPA compliance

**Estimated Cost:** $5,000-$10,000

**Timeline:** Week 0-2

---

## Emergency Contacts

**FTC COPPA Inquiries:**
- Website: https://www.ftc.gov/COPPA
- Phone: 1-877-FTC-HELP (1-877-382-4357)
- Email: consumerline@ftc.gov

**Legal Team:**
- Privacy Officer: privacy@familyhub.app
- COPPA Attorney: [Name] ([Email])

**Incident Response (COPPA Violation):**
1. Immediately notify parent via email
2. Delete child's data within 24 hours
3. Document incident and remediation
4. Notify FTC if required

---

## Conclusion

COPPA compliance is CRITICAL to Family Hub's legal foundation. Follow this workflow meticulously and engage a COPPA specialist attorney before public launch.

**Key Takeaways:**
- Verifiable parental consent is required (email + confirmation)
- Collect only minimal data (first name, birthdate, tasks)
- No email, phone, location, photos for children < 13
- No marketing to children
- Parents have full control (view, modify, delete)
- Age transition at 13 is automatic

**Next Steps:**
1. Week 0: Engage COPPA attorney
2. Week 3-4: Implement parental consent flow
3. Week 4: Test COPPA workflow end-to-end
4. Week 10: Legal sign-off before public launch

---

**Document Version:** 1.0
**Last Updated:** December 20, 2025
**Next Review:** March 31, 2026

---

**END OF COPPA WORKFLOW**
