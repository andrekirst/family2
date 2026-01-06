# Quartz.NET Background Jobs - Quick Start Guide

## Step 1: Apply Database Migration

```bash
cd /home/andrekirst/git/github/andrekirst/family2

# Option A: Using psql directly
psql -U postgres -d family_hub -f docs/migrations/PHASE_2D_QUARTZ_SETUP_MANUAL_MIGRATION.sql

# Option B: Using EF Core Migrations (after fixing compile errors)
cd src/api
dotnet ef migrations add AddQueuedManagedAccountCreations --context AuthDbContext \
    --project Modules/FamilyHub.Modules.Auth --startup-project FamilyHub.Api
dotnet ef database update --context AuthDbContext
```

## Step 2: Verify Quartz Configuration

The Quartz.NET scheduler is configured in `/src/api/FamilyHub.Api/Program.cs` with:

**Job 1: ManagedAccountRetryJob**
- Schedule: Every 1 minute
- Purpose: Retry failed Zitadel account creations
- Retry Schedule: 1min → 5min → 15min → 1hr → 4hr (5 attempts max)

**Job 2: ExpiredInvitationCleanupJob**
- Schedule: Daily at 3 AM UTC
- Purpose: Delete invitations expired >30 days ago

## Step 3: Start the API

```bash
cd src/api/FamilyHub.Api
dotnet run
```

**Expected Logs (via Serilog → Seq):**

```
[INF] Quartz Scheduler started
[INF] ManagedAccountRetryJob scheduled with trigger: Every 1 minute
[INF] ExpiredInvitationCleanupJob scheduled with trigger: Daily at 3 AM UTC
[DBG] ManagedAccountRetryJob starting execution
[DBG] No pending jobs ready for retry
[DBG] ManagedAccountRetryJob completed successfully
```

## Step 4: Test Manually

### Test 1: Managed Account Retry Job

```sql
-- Insert a test queued job
INSERT INTO auth.queued_managed_account_creations (
    id, family_id, username, full_name, role, encrypted_password,
    created_by_user_id, retry_count, status, next_retry_at,
    created_at, updated_at
) VALUES (
    gen_random_uuid(),
    (SELECT id FROM auth.families LIMIT 1),
    'testuser',
    'Test User',
    'managed_account',
    'ENCRYPTED_PASSWORD_PLACEHOLDER', -- TODO: Use real encryption!
    (SELECT id FROM auth.users LIMIT 1),
    0,
    'pending',
    NOW(), -- Ready for immediate processing
    NOW(),
    NOW()
);

-- Wait 1 minute for job to run

-- Check job status (should be 'processing' or 'completed/failed')
SELECT id, username, status, retry_count, next_retry_at, last_error_message
FROM auth.queued_managed_account_creations
ORDER BY created_at DESC
LIMIT 10;

-- Check if User was created (if Zitadel call succeeded)
SELECT id, username, full_name, email
FROM auth.users
WHERE username = 'testuser';
```

### Test 2: Expired Invitation Cleanup Job

```sql
-- Insert a test expired invitation (>30 days old)
INSERT INTO auth.family_member_invitations (
    id, family_id, email, role, status, invitation_token,
    invitation_display_code, invited_by_user_id,
    expires_at, created_at, updated_at
) VALUES (
    gen_random_uuid(),
    (SELECT id FROM auth.families LIMIT 1),
    'test@example.com',
    'member',
    'expired',
    'expired_test_token',
    'EXP123',
    (SELECT id FROM auth.users LIMIT 1),
    NOW() - INTERVAL '35 days', -- Expired 35 days ago
    NOW() - INTERVAL '35 days',
    NOW()
);

-- Wait until 3 AM UTC (or change cron schedule for testing)
-- Alternatively, trigger job manually via Quartz API

-- Check if invitation was deleted
SELECT COUNT(*) FROM auth.family_member_invitations
WHERE status = 'expired' AND expires_at < NOW() - INTERVAL '30 days';
-- Should be 0 after cleanup job runs
```

## Step 5: Monitor via Seq

**Seq URL:** http://localhost:5341

**Search Queries:**

```
# All Quartz job executions
@Message like "%Job%"

# Managed account retry job
@Message like "%ManagedAccountRetryJob%"

# Expired invitation cleanup job
@Message like "%ExpiredInvitationCleanupJob%"

# Job errors
@Level = "Error" AND @Message like "%Job%"
```

## Step 6: Verify Job Scheduling

```csharp
// In Program.cs, add temporary logging after AddQuartz():
var scheduler = app.Services.GetRequiredService<ISchedulerFactory>().GetScheduler().Result;
var jobKeys = scheduler.GetJobKeys(GroupMatcher<JobKey>.AnyGroup()).Result;

foreach (var jobKey in jobKeys)
{
    var triggers = scheduler.GetTriggersOfJob(jobKey).Result;
    Log.Information("Job {JobName}: {TriggerCount} triggers", jobKey.Name, triggers.Count);

    foreach (var trigger in triggers)
    {
        Log.Information("  Trigger: {TriggerName}, Next Fire: {NextFire}",
            trigger.Key.Name, trigger.GetNextFireTimeUtc());
    }
}
```

## Troubleshooting

### Job Not Running

**Check:**
1. Quartz.NET packages installed correctly
2. Jobs registered in Program.cs
3. QuartzHostedService started (check logs)
4. Database connection working

**Logs to Check:**
```
[ERR] Failed to execute job
[ERR] Database connection failed
```

### Jobs Running but No Data Processing

**Check:**
1. Database table `queued_managed_account_creations` exists
2. Repository registered in DI container (`AuthModuleServiceRegistration.cs`)
3. Scoped service resolution working (check DI errors)

### Password Encryption Errors

**Current Implementation:** Placeholder (NOT production-ready!)

**TODO:** Implement proper encryption using Data Protection API

```csharp
// Add to Program.cs
services.AddDataProtection()
    .PersistKeysToDbContext<AuthDbContext>();

// Create encryption service
public class PasswordEncryptionService(IDataProtectionProvider provider)
{
    private readonly IDataProtector _protector = provider.CreateProtector("ManagedAccountPasswords");

    public string Encrypt(string plaintext) => _protector.Protect(plaintext);
    public string Decrypt(string ciphertext) => _protector.Unprotect(ciphertext);
}
```

### Zitadel API Errors

**Common Issues:**
- Invalid access token (check Zitadel settings)
- Rate limiting (exponential backoff handles this)
- Network errors (retry logic handles this)
- Invalid username format (permanent error - should NOT retry)

**Check Logs:**
```
[WRN] Failed to create Zitadel user for job {JobId}, attempt {RetryCount}: {Error}
[ERR] Job {JobId} failed permanently after {RetryCount} attempts
```

## Files Reference

**Key Files:**
- **Jobs:** `/src/api/Modules/FamilyHub.Modules.Auth/Infrastructure/BackgroundJobs/`
  - `ManagedAccountRetryJob.cs`
  - `ExpiredInvitationCleanupJob.cs`

- **Domain:** `/src/api/Modules/FamilyHub.Modules.Auth/Domain/`
  - `QueuedManagedAccountCreation.cs`
  - `ValueObjects/QueuedJobStatus.cs`

- **Config:** `/src/api/FamilyHub.Api/Program.cs`

- **Migration:** `/docs/migrations/PHASE_2D_QUARTZ_SETUP_MANUAL_MIGRATION.sql`

## Next Steps

1. ✅ Apply database migration
2. ✅ Start API and verify scheduler logs
3. ⏳ Fix compile errors in CancelInvitation/ResendInvitation commands
4. ⏳ Implement password encryption (Data Protection API)
5. ⏳ Write integration tests
6. ⏳ Add Prometheus metrics
7. ⏳ Enable domain event publishing

---

**Documentation:** See `/PHASE_2D_QUARTZ_IMPLEMENTATION_SUMMARY.md` for complete implementation details.
