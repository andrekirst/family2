# Zitadel OAuth Setup - Quick Start Guide

Choose your preferred approach:

---

## ⚡ Option 1: Automated Setup (Recommended - 5 minutes)

**Step 1: Register in Zitadel (one-time)**

1. Open: http://localhost:8080
2. Click **"Registrieren"** (Register)
3. Fill in your details:
   - Email: your-email@example.com
   - First Name: Your Name
   - Last Name: Your Last Name
   - Password: Choose a strong password
4. Click **"Registrieren"**
5. You are now the admin (first user = admin automatically)

**Step 2: Create Personal Access Token**

1. Login to Zitadel: http://localhost:8080
2. Click your profile icon (top right)
3. Click **"Settings"** or **"Einstellungen"**
4. Navigate to: **"Personal Access Tokens"** or **"Persönliche Access Tokens"**
5. Click **"New"** or **"Neu"**
6. Name: `OAuth App Creation`
7. Expiration: `1 day` (or longer)
8. Click **"Create"** or **"Erstellen"**
9. **⚠️ CRITICAL**: Copy the token immediately (shown only once!)

**Step 3: Run Automated Script**

```bash
cd /tmp
python3 create-zitadel-oauth-app.py --token YOUR_TOKEN_HERE
```

**Done!** The script will:

- ✅ Create project "Family Hub"
- ✅ Create OIDC application "Family Hub Web" with PKCE
- ✅ Configure redirect URIs
- ✅ Update backend configuration automatically
- ✅ Display Client ID

---

## 📋 Option 2: Manual Setup (15 minutes)

Follow the detailed guide: [ZITADEL_OAUTH_SETUP.md](./ZITADEL_OAUTH_SETUP.md)

---

## 🧪 Testing the OAuth Flow

After setup (either option):

1. **Start Backend**:

   ```bash
   cd /home/andrekirst/git/github/andrekirst/family2/src/api/FamilyHub.Api
   dotnet run
   ```

2. **Start Frontend**:

   ```bash
   cd /home/andrekirst/git/github/andrekirst/family2/src/frontend/family-hub-web
   ng serve
   ```

3. **Test Login**:
   - Open: http://localhost:4200
   - Click: **"Sign in with Zitadel"**
   - You'll be redirected to Zitadel
   - Login with your credentials
   - You'll be redirected back to dashboard
   - ✅ SUCCESS if you see your email displayed

---

## 🔧 Troubleshooting

### Script fails with "Failed to get organizations"

- **Cause**: Invalid or expired token
- **Fix**: Create a new Personal Access Token and try again

### Script fails with "Zitadel is not running"

- **Cause**: Docker containers not started
- **Fix**:

  ```bash
  cd /home/andrekirst/git/github/andrekirst/family2/infrastructure/docker
  docker-compose up -d
  ```

### "Invalid redirect_uri" error during login

- **Cause**: Redirect URI mismatch
- **Fix**: Verify in Zitadel application settings:
  - Redirect URI: `http://localhost:4200/auth/callback` (exact match, no trailing slash)

### Backend shows "invalid_client" error

- **Cause**: Client ID mismatch
- **Fix**: Check `/src/api/FamilyHub.Api/appsettings.Development.json`:

  ```json
  {
    "Zitadel": {
      "ClientId": "YOUR_CLIENT_ID_FROM_SCRIPT_OUTPUT"
    }
  }
  ```

---

## 📝 What Gets Created

**In Zitadel:**

- Project: "Family Hub"
- Application: "Family Hub Web"
  - Type: Web Application
  - Auth Method: PKCE (no client secret required)
  - Redirect URI: `http://localhost:4200/auth/callback`
  - Post Logout URI: `http://localhost:4200/login`
  - Grant Types: Authorization Code, Refresh Token

**In Backend:**

- Updated: `/src/api/FamilyHub.Api/appsettings.Development.json`

  ```json
  {
    "Zitadel": {
      "Authority": "http://localhost:8080",
      "ClientId": "123456789012345678@familyhub",
      "RedirectUri": "http://localhost:4200/auth/callback",
      "Scopes": "openid profile email",
      "Audience": "family-hub-api"
    }
  }
  ```

---

## ✅ Next Steps After Setup

1. ✅ OAuth application created
2. ✅ Backend configured
3. 🔄 Start backend and frontend
4. 🧪 Test complete OAuth flow
5. 🎉 You're ready to develop!

---

**Status**: Ready for OAuth integration
**Last Updated**: 2025-12-22
