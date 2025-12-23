# Multi-Tenancy Strategy - Family Hub

**Version:** 1.0
**Date:** 2025-12-19  
**Status:** Architecture Approved
**Author:** Cloud Architect (Claude Code)

---

## Executive Summary

This document defines the multi-tenancy strategy for Family Hub, where each family represents one tenant. The strategy balances cost efficiency, data isolation, operational simplicity, and scalability from 10 families to 10,000+ families.

**Key Decision: Shared Database with Row-Level Security (RLS)**

**Rationale:**

- **Cost**: $100-400/month for 100 families vs $10,000+/month for dedicated databases
- **Operations**: Single database to backup, monitor, and maintain
- **Security**: PostgreSQL RLS provides strong tenant isolation
- **Scalability**: Can shard later if needed (10,000+ families)

---

## Table of Contents

1. [Tenant Isolation Approach](#1-tenant-isolation-approach)
2. [Database Isolation Strategy](#2-database-isolation-strategy)
3. [Tenant Onboarding Automation](#3-tenant-onboarding-automation)
4. [Resource Quotas and Limits](#4-resource-quotas-and-limits)
5. [Cost Allocation per Tenant](#5-cost-allocation-per-tenant)

---

## 1. Tenant Isolation Approach

### 1.1 Isolation Models Comparison

| Model                               | Pros                                              | Cons                                                             | Decision        |
| ----------------------------------- | ------------------------------------------------- | ---------------------------------------------------------------- | --------------- |
| **Dedicated DB per Tenant**         | Maximum isolation, per-tenant backup/restore      | Expensive ($100/tenant/month), complex operations, 100x overhead | ❌ Rejected     |
| **Dedicated Schema per Tenant**     | Good isolation, shared infrastructure             | DB connection overhead, migration complexity                     | ❌ Rejected     |
| **Shared DB with RLS**              | Cost-efficient, simple operations, good isolation | Risk of misconfiguration, potential noisy neighbor               | ✅ **Selected** |
| **Kubernetes Namespace per Tenant** | Strong isolation, resource quotas                 | Massive overhead (100+ namespaces), cost prohibitive             | ❌ Rejected     |

### 1.2 Selected Approach: Shared DB with Row-Level Security

```
┌─────────────────────────────────────────────────────────────┐
│               Multi-Tenancy Architecture                     │
├─────────────────────────────────────────────────────────────┤
│                                                              │
│  Application Layer (Shared Kubernetes Pods)                 │
│  ┌────────────────────────────────────────────────────┐    │
│  │  Calendar Service (3 replicas)                     │    │
│  │  - Serves all families                             │    │
│  │  - Sets family_group_id context per request        │    │
│  │  - PostgreSQL RLS enforces data isolation          │    │
│  └────────────────────────────────────────────────────┘    │
│                          │                                   │
│                          │ SQL Query                         │
│                          │ SET app.current_family_id = uuid  │
│                          ▼                                   │
│  ┌────────────────────────────────────────────────────┐    │
│  │    PostgreSQL (Shared Database)                    │    │
│  │                                                     │    │
│  │  ┌──────────────────────────────────────────────┐ │    │
│  │  │  calendar.events table                        │ │    │
│  │  │                                               │ │    │
│  │  │  id  | family_group_id | title | start_time │ │    │
│  │  │  ───────────────────────────────────────────  │ │    │
│  │  │  1   | uuid-family-A   | ...   | ...        │ │    │
│  │  │  2   | uuid-family-B   | ...   | ...        │ │    │
│  │  │  3   | uuid-family-A   | ...   | ...        │ │    │
│  │  │                                               │ │    │
│  │  │  RLS Policy:                                  │ │    │
│  │  │    WHERE family_group_id =                    │ │    │
│  │  │      current_setting('app.current_family_id') │ │    │
│  │  └──────────────────────────────────────────────┘ │    │
│  └────────────────────────────────────────────────────┘    │
│                                                              │
│  Result: Family A can ONLY see rows with family_group_id=A  │
│          Family B can ONLY see rows with family_group_id=B  │
│                                                              │
└─────────────────────────────────────────────────────────────┘
```

---

## 2. Database Isolation Strategy

### 2.1 Row-Level Security Implementation

**Step 1: Table Design with tenant_id Column**

```sql
-- Every table includes family_group_id
CREATE TABLE calendar.events (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    family_group_id UUID NOT NULL REFERENCES auth.family_groups(id),
    title TEXT NOT NULL,
    description TEXT,
    start_time TIMESTAMP WITH TIME ZONE NOT NULL,
    end_time TIMESTAMP WITH TIME ZONE NOT NULL,
    created_by UUID NOT NULL,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT NOW()
);

-- Index on family_group_id for performance
CREATE INDEX idx_calendar_events_family_group
    ON calendar.events(family_group_id, start_time);
```

**Step 2: Enable Row-Level Security**

```sql
-- Enable RLS on table
ALTER TABLE calendar.events ENABLE ROW LEVEL SECURITY;

-- Create policy: Users can only see their family's data
CREATE POLICY family_isolation_policy ON calendar.events
    FOR ALL
    USING (
        family_group_id = current_setting('app.current_family_id', true)::UUID
    )
    WITH CHECK (
        family_group_id = current_setting('app.current_family_id', true)::UUID
    );

-- Alternative policy: More complex multi-family access
CREATE POLICY family_member_access_policy ON calendar.events
    FOR ALL
    USING (
        family_group_id IN (
            SELECT fm.family_group_id
            FROM auth.family_members fm
            WHERE fm.user_id = current_setting('app.current_user_id', true)::UUID
              AND fm.is_active = true
        )
    )
    WITH CHECK (
        family_group_id IN (
            SELECT fm.family_group_id
            FROM auth.family_members fm
            WHERE fm.user_id = current_setting('app.current_user_id', true)::UUID
              AND fm.is_active = true
              AND fm.role IN ('Owner', 'Admin', 'Member')
        )
    );
```

**Step 3: Application Sets Context**

```csharp
// C# implementation in service
public class CalendarService
{
    private readonly IDbConnection _connection;

    public async Task<List<CalendarEvent>> GetEventsForFamily(Guid familyGroupId, Guid userId)
    {
        // Set family context for RLS
        await _connection.ExecuteAsync(
            "SET app.current_family_id = @familyId; SET app.current_user_id = @userId",
            new { familyId = familyGroupId, userId = userId }
        );

        // Query - RLS automatically filters
        var events = await _connection.QueryAsync<CalendarEvent>(
            "SELECT * FROM calendar.events WHERE start_time > @startDate",
            new { startDate = DateTime.UtcNow }
        );

        return events.ToList();
    }
}

// Alternative: Connection string per request with RLS
public class PostgresConnectionFactory
{
    public async Task<IDbConnection> CreateConnectionForFamily(Guid familyGroupId, Guid userId)
    {
        var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();

        // Set context variables
        using (var cmd = connection.CreateCommand())
        {
            cmd.CommandText = "SET app.current_family_id = @familyId; SET app.current_user_id = @userId";
            cmd.Parameters.AddWithValue("familyId", familyGroupId);
            cmd.Parameters.AddWithValue("userId", userId);
            await cmd.ExecuteNonQueryAsync();
        }

        return connection;
    }
}
```

### 2.2 Schema-per-Service Strategy

```sql
-- Each service has its own schema
CREATE SCHEMA auth;        -- Auth Service
CREATE SCHEMA calendar;    -- Calendar Service
CREATE SCHEMA tasks;       -- Task Service
CREATE SCHEMA shopping;    -- Shopping Service
CREATE SCHEMA health;      -- Health Service
CREATE SCHEMA meal_planning; -- Meal Planning Service
CREATE SCHEMA finance;     -- Finance Service
CREATE SCHEMA communication; -- Communication Service

-- Service users only have access to their schema
CREATE USER calendar_service WITH PASSWORD 'secure_password';
GRANT ALL PRIVILEGES ON SCHEMA calendar TO calendar_service;
GRANT USAGE ON SCHEMA auth TO calendar_service; -- Read-only for auth lookups

-- Prevent cross-schema access
REVOKE ALL ON SCHEMA tasks FROM calendar_service;
REVOKE ALL ON SCHEMA shopping FROM calendar_service;
```

### 2.3 Data Isolation Testing

```sql
-- Test RLS policies
-- 1. Connect as service user
SET ROLE calendar_service;

-- 2. Set family context (Family A)
SET app.current_family_id = 'uuid-family-A';

-- 3. Query data (should only see Family A's events)
SELECT COUNT(*) FROM calendar.events;
-- Expected: Only Family A's event count

-- 4. Try to bypass RLS (should fail)
SELECT COUNT(*) FROM calendar.events WHERE family_group_id = 'uuid-family-B';
-- Expected: 0 rows (RLS filters out Family B)

-- 5. Verify no data leakage
SELECT family_group_id, COUNT(*) FROM calendar.events GROUP BY family_group_id;
-- Expected: Only uuid-family-A appears in results

-- 6. Test as superuser (RLS bypassed)
RESET ROLE;
SELECT family_group_id, COUNT(*) FROM calendar.events GROUP BY family_group_id;
-- Expected: All families visible (admin access)
```

---

## 3. Tenant Onboarding Automation

### 3.1 Onboarding Flow

```
User Registration
       ↓
Create Family Group (database record)
       ↓
Initialize Default Data (templates, settings)
       ↓
Send Welcome Email
       ↓
Ready to Use
```

### 3.2 Onboarding API

```csharp
public class FamilyOnboardingService
{
    public async Task<FamilyGroup> OnboardNewFamily(CreateFamilyRequest request)
    {
        using var transaction = await _dbContext.Database.BeginTransactionAsync();

        try
        {
            // 1. Create family group
            var familyGroup = new FamilyGroup
            {
                Id = Guid.NewGuid(),
                Name = request.FamilyName,
                OwnerId = request.UserId,
                CreatedAt = DateTime.UtcNow,
                SubscriptionTier = SubscriptionTier.Free
            };
            await _dbContext.FamilyGroups.AddAsync(familyGroup);

            // 2. Add owner as family member
            var ownerMember = new FamilyMember
            {
                FamilyGroupId = familyGroup.Id,
                UserId = request.UserId,
                Role = FamilyRole.Owner,
                JoinedAt = DateTime.UtcNow,
                IsActive = true
            };
            await _dbContext.FamilyMembers.AddAsync(ownerMember);

            // 3. Initialize default settings
            await InitializeDefaultSettings(familyGroup.Id);

            // 4. Create sample data (optional)
            if (request.IncludeSampleData)
            {
                await CreateSampleData(familyGroup.Id, request.UserId);
            }

            // 5. Set resource quotas
            await SetResourceQuotas(familyGroup.Id, SubscriptionTier.Free);

            // 6. Publish event for other services
            await _eventBus.PublishAsync(new FamilyGroupCreatedEvent
            {
                FamilyGroupId = familyGroup.Id,
                OwnerId = request.UserId,
                CreatedAt = DateTime.UtcNow
            });

            await transaction.CommitAsync();

            // 7. Send welcome email (async)
            await _emailService.SendWelcomeEmailAsync(request.UserId, familyGroup.Name);

            return familyGroup;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    private async Task InitializeDefaultSettings(Guid familyGroupId)
    {
        var settings = new FamilySettings
        {
            FamilyGroupId = familyGroupId,
            Timezone = "UTC",
            Language = "en-US",
            DateFormat = "MM/dd/yyyy",
            EnableEventChains = true,
            NotificationPreferences = new NotificationPreferences
            {
                EmailEnabled = true,
                PushEnabled = true,
                Frequency = NotificationFrequency.RealTime
            }
        };
        await _dbContext.FamilySettings.AddAsync(settings);
    }

    private async Task CreateSampleData(Guid familyGroupId, Guid userId)
    {
        // Sample calendar event
        await _calendarService.CreateEventAsync(new CreateEventRequest
        {
            FamilyGroupId = familyGroupId,
            Title = "Welcome to Family Hub!",
            StartTime = DateTime.UtcNow.AddDays(1),
            EndTime = DateTime.UtcNow.AddDays(1).AddHours(1),
            CreatedBy = userId
        });

        // Sample task
        await _taskService.CreateTaskAsync(new CreateTaskRequest
        {
            FamilyGroupId = familyGroupId,
            Title = "Explore Family Hub features",
            Priority = TaskPriority.Low,
            CreatedBy = userId
        });
    }

    private async Task SetResourceQuotas(Guid familyGroupId, SubscriptionTier tier)
    {
        var quotas = tier switch
        {
            SubscriptionTier.Free => new ResourceQuotas
            {
                MaxMembers = 6,
                MaxCalendarEvents = 1000,
                MaxTasks = 500,
                MaxShoppingLists = 50,
                MaxRecipes = 100,
                StorageMB = 100
            },
            SubscriptionTier.Premium => new ResourceQuotas
            {
                MaxMembers = 10,
                MaxCalendarEvents = 10000,
                MaxTasks = 5000,
                MaxShoppingLists = 500,
                MaxRecipes = 1000,
                StorageMB = 1000
            },
            _ => throw new ArgumentException("Invalid subscription tier")
        };

        await _dbContext.ResourceQuotas.AddAsync(new ResourceQuota
        {
            FamilyGroupId = familyGroupId,
            Quotas = quotas,
            CreatedAt = DateTime.UtcNow
        });
    }
}
```

### 3.3 Tenant Offboarding

```csharp
public class FamilyOffboardingService
{
    public async Task OffboardFamily(Guid familyGroupId, string reason)
    {
        using var transaction = await _dbContext.Database.BeginTransactionAsync();

        try
        {
            // 1. Export family data (GDPR compliance)
            var exportPath = await ExportFamilyData(familyGroupId);

            // 2. Anonymize PII (if required by data retention policy)
            // OR
            // 3. Soft delete (mark as deleted)
            var familyGroup = await _dbContext.FamilyGroups.FindAsync(familyGroupId);
            familyGroup.DeletedAt = DateTime.UtcNow;
            familyGroup.DeletionReason = reason;

            // 4. Cancel subscriptions
            await _billingService.CancelSubscription(familyGroupId);

            // 5. Notify services
            await _eventBus.PublishAsync(new FamilyGroupDeletedEvent
            {
                FamilyGroupId = familyGroupId,
                DeletedAt = DateTime.UtcNow
            });

            await transaction.CommitAsync();

            // 6. Schedule hard delete (after retention period, e.g., 90 days)
            await _scheduler.ScheduleHardDelete(familyGroupId, DateTime.UtcNow.AddDays(90));
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
}
```

---

## 4. Resource Quotas and Limits

### 4.1 Subscription Tiers

```yaml
subscription_tiers:
  free:
    max_family_members: 6
    max_calendar_events: 1000
    max_tasks: 500
    max_shopping_lists: 50
    max_recipes: 100
    storage_mb: 100
    event_chains_enabled: true
    api_rate_limit: 100 req/min
    support_level: community

  premium:
    price: "$9.99/month"
    max_family_members: 10
    max_calendar_events: 10000
    max_tasks: 5000
    max_shopping_lists: 500
    max_recipes: 1000
    storage_mb: 1000
    event_chains_enabled: true
    advanced_event_chains: true
    api_rate_limit: 500 req/min
    support_level: email

  family:
    price: "$14.99/month"
    max_family_members: 15
    max_calendar_events: unlimited
    max_tasks: unlimited
    max_shopping_lists: unlimited
    max_recipes: unlimited
    storage_mb: 5000
    event_chains_enabled: true
    advanced_event_chains: true
    custom_event_chains: true
    api_rate_limit: 1000 req/min
    support_level: priority
    white_label_option: true
```

### 4.2 Quota Enforcement

```csharp
// Middleware for quota checking
public class ResourceQuotaMiddleware
{
    private readonly RequestDelegate _next;

    public async Task InvokeAsync(HttpContext context, IResourceQuotaService quotaService)
    {
        var familyGroupId = context.User.GetFamilyGroupId();
        var resourceType = GetResourceTypeFromRequest(context.Request);

        // Check quota before processing
        var canProceed = await quotaService.CheckQuotaAsync(familyGroupId, resourceType);

        if (!canProceed)
        {
            context.Response.StatusCode = 429; // Too Many Requests
            await context.Response.WriteAsJsonAsync(new
            {
                error = "Resource quota exceeded",
                message = "Upgrade to Premium to increase limits",
                upgrade_url = "/pricing"
            });
            return;
        }

        await _next(context);
    }
}

// Service implementation
public class ResourceQuotaService : IResourceQuotaService
{
    public async Task<bool> CheckQuotaAsync(Guid familyGroupId, ResourceType resourceType)
    {
        var quota = await GetQuotaAsync(familyGroupId);
        var usage = await GetUsageAsync(familyGroupId, resourceType);

        return resourceType switch
        {
            ResourceType.CalendarEvent => usage.CalendarEvents < quota.MaxCalendarEvents,
            ResourceType.Task => usage.Tasks < quota.MaxTasks,
            ResourceType.ShoppingList => usage.ShoppingLists < quota.MaxShoppingLists,
            ResourceType.Recipe => usage.Recipes < quota.MaxRecipes,
            ResourceType.Storage => usage.StorageMB < quota.StorageMB,
            _ => true
        };
    }

    public async Task IncrementUsageAsync(Guid familyGroupId, ResourceType resourceType)
    {
        // Increment usage counter (Redis for performance)
        var key = $"usage:{familyGroupId}:{resourceType}";
        await _redis.IncrementAsync(key);

        // Periodically sync to database
        // Background job runs every hour to persist Redis counters to PostgreSQL
    }
}
```

### 4.3 Rate Limiting

```yaml
# NGINX Ingress rate limiting
apiVersion: networking.k8s.io/v1
kind: Ingress
metadata:
  name: family-hub-ingress
  annotations:
    nginx.ingress.kubernetes.io/limit-rps: "10" # 10 requests per second
    nginx.ingress.kubernetes.io/limit-burst-multiplier: "5"
    nginx.ingress.kubernetes.io/limit-connections: "10"
```

```csharp
// Application-level rate limiting (per family)
public class FamilyRateLimitingMiddleware
{
    public async Task InvokeAsync(HttpContext context, IRateLimiter rateLimiter)
    {
        var familyGroupId = context.User.GetFamilyGroupId();
        var tier = await GetSubscriptionTier(familyGroupId);

        var limit = tier switch
        {
            SubscriptionTier.Free => 100,  // 100 req/min
            SubscriptionTier.Premium => 500,
            SubscriptionTier.Family => 1000,
            _ => 50
        };

        var allowed = await rateLimiter.AllowRequestAsync(familyGroupId, limit);

        if (!allowed)
        {
            context.Response.StatusCode = 429;
            context.Response.Headers["Retry-After"] = "60";
            await context.Response.WriteAsJsonAsync(new
            {
                error = "Rate limit exceeded",
                limit = limit,
                message = "Please wait before making more requests"
            });
            return;
        }

        await _next(context);
    }
}
```

---

## 5. Cost Allocation per Tenant

### 5.1 Metrics to Track

```yaml
# Prometheus metrics per tenant
metrics:
  - metric: database_queries_total
    labels: [family_group_id]
    description: "Total database queries by family"

  - metric: database_query_duration_seconds
    labels: [family_group_id, query_type]
    description: "Database query duration by family"

  - metric: api_requests_total
    labels: [family_group_id, endpoint, status]
    description: "Total API requests by family"

  - metric: storage_bytes
    labels: [family_group_id, storage_type]
    description: "Storage used by family (DB + files)"

  - metric: event_bus_messages_total
    labels: [family_group_id, event_type]
    description: "Event bus messages by family"

  - metric: cpu_seconds_total
    labels: [family_group_id]
    description: "Estimated CPU usage by family"

  - metric: memory_bytes
    labels: [family_group_id]
    description: "Estimated memory usage by family"
```

### 5.2 Cost Calculation Model

```python
# Cost allocation script
def calculate_family_cost(family_group_id, month):
    # 1. Database cost
    query_count = get_metric('database_queries_total', family_group_id, month)
    query_duration = get_metric('database_query_duration_seconds', family_group_id, month)
    db_cost = (query_duration / total_query_duration) * total_db_cost

    # 2. Storage cost
    storage_bytes = get_metric('storage_bytes', family_group_id, month)
    storage_cost = (storage_bytes / 1024**3) * 0.10  # $0.10 per GB

    # 3. API request cost
    api_requests = get_metric('api_requests_total', family_group_id, month)
    api_cost = api_requests * 0.0001  # $0.0001 per request

    # 4. Event bus cost
    event_messages = get_metric('event_bus_messages_total', family_group_id, month)
    event_cost = event_messages * 0.00001  # $0.00001 per message

    # 5. Fixed overhead (infrastructure, monitoring, etc.)
    overhead_cost = total_overhead_cost / total_active_families

    # Total cost per family
    total_cost = db_cost + storage_cost + api_cost + event_cost + overhead_cost

    return {
        'family_group_id': family_group_id,
        'month': month,
        'database_cost': db_cost,
        'storage_cost': storage_cost,
        'api_cost': api_cost,
        'event_cost': event_cost,
        'overhead_cost': overhead_cost,
        'total_cost': total_cost
    }

# Example output:
# Family A (Free tier):
#   Database: $0.20
#   Storage: $0.01 (100 MB)
#   API: $0.10 (1000 requests)
#   Events: $0.05 (5000 events)
#   Overhead: $2.00
#   Total: $2.36/month

# Family B (Premium tier):
#   Database: $1.50
#   Storage: $0.10 (1 GB)
#   API: $1.00 (10000 requests)
#   Events: $0.50 (50000 events)
#   Overhead: $2.00
#   Total: $5.10/month
#   Revenue: $9.99/month
#   Margin: $4.89/month (49%)
```

### 5.3 Cost Dashboard (Grafana)

```yaml
# Grafana dashboard for cost allocation
dashboard:
  title: "Family Hub - Cost per Tenant"
  panels:
    - title: "Cost per Family (Top 20)"
      query: |
        sum(
          database_query_duration_seconds{namespace="family-hub"} * 0.05 +
          storage_bytes{namespace="family-hub"} / 1024^3 * 0.10 +
          api_requests_total{namespace="family-hub"} * 0.0001
        ) by (family_group_id)

    - title: "Revenue vs Cost"
      query: |
        sum(subscription_revenue) by (family_group_id) -
        sum(calculated_cost) by (family_group_id)

    - title: "Margin by Subscription Tier"
      query: |
        (subscription_revenue - calculated_cost) / subscription_revenue * 100
        group by (subscription_tier)
```

---

## Appendix A: Migration Path to Dedicated DBs

If needed at scale (10,000+ families):

```yaml
migration_strategy:
  phase_1: "Shared DB with RLS (0-10,000 families)"
  phase_2: "Database sharding by family_group_id (10,000-100,000 families)"
  phase_3: "Dedicated DB clusters per region (100,000+ families)"

sharding_approach:
  shard_key: family_group_id
  shard_count: 10 (initially)
  shard_distribution: consistent_hashing
  tools: Citus, PostgreSQL logical replication
```

---

**Document Status:** Architecture Approved
**Last Updated:** 2025-12-19
**Maintained By:** Cloud Architect + Database Team
