# Manual Testing Checklist - GraphQL Subscriptions

**Issue:** #84 - GraphQL Subscriptions for Real-Time Family Updates
**Status:** Phase 6 - Verification & Documentation
**Date:** 2026-01-14

---

## Prerequisites

Before starting manual testing, ensure:

- ✅ Docker Compose is running (PostgreSQL, RabbitMQ, Redis, Seq, Zitadel)
- ✅ API is running (`dotnet run --project src/api/FamilyHub.Api`)
- ✅ Frontend is running (`npm start` in `src/frontend/family-hub-web`)
- ✅ At least 2 test users exist in Zitadel
- ✅ At least 1 test family exists with multiple members

### Quick Setup

```bash
# 1. Start infrastructure
cd infrastructure/docker
docker-compose up -d

# 2. Verify services are healthy
docker ps
# Expected: All containers healthy (PostgreSQL, RabbitMQ, Redis, Seq, Zitadel)

# 3. Check Redis
docker exec familyhub-redis redis-cli ping
# Expected: PONG

# 4. Start API
cd ../../src/api
dotnet run --project FamilyHub.Api
# Expected: API running on http://localhost:5002

# 5. Verify GraphQL endpoint
curl -X POST http://localhost:5002/graphql \
  -H "Content-Type: application/json" \
  -d '{"query":"{ __typename }"}'
# Expected: {"data":{"__typename":"Query"}}
```

---

## Test Scenarios

### Scenario 1: WebSocket Connection Establishment

**Objective:** Verify GraphQL Playground can establish WebSocket connection with JWT authentication.

#### Steps

1. **Open GraphQL Playground:**
   - Navigate to: http://localhost:5002/graphql
   - Click "HTTP HEADERS" tab at bottom

2. **Authenticate:**
   - Login via frontend (http://localhost:4200) to get JWT token
   - Copy JWT token from browser localStorage or network tab
   - Add to GraphQL Playground headers:

     ```json
     {
       "Authorization": "Bearer <your-jwt-token>"
     }
     ```

3. **Test WebSocket Connection:**
   - Switch to "SUBSCRIPTIONS" tab in Playground
   - Execute test subscription:

     ```graphql
     subscription {
       __typename
     }
     ```

   - **Expected:** Connection established (green indicator)
   - **Expected:** No errors in console

4. **Verify Logs:**
   - Check Seq (http://localhost:5341)
   - Search for: `"JWT token validated"`
   - **Expected:** Log entry showing user ID

✅ **PASS CRITERIA:** WebSocket connection established, JWT validated, no errors

---

### Scenario 2: Family Members Subscription (Authorized)

**Objective:** Verify family members receive real-time updates when member joins.

#### Setup

- User A: Family owner (already member of Family 123)
- User B: Has pending invitation to Family 123

#### Steps

1. **Subscribe as User A:**
   - Open GraphQL Playground in Browser 1
   - Authenticate as User A
   - Execute subscription:

     ```graphql
     subscription {
       familyMembersChanged(familyId: "123e4567-e89b-12d3-a456-426614174000") {
         familyId
         changeType
         member {
           userId
           email
           role
           joinedAt
         }
       }
     }
     ```

   - **Expected:** Subscription active (listening icon)
   - Check Seq logs for: `"User {UserId} subscribed to family members changes"`

2. **Trigger Event as User B:**
   - Open new browser tab/window (Browser 2)
   - Authenticate as User B
   - Execute mutation:

     ```graphql
     mutation {
       acceptInvitation(input: { token: "<invitation-token>" }) {
         familyId
         familyName
         role
       }
     }
     ```

   - **Expected:** Mutation succeeds

3. **Verify Subscription Update (Browser 1):**
   - Check GraphQL Playground in Browser 1
   - **Expected:** New event appears:

     ```json
     {
       "data": {
         "familyMembersChanged": {
           "familyId": "123e4567-e89b-12d3-a456-426614174000",
           "changeType": "ADDED",
           "member": {
             "userId": "<user-b-id>",
             "email": "userb@example.com",
             "role": "MEMBER",
             "joinedAt": "2026-01-14T10:30:00Z"
           }
         }
       }
     }
     ```

4. **Verify Logs:**
   - Check Seq for: `"Published subscription message to topic family-members-changed:{familyId}"`
   - **Expected:** Log entry confirming message published

✅ **PASS CRITERIA:** Subscription receives ADDED event immediately after invitation accepted

---

### Scenario 3: Family Members Subscription (Unauthorized)

**Objective:** Verify non-members cannot subscribe to family updates.

#### Setup

- User C: NOT a member of Family 123

#### Steps

1. **Attempt Subscribe as Non-Member:**
   - Open GraphQL Playground
   - Authenticate as User C (not in family)
   - Execute subscription:

     ```graphql
     subscription {
       familyMembersChanged(familyId: "123e4567-e89b-12d3-a456-426614174000") {
         changeType
       }
     }
     ```

   - **Expected:** Subscription appears to start but immediately terminates (yield break)

2. **Verify Authorization Logs:**
   - Check Seq for warning:
     - `"User {UserId} attempted to subscribe to family {FamilyId} without membership"`
   - **Expected:** Warning logged

3. **Verify No Events Received:**
   - Trigger a family member event (accept invitation in that family)
   - **Expected:** User C's subscription receives NOTHING

✅ **PASS CRITERIA:** Non-members cannot receive family member updates

---

### Scenario 4: Pending Invitations Subscription (OWNER/ADMIN)

**Objective:** Verify OWNER/ADMIN users receive real-time invitation updates.

#### Setup

- User A: Family owner (OWNER role in Family 123)

#### Steps

1. **Subscribe as Owner:**
   - Authenticate as User A (OWNER)
   - Execute subscription:

     ```graphql
     subscription {
       pendingInvitationsChanged(familyId: "123e4567-e89b-12d3-a456-426614174000") {
         familyId
         changeType
         invitation {
           email
           role
           displayCode
           expiresAt
         }
       }
     }
     ```

   - **Expected:** Subscription active

2. **Create New Invitation:**
   - In separate tab, execute mutation:

     ```graphql
     mutation {
       inviteFamilyMembersByEmail(input: {
         familyId: "123e4567-e89b-12d3-a456-426614174000"
         invitations: [{
           email: "newuser@example.com"
           role: MEMBER
           message: "Welcome!"
         }]
       }) {
         successCount
       }
     }
     ```

3. **Verify Subscription Update:**
   - **Expected:** New event in subscription tab:

     ```json
     {
       "data": {
         "pendingInvitationsChanged": {
           "familyId": "123e4567-e89b-12d3-a456-426614174000",
           "changeType": "ADDED",
           "invitation": {
             "email": "newuser@example.com",
             "role": "MEMBER",
             "displayCode": "ABC123",
             "expiresAt": "2026-01-21T10:30:00Z"
           }
         }
       }
     }
     ```

4. **Test Invitation Removal:**
   - Cancel the invitation:

     ```graphql
     mutation {
       cancelInvitation(input: { token: "<invitation-token>" }) {
         success
       }
     }
     ```

   - **Expected:** REMOVED event received:

     ```json
     {
       "changeType": "REMOVED",
       "invitation": null
     }
     ```

✅ **PASS CRITERIA:** OWNER/ADMIN receives ADDED and REMOVED invitation events

---

### Scenario 5: Pending Invitations Subscription (MEMBER Role)

**Objective:** Verify MEMBER role cannot subscribe to invitation updates.

#### Setup

- User D: Family member with MEMBER role (not OWNER/ADMIN)

#### Steps

1. **Attempt Subscribe as Member:**
   - Authenticate as User D (MEMBER role)
   - Execute subscription:

     ```graphql
     subscription {
       pendingInvitationsChanged(familyId: "123e4567-e89b-12d3-a456-426614174000") {
         changeType
       }
     }
     ```

   - **Expected:** Subscription terminates immediately (yield break)

2. **Verify Authorization Logs:**
   - Check Seq for warning:
     - `"User {UserId} with role {Role} attempted to subscribe to invitations for family {FamilyId} (requires OWNER or ADMIN)"`
   - **Expected:** Warning logged with role MEMBER

✅ **PASS CRITERIA:** MEMBER role cannot receive invitation updates

---

### Scenario 6: Redis Health Check

**Objective:** Verify Redis health check reports correct status.

#### Steps

1. **Check Health Endpoint:**

   ```bash
   curl http://localhost:5002/health/redis
   ```

   - **Expected:**

     ```json
     {
       "status": "Healthy",
       "timestamp": "2026-01-14T10:30:00Z",
       "checks": [{
         "name": "redis",
         "status": "Healthy",
         "description": null,
         "duration": 15
       }]
     }
     ```

2. **Test Redis Unavailable:**

   ```bash
   # Stop Redis
   docker stop familyhub-redis

   # Check health
   curl http://localhost:5002/health/redis
   ```

   - **Expected:**

     ```json
     {
       "status": "Unhealthy",
       "checks": [{
         "name": "redis",
         "status": "Unhealthy",
         "exception": "Connection failed"
       }]
     }
     ```

3. **Restart Redis:**

   ```bash
   docker start familyhub-redis

   # Wait 5 seconds, check health again
   curl http://localhost:5002/health/redis
   ```

   - **Expected:** Status returns to Healthy

✅ **PASS CRITERIA:** Health check accurately reports Redis status

---

### Scenario 7: Subscription Resilience (Redis Unavailable)

**Objective:** Verify subscriptions fail gracefully when Redis is down.

#### Steps

1. **Stop Redis:**

   ```bash
   docker stop familyhub-redis
   ```

2. **Trigger Event:**
   - Execute `acceptInvitation` mutation
   - **Expected:** Mutation succeeds (invitation accepted in database)

3. **Verify Error Logging:**
   - Check Seq for: `"Failed to publish subscription message to topic..."`
   - **Expected:** Error logged but mutation not affected

4. **Verify No Subscription Updates:**
   - Existing subscriptions receive no events
   - **Expected:** Graceful degradation (no crashes)

5. **Restart Redis:**

   ```bash
   docker start familyhub-redis
   ```

6. **Trigger New Event:**
   - Execute another mutation
   - **Expected:** Subscriptions work again

✅ **PASS CRITERIA:** Business operations succeed even when Redis is unavailable

---

### Scenario 8: Multiple Concurrent Subscriptions

**Objective:** Verify multiple clients can subscribe simultaneously.

#### Steps

1. **Open 3 Browser Windows:**
   - Browser 1: User A (family owner)
   - Browser 2: User B (family member)
   - Browser 3: User E (another family member)

2. **Subscribe in All Browsers:**
   - All execute the same `familyMembersChanged` subscription

3. **Trigger Event:**
   - User F accepts invitation to family
   - **Expected:** ALL 3 subscriptions receive ADDED event

4. **Verify Logs:**
   - Check Seq for 3 separate subscription log entries
   - **Expected:** Each user's subscription logged independently

✅ **PASS CRITERIA:** All authorized subscribers receive the same event

---

### Scenario 9: Monitor Redis PubSub Activity

**Objective:** Verify messages are actually flowing through Redis.

#### Steps

1. **Start Redis Monitor:**

   ```bash
   docker exec -it familyhub-redis redis-cli
   > MONITOR
   ```

2. **Trigger Event:**
   - Accept invitation or create new invitation

3. **Verify Redis Traffic:**
   - **Expected:** See messages like:

     ```
     "PUBLISH" "family-members-changed:123e4567-e89b-12d3-a456-426614174000" "{...payload...}"
     ```

4. **Check Topic Pattern:**
   - Verify topic matches subscription resolver: `family-members-changed:{familyId}`
   - **Expected:** Exact match

✅ **PASS CRITERIA:** Redis PubSub receives messages with correct topic format

---

## Troubleshooting

### Subscription Not Receiving Events

1. **Check WebSocket Connection:**
   - Browser DevTools → Network → WS filter
   - Verify WebSocket connection is established
   - Check for connection errors

2. **Check Authentication:**
   - Verify JWT token is valid and not expired
   - Check Seq for authentication failures

3. **Check Authorization:**
   - Verify user is member of the family
   - Check Seq for authorization warnings

4. **Check Redis:**

   ```bash
   docker exec familyhub-redis redis-cli ping
   # Expected: PONG
   ```

5. **Check Seq Logs:**
   - Search for: `"Published subscription message"`
   - If missing, subscription publisher not being called

### Redis Connection Errors

```bash
# Check Redis container
docker ps | grep redis

# Check Redis logs
docker logs familyhub-redis

# Restart Redis
docker restart familyhub-redis
```

### GraphQL Playground Issues

- **Clear browser cache** and reload
- **Try incognito/private mode** to rule out extension interference
- **Check CORS settings** in Program.cs

---

## Success Criteria Checklist

✅ All scenarios completed without errors
✅ Subscriptions receive events within 1 second
✅ Authorization works correctly (family membership, roles)
✅ Redis health check reports accurate status
✅ Business operations succeed even when Redis unavailable
✅ Multiple concurrent subscriptions work
✅ No errors in Seq logs (except expected authorization warnings)
✅ Redis PubSub shows correct message flow

---

## Next Steps After Verification

1. **Run Automated Tests:**

   ```bash
   dotnet test --filter "FullyQualifiedName~Subscription"
   ```

2. **Update Documentation:**
   - Mark subscriptions as ✅ IMPLEMENTED in INVITATION_SCHEMA.md
   - Add Redis to infrastructure documentation

3. **Create PR:**
   - Commit all changes
   - Create pull request with comprehensive description
   - Reference issue #84

---

**Last Updated:** 2026-01-14
**Tested By:** [Your Name]
**Test Environment:** Local Development (Docker Compose)
**API Version:** .NET 10
**GraphQL Framework:** Hot Chocolate v14
