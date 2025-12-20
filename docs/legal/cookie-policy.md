# Family Hub - Cookie Policy

**Effective Date:** January 1, 2026
**Last Updated:** December 20, 2025
**Version:** 1.0

---

## What Are Cookies?

Cookies are small text files that websites and apps place on your device (computer, phone, tablet) when you visit them. They help websites remember your preferences, keep you logged in, and provide a better user experience.

**Types of data stored:**
- Session identifiers (keep you logged in)
- Preferences (dark mode, language, timezone)
- Analytics data (pages visited, features used)

**Cookies are NOT:**
- ❌ Viruses or malware
- ❌ Programs that run on your device
- ❌ Able to access other files on your device

---

## How Family Hub Uses Cookies

We use cookies to provide, improve, and secure our Service. Here's a breakdown of each type:

### 1. Essential Cookies (Required)

**Purpose:** These cookies are necessary for the Service to work. You cannot disable them.

**What they do:**
- **Authentication**: Keep you logged in to your account
- **Session management**: Maintain your active session
- **Security**: Prevent CSRF (Cross-Site Request Forgery) attacks
- **Load balancing**: Route your requests to the correct server

**Examples:**

| Cookie Name | Purpose | Duration | Domain |
|-------------|---------|----------|--------|
| `fh_session` | Session identifier | Session | familyhub.app |
| `fh_csrf` | CSRF protection token | Session | familyhub.app |
| `fh_auth` | OAuth authentication state | 1 hour | familyhub.app |

**Can you disable them?** No. Without these cookies, you cannot log in or use the Service.

---

### 2. Performance Cookies (Optional)

**Purpose:** These cookies help us understand how you use the Service so we can improve it.

**What they do:**
- **Analytics**: Track page views, feature usage, user flows
- **Error monitoring**: Detect bugs and crashes
- **Performance monitoring**: Measure page load times

**Examples:**

| Cookie Name | Purpose | Duration | Domain |
|-------------|---------|----------|--------|
| `fh_analytics` | Analytics tracking ID | 2 years | familyhub.app |
| `plausible_ignore` | Opt-out of Plausible Analytics | Persistent | plausible.io |

**Third-Party Service:**
- **Plausible Analytics**: Privacy-friendly analytics (GDPR compliant, no personal data)
- **Privacy Policy**: https://plausible.io/privacy

**Can you disable them?** Yes. Go to Settings → Privacy → Cookie Preferences → Performance Cookies: Off.

**What happens if disabled?**
- We won't track which pages you visit
- We'll still fix bugs reported by other users
- Service functionality is not affected

---

### 3. Preference Cookies (Optional)

**Purpose:** These cookies remember your preferences so you don't have to set them every time.

**What they do:**
- **Theme**: Remember dark mode vs. light mode
- **Language**: Remember your language preference (English, Spanish, etc.)
- **Timezone**: Display events in your local timezone
- **Layout**: Remember your dashboard layout (widgets you've added/removed)

**Examples:**

| Cookie Name | Purpose | Duration | Domain |
|-------------|---------|----------|--------|
| `fh_theme` | Dark mode preference | 1 year | familyhub.app |
| `fh_lang` | Language preference | 1 year | familyhub.app |
| `fh_tz` | Timezone offset | 1 year | familyhub.app |

**Can you disable them?** Yes. Go to Settings → Privacy → Cookie Preferences → Preference Cookies: Off.

**What happens if disabled?**
- You'll see default theme (light mode) every time
- Events display in UTC instead of your timezone
- You'll need to set language preference each session

---

### 4. Marketing Cookies (We Don't Use These)

**Family Hub does NOT use marketing or advertising cookies.**

**We do NOT:**
- ❌ Track you across other websites
- ❌ Build advertising profiles
- ❌ Share data with advertising networks
- ❌ Use Facebook Pixel, Google Ads, or similar tracking

**Why?** We're privacy-first. We don't sell your data or show ads.

---

## Third-Party Cookies

We use a few third-party services that set their own cookies:

### Zitadel (Authentication)

**Purpose:** OAuth 2.0 authentication (login)

**Cookies set:**
- `zitadel_session`: OAuth session identifier
- `zitadel_csrf`: CSRF protection

**Duration:** Session (deleted when you close browser)

**Can you disable?** No. Required for login.

**Privacy Policy:** [Link to Zitadel Privacy Policy]

**Location:** Germany (GDPR compliant)

---

### Stripe (Payment Processing)

**Purpose:** Credit card processing for Premium/Family subscriptions

**Cookies set:**
- `__stripe_mid`: Merchant identifier (fraud detection)
- `__stripe_sid`: Session identifier

**Duration:** 1 year

**Can you disable?** No. Required for payment processing.

**Privacy Policy:** https://stripe.com/privacy

**Compliance:** PCI DSS Level 1 certified

**When set:** Only when you visit the payment page (upgrade to Premium)

---

### Plausible Analytics (Optional)

**Purpose:** Privacy-friendly web analytics

**Cookies set:** None (cookieless analytics)

**Data collected:** Page views, referrers (anonymized, no IP addresses)

**Duration:** N/A (no cookies)

**Can you disable?** Yes. Settings → Privacy → Analytics: Off

**Privacy Policy:** https://plausible.io/privacy

**Compliance:** GDPR compliant, no personal data

---

## How to Manage Cookies

### Option 1: Family Hub Settings (Recommended)

**Control cookies directly in the Service:**

1. Go to **Settings** → **Privacy** → **Cookie Preferences**
2. Toggle each cookie category on/off:
   - Essential Cookies: ✅ (cannot disable)
   - Performance Cookies: ✅/❌
   - Preference Cookies: ✅/❌
3. Click **Save Changes**

**Your choices are saved** (ironically, using a cookie 😊).

---

### Option 2: Browser Settings

**You can block or delete cookies in your browser:**

**Google Chrome:**
1. Settings → Privacy and security → Cookies and other site data
2. Choose "Block all cookies" or "Block third-party cookies"
3. To delete: "See all cookies and site data" → Remove familyhub.app

**Firefox:**
1. Settings → Privacy & Security → Cookies and Site Data
2. Choose "Block cookies" or select exceptions
3. To delete: "Manage Data" → Remove familyhub.app

**Safari:**
1. Preferences → Privacy → Manage Website Data
2. Search for familyhub.app → Remove
3. Or enable "Block all cookies"

**Edge:**
1. Settings → Cookies and site permissions → Manage and delete cookies
2. Choose "Block all cookies" or exceptions
3. To delete: "See all cookies and site data" → Remove familyhub.app

**Mobile (iOS Safari):**
1. Settings → Safari → Block All Cookies

**Mobile (Android Chrome):**
1. Settings → Site settings → Cookies
2. Choose "Block third-party cookies" or "Block all cookies"

**⚠️ Warning:** Blocking all cookies will prevent you from logging in to Family Hub.

---

### Option 3: Do Not Track (DNT)

**We respect your browser's Do Not Track (DNT) setting.**

**If DNT is enabled:**
- Performance cookies are automatically disabled
- Essential cookies remain active (required for functionality)
- Preference cookies respect your choice

**How to enable DNT:**

**Chrome:**
- Settings → Privacy and security → Send a "Do Not Track" request

**Firefox:**
- Settings → Privacy & Security → Send websites a "Do Not Track" signal

**Safari:**
- Preferences → Privacy → Website tracking: "Prevent cross-site tracking"

---

## Cookie Consent Banner

### First Visit

**When you first visit Family Hub, you'll see a cookie consent banner:**

```
🍪 We use cookies to provide and improve our Service.

Essential cookies are required for the Service to work.
Performance and preference cookies help us improve your experience.

[Accept All Cookies] [Customize] [Essential Only]

Learn more: Cookie Policy
```

**Your choices:**

**Accept All Cookies:**
- Essential, performance, and preference cookies enabled
- Recommended for best experience

**Customize:**
- Choose which cookie categories to enable
- Opens detailed cookie preferences

**Essential Only:**
- Only essential cookies enabled
- Performance and preference cookies disabled

### Managing Consent Later

**Change your mind?**
- Go to Settings → Privacy → Cookie Preferences
- Update your choices anytime

---

## Children Under 13

**Special protections for children:**

**We do NOT:**
- ❌ Use tracking cookies for children under 13
- ❌ Use analytics cookies for child accounts
- ❌ Profile children for any purpose

**Child accounts only use:**
- ✅ Essential cookies (authentication, security)
- ❌ No performance or preference cookies

**Parental Control:**
- Parents can view cookie settings for child accounts
- Parents cannot enable tracking cookies for children (COPPA compliance)

---

## International Users

### EU/UK (GDPR)

**We comply with GDPR cookie consent requirements:**

- ✅ Clear information about cookies (this policy)
- ✅ Explicit consent before non-essential cookies
- ✅ Granular control (choose which cookies to accept)
- ✅ Withdraw consent anytime
- ✅ No cookie walls (you can use the Service with essential cookies only)

**ePrivacy Directive compliance:**
- Consent obtained before cookies are set (except essential)
- Consent is freely given, specific, informed, and unambiguous

### California (CCPA)

**We do NOT sell personal information.**

**Cookies we use:**
- Do NOT track you across websites
- Do NOT build advertising profiles
- Do NOT share data with third-party advertisers

**You have no "sale" to opt out of.**

---

## Cookie Lifespan

**How long do cookies last?**

| Cookie Type | Duration | When Deleted |
|-------------|----------|--------------|
| **Session cookies** | Until you close your browser | Immediately after closing browser |
| **Persistent cookies** | 1-2 years | After expiration date or manual deletion |

**We delete expired cookies automatically.**

---

## Updates to This Policy

**We may update this Cookie Policy to:**
- Add new cookies (for new features)
- Remove cookies (if we stop using a service)
- Improve clarity

**When we update:**
1. **Notice**: We'll update the "Last Updated" date at the top
2. **Notification**: Material changes will trigger an email notification
3. **Re-consent**: For significant changes, we may ask for your consent again

**Review this policy periodically** for the latest information.

---

## Contact Us

**Questions about cookies?**

- **Email:** privacy@familyhub.app
- **Subject:** Cookie Policy Inquiry
- **Response Time:** Within 2 business days

**Data Protection Officer (EU/UK):**
- **Email:** dpo@familyhub.app

**Postal Address:**
Family Hub, Inc.
123 Main Street, Suite 100
Wilmington, DE 19801
United States of America

---

## Additional Resources

**Learn more about cookies:**
- **All About Cookies:** https://www.allaboutcookies.org
- **EU Cookie Law:** https://gdpr.eu/cookies
- **FTC Consumer Information:** https://www.ftc.gov/site-information/privacy-policy/internet-cookies

**Browser cookie guides:**
- **Chrome:** https://support.google.com/chrome/answer/95647
- **Firefox:** https://support.mozilla.org/en-US/kb/clear-cookies-and-site-data-firefox
- **Safari:** https://support.apple.com/guide/safari/manage-cookies-sfri11471
- **Edge:** https://support.microsoft.com/en-us/microsoft-edge/delete-cookies-in-microsoft-edge-63947406-40ac-c3b8-57b9-2a946a29ae09

---

## Summary Table

**Quick reference of all cookies:**

| Cookie Name | Type | Purpose | Duration | Can Disable? |
|-------------|------|---------|----------|--------------|
| `fh_session` | Essential | Session management | Session | No |
| `fh_csrf` | Essential | CSRF protection | Session | No |
| `fh_auth` | Essential | OAuth state | 1 hour | No |
| `fh_analytics` | Performance | Analytics tracking | 2 years | Yes |
| `fh_theme` | Preference | Dark mode | 1 year | Yes |
| `fh_lang` | Preference | Language | 1 year | Yes |
| `fh_tz` | Preference | Timezone | 1 year | Yes |
| `zitadel_session` | Third-Party | OAuth login | Session | No |
| `__stripe_mid` | Third-Party | Fraud detection | 1 year | No* |

*Only set when visiting payment page.

---

**Acceptance:**

**BY USING FAMILY HUB, YOU CONSENT TO OUR USE OF ESSENTIAL COOKIES.**

**For non-essential cookies, we will ask for your explicit consent via the cookie banner.**

---

**Version History:**
- v1.0 (December 20, 2025): Initial Cookie Policy

**Last Updated**: December 20, 2025
**Effective Date**: January 1, 2026
