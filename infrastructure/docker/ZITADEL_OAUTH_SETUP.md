# Zitadel OAuth Application Setup Guide

## Prerequisites

- ✅ Zitadel is running at http://localhost:8080
- ✅ You have registered an admin account in Zitadel

---

## Step-by-Step Setup

### Step 1: Access Zitadel Console

1. Open your browser and go to: **http://localhost:8080**
2. Log in with your admin credentials

### Step 2: Create a Project

1. In the left sidebar, click **"Projects"** (Projekte)
2. Click the **"Create New Project"** button (+ or "Neues Projekt erstellen")
3. Fill in the project details:
   - **Name**: `Family Hub`
   - **Description** (optional): `Family Hub OAuth Integration`
4. Click **"Continue"** or **"Erstellen"** (Create)

### Step 3: Create an Application

1. Inside the "Family Hub" project, click **"New"** or **"Applications"** → **"New Application"**
2. Select application type: **"WEB"**
3. Fill in application details:
   - **Name**: `Family Hub Web`
   - **Authentication Method**: **PKCE** (Proof Key for Code Exchange)
     - This is the most secure method for SPAs

### Step 4: Configure Redirect URIs

1. In the **Redirect URIs** section, add:
   ```
   http://localhost:4200/auth/callback
   ```

2. (Optional) In **Post Logout Redirect URIs**, add:
   ```
   http://localhost:4200/login
   ```

### Step 5: Configure Grant Types

Make sure these grant types are enabled:
- ✅ **Authorization Code** (required for OAuth flow)
- ✅ **Refresh Token** (recommended for token refresh)

### Step 6: Save and Get Credentials

1. Click **"Continue"** or **"Save"**
2. You'll see the application credentials screen with:

   **⚠️ CRITICAL - Copy these immediately:**
   - **Client ID**: Something like `271234567890123456@familyhub`
   - **Client Secret**: A long random string (shown only once!)

3. **Copy both values** - you'll need them to configure the backend

### Step 7: (Optional) Additional Configuration

You can also configure:
- **Token Lifetime**: How long access tokens are valid (default: 12 hours)
- **Refresh Token Lifetime**: How long refresh tokens are valid (default: 30 days)
- **ID Token Lifetime**: How long ID tokens are valid (default: 12 hours)

---

## After Setup

Once you have the **Client ID** and **Client Secret**, you need to update the backend configuration:

### Option 1: Provide Credentials to Claude Code

Just paste your Client ID and Client Secret in the chat, and I'll update the backend configuration automatically.

### Option 2: Manual Configuration

Edit `/src/api/FamilyHub.Api/appsettings.Development.json`:

```json
{
  "Zitadel": {
    "Authority": "http://localhost:8080",
    "ClientId": "YOUR_CLIENT_ID_HERE",
    "ClientSecret": "YOUR_CLIENT_SECRET_HERE",
    "RedirectUri": "http://localhost:4200/auth/callback",
    "Scopes": "openid profile email",
    "Audience": "family-hub-api"
  }
}
```

Then restart the backend:
```bash
cd /home/andrekirst/git/github/andrekirst/family2/src/api/FamilyHub.Api
dotnet run
```

---

## Testing the OAuth Flow

After configuration:

1. Open Angular frontend: http://localhost:4200
2. Click "Sign in with Zitadel"
3. You'll be redirected to Zitadel login
4. After successful login, you'll be redirected back to the dashboard

---

## Troubleshooting

### "Invalid redirect_uri" error
- Verify the redirect URI in Zitadel matches exactly: `http://localhost:4200/auth/callback`
- No trailing slashes

### "Invalid client" error
- Verify Client ID and Client Secret are correct
- Verify they're properly set in appsettings.Development.json

### "PKCE verification failed" error
- Ensure Authentication Method is set to "PKCE" in Zitadel
- Check browser console for codeVerifier in sessionStorage

---

## Security Notes

⚠️ **For Production:**
- Use HTTPS for all URLs
- Store Client Secret in Azure Key Vault or environment variables
- Never commit Client Secret to git
- Use shorter token lifetimes
- Enable email verification
- Set up proper logout handling

---

**Status**: Ready to configure
**Last Updated**: 2025-12-22
